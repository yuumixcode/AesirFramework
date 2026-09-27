using System.Linq;
using NUnit.Framework;
using Runestone.AesirModules.Editor;
using UnityEditor;
using UnityEditor.Build;

namespace Runestone.AesirModules.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="AesirUniTaskDefineKeeper" />：宏同步决策矩阵（UPM 交由 versionDefines 不干预 /
    /// 程序集在场补宏 / 不在场移除）与适配程序集装配不变量
    /// （Runestone.AesirModules.UniTask 程序集是否加载 ⟺ AESIR_MODULES_UNITASK 存在）。
    /// </summary>
    /// <remarks>
    /// 决策为纯函数，不触碰 PlayerSettings；装配不变量在两种健康状态下均成立
    /// （本仓库无 UniTask → 宏不存在、适配程序集被 defineConstraints 排除；
    /// 任意装了 UniTask 的工程 → 宏存在、适配程序集参与编译加载），
    /// 断言的实质是「宏与程序集装配必须一致」，defineConstraints / versionDefines / 维护器任一环节断裂即红。
    /// </remarks>
    public class AesirUniTaskDefineKeeperTests
    {
        #region 决策矩阵

        [Test]
        public void DecideDefineAction_UpmInstalled_AlwaysDeferToVersionDefines()
        {
            Assert.IsNull(AesirUniTaskDefineKeeper.DecideDefineAction(true, true),
                "UPM 安装形态由 versionDefines 全权管理，程序集在场也不得写全局宏");
            Assert.IsNull(AesirUniTaskDefineKeeper.DecideDefineAction(true, false),
                "UPM 安装形态由 versionDefines 全权管理，程序集不在场也不得清全局宏");
        }

        [Test]
        public void DecideDefineAction_AssemblyLoadedWithoutUpm_EnsuresDefine()
        {
            Assert.IsTrue(AesirUniTaskDefineKeeper.DecideDefineAction(false, true),
                "非 UPM 形态（unitypackage / DLL）且 UniTask 程序集在场 → 确保全局宏存在");
        }

        [Test]
        public void DecideDefineAction_AssemblyAbsentWithoutUpm_RemovesDefine()
        {
            Assert.IsFalse(AesirUniTaskDefineKeeper.DecideDefineAction(false, false),
                "非 UPM 形态且 UniTask 程序集不在场 → 确保全局宏移除（避免陈旧宏冻结编译）");
        }

        #endregion

        #region 装配不变量

        [Test]
        public void UniTaskAdapterAssembly_LoadedState_MatchesDefinePresence()
        {
            var adapterLoaded = IsAdapterAssemblyLoaded();
            var definePresent = IsDefinePresent();

            if (adapterLoaded)
            {
                Assert.IsTrue(definePresent,
                    "适配程序集已加载但 AESIR_MODULES_UNITASK 缺失——defineConstraints 门控失效");
            }
            else
            {
                Assert.IsFalse(definePresent,
                    "AESIR_MODULES_UNITASK 存在但适配程序集未加载——versionDefines / 宏维护器链路断裂");
            }
        }

        [Test]
        public void IsUniTaskAssemblyLoaded_MatchesDomainState()
        {
            var expected = System.AppDomain.CurrentDomain.GetAssemblies()
                .Any(assembly => assembly.GetName().Name == AesirUniTaskDefineKeeper.UniTaskAssemblyName);
            Assert.AreEqual(expected, AesirUniTaskDefineKeeper.IsUniTaskAssemblyLoaded(),
                "程序集检测应与域内实际状态一致");
        }

        #endregion

        /// <summary>适配程序集（Runestone.AesirModules.UniTask）是否已加载进当前域。</summary>
        static bool IsAdapterAssemblyLoaded()
        {
            return System.AppDomain.CurrentDomain.GetAssemblies()
                .Any(assembly => assembly.GetName().Name == "Runestone.AesirModules.UniTask");
        }

        /// <summary>当前选中构建目标的宏列表是否包含 AESIR_MODULES_UNITASK。</summary>
        static bool IsDefinePresent()
        {
            var target = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            return PlayerSettings.GetScriptingDefineSymbols(target)
                .Split(';')
                .Any(symbol => symbol.Trim() == AesirUniTaskDefineKeeper.DefineName);
        }
    }
}
