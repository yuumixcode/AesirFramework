using System.Linq;
using NUnit.Framework;
using UnityEditor;

namespace Runestone.AesirModules.Tests.Editor
{
    /// <summary>
    /// 测试场景卫生守护：测试场景只允许在 PlayMode 套件运行期间临时登记进 <see cref="EditorBuildSettings" />，
    /// 绝不允许常驻——常驻即被打进玩家构建，并污染宿主工程配置。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     锁定 2026-09-25 修复：PlayMode 场景套件此前经 <c>[InitializeOnLoadMethod]</c> 在编辑模式域加载期
    ///     把测试场景登记为 BuildSettings enabled 条目且从不摘除，条目随工程配置落盘、进入构建；
    ///     包导入消费工程后测试场景路径不存在，还会直接抛 CreationException。现行机制为
    ///     <c>IPrebuildSetup</c> 登记 + <c>IPostBuildCleanup</c> 摘除（仅存在于本次运行期间）。
    ///     </para>
    ///     <para>
    ///     本用例在 EditMode 运行：若上一轮 PlayMode 运行被强杀（编辑器崩溃、进程被 kill）留下条目，
    ///     这里即红灯——重跑一次 PlayMode 套件（预构建阶段按名回收遗留条目）即可恢复。
    ///     </para>
    /// </remarks>
    public class SceneModuleTestSceneHygieneTests
    {
        [Test]
        public void BuildSettings_DoesNotPermanentlyContainTestScenes()
        {
            var testScenes = EditorBuildSettings.scenes
                .Where(scene => IsUnderTestFolder(scene.path))
                .Select(scene => scene.path)
                .ToArray();

            Assert.IsEmpty(testScenes,
                "测试场景不得常驻 EditorBuildSettings（会被打进玩家构建）：" + string.Join(", ", testScenes));
        }

        /// <summary>测试场景位于包内 <c>Tests/</c> 目录——Assets 安装与 Packages 安装两种形态都覆盖。</summary>
        static bool IsUnderTestFolder(string path)
        {
            return path.Replace('\\', '/').Contains("/Tests/");
        }
    }
}
