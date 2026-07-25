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
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-999)]
    public class UIModule : AesirMonoBehaviour
    {
        static UIModule _instance;
        readonly Dictionary<Type, GameObject> _prefabDict = new Dictionary<Type, GameObject>();
        readonly Dictionary<Type, IUIPanel> _uiPanelDict = new Dictionary<Type, IUIPanel>();
        readonly Dictionary<Type, IUIPanel> _activatedPanelDict = new Dictionary<Type, IUIPanel>();
        readonly Dictionary<Type, IUIPanel> _deactivatedPanelDict = new Dictionary<Type, IUIPanel>();

        IUIAssetLoader _loader;
        UIRoot _uiRoot;

        /// <summary>
        /// 全局单例入口。首次访问时自动创建 GameObject 并初始化 UI 根节点。
        /// </summary>
        public static UIModule Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = AesirModules.GetOrAddChild<UIModule>();
                }

                return _instance;
            }
        }

        /// <summary>
        /// UI 专用相机。正交、depth=1、cullingMask=0。
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
        /// 打开面板。已存在则置顶并重新 Show；不存在则实例化并驱动生命周期。
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

                var panelGo = Instantiate(prefab);
                uiPanel = panelGo.GetComponent<IUIPanel>();
                if (uiPanel == null)
                {
                    AesirModulesDebug.LogError(panelGo, AesirModulesDebug.UIModuleTag,
                        "预制体[" + prefab.name + "]没有挂载实现了 IUIPanel 的组件");
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
                _uiPanelDict[panelType] = uiPanel;
                _activatedPanelDict[panelType] = uiPanel;
                return uiPanel;
            }

            if (_activatedPanelDict.TryGetValue(panelType, out var activatedPanel))
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

            if (_deactivatedPanelDict.TryGetValue(panelType, out var deactivatedPanel))
            {
                var root = _uiRoot.GetLayerRoot(deactivatedPanel.Layer);
                if (root != null && ((MonoBehaviour)deactivatedPanel).transform.parent != root)
                {
                    ((MonoBehaviour)deactivatedPanel).transform.SetParent(root, false);
                }

                ((MonoBehaviour)deactivatedPanel).transform.SetAsLastSibling();
                deactivatedPanel.Show(payload);
                _deactivatedPanelDict.Remove(panelType);
                _activatedPanelDict[panelType] = deactivatedPanel;
                return deactivatedPanel;
            }

            return null;
        }

        /// <summary>
        /// 关闭面板（泛型）。按 <see cref="IUIPanel.DestroyOnHide" /> 决定销毁或隐藏。
        /// </summary>
        /// <typeparam name="T">面板类型。</typeparam>
        public void HidePanel<T>() where T : IUIPanel => HidePanel(typeof(T));

        /// <summary>
        /// 关闭面板。按 <see cref="IUIPanel.DestroyOnHide" /> 决定销毁或隐藏。
        /// </summary>
        /// <param name="panelType">面板类型。</param>
        public void HidePanel(Type panelType)
        {
            EnsureReady();
            if (panelType == null || !_uiPanelDict.ContainsKey(panelType))
            {
                return;
            }

            if (_activatedPanelDict.Remove(panelType, out var panel))
            {
                if (panel.DestroyOnHide)
                {
                    _uiPanelDict.Remove(panelType);
                    panel.DestroyPanel();
                }
                else
                {
                    panel.Hide();
                    _deactivatedPanelDict[panelType] = panel;
                }
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

            var panelGo = Instantiate(prefab);
            var uiPanel = panelGo.GetComponent<IUIPanel>();
            if (uiPanel == null)
            {
                AesirModulesDebug.LogError(panelGo, AesirModulesDebug.UIModuleTag,
                    "预制体[" + prefab.name + "]没有挂载实现了 IUIPanel 的组件");
                return false;
            }

            var root = _uiRoot.GetLayerRoot(uiPanel.Layer);
            if (root != null)
            {
                panelGo.transform.SetParent(root, false);
                panelGo.transform.SetAsLastSibling();
            }

            uiPanel.Initialize();
            panelGo.SetActive(false);
            _uiPanelDict[panelType] = uiPanel;
            _deactivatedPanelDict[panelType] = uiPanel;
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
