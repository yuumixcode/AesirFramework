using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Runestone.AesirArchitecture.Tests
{
    /// <summary>
    /// 验证 <see cref="RemoveListenerOnSceneUnloadedTrigger" /> 的场景句柄分桶与自动移除行为。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 分桶键为 <see cref="Scene.handle"/>（int）而非场景名，保证：不同路径下的同名场景互不共享桶；
    /// 显式指定归属场景的监听不受其他场景（包括活动场景）卸载的影响。
    /// </para>
    /// <para>
    /// 测试覆盖两个维度：
    /// <list type="number">
    /// <item><b>显式归属场景</b>：additive 流程中活动场景与归属场景不同时，
    /// 卸载活动场景不得误清归属场景的监听，卸载归属场景时监听被正确移除。</item>
    /// <item><b>默认活动场景</b>：无参版本按注册时的活动场景归桶，该场景卸载时监听被移除。</item>
    /// </list>
    /// </para>
    /// <para>
    /// 场景加载/卸载回调仅在 PlayMode 下可用，故使用 <c>[UnityTest]</c>。
    /// 每个测试在 <see cref="TearDown" /> 中卸载测试创建的场景并恢复原活动场景，确保测试间隔离。
    /// </para>
    /// </remarks>
    /// <seealso cref="RemoveListenerExtensions"/>
    /// <seealso cref="RemoveListenerOnSceneUnloadedTrigger"/>
    public class RemoveListenerOnSceneUnloadedTriggerTests
    {
        Scene _originalActiveScene;

        /// <summary>
        /// 保存当前活动场景，供测试后恢复
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _originalActiveScene = SceneManager.GetActiveScene();
        }

        /// <summary>
        /// 尽力卸载测试创建的场景并恢复原活动场景，确保测试间隔离
        /// </summary>
        /// <remarks>
        /// 使用 <c>UnloadSceneAsync</c> 触发卸载（无需等待完成），即使断言失败也尽力清理，
        /// 避免残留场景影响后续测试的场景创建（<c>CreateScene</c> 要求场景名在已加载场景中唯一）。
        /// </remarks>
        [TearDown]
        public void TearDown()
        {
            UnloadIfLoaded("AesirBucketSceneA");
            UnloadIfLoaded("AesirBucketSceneB");
            UnloadIfLoaded("AesirBucketSceneC");
            SceneManager.SetActiveScene(_originalActiveScene);
        }

        /// <summary>
        /// 验证显式指定归属场景时，其他场景（含活动场景）的卸载不会误清监听，归属场景卸载时监听被正确移除。
        /// </summary>
        /// <remarks>
        /// 模拟 additive 多场景流程：监听归属场景 A，但注册时活动场景为 B。
        /// <list type="number">
        /// <item>卸载无关场景 B → 监听仍然生效（按句柄分桶，B 的卸载不触碰 A 的桶）；</item>
        /// <item>卸载归属场景 A → 监听自动移除，事件不再回调。</item>
        /// </list>
        /// 若分桶按场景名且注册时误用活动场景名（旧缺陷），卸载 B 会误清监听、卸载 A 反而不清理——
        /// 此测试两处断言均会失败。
        /// </remarks>
        [UnityTest]
        public IEnumerator RemoveListenerWhenOnSceneUnloaded_ExplicitScene_OtherSceneUnloadKeepsListener()
        {
            var sceneA = SceneManager.CreateScene("AesirBucketSceneA");
            var sceneB = SceneManager.CreateScene("AesirBucketSceneB");
            SceneManager.SetActiveScene(sceneB);

            var evt = new MiniEvent();
            var count = 0;
            var handle = evt.AddListener(() => count++);
            handle.RemoveListenerWhenOnSceneUnloaded(sceneA);

            evt.Invoke();
            Assert.AreEqual(1, count, "注册后监听应生效");

            var unloadB = SceneManager.UnloadSceneAsync(sceneB);
            yield return unloadB;

            evt.Invoke();
            Assert.AreEqual(2, count, "卸载无关场景 B 不应移除归属场景 A 的监听");

            var unloadA = SceneManager.UnloadSceneAsync(sceneA);
            yield return unloadA;

            evt.Invoke();
            Assert.AreEqual(2, count, "卸载归属场景 A 后监听应被自动移除");
            AesirArchitectureDebug.LogTestInfo(
                "RemoveListenerWhenOnSceneUnloaded(显式场景): 无关场景卸载不误清，归属场景卸载正确移除");
        }

        /// <summary>
        /// 验证无参版本按注册时的活动场景归桶：该场景卸载时监听被自动移除。
        /// </summary>
        /// <remarks>
        /// 覆盖向后兼容的默认路径：不显式指定场景时，行为与旧版一致——
        /// 归入当前活动场景的桶，随该场景卸载而清理。
        /// </remarks>
        [UnityTest]
        public IEnumerator RemoveListenerWhenOnSceneUnloaded_NoScene_RemovesOnActiveSceneUnload()
        {
            var sceneC = SceneManager.CreateScene("AesirBucketSceneC");
            SceneManager.SetActiveScene(sceneC);

            var evt = new MiniEvent();
            var count = 0;
            var handle = evt.AddListener(() => count++);
            handle.RemoveListenerWhenOnSceneUnloaded();

            evt.Invoke();
            Assert.AreEqual(1, count, "注册后监听应生效");

            var unloadC = SceneManager.UnloadSceneAsync(sceneC);
            yield return unloadC;

            evt.Invoke();
            Assert.AreEqual(1, count, "活动场景卸载后监听应被自动移除");
            AesirArchitectureDebug.LogTestInfo("RemoveListenerWhenOnSceneUnloaded(默认): 活动场景卸载后监听自动移除");
        }

        /// <summary>
        /// 若指定名称的场景仍处于加载状态则触发卸载（不等待完成）
        /// </summary>
        static void UnloadIfLoaded(string sceneName)
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName && scene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }
    }
}
