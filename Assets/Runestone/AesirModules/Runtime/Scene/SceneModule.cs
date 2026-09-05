using System;
using System.Collections;
using System.Collections.Generic;
using Runestone.AesirArchitecture;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Runestone.AesirModules
{
    /// <summary>
    /// 场景加载与叠加管理模块。
    /// <para>
    /// 语义对齐 Unity 原生 LoadSceneMode：Single 卸载全部场景并重设激活场景；
    /// Additive 纯叠加、不改变激活场景，叠加场景统一记入追踪列表（UnloadScene 卸载时自动移出）。
    /// Addressable 场景（<see cref="SceneAssetWrapperState.Addressable" />）不归本模块加载，
    /// 请通过 Addressables API 加载。
    /// </para>
    /// </summary>
    public class SceneModule : AesirMonoBehaviour
    {
#if ODIN_INSPECTOR
        [DetailedInfoBox("SceneModule 内部机制...",
            "SceneModule 内部有预设启动场景名称数组，如果没有自定义启动场景，将会自动搜索 BuildSettings，查找启动场景")]
        [LabelText("自定义启动场景")]
#endif
        [SerializeField]
        SceneAssetWrapper bootstrapScene;

        /// <summary>
        /// 叠加场景路径列表，追踪所有经本模块 Additive 加载、尚未卸载的场景
        /// </summary>
        readonly List<string> _addedScenePaths = new List<string>();

        /// <summary>
        /// 当前正在进行的异步加载操作列表
        /// </summary>
        readonly List<AsyncOperation> _loadingOperations = new List<AsyncOperation>();

        /// <summary>
        /// 预设的启动场景名称数组（运行时自动搜索与编辑器 BootstrapSceneHelper 共用的单一事实来源）
        /// </summary>
        public static readonly string[] PresetBootstrapSceneNames =
        {
            "Bootstrap", "BootstrapScene", "Bootstrapper", "BootstrapperScene", "bootstrap_scene",
            "bootstrap", "bootstrapper_scene", "bootstrapper"
        };

        /// <summary>
        /// 启动场景引用，可以获取路径和名称
        /// </summary>
        public SceneAssetWrapper BootstrapSceneAssetWrapper => bootstrapScene;

        /// <summary>
        /// 启动场景，Scene 结构体
        /// </summary>
        public Scene BootstrapScene { get; private set; }

        /// <summary>
        /// 最后一个已经加载的场景，Scene 结构体
        /// </summary>
        public Scene LastLoadedScene { get; private set; }

        /// <summary>
        /// 叠加场景路径（只读）。含所有经本模块 Additive 加载、尚未卸载的场景。
        /// </summary>
        public IReadOnlyList<string> AddedScenePaths => _addedScenePaths;

        #region 公共方法

        /// <summary>
        /// 获取当前所有加载进度 (0-1) 的平均值
        /// </summary>
        public float GetTotalLoadingProgress()
        {
            var count = _loadingOperations.Count;
            if (count == 0)
            {
                return 1f;
            }

            var total = 0f;
            for (var i = 0; i < _loadingOperations.Count; i++)
            {
                var op = _loadingOperations[i];
                if (op != null)
                {
                    total += op.progress;
                }
            }

            return total / count;
        }

        /// <summary>
        /// 加载场景。Single 模式：卸载全部场景、重设激活场景、清空叠加追踪。可传入完成/失败回调。
        /// </summary>
        public void LoadSceneSingle(string scenePath, Action onCompleted = null, Action onFailed = null)
        {
            StartCoroutine(LoadSceneInternal(scenePath, onCompleted, onFailed, LoadSceneMode.Single));
        }

        /// <summary>
        /// 加载场景。Single 模式。通过 <see cref="SceneAssetWrapper" /> 指定场景。
        /// 引用无效（空/不在 BuildSettings）或为 Addressable 场景时走失败回调。
        /// </summary>
        public void LoadSceneSingle(SceneAssetWrapper sceneRef, Action onCompleted = null,
            Action onFailed = null)
        {
            if (!TryGetLoadablePath(sceneRef, onFailed, out var path))
            {
                return;
            }

            LoadSceneSingle(path, onCompleted, onFailed);
        }

        /// <summary>
        /// 加载场景。Additive 模式：纯叠加、不改变激活场景（对齐 Unity 原生语义），并记入叠加追踪。
        /// 可传入完成/失败回调。
        /// </summary>
        public void LoadSceneAdditive(string scenePath, Action onCompleted = null, Action onFailed = null)
        {
            StartCoroutine(LoadSceneInternal(scenePath, onCompleted, onFailed, LoadSceneMode.Additive));
        }

        /// <summary>
        /// 加载场景。Additive 模式。通过 <see cref="SceneAssetWrapper" /> 指定场景。
        /// 引用无效（空/不在 BuildSettings）或为 Addressable 场景时走失败回调。
        /// </summary>
        public void LoadSceneAdditive(SceneAssetWrapper sceneRef, Action onCompleted = null,
            Action onFailed = null)
        {
            if (!TryGetLoadablePath(sceneRef, onFailed, out var path))
            {
                return;
            }

            LoadSceneAdditive(path, onCompleted, onFailed);
        }

        /// <summary>
        /// 卸载场景。若该场景在叠加追踪列表中则自动移出。可传入卸载完成/失败回调。
        /// </summary>
        public void UnloadScene(string scenePath, Action onUnloaded = null, Action onFailed = null)
        {
            StartCoroutine(UnloadSceneInternal(scenePath, onUnloaded, onFailed));
        }

        /// <summary>
        /// 卸载场景。通过 <see cref="SceneAssetWrapper" /> 指定场景。
        /// </summary>
        public void UnloadScene(SceneAssetWrapper sceneRef, Action onUnloaded = null, Action onFailed = null)
        {
            if (sceneRef == null || !sceneRef.TryGetScenePath(out var path))
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag, "场景引用为空，无法卸载。");
                onFailed?.Invoke();
                return;
            }

            UnloadScene(path, onUnloaded, onFailed);
        }

        /// <summary>
        /// 重新加载当前激活场景。异步 Single 模式，会清空叠加场景追踪。
        /// 编辑器中激活场景尚未保存（无有效路径）时走失败回调。
        /// </summary>
        public void ReloadScene(Action onCompleted = null, Action onFailed = null)
        {
            var path = SceneManager.GetActiveScene().path;
            if (string.IsNullOrEmpty(path))
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag,
                    "当前激活场景未保存（无有效路径），无法重载。");
                onFailed?.Invoke();
                return;
            }

            LoadSceneSingle(path, onCompleted, onFailed);
        }

        /// <summary>
        /// 卸载所有经本模块叠加加载的场景。可传入全部卸载完成回调。
        /// </summary>
        public void UnloadAllAddedScenes(Action onAllUnloaded = null)
        {
            StartCoroutine(UnloadAllAddedScenesInternal(onAllUnloaded));
        }

        #endregion

        #region 单例 & 生命周期

        static SceneModule _instance;

        /// <summary>
        /// 全局单例入口。
        /// 优先在已加载场景中查找预放置的实例；未找到时在 <see cref="AesirModules" />（DDOL）下创建子物体。
        /// </summary>
        public static SceneModule Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                // 尝试在已加载的场景中查找预放置的实例
                // 使用 FindAnyObjectByType 而非 FindFirstObjectByType，后者因依赖 InstanceID 排序在 Unity 6 中已废弃
                _instance = FindAnyObjectByType<SceneModule>();
                if (_instance != null)
                {
                    return _instance;
                }

                // 未找到预放置实例 → 在 AesirModules 下创建（跟随父级 DDOL）
                _instance = AesirModules.GetOrAddChild<SceneModule>();
                return _instance;
            }
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            AutoSetupBootstrapScene();
        }

        void OnDestroy()
        {
            if (_instance != null && _instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region 内部方法

        /// <summary>
        /// 校验 <see cref="SceneAssetWrapper" /> 能否经 BuildSettings 途径加载：
        /// 空引用、不安全引用（不在 BuildSettings）、Addressable 场景都会被拒绝并触发失败回调。
        /// </summary>
        static bool TryGetLoadablePath(SceneAssetWrapper sceneRef, Action onFailed, out string path)
        {
            path = null;
            if (sceneRef == null)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag,
                    "场景引用为空（SceneAssetWrapper == null）。");
                onFailed?.Invoke();
                return false;
            }

            var state = sceneRef.State;
            if (state == SceneAssetWrapperState.Addressable)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag,
                    $"场景 {sceneRef} 为 Addressable 场景，SceneModule 无法加载，请通过 Addressables API 加载。");
                onFailed?.Invoke();
                return false;
            }

            if (!sceneRef.TryGetScenePath(out path) || state == SceneAssetWrapperState.Unsafe)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag,
                    $"场景引用无效（不在 BuildSettings）：{sceneRef}");
                onFailed?.Invoke();
                return false;
            }

            return true;
        }

        IEnumerator LoadSceneInternal(string scenePath, Action onCompleted, Action onFailed,
            LoadSceneMode mode)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag, $"无效场景路径: {scenePath}");
                onFailed?.Invoke();
                yield break;
            }

            // Single 模式会卸载所有场景，清空叠加场景追踪
            if (mode == LoadSceneMode.Single)
            {
                _addedScenePaths.Clear();
            }

            var op = SceneManager.LoadSceneAsync(scenePath, mode);
            if (op == null)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag, $"无法加载场景: {scenePath}");
                onFailed?.Invoke();
                yield break;
            }

            _loadingOperations.Add(op);
            yield return op;
            _loadingOperations.Remove(op);

            LastLoadedScene = SceneManager.GetSceneByPath(scenePath);
            if (mode == LoadSceneMode.Single)
            {
                // 仅 Single 模式改变激活场景（对齐 Unity 原生 Additive 语义）
                SceneManager.SetActiveScene(LastLoadedScene);
            }
            else if (!_addedScenePaths.Contains(scenePath))
            {
                // 重复叠加同一路径时按路径粒度只追踪一次
                _addedScenePaths.Add(scenePath);
            }

            onCompleted?.Invoke();
        }

        IEnumerator UnloadSceneInternal(string scenePath, Action onUnloaded, Action onFailed)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag, $"无效场景路径: {scenePath}");
                onFailed?.Invoke();
                yield break;
            }

            var op = SceneManager.UnloadSceneAsync(scenePath);
            if (op == null)
            {
                AesirModulesDebug.LogWarning(AesirModulesDebug.SceneModuleTag,
                    $"场景卸载失败或场景不存在: {scenePath}");
                onFailed?.Invoke();
                yield break;
            }

            yield return op;
            _addedScenePaths.RemoveAll(p => p == scenePath);
            onUnloaded?.Invoke();
        }

        IEnumerator UnloadAllAddedScenesInternal(Action onAllUnloaded)
        {
            for (var i = 0; i < _addedScenePaths.Count; i++)
            {
                yield return SceneManager.UnloadSceneAsync(_addedScenePaths[i]);
            }

            _addedScenePaths.Clear();
            onAllUnloaded?.Invoke();
        }

        void AutoSetupBootstrapScene()
        {
            if (bootstrapScene != null && bootstrapScene.TryGetScenePath(out var path))
            {
                BootstrapScene = SceneManager.GetSceneByPath(path);
            }
            else
            {
                for (var i = 0; i < PresetBootstrapSceneNames.Length; i++)
                {
                    var scene = SceneManager.GetSceneByName(PresetBootstrapSceneNames[i]);
                    if (!scene.IsValid())
                    {
                        continue;
                    }

                    BootstrapScene = scene;
                    break;
                }
            }
        }

        #endregion
    }
}
