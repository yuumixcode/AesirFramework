using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// Aesir 示例构建剔除钩子 — 构建发起时自动把本次构建场景列表中的 Aesir 示例场景剔除，并输出明细日志。
    /// <para>
    /// 示例面向编辑器内学习，不应进入玩家构建：示例脚本已由整文件 <c>#if UNITY_EDITOR</c> 在编译期剔除，
    /// 本钩子补上场景侧——用户手动加进 Build Settings 的示例场景在构建时自动跳过，
    /// 且 Build Settings 窗口中的场景列表数据不被修改（剔除只作用于本次构建选项）。
    /// </para>
    /// <para>
    /// 实现：编辑器加载时经 <see cref="BuildPlayerWindow.RegisterBuildPlayerHandler" /> 注册构建入口回调，
    /// 在回调中改写 <see cref="BuildPlayerOptions.scenes" /> 后转交
    /// <see cref="BuildPlayerWindow.DefaultBuildMethods.BuildPlayer" />。
    /// 不用 <c>IPreprocessBuildWithReport</c>：其 OnPreprocessBuild 触发时场景列表已快照进构建选项，
    /// 回调内改 <c>EditorBuildSettings</c> 影响不到本次构建、还会污染持久数据。
    /// </para>
    /// </summary>
    /// <remarks>
    /// 边界：只拦截 Build Settings 窗口发起的构建——自定义构建脚本 / CI 直接调用
    /// <c>BuildPipeline.BuildPlayer</c> 时不经过本钩子，场景列表由调用方自行组织。
    /// </remarks>
    internal static class AesirSamplesBuildFilter
    {
        /// <summary>
        /// 注册构建入口回调（域重载后委托清空，每次重载重新注册；
        /// RegisterBuildPlayerHandler 为覆盖式注册，不会累积多个回调）。
        /// </summary>
        [InitializeOnLoadMethod]
        static void Register() => BuildPlayerWindow.RegisterBuildPlayerHandler(OnBuildPlayer);

        /// <summary>构建入口回调：剔除示例场景后转交默认构建流程。</summary>
        static void OnBuildPlayer(BuildPlayerOptions options)
        {
            options.scenes = FilterSampleScenes(options.scenes, out var removedScenes);
            if (removedScenes.Count > 0)
            {
                Debug.Log("[Aesir Build] 已从本次构建剔除 " + removedScenes.Count + " 个 Aesir 示例场景（示例不进玩家构建）：\n" +
                          string.Join("\n", removedScenes));
            }

            BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
        }

        /// <summary>
        /// 判断构建场景是否属于 Aesir 示例，覆盖三类真实安装形态下的示例场景路径：
        /// <list type="bullet">
        ///     <item>
        ///     开发仓库 / unitypackage 导入：<c>&lt;定位到的安装根&gt;/&lt;包目录&gt;/Samples/&lt;示例&gt;/…</c>
        ///     （安装根由 <see cref="AesirAssetPaths" /> 锚点定位，Runestone 可移动到项目任意文件夹，
        ///     默认为 <c>Assets/Runestone</c>）
        ///     </item>
        ///     <item>Package Manager 导入（Aesir Architecture）：<c>Assets/Samples/Aesir Architecture/&lt;版本&gt;/…</c></item>
        ///     <item>Package Manager 导入（Aesir Modules）：<c>Assets/Samples/Aesir Modules/&lt;版本&gt;/…</c></item>
        /// </list>
        /// Package Manager 导入根下只匹配 Aesir 两包前缀——用户经其他包导入的 Samples 不受影响。
        /// </summary>
        internal static bool IsSampleScene(string scenePath) =>
            IsSampleScene(scenePath, AesirAssetPaths.InstallRoots);

        /// <summary>
        /// <see cref="IsSampleScene(string)" /> 的可注入重载（测试用）：按给定安装根列表判定。
        /// </summary>
        /// <param name="scenePath">场景路径。</param>
        /// <param name="installRoots">本地安装根列表（项目相对路径）。</param>
        internal static bool IsSampleScene(string scenePath, IReadOnlyList<string> installRoots)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                return false;
            }

            // Package Manager 导入形态：Unity 固定路径，不受 Runestone 移动影响
            if (scenePath.StartsWith("Assets/Samples/Aesir Architecture/",
                    StringComparison.OrdinalIgnoreCase) ||
                scenePath.StartsWith("Assets/Samples/Aesir Modules/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Assets 安装形态：定位到的安装根（可移动）下任意层级的 Samples 段
            if (installRoots == null)
            {
                return false;
            }

            foreach (var root in installRoots)
            {
                if (scenePath.StartsWith(root + "/", StringComparison.OrdinalIgnoreCase) &&
                    scenePath.IndexOf("/Samples/", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 从构建场景列表剔除 Aesir 示例场景，保留序不变。
        /// </summary>
        /// <param name="scenes">本次构建的场景路径列表。</param>
        /// <param name="removedScenes">被剔除的示例场景路径（按出现顺序）。</param>
        /// <returns>剔除后的场景列表。</returns>
        internal static string[] FilterSampleScenes(string[] scenes, out List<string> removedScenes)
        {
            removedScenes = new List<string>();
            var kept = new List<string>(scenes.Length);
            foreach (var scene in scenes)
            {
                if (IsSampleScene(scene))
                {
                    removedScenes.Add(scene);
                }
                else
                {
                    kept.Add(scene);
                }
            }

            return kept.ToArray();
        }
    }
}
