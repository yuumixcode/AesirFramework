using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Runestone.AesirArchitecture.Editor;
using UnityEditor;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="AesirAssetPaths" /> 锚点定位机制（参照 Odin 的 Lookup 资产）：
    /// 包根推导 / 父目录 / 包根验证纯函数，以及真实锚点资产的 GUID 定位链——
    /// Runestone 可移动到项目任意文件夹的核心保证。
    /// </summary>
    public class AesirAssetPathsTests
    {
        #region 纯函数 — 包根推导与父目录

        [Test]
        public void GetPackageRootFromLookupAssetPath_StandardLookupAsset()
        {
            // 锚点在包根：所在目录即包根（与包名无关，未来新包零改动接入）
            Assert.AreEqual("Assets/Custom/RS/AesirArchitecture",
                AesirAssetPaths.GetPackageRootFromLookupAssetPath(
                    "Assets/Custom/RS/AesirArchitecture/AesirPathLookup.asset"));
            Assert.AreEqual("Assets/Runestone/AesirModules",
                AesirAssetPaths.GetPackageRootFromLookupAssetPath(
                    "Assets/Runestone/AesirModules/AesirPathLookup.asset"));
        }

        [Test]
        public void GetPackageRootFromLookupAssetPath_EmptyInputsReturnNull()
        {
            Assert.IsNull(AesirAssetPaths.GetPackageRootFromLookupAssetPath(null));
            Assert.IsNull(AesirAssetPaths.GetPackageRootFromLookupAssetPath(string.Empty));
            // 裸文件名（无目录层级）与根级文件（首字符即斜杠）均无包根
            Assert.IsNull(AesirAssetPaths.GetPackageRootFromLookupAssetPath("AesirPathLookup.asset"));
            Assert.IsNull(AesirAssetPaths.GetPackageRootFromLookupAssetPath("/AesirPathLookup.asset"));
        }

        [Test]
        public void GetParentDirectory_ProjectRelativePaths()
        {
            Assert.AreEqual("Assets/Runestone",
                AesirAssetPaths.GetParentDirectory("Assets/Runestone/AesirArchitecture"));
            Assert.AreEqual("Assets/MyCompany/RS",
                AesirAssetPaths.GetParentDirectory("Assets/MyCompany/RS/AesirModules"));
            Assert.IsNull(AesirAssetPaths.GetParentDirectory(null));
            Assert.IsNull(AesirAssetPaths.GetParentDirectory(string.Empty));
            // "Assets" 是项目相对路径的第一层，无父层
            Assert.IsNull(AesirAssetPaths.GetParentDirectory("Assets"));
        }

        #endregion

        #region 纯函数 — 包根验证（Temp 临时目录 fixture）

        [Test]
        public void LooksLikeAesirPackageRootAbsolute_ValidatesPackageJson()
        {
            var dir = CreateTempDir();
            try
            {
                var pkgJsonPath = Path.Combine(dir, "package.json");

                // 含 Aesir id 前缀的 package.json → 判为 Aesir 包根
                File.WriteAllText(pkgJsonPath,
                    "{ \"name\": \"cn.runestone.aesir.architecture\", \"version\": \"0.23.0\" }");
                Assert.IsTrue(AesirAssetPaths.LooksLikeAesirPackageRootAbsolute(dir));

                // 非 Aesir 包 → 不判为包根（防止同名目录误判）
                File.WriteAllText(pkgJsonPath, "{ \"name\": \"com.other.pkg\" }");
                Assert.IsFalse(AesirAssetPaths.LooksLikeAesirPackageRootAbsolute(dir));

                // 无 package.json → 不判为包根
                File.Delete(pkgJsonPath);
                Assert.IsFalse(AesirAssetPaths.LooksLikeAesirPackageRootAbsolute(dir));

                // 空输入 fail-closed
                Assert.IsFalse(AesirAssetPaths.LooksLikeAesirPackageRootAbsolute(null));
                Assert.IsFalse(AesirAssetPaths.LooksLikeAesirPackageRootAbsolute(string.Empty));
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        /// <summary>Temp 下的临时 fixture 目录（项目根 Temp/ 已被 .gitignore 忽略，退出时由 Unity 清理）。</summary>
        static string CreateTempDir()
        {
            var dir = Path.Combine("Temp", "AesirAssetPathsTests", Path.GetRandomFileName());
            Directory.CreateDirectory(dir);
            return dir;
        }

        #endregion

        #region 集成 — 真实锚点资产与定位链

        [Test]
        public void LookupAssets_ResolveByFixedGuid()
        {
            // 两包锚点资产经固定 GUID 可定位（文件夹移动后 GUID 不变，定位靠它）
            var archPath = AssetDatabase.GUIDToAssetPath(AesirAssetPaths.ArchitectureLookupAssetGuid);
            var modPath = AssetDatabase.GUIDToAssetPath(AesirAssetPaths.ModulesLookupAssetGuid);

            Assert.IsNotEmpty(archPath);
            Assert.IsNotEmpty(modPath);
            Assert.AreEqual(AesirAssetPaths.LookupAssetFileName, Path.GetFileName(archPath));
            Assert.AreEqual(AesirAssetPaths.LookupAssetFileName, Path.GetFileName(modPath));

            // 期望 GUID 推导与锚点实际 GUID 一致
            Assert.AreEqual(AesirAssetPaths.ArchitectureLookupAssetGuid,
                AesirAssetPaths.GetExpectedLookupAssetGuid(archPath));
            Assert.AreEqual(AesirAssetPaths.ModulesLookupAssetGuid,
                AesirAssetPaths.GetExpectedLookupAssetGuid(modPath));
            Assert.IsNull(AesirAssetPaths.GetExpectedLookupAssetGuid("Assets/Folder/AesirPathLookup.asset"),
                "未知包目录名应返回 null（无法给出期望 GUID）");
        }

        [Test]
        public void InstallRoots_ContainsDefaultRootInDevelopmentRepository()
        {
            // 开发仓库默认位置安装：默认根被收录；主安装根为项目相对路径形态（无尾斜杠）
            Assert.IsTrue(AesirAssetPaths.InstallRoots.Contains(AesirAssetPaths.DefaultInstallRoot));
            Assert.AreEqual("Assets/Runestone", AesirAssetPaths.PrimaryInstallRoot);
            Assert.IsFalse(AesirAssetPaths.PrimaryInstallRoot.EndsWith("/"),
                "安装根不得带尾斜杠（消费端以 root + \"/\" 拼前缀）");
        }

        [Test]
        public void InstallRoots_AllEntriesAreUnderAssets()
        {
            // Assets 形态安装必在 Assets/ 之下（Unity 资产不能移出 Assets；防御断言锁定形态契约）
            foreach (var root in AesirAssetPaths.InstallRoots)
            {
                Assert.IsTrue(root.StartsWith("Assets/", StringComparison.Ordinal),
                    $"安装根应在 Assets/ 下：{root}");
            }
        }

        #endregion
    }
}
