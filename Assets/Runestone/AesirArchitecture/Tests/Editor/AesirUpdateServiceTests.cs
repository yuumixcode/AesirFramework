using System;
using System.IO;
using NUnit.Framework;
using Runestone.AesirArchitecture.Editor;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="AesirUpdateService" /> 的纯逻辑部分：版本比较、package.json 字段解析、
    /// 清单差集计算与残留删除、空目录回收、备份复制与裁剪、清单 JSON 解析与合并、Release 资产定位。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     网络请求（GitHub API / 资产下载）与 AssetDatabase.ImportPackage 涉及外部依赖，不在单测范围，
    ///     由编辑器内手动验证。本类只覆盖可在临时目录中确定性行为的文件与字符串逻辑。
    ///     </para>
    ///     <para>
    ///     临时目录放在项目 Temp/ 下（Unity 会忽略且不入库），每个测试用例使用独立随机子目录并在
    ///     TearDown 中删除，避免用例间与重复运行间相互污染。
    ///     </para>
    /// </remarks>
    public class AesirUpdateServiceTests
    {
        /// <summary>本用例独占的临时根目录（绝对路径）。</summary>
        string _testRoot;

        [SetUp]
        public void SetUp()
        {
            _testRoot = Path.Combine(AesirUpdateService.ToAbsolutePath("Temp"),
                "AesirUpdateServiceTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testRoot);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testRoot))
            {
                Directory.Delete(_testRoot, true);
            }
        }

        #region 版本比较

        [Test]
        public void CompareVersion_NumericNotLexicographic()
        {
            // 0.14 与 0.9 按数字比较必须大于（按字符串比较会得出相反结果）
            Assert.Greater(AesirUpdateService.CompareVersion("0.14.0", "0.9.0"), 0);
        }

        [Test]
        public void CompareVersion_PrefixAndPadding()
        {
            Assert.AreEqual(0, AesirUpdateService.CompareVersion("v0.14.0", "0.14.0"));
            Assert.AreEqual(0, AesirUpdateService.CompareVersion("1.0", "1.0.0"));
            Assert.AreEqual(0, AesirUpdateService.CompareVersion("", "0.0.0"));
        }

        [Test]
        public void CompareVersion_Ordering()
        {
            Assert.Less(AesirUpdateService.CompareVersion("0.14.0", "0.14.1"), 0);
            Assert.Greater(AesirUpdateService.CompareVersion("0.15.0", "0.14.0"), 0);
            Assert.Greater(AesirUpdateService.CompareVersion("1.0.0", "0.99.99"), 0);
        }

        #endregion

        #region package.json 解析

        [Test]
        public void ParsePackageJson_ExtractsNameAndVersion()
        {
            var path = Path.Combine(_testRoot, "package.json");
            File.WriteAllText(path,
                "{\n  \"name\": \"cn.runestone.aesir.architecture\",\n  \"displayName\": \"Aesir Architecture\",\n  \"version\": \"0.14.0\"\n}");

            var (name, version) = AesirUpdateService.ParsePackageJson(path);

            Assert.AreEqual("cn.runestone.aesir.architecture", name);
            Assert.AreEqual("0.14.0", version);
        }

        [Test]
        public void ParsePackageJson_MissingFileReturnsEmpty()
        {
            var (name, version) = AesirUpdateService.ParsePackageJson(
                Path.Combine(_testRoot, "not-exist.json"));

            Assert.IsEmpty(name);
            Assert.IsEmpty(version);
        }

        #endregion

        #region 清单差集与残留删除

        [Test]
        public void ComputeStaleFiles_ReturnsRemovedEntriesWithinPackageOnly()
        {
            string[] previous =
            {
                "Assets/Runestone/AesirArchitecture/A.cs",
                "Assets/Runestone/AesirArchitecture/Old/Old.cs",
                "Assets/Runestone/AesirArchitecture/Keep.cs",
                "Assets/Runestone/AesirModules/B.cs",
            };
            string[] current =
            {
                "Assets/Runestone/AesirArchitecture/A.cs",
                "Assets/Runestone/AesirArchitecture/Keep.cs",
                "Assets/Runestone/AesirArchitecture/New.cs",
            };

            var stale = AesirUpdateService.ComputeStaleFiles(
                previous, current, "Assets/Runestone/AesirArchitecture");

            // 只保留本包范围内、新版清单中不存在的条目；Modules 的 B.cs 不归本次清理
            CollectionAssert.AreEqual(new[] { "Assets/Runestone/AesirArchitecture/Old/Old.cs" }, stale);
        }

        [Test]
        public void ComputeStaleFiles_EmptyPreviousMeansNoDeletionBasis()
        {
            // 首次安装 / 无历史记录时没有任何删除依据，必须返回空（宁可残留不误删）
            var stale = AesirUpdateService.ComputeStaleFiles(
                null, new[] { "Assets/Runestone/AesirArchitecture/A.cs" }, "Assets/Runestone/AesirArchitecture");

            Assert.IsEmpty(stale);
        }

        [Test]
        public void DeleteStaleEntries_RemovesFilesWithMetaAndEmptyDirs()
        {
            var pkgRoot = Path.Combine(_testRoot, "Pkg");
            CreateFileWithMeta(Path.Combine(pkgRoot, "A", "a.cs"));
            CreateFileWithMeta(Path.Combine(pkgRoot, "Old", "old.cs"));
            CreateFileWithMeta(Path.Combine(pkgRoot, "Old", "Sub", "x.cs"));
            Directory.CreateDirectory(Path.Combine(pkgRoot, "Old", "EmptyDir"));

            var stale = new[]
            {
                Rel(Path.Combine(pkgRoot, "Old", "old.cs")),
                Rel(Path.Combine(pkgRoot, "Old", "Sub", "x.cs")),
                Rel(Path.Combine(pkgRoot, "Old", "EmptyDir")),
                Rel(Path.Combine(_testRoot, "Ghost.cs")), // 不存在的条目应被忽略
            };

            var deleted = AesirUpdateService.DeleteStaleEntries(stale);

            Assert.AreEqual(3, deleted); // 2 个文件 + 1 个空目录
            Assert.IsFalse(File.Exists(Path.Combine(pkgRoot, "Old", "old.cs")));
            Assert.IsFalse(File.Exists(Path.Combine(pkgRoot, "Old", "old.cs.meta")));
            Assert.IsFalse(Directory.Exists(Path.Combine(pkgRoot, "Old", "EmptyDir")));
            Assert.IsTrue(File.Exists(Path.Combine(pkgRoot, "A", "a.cs")));
        }

        [Test]
        public void PruneEmptyDirectories_DeepesFirstAndKeepsRoot()
        {
            var pkgRoot = Path.Combine(_testRoot, "Pkg");
            CreateFileWithMeta(Path.Combine(pkgRoot, "A", "a.cs"));
            CreateFileWithMeta(Path.Combine(pkgRoot, "Old", "Sub", "x.cs"));

            // 先删掉唯一文件，让 Old/Sub 与 Old 依次变空
            File.Delete(Path.Combine(pkgRoot, "Old", "Sub", "x.cs"));
            File.Delete(Path.Combine(pkgRoot, "Old", "Sub", "x.cs.meta"));

            var pruned = AesirUpdateService.PruneEmptyDirectories(Rel(pkgRoot));

            Assert.AreEqual(2, pruned); // Sub 与 Old 先后被回收
            Assert.IsFalse(Directory.Exists(Path.Combine(pkgRoot, "Old")));
            Assert.IsTrue(Directory.Exists(pkgRoot)); // 包根目录本身不回收
            Assert.IsTrue(File.Exists(Path.Combine(pkgRoot, "A", "a.cs")));
        }

        #endregion

        #region 备份

        [Test]
        public void BackupRunestone_CopiesContentAndPrunesOldest()
        {
            var srcAbs = Path.Combine(_testRoot, "src");
            Directory.CreateDirectory(srcAbs);
            File.WriteAllText(Path.Combine(srcAbs, "hello.txt"), "content");

            var sourceRel = Rel(srcAbs);
            var backupRootRel = Rel(Path.Combine(_testRoot, "backups"));

            var first = AesirUpdateService.BackupRunestone("20260101-000000_v0.1.0", sourceRel, backupRootRel, 2);
            var second = AesirUpdateService.BackupRunestone("20260102-000000_v0.2.0", sourceRel, backupRootRel, 2);
            var third = AesirUpdateService.BackupRunestone("20260103-000000_v0.3.0", sourceRel, backupRootRel, 2);

            // 时间戳前缀保证 Ordinal 排序即时间序：保留最近 2 份，最旧的被裁掉
            Assert.IsFalse(Directory.Exists(first));
            Assert.IsTrue(Directory.Exists(second));
            Assert.IsTrue(Directory.Exists(third));
            StringAssert.AreEqualIgnoringCase("content",
                File.ReadAllText(Path.Combine(third, "hello.txt")));
        }

        [Test]
        public void BackupRunestone_MissingSourceReturnsNull()
        {
            var result = AesirUpdateService.BackupRunestone("20260101-000000_v0.1.0",
                Rel(Path.Combine(_testRoot, "not-exist")), Rel(Path.Combine(_testRoot, "backups")), 3);

            Assert.IsNull(result);
        }

        #endregion

        #region 清单 JSON 与合并

        [Test]
        public void ParseFilesManifest_ArrayRoundtrip()
        {
            const string json = @"
{
    ""packages"": [
        { ""name"": ""AesirArchitecture"", ""version"": ""0.15.0"", ""files"": [""Assets/Runestone/AesirArchitecture/package.json""] },
        { ""name"": ""AesirModules"", ""version"": ""0.15.0"", ""files"": [] }
    ]
}";

            var manifest = AesirUpdateService.ParseFilesManifest(json);

            Assert.IsNotNull(manifest);
            Assert.AreEqual(2, manifest.packages.Length);
            Assert.AreEqual("0.15.0", manifest.GetPackage("AesirArchitecture").version);
            Assert.IsNull(manifest.GetPackage("NotInstalled"));
        }

        [Test]
        public void ParseFilesManifest_InvalidOrEmptyReturnsNull()
        {
            Assert.IsNull(AesirUpdateService.ParseFilesManifest(null));
            Assert.IsNull(AesirUpdateService.ParseFilesManifest(""));
            Assert.IsNull(AesirUpdateService.ParseFilesManifest("not a json"));
        }

        [Test]
        public void MergePackageEntry_ReplacesByNameAndAppendsNew()
        {
            var local = new AesirUpdateService.FilesManifest
            {
                packages = new[]
                {
                    new AesirUpdateService.FilesManifest.PackageEntry
                    {
                        name = "AesirArchitecture", version = "0.14.0",
                        files = new[] { "Assets/Runestone/AesirArchitecture/old.cs" }
                    },
                }
            };
            var incoming = new AesirUpdateService.FilesManifest.PackageEntry
            {
                name = "AesirArchitecture", version = "0.15.0",
                files = new[] { "Assets/Runestone/AesirArchitecture/new.cs" }
            };
            var modules = new AesirUpdateService.FilesManifest.PackageEntry
            {
                name = "AesirModules", version = "0.15.0", files = Array.Empty<string>()
            };

            var merged = AesirUpdateService.MergePackageEntry(local, incoming);
            merged = AesirUpdateService.MergePackageEntry(merged, modules);

            Assert.AreEqual(2, merged.packages.Length);
            Assert.AreEqual("0.15.0", merged.GetPackage("AesirArchitecture").version);
            CollectionAssert.AreEqual(new[] { "Assets/Runestone/AesirArchitecture/new.cs" },
                merged.GetPackage("AesirArchitecture").files);
            Assert.IsNotNull(merged.GetPackage("AesirModules"));
            // 输入清单不被修改（合并返回新实例）
            Assert.AreEqual("0.14.0", local.GetPackage("AesirArchitecture").version);
        }

        [Test]
        public void MergePackageEntry_NullLocalStartsFresh()
        {
            var entry = new AesirUpdateService.FilesManifest.PackageEntry
            {
                name = "AesirArchitecture", version = "0.15.0", files = Array.Empty<string>()
            };

            var merged = AesirUpdateService.MergePackageEntry(null, entry);

            Assert.AreEqual(1, merged.packages.Length);
            Assert.AreSame(entry, merged.packages[0]);
        }

        #endregion

        #region Release 资产定位

        [Test]
        public void FindUnityPackageAsset_MatchesNamingConvention()
        {
            var release = new AesirUpdateService.ReleaseInfo
            {
                tag_name = "v0.15.0",
                assets = new[]
                {
                    new AesirUpdateService.ReleaseAsset
                        { name = "AesirArchitecture-v0.15.0.unitypackage", browser_download_url = "url-arch" },
                    new AesirUpdateService.ReleaseAsset
                        { name = "AesirModules-v0.15.0.unitypackage", browser_download_url = "url-mods" },
                    new AesirUpdateService.ReleaseAsset
                        { name = "files-manifest.json", browser_download_url = "url-manifest" },
                    new AesirUpdateService.ReleaseAsset
                        { name = "source.zip", browser_download_url = "url-zip" },
                }
            };

            Assert.AreEqual("url-arch",
                AesirUpdateService.FindUnityPackageAsset(release, "AesirArchitecture").browser_download_url);
            Assert.AreEqual("url-mods",
                AesirUpdateService.FindUnityPackageAsset(release, "AesirModules").browser_download_url);
            Assert.IsNull(AesirUpdateService.FindUnityPackageAsset(release, "NotShipped"));
            Assert.AreEqual("url-manifest", AesirUpdateService.FindManifestAsset(release).browser_download_url);
        }

        #endregion

        #region 辅助

        /// <summary>把绝对路径转为项目相对路径（正斜杠），供 DeleteStaleEntries 等相对路径 API 使用。</summary>
        string Rel(string absolutePath)
        {
            var projectRoot = Path.GetFullPath(AesirUpdateService.ProjectRootPath);
            var full = Path.GetFullPath(absolutePath);
            var relative = full.StartsWith(projectRoot, StringComparison.Ordinal)
                ? full.Substring(projectRoot.Length)
                : full;
            return relative.Replace('\\', '/').TrimStart('/');
        }

        /// <summary>创建带同名 .meta 的文件（模拟真实资产布局）。</summary>
        static void CreateFileWithMeta(string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, "// test");
            File.WriteAllText(filePath + ".meta", "fileFormatVersion: 2");
        }

        #endregion
    }
}
