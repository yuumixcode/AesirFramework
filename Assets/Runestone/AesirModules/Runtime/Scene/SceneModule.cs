using System;
using System.Collections;
using System.Collections.Generic;
using Runestone.AesirArchitecture;
using UnityEngine;
using UnityEngine.SceneManagement;
#if AESIR_MODULES_UNITASK
using System.Threading;
using Cysharp.Threading.Tasks;
#endif
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Runestone.AesirModules
{
    /// <summary>
    /// 场景加载与叠加管理模块（MonoBehaviour 单例）—— 公开 API 全部为静态成员，经 <see cref="Instance" /> 单例转发。
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
    /// <para>
    /// 异步驱动：游戏工程包含 UniTask 时（宏 <c>AESIR_MODULES_UNITASK</c> 由编辑器自动维护），
    /// 内部加载/卸载流程改由 UniTask 驱动，<c>SceneModuleUniTask</c> 适配程序集额外提供可 await 的
    /// UniTask 返回 API；未包含 UniTask 时回退为协程驱动，公开 API 与回调语义完全一致。
    /// </para>
    /// </summary>
    public class SceneModule : AesirMonoBehaviour
    {
        /// <summary>
        /// 是否将本物体加入 DontDestroyOnLoad 场景。仅在本物体为根物体（场景预放置）时生效；
        /// 运行时自动创建于 <see cref="AesirModules" /> 宿主下时跟随宿主的 DDOL 决策，本字段不参与判断。
        /// </summary>
        [SerializeField]
        bool dontDestroyOnLoad = true;

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
            "建议保持 Dont Destroy On Load 开启：Single 加载会卸载所有旧场景，关闭 DDOL 的本模块实例将随场景销毁，进行中的加载回调会随流程一并中断。")]
        [LabelText("自定义启动场景")]
