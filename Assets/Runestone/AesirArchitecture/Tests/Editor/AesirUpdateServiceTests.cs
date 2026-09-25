using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Runestone.AesirArchitecture.Editor;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="AesirUpdateService" /> 的纯逻辑部分：版本比较、package.json 字段解析、
    /// 清单差集计算与残留删除、空目录回收、备份复制与裁剪、清单 JSON 解析与合并、
    /// update-info 解析、重定向 tag 提取、下载地址构造。
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
            _testRoot = Path.Combine(AesirUpdateService.ToAbsolutePath("Temp"), "AesirUpdateServiceTests",
                Guid.NewGuid().ToString("N"));
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

        #region 清单落盘往返

        [Test]
        public void SaveAndLoadLocalManifest_RoundTrips()
        {
            var stateAbs = AesirUpdateService.ToAbsolutePath(AesirUpdateService.StateFilePath);
            var stateDirAbs = AesirUpdateService.ToAbsolutePath(AesirUpdateService.StateDirName);
            var hadOriginal = File.Exists(stateAbs);
            var original = hadOriginal ? File.ReadAllText(stateAbs) : null;

            try
            {
                var manifest = new AesirUpdateService.FilesManifest
                {
                    packages = new[]
                    {
                        new AesirUpdateService.FilesManifest.PackageEntry
                        {
                            name = "AesirArchitecture",
                            version = "0.20.0",
                            files = new[]
                            {
                                "Assets/Runestone/AesirArchitecture/a.cs",
                                "Assets/Runestone/AesirArchitecture/b.cs"
                            }
                        }
                    }
                };
                AesirUpdateService.SaveLocalManifest(manifest);

                var loaded = AesirUpdateService.LoadLocalManifest();
                Assert.IsNotNull(loaded, "写入后应能读回清单");
                Assert.AreEqual(1, loaded.packages.Length);
                Assert.AreEqual("AesirArchitecture", loaded.packages[0].name);
                Assert.AreEqual("0.20.0", loaded.packages[0].version);
                Assert.AreEqual(2, loaded.packages[0].files.Length);
                Assert.AreEqual("Assets/Runestone/AesirArchitecture/a.cs", loaded.packages[0].files[0]);
            }
            finally
            {
                // 恢复测试前的真实状态文件（项目状态不受测试污染）
                if (hadOriginal)
                {
                    File.WriteAllText(stateAbs, original);
                }
                else if (File.Exists(stateAbs))
                {
                    File.Delete(stateAbs);
                    if (Directory.Exists(stateDirAbs) &&
                        !Enumerable.Any(Directory.EnumerateFileSystemEntries(stateDirAbs)))
                    {
                        Directory.Delete(stateDirAbs);
                    }
                }
            }
        }

        #endregion

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

        [Test]
        public void CompareVersion_Prerelease_LowerThanRelease()
        {
            // SemVer：预发布版低于同号正式版（装过 rc 后正式版必须被判为更新）
            Assert.Less(AesirUpdateService.CompareVersion("0.15.0-rc1", "0.15.0"), 0);
            Assert.Greater(AesirUpdateService.CompareVersion("0.15.0", "0.15.0-rc1"), 0);
            Assert.Less(AesirUpdateService.CompareVersion("v0.16.0-alpha", "0.16.0"), 0);

            // 预发布号仍按数字段优先比较
            Assert.Less(AesirUpdateService.CompareVersion("0.15.0-rc1", "0.16.0"), 0);
            Assert.Greater(AesirUpdateService.CompareVersion("0.16.0-alpha", "0.15.99"), 0);

            // 两侧均为预发布：按标识文本排序
            Assert.Less(AesirUpdateService.CompareVersion("0.15.0-alpha", "0.15.0-beta"), 0);
            Assert.AreEqual(0, AesirUpdateService.CompareVersion("0.15.0-rc1", "0.15.0-rc1"));
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
            var (name, version) =
                AesirUpdateService.ParsePackageJson(Path.Combine(_testRoot, "not-exist.json"));

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
                "Assets/Runestone/AesirModules/B.cs"
            };
            string[] current =
            {
                "Assets/Runestone/AesirArchitecture/A.cs",
                "Assets/Runestone/AesirArchitecture/Keep.cs",
                "Assets/Runestone/AesirArchitecture/New.cs"
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
            var stale = AesirUpdateService.ComputeStaleFiles(null,
                new[] { "Assets/Runestone/AesirArchitecture/A.cs" }, "Assets/Runestone/AesirArchitecture");

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
                Rel(Path.Combine(_testRoot, "Ghost.cs")) // 不存在的条目应被忽略
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

            var first =
                AesirUpdateService.BackupRunestone("20260101-000000_v0.1.0", sourceRel, backupRootRel, 2);
            var second =
                AesirUpdateService.BackupRunestone("20260102-000000_v0.2.0", sourceRel, backupRootRel, 2);
            var third =
                AesirUpdateService.BackupRunestone("20260103-000000_v0.3.0", sourceRel, backupRootRel, 2);

            // 时间戳前缀保证 Ordinal 排序即时间序：保留最近 2 份，最旧的被裁掉
            Assert.IsFalse(Directory.Exists(first));
            Assert.IsTrue(Directory.Exists(second));
            Assert.IsTrue(Directory.Exists(third));
            StringAssert.AreEqualIgnoringCase("content", File.ReadAllText(Path.Combine(third, "hello.txt")));
        }

        [Test]
        public void BackupRunestone_MissingSourceReturnsNull()
        {
            var result = AesirUpdateService.BackupRunestone("20260101-000000_v0.1.0",
                Rel(Path.Combine(_testRoot, "not-exist")), Rel(Path.Combine(_testRoot, "backups")));

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
                    }
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

        #region 远程检测纯逻辑（update-info 解析 / 重定向 tag 提取 / 下载地址构造）

        [Test]
        public void ParseUpdateInfo_VersionTagAndPackages()
        {
            const string json = @"
{
    ""version"": ""0.15.0"",
    ""tag"": ""v0.15.0"",
    ""packages"": [
        { ""name"": ""AesirArchitecture"", ""version"": ""0.15.0"", ""files"": [""Assets/Runestone/AesirArchitecture/package.json""] },
        { ""name"": ""AesirModules"", ""version"": ""0.15.0"", ""files"": [] }
    ]
}";

            var info = AesirUpdateService.ParseUpdateInfo(json);

            Assert.IsNotNull(info);
            Assert.AreEqual("0.15.0", info.version);
            Assert.AreEqual("v0.15.0", info.tag);
            Assert.AreEqual(2, info.packages.Length);
            Assert.AreEqual("0.15.0", info.GetPackage("AesirArchitecture").version);
            Assert.IsNull(info.GetPackage("NotInstalled"));
        }

        [Test]
        public void ParseUpdateInfo_InvalidOrEmptyReturnsNull()
        {
            Assert.IsNull(AesirUpdateService.ParseUpdateInfo(null));
            Assert.IsNull(AesirUpdateService.ParseUpdateInfo(""));
            Assert.IsNull(AesirUpdateService.ParseUpdateInfo("not a json"));
        }

        [Test]
        public void ExtractTagFromLocation_GitHubRedirectFormats()
        {
            // 绝对地址（curl 实测格式）
            Assert.AreEqual("v1.0.246-Unity2018Compatible",
                AesirUpdateService.ExtractTagFromLocation(
                    "https://github.com/liangxiegame/QFramework/releases/tag/v1.0.246-Unity2018Compatible"));
            // 相对地址（Location 可能只给路径）
            Assert.AreEqual("v0.15.0",
                AesirUpdateService.ExtractTagFromLocation("/yuumixcode/AesirFramework/releases/tag/v0.15.0"));
            // 不匹配 / 空值
            Assert.IsNull(
                AesirUpdateService.ExtractTagFromLocation("https://github.com/yuumixcode/AesirFramework"));
            Assert.IsNull(AesirUpdateService.ExtractTagFromLocation(null));
            Assert.IsNull(AesirUpdateService.ExtractTagFromLocation(""));
        }

        [Test]
        public void ReleaseSnapshot_UnityPackageUrlFollowsNamingConvention()
        {
            var snapshot = new AesirUpdateService.ReleaseSnapshot
            {
                Source = "jsDelivr (cdn.jsdelivr.net)",
                Tag = "v0.15.0"
            };

            // 资产命名约定 <包目录名>-v<版本>.unitypackage，下载走 GitHub Release 直链
            StringAssert.AreEqualIgnoringCase(
                "https://github.com/yuumixcode/AesirFramework/releases/download/v0.15.0/AesirArchitecture-v0.15.0.unitypackage",
                snapshot.GetUnityPackageUrl("AesirArchitecture"));
            StringAssert.AreEqualIgnoringCase(
                "https://github.com/yuumixcode/AesirFramework/releases/download/v0.15.0/AesirModules-v0.15.0.unitypackage",
                snapshot.GetUnityPackageUrl("AesirModules"));
        }

        #endregion

        #region 更新日志（CHANGELOG 解析 / 筛选 / 渲染 / 摘要）

        /// <summary>与仓库内包 CHANGELOG 同构的样例文本（含 Unreleased、分隔线与空行干扰）。</summary>
        const string SampleChangelog = @"
