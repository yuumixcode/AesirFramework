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
    /// <remarks>
    /// 是否加入 DontDestroyOnLoad 场景由序列化字段 <see cref="dontDestroyOnLoad" /> 统一控制，
    /// 场景预放置与运行时创建两种来源共用同一份决策（默认勾选，跨场景持久；预放置为子物体时本字段不参与判断，DDOL 跟随宿主）。
    /// 取消勾选时实例保留在所在场景、随场景卸载销毁——必须自行处理多场景叠加（Additive）加载下的生命周期管理。
    /// </remarks>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-999)]
    public class UIRoot : AesirMonoBehaviour
    {
        /// <summary>
        /// 是否将本物体加入 DontDestroyOnLoad 场景（仅根物体生效；预放置为子物体时本字段不参与判断，DDOL 跟随宿主）。
        /// </summary>
        /// <remarks>
        /// 默认 true（跨场景持久）。设为 false 时实例保留在所在场景、随场景卸载销毁，
        /// 必须自行处理多场景叠加（Additive）加载下的生命周期管理；
        /// 运行时自动创建的实例恒以默认值 true 创建（AddComponent 同步触发 Awake，无法在创建后修改）。
        /// <para>
        /// Inspector 呈现（字段说明 InfoBox 与关闭警告 InfoBox）由
        /// <c>UIRootAttributeProcessor</c> 动态注入，运行时代码不持有任何 Inspector 样式特性。
        /// </para>
        /// </remarks>
        [SerializeField]
        bool dontDestroyOnLoad = true;

        internal const int UILayerIndex = 5;
        const int TransparentFXLayerIndex = 1;
        const int UILayerMask = (1 << UILayerIndex) | (1 << TransparentFXLayerIndex);
        internal const string LayerCanvasesFieldName = nameof(layerCanvases);
        internal const string UICanvasConfigFieldName = nameof(uiCanvasConfigSO);
        internal const string DontDestroyOnLoadFieldName = nameof(dontDestroyOnLoad);

        static UIRoot _instance;
        static UICanvasConfigSO _defaultCanvasConfig;

        /// <summary>
        /// 域加载时重置静态单例，兼容关闭 Domain Reload 的 Play 模式设置。
        /// </summary>
        /// <remarks>
        /// 非泛型类按框架铁律在类内声明 <c>[RuntimeInitializeOnLoadMethod]</c> 自重置，
        /// 而非经 <see cref="ResetStaticsAssistant" />（该助手仅服务泛型类——泛型类中的 RIOLM 会被 Unity 静默跳过）。
        /// <c>_defaultCanvasConfig</c> 是运行时按需创建的默认配置资产（见 <c>CreateDefault</c>），
        /// 非场景引用：解除引用后交由 GC 回收（无强引用），下次访问 <c>DefaultCanvasConfig</c> 时重建。
        /// <c>CreateInputModule</c> 由 Runestone.AesirModules.InputSystem 程序集注册（<c>InputSystemModuleHook</c>：
        /// 编辑器 <c>[InitializeOnLoad]</c> 静态构造 + 运行时 <c>BeforeSceneLoad</c> 两级注册），
        /// 此处清空不会丢注册——本重置跑在 <c>SubsystemRegistration</c>，早于 <c>BeforeSceneLoad</c>。
        /// 仅清空静态引用：既有物体仍留在场景中，下次 <see cref="Instance" /> 访问经
        /// <c>FindAnyObjectByType</c> 兜底重发现。
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _instance = null;
            _defaultCanvasConfig = null;
            CreateInputModule = null;
        }

        static readonly Dictionary<UILayer, int> LayerSortOrders = new Dictionary<UILayer, int>
        {
            { UILayer.Background, 100 },
            { UILayer.Normal, 200 },
            { UILayer.Popup, 300 },
            { UILayer.Top, 400 }
        };

        /// <summary>
        /// 层级枚举值缓存。静态初始化一次，避免每次构建调用 <see cref="Enum.GetValues" /> 产生装箱分配。
        /// </summary>
        static readonly UILayer[] PresetLayers = (UILayer[])Enum.GetValues(typeof(UILayer));

        [SerializeField]
        UICanvasConfigSO uiCanvasConfigSO;

        /// <summary>
        /// UI 专用相机引用。首次构建时赋值并随场景序列化持久，后续初始化引用非空即跳过，不按物体名查找。
        /// </summary>
        [SerializeField]
        [HideInInspector]
        Camera uiCamera;

        /// <summary>
        /// 自建 EventSystem 引用。非空即跳过全场景存在性扫描；
        /// 宿主场景已有外来 EventSystem 时不持有引用（不归本组件管理）。
        /// </summary>
        [SerializeField]
        [HideInInspector]
        EventSystem eventSystem;

        /// <summary>
        /// 层级 Canvas 引用表。首次构建时赋值并随场景序列化持久；
        /// 存在性判定只依赖引用非空（Unity 假 null 即子物体已销毁、需重建），不按物体名查找。
        /// </summary>
        /// <remarks>
        /// Inspector 呈现（仅运行时显示）由 <c>UIRootAttributeProcessor</c> 注入；
        /// <see cref="HideInInspector" /> 兜底非 Odin 环境的默认 Inspector，运行时代码不持有 Inspector 样式特性。
        /// </remarks>
        [SerializeField]
        [HideInInspector]
        List<LayerCanvasEntry> layerCanvases = new List<LayerCanvasEntry>();

        /// <summary>
        /// 自定义输入模块创建回调。
        /// 由 Runestone.AesirModules.InputSystem 程序集在 InputSystem 启用时注册，
        /// 使用 InputSystemUIInputModule 替代默认的 StandaloneInputModule。
        /// 为 null 时使用 StandaloneInputModule。
        /// </summary>
        public static Action<GameObject> CreateInputModule { get; set; }

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
                // 含未激活对象：未激活的预放置实例不被 Awake 赋值，Exclude 会让它被判为不存在而重复创建（Inspector 配置随之失效）
                _instance = FindAnyObjectByType<UIRoot>(FindObjectsInactive.Include);
                if (_instance != null)
                {
                    return _instance;
                }

                // 未找到预放置实例 → 运行时创建；AddComponent 同步触发 Awake，
                // 由 dontDestroyOnLoad 默认值（true）决定自动加入 DDOL 场景
                var go = new GameObject("[UIRoot]");
                _instance = go.AddComponent<UIRoot>();
                return _instance;
            }
        }

        public Camera UICamera => uiCamera;

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

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                // 对齐 RAA 与 RAM 其余模块的销毁粒度（AesirArchitecture / MonoLifecycleProxy / UIModule /
                // AudioModule / EventModule / SceneModule）：只销毁本组件，不连带销毁用户物体上的其它组件，
                // 也不连带销毁已按约定搭好的四层 Canvas 层级
                Destroy(this);
                return;
            }

            _instance = this;

            if (dontDestroyOnLoad && transform.root == transform)
            {
                // 仅根物体可加入 DDOL 场景；预放置为子物体时本字段不参与判断，DDOL 跟随宿主，与 UIModule 范式同形
                DontDestroyOnLoad(gameObject);
            }

            // UIModule 不在此处反向注册（推方向）：
            // UIModule.EnsureReady 在每次面板操作时经 UIRoot.Instance 惰性发现本实例（拉方向，已可自愈），
            // 推注册会在编辑器菜单创建 UIRoot 时连带触发 UIModule → [Aesir Modules] 宿主的运行时懒创建链，
            // 使运行时创建路径的物体被意外固化进场景。
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
        /// 构建 UI 层级结构。幂等调用：引用非空的物体直接跳过（子物体重命名不受影响），
        /// 引用缺失时按约定名回收旧版已搭建的子物体，不会重复创建。
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
        /// 获取指定层级的根 Transform。层 Canvas 引用缺失（子物体被删除或引用丢失）时记录错误并返回 null；
        /// 子物体被重命名不受影响（引用与名称解耦）。
        /// </summary>
        public Transform GetLayerRoot(UILayer layer)
        {
            var canvas = FindLayerCanvas(layer);
            if (canvas == null)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.UIModuleTag,
                    $"UIRoot 缺少 {layer} 层的 Canvas（层引用缺失或对应子物体被删除），" + "面板将无法挂载到该层，请重建 UIRoot 层级");
                return null;
            }

            return canvas.transform;
        }

        /// <summary>
        /// 统一 Canvas 配置（层 Canvas 与窗口接线共用）。未设置时回退默认配置。
        /// </summary>
        internal UICanvasConfigSO CanvasConfig
        {
            get
            {
                if (uiCanvasConfigSO == null)
                {
                    EnsureCanvasConfig();
                }

                return uiCanvasConfigSO;
            }
        }

        void EnsureUIComponents()
        {
            EnsureUICamera();
            EnsureEventSystem();
            EnsurePresetLayers();
            SetLayerRecursively(transform, UILayerIndex);
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

            foreach (var entry in layerCanvases)
            {
                if (entry.canvas == null)
                {
                    continue;
                }

                uiCanvasConfigSO.ApplyToCanvas(entry.canvas);
                entry.canvas.sortingOrder = LayerSortOrders[entry.layer];
            }
        }

        void EnsureUICamera()
        {
            if (uiCamera != null)
            {
                return;
            }

            // 兼容旧版已搭建层级：引用缺失时按约定名一次性回收既有子物体（保存场景后引用持久化，此后不再按名查找）
            var existing = FindChild("UICamera");
            if (existing != null)
            {
                uiCamera = existing.GetComponent<Camera>();
                return;
            }

            var camGo = new GameObject("UICamera");
            camGo.transform.SetParent(transform, false);
            uiCamera = camGo.AddComponent<Camera>();
            uiCamera.clearFlags = CameraClearFlags.Depth;
            uiCamera.orthographic = true;
            uiCamera.cullingMask = UILayerMask;
            uiCamera.depth = 1;
        }

        void EnsureEventSystem()
        {
            if (eventSystem != null)
            {
                return;
            }

            // 全场景检查而非仅检查 UIRoot 自身子物体，避免宿主场景已有 EventSystem 时重复创建导致输入事件行为未定义
            // 含未激活对象：被禁用的既有 EventSystem 同样属于"已存在"，Exclude 会让人误判缺失而再造一个
            if (FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            var esGo = new GameObject("EventSystem");
            esGo.transform.SetParent(transform, false);
            eventSystem = esGo.AddComponent<EventSystem>();
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
            foreach (var layer in PresetLayers)
            {
                // 引用非空即视为该层已存在（含子物体被重命名的情况），不做任何查找
                var canvas = FindLayerCanvas(layer);
                if (canvas != null)
                {
                    continue;
                }

                var layerName = layer + "Layer";

                // 兼容旧版已搭建层级：引用缺失时按约定名回收既有子物体，避免重复创建；
                // 同名子物体存在但缺少 Canvas 时就地补齐（既不重复创建，也不静默跳过——跳过会让该层永久缺失）
                var existing = FindChild(layerName);
                if (existing != null)
                {
                    SetLayerCanvas(layer, BuildLayerCanvas(existing, layer));
                    continue;
                }

                var layerGo = new GameObject(layerName);
                layerGo.transform.SetParent(transform, false);
                SetLayerCanvas(layer, BuildLayerCanvas(layerGo.transform, layer));
            }
        }

        /// <summary>
        /// 在指定层物体上补齐并配置层 Canvas（新建层与"同名子物体缺少 Canvas"的修复路径共用，
        /// 避免两条路径行为漂移）。已具备的组件不重复添加。
        /// </summary>
        Canvas BuildLayerCanvas(Transform layerTransform, UILayer layer)
        {
            var layerGo = layerTransform.gameObject;

            // 层根须为 RectTransform：新建层由普通 GameObject 转换，已是 RectTransform 的既有层跳过
            if (!(layerTransform is RectTransform))
            {
                layerGo.AddComponent<RectTransform>();
            }

            var canvas = layerGo.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = layerGo.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiCamera;
            canvas.sortingOrder = LayerSortOrders[layer];

            if (layerGo.GetComponent<CanvasScaler>() == null)
            {
                layerGo.AddComponent<CanvasScaler>();
            }

            if (layerGo.GetComponent<GraphicRaycaster>() == null)
            {
                layerGo.AddComponent<GraphicRaycaster>();
            }

            return canvas;
        }

        internal static void SetLayerRecursively(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            for (var i = 0; i < root.childCount; i++)
            {
                SetLayerRecursively(root.GetChild(i), layer);
            }
        }

        /// <summary>
        /// 从序列化引用表中查找指定层的 Canvas。引用已销毁时返回 Unity 假 null，由调用方按"层缺失"处理。
        /// </summary>
        Canvas FindLayerCanvas(UILayer layer)
        {
            foreach (var entry in layerCanvases)
            {
                if (entry.layer == layer)
                {
                    return entry.canvas;
                }
            }

            return null;
        }

        /// <summary>
        /// 写入或替换指定层的 Canvas 引用。子物体被删除重建时覆盖同层的失效引用，避免残留重复条目。
        /// </summary>
        void SetLayerCanvas(UILayer layer, Canvas canvas)
        {
            for (var i = 0; i < layerCanvases.Count; i++)
            {
                if (layerCanvases[i].layer == layer)
                {
                    layerCanvases[i] = new LayerCanvasEntry(layer, canvas);
                    return;
                }
            }

            layerCanvases.Add(new LayerCanvasEntry(layer, canvas));
        }

        /// <summary>
        /// 仅服务于旧版层级的一次性回收：<see cref="EnsureUICamera" /> 与 <see cref="EnsurePresetLayers" />
        /// 在引用缺失时按约定名找回既有子物体；引用就绪后不再调用。
        /// </summary>
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

        /// <summary>
        /// 层级 Canvas 的序列化引用条目。Unity 无法序列化字典，改以列表存储（条目数恒等于层数，运行时线性查找即可）。
        /// </summary>
        [Serializable]
        struct LayerCanvasEntry
        {
            // C# 包含类无法访问嵌套类型的 private 成员，序列化数据载体字段以 internal 暴露给 UIRoot
            [SerializeField]
            internal UILayer layer;

            [SerializeField]
            internal Canvas canvas;

            public LayerCanvasEntry(UILayer layer, Canvas canvas)
            {
                this.layer = layer;
                this.canvas = canvas;
            }
        }

#if UNITY_EDITOR
        public const string DefaultCanvasConfigPath =
            "Assets/Resources/UIConfig_Default/Default_UICanvasConfig.asset";

        internal const string CreateCanvasConfigAssetMethodName = nameof(CreateAndLoadCanvasConfigAsset);

        /// <summary>
        /// 确保默认 UICanvasConfig 资产存在：已存在则直接加载返回，不存在则创建目录与资产。
        /// 幂等操作，供 Inspector 按钮（<see cref="CreateAndLoadCanvasConfigAsset" />）与
        /// Assets/Create 菜单（UIModuleMenuItems）共用，避免两份等价实现产生行为漂移。
        /// </summary>
        public static UICanvasConfigSO EnsureDefaultCanvasConfigAsset()
        {
            var existing = AssetDatabase.LoadAssetAtPath<UICanvasConfigSO>(DefaultCanvasConfigPath);
            if (existing != null)
            {
                return existing;
            }

            var dir = Path.GetDirectoryName(DefaultCanvasConfigPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }

            var config = ScriptableObject.CreateInstance<UICanvasConfigSO>();
            AssetDatabase.CreateAsset(config, DefaultCanvasConfigPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return config;
        }

        /// <summary>
        /// Inspector 按钮（Odin）：确保默认资产存在并加载到 [UIRoot]（幂等）。
        /// </summary>
        public void CreateAndLoadCanvasConfigAsset()
        {
            uiCanvasConfigSO = EnsureDefaultCanvasConfigAsset();
            EditorUtility.SetDirty(this);
            AesirModulesDebug.Log(AesirModulesDebug.UIModuleTag,
                "成功加载默认的 UICanvasConfig 资产到 [UIRoot]，资产路径为：" + DefaultCanvasConfigPath);
        }
#endif
    }
}
