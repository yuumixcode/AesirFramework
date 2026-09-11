using System;
using System.Collections;
using System.Collections.Generic;
using Runestone.AesirArchitecture;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// UI 管理器（MonoBehaviour 单例）。
    /// 负责面板生命周期管理，UI 根节点构建委托给 <see cref="UIRoot" />。
    /// </summary>
    /// <remarks>
    /// 是否加入 DontDestroyOnLoad 场景由序列化字段 <see cref="dontDestroyOnLoad" /> 控制，
    /// 仅在本物体为根物体（场景预放置）时生效；运行时自动创建的实例挂载在 <see cref="AesirModules" /> 宿主下，
    /// 实际是否 DDOL 跟随宿主的 <c>dontDestroyOnLoad</c> 决策。
    /// </remarks>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-999)]
    public class UIModule : AesirMonoBehaviour
    {
        internal const string DontDestroyOnLoadFieldName = nameof(dontDestroyOnLoad);
        static UIModule _instance;

        /// <summary>
        /// 是否将本物体加入 DontDestroyOnLoad 场景。仅在本物体为根物体（场景预放置）时生效。
        /// </summary>
        /// <remarks>
        /// 默认 true（跨场景持久）。设为 false 时实例保留在所在场景、随场景卸载销毁，
        /// 必须自行处理多场景叠加（Additive）加载下的生命周期管理。
        /// 运行时自动创建的实例挂载在 [Aesir Modules] 宿主下（非根物体），
        /// DDOL 跟随宿主决策，本字段不参与判断。
        /// <para>
        /// Inspector 呈现（字段说明 InfoBox 与关闭警告 InfoBox）由
        /// <c>UIModuleAttributeProcessor</c> 动态注入，运行时代码不持有任何 Inspector 样式特性。
        /// </para>
        /// </remarks>
        [SerializeField]
        bool dontDestroyOnLoad = true;

        /// <summary>
        /// 面板实例注册表，键 = 面板实例的实际类型。
        /// 激活/停用状态由面板自身的 <see cref="IUIPanel.IsOpen" /> 承担，注册表不重复记录，
        /// 由此排除多表不一致的可能。Show/Hide/Get 须以同一实际类型调用；
        /// 以基类类型调用且注册表已存在派生实例时给出键语义诊断（见 <see cref="FindRegisteredRelatedPanelKey" />）。
        /// </summary>
        readonly Dictionary<Type, IUIPanel> _panelDict = new Dictionary<Type, IUIPanel>();

        /// <summary>面板预制体注册表，键 = 注册时声明的类型（与实例注册表的实际类型键相互独立）。</summary>
        readonly Dictionary<Type, GameObject> _prefabDict = new Dictionary<Type, GameObject>();

        IUIAssetLoader _loader;
        UIRoot _uiRoot;

        /// <summary>
        /// 全局单例入口。
        /// 优先在已加载场景中查找预放置的实例；未找到时在 <see cref="AesirModules" />（DDOL）下创建子物体。
        /// </summary>
        public static UIModule Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                // 尝试在已加载的场景中查找预放置的实例
                // 使用 FindAnyObjectByType 而非 FindFirstObjectByType，后者因依赖 InstanceID 排序在 Unity 6 中已废弃
                _instance = FindAnyObjectByType<UIModule>();
                if (_instance != null)
                {
                    return _instance;
                }

                // 未找到预放置实例 → 在 AesirModules 下创建（跟随父级 DDOL）
                _instance = AesirModules.GetOrAddChild<UIModule>();
                return _instance;
            }
        }

        /// <summary>
        /// UI 专用相机。正交、depth=1、cullingMask=含 UI 层 (5) 和 TransparentFX 层 (1)。
        /// </summary>
        public Camera UICamera => _uiRoot?.UICamera;

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _loader ??= new ResourcesUILoader();

            // 非根物体（运行时自动创建于 [Aesir Modules] 宿主下）时 DDOL 跟随宿主，本字段不参与判断
            if (!dontDestroyOnLoad)
            {
                AesirModulesDebug.LogWarning(AesirModulesDebug.UIModuleTag,
                    "dontDestroyOnLoad 已关闭：实例保留在所在场景、随场景卸载销毁，" + "必须自行处理多场景叠加（Additive）加载下的生命周期");
            }
            else if (transform.root == transform)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        void OnDestroy()
        {
            if (_instance != null && _instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// 替换默认的面板资源加载器。
        /// </summary>
        /// <param name="loader">自定义加载器。加载契约为同步语义（如同步缓存、Resources）；Addressables 等异步管线需自行预加载后同步返回。</param>
        public void RegisterAssetLoader(IUIAssetLoader loader)
        {
            _loader = loader;
        }

        /// <summary>
        /// 注册面板类型对应的预制体（泛型版本）。
        /// </summary>
        /// <typeparam name="T">面板类型，须实现 <see cref="IUIPanel" />。</typeparam>
        /// <param name="prefab">面板预制体。</param>
        public void RegisterPanelPrefab<T>(GameObject prefab) where T : MonoBehaviour, IUIPanel =>
            RegisterPanelPrefab(typeof(T), prefab);

        /// <summary>
        /// 注册面板类型对应的预制体。
        /// </summary>
        /// <param name="panelType">面板类型。</param>
        /// <param name="prefab">面板预制体。</param>
        public void RegisterPanelPrefab(Type panelType, GameObject prefab)
        {
            if (panelType == null || prefab == null)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.UIModuleTag,
                    "注册预制体资源到 [UIModule] 中失败：panelType 或 prefab 为空");
                return;
            }

            _prefabDict[panelType] = prefab;
        }

        /// <summary>
        /// 打开面板（泛型）。已存在则置顶并重新 Show；不存在则实例化并驱动生命周期。
        /// </summary>
        /// <typeparam name="T">面板类型。</typeparam>
        /// <param name="payload">传递给 OnShow 的数据。</param>
        /// <param name="path">可选的资源路径。注册表中不存在时通过加载器加载，加载后自动注册到注册表。</param>
        /// <returns>面板实例，失败返回 null。</returns>
        public T ShowPanel<T>(object payload = null, string path = null) where T : MonoBehaviour, IUIPanel =>
            ShowPanel(typeof(T), payload, path) as T;

        /// <summary>
        /// 打开面板（泛型 + 强类型 payload）。payload 以泛型参数传递，调用侧获得编译期类型约束；
        /// 面板内部仍经 <see cref="IUIPanel.Show(object)" /> 接收后按需转换（运行时类型安全仍由面板内转换保证）。
        /// </summary>
        /// <typeparam name="TPanel">面板类型。</typeparam>
        /// <typeparam name="TPayload">payload 类型。</typeparam>
        /// <param name="payload">传递给 OnShow 的强类型数据。</param>
        /// <param name="path">可选的资源路径。注册表中不存在时通过加载器加载，加载后自动注册到注册表。</param>
        /// <returns>面板实例，失败返回 null。</returns>
        public TPanel ShowPanel<TPanel, TPayload>(TPayload payload, string path = null)
            where TPanel : MonoBehaviour, IUIPanel =>
            ShowPanel(typeof(TPanel), payload, path) as TPanel;

        /// <summary>
        /// 打开面板。已存在（激活或停用）则置顶并重新 Show；不存在则实例化并驱动生命周期。
        /// <para>
        /// 新面板以停用状态实例化，按 挂层 → <see cref="IUIPanel.Initialize" /> → <see cref="IUIPanel.Show" /> 顺序驱动，
        /// Awake/OnEnable 推迟到 Show 内部激活时才触发，保证 OnEnable 可安全访问 OnInit 之后才有值的引用。
        /// </para>
        /// <para>
        /// 面板注册表以实例的实际类型为键：以基类类型调用且注册表已存在派生实例时记录错误并返回 null
        /// （不会重复实例化）；需以实际类型（或面板内 <see cref="AesirBasePanel.HideSelf" />）操作。
        /// </para>
        /// <para>
        /// 面板所属层的 Canvas 缺失（UIRoot 层级结构性损坏）时记录错误并中止本次显示，不保留半挂载实例。
        /// </para>
        /// </summary>
        /// <param name="panelType">面板类型。</param>
        /// <param name="payload">传递给 OnShow 的数据。</param>
        /// <param name="path">可选的资源路径。注册表中不存在时通过加载器加载，加载后自动注册到注册表。</param>
        /// <returns>面板实例，失败返回 null。</returns>
        public IUIPanel ShowPanel(Type panelType, object payload = null, string path = null)
        {
            EnsureReady();
            if (panelType == null)
            {
                return null;
            }

            if (!_panelDict.TryGetValue(panelType, out var panel))
            {
                // 键语义诊断：注册表已存在派生实例时，按实际类型键约定拒绝本次调用，
                // 防止以基类类型反复 Show 造成重复实例化
                var relatedKey = FindRegisteredRelatedPanelKey(panelType);
                if (relatedKey != null)
                {
                    AesirModulesDebug.LogError(AesirModulesDebug.UIModuleTag,
                        $"面板注册表以实例的实际类型为键：已存在 {relatedKey.Name} 的实例，" +
                        $"请以实际类型调用 ShowPanel（{panelType.Name} 是其基类或接口）");
                    return null;
                }

                panel = InstantiateAndAttach(panelType, path);
                if (panel == null)
                {
                    return null;
                }

                panel.Initialize();
                panel.Show(payload);
                _panelDict[panel.GetType()] = panel;
                return panel;
            }

            // 已存在（激活或停用）：置顶并重新 Show，激活/停用状态由面板自身 IsOpen 记录
            var root = _uiRoot.GetLayerRoot(panel.Layer);
            if (root == null)
            {
                // GetLayerRoot 已记录层缺失错误；层级结构性损坏时中止显示，面板保持原状态
                return null;
            }

            var mono = (MonoBehaviour)panel;
            if (mono.transform.parent != root)
            {
                mono.transform.SetParent(root, false);
            }

            mono.transform.SetAsLastSibling();
            panel.Show(payload);
            return panel;
        }

        /// <summary>
        /// 关闭面板（泛型）。按 <see cref="IUIPanel.DestroyOnHide" /> 决定销毁或隐藏。
        /// </summary>
        /// <typeparam name="T">面板类型。</typeparam>
        public void HidePanel<T>() where T : MonoBehaviour, IUIPanel => HidePanel(typeof(T));

        /// <summary>
        /// 关闭面板。按 <see cref="IUIPanel.DestroyOnHide" /> 决定销毁或隐藏。
        /// <para>
        /// 注册表以面板实例的实际类型为键：以基类类型调用且注册表已存在派生实例时记录警告提示键语义；
        /// 无关联实例时按幂等语义静默返回。已停用（或未显示）的面板重复关闭同样为幂等操作。
        /// </para>
        /// </summary>
        /// <param name="panelType">面板类型。</param>
        public void HidePanel(Type panelType)
        {
            EnsureReady();
            if (panelType == null)
            {
                return;
            }

            if (!_panelDict.TryGetValue(panelType, out var panel))
            {
                var relatedKey = FindRegisteredRelatedPanelKey(panelType);
                if (relatedKey != null)
                {
                    AesirModulesDebug.LogWarning(AesirModulesDebug.UIModuleTag,
                        $"未关闭任何面板：注册表以实例的实际类型为键，已存在 {relatedKey.Name} 的实例，" +
                        $"请以实际类型调用 HidePanel（{panelType.Name} 是其基类或接口）");
                }

                return;
            }

            if (!panel.IsOpen)
            {
                return;
            }

            if (panel.DestroyOnHide)
            {
                _panelDict.Remove(panelType);
                panel.DestroyPanel();
            }
            else
            {
                panel.Hide();
            }
        }

        /// <summary>
        /// 获取已注册的面板实例。键为面板实例的实际类型；精确未命中时静默返回 null，
        /// 仅当注册表存在派生实例（疑似以基类类型误查）时记录键语义警告。
        /// </summary>
        /// <typeparam name="T">面板类型。</typeparam>
        /// <returns>面板实例，未注册返回 null。</returns>
        public T GetPanel<T>() where T : MonoBehaviour, IUIPanel
        {
            var panelType = typeof(T);
            if (_panelDict.TryGetValue(panelType, out var panel))
            {
                return panel as T;
            }

            var relatedKey = FindRegisteredRelatedPanelKey(panelType);
            if (relatedKey != null)
            {
                AesirModulesDebug.LogWarning(AesirModulesDebug.UIModuleTag,
                    $"未获取到面板实例：注册表以实例的实际类型为键，已存在 {relatedKey.Name} 的实例，" +
                    $"请以实际类型调用 GetPanel（{panelType.Name} 是其基类或接口）");
            }

            return null;
        }

        public bool ContainPrefabAsset<T>() where T : MonoBehaviour, IUIPanel
        {
            var panelType = typeof(T);
            return _prefabDict.ContainsKey(panelType);
        }

        /// <summary>
        /// 预热面板（泛型）。预实例化并隐藏面板，后续 <see cref="ShowPanel{T}(object, string)" /> 直接复用，
        /// 避免首次打开时的实例化卡顿。
        /// </summary>
        /// <typeparam name="T">面板类型。</typeparam>
        /// <param name="path">可选的资源路径。注册表中不存在时通过加载器加载，加载后自动注册到注册表。</param>
        /// <returns>预热成功或面板已存在时返回 true。</returns>
        public bool PrewarmPanel<T>(string path = null) where T : MonoBehaviour, IUIPanel =>
            PrewarmPanel(typeof(T), path);

        /// <summary>
        /// 预热面板。预实例化并隐藏面板，后续 <see cref="ShowPanel(Type, object, string)" /> 直接复用，
        /// 避免首次打开时的实例化卡顿。
        /// <para>面板以停用状态实例化，预热期不触发 Awake/OnEnable，待首次 Show 时再激活。</para>
        /// </summary>
        /// <param name="panelType">面板类型。</param>
        /// <param name="path">可选的资源路径。注册表中不存在时通过加载器加载，加载后自动注册到注册表。</param>
        /// <returns>预热成功或面板已存在时返回 true。</returns>
        public bool PrewarmPanel(Type panelType, string path = null)
        {
            EnsureReady();
            if (panelType == null)
            {
                return false;
            }

            if (_panelDict.ContainsKey(panelType))
            {
                return true;
            }

            var panel = InstantiateAndAttach(panelType, path);
            if (panel == null)
            {
                return false;
            }

            panel.Initialize();
            _panelDict[panel.GetType()] = panel;
            return true;
        }

        /// <summary>
        /// 预热所有已注册的面板，逐帧实例化以分摊性能开销。
        /// </summary>
        /// <param name="onComplete">全部预热完成后的回调（可为空）。</param>
        public void PrewarmAll(Action onComplete = null)
        {
            EnsureReady();
            StartCoroutine(PrewarmAllInternal(onComplete));
        }

        /// <summary>
        /// 静态快捷：注册面板预制体。
        /// </summary>
        /// <typeparam name="T">面板类型，须继承 <see cref="AesirBasePanel" />。</typeparam>
        /// <param name="prefab">面板预制体。</param>
        public static void RegisterPrefab<T>(GameObject prefab) where T : MonoBehaviour, IUIPanel =>
            Instance.RegisterPanelPrefab<T>(prefab);

        public static void RegisterPrefab<T>(T prefab) where T : MonoBehaviour, IUIPanel =>
            Instance.RegisterPanelPrefab<T>(prefab.gameObject);

        /// <summary>
        /// 静态快捷：打开面板。
        /// </summary>
        /// <typeparam name="T">面板类型，须继承 <see cref="AesirBasePanel" />。</typeparam>
        /// <param name="payload">传递给 OnShow 的数据。</param>
        /// <param name="path">资源路径，用于加载预制体。使用 UIAssetLoader 加载。</param>
        /// <returns>面板实例。</returns>
        public static T Show<T>(object payload = null, string path = null)
            where T : MonoBehaviour, IUIPanel =>
            Instance.ShowPanel<T>(payload, path);

        /// <summary>
        /// 静态快捷：打开面板（强类型 payload 版本，同 <see cref="ShowPanel{TPanel, TPayload}(TPayload, string)" />）。
        /// </summary>
        /// <typeparam name="TPanel">面板类型。</typeparam>
        /// <typeparam name="TPayload">payload 类型。</typeparam>
        /// <param name="payload">传递给 OnShow 的强类型数据。</param>
        /// <param name="path">资源路径，用于加载预制体。使用 UIAssetLoader 加载。</param>
        /// <returns>面板实例，失败返回 null。</returns>
        public static TPanel Show<TPanel, TPayload>(TPayload payload, string path = null)
            where TPanel : MonoBehaviour, IUIPanel =>
            Instance.ShowPanel<TPanel, TPayload>(payload, path);

        /// <summary>
        /// 静态快捷：关闭面板。
        /// </summary>
        /// <typeparam name="T">面板类型，须继承 <see cref="AesirBasePanel" />。</typeparam>
        public static void Hide<T>() where T : MonoBehaviour, IUIPanel =>
            Instance.HidePanel<T>();

        public static T Get<T>() where T : MonoBehaviour, IUIPanel => Instance.GetPanel<T>();

        public static bool ContainPrefab<T>() where T : MonoBehaviour, IUIPanel =>
            Instance.ContainPrefabAsset<T>();

        /// <summary>
        /// 静态快捷：预热面板。
        /// </summary>
        /// <typeparam name="T">面板类型。</typeparam>
        /// <param name="path">可选的资源路径。注册表中不存在时通过加载器加载，加载后自动注册到注册表。</param>
        /// <returns>预热成功或面板已存在时返回 true。</returns>
        public static bool Prewarm<T>(string path = null) where T : MonoBehaviour, IUIPanel =>
            Instance.PrewarmPanel<T>(path);

        // ---------------- 内部辅助 ----------------

        /// <summary>
        /// 面板实例被销毁时反向清理注册表（由 <see cref="AesirBasePanel.OnDestroy" /> 调用）。
        /// 静态入口避免面板销毁阶段（如场景卸载）触发 <see cref="Instance" /> 的懒创建副作用。
        /// </summary>
        internal static void RemovePanelRecord(IUIPanel panel)
        {
            if (_instance == null)
            {
                return;
            }

            var panelKey = panel.GetType();
            if (_instance._panelDict.TryGetValue(panelKey, out var recorded) && recorded == panel)
            {
                _instance._panelDict.Remove(panelKey);
            }
        }

        /// <summary>
        /// 以停用状态实例化面板预制体并挂载到所属 UI 层（Show 与 Prewarm 共用）。
        /// 预制体缺失、未挂载 <see cref="IUIPanel" /> 组件或所属层 Canvas 缺失时记录错误、清理实例并返回 null。
        /// 克隆体创建时不触发 Awake/OnEnable，保证生命周期严格为
        /// 挂层 → <see cref="IUIPanel.Initialize" /> → <see cref="IUIPanel.Show" />，
        /// Awake/OnEnable 推迟到 Show 内部激活时才触发。
        /// </summary>
        IUIPanel InstantiateAndAttach(Type panelType, string path)
        {
            var prefab = ResolvePrefab(panelType, path);
            if (prefab == null)
            {
                return null;
            }

            var panelGo = InstantiateInactive(prefab);
            var panel = panelGo.GetComponent<IUIPanel>();
            if (panel == null)
            {
                AesirModulesDebug.LogError(panelGo, AesirModulesDebug.UIModuleTag,
                    "预制体[" + prefab.name + "]没有挂载实现了 IUIPanel 的组件");
                Destroy(panelGo);
                return null;
            }

            var root = _uiRoot.GetLayerRoot(panel.Layer);
            if (root == null)
            {
                // GetLayerRoot 已记录层缺失错误；UIRoot 层级结构性损坏时不保留半挂载实例
                Destroy(panelGo);
                return null;
            }

            panelGo.transform.SetParent(root, false);
            panelGo.transform.SetAsLastSibling();
            return panel;
        }

        /// <summary>
        /// 键语义诊断：查找注册表中以 panelType 为基类或接口的实例键（仅精确未命中时调用）。
        /// 返回 null 表示注册表无关联实例，调用方按幂等语义静默处理。
        /// </summary>
        Type FindRegisteredRelatedPanelKey(Type panelType)
        {
            foreach (var key in _panelDict.Keys)
            {
                if (key != panelType && panelType.IsAssignableFrom(key))
                {
                    return key;
                }
            }

            return null;
        }

        /// <summary>
        /// 以停用状态实例化面板预制体：克隆前临时停用源预制体，克隆后立即恢复。
        /// 克隆体创建时不触发 Awake/OnEnable。
        /// </summary>
        GameObject InstantiateInactive(GameObject prefab)
        {
            var wasActive = prefab.activeSelf;
            if (wasActive)
            {
                prefab.SetActive(false);
            }

            var clone = Instantiate(prefab);
            if (wasActive)
            {
                prefab.SetActive(true);
            }

            return clone;
        }

        void EnsureReady()
        {
            if (_uiRoot == null)
            {
                _uiRoot = UIRoot.Instance;
            }
        }

        GameObject ResolvePrefab(Type panelType, string path)
        {
            if (_prefabDict.TryGetValue(panelType, out var prefab))
            {
                return prefab;
            }

            if (path != null)
            {
                prefab = _loader.Load(path);
                if (prefab != null)
                {
                    _prefabDict[panelType] = prefab;
                    return prefab;
                }
            }

            AesirModulesDebug.LogError(AesirModulesDebug.UIModuleTag, $"面板 {panelType.Name} 未注册预制体且未提供 path");
            return null;
        }

        IEnumerator PrewarmAllInternal(Action onComplete)
        {
            var panelTypes = new List<Type>(_prefabDict.Keys);
            for (var i = 0; i < panelTypes.Count; i++)
            {
                if (!PrewarmPanel(panelTypes[i]))
                {
                    AesirModulesDebug.LogWarning(AesirModulesDebug.UIModuleTag,
                        $"面板 {panelTypes[i].Name} 预热失败，已跳过");
                }

                yield return null;
            }

            onComplete?.Invoke();
        }
    }
}
