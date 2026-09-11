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
    /// <para>
    /// 加载/卸载完成会同步广播 <see cref="SceneLoadedEvent" /> / <see cref="SceneUnloadedEvent" />
    /// （参数为场景路径），供多个系统订阅场景生命周期。
    /// </para>
    /// </summary>
    public class SceneModule : AesirMonoBehaviour
    {
        /// <summary>
        /// <see cref="AsyncOperation.progress" /> 在场景激活前的上限（Unity 已知行为：
        /// 进度停在 0.9、激活瞬间跳 1）。onProgress 回调按此系数归一化，进度条可平滑走到 100%。
        /// </summary>
        const float SceneLoadProgressCap = 0.9f;

        /// <summary>
        /// 预设的启动场景名称（运行时 BootstrapSceneHelper 共用的单一事实来源）。
        /// 仅供编辑器 BootstrapSceneHelper 按名称搜集启动场景使用，本模块运行时不做自动搜索。
        /// </summary>
        public static readonly IReadOnlyList<string> PresetBootstrapSceneNames = new[]
        {
            "Bootstrap", "BootstrapScene", "Bootstrapper", "BootstrapperScene", "bootstrap_scene",
            "bootstrap", "bootstrapper_scene", "bootstrapper"
        };
#if ODIN_INSPECTOR
        [DetailedInfoBox("预放置须知",
            "建议保持 Dont Destroy On Load 开启：Single 加载会卸载所有旧场景，关闭 DDOL 的本模块实例将随场景销毁，进行中的加载回调会随协程一并中断。")]
        [LabelText("自定义启动场景")]
#endif
        [SerializeField]
        SceneAssetWrapper bootstrapScene;

        /// <summary>
        /// 是否将本物体加入 DontDestroyOnLoad 场景。仅在本物体为根物体（场景预放置）时生效；
        /// 运行时自动创建于 <see cref="AesirModules" /> 宿主下时跟随宿主的 DDOL 决策，本字段不参与判断。
        /// </summary>
        [SerializeField]
        bool dontDestroyOnLoad = true;

        /// <summary>
        /// 叠加场景路径列表，追踪所有经本模块 Additive 加载、尚未卸载的场景
        /// </summary>
        readonly List<string> _addedScenePaths = new List<string>();

        /// <summary>
        /// 启动场景引用（编辑器 BootstrapSceneHelper 的工作流之外，供用户代码读取路径/名称自行编排启动流程）。
        /// </summary>
        public SceneAssetWrapper BootstrapSceneAssetWrapper => bootstrapScene;

        /// <summary>
        /// 最后一个已经加载的场景，Scene 结构体
        /// </summary>
        public Scene LastLoadedScene { get; private set; }

        /// <summary>
        /// 叠加场景路径（只读）。含所有经本模块 Additive 加载、尚未卸载的场景。
        /// </summary>
        public IReadOnlyList<string> AddedScenePaths => _addedScenePaths;

        /// <summary>
        /// 场景加载完成事件（Single 与 Additive 均触发；参数为场景路径）。
        /// 在 onCompleted 回调之前广播。
        /// </summary>
        public MiniEvent<string> SceneLoadedEvent { get; } = new MiniEvent<string>();

        /// <summary>
        /// 场景卸载完成事件（参数为场景路径）。在 onUnloaded / onAllUnloaded 回调之前广播。
        /// </summary>
        public MiniEvent<string> SceneUnloadedEvent { get; } = new MiniEvent<string>();

        #region 公共方法

        /// <summary>
        /// 加载场景。Single 模式：卸载全部场景、重设激活场景、加载成功后清空叠加追踪（失败时保留）。
        /// 可传入完成/失败回调与逐帧进度回调（0-1，已按 0.9 激活上限归一化）。
        /// </summary>
        public void LoadSceneSingle(string scenePath,
            Action onCompleted = null,
            Action onFailed = null,
            Action<float> onProgress = null)
        {
            StartCoroutine(LoadSceneInternal(scenePath, onCompleted, onFailed, onProgress,
                LoadSceneMode.Single));
        }

        /// <summary>
        /// 加载场景。Single 模式。通过 <see cref="SceneAssetWrapper" /> 指定场景。
        /// 引用无效（空/不在 BuildSettings）或为 Addressable 场景时走失败回调。
        /// </summary>
        public void LoadSceneSingle(SceneAssetWrapper sceneRef,
            Action onCompleted = null,
            Action onFailed = null,
            Action<float> onProgress = null)
        {
            if (!TryGetLoadablePath(sceneRef, onFailed, out var path))
            {
                return;
            }

            LoadSceneSingle(path, onCompleted, onFailed, onProgress);
        }

        /// <summary>
        /// 加载场景。Additive 模式：纯叠加、不改变激活场景（对齐 Unity 原生语义），并记入叠加追踪。
        /// 可传入完成/失败回调与逐帧进度回调（0-1，已按 0.9 激活上限归一化）。
        /// <para>
        /// 约定：请勿对同一路径重复叠加加载——Unity 会加载两个场景实例，而追踪列表按路径粒度只记录一次，
        /// <see cref="UnloadScene(string, Action, Action)" /> 按路径卸载时只卸载其中一个实例，剩余实例将脱离追踪。
        /// </para>
        /// </summary>
        public void LoadSceneAdditive(string scenePath,
            Action onCompleted = null,
            Action onFailed = null,
            Action<float> onProgress = null)
        {
            StartCoroutine(LoadSceneInternal(scenePath, onCompleted, onFailed, onProgress,
                LoadSceneMode.Additive));
        }

        /// <summary>
        /// 加载场景。Additive 模式。通过 <see cref="SceneAssetWrapper" /> 指定场景。
        /// 引用无效（空/不在 BuildSettings）或为 Addressable 场景时走失败回调。
        /// </summary>
        public void LoadSceneAdditive(SceneAssetWrapper sceneRef,
            Action onCompleted = null,
            Action onFailed = null,
            Action<float> onProgress = null)
        {
            if (!TryGetLoadablePath(sceneRef, onFailed, out var path))
            {
                return;
            }

            LoadSceneAdditive(path, onCompleted, onFailed, onProgress);
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
        /// 把已加载的指定场景设为激活场景（多场景叠加工作流的高频操作，决定光照设置来源与
        /// Instantiate 默认落点）。对齐 Unity 原生 SetActiveScene 语义，返回是否成功；
        /// 场景未加载或引用无效时输出错误并返回 false。
        /// </summary>
        public bool SetActiveScene(string scenePath)
        {
            var scene = SceneManager.GetSceneByPath(scenePath);
            if (!scene.IsValid())
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag, $"场景未加载，无法设为激活场景: {scenePath}");
                return false;
            }

            SceneManager.SetActiveScene(scene);
            return true;
        }

        /// <summary>
        /// 把已加载的指定场景设为激活场景。通过 <see cref="SceneAssetWrapper" /> 指定场景。
        /// </summary>
        public bool SetActiveScene(SceneAssetWrapper sceneRef)
        {
            if (sceneRef == null || !sceneRef.TryGetLoadedScene(out var scene))
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag,
                    $"场景引用无效或场景未加载，无法设为激活场景: {sceneRef}");
                return false;
            }

            SceneManager.SetActiveScene(scene);
            return true;
        }

        /// <summary>
        /// 重新加载当前激活场景。异步 Single 模式，加载成功后清空叠加场景追踪。
        /// 编辑器中激活场景尚未保存（无有效路径）时走失败回调。
        /// </summary>
        public void ReloadScene(Action onCompleted = null, Action onFailed = null)
        {
            var path = SceneManager.GetActiveScene().path;
            if (string.IsNullOrEmpty(path))
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag, "当前激活场景未保存（无有效路径），无法重载。");
                onFailed?.Invoke();
                return;
            }

            LoadSceneSingle(path, onCompleted, onFailed);
        }

        /// <summary>
        /// 卸载所有经本模块叠加加载的场景。单个场景卸载失败（场景已被外部卸载）时跳过并告警，不影响其余场景。
        /// 可传入全部卸载完成回调。
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

        /// <summary>
        /// 兼容 Enter Play Mode（跳过域重载）的静态状态重置。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _instance = null;
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                // 对齐 RAA 先例：只销毁本组件，不连带销毁用户同物体上的其他组件
                Destroy(this);
                return;
            }

            _instance = this;

            // 非根物体（运行时自动创建于 [Aesir Modules] 宿主下）时 DDOL 跟随宿主，本字段不参与判断
            if (!dontDestroyOnLoad)
            {
                AesirModulesDebug.LogWarning(AesirModulesDebug.SceneModuleTag,
                    "dontDestroyOnLoad 已关闭：实例保留在所在场景、随场景卸载销毁，LoadSceneSingle 将销毁本模块并中断加载回调");
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

        IEnumerator LoadSceneInternal(string scenePath,
            Action onCompleted,
            Action onFailed,
            Action<float> onProgress,
            LoadSceneMode mode)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag, $"无效场景路径: {scenePath}");
                onFailed?.Invoke();
                yield break;
            }

            var op = SceneManager.LoadSceneAsync(scenePath, mode);
            if (op == null)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag, $"无法加载场景: {scenePath}");
                onFailed?.Invoke();
                yield break;
            }

            // 逐帧轮询而非 yield return op：onProgress 需要每帧报告归一化进度
            while (!op.isDone)
            {
                onProgress?.Invoke(Mathf.Min(op.progress / SceneLoadProgressCap, 1f));
                yield return null;
            }

            onProgress?.Invoke(1f);

            LastLoadedScene = SceneManager.GetSceneByPath(scenePath);
            if (mode == LoadSceneMode.Single)
            {
                // Single 加载已卸载全部旧场景，叠加追踪随之失效——加载成功后才清空（失败时保留旧追踪）
                _addedScenePaths.Clear();
                SceneManager.SetActiveScene(LastLoadedScene);
            }
            else if (!_addedScenePaths.Contains(scenePath))
            {
                // 重复叠加同一路径时按路径粒度只追踪一次
                _addedScenePaths.Add(scenePath);
            }

            SceneLoadedEvent.Invoke(scenePath);
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
                AesirModulesDebug.LogWarning(AesirModulesDebug.SceneModuleTag, $"场景卸载失败或场景不存在: {scenePath}");
                onFailed?.Invoke();
                yield break;
            }

            yield return op;
            _addedScenePaths.RemoveAll(p => p == scenePath);
            SceneUnloadedEvent.Invoke(scenePath);
            onUnloaded?.Invoke();
        }

        IEnumerator UnloadAllAddedScenesInternal(Action onAllUnloaded)
        {
            for (var i = 0; i < _addedScenePaths.Count; i++)
            {
                var scenePath = _addedScenePaths[i];
                var op = SceneManager.UnloadSceneAsync(scenePath);
                if (op == null)
                {
                    // 单个场景已被外部卸载（或不存在）时跳过，不影响其余场景
                    AesirModulesDebug.LogWarning(AesirModulesDebug.SceneModuleTag,
                        $"场景卸载失败或场景不存在，已跳过: {scenePath}");
                    continue;
                }

                yield return op;
                SceneUnloadedEvent.Invoke(scenePath);
            }

            _addedScenePaths.Clear();
            onAllUnloaded?.Invoke();
        }

        #endregion
    }
}
