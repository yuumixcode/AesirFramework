#if UNITY_EDITOR // PlayMode 测试仅在编辑器内运行：需 UnityEditor 在编辑模式预构建阶段登记 / 摘除 BuildSettings
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Runestone.AesirModules.Tests
{
    /// <summary>
    /// <see cref="SceneModuleUniTask" /> 的 PlayMode 守护用例（覆盖 8 个公开 API 的加载 / 卸载 / 取消 / 宿主销毁链路）。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     为什么在 PlayMode：UniTask 驱动链是运行时行为——EditMode 下 <c>LoadSceneAsync</c> 空转、
    ///     <c>UnloadSceneAsync</c> 直接抛异常（见 <see cref="SceneModulePlayModeTests" /> 类备注），
    ///     真实加载只能在此验证。程序集带 <c>defineConstraints</c>（<c>UNITY_INCLUDE_TESTS</c> +
    ///     <c>AESIR_MODULES_UNITASK</c>）：**未装 UniTask 的工程整体不编译、计数不变**。
    ///     </para>
    ///     <para>
    ///     命名：程序集名带 <c>UniTask</c> 段（对齐测试程序集命名规则），但 <c>namespace</c> 停在
    ///     <c>Runestone.AesirModules.Tests</c>——否则 <c>UniTask</c> 段会成为同名兄弟命名空间，
    ///     遮蔽 <c>Cysharp.Threading.Tasks.UniTask</c>（CS0118，与 <c>Tests/Editor/Scene*</c> 段名同款坑）。
    ///     </para>
    ///     <para>
    ///     用例与方案 P2-1 清单的对应：#1 单参加载（含 #9 onProgress 单调性与归一化上限）、#2 wrapper 重载、
    ///     #3 叠加加载、#4 卸载与批量卸载、#5 无效路径、#6 入口即取消、#7 加载中取消、#8 宿主销毁。
    ///     </para>
    ///     <para>
    ///     测试卫生（照抄 <see cref="SceneModulePlayModeTests" /> 四件套）：BuildSettings 经
    ///     <see cref="IPrebuildSetup" /> / <see cref="IPostBuildCleanup" /> 仅存在于本次运行期间；
    ///     场景按文件名经 AssetDatabase 定位（Assets / 嵌入式 / UPM 三种安装形态自适应）；
    ///     域加载兜底清扫回收被强杀遗留的条目。
    ///     </para>
    /// </remarks>
    public class SceneModuleUniTaskPlayModeTests : IPrebuildSetup, IPostBuildCleanup
    {
        const string SceneAName = "SceneModulePlayTestA";
        const string SceneBName = "SceneModulePlayTestB";

        static string _sceneAPath;
        static string _sceneBPath;

        static string SceneAPath => _sceneAPath ??= ResolveTestScenePath(SceneAName);

        static string SceneBPath => _sceneBPath ??= ResolveTestScenePath(SceneBName);

        #region BuildSettings 登记（编辑模式预构建 / 清理阶段）

        void IPrebuildSetup.Setup()
        {
            EditorBuildSettings.scenes = EditorBuildSettings.scenes
                .Where(scene => !IsTestScenePath(scene.path))
                .Concat(new[] { SceneAPath, SceneBPath }
                    .Select(path => new EditorBuildSettingsScene(path, true)))
                .ToArray();
        }

        void IPostBuildCleanup.Cleanup() => UnregisterTestScenes();

        [InitializeOnLoadMethod]
        static void SweepStaleTestScenesOnDomainLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            UnregisterTestScenes();
        }

        static void UnregisterTestScenes()
        {
            var kept = EditorBuildSettings.scenes.Where(scene => !IsTestScenePath(scene.path)).ToArray();
            if (kept.Length != EditorBuildSettings.scenes.Length)
            {
                EditorBuildSettings.scenes = kept;
            }
        }

        static bool IsTestScenePath(string path) =>
            Path.GetFileNameWithoutExtension(path) == SceneAName ||
            Path.GetFileNameWithoutExtension(path) == SceneBName;

        /// <summary>按文件名定位测试场景（不写死 Assets 相对路径：安装形态可能是 Assets / Packages）。</summary>
        static string ResolveTestScenePath(string sceneName)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Scene " + sceneName))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == sceneName)
                {
                    return path;
                }
            }

            Assert.Fail($"未找到测试场景 {sceneName}（应与本测试程序集同级的 TestScenes/ 目录）");
            return null;
        }

        #endregion

        #region 等待辅助

        /// <summary>
        /// 逐帧轮询等待 <see cref="UniTask" /> 完成。异常经 <paramref name="onError" /> 回收
        /// （便于断言异常类型而非让用例直接失败）；<paramref name="onError" /> 为 null 时异常照常向上传播。
        /// </summary>
        static IEnumerator Await(UniTask task, Action<Exception> onError = null)
        {
            var awaiter = task.GetAwaiter();
            while (!awaiter.IsCompleted)
            {
                yield return null;
            }

            try
            {
                awaiter.GetResult();
            }
            catch (Exception e) when (onError != null)
            {
                onError(e);
            }
        }

        /// <summary>按真实时间等待条件成立（batchmode 下帧耗时远短于真实时间，不能按帧数等待）。</summary>
        static IEnumerator WaitUntil(Func<bool> condition, float timeoutSeconds, string what)
        {
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!condition() && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.IsTrue(condition(), $"等待超时：{what}（{timeoutSeconds} 秒）");
        }

        #endregion

        #region 生命周期卫生（SetUp / TearDown）

        /// <summary>锚场景名：运行时创建的空场景，不随包分发、与宿主工程无关。</summary>
        const string AnchorSceneName = "AesirUniTaskTestAnchor";

        static Scene _originalActiveScene;

        /// <summary>备好锚场景、清扫测试场景实例，随后捕获原始激活场景（供 <see cref="TearDown" /> 还原）。</summary>
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            EnsureAnchorScene();
            yield return SweepTestScenes();
            _originalActiveScene = SceneManager.GetActiveScene();
            yield return null;
        }

        /// <summary>清扫本用例遗留的测试场景实例并还原激活场景（BuildSettings 登记由 <see cref="IPostBuildCleanup" /> 负责）。</summary>
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return SweepTestScenes();

            if (_originalActiveScene.IsValid() && _originalActiveScene.isLoaded)
            {
                SceneManager.SetActiveScene(_originalActiveScene);
            }

            yield return null;
        }

        /// <summary>
        /// 保证锚场景存在（按名查找，不存在则运行时创建）：它使 <c>sceneCount &gt; 1</c> 恒成立，
        /// 于是每次都能把测试场景实例清扫干净。
        /// </summary>
        static void EnsureAnchorScene()
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == AnchorSceneName)
                {
                    return;
                }
            }

            SceneManager.CreateScene(AnchorSceneName);
        }

        /// <summary>
        /// 卸载所有已加载的测试场景实例（按 <see cref="Scene" /> 句柄逐个卸载）。
        /// </summary>
        /// <remarks>
        ///     <para>
        ///     为什么必须清扫：Single 加载会卸载全部旧场景，本套件与前序套件
        ///     （<see cref="SceneModulePlayModeTests" />）的 Single 用例都会"留下"它加载的那个场景。该遗留实例
        ///     一旦与后续用例的叠加目标同路径，叠加加载就会产出**第二个实例**——模块追踪按路径粒度只记一条、
        ///     按路径卸载只移除其一（见 <see cref="SceneModule.LoadSceneAdditive" /> 的约定），断言随之被污染：
        ///     实测遗留 B 时"卸载后场景应消失"必然失败，探针逐帧枚举证实 <c>scenes=[B, A, B]</c>
        ///     （卸载本身在两条驱动路径上时机一致，同帧即反映实例减少，故失败与驱动无关）。
        ///     </para>
        ///     <para>
        ///     锚场景是清扫能力的前提：Unity 拒绝卸载最后一个已加载场景，仅有"唯一遗留测试场景"时它无法被
        ///     清除。锚场景令本套件**与用例次序、与前序套件遗留解耦**（协程套件改用 <c>[Order]</c> 次序规避
        ///     同一问题，本套件用锚场景根治）。按句柄而非路径卸载：同路径多实例时路径卸载行为不可控。
        ///     </para>
        /// </remarks>
        static IEnumerator SweepTestScenes()
        {
            while (SceneManager.sceneCount > 1)
            {
                var target = default(Scene);
                for (var i = 0; i < SceneManager.sceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    if (scene.name == SceneAName || scene.name == SceneBName)
                    {
                        target = scene;
                        break;
                    }
                }

                if (!target.IsValid())
                {
                    yield break;
                }

                yield return SceneManager.UnloadSceneAsync(target);
            }
        }

        #endregion

        #region 用例

        [UnityTest]
        public IEnumerator LoadSceneSingleAsync_Path_SucceedsAndClearsTracking()
        {
            var loadedCount = 0;
            var progressReachedOne = false;
            var progressReachedOneBeforeEvent = false;
            void OnLoaded(string _)
            {
                loadedCount++;
                progressReachedOneBeforeEvent = progressReachedOne;
            }

            SceneModule.SceneLoadedEvent.AddListener(OnLoaded);
            var progress = new List<float>();
            try
            {
                yield return Await(SceneModuleUniTask.LoadSceneSingleAsync(SceneAPath, p =>
                {
                    progress.Add(p);
                    if (p >= 1f - 1e-3f)
                    {
                        progressReachedOne = true;
                    }
                }));

                Assert.AreEqual(SceneAPath, SceneManager.GetActiveScene().path, "Single 加载后激活场景应为目标");
                Assert.AreEqual(1, loadedCount, "SceneLoadedEvent 应恰好触发一次");
                Assert.AreEqual(0, SceneModule.AddedScenePaths.Count, "Single 加载成功后应清空叠加追踪");

                // #9 onProgress：至少报告一次、单调不减、归一化到 1.0 收尾（CompleteLoad 收尾 onProgress(1f)，
                // 与协程驱动路径一致——加载期间按 progressCap 归一化，故中间值不超过 1）
                Assert.Greater(progress.Count, 0, "onProgress 应至少报告一次");
                for (var i = 1; i < progress.Count; i++)
                {
                    Assert.GreaterOrEqual(progress[i], progress[i - 1], "进度应单调不减");
                    Assert.LessOrEqual(progress[i], 1f + 1e-4f, "归一化进度不应超过 1");
                }

                Assert.GreaterOrEqual(progress[progress.Count - 1], 1f - 1e-3f,
                    "进度末值应归一化到 1.0（0.9 激活上限归一化，CompleteLoad 收尾）");
                Assert.IsTrue(progressReachedOneBeforeEvent, "事件广播时进度应已报告到 1.0（onProgress(1f) 先于广播）");
            }
            finally
            {
                SceneModule.SceneLoadedEvent.RemoveListener(OnLoaded);
            }
        }

        [UnityTest]
        public IEnumerator LoadSceneSingleAsync_Wrapper_Succeeds()
        {
            var wrapper = SceneAssetWrapper.FromScenePath(SceneBPath);
            Assert.IsNotNull(wrapper, "前置：wrapper 构造成功");

            yield return Await(SceneModuleUniTask.LoadSceneSingleAsync(wrapper));

            Assert.AreEqual(SceneBPath, SceneManager.GetActiveScene().path, "wrapper 重载与路径重载语义一致");
        }

        [UnityTest]
        public IEnumerator LoadSceneAdditiveAsync_KeepsActiveSceneAndTracksPath()
        {
            var activeBefore = SceneManager.GetActiveScene().path;

            yield return Await(SceneModuleUniTask.LoadSceneAdditiveAsync(SceneAPath));

            Assert.AreEqual(activeBefore, SceneManager.GetActiveScene().path, "Additive 不改变激活场景");
            CollectionAssert.Contains(SceneModule.AddedScenePaths, SceneAPath, "叠加加载应记入追踪列表");

            // 收尾：卸载本次叠加的场景，避免影响后续用例
            yield return Await(SceneModuleUniTask.UnloadSceneAsync(SceneAPath));
        }

        [UnityTest]
        public IEnumerator UnloadSceneAsync_And_UnloadAllAddedScenesAsync()
        {
            yield return Await(SceneModuleUniTask.LoadSceneAdditiveAsync(SceneBPath));

            var unloadedCount = 0;
            var eventCount = 0;
            void OnUnloaded(string _) => eventCount++;
            SceneModule.SceneUnloadedEvent.AddListener(OnUnloaded);
            try
            {
                yield return Await(SceneModuleUniTask.UnloadSceneAsync(SceneBPath));
                unloadedCount = 1;

                Assert.AreEqual(1, eventCount, "SceneUnloadedEvent 应恰好触发一次");
                Assert.IsFalse(SceneManager.GetSceneByPath(SceneBPath).isLoaded, "场景应从场景管理器消失");
                Assert.IsFalse(SceneModule.AddedScenePaths.Contains(SceneBPath), "卸载后应移出叠加追踪");
            }
            finally
            {
                SceneModule.SceneUnloadedEvent.RemoveListener(OnUnloaded);
            }

            Assert.AreEqual(1, unloadedCount, "前置：卸载用例已执行");

            // 空追踪调 UnloadAllAddedScenesAsync 是 no-op（不抛）
            yield return Await(SceneModuleUniTask.UnloadAllAddedScenesAsync());

            Assert.AreEqual(0, SceneModule.AddedScenePaths.Count, "批量卸载后叠加追踪应清空");
        }

        [UnityTest]
        public IEnumerator LoadSceneSingleAsync_InvalidPath_ThrowsInvalidOperationException()
        {
            var loadedCount = 0;
            void OnLoaded(string _) => loadedCount++;
            SceneModule.SceneLoadedEvent.AddListener(OnLoaded);
            try
            {
                Exception caught = null;
                // SceneModule 会输出错误日志（门面前缀带富文本），用 Regex 子串兜住，否则判为 Unexpected log
                LogAssert.Expect(LogType.Error,
                    new System.Text.RegularExpressions.Regex("无效场景路径"));

                yield return Await(SceneModuleUniTask.LoadSceneSingleAsync(string.Empty), e => caught = e);

                Assert.IsNotNull(caught, "无效路径应让 await 侧以异常收场");
                Assert.IsInstanceOf<InvalidOperationException>(caught, "失败语义为 InvalidOperationException");
                Assert.AreEqual(0, loadedCount, "失败不得触发 SceneLoadedEvent");
            }
            finally
            {
                SceneModule.SceneLoadedEvent.RemoveListener(OnLoaded);
            }
        }

        [UnityTest]
        public IEnumerator LoadSceneSingleAsync_AlreadyCancelled_ThrowsSynchronouslyWithoutLoading()
        {
            var loadedCount = 0;
            void OnLoaded(string _) => loadedCount++;
            SceneModule.SceneLoadedEvent.AddListener(OnLoaded);
            try
            {
                using var cts = new CancellationTokenSource();
                cts.Cancel();

                // 入口即取消：同步抛（不进入等待），也不向 SceneModule 发起加载
                Assert.Throws<OperationCanceledException>(() =>
                    SceneModuleUniTask.LoadSceneSingleAsync(SceneAPath, null, cts.Token));

                yield return null;
                Assert.AreEqual(0, loadedCount, "入口即取消不得发起加载");
            }
            finally
            {
                SceneModule.SceneLoadedEvent.RemoveListener(OnLoaded);
            }
        }

        [UnityTest]
        public IEnumerator LoadSceneSingleAsync_CancelledDuringLoad_StopsWaitingButLoadCompletes()
        {
            using var cts = new CancellationTokenSource();
            var task = SceneModuleUniTask.LoadSceneSingleAsync(SceneAPath, null, cts.Token);
            cts.Cancel();

            Exception caught = null;
            yield return Await(task, e => caught = e);

            Assert.IsNotNull(caught, "取消应让 await 侧以异常收场");
            Assert.IsInstanceOf<OperationCanceledException>(caught, "取消仅中止等待");

            // 底层流程继续完成：稍后场景确实已加载（锁定"取消仅中止等待"的公开语义）
            yield return WaitUntil(() => SceneManager.GetActiveScene().path == SceneAPath, 10f,
                "取消后底层加载流程应继续完成");
        }

        [UnityTest]
        public IEnumerator LoadSceneAdditiveAsync_HostDestroyed_StopsWaitingWithoutHanging()
        {
            var task = SceneModuleUniTask.LoadSceneAdditiveAsync(SceneBPath);
            var host = SceneModule.Instance;
            Assert.IsNotNull(host, "前置：模块宿主存在");

            // 销毁宿主组件：await 侧的 destroyCancellationToken 链接应让等待方以取消收场（不无限悬挂）
            UnityEngine.Object.Destroy(host);

            Exception caught = null;
            yield return Await(task, e => caught = e);

            Assert.IsNotNull(caught, "宿主销毁后等待方应收场");
            Assert.IsInstanceOf<OperationCanceledException>(caught, "宿主销毁以取消语义收场");
        }

        #endregion
    }
}
#endif