#endif
        [SerializeField]
        SceneAssetWrapper bootstrapScene;

        /// <summary>
        /// 最后一个已经加载的场景。MonoBehaviour 运行状态一律使用显式非序列化字段（自动属性 backing field
        /// 会被场景序列化残留，跨 Play 污染状态）。
        /// </summary>
        Scene _lastLoadedScene;

        /// <summary>
        /// 场景加载完成事件（Single 与 Additive 均触发；参数为场景路径）。在 onCompleted 回调之前广播。
        /// <para>
        /// 静态共享：无论模块实例被重建多少次（宿主被卸载后 <see cref="Instance" /> 懒创建新实例），
        /// 始终是同一个事件对象——DDOL 常驻系统持有的监听句柄不会因实例更替而静默失效。
        /// </para>
        /// </summary>
        static readonly MiniEvent<string> _sceneLoadedEvent = new MiniEvent<string>();

        /// <summary>
        /// 场景卸载完成事件（参数为场景路径）。在 onUnloaded / onAllUnloaded 回调之前广播。
        /// 静态共享的理由同 <see cref="_sceneLoadedEvent" />。
        /// </summary>
        static readonly MiniEvent<string> _sceneUnloadedEvent = new MiniEvent<string>();

        /// <summary>
        /// 叠加场景路径列表，追踪所有经本模块 Additive 加载、尚未卸载的场景
        /// </summary>
        readonly List<string> _addedScenePaths = new List<string>();

        /// <summary>
        /// UnloadAll 迭代快照缓冲区（复用，Clear 保留容量）。
        /// 广播期间监听者嵌套加载/卸载只改 <see cref="_addedScenePaths" />，不干扰本趟快照迭代；
        /// 每个场景卸载成功即同步移出追踪，批量卸载期间 <see cref="AddedScenePaths" /> 始终反映真实状态。
        /// </summary>
        readonly List<string> _unloadSnapshotBuffer = new List<string>();

        /// <summary>
        /// 批量卸载重入深度。<see cref="SceneUnloadedEvent" /> 的监听者回调内再调 UnloadAllAddedScenes 时，
        /// 内层改用局部快照迭代——复用缓冲是单实例资源，两层流程共用同一 List 会让内层 finally Clear
        /// 清空外层正在迭代的列表（外层提前退出、剩余场景漏卸而 onAllUnloaded 仍误报完成）。
        /// </summary>
        int _unloadDepth;

        #region 公开 API — 场景加载与卸载

        /// <summary>
        /// 加载场景（静态门面）。Single 模式：卸载全部场景、重设激活场景、加载成功后清空叠加追踪（失败时保留）。
        /// 可传入完成/失败回调与逐帧进度回调（0-1，已按激活上限归一化——上限配置于 <see cref="SceneModuleConfigSO" />，默认 0.9）。
        /// </summary>
        public static void LoadSceneSingle(string scenePath,
            Action onCompleted = null,
            Action onFailed = null,
            Action<float> onProgress = null)
        {
            Instance.LoadScene(scenePath, onCompleted, onFailed, onProgress, LoadSceneMode.Single);
        }

        /// <summary>
        /// 加载场景（静态门面）。Single 模式。通过 <see cref="SceneAssetWrapper" /> 指定场景。
        /// 引用无效（空/不在 BuildSettings）或为 Addressable 场景时走失败回调。
        /// </summary>
        public static void LoadSceneSingle(SceneAssetWrapper sceneRef,
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
        /// 加载场景（静态门面）。Additive 模式：纯叠加、不改变激活场景（对齐 Unity 原生语义），并记入叠加追踪。
        /// 可传入完成/失败回调与逐帧进度回调（0-1，已按激活上限归一化——上限配置于 <see cref="SceneModuleConfigSO" />，默认 0.9）。
        /// <para>
        /// 约定：请勿对同一路径重复叠加加载——Unity 会加载两个场景实例，而追踪列表按路径粒度只记录一次，
        /// <see cref="UnloadScene(string, Action, Action)" /> 按路径卸载时只卸载其中一个实例，剩余实例将脱离追踪。
        /// </para>
        /// </summary>
        public static void LoadSceneAdditive(string scenePath,
            Action onCompleted = null,
            Action onFailed = null,
            Action<float> onProgress = null)
        {
            Instance.LoadScene(scenePath, onCompleted, onFailed, onProgress, LoadSceneMode.Additive);
        }

        /// <summary>
        /// 加载场景（静态门面）。Additive 模式。通过 <see cref="SceneAssetWrapper" /> 指定场景。
        /// 引用无效（空/不在 BuildSettings）或为 Addressable 场景时走失败回调。
        /// </summary>
        public static void LoadSceneAdditive(SceneAssetWrapper sceneRef,
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
        /// 卸载场景（静态门面）。若该场景在叠加追踪列表中则自动移出。可传入卸载完成/失败回调。
        /// </summary>
        public static void UnloadScene(string scenePath, Action onUnloaded = null, Action onFailed = null)
        {
            Instance.UnloadSceneCore(scenePath, onUnloaded, onFailed);
        }

        /// <summary>
        /// 卸载场景（静态门面）。通过 <see cref="SceneAssetWrapper" /> 指定场景。
        /// </summary>
        public static void UnloadScene(SceneAssetWrapper sceneRef, Action onUnloaded = null, Action onFailed = null)
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
        /// 把已加载的指定场景设为激活场景（静态门面，纯静态操作，不会创建模块实例）。
        /// 多场景叠加工作流的高频操作，决定光照设置来源与 Instantiate 默认落点。
        /// 对齐 Unity 原生 SetActiveScene 语义，返回是否成功；场景未加载或引用无效时输出错误并返回 false。
        /// </summary>
        public static bool SetActiveScene(string scenePath)
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
        /// 把已加载的指定场景设为激活场景（静态门面）。通过 <see cref="SceneAssetWrapper" /> 指定场景。
        /// </summary>
        public static bool SetActiveScene(SceneAssetWrapper sceneRef)
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
        /// 重新加载当前激活场景（静态门面）。异步 Single 模式，加载成功后清空叠加场景追踪。
        /// 编辑器中激活场景尚未保存（无有效路径）时走失败回调。
        /// </summary>
        public static void ReloadScene(Action onCompleted = null, Action onFailed = null)
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
        /// 卸载所有经本模块叠加加载的场景（静态门面）。单个场景卸载失败（场景已被外部卸载）时跳过并告警，不影响其余场景。
        /// 可传入全部卸载完成回调。
        /// </summary>
        public static void UnloadAllAddedScenes(Action onAllUnloaded = null)
        {
            Instance.UnloadAllAddedScenesCore(onAllUnloaded);
        }

        #endregion

        #region 公开 API — 状态与事件

        /// <summary>
        /// 最后一个已经加载的场景，Scene 结构体（静态门面，经单例转发）。
        /// </summary>
        public static Scene LastLoadedScene => Instance._lastLoadedScene;

        /// <summary>
        /// 叠加场景路径（只读快照，静态门面）。含所有经本模块 Additive 加载、尚未卸载的场景。
        /// <para>
        /// 返回 <see cref="_addedScenePaths" /> 的副本：调用方既无法回转 <c>List&lt;string&gt;</c> 修改模块状态，
        /// 也不会在监听者于广播回调里触发 CompleteLoad / CompleteUnload 时被抛
        /// <see cref="InvalidOperationException" />。本属性不在逐帧路径上（仅状态查询与测试断言使用），无需缓存。
        /// </para>
        /// </summary>
        public static IReadOnlyList<string> AddedScenePaths => Instance._addedScenePaths.ToArray();

        /// <summary>
        /// 场景加载完成事件（静态门面）。Single 与 Additive 均触发；参数为场景路径。
        /// 在 onCompleted 回调之前广播。事件对象为静态共享，访问不创建模块实例。
        /// </summary>
        public static MiniEvent<string> SceneLoadedEvent => _sceneLoadedEvent;

        /// <summary>
        /// 场景卸载完成事件（静态门面）。参数为场景路径。在 onUnloaded / onAllUnloaded 回调之前广播。
        /// 事件对象为静态共享，访问不创建模块实例。
        /// </summary>
        public static MiniEvent<string> SceneUnloadedEvent => _sceneUnloadedEvent;

        /// <summary>
        /// 启动场景引用（静态门面，经单例转发）。编辑器 BootstrapSceneHelper 的工作流之外，
        /// 供用户代码读取路径/名称自行编排启动流程。预放置实例的序列化字段非 null 时优先返回；
        /// 未赋值时回退 <see cref="SceneModuleConfigSO" /> 的全局启动场景（无需预放置即可在 Project 窗口配置），
        /// 两者均未配置时返回 null。
        /// </summary>
        public static SceneAssetWrapper BootstrapSceneAssetWrapper
        {
            get
            {
                var instanceScene = Instance.bootstrapScene;
                return instanceScene != null ? instanceScene : SceneModuleConfigSO.Instance.bootstrapScene;
            }
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
                // 含未激活对象：未激活的预放置实例不被 Awake 赋值，Exclude 会让它被判为不存在而重复创建（Inspector 配置随之失效）
                _instance = FindAnyObjectByType<SceneModule>(FindObjectsInactive.Include);
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
        /// <remarks>
        /// 共享事件只清空监听者、不重建对象——保持「同一次 Play 内事件对象恒定」不变，
        /// 同时避免上一轮 Play 的监听者泄漏到本轮（对齐重建实例时的清零效果）。
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _instance = null;
            _sceneLoadedEvent.Dispose();
            _sceneUnloadedEvent.Dispose();
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
            if (dontDestroyOnLoad && transform.root == transform)
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

        #region 内部实现

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

        /// <summary>
        /// 校验加载入参并取得异步操作，加载流程的公共入口。
        /// 游戏工程包含 UniTask 时（<c>AESIR_MODULES_UNITASK</c>）由 UniTask 驱动逐帧等待与完成回调，
        /// 否则回退为协程驱动——两者共享校验与完成记账，公开 API 与回调语义完全一致。
        /// </summary>
        void LoadScene(string scenePath,
            Action onCompleted,
            Action onFailed,
            Action<float> onProgress,
            LoadSceneMode mode)
        {
#if AESIR_MODULES_UNITASK
            LoadSceneAsyncInternal(scenePath, onCompleted, onFailed, onProgress, mode).Forget();
#else
            StartCoroutine(LoadSceneInternal(scenePath, onCompleted, onFailed, onProgress, mode));
#endif
        }

        /// <summary>
        /// 卸载场景流程的公共入口（异步驱动的选择同 <see cref="LoadScene" />）。
        /// </summary>
        void UnloadSceneCore(string scenePath, Action onUnloaded, Action onFailed)
        {
#if AESIR_MODULES_UNITASK
            UnloadSceneAsyncInternal(scenePath, onUnloaded, onFailed).Forget();
#else
            StartCoroutine(UnloadSceneInternal(scenePath, onUnloaded, onFailed));
#endif
        }

        /// <summary>
        /// 批量卸载叠加场景流程的公共入口（异步驱动的选择同 <see cref="LoadScene" />）。
        /// </summary>
        void UnloadAllAddedScenesCore(Action onAllUnloaded)
        {
#if AESIR_MODULES_UNITASK
            UnloadAllAddedScenesAsyncInternal(onAllUnloaded).Forget();
#else
            StartCoroutine(UnloadAllAddedScenesInternal(onAllUnloaded));
#endif
        }

        /// <summary>
        /// 加载进度归一化上限：取自模块配置 <see cref="SceneModuleConfigSO" />（默认 0.9），
        /// 每次加载时读取（协程与 UniTask 两条驱动路径共用）；钳制到 (0, 1] 防止误配置造成除零或反向进度。
        /// </summary>
        static float ProgressCap => Mathf.Clamp(SceneModuleConfigSO.Instance.progressCap, 0.01f, 1f);

        /// <summary>
        /// 校验加载入参并取得 <see cref="AsyncOperation" />（协程与 UniTask 两条驱动路径共用）。
        /// </summary>
        static bool BeginLoad(string scenePath, LoadSceneMode mode, Action onFailed, out AsyncOperation operation)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag, $"无效场景路径: {scenePath}");
                onFailed?.Invoke();
                operation = null;
                return false;
            }

            operation = SceneManager.LoadSceneAsync(scenePath, mode);
            if (operation == null)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag, $"无法加载场景: {scenePath}");
                onFailed?.Invoke();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 校验卸载入参并取得 <see cref="AsyncOperation" />（协程与 UniTask 两条驱动路径共用）。
        /// </summary>
        static bool BeginUnload(string scenePath, Action onFailed, out AsyncOperation operation)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag, $"无效场景路径: {scenePath}");
                onFailed?.Invoke();
                operation = null;
                return false;
            }

            // 仅剩一个"真实"场景时（Single 加载后叠加追踪被清空）卸载会由引擎直接报原始错误，
            // 此处提前拦截并给出模块自身可操作的失败反馈。
            // 注意不能直接用 SceneManager.sceneCount：它把 DontDestroyOnLoad 伪场景计入，
            // 而本框架默认 DDOL 常驻（AesirModules / EventModule / AudioModule 均在 DDOL），
            // 真实场景只剩一个时 sceneCount 通常为 2，直接比较会漏判。
            if (CountLoadedRealScenes() <= 1)
            {
                AesirModulesDebug.LogWarning(AesirModulesDebug.SceneModuleTag,
                    $"当前仅剩一个已加载场景，无法卸载: {scenePath}（请改用 Single 模式加载目标场景完成切换）");
                onFailed?.Invoke();
                operation = null;
                return false;
            }

            operation = SceneManager.UnloadSceneAsync(scenePath);
            if (operation == null)
            {
                AesirModulesDebug.LogWarning(AesirModulesDebug.SceneModuleTag, $"场景卸载失败或场景不存在: {scenePath}");
                onFailed?.Invoke();
                return false;
            }

            return true;
        }

        /// <summary>Unity 的 DontDestroyOnLoad 伪场景名——它不承载关卡内容，不计入"可卸载的真实场景"。</summary>
        const string DontDestroyOnLoadSceneName = "DontDestroyOnLoad";

        /// <summary>
        /// 统计已加载的真实场景数量（排除 <see cref="DontDestroyOnLoadSceneName" /> 伪场景）。
        /// </summary>
        /// <remarks>
        /// 直接用 <c>SceneManager.sceneCount</c> 会因 DDOL 常驻而虚高（见调用点的说明），
        /// 故按场景逐个判定。
        /// </remarks>
        /// <returns>已加载且非 DDOL 伪场景的场景数量</returns>
        static int CountLoadedRealScenes()
        {
            var count = 0;
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && scene.name != DontDestroyOnLoadSceneName)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 加载完成后的统一记账：进度收尾到 1.0、LastLoadedScene 更新、Single 清空叠加追踪并重设激活场景
        /// / Additive 登记追踪、广播 SceneLoadedEvent、触发 onCompleted。
        /// </summary>
        void CompleteLoad(string scenePath, LoadSceneMode mode, Action<float> onProgress, Action onCompleted)
        {
            onProgress?.Invoke(1f);

            _lastLoadedScene = SceneManager.GetSceneByPath(scenePath);
            if (mode == LoadSceneMode.Single)
            {
                // Single 加载已卸载全部旧场景，叠加追踪随之失效——加载成功后才清空（失败时保留旧追踪）
                _addedScenePaths.Clear();
                if (_lastLoadedScene.IsValid())
                {
                    SceneManager.SetActiveScene(_lastLoadedScene);
                }
                else
                {
                    // 路径大小写/归一化差异时 GetSceneByPath 可能取回无效 Scene——记录错误而非抛异常
                    AesirModulesDebug.LogError(AesirModulesDebug.SceneModuleTag,
                        $"场景加载完成但未能按路径取回有效场景，跳过激活: {scenePath}");
                }
            }
            else if (!_addedScenePaths.Contains(scenePath))
            {
                // 重复叠加同一路径时按路径粒度只追踪一次
                _addedScenePaths.Add(scenePath);
            }

            _sceneLoadedEvent.Invoke(scenePath);
            onCompleted?.Invoke();
        }

        /// <summary>
        /// 卸载完成后的统一记账：移出叠加追踪、广播 SceneUnloadedEvent、触发 onUnloaded。
        /// </summary>
        void CompleteUnload(string scenePath, Action onUnloaded)
        {
            _addedScenePaths.RemoveAll(p => p == scenePath);
            _sceneUnloadedEvent.Invoke(scenePath);
            onUnloaded?.Invoke();
        }

        /// <summary>
        /// 取批量卸载的迭代快照：重入（嵌套调用）时改用局部快照，顶层复用共享缓冲（Clear 保留容量）。
        /// </summary>
        List<string> AcquireUnloadSnapshot()
        {
            return _unloadDepth > 0 ? new List<string>() : _unloadSnapshotBuffer;
        }

        #region 协程驱动（默认实现）

        IEnumerator LoadSceneInternal(string scenePath,
            Action onCompleted,
            Action onFailed,
            Action<float> onProgress,
            LoadSceneMode mode)
        {
            if (!BeginLoad(scenePath, mode, onFailed, out var operation))
            {
                yield break;
            }

            // 逐帧轮询而非 yield return op：onProgress 需要每帧报告归一化进度
            while (!operation.isDone)
            {
                onProgress?.Invoke(Mathf.Min(operation.progress / ProgressCap, 1f));
                yield return null;
            }

            CompleteLoad(scenePath, mode, onProgress, onCompleted);
        }

        IEnumerator UnloadSceneInternal(string scenePath, Action onUnloaded, Action onFailed)
        {
            if (!BeginUnload(scenePath, onFailed, out var operation))
            {
                yield break;
            }

            yield return operation;
            CompleteUnload(scenePath, onUnloaded);
        }

        IEnumerator UnloadAllAddedScenesInternal(Action onAllUnloaded)
        {
            var snapshot = AcquireUnloadSnapshot();
            _unloadDepth++;
            try
            {
                // 遍历快照：广播期间监听者可能嵌套加载/卸载（修改 _addedScenePaths），
                // 基于快照迭代不被干扰；广播期间新叠加的场景不在本趟卸载范围内
                snapshot.AddRange(_addedScenePaths);
                for (var i = 0; i < snapshot.Count; i++)
                {
                    var scenePath = snapshot[i];
                    var operation = SceneManager.UnloadSceneAsync(scenePath);
                    if (operation == null)
                    {
                        // 单个场景已被外部卸载（或不存在）时跳过，不影响其余场景；
                        // 不存在的场景同步移出追踪（追踪残留属陈旧状态）
                        AesirModulesDebug.LogWarning(AesirModulesDebug.SceneModuleTag,
                            $"场景卸载失败或场景不存在，已跳过: {scenePath}");
                        _addedScenePaths.RemoveAll(p => p == scenePath);
                        continue;
                    }

                    yield return operation;
                    // 每卸一个即移出追踪：批量卸载期间 AddedScenePaths 始终反映真实状态
                    CompleteUnload(scenePath, null);
                }
            }
            finally
            {
                snapshot.Clear();
                _unloadDepth--;
            }

            onAllUnloaded?.Invoke();
        }

        #endregion

