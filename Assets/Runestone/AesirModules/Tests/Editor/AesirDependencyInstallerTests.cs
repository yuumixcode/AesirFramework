using System.Reflection;
using NUnit.Framework;
using Runestone.AesirModules.Editor.Bootstrap;

namespace Runestone.AesirModules.Tests.Editor
{
    /// <summary>
    /// AesirDependencyInstaller（依赖补全器）单元测试。
    /// </summary>
    /// <remarks>
    /// 开发仓库中 RAA 以 Assets 形态在场，因此「缺依赖」分支无法真实复现；
    /// 缺失判定依赖各纯函数的参数化矩阵覆盖，在场判定直接断言本仓库环境（RAA 已装 ⇔ 菜单隐藏）。
    /// </remarks>
    public class AesirDependencyInstallerTests
    {
        const string ArchitecturePackageRoot = "Assets/Runestone/AesirArchitecture";
        const string ModulesPackageRoot = "Assets/Runestone/AesirModules";

        #region 依赖 URL

        [Test]
        public void DependencyGitUrl_AnchorsEvergreenLatestBranch()
        {
            // 常驻 latest 分支：URL 永久固定，一次输入即可持续获取最新版
            // （旧策略的版本分支随发版轮换并删除，钉住它的 URL 会失效报 "Could not clone"）。
            // 因此 URL 不得包含版本号——发版后无需回改本断言。
            Assert.AreEqual("https://github.com/yuumixcode/AesirFramework.git#AesirArchitecture-latest",
                AesirDependencyInstaller.DependencyGitUrl);
            Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(
                AesirDependencyInstaller.DependencyGitUrl, @"-v\d"),
                "依赖 URL 不得锚定随发版删除的版本分支");
        }

        #endregion

        #region 锚点路径 → 包根推导

        [Test]
        public void GetPackageRootFromAssetPath_ExtractsParentDirectory()
        {
            Assert.AreEqual(ArchitecturePackageRoot,
                AesirDependencyInstaller.GetPackageRootFromAssetPath(
                    ArchitecturePackageRoot + "/AesirPathLookup.asset"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("AesirPathLookup.asset")]
        public void GetPackageRootFromAssetPath_ReturnsNullForInvalidInput(string assetPath)
        {
            Assert.IsNull(AesirDependencyInstaller.GetPackageRootFromAssetPath(assetPath));
        }

        #endregion

        #region 包根 package.json 验证

        [Test]
        public void PackageRootHasArchitectureId_TrueForRealArchitectureRoot()
        {
            Assert.IsTrue(AesirDependencyInstaller.PackageRootHasArchitectureId(ArchitecturePackageRoot));
        }

        [Test]
        public void PackageRootHasArchitectureId_FalseForModulesRoot()
        {
            // RAM 包根 name 为 cn.runestone.aesir.modules；其 dependencies 虽引用 RAA 包 id，
            // 判定按 name 字段精确匹配，不得被依赖声明的子串误导
            Assert.IsFalse(AesirDependencyInstaller.PackageRootHasArchitectureId(ModulesPackageRoot));
        }

        [TestCase("Assets/Nonexistent")]
        [TestCase(null)]
        [TestCase("")]
        public void PackageRootHasArchitectureId_FalseForMissingOrEmpty(string packageRoot)
        {
            Assert.IsFalse(AesirDependencyInstaller.PackageRootHasArchitectureId(packageRoot));
        }

        #endregion

        #region UPM 注册名判定

        [Test]
        public void ContainsArchitectureName_Matrix()
        {
            // 特性实参不支持数组创建表达式（本编译器 CS0182），矩阵在方法体内展开
            Assert.IsFalse(AesirDependencyInstaller.ContainsArchitectureName(null));
            Assert.IsFalse(AesirDependencyInstaller.ContainsArchitectureName(new string[] { }));
            Assert.IsFalse(AesirDependencyInstaller.ContainsArchitectureName(
                new string[] { "com.unity.test-framework" }));
            Assert.IsFalse(AesirDependencyInstaller.ContainsArchitectureName(
                new string[] { "cn.runestone.aesir.modules" }));
            Assert.IsTrue(AesirDependencyInstaller.ContainsArchitectureName(
                new string[] { "com.unity.test-framework", "cn.runestone.aesir.architecture" }));
        }

        [Test]
        public void IsRegisteredInPackageManager_FalseInDevRepository()
        {
            // 开发仓库 RAA 源码在 Assets 下、不经 UPM 注册——同时实证
            // PackageInfo.GetAllRegisteredPackages() 在本编辑器版本可用且不抛异常
            Assert.IsFalse(AesirDependencyInstaller.IsRegisteredInPackageManager());
        }

        #endregion

        #region 在场总判定（开发仓库环境）

        [Test]
        public void HasArchitectureLookupAsset_TrueInDevRepository()
        {
            // 实测锚点定位链：FindAssets("AesirPathLookup") → 路径 → 包根 package.json 验证
            Assert.IsTrue(AesirDependencyInstaller.HasArchitectureLookupAsset());
        }

        [Test]
        public void IsArchitectureInstalled_TrueInDevRepository()
        {
            Assert.IsTrue(AesirDependencyInstaller.IsArchitectureInstalled());
        }

        [Test]
        public void ValidateMenuVisible_FalseWhenArchitectureInstalled()
        {
            // RAA 在场时菜单必须隐藏（UPM 自动拉取形态与正常安装形态均不应出现）
            var method = typeof(AesirDependencyInstaller).GetMethod("ValidateMenuVisible",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method);
            Assert.IsFalse((bool)method.Invoke(null, null));
        }

        #endregion
    }
}
