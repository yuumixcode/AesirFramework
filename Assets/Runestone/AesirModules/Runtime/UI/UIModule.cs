using System;
using System.Collections;
using System.Collections.Generic;
using Runestone.AesirArchitecture;
using UnityEngine;
#if ODIN_INSPECTOR
#endif

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

        readonly Dictionary<Type, IUIPanel> _activatedPanelDict = new Dictionary<Type, IUIPanel>();
        readonly Dictionary<Type, IUIPanel> _deactivatedPanelDict = new Dictionary<Type, IUIPanel>();
        readonly Dictionary<Type, GameObject> _prefabDict = new Dictionary<Type, GameObject>();
        readonly Dictionary<Type, IUIPanel> _uiPanelDict = new Dictionary<Type, IUIPanel>();

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
        /// <param name="loader">自定义加载器（如 Addressables 实现）。</param>
        public void RegisterAssetLoader(IUIAssetLoader loader)
        {
            _loader = loader;
        }

        public void RegisterUIRoot(UIRoot uiRoot)
        {
            _uiRoot = uiRoot;
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
        /// 面板内部仍经 <see cref="IUIPanel.Show(object)" /> 接收后按需转换。
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
        /// 打开面板。已存在则置顶并重新 Show；不存在则实例化并驱动生命周期。
        /// <para>
        /// 新面板以停用状态实例化，按 挂层 → <see cref="IUIPanel.Initialize" /> → <see cref="IUIPanel.Show" /> 顺序驱动，
        /// Awake/OnEnable 推迟到 Show 内部激活时才触发，保证 OnEnable 可安全访问 OnInit 之后才有值的引用。
        /// </para>
        /// <para>
        /// 面板注册以实例的实际类型为键；以基类类型 Show 后，需以实际类型（或面板内 <see cref="AesirBasePanel.HideSelf" />）关闭。
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

            if (!_uiPanelDict.TryGetValue(panelType, out var uiPanel))
            {
                var prefab = ResolvePrefab(panelType, path);
                if (prefab == null)
                {
                    return null;
                }

                var panelGo = InstantiateInactive(prefab);
                uiPanel = panelGo.GetComponent<IUIPanel>();
                if (uiPanel == null)
                {
                    AesirModulesDebug.LogError(panelGo, AesirModulesDebug.UIModuleTag,
                        "预制体[" + prefab.name + "]没有挂载实现了 IUIPanel 的组件");
                    Destroy(panelGo);
                    return null;
                }

                var root = _uiRoot.GetLayerRoot(uiPanel.Layer);
                if (root != null)
                {
                    panelGo.transform.SetParent(root, false);
                    panelGo.transform.SetAsLastSibling();
                }

                uiPanel.Initialize();
                uiPanel.Show(payload);
                var panelKey = uiPanel.GetType();
                _uiPanelDict[panelKey] = uiPanel;
                _activatedPanelDict[panelKey] = uiPanel;
                return uiPanel;
            }

            var key = uiPanel.GetType();
            if (_activatedPanelDict.TryGetValue(key, out var activatedPanel) && activatedPanel == uiPanel)
            {
                var root = _uiRoot.GetLayerRoot(activatedPanel.Layer);
                if (root != null && ((MonoBehaviour)activatedPanel).transform.parent != root)
                {
                    ((MonoBehaviour)activatedPanel).transform.SetParent(root, false);
                }

                ((MonoBehaviour)activatedPanel).transform.SetAsLastSibling();
                activatedPanel.Show(payload);
                return activatedPanel;
            }

            if (_deactivatedPanelDict.TryGetValue(key, out var deactivatedPanel) &&
                deactivatedPanel == uiPanel)
            {
                var root = _uiRoot.GetLayerRoot(deactivatedPanel.Layer);
                if (root != null && ((MonoBehaviour)deactivatedPanel).transform.parent != root)
                {
                    ((MonoBehaviour)deactivatedPanel).transform.SetParent(root, false);
                }

                ((MonoBehaviour)deactivatedPanel).transform.SetAsLastSibling();
                deactivatedPanel.Show(payload);
                _deactivatedPanelDict.Remove(key);
                _activatedPanelDict[key] = deactivatedPanel;
                return deactivatedPanel;
            }

            AesirModulesDebug.LogError(AesirModulesDebug.UIModuleTag,
                $"面板 {key.Name} 存在于实例注册表，但既不在激活表也不在停用表，内部状态异常，无法显示");
            return null;
        }

        /// <summary>
        /// 关闭面板（泛型）。按 <see cref="IUIPanel.DestroyOnHide" /> 决定销毁或隐藏。
        /// </summary>
        /// <typeparam name="T">面板类型。</typeparam>
        public void HidePanel<T>() where T : IUIPanel => HidePanel(typeof(T));

        /// <summary>
        /// 关闭面板。按 <see cref="IUIPanel.DestroyOnHide" /> 决定销毁或隐藏。
        /// <para>状态字典以面板实例的实际类型为键，与 <see cref="ShowPanel(Type, object, string)" /> 的注册键保持一致。</para>
        /// </summary>
        /// <param name="panelType">面板类型。</param>
        public void HidePanel(Type panelType)
        {
            EnsureReady();
            if (panelType == null || !_uiPanelDict.TryGetValue(panelType, out var panel))
            {
                return;
            }

            var panelKey = panel.GetType();
            if (!_activatedPanelDict.TryGetValue(panelKey, out var activated) || activated != panel)
            {
                return;
            }

            _activatedPanelDict.Remove(panelKey);
            if (panel.DestroyOnHide)
            {
                _uiPanelDict.Remove(panelKey);
                panel.DestroyPanel();
            }
            else
            {
                panel.Hide();
                _deactivatedPanelDict[panelKey] = panel;
            }
        }

        public T GetPanel<T>() where T : MonoBehaviour, IUIPanel
        {
            var panelType = typeof(T);
            if (_uiPanelDict.TryGetValue(panelType, out var panel))
            {
                return panel as T;
            }

            AesirModulesDebug.LogWarning(AesirModulesDebug.UIModuleTag,
                $"无法获取到 {panelType.Name}。请确保面板已完成实例化。");
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

            if (_uiPanelDict.ContainsKey(panelType))
            {
                return true;
            }

            var prefab = ResolvePrefab(panelType, path);
            if (prefab == null)
            {
                return false;
            }

            var panelGo = InstantiateInactive(prefab);
            var uiPanel = panelGo.GetComponent<IUIPanel>();
            if (uiPanel == null)
            {
                AesirModulesDebug.LogError(panelGo, AesirModulesDebug.UIModuleTag,
                    "预制体[" + prefab.name + "]没有挂载实现了 IUIPanel 的组件");
                Destroy(panelGo);
                return false;
            }

            var root = _uiRoot.GetLayerRoot(uiPanel.Layer);
            if (root != null)
            {
                panelGo.transform.SetParent(root, false);
                panelGo.transform.SetAsLastSibling();
            }

            uiPanel.Initialize();
            var panelKey = uiPanel.GetType();
            _uiPanelDict[panelKey] = uiPanel;
            _deactivatedPanelDict[panelKey] = uiPanel;
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
            if (_instance._uiPanelDict.TryGetValue(panelKey, out var recorded) && recorded == panel)
            {
                _instance._uiPanelDict.Remove(panelKey);
            }

            if (_instance._activatedPanelDict.TryGetValue(panelKey, out var activated) && activated == panel)
            {
                _instance._activatedPanelDict.Remove(panelKey);
            }

            if (_instance._deactivatedPanelDict.TryGetValue(panelKey, out var deactivated) &&
                deactivated == panel)
            {
                _instance._deactivatedPanelDict.Remove(panelKey);
            }
        }

        /// <summary>
        /// 以停用状态实例化面板预制体：克隆前临时停用源预制体，克隆后立即恢复。
        /// 克隆体创建时不触发 Awake/OnEnable，保证生命周期严格为
        /// 挂层 → <see cref="IUIPanel.Initialize" /> → <see cref="IUIPanel.Show" />，
        /// Awake/OnEnable 推迟到 Show 内部激活时才触发。
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
