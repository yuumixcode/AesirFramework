using System;
using System.Collections.Generic;
using System.IO;
using Runestone.AesirArchitecture;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Runestone.AesirModules
{
    /// <summary>
    /// UI 根节点组件。
    /// 负责创建 UICamera、EventSystem、分层 Canvas 以及应用 Canvas 统一配置。
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-999)]
    public class UIRoot : AesirMonoBehaviour
    {
        const int UILayerIndex = 5;
        const int TransparentFXLayerIndex = 1;
        const int UILayerMask = (1 << UILayerIndex) | (1 << TransparentFXLayerIndex);
        internal const string LayerCanvasesFieldName = nameof(_layerCanvases);
        internal const string UICanvasConfigFieldName = nameof(uiCanvasConfigSO);

        static UIRoot _instance;
        static UICanvasConfigSO _defaultCanvasConfig;

        /// <summary>
        /// 自定义输入模块创建回调。
        /// 由 Runestone.AesirModules.InputSystem 程序集在 InputSystem 启用时注册，
        /// 使用 InputSystemUIInputModule 替代默认的 StandaloneInputModule。
        /// 为 null 时使用 StandaloneInputModule。
        /// </summary>
        public static Action<GameObject> CreateInputModule { get; set; }

        /// <summary>
        /// 一次性临时标记：通知下一次 <see cref="Awake" /> 调用需要执行 <see cref="UnityEngine.Object.DontDestroyOnLoad" />。
        /// 由 <see cref="Instance" /> getter 在创建实例前置为 true，Awake 消费后立即重置为 false。
        /// </summary>
        static bool _pendingDontDestroyOnLoad;

        static readonly Dictionary<UILayer, int> LayerSortOrders = new Dictionary<UILayer, int>
        {
            { UILayer.Background, 100 },
            { UILayer.Normal, 200 },
            { UILayer.Popup, 300 },
            { UILayer.Top, 400 }
        };

        readonly Dictionary<UILayer, Canvas> _layerCanvases = new Dictionary<UILayer, Canvas>();

        [SerializeField]
        UICanvasConfigSO uiCanvasConfigSO;

        public static UIRoot Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                // 尝试在已加载的场景中查找预放置的实例
                // 使用 FindAnyObjectByType 而非 FindFirstObjectByType，后者因依赖 InstanceID 排序在 Unity 6 中已废弃
                _instance = FindAnyObjectByType<UIRoot>();
                if (_instance != null)
                {
                    return _instance;
                }

                // 未找到预放置实例 → 运行时创建，标记后由 Awake 决定是否 DDOL
                _pendingDontDestroyOnLoad = true;
                var go = new GameObject("[UIRoot]");
                // AddComponent 在主线程同步执行，Awake 会在 AddComponent 返回前完成，
                // 此时 _pendingDontDestroyOnLoad 已被 Awake 消费完毕，可以安全重置。
                // 重置后标志不会残留，避免影响后续 Awake（如 Enter Play Mode 触发的 Domain Reload）。
                _instance = go.AddComponent<UIRoot>();
                _pendingDontDestroyOnLoad = false;
                return _instance;
            }
        }

        public Camera UICamera { get; private set; }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            // 仅需要跨场景持久化的实例使用 DontDestroyOnLoad；场景中预放置的实例保留在场景中
            if (_pendingDontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            UIModule.Instance.RegisterUIRoot(this);
            Initialize();
        }

        void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        void Initialize()
        {
            EnsureUIComponents();
            EnsureCanvasConfig();
            ApplyCanvasConfig();
        }

        /// <summary>
        /// 构建 UI 层级结构。幂等调用，已存在的子物体不会重复创建。
        /// 与运行时 <see cref="Initialize" /> 走同一套配置路径：
        /// 优先应用 Inspector 已序列化的 <see cref="uiCanvasConfigSO" />，未设置时使用静态缓存的默认配置。
        /// </summary>
        public void Build()
        {
            EnsureUIComponents();
            EnsureCanvasConfig();
            ApplyCanvasConfig();
        }

        /// <summary>
        /// 获取指定层级的根 Transform。层 Canvas 缺失（子物体被删除或重命名）时记录错误并返回 null。
        /// </summary>
        public Transform GetLayerRoot(UILayer layer)
        {
            var canvas = _layerCanvases.GetValueOrDefault(layer);
            if (canvas == null)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.UIModuleTag,
                    $"UIRoot 缺少 {layer} 层的 Canvas（子物体 {layer}Layer 缺失或被重命名），" +
                    "面板将无法挂载到该层，请重建 UIRoot 层级");
                return null;
            }

            return canvas.transform;
        }

        void EnsureUIComponents()
        {
            EnsureUICamera();
            EnsureEventSystem();
            EnsurePresetLayers();
            SetLayerRecursively(transform, UILayerIndex);
            CacheLayerCanvases();
        }

        /// <summary>
        /// 运行时默认配置（仅内存实例）。静态缓存避免编辑器下反复调用 <see cref="Build" /> 时重复 CreateInstance 造成泄漏。
        /// </summary>
        static UICanvasConfigSO DefaultCanvasConfig
        {
            get
            {
                if (_defaultCanvasConfig == null)
                {
                    _defaultCanvasConfig = UICanvasConfigSO.CreateDefault();
                }

                return _defaultCanvasConfig;
            }
        }

        void EnsureCanvasConfig()
        {
            if (uiCanvasConfigSO != null)
            {
                return;
            }

            uiCanvasConfigSO = DefaultCanvasConfig;
        }

        void ApplyCanvasConfig()
        {
            if (uiCanvasConfigSO == null)
            {
                return;
            }

            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                var canvas = _layerCanvases.GetValueOrDefault(layer);
                if (canvas == null)
                {
                    continue;
                }

                uiCanvasConfigSO.ApplyToCanvas(canvas);
                canvas.sortingOrder = LayerSortOrders[layer];
            }
        }

        void EnsureUICamera()
        {
            var existing = FindChild("UICamera");
            if (existing != null)
            {
                UICamera = existing.GetComponent<Camera>();
                return;
            }

            var camGo = new GameObject("UICamera");
            camGo.transform.SetParent(transform, false);
            UICamera = camGo.AddComponent<Camera>();
            UICamera.clearFlags = CameraClearFlags.Depth;
            UICamera.orthographic = true;
            UICamera.cullingMask = UILayerMask;
            UICamera.depth = 1;
        }

        void EnsureEventSystem()
        {
            // 全场景检查而非仅检查 UIRoot 自身子物体，避免宿主场景已有 EventSystem 时重复创建导致输入事件行为未定义
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var esGo = new GameObject("EventSystem");
            esGo.transform.SetParent(transform, false);
            esGo.AddComponent<EventSystem>();
            if (CreateInputModule != null)
            {
                CreateInputModule(esGo);
            }
            else
            {
                esGo.AddComponent<StandaloneInputModule>();
            }
        }

        void EnsurePresetLayers()
        {
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                var layerName = layer + "Layer";
                if (FindChild(layerName) != null)
                {
                    continue;
                }

                var layerGo = new GameObject(layerName);
                layerGo.transform.SetParent(transform, false);
                layerGo.AddComponent<RectTransform>();
                var canvas = layerGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = UICamera;
                canvas.sortingOrder = LayerSortOrders[layer];
                layerGo.AddComponent<CanvasScaler>();
                layerGo.AddComponent<GraphicRaycaster>();
            }
        }

        static void SetLayerRecursively(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            for (var i = 0; i < root.childCount; i++)
            {
                SetLayerRecursively(root.GetChild(i), layer);
            }
        }

        void CacheLayerCanvases()
        {
            _layerCanvases.Clear();
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                var child = FindChild(layer + "Layer");
                if (child != null)
                {
                    var canvas = child.GetComponent<Canvas>();
                    if (canvas != null)
                    {
                        _layerCanvases[layer] = canvas;
                    }
                }
            }
        }

        Transform FindChild(string childName)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }
            }

            return null;
        }

