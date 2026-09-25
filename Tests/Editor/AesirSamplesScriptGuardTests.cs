using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Runestone.AesirArchitecture.Editor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 示例脚本构建剔除守护 — 运行时示例程序集内的每个 .cs 必须整文件 <c>#if UNITY_EDITOR</c> 包裹，
    /// 保证玩家构建示例类型 0 入包（「示例不进玩家构建」的脚本侧；场景侧见
    /// <see cref="AesirSamplesBuildFilterTests" />）。
    /// </summary>
    /// <remarks>
    /// 示例程序集有意为运行时程序集（Editor-only asmdef 的 MonoBehaviour 禁止挂载到场景物体，
    /// 0.14.0 实测全示例场景 Missing Script），故构建剔除只能靠整文件包裹；本守护把该约定锁进测试，
    /// 新增示例脚本漏包裹、包裹格式偏差（如 using 指令落在 #if 之外）都会在此失败。
    /// <para>
    /// 扫描覆盖三类真实安装形态：开发仓库 / unitypackage（经 <see cref="AesirAssetPaths" />
    /// 锚点定位的安装根下 <c>&lt;包&gt;/Samples/</c> 与 <c>Samples~/</c> 双份）与
    /// Package Manager 导入（<c>Assets/Samples/Aesir Architecture|Modules/</c>，存在才扫——
    /// UPM 安装且未导入示例的消费者环境自然通过）。
    /// </para>
    /// </remarks>
    public class AesirSamplesScriptGuardTests
    {
        [Test]
        public void RuntimeSampleScripts_AreWholeFileEditorWrapped()
        {
            var violations = new List<string>();
            var scannedRuntimeScripts = 0;
            foreach (var samplesDir in EnumerateSampleDirectories())
            {
                ScanDirectory(samplesDir, violations, ref scannedRuntimeScripts);
            }

            // sanity：本地存在 Aesir 安装根时必须实际扫到运行时示例脚本，
            // 防止扫描路径逻辑损坏后"0 个文件 = 通过"的假绿
            if (AesirAssetPaths.InstallRoots.Count > 0)
            {
                Assert.Greater(scannedRuntimeScripts, 0,
                    "扫描到 0 个运行时示例脚本——扫描逻辑可能已损坏");
            }

            Assert.IsEmpty(violations,
                "存在未整文件 #if UNITY_EDITOR 包裹的运行时示例脚本（玩家构建将包含示例类型）:\n" +
                string.Join("\n", violations));
        }

        #region 扫描实现

        /// <summary>待扫描的示例目录（项目相对路径）：安装根 × 两包 × Samples 双目录 + Package Manager 导入根。</summary>
        static IEnumerable<string> EnumerateSampleDirectories()
        {
            foreach (var root in AesirAssetPaths.InstallRoots)
            {
                foreach (var package in new[] { "AesirArchitecture", "AesirModules" })
                {
                    foreach (var samples in new[] { "Samples", "Samples~" })
                    {
                        var dir = root + "/" + package + "/" + samples;
                        if (Directory.Exists(ToAbsolute(dir)))
                        {
                            yield return dir;
                        }
                    }
                }
            }

            foreach (var packageManagerRoot in new[]
                     {
                         "Assets/Samples/Aesir Architecture",
                         "Assets/Samples/Aesir Modules"
                     })
            {
                if (Directory.Exists(ToAbsolute(packageManagerRoot)))
                {
                    yield return packageManagerRoot;
                }
            }
        }

        /// <summary>扫描一个示例目录：运行时程序集内每个 .cs 必须整文件包裹，无 asmdef 归属视为违规。</summary>
        static void ScanDirectory(string projectRelativeDir, List<string> violations,
            ref int scannedRuntimeScripts)
        {
            var absoluteDir = ToAbsolute(projectRelativeDir);

            // asmdef 注册表：目录绝对路径 → 是否 Editor-only（includePlatforms 含 Editor）
            var asmdefs = new Dictionary<string, bool>();
            foreach (var asmdefPath in Directory.GetFiles(
                         absoluteDir, "*.asmdef", SearchOption.AllDirectories))
            {
                asmdefs[Path.GetDirectoryName(asmdefPath)] = IsEditorOnlyAssembly(asmdefPath);
            }

            foreach (var scriptPath in Directory.GetFiles(
                         absoluteDir, "*.cs", SearchOption.AllDirectories))
            {
                if (!TryFindNearestAsmdef(scriptPath, asmdefs, out var editorOnly))
                {
                    violations.Add("无 asmdef 归属: " + scriptPath);
                    continue;
                }

                if (editorOnly)
                {
                    continue; // Editor-only 程序集整体不进玩家构建，无需包裹
                }

                scannedRuntimeScripts++;
                if (!IsWholeFileEditorWrapped(scriptPath))
                {
                    violations.Add("未整文件包裹: " + scriptPath);
                }
            }
        }

        /// <summary>asmdef 是否为 Editor-only 程序集（includePlatforms 含 Editor）。</summary>
        static bool IsEditorOnlyAssembly(string asmdefPath)
        {
            var definition = JsonUtility.FromJson<AssemblyDefinition>(File.ReadAllText(asmdefPath));
            var platforms = definition.includePlatforms;
            if (platforms == null)
            {
                return false;
            }

            foreach (var platform in platforms)
            {
                if (string.Equals(platform, "Editor", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>从脚本所在目录逐级向上找最近 asmdef；找不到返回 false（示例脚本必须归属程序集）。</summary>
        static bool TryFindNearestAsmdef(string scriptPath, Dictionary<string, bool> asmdefs,
            out bool editorOnly)
        {
            var directory = Path.GetDirectoryName(scriptPath);
            while (!string.IsNullOrEmpty(directory))
            {
                if (asmdefs.TryGetValue(directory, out editorOnly))
                {
                    return true;
                }

                directory = Path.GetDirectoryName(directory);
            }

            editorOnly = false;
            return false;
        }

        /// <summary>整文件包裹判定：首个非空行以 <c>#if UNITY_EDITOR</c> 开头且最后一个非空行是 <c>#endif</c>。</summary>
        static bool IsWholeFileEditorWrapped(string scriptPath)
        {
            var lines = File.ReadAllText(scriptPath).Trim().Split('\n');
            string first = null;
            string last = null;
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                first ??= line.TrimStart();
                last = line.TrimStart();
            }

            return first != null &&
                   first.StartsWith("#if UNITY_EDITOR", StringComparison.Ordinal) &&
                   last.StartsWith("#endif", StringComparison.Ordinal);
        }

        /// <summary>asmdef 的最小解析模型（JsonUtility 忽略未声明字段）。</summary>
        [Serializable]
        class AssemblyDefinition
        {
            public string name;
            public string[] includePlatforms;
        }

        #endregion

        #region 路径工具

        /// <summary>项目相对路径 → 绝对路径。</summary>
        static string ToAbsolute(string projectRelativePath) =>
            Path.GetFullPath(Path.Combine(ProjectRootPath, projectRelativePath));

        /// <summary>Unity 项目根目录（Assets 的上一级）。</summary>
        static string ProjectRootPath =>
            Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;

        #endregion
    }
}
