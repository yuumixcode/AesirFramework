using System;
using System.Collections.Generic;
using NUnit.Framework;
using Runestone.AesirArchitecture.Editor;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="AesirGetStartedService" /> 的 UPM 示例导入链路：确认框 / Toast 文案构造、
    /// 导入后位置推导，与 <see cref="AesirGetStartedService.ImportUpmSample" /> 的结果矩阵。
    /// Sample 查找经可选参数注入伪造实现，不触碰真实 Package Manager（不弹确认框、不动文件系统）。
    /// </summary>
    public class AesirGetStartedServiceTests
    {
        const string PackageId = "cn.runestone.aesir.architecture";
        const string PackageDisplayName = "Aesir Architecture";
        const string PackageVersion = "0.28.0";
        const string SampleName = "Counter-MVC-Quick（快捷档）";

        #region 夹具构造

        static AesirGetStartedService.AesirPackageInfo CreateUpmPackage() => new AesirGetStartedService.AesirPackageInfo
        {
            Id = PackageId,
            DisplayName = PackageDisplayName,
            Version = PackageVersion,
            InstallType = AesirGetStartedService.AesirInstallType.Upm
        };

        static AesirGetStartedService.AesirSampleInfo CreateSample(
            AesirGetStartedService.AesirPackageInfo pkg,
            string displayName = SampleName,
            string description = "MVC 快捷写法计数器示例。") =>
            new AesirGetStartedService.AesirSampleInfo
            {
                Package = pkg,
                DisplayName = displayName,
                Description = description,
                RelativeDir = "Counter-Mvc-Quick"
            };

        static AesirGetStartedService.UpmSampleHandle Handle(
            string displayName,
            bool isImported,
            Func<bool> import) =>
            new AesirGetStartedService.UpmSampleHandle
            {
                DisplayName = displayName,
                IsImported = isImported,
                Import = import
            };

        /// <summary>总是返回失败的导入委托（未调用即失败用例的默认占位）。</summary>
        static Func<bool> FailingImport() => () => false;

        #endregion

        #region 文案构造

        [Test]
        public void BuildNotImportedToastMessage_ContainsGuidanceAndSampleName()
        {
            var text = AesirGetStartedService.BuildNotImportedToastMessage(CreateSample(CreateUpmPackage()));

            Assert.AreEqual($"请点击右侧「导入 Sample」按钮导入 {SampleName} Sample 案例", text);
        }

        [Test]
        public void BuildImportConfirmMessage_ContainsPackageSampleDescriptionAndPath()
        {
            var sample = CreateSample(CreateUpmPackage());

            var text = AesirGetStartedService.BuildImportConfirmMessage(sample);

            StringAssert.Contains($"是否导入「{PackageDisplayName}」的「{SampleName}」示例？", text);
            StringAssert.Contains(sample.Description, text);
            StringAssert.Contains(AesirGetStartedService.GetUpmSampleImportPath(sample), text);
            StringAssert.Contains("导入后示例位于：", text);
        }

        [Test]
        public void BuildImportConfirmMessage_WithoutDescription_OmitsEmptySection()
        {
            var sample = CreateSample(CreateUpmPackage(), description: null);

            var text = AesirGetStartedService.BuildImportConfirmMessage(sample);

            StringAssert.DoesNotContain("\n\n\n", text);
            StringAssert.Contains("导入后示例位于：", text);
        }

        [Test]
        public void GetUpmSampleImportPath_MatchesPackageManagerLandingPath()
        {
            var sample = CreateSample(CreateUpmPackage());

            Assert.AreEqual(
                $"Assets/Samples/{PackageDisplayName}/{PackageVersion}/{SampleName}",
                AesirGetStartedService.GetUpmSampleImportPath(sample));
        }

        #endregion

        #region ImportUpmSample 结果矩阵

        [Test]
        public void ImportUpmSample_MatchingSample_ImportsAndReportsPath()
        {
            var sample = CreateSample(CreateUpmPackage());
            var importCalls = 0;

            var result = AesirGetStartedService.ImportUpmSample(sample, out var message,
                (id, version) => new List<AesirGetStartedService.UpmSampleHandle>
                {
                    Handle(SampleName, false, () =>
                    {
                        importCalls++;
                        return true;
                    })
                });

            Assert.AreEqual(AesirGetStartedService.AesirSampleImportResult.Imported, result);
            Assert.AreEqual(1, importCalls);
            StringAssert.Contains(AesirGetStartedService.GetUpmSampleImportPath(sample), message);
        }

        [Test]
        public void ImportUpmSample_AlreadyImported_SkipsImport()
        {
            var sample = CreateSample(CreateUpmPackage());
            var importCalls = 0;

            var result = AesirGetStartedService.ImportUpmSample(sample, out var message,
                (id, version) => new List<AesirGetStartedService.UpmSampleHandle>
                {
                    Handle(SampleName, true, () =>
                    {
                        importCalls++;
                        return true;
                    })
                });

            Assert.AreEqual(AesirGetStartedService.AesirSampleImportResult.Imported, result);
            Assert.AreEqual(0, importCalls);
            StringAssert.Contains("已导入", message);
        }

        [Test]
        public void ImportUpmSample_ImportReturnsFalse_Fails()
        {
            var sample = CreateSample(CreateUpmPackage());

            var result = AesirGetStartedService.ImportUpmSample(sample, out var message,
                (id, version) => new List<AesirGetStartedService.UpmSampleHandle>
                {
                    Handle(SampleName, false, FailingImport())
                });

            Assert.AreEqual(AesirGetStartedService.AesirSampleImportResult.Failed, result);
            StringAssert.Contains("Samples 标签页", message);
        }

        [Test]
        public void ImportUpmSample_DisplayNameMismatch_OrdinalNotFound()
        {
            // 匹配键为 Ordinal 精确比较——大小写或全角差异的显示名不得误命中
            var sample = CreateSample(CreateUpmPackage());

            var result = AesirGetStartedService.ImportUpmSample(sample, out var message,
                (id, version) => new List<AesirGetStartedService.UpmSampleHandle>
                {
                    Handle(SampleName.ToLowerInvariant(), false, FailingImport())
                });

            Assert.AreEqual(AesirGetStartedService.AesirSampleImportResult.NotFound, result);
            StringAssert.Contains("未在 Package Manager 示例清单中找到", message);
        }

        [Test]
        public void ImportUpmSample_EmptyOrNullList_NotFound()
        {
            var sample = CreateSample(CreateUpmPackage());

            // 空清单
            Assert.AreEqual(AesirGetStartedService.AesirSampleImportResult.NotFound,
                AesirGetStartedService.ImportUpmSample(sample, out _, (id, version) =>
                    new List<AesirGetStartedService.UpmSampleHandle>()));

            // finder 返回 null（真实 API 异常形态同样不得抛 NRE）
            Assert.AreEqual(AesirGetStartedService.AesirSampleImportResult.NotFound,
                AesirGetStartedService.ImportUpmSample(sample, out _, (id, version) => null));

            // 清单含 null 条目（跳过而非中断）
            Assert.AreEqual(AesirGetStartedService.AesirSampleImportResult.NotFound,
                AesirGetStartedService.ImportUpmSample(sample, out _, (id, version) =>
                    new List<AesirGetStartedService.UpmSampleHandle> { null }));
        }

        [Test]
        public void ImportUpmSample_FinderReceivesPackageIdAndVersion()
        {
            var sample = CreateSample(CreateUpmPackage());
            string seenId = null;
            string seenVersion = null;

            AesirGetStartedService.ImportUpmSample(sample, out _,
                (id, version) =>
                {
                    seenId = id;
                    seenVersion = version;
                    return new List<AesirGetStartedService.UpmSampleHandle>();
                });

            Assert.AreEqual(PackageId, seenId);
            Assert.AreEqual(PackageVersion, seenVersion);
        }

        [Test]
        public void ImportUpmSample_MissingPackageInfo_Fails()
        {
            var result = AesirGetStartedService.ImportUpmSample(null, out var message,
                (id, version) => new List<AesirGetStartedService.UpmSampleHandle>());

            Assert.AreEqual(AesirGetStartedService.AesirSampleImportResult.Failed, result);
            StringAssert.Contains("示例信息缺失", message);
        }

        #endregion
    }
}
