#if UNITY_EDITOR // PlayMode 测试仅在编辑器内运行：需要 UnityEditor 在编辑模式预构建阶段登记/摘除 BuildSettings
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Runestone.AesirModules.Tests.Runtime
{
    /// <summary>
    /// <see cref="SceneModule" /> 真实加载/卸载成功路径的 PlayMode 测试（全仓锐评 02-优化方案 B3-4）。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     EditMode 下 <c>SceneManager.LoadSceneAsync</c> / <c>UnloadSceneAsync</c> 不可用（前者空转、后者直接抛异常），
    ///     行为层成功路径只能经真实加载验证。测试场景为测试程序集同级 <c>TestScenes/</c> 下的两个最小 .unity 资产。
    ///     </para>
    ///     <para>
    ///     BuildSettings 登记经 <see cref="IPrebuildSetup" /> / <see cref="IPostBuildCleanup" />：预构建阶段在
    ///     <b>编辑模式</b>登记（enabled 条目），退出 Play 后立即摘除——条目只存在于本次运行期间，
    ///     不进玩家构建、不污染宿主工程配置。不能在 PlayMode 内登记：<c>LoadSceneAsync</c> 校验的是进入 Play 时
    ///     固化的构建场景列表，PlayMode 内写 <c>EditorBuildSettings.scenes</c> 不被运行中的场景管理器采纳；
    ///     且本引擎对 disabled 条目同样拒绝运行时加载。
    ///     </para>
    ///     <para>
    ///     测试场景按文件名经 AssetDatabase 定位，不写死 Assets 相对路径——测试随包进入任意工程时安装形态不同
    ///     （Assets 安装为 <c>Assets/Runestone/…</c>，嵌入式包与 UPM Git 安装为 <c>Packages/…</c>）。
    ///     </para>
    /// </remarks>
    /// <seealso cref="SceneModule" />
    public class SceneModulePlayModeTests : IPrebuildSetup, IPostBuildCleanup
    {
        const string SceneAName = "SceneModulePlayTestA";
        const string SceneBName = "SceneModulePlayTestB";

        static string _sceneAPath;
        static string _sceneBPath;

        Scene _originalActiveScene;

        static string SceneAPath => _sceneAPath ??= ResolveTestScenePath(SceneAName);

        static string SceneBPath => _sceneBPath ??= ResolveTestScenePath(SceneBName);

        #region BuildSettings 登记（编辑模式预构建 / 清理阶段）

        /// <summary>
        /// 进入 Play 前的编辑模式阶段：把测试场景登记为 BuildSettings enabled 条目（追加到列表末尾）。
        /// </summary>
        /// <remarks>
        /// 登记前先按文件名剔除既有同名条目——上次运行被强杀遗留的条目、包被移动后指向旧路径的条目都在此回收，
        /// 保证登记后每个测试场景恰好一个 enabled 条目、且指向本次解析出的真实路径。
        /// </remarks>
        void IPrebuildSetup.Setup()
        {
            var testPaths = new[] { SceneAPath, SceneBPath };
            EditorBuildSettings.scenes = EditorBuildSettings.scenes
                .Where(scene => !IsTestScenePath(scene.path))
                .Concat(testPaths.Select(path => new EditorBuildSettingsScene(path, true)))
                .ToArray();
        }

        /// <summary>
        /// 退出 Play 后的编辑模式阶段：摘除测试场景条目，还原宿主工程 BuildSettings。
        /// </summary>
        /// <remarks>
        /// 登记时统一追加到列表末尾，摘除后列表与登记前逐位一致。测试场景路径是测试私有资产，宿主工程不可能
        /// 合法登记它们，"按名剔除"即等价于完整还原，且无需依赖跨域重载存活的状态（预构建与清理分处两个域）。
        /// </remarks>
        void IPostBuildCleanup.Cleanup() => UnregisterTestScenes();

        /// <summary>
        /// 域加载兜底清扫：测试运行被强杀（编辑器崩溃、进程被 kill）会留下已登记条目，
        /// 编辑模式域加载即清扫，保证测试场景永远不会进入玩家构建。
        /// 进入 / 处于 PlayMode 时跳过——预构建阶段刚登记的条目正被本次运行使用。
        /// </summary>
        [InitializeOnLoadMethod]
        static void SweepStaleTestScenesOnDomainLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            UnregisterTestScenes();
        }

        /// <summary>按名剔除测试场景条目（幂等：无残留时不写盘）。</summary>
        static void UnregisterTestScenes()
        {
            var scenes = EditorBuildSettings.scenes;
            var kept = scenes.Where(scene => !IsTestScenePath(scene.path)).ToArray();
            if (kept.Length != scenes.Length)
            {
                EditorBuildSettings.scenes = kept;
            }
        }

        static bool IsTestScenePath(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            return name == SceneAName || name == SceneBName;
        }

        /// <summary>
        /// 按文件名在 AssetDatabase 中定位测试场景资产（随包安装形态自适应）。
        /// </summary>
        static string ResolveTestScenePath(string sceneName)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Scene " + sceneName))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(Path.GetFileNameWithoutExtension(path), sceneName, StringComparison.Ordinal))
                {
                    return path;
                }
            }

            Assert.Fail($"未找到测试场景资产 {sceneName}.unity——应随测试程序集位于 TestScenes/ 目录");
            return null;
        }

        #endregion

        /// <summary>
        /// 捕获原始激活场景，并校验预构建阶段已完成登记（未登记时给出明确诊断，而非留待加载静默失败）。
        /// </summary>
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _originalActiveScene = SceneManager.GetActiveScene();

            Assert.IsTrue(EditorBuildSettings.scenes.Any(scene => scene.path == SceneAPath && scene.enabled),
                $"前置：预构建阶段（IPrebuildSetup）应已把 {SceneAPath} 登记进 EditorBuildSettings");

            yield return null;
        }

        /// <summary>
        /// 还原激活场景；卸载本套件遗留的测试场景（BuildSettings 由 <see cref="IPostBuildCleanup" /> 负责）。
        /// Single 用例会卸载测试运行器场景（其本为一次性 InitTestScene），此后 TestSceneA 可能是唯一已加载场景——
        /// Unity 不允许卸载最后一个已加载场景，此时保留 TestSceneA 作为后续用例的环境场景。
        /// </summary>
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // SceneB 只会在叠加用例中被加载，此时必有其他场景共存，可安全卸载
            if (SceneManager.GetSceneByPath(SceneBPath).isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(SceneBPath);
            }

            // SceneA 仅在仍有多场景共存时卸载；作为唯一场景时保留（避免“卸载最后一个场景”错误日志污染后续用例）
            if (SceneManager.GetSceneByPath(SceneAPath).isLoaded && SceneManager.sceneCount > 1)
            {
                yield return SceneManager.UnloadSceneAsync(SceneAPath);
            }

            if (_originalActiveScene.IsValid() && _originalActiveScene.isLoaded)
            {
                SceneManager.SetActiveScene(_originalActiveScene);
            }

            yield return null;
        }

        /// <summary>
        /// Single 成功路径：进度归一化到 1.0 → SceneLoadedEvent → onCompleted 的顺序；激活场景切换；
        /// 叠加追踪清空；LastLoadedScene 更新；模块 DDOL 存活（Single 卸载全部旧场景不杀模块）。
        /// </summary>
        [Order(4)]
        [UnityTest]
        public IEnumerator LoadSceneSingle_SuccessPath_CallbacksInOrder_ActiveSceneSwitched_ModuleSurvives()
        {
            var module = SceneModule.Instance;
            Assert.IsFalse(module == null, "前置：SceneModule.Instance 应可用");

            var order = new List<string>();
            var lastProgress = 0f;
            var progressReachedOne = false;
            var progressReachedOneBeforeEvent = false;
            var completed = false;
            var failed = false;

            var handle = module.SceneLoadedEvent.AddListener(path =>
            {
                Assert.AreEqual(SceneAPath, path, "SceneLoadedEvent 参数应为场景路径");
                // 事件广播时进度应已归一化到 1.0（0.9 激活上限归一化，onProgress(1f) 先于广播）
                progressReachedOneBeforeEvent = progressReachedOne;
                order.Add("event");
            });

            try
            {
                module.LoadSceneSingle(SceneAPath, () =>
                {
                    completed = true;
                    order.Add("completed");
                }, () => failed = true, p =>
                {
                    lastProgress = Mathf.Max(lastProgress, p);
                    if (p >= 1f - 1e-3f)
                    {
                        progressReachedOne = true;
                    }
                });

                yield return WaitUntil(() => completed || failed, "Single 加载应在超时前回调");
            }
            finally
            {
                handle.Dispose();
            }

            Assert.IsFalse(failed, "成功路径不应触发失败回调");
            Assert.AreEqual(new[] { "event", "completed" }, order, "SceneLoadedEvent 应先于 onCompleted 广播");
            Assert.GreaterOrEqual(lastProgress, 1f - 1e-3f, "最终进度应归一化到 1.0（0.9 激活上限归一化）");
            Assert.IsTrue(progressReachedOneBeforeEvent, "事件广播时进度应已报告到 1.0（onProgress(1f) 先于广播）");

            Assert.AreEqual(SceneAPath, module.LastLoadedScene.path, "LastLoadedScene 应为刚加载的场景");
            Assert.AreEqual(SceneAPath, SceneManager.GetActiveScene().path, "Single 加载后激活场景应为新场景");
            Assert.AreEqual(0, module.AddedScenePaths.Count, "Single 加载后叠加追踪应清空");

            // DDOL 存活：模块经 [Aesir Modules] 宿主 DDOL，Single 卸载全部旧场景不杀模块——回调链因此得以完整
            Assert.AreSame(module, SceneModule.Instance, "Single 加载后模块应为同一实例（DDOL 存活）");
            Assert.IsFalse(module == null, "模块不应被销毁");
        }

        /// <summary>
        /// Additive 成功路径：完成回调、追踪登记、激活场景保持不变（对齐 Unity 原生语义）。
        /// </summary>
        /// <remarks>
        /// [Order] 固定执行序：Single 用例必须最后执行——它会把测试场景 A 留作唯一已加载场景
        /// （TearDown 不得卸载最后一个场景），其后的用例对已加载的 A 再叠加会制造双实例，
        /// 违反模块"勿对同一路径重复叠加加载"的文档约定并使卸载断言失真。
        /// </remarks>
        [Order(1)]
        [UnityTest]
        public IEnumerator LoadSceneAdditive_SuccessPath_TracksScene_KeepsActiveScene()
        {
            var module = SceneModule.Instance;
            var activeBefore = SceneManager.GetActiveScene();
            var completed = false;
            var failed = false;

            module.LoadSceneAdditive(SceneAPath, () => completed = true, () => failed = true);

            yield return WaitUntil(() => completed || failed, "Additive 加载应在超时前回调");

            Assert.IsFalse(failed, "成功路径不应触发失败回调");
            Assert.AreEqual(1, module.AddedScenePaths.Count, "叠加加载应登记追踪");
            Assert.AreEqual(SceneAPath, module.AddedScenePaths[0], "追踪路径应为叠加场景路径");
            Assert.AreEqual(activeBefore, SceneManager.GetActiveScene(), "Additive 不应改变激活场景");
            Assert.IsTrue(SceneManager.GetSceneByPath(SceneAPath).isLoaded, "场景应真实加载");
        }

        /// <summary>
        /// UnloadAllAddedScenes 成功路径：全部叠加场景卸载、逐个移出追踪、完成回调、卸载事件逐场景广播。
        /// </summary>
        [Order(2)]
        [UnityTest]
        public IEnumerator UnloadAllAddedScenes_UnloadsAll_ClearsTracking_AndInvokesCallback()
        {
            var module = SceneModule.Instance;
            var aLoaded = false;
            var bLoaded = false;
            module.LoadSceneAdditive(SceneAPath, () => aLoaded = true);
            module.LoadSceneAdditive(SceneBPath, () => bLoaded = true);
            yield return WaitUntil(() => aLoaded && bLoaded, "两个叠加场景应在超时前加载完成");

            Assert.AreEqual(2, module.AddedScenePaths.Count, "前置：两个场景均已入追踪");

            var unloadedEvents = new List<string>();
            var handle = module.SceneUnloadedEvent.AddListener(unloadedEvents.Add);
            var allUnloaded = false;
            try
            {
                module.UnloadAllAddedScenes(() => allUnloaded = true);
                yield return WaitUntil(() => allUnloaded, "UnloadAll 应在超时前完成全部卸载");
            }
            finally
            {
                handle.Dispose();
            }

            Assert.AreEqual(2, unloadedEvents.Count, "每个卸载的场景应各广播一次 SceneUnloadedEvent");
            Assert.IsTrue(unloadedEvents.Contains(SceneAPath) && unloadedEvents.Contains(SceneBPath),
                "卸载事件参数应覆盖两个叠加场景");
            Assert.AreEqual(0, module.AddedScenePaths.Count, "批量卸载后追踪应清空");
            Assert.IsFalse(SceneManager.GetSceneByPath(SceneAPath).isLoaded, "场景 A 应已卸载");
            Assert.IsFalse(SceneManager.GetSceneByPath(SceneBPath).isLoaded, "场景 B 应已卸载");
        }

        /// <summary>
        /// 快照迭代语义（P2-S1 修复锁定）：卸载广播期间监听者嵌套叠加加载的新场景，
        /// 不在本趟卸载范围内（快照在趟首固定），且正常入追踪。
        /// </summary>
        [Order(3)]
        [UnityTest]
        public IEnumerator UnloadAllAddedScenes_SnapshotIteration_NestedLoadDuringBroadcastNotInThisBatch()
        {
            var module = SceneModule.Instance;
            var aLoaded = false;
            module.LoadSceneAdditive(SceneAPath, () => aLoaded = true);
            yield return WaitUntil(() => aLoaded, "叠加场景 A 应在超时前加载完成");

            var nestedLoadStarted = false;
            var bLoaded = false;
            var allUnloaded = false;
            var handle = module.SceneUnloadedEvent.AddListener(path =>
            {
                // A 的卸载广播内嵌套叠加 B：快照在趟首固定为 [A]，B 不在本趟卸载范围
                if (path == SceneAPath && !nestedLoadStarted)
                {
                    nestedLoadStarted = true;
                    module.LoadSceneAdditive(SceneBPath, () => bLoaded = true);
                }
            });

            try
            {
                module.UnloadAllAddedScenes(() => allUnloaded = true);
                yield return WaitUntil(() => allUnloaded, "UnloadAll 应在超时前完成本趟卸载");
            }
            finally
            {
                handle.Dispose();
            }

            Assert.IsTrue(nestedLoadStarted, "前置：嵌套加载应在广播期间被触发");
            Assert.IsTrue(allUnloaded, "本趟卸载应正常完成（嵌套加载不干扰快照迭代）");
            Assert.IsFalse(SceneManager.GetSceneByPath(SceneAPath).isLoaded, "快照内的 A 应已卸载");

            yield return WaitUntil(() => bLoaded, "嵌套加载的 B 应在超时前完成加载");
            Assert.IsTrue(SceneManager.GetSceneByPath(SceneBPath).isLoaded, "广播期间新叠加的 B 不应在本趟被卸载（快照语义）");
            Assert.AreEqual(1, module.AddedScenePaths.Count, "B 应正常入追踪");
            Assert.AreEqual(SceneBPath, module.AddedScenePaths[0], "追踪中应只剩 B");
        }

        /// <summary>
        /// 批量卸载重入保护（第二轮全仓锐评 P2-1 修复锁定）：卸载广播的监听者回调内再调
        /// UnloadAllAddedScenes（嵌套卸载全部）时，内层改用局部快照迭代——
        /// 修复前内层与外层共用复用快照缓冲，内层 finally Clear 会清空外层正在迭代的列表，
        /// 外层提前退出、剩余场景漏卸而 onAllUnloaded 仍误报完成。修复后内外两层各自完整完成。
        /// </summary>
        [Order(3)]
        [UnityTest]
        public IEnumerator UnloadAllAddedScenes_ReentrantUnloadDuringBroadcast_BothBatchesComplete()
        {
            var module = SceneModule.Instance;
            var aLoaded = false;
            var bLoaded = false;
            module.LoadSceneAdditive(SceneAPath, () => aLoaded = true);
            module.LoadSceneAdditive(SceneBPath, () => bLoaded = true);
            yield return WaitUntil(() => aLoaded && bLoaded, "两个叠加场景应在超时前加载完成");

            var outerCompleted = false;
            var innerCompleted = false;
            var reentryTriggered = false;
            var handle = module.SceneUnloadedEvent.AddListener(path =>
            {
                if (reentryTriggered)
                {
                    return;
                }

                // 收到首个卸载广播（外层迭代中）时嵌套再调 UnloadAllAddedScenes
                reentryTriggered = true;
                module.UnloadAllAddedScenes(() => innerCompleted = true);
            });

            try
            {
                module.UnloadAllAddedScenes(() => outerCompleted = true);
                yield return WaitUntil(() => outerCompleted && innerCompleted, "内外两层批量卸载均应在超时前完成");
            }
            finally
            {
                handle.Dispose();
            }

            Assert.IsTrue(reentryTriggered, "前置：广播期间应触发嵌套卸载");
            Assert.IsTrue(outerCompleted, "外层 onAllUnloaded 应正常完成（不被内层 Clear 截断）");
            Assert.IsTrue(innerCompleted, "内层 onAllUnloaded 应正常完成");
            Assert.AreEqual(0, module.AddedScenePaths.Count, "全部场景应被卸载、追踪清空");
            Assert.IsFalse(SceneManager.GetSceneByPath(SceneAPath).isLoaded, "A 应已卸载");
            Assert.IsFalse(SceneManager.GetSceneByPath(SceneBPath).isLoaded, "B 应已卸载（修复前会漏卸）");
        }

        /// <summary>
        /// 逐帧等待条件成立，超时断言失败（防止用例挂死拖垮整个批跑）。
        /// </summary>
        static IEnumerator WaitUntil(Func<bool> condition, string timeoutMessage, float timeoutSeconds = 30f)
        {
            var elapsed = 0f;
            while (!condition() && elapsed < timeoutSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.IsTrue(condition(), timeoutMessage + $"（等待 {elapsed:F1}s 超时）");
        }
    }
}
#endif
