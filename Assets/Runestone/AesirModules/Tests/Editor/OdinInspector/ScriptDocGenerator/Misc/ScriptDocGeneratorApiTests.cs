using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Runestone.AesirModules.ScriptDocGenerator.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>
    /// ScriptDocGeneratorAPI 静态门面回归：
    /// 程序集解析（短名 / FullName）、文件夹类型解析（普通类不依赖 MonoScript.GetClass）、
    /// 路径归一化、以及无 UI 端到端生成（写入系统 Temp，TearDown 清理，不触碰 Assets）。
    /// </summary>
    public class ScriptDocGeneratorApiTests
    {
        string _tempOutputFolder;

        [SetUp]
        public void SetUp()
        {
            _tempOutputFolder = Path.Combine(Path.GetTempPath(), "AesirScriptDocGeneratorApiTests");
            if (Directory.Exists(_tempOutputFolder))
            {
                Directory.Delete(_tempOutputFolder, true);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempOutputFolder))
            {
                Directory.Delete(_tempOutputFolder, true);
            }
        }

        #region 程序集解析

        [Test]
        public void ResolveScriptAssembly_ShortNameAndFullName_BothFound()
        {
            const string shortName = "Runestone.AesirModules.Editor.Bootstrap";

            var byShortName = ScriptDocGeneratorAPI.ResolveScriptAssembly(shortName);
            Assert.That(byShortName, Is.Not.Null, "短名（面板下拉同款）必须能解析到域内脚本程序集");

            var byFullName = ScriptDocGeneratorAPI.ResolveScriptAssembly(byShortName.FullName);
            Assert.That(byFullName, Is.EqualTo(byShortName), "FullName（面板序列化同款）必须解析到同一程序集");
        }

        [Test]
        public void ResolveScriptAssembly_MissingOrNull_ReturnsNull()
        {
            Assert.That(ScriptDocGeneratorAPI.ResolveScriptAssembly(null), Is.Null);
            Assert.That(ScriptDocGeneratorAPI.ResolveScriptAssembly(string.Empty), Is.Null);
            Assert.That(ScriptDocGeneratorAPI.ResolveScriptAssembly("NoSuch.Aesir.Assembly"), Is.Null);
        }

        #endregion

        #region 文件夹路径归一化

        [Test]
        public void TryNormalizeAssetFolderPath_ValidRelativePath_Accepted()
        {
            // Assets 根目录在任何项目形态（开发仓 / UPM 消费者工程）都存在
            var ok = ScriptDocGeneratorAPI.TryNormalizeAssetFolderPath("Assets/", out var normalized);

            Assert.That(ok, Is.True, "Assets 根目录必须通过归一化");
            Assert.That(normalized, Is.EqualTo("Assets"), "尾部斜杠应被去除");
        }

        [Test]
        public void TryNormalizeAssetFolderPath_AbsolutePathInsideProject_ConvertedToRelative()
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            Assume.That(projectRoot, Is.Not.Null);
            var absoluteAssets = projectRoot.Replace('\\', '/') + "/Assets";

            var ok = ScriptDocGeneratorAPI.TryNormalizeAssetFolderPath(absoluteAssets, out var normalized);

            Assert.That(ok, Is.True, "项目内绝对路径必须通过归一化");
            Assert.That(normalized, Is.EqualTo("Assets"), "项目内绝对路径应转换为项目相对路径");
        }

        [Test]
        public void TryNormalizeAssetFolderPath_InvalidPaths_Rejected()
        {
            Assert.That(ScriptDocGeneratorAPI.TryNormalizeAssetFolderPath(null, out _), Is.False);
            Assert.That(ScriptDocGeneratorAPI.TryNormalizeAssetFolderPath("  ", out _), Is.False);
            Assert.That(ScriptDocGeneratorAPI.TryNormalizeAssetFolderPath("/Applications", out _),
                Is.False, "项目外绝对路径 AssetDatabase 索引不到，必须拒绝");
            Assert.That(ScriptDocGeneratorAPI.TryNormalizeAssetFolderPath("Assets/NoSuchFolder", out _),
                Is.False, "不存在的目录必须拒绝");
            Assert.That(ScriptDocGeneratorAPI.TryNormalizeAssetFolderPath("Library/PackageManager", out _),
                Is.False, "Assets / Packages 之外的路径必须拒绝");
        }

        #endregion

        #region 文件夹类型解析

        [Test]
        public void ResolveTypesInFolder_SceneExceptionsFolder_FindsPlainClrTypes()
        {
            var exceptionsFolder = LocateSceneExceptionsFolder();
            Assume.That(exceptionsFolder, Is.Not.Null, "包内 Scene/Exceptions 目录应可定位（Assets / UPM 形态自适应）");

            var result = new ScriptDocGenerationResult();
            var types = ScriptDocGeneratorAPI.ResolveTypesInFolder(exceptionsFolder, result);

            // 普通异常类不是 UnityEngine.Object 派生，MonoScript.GetClass 拿不到——
            // 该用例锁定"源码扫描 + 简单名索引"路径必须覆盖普通 C# 类型
            Assert.That(types.Select(type => type.Name),
                Does.Contain(nameof(SceneAssetWrapperException)));
            Assert.That(types.Select(type => type.Name),
                Does.Contain(nameof(EmptySceneAssetWrapperException)));
            Assert.That(result.UnresolvedTypeNames, Is.Empty,
                "Exceptions 目录下类型全部在当前编译域内，不应有未解析项");
        }

        [Test]
        public void ResolveTypesInFolder_FolderWithoutScripts_ReturnsEmpty()
        {
            // 自建临时空文件夹夹具（谁创建谁删除），不依赖宿主工程目录结构
            var fixtureFolder = CreateFixtureFolder("SdgApiEmptyFolderFixture");
            try
            {
                var result = new ScriptDocGenerationResult();
                var types = ScriptDocGeneratorAPI.ResolveTypesInFolder(fixtureFolder, result);

                Assert.That(types, Is.Empty, "无脚本的文件夹不应解析出任何类型");
                Assert.That(result.UnresolvedTypeNames, Is.Empty);
            }
            finally
            {
                AssetDatabase.DeleteAsset(fixtureFolder);
            }
        }

        #endregion

        #region 端到端生成（写入系统 Temp）

        [Test]
        public void GenerateDocsForType_DefaultSettings_WritesMarkdown()
        {
            var result = ScriptDocGeneratorAPI.GenerateDocsForType(typeof(ScriptDocGeneratorTestEnum),
                ScriptDocGeneratorAPI.DefaultSettings, _tempOutputFolder);

            Assert.That(result.Success, Is.True);
            Assert.That(result.GeneratedCount, Is.EqualTo(1));
            Assert.That(result.SettingsName, Does.Contain("DefaultCnScriptingAPI"));
            var file = result.GeneratedFiles[0];
            Assert.That(File.Exists(file), Is.True, "文档应写入输出目录：" + file);
            Assert.That(File.ReadAllText(file), Does.Contain(nameof(ScriptDocGeneratorTestEnum)),
                "文档内容应包含类型名");
        }

        [Test]
        public void GenerateDocsForType_ZensicalSettings_ProducesFrontMatter()
        {
            var result = ScriptDocGeneratorAPI.GenerateDocsForType(typeof(ScriptDocGeneratorTestEnum),
                ScriptDocGeneratorAPI.ZensicalSettings, _tempOutputFolder);

            Assert.That(result.Success, Is.True);
            Assert.That(result.SettingsName, Does.Contain("ZensicalScriptingAPI"));
            Assert.That(File.ReadAllText(result.GeneratedFiles[0]), Does.StartWith("---"),
                "Zensical 生成器的产出必须自带 YAML Front Matter");
        }

        [Test]
        public void GenerateDocsForType_NullType_ReportsErrorAndReturnsEmptyResult()
        {
            // 日志经 AesirModulesDebug 输出（带 <color> 富文本前缀），故用 Regex 做子串匹配
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("请选择有效的目标类型"));

            var result = ScriptDocGeneratorAPI.GenerateDocsForType(null,
                ScriptDocGeneratorAPI.DefaultSettings, _tempOutputFolder);

            Assert.That(result.Success, Is.False);
            Assert.That(result.GeneratedFiles, Is.Empty);
        }

        [Test]
        public void GenerateDocsForTypes_EmptyList_ReportsError()
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("类型列表为空，无法生成文档"));

            var result = ScriptDocGeneratorAPI.GenerateDocsForTypes(Array.Empty<Type>(),
                ScriptDocGeneratorAPI.DefaultSettings, _tempOutputFolder);

            Assert.That(result.Success, Is.False);
        }

        [Test]
        public void GenerateDocsForAssembly_BootstrapShortName_GeneratesDocs()
        {
            var result = ScriptDocGeneratorAPI.GenerateDocsForAssembly("Runestone.AesirModules.Editor.Bootstrap",
                ScriptDocGeneratorAPI.DefaultSettings, _tempOutputFolder);

            Assert.That(result.Success, Is.True, "Bootstrap 程序集应至少为 AesirDependencyInstaller 生成文档");
            Assert.That(result.GeneratedCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(result.GeneratedFiles.All(File.Exists), Is.True);
        }

        [Test]
        public void GenerateDocsForAssembly_MissingAssembly_ReportsError()
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(
                @"找不到目标程序集"));

            var result = ScriptDocGeneratorAPI.GenerateDocsForAssembly("NoSuch.Aesir.Assembly",
                ScriptDocGeneratorAPI.DefaultSettings, _tempOutputFolder);

            Assert.That(result.Success, Is.False);
        }

        [Test]
        public void GenerateDocsForFolder_SceneExceptions_GeneratesAllExceptionDocs()
        {
            var exceptionsFolder = LocateSceneExceptionsFolder();
            Assume.That(exceptionsFolder, Is.Not.Null);

            var result = ScriptDocGeneratorAPI.GenerateDocsForFolder(exceptionsFolder,
                ScriptDocGeneratorAPI.DefaultSettings, _tempOutputFolder);

            Assert.That(result.Success, Is.True);
            Assert.That(result.GeneratedCount, Is.GreaterThanOrEqualTo(5),
                "Exceptions 目录含 5 个异常类（含嵌套派生），应全部生成");
            Assert.That(result.UnresolvedTypeNames, Is.Empty);
        }

        [Test]
        public void GenerateDocsForFolder_InvalidPath_ReportsError()
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(
                @"无效的脚本文件夹路径"));

            var result = ScriptDocGeneratorAPI.GenerateDocsForFolder("Library/PackageManager",
                ScriptDocGeneratorAPI.DefaultSettings, _tempOutputFolder);

            Assert.That(result.Success, Is.False);
        }

        #endregion

        [Test]
        public void FindAllSettings_ContainsBothBuiltInPresets()
        {
            // 先访问两个预设确保已创建，再断言枚举结果包含它们
            var defaults = ScriptDocGeneratorAPI.DefaultSettings;
            var zensical = ScriptDocGeneratorAPI.ZensicalSettings;

            var all = ScriptDocGeneratorAPI.FindAllSettings();

            Assert.That(all, Has.Count.GreaterThanOrEqualTo(2));
            Assert.That(all, Does.Contain(defaults));
            Assert.That(all, Does.Contain(zensical));
        }

        #region 夹具定位

        /// <summary>
        /// 按文件名定位包内 Scene/Exceptions 目录（Assets 安装 / UPM 形态自适应）。
        /// </summary>
        static string LocateSceneExceptionsFolder()
        {
            foreach (var guid in AssetDatabase.FindAssets("SceneAssetWrapperException"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("SceneAssetWrapperException.cs", StringComparison.OrdinalIgnoreCase))
                {
                    return Path.GetDirectoryName(path)?.Replace('\\', '/');
                }
            }

            return null;
        }

        /// <summary>
        /// 在包内测试目录下创建临时夹具文件夹，返回其 Assets 相对路径（调用方负责 DeleteAsset 回收）。
        /// </summary>
        static string CreateFixtureFolder(string folderName)
        {
            var miscFolder = LocateFolderOfThisTestFile();
            Assume.That(miscFolder, Is.Not.Null, "应能定位到本测试所在目录");
            var fixtureGuid = AssetDatabase.CreateFolder(miscFolder, folderName);
            return AssetDatabase.GUIDToAssetPath(fixtureGuid);
        }

        /// <summary>
        /// 按本测试文件名定位其所在目录（Assets 安装 / UPM 形态自适应）。
        /// </summary>
        static string LocateFolderOfThisTestFile()
        {
            foreach (var guid in AssetDatabase.FindAssets(nameof(ScriptDocGeneratorApiTests)))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("ScriptDocGeneratorApiTests.cs", StringComparison.OrdinalIgnoreCase))
                {
                    return Path.GetDirectoryName(path)?.Replace('\\', '/');
                }
            }

            return null;
        }

        #endregion
    }
}
