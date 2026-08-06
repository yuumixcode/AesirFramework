using System;
using System.Collections;
using System.Collections.Generic;
using Runestone.AesirArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runestone.AesirModules
{
    public class SceneModule : AesirMonoBehaviour
    {
        [DetailedInfoBox("SceneModule 内部机制...",
            "SceneModule 内部有预设启动场景名称数组，如果没有自定义启动场景，将会自动搜索 BuildSettings，查找启动场景")]
        [LabelText("自定义启动场景")]
        [SerializeField]
        SceneAssetWrapper bootstrapScene;

        /// <summary>
        /// 当前正在进行的异步加载操作列表
        /// </summary>
        readonly List<AsyncOperation> _loadingOperations = new List<AsyncOperation>();

        /// <summary>
        /// 动态添加的场景路径列表，追踪通过 AddScene 加载的叠加场景
        /// </summary>
        readonly List<string> _addedScenePaths = new List<string>();

        /// <summary>
        /// 预设的启动场景名称数组
        /// </summary>
        readonly string[] _presetBootstrapSceneNames =
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
        /// 动态添加的场景路径（只读）
        /// </summary>
        public IReadOnlyList<string> AddedScenePaths => _addedScenePaths;

        #region *** 可调用的公共方法 ***

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
        /// 加载场景。Single 模式。可传入加载完成回调方法。
        /// </summary>
        public void LoadSceneSingle(string scenePath, Action onCompleted = null)
        {
            StartCoroutine(LoadSceneInternal(scenePath, onCompleted, LoadSceneMode.Single));
        }

        /// <summary>
        /// 加载场景。Single 模式。通过 SceneAssetWrapper 指定场景。可传入加载完成回调方法。
        /// </summary>
        public void LoadSceneSingleWithSceneAssetWrapper(SceneAssetWrapper sceneRef,
            Action onCompleted = null)
        {
            LoadSceneSingle(sceneRef.ScenePath, onCompleted);
        }

        /// <summary>
        /// 加载场景。Additive 模式。可传入加载完成回调方法。
        /// </summary>
        public void LoadSceneAdditive(string scenePath, Action onCompleted = null)
        {
            StartCoroutine(LoadSceneInternal(scenePath, onCompleted, LoadSceneMode.Additive));
        }

        /// <summary>
        /// 加载场景。Additive 模式。通过 SceneAssetWrapper 指定场景。可传入加载完成回调方法。
        /// </summary>
        public void LoadSceneAdditiveWithSceneAssetWrapper(SceneAssetWrapper sceneRef,
            Action onCompleted = null)
        {
            LoadSceneAdditive(sceneRef.ScenePath, onCompleted);
        }

        /// <summary>
        /// 卸载场景。用于 Additive 模式手动卸载部分场景。可传入卸载完成回调方法。
        /// </summary>
        public void UnloadScene(string scenePath, Action onUnloaded = null)
        {
            StartCoroutine(UnloadSceneInternal(scenePath, onUnloaded));
        }

        /// <summary>
        /// 卸载场景。通过 SceneAssetWrapper 指定场景。用于 Additive 模式手动卸载部分场景。可传入卸载完成回调方法。
        /// </summary>
        public void UnloadSceneWithSceneAssetWrapper(SceneAssetWrapper sceneRef, Action onUnloaded = null)
        {
            UnloadScene(sceneRef.ScenePath, onUnloaded);
        }

        /// <summary>
        /// 重新加载当前激活场景。Single 模式。
        /// </summary>
        public void ReloadScene()
        {
            var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex);
        }

        /// <summary>
        /// 动态添加场景。纯叠加模式，不卸载已有场景。可传入加载完成回调方法。
        /// </summary>
        public void AddScene(string scenePath, Action onCompleted = null)
        {
            StartCoroutine(AddSceneInternal(scenePath, onCompleted));
        }

        /// <summary>
        /// 动态添加场景。纯叠加模式，不卸载已有场景。通过 SceneAssetWrapper 指定场景。可传入加载完成回调方法。
        /// </summary>
        public void AddSceneWithSceneAssetWrapper(SceneAssetWrapper sceneRef, Action onCompleted = null)
        {
            AddScene(sceneRef.ScenePath, onCompleted);
        }

        /// <summary>
        /// 卸载动态添加的场景。从追踪列表中移除并卸载。可传入卸载完成回调方法。
        /// </summary>
        public void UnloadAddedScene(string scenePath, Action onUnloaded = null)
        {
            StartCoroutine(UnloadAddedSceneInternal(scenePath, onUnloaded));
        }

        /// <summary>
        /// 卸载动态添加的场景。通过 SceneAssetWrapper 指定场景。可传入卸载完成回调方法。
        /// </summary>
        public void UnloadAddedSceneWithSceneAssetWrapper(SceneAssetWrapper sceneRef,
            Action onUnloaded = null)
        {
            UnloadAddedScene(sceneRef.ScenePath, onUnloaded);
        }

        /// <summary>
        /// 卸载所有动态添加的场景。可传入全部卸载完成回调方法。
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

        IEnumerator LoadSceneInternal(string scenePath, Action onCompleted, LoadSceneMode mode)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag, $"无效场景路径: {scenePath}");
                yield break;
            }

            if (mode == LoadSceneMode.Additive)
            {
                yield return UnloadLastLoadedSceneInternal();
            }
            else
            {
                // Single 模式会卸载所有场景，清空动态添加的场景追踪
                _addedScenePaths.Clear();
            }

            var op = SceneManager.LoadSceneAsync(scenePath, mode);
            if (op == null)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag, $"无法加载场景: {scenePath}");
                yield break;
            }

            _loadingOperations.Add(op);
            yield return op;
            _loadingOperations.Remove(op);
            LastLoadedScene = SceneManager.GetSceneByPath(scenePath);
            SceneManager.SetActiveScene(LastLoadedScene);
            onCompleted?.Invoke();
        }

        IEnumerator UnloadLastLoadedSceneInternal()
        {
            if (!LastLoadedScene.IsValid())
            {
                yield break;
            }

            if (LastLoadedScene != BootstrapScene)
            {
                yield return SceneManager.UnloadSceneAsync(LastLoadedScene);
            }
        }

        IEnumerator UnloadSceneInternal(string scenePath, Action onUnloaded)
        {
            var op = SceneManager.UnloadSceneAsync(scenePath);
            if (op == null)
            {
                AesirModulesDebug.LogWarning(AesirModulesDebug.SceneModuleTag, $"场景卸载失败或场景不存在: {scenePath}");
                yield break;
            }

            yield return op;
            onUnloaded?.Invoke();
        }

        IEnumerator AddSceneInternal(string scenePath, Action onCompleted)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag, $"无效场景路径: {scenePath}");
                yield break;
            }

            var op = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
            if (op == null)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag, $"无法加载场景: {scenePath}");
                yield break;
            }

            _loadingOperations.Add(op);
            yield return op;
            _loadingOperations.Remove(op);
            _addedScenePaths.Add(scenePath);
            onCompleted?.Invoke();
        }

        IEnumerator UnloadAddedSceneInternal(string scenePath, Action onUnloaded)
        {
            var op = SceneManager.UnloadSceneAsync(scenePath);
            if (op == null)
            {
                AesirModulesDebug.LogWarning(AesirModulesDebug.SceneModuleTag, $"场景卸载失败或场景不存在: {scenePath}");
                yield break;
            }

            yield return op;
            _addedScenePaths.Remove(scenePath);
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
            if (bootstrapScene != null)
            {
                BootstrapScene = SceneManager.GetSceneByPath(bootstrapScene.ScenePath);
            }
            else
            {
                for (var i = 0; i < _presetBootstrapSceneNames.Length; i++)
                {
                    var scene = SceneManager.GetSceneByName(_presetBootstrapSceneNames[i]);
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
