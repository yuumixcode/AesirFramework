#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
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
                if (_instance == null)
                {
                    var go = new GameObject("[UIRoot]");
                    _instance = go.AddComponent<UIRoot>();
                }

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
            DontDestroyOnLoad(gameObject);
            UIModule.Instance.RegisterUIRoot(this);
            Initialize();
        }

        void Initialize()
        {
            EnsureUIComponents();
            EnsureCanvasConfig();
            ApplyCanvasConfig();
        }

        /// <summary>
        /// 构建 UI 层级结构。幂等调用，已存在的子物体不会重复创建。
        /// </summary>
        public void Build()
        {
            EnsureUIComponents();
            var defaultUICanvasConfigSO = ScriptableObject.CreateInstance<UICanvasConfigSO>();
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                var canvas = _layerCanvases.GetValueOrDefault(layer);
                if (canvas == null)
                {
                    continue;
                }

                defaultUICanvasConfigSO.ApplyToCanvas(canvas);
                canvas.sortingOrder = LayerSortOrders[layer];
            }
        }

        /// <summary>
        /// 获取指定层级的根 Transform。
        /// </summary>
        public Transform GetLayerRoot(UILayer layer)
        {
            var canvas = _layerCanvases.GetValueOrDefault(layer);
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

        void EnsureCanvasConfig()
        {
            if (uiCanvasConfigSO != null)
            {
                return;
            }

            uiCanvasConfigSO = UICanvasConfigSO.CreateDefault();
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
            if (FindChild("EventSystem") != null)
            {
                return;
            }

            var esGo = new GameObject("EventSystem");
            esGo.transform.SetParent(transform, false);
            esGo.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            esGo.AddComponent<InputSystemUIInputModule>();
#else
            esGo.AddComponent<StandaloneInputModule>();
#endif
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
