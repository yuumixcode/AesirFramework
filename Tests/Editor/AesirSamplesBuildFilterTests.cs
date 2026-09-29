using System.Reflection;
using NUnit.Framework;
using Runestone.AesirArchitecture.Editor;
using UnityEditor;

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
    /// 覆盖范围：路径匹配 / 列表过滤的纯逻辑 + 「构建入口钩子已挂载」的注册断言（命门，钩子没挂上示例场景就会进包）。
    /// 真实构建流程（钩子内转交 <c>DefaultBuildMethods.BuildPlayer</c> 与日志输出）不在单测范围，由编辑器内手动验证。
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

        [Test]
        public void FilterSampleScenes_InjectableRoots_FiltersChainAfterMovedInstallRoot()
        {
            // 可注入重载：把整条过滤链放到"移动后的安装根"上验证——
            // 生产重载读真实安装根（本仓为 Assets/Runestone），移动形态只能经此重载覆盖
            var movedRoots = new[] { "Assets/ThirdParty/Runestone" };
            var scenes = new[]
            {
                "Assets/Scenes/Game.unity",
                "Assets/ThirdParty/Runestone/AesirArchitecture/Samples/MiniEvent/Scene/MiniEventSample.unity",
                "Assets/Runestone/AesirArchitecture/Samples/MiniEvent/Scene/MiniEventSample.unity",
                // PM 导入形态不受安装根移动影响，仍应被剔除
                "Assets/Samples/Aesir Modules/0.30.0/Events/01_KeyPress/scene.unity"
            };

            var kept = AesirSamplesBuildFilter.FilterSampleScenes(scenes, movedRoots, out var removed);

            CollectionAssert.AreEqual(new[] { "Assets/Scenes/Game.unity",
                "Assets/Runestone/AesirArchitecture/Samples/MiniEvent/Scene/MiniEventSample.unity" }, kept);
            CollectionAssert.AreEqual(new[]
            {
                "Assets/ThirdParty/Runestone/AesirArchitecture/Samples/MiniEvent/Scene/MiniEventSample.unity",
                "Assets/Samples/Aesir Modules/0.30.0/Events/01_KeyPress/scene.unity"
            }, removed);
        }

        [Test]
        public void OnBuildPlayer_HookIsRegisteredOnBuildPlayerWindow()
        {
            // 构建剔除的命门是钩子确实挂上了 Build Settings 窗口的构建入口
            // （RegisterBuildPlayerHandler 是覆盖式注册，域加载后由 [InitializeOnLoadMethod] 重注册）
            var field = FindHandlerField();
            // Unity 版本更名会让该私有字段找不到：降级为跳过而非误报失败
            Assume.That(field, Is.Not.Null, "未找到 BuildPlayerWindow 的构建入口处理委托字段");

            var previous = field.GetValue(null);
            try
            {
                field.SetValue(null, null);
                InvokeInitializeOnLoadMethods();

                Assert.IsNotNull(field.GetValue(null),
                    "域加载期应把构建入口钩子挂到 Build Settings 窗口（否则示例场景会进玩家构建）");
            }
            finally
            {
                field.SetValue(null, previous);
            }
        }

        /// <summary>按候选名查找 <see cref="BuildPlayerWindow" /> 的构建入口处理委托字段（跨版本更名容错）。</summary>
        static FieldInfo FindHandlerField()
        {
            foreach (var name in new[] { "buildPlayerHandler", "s_BuildPlayerHandler", "m_BuildPlayerHandler" })
            {
                var field = typeof(BuildPlayerWindow).GetField(name, BindingFlags.NonPublic | BindingFlags.Static);
                if (field != null)
                {
                    return field;
                }
            }

            return null;
        }

        /// <summary>
        /// 手动触发 <see cref="AesirSamplesBuildFilter" /> 的 <c>[InitializeOnLoadMethod]</c>（域已加载过，不会自动重跑）。
        /// </summary>
        static void InvokeInitializeOnLoadMethods()
        {
            var methods = typeof(AesirSamplesBuildFilter).GetMethods(
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<InitializeOnLoadMethodAttribute>() != null)
                {
                    method.Invoke(null, null);
                }
            }
        }

        #endregion
    }
}