#if UNITY_EDITOR
        public const string DefaultCanvasConfigPath =
            "Assets/Resources/UIConfig_Default/Default_UICanvasConfig.asset";

        internal const string CreateCanvasConfigAssetMethodName = nameof(CreateAndLoadCanvasConfigAsset);
        public void CreateAndLoadCanvasConfigAsset()
        {
            var existing = AssetDatabase.LoadAssetAtPath<UICanvasConfigSO>(DefaultCanvasConfigPath);
            if (existing != null)
            {
                uiCanvasConfigSO = existing;
                EditorUtility.SetDirty(this);
                AesirModulesDebug.Log(AesirModulesDebug.UIModuleTag,
                    "成功加载默认的 UICanvasConfig 资产到 [UIRoot]，资产路径为：" + DefaultCanvasConfigPath);
                return;
            }

            var dir = Path.GetDirectoryName(DefaultCanvasConfigPath);
            if (!Directory.Exists(dir))
            {
                if (dir != null)
                {
                    Directory.CreateDirectory(dir);
                }

                AssetDatabase.Refresh();
            }

            var config = ScriptableObject.CreateInstance<UICanvasConfigSO>();
            AssetDatabase.CreateAsset(config, DefaultCanvasConfigPath);
            AssetDatabase.SaveAssets();
            uiCanvasConfigSO = config;
            EditorUtility.SetDirty(this);
            AssetDatabase.Refresh();
            AesirModulesDebug.Log(AesirModulesDebug.UIModuleTag,
                "成功创建默认的 UICanvasConfig 资产并加载到 [UIRoot]，路径为：" + DefaultCanvasConfigPath);
        }
#endif
    }
}
