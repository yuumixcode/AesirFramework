using NUnit.Framework;
using Runestone.AesirArchitecture.Editor;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="AesirSamplesBuildFilter" /> 的剔除规则：
    /// 开发仓库 / unitypackage 导入（<c>Assets/Runestone/&lt;包&gt;/Samples/</c>，安装根经
    /// <see cref="AesirAssetPaths" /> 锚点定位、可移动到项目任意文件夹）与
    /// Package Manager 导入（<c>Assets/Samples/Aesir Architecture|Modules/&lt;版本&gt;/</c>）
    /// 两类形态的示例场景匹配，非示例路径不误伤，混合列表过滤保持保留序。
    /// </summary>
    /// <remarks>
    /// 构建入口回调（RegisterBuildPlayerHandler 链路与日志输出）涉及真实构建流程，不在单测范围，
    /// 由编辑器内手动验证；本类只锁定路径匹配与列表过滤的纯逻辑。
    /// </remarks>
    public class AesirSamplesBuildFilterTests
    {
        #region IsSampleScene — 命中

        [Test]
        public void IsSampleScene_DevelopmentRepoPaths()
        {
            // 开发仓库形态：Assets/Runestone/<包>/Samples/<示例>/…（RAA / RAM 各一）
            Assert.IsTrue(AesirSamplesBuildFilter.IsSampleScene(
                "Assets/Runestone/AesirArchitecture/Samples/Counter-Mvc-Quick/Scene/SampleForCounterMvcQuick.unity"));
            Assert.IsTrue(AesirSamplesBuildFilter.IsSampleScene(
                "Assets/Runestone/AesirModules/Samples/Events/02_Filters/Scene/SampleForEventFilters.unity"));
            Assert.IsTrue(AesirSamplesBuildFilter.IsSampleScene(
                "Assets/Runestone/AesirArchitecture/Samples/PlaneWar/Scene/SampleForPlaneWarMono.unity"));
        }

        [Test]
        public void IsSampleScene_PackageManagerImportedPaths()
        {
            // Package Manager 导入形态：Assets/Samples/<包 displayName>/<版本>/<示例>/…（版本无关）
            Assert.IsTrue(AesirSamplesBuildFilter.IsSampleScene(
                "Assets/Samples/Aesir Architecture/0.23.0/Counter-Mvc-Quick/Scene/SampleForCounterMvcQuick.unity"));
            Assert.IsTrue(AesirSamplesBuildFilter.IsSampleScene(
                "Assets/Samples/Aesir Modules/0.22.0/Audio/01_BasicUsage/Scene/SampleForAudioBasic.unity"));
        }

        [Test]
        public void IsSampleScene_CaseInsensitive()
        {
            // 路径大小写统一按不敏感匹配，避免大小写变体场景漏剔除
            Assert.IsTrue(AesirSamplesBuildFilter.IsSampleScene(
                "assets/runestone/aesirarchitecture/samples/counter-mvc-quick/scene/sample.unity"));
            Assert.IsTrue(AesirSamplesBuildFilter.IsSampleScene(
                "Assets/Samples/AESIR ARCHITECTURE/0.23.0/Demo/scene.unity"));
        }

        #endregion

        #region IsSampleScene — 不误伤

        [Test]
        public void IsSampleScene_NonSamplePaths()
        {
            // 项目根场景、包内非 Samples 目录：不匹配
            Assert.IsFalse(AesirSamplesBuildFilter.IsSampleScene("Assets/Scenes/SampleScene.unity"));
            Assert.IsFalse(
                AesirSamplesBuildFilter.IsSampleScene("Assets/Runestone/AesirArchitecture/Runtime/x.unity"));
            Assert.IsFalse(
                AesirSamplesBuildFilter.IsSampleScene("Assets/Runestone/AesirModules/Editor/x.unity"));
            // 用户的 Assets/Samples/ 下其他包导入：不受影响
            Assert.IsFalse(AesirSamplesBuildFilter.IsSampleScene(
                "Assets/Samples/Some Other Package/1.0/Demo/Demo.unity"));
            // 前缀带斜杠收尾：不误匹配 “Aesir ArchitectureX” 之类目录名
            Assert.IsFalse(AesirSamplesBuildFilter.IsSampleScene(
                "Assets/Samples/Aesir Architecture Extra/1.0/Demo.unity"));
            // 名为 MySamples 等近似目录：不匹配
            Assert.IsFalse(AesirSamplesBuildFilter.IsSampleScene("Assets/MySamples/scene.unity"));
            Assert.IsFalse(
                AesirSamplesBuildFilter.IsSampleScene(
                    "Assets/Runestone/AesirArchitecture/MySamples/s.unity"));
        }

        [Test]
        public void IsSampleScene_EmptyPaths()
        {
            Assert.IsFalse(AesirSamplesBuildFilter.IsSampleScene(null));
            Assert.IsFalse(AesirSamplesBuildFilter.IsSampleScene(string.Empty));
        }

        #endregion

        #region IsSampleScene — Runestone 移动后（注入定位根）

        [Test]
        public void IsSampleScene_MovedInstallRoot_MatchesSamplesUnderIt()
        {
            // Runestone 移动到项目任意文件夹：注入锚点定位到的安装根，示例场景照常剔除
            var roots = new[] { "Assets/MyCompany/Runestone" };
            Assert.IsTrue(AesirSamplesBuildFilter.IsSampleScene(
                "Assets/MyCompany/Runestone/AesirArchitecture/Samples/Counter-Mvc-Quick/Scene/SampleForCounterMvcQuick.unity",
                roots));
            Assert.IsTrue(AesirSamplesBuildFilter.IsSampleScene(
                "Assets/MyCompany/Runestone/AesirModules/Samples/Events/02_Filters/Scene/SampleForEventFilters.unity",
                roots));

            // 移动后：非 Samples 目录不误伤；未注入的默认位置路径不命中
            Assert.IsFalse(AesirSamplesBuildFilter.IsSampleScene(
                "Assets/MyCompany/Runestone/AesirArchitecture/Runtime/x.unity", roots));
            Assert.IsFalse(AesirSamplesBuildFilter.IsSampleScene(
                "Assets/Runestone/AesirArchitecture/Samples/x/s.unity", roots));
        }

        [Test]
        public void IsSampleScene_MultipleRoots_PmPathsStillMatch()
        {
            // 多安装根（部分移动场景）下各自命中；PM 导入路径恒命中（Unity 固定路径）
            var roots = new[] { "Assets/Runestone", "Assets/Custom/RS" };
            Assert.IsTrue(AesirSamplesBuildFilter.IsSampleScene(
                "Assets/Runestone/AesirArchitecture/Samples/x/s.unity", roots));
            Assert.IsTrue(AesirSamplesBuildFilter.IsSampleScene(
                "Assets/Custom/RS/AesirModules/Samples/y/s.unity", roots));
            Assert.IsTrue(AesirSamplesBuildFilter.IsSampleScene(
                "Assets/Samples/Aesir Architecture/0.23.0/Demo/scene.unity", roots));
        }

        [Test]
        public void IsSampleScene_NullRootList_IsSafe()
        {
            Assert.IsFalse(AesirSamplesBuildFilter.IsSampleScene(
                "Assets/Runestone/AesirArchitecture/Samples/x/s.unity", null));
        }

        #endregion

        #region FilterSampleScenes

        [Test]
        public void FilterSampleScenes_MixedListKeepsOrder()
        {
            var scenes = new[]
            {
                "Assets/Scenes/Game.unity",
                "Assets/Runestone/AesirArchitecture/Samples/MiniEvent/Scene/MiniEventSample.unity",
                "Assets/Scenes/Menu.unity",
                "Assets/Samples/Aesir Modules/0.23.0/Events/01_KeyPress/scene.unity"
            };

            var kept = AesirSamplesBuildFilter.FilterSampleScenes(scenes, out var removed);

            // 非示例场景全部保留且顺序不变
            CollectionAssert.AreEqual(new[] { "Assets/Scenes/Game.unity", "Assets/Scenes/Menu.unity" }, kept);
            // 被剔除的示例场景按出现顺序记录
            CollectionAssert.AreEqual(new[]
            {
                "Assets/Runestone/AesirArchitecture/Samples/MiniEvent/Scene/MiniEventSample.unity",
                "Assets/Samples/Aesir Modules/0.23.0/Events/01_KeyPress/scene.unity"
            }, removed);
        }

        [Test]
        public void FilterSampleScenes_AllSamples()
        {
            // 全为示例场景：结果为空列表、明细完整（空场景列表是否可构建由构建流程自身报错）
            var kept = AesirSamplesBuildFilter.FilterSampleScenes(new[]
            {
                "Assets/Runestone/AesirArchitecture/Samples/MiniEvent/Scene/MiniEventSample.unity",
                "Assets/Samples/Aesir Architecture/0.23.0/PlaneWar/Scene/SampleForPlaneWarMono.unity"
            }, out var removed);

            Assert.AreEqual(0, kept.Length);
            Assert.AreEqual(2, removed.Count);
        }

        [Test]
        public void FilterSampleScenes_NoneSamples()
        {
            // 无示例场景：原样保留、明细为空
            var kept = AesirSamplesBuildFilter.FilterSampleScenes(new[]
            {
                "Assets/Scenes/Game.unity",
                "Assets/Scenes/Menu.unity"
            }, out var removed);

            CollectionAssert.AreEqual(new[] { "Assets/Scenes/Game.unity", "Assets/Scenes/Menu.unity" }, kept);
            Assert.AreEqual(0, removed.Count);
        }

        #endregion
    }
}