# Changelog

本项目的所有重要变更均会记录在此文件中。

## [Unreleased]

### 规划中

- 某个规划项

## [0.21.0] - 2026-09-13

### Added

- 新功能甲
- 新功能乙

### Fixed

- 修复丙

---

## [0.20.0] - 2026-09-11

### Changed

- 变更丁

## [0.19.0]

### Fixed

- 修复戊
";

        [Test]
        public void ParseChangelogSections_ParsesVersionDateAndContentInOrder()
        {
            var sections = AesirUpdateService.ParseChangelogSections(SampleChangelog);

            Assert.AreEqual(3, sections.Count);
            Assert.AreEqual("0.21.0", sections[0].Version);
            Assert.AreEqual("2026-09-13", sections[0].Date);
            StringAssert.Contains("### Added", sections[0].Content);
            StringAssert.Contains("- 新功能乙", sections[0].Content);
            StringAssert.Contains("- 修复丙", sections[0].Content);

            Assert.AreEqual("0.20.0", sections[1].Version);
            StringAssert.Contains("- 变更丁", sections[1].Content);

            // 日期可缺省
            Assert.AreEqual("0.19.0", sections[2].Version);
            Assert.IsEmpty(sections[2].Date);
        }

        [Test]
        public void ParseChangelogSections_SkipsNonVersionHeadersAndCleansContent()
        {
            var sections = AesirUpdateService.ParseChangelogSections(SampleChangelog);

            // Unreleased 不产生段落；段落正文不越过下一个版本标题
            Assert.IsFalse(sections.Exists(s => s.Version.Contains("Unreleased")));
            StringAssert.DoesNotContain("规划中", sections[0].Content);
            StringAssert.DoesNotContain("0.20.0", sections[0].Content);
            // 首尾空行与 --- 分隔线被剔除
            StringAssert.DoesNotContain("---", sections[0].Content);
            Assert.IsFalse(sections[0].Content.StartsWith("\n"));
            Assert.IsFalse(sections[0].Content.EndsWith("\n"));
        }

        [Test]
        public void ParseChangelogSections_OtherLevel2HeaderEndsCurrentSection()
        {
            // 根聚合 CHANGELOG 中的「## 当前版本 / Current Version」不属于任何版本段落
            const string markdown = "## [0.21.0] - 2026-09-13\n\n- 新功能甲\n\n" +
                                    "## 当前版本 / Current Version\n\n| 表格 |\n\n" +
                                    "## [0.20.0] - 2026-09-11\n\n- 变更丁\n";

            var sections = AesirUpdateService.ParseChangelogSections(markdown);

            Assert.AreEqual(2, sections.Count);
            StringAssert.DoesNotContain("当前版本", sections[0].Content);
            StringAssert.DoesNotContain("表格", sections[0].Content);
        }

        [Test]
        public void ParseChangelogSections_NullOrEmptyReturnsEmpty()
        {
            Assert.IsEmpty(AesirUpdateService.ParseChangelogSections(null));
            Assert.IsEmpty(AesirUpdateService.ParseChangelogSections(""));
            Assert.IsEmpty(AesirUpdateService.ParseChangelogSections("# 只有标题\n- 无版本段落\n"));
        }

        [Test]
        public void CollectNewerSections_FiltersByLocalAndRemoteRange()
        {
            var sections = AesirUpdateService.ParseChangelogSections(SampleChangelog);

            // 本地 0.20.0 → 远程 v0.21.0（tag 带 v 前缀）：只剩 0.21.0
            var newer = AesirUpdateService.CollectNewerSections(sections, "0.20.0", "v0.21.0");
            Assert.AreEqual(1, newer.Count);
            Assert.AreEqual("0.21.0", newer[0].Version);

            // 本地已是最新 → 空
            Assert.IsEmpty(AesirUpdateService.CollectNewerSections(sections, "0.21.0", "0.21.0"));

            // 远程不设上限：本地 0.19.0 → 0.20.0 与 0.21.0（保持新 → 旧顺序）
            var all = AesirUpdateService.CollectNewerSections(sections, "0.19.0", null);
            Assert.AreEqual(2, all.Count);
            Assert.AreEqual("0.21.0", all[0].Version);
            Assert.AreEqual("0.20.0", all[1].Version);

            // 上限裁剪：本地 0.18.0、远程 0.20.0 → 不含 0.21.0
            var capped = AesirUpdateService.CollectNewerSections(sections, "0.18.0", "0.20.0");
            Assert.AreEqual(2, capped.Count);
            Assert.IsFalse(capped.Exists(s => s.Version == "0.21.0"));
        }

        [Test]
        public void RenderChangelogText_RestoresHeadersAndContent()
        {
            var sections = AesirUpdateService.ParseChangelogSections(SampleChangelog);
            var rendered = AesirUpdateService.RenderChangelogText(
                AesirUpdateService.CollectNewerSections(sections, "0.20.0", "0.21.0"));

            StringAssert.Contains("## [0.21.0] - 2026-09-13", rendered);
            StringAssert.Contains("- 新功能甲", rendered);
            StringAssert.DoesNotContain("0.19.0", rendered);

            Assert.IsEmpty(AesirUpdateService.RenderChangelogText(null));
            Assert.IsEmpty(AesirUpdateService.RenderChangelogText(
                new List<AesirUpdateService.ChangelogSection>()));
        }

        [Test]
        public void LoadLocalChangelog_ReadsFileOrReturnsNull()
        {
            var pkgRel = Rel(Path.Combine(_testRoot, "Pkg"));
            Assert.IsNull(AesirUpdateService.LoadLocalChangelog(pkgRel));

            Directory.CreateDirectory(Path.Combine(_testRoot, "Pkg"));
            File.WriteAllText(Path.Combine(_testRoot, "Pkg", "CHANGELOG.md"), "# Changelog\n内容");
            Assert.AreEqual("# Changelog\n内容", AesirUpdateService.LoadLocalChangelog(pkgRel));
        }

        #endregion

        #region 更新确认框文本

        [Test]
        public void BuildUpdateConfirmation_ListsTargetsAndBackupNotice()
        {
            var targets = new[]
            {
                new AesirUpdateService.InstalledPackage { DirName = "AesirArchitecture", Version = "0.20.0" },
                new AesirUpdateService.InstalledPackage { DirName = "AesirModules", Version = "0.19.0" }
            };

            var message = AesirUpdateService.BuildUpdateConfirmation(targets, "v0.21.0", false);

            StringAssert.Contains("AesirArchitecture", message);
            StringAssert.Contains("v0.20.0 → v0.21.0", message);
            StringAssert.Contains("v0.19.0 → v0.21.0", message);
            StringAssert.Contains(AesirUpdateService.BackupDirName, message);
            StringAssert.Contains("确认开始更新？", message);
            // 非 git 仓库不带开发仓库警告
            StringAssert.DoesNotContain(".git", message);
        }

        [Test]
        public void BuildUpdateConfirmation_GitRepositoryAppendsWarning()
        {
            var targets = new[]
            {
                new AesirUpdateService.InstalledPackage { DirName = "AesirArchitecture", Version = "0.20.0" }
            };

            var message = AesirUpdateService.BuildUpdateConfirmation(targets, "v0.21.0", true);

            StringAssert.Contains(".git", message);
            StringAssert.Contains("开发仓库", message);
        }

        #endregion

        #region 待更新包计算

        static AesirUpdateService.InstalledPackage Pkg(string dirName, string packageId, string version) =>
            new AesirUpdateService.InstalledPackage
            {
                DirName = dirName, PackageId = packageId, Version = version,
                AssetsPath = "Assets/Runestone/" + dirName
            };

        [Test]
        public void ComputeOutdatedPackages_SortsByDependencyOrder()
        {
            // Modules 在输入中排前——输出必须 Architecture 在前（依赖顺序）
            var packages = new List<AesirUpdateService.InstalledPackage>
            {
                Pkg("AesirModules", "cn.runestone.aesir.modules", "0.19.0"),
                Pkg("AesirArchitecture", "cn.runestone.aesir.architecture", "0.19.0")
            };

            var outdated = AesirUpdateService.ComputeOutdatedPackages(packages, "0.20.0");

            Assert.AreEqual(2, outdated.Count);
            Assert.AreEqual("cn.runestone.aesir.architecture", outdated[0].PackageId,
                "Architecture 必须先于 Modules（依赖顺序）");
            Assert.AreEqual("cn.runestone.aesir.modules", outdated[1].PackageId);
        }

        [Test]
        public void ComputeOutdatedPackages_VersionBoundaries()
        {
            var packages = new List<AesirUpdateService.InstalledPackage>
            {
                Pkg("AesirArchitecture", "cn.runestone.aesir.architecture", "0.20.0"),
                Pkg("AesirModules", "cn.runestone.aesir.modules", "0.19.0")
            };

            // 本地 == 远程：不算待更新；本地 > 远程：不算待更新；空远程：空列表
            Assert.AreEqual(1, AesirUpdateService.ComputeOutdatedPackages(packages, "0.20.0").Count,
                "仅低于远程的包计入");
            Assert.AreEqual(0, AesirUpdateService.ComputeOutdatedPackages(packages, "0.19.0").Count);
            Assert.AreEqual(0, AesirUpdateService.ComputeOutdatedPackages(packages, null).Count);
            Assert.AreEqual(0, AesirUpdateService.ComputeOutdatedPackages(packages, "").Count);
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