#if AESIR_MODULES_UNITASK
        #region UniTask 驱动（工程包含 UniTask 时替代协程实现）

        /// <summary>
        /// 宿主销毁的取消令牌：对齐协程随宿主销毁而终止的语义——宿主被销毁（如关闭 DDOL 后的 Single 加载）
        /// 时流程静默中止，进行中的回调不再触发。
        /// </summary>
        CancellationToken HostDestroyToken => destroyCancellationToken;

        async UniTaskVoid LoadSceneAsyncInternal(string scenePath,
            Action onCompleted,
            Action onFailed,
            Action<float> onProgress,
            LoadSceneMode mode)
        {
            try
            {
                if (!BeginLoad(scenePath, mode, onFailed, out var operation))
                {
                    return;
                }

                // 逐帧轮询：onProgress 需要每帧报告归一化进度（与协程实现逐帧等价）
                while (!operation.isDone)
                {
                    onProgress?.Invoke(Mathf.Min(operation.progress / ProgressCap, 1f));
                    await UniTask.NextFrame(HostDestroyToken);
                }

                CompleteLoad(scenePath, mode, onProgress, onCompleted);
            }
            catch (OperationCanceledException)
            {
                // 宿主销毁：静默中止（回调链随宿主消亡，对齐协程被停止的语义）
            }
        }

        async UniTaskVoid UnloadSceneAsyncInternal(string scenePath, Action onUnloaded, Action onFailed)
        {
            try
            {
                if (!BeginUnload(scenePath, onFailed, out var operation))
                {
                    return;
                }

                await operation.ToUniTask(cancellationToken: HostDestroyToken);
                CompleteUnload(scenePath, onUnloaded);
            }
            catch (OperationCanceledException)
            {
                // 宿主销毁：静默中止（对齐协程被停止的语义）
            }
        }

        async UniTaskVoid UnloadAllAddedScenesAsyncInternal(Action onAllUnloaded)
        {
            try
            {
                var snapshot = AcquireUnloadSnapshot();
                _unloadDepth++;
                try
                {
                    // 遍历快照：广播期间监听者可能嵌套加载/卸载（修改 _addedScenePaths），
                    // 基于快照迭代不被干扰；广播期间新叠加的场景不在本趟卸载范围内
                    snapshot.AddRange(_addedScenePaths);
                    for (var i = 0; i < snapshot.Count; i++)
                    {
                        var scenePath = snapshot[i];
                        var operation = SceneManager.UnloadSceneAsync(scenePath);
                        if (operation == null)
                        {
                            // 单个场景已被外部卸载（或不存在）时跳过，不影响其余场景；
                            // 不存在的场景同步移出追踪（追踪残留属陈旧状态）
                            AesirModulesDebug.LogWarning(AesirModulesDebug.SceneModuleTag,
                                $"场景卸载失败或场景不存在，已跳过: {scenePath}");
                            _addedScenePaths.RemoveAll(p => p == scenePath);
                            continue;
                        }

                        await operation.ToUniTask(cancellationToken: HostDestroyToken);
                        // 每卸一个即移出追踪：批量卸载期间 AddedScenePaths 始终反映真实状态
                        CompleteUnload(scenePath, null);
                    }
                }
                finally
                {
                    snapshot.Clear();
                    _unloadDepth--;
                }

                onAllUnloaded?.Invoke();
            }
            catch (OperationCanceledException)
            {
                // 宿主销毁：静默中止（finally 已保证快照清理与重入深度配平）
            }
        }

        #endregion
#endif

        #endregion
    }
}
