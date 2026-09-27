using System.Linq;
using UnityEngine;
using NUnit.Framework;
using Runestone.AesirModules.Editor;
using UnityEditor;
using UnityEditor.Build;

namespace Runestone.AesirModules.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="AesirUniTaskDefineKeeper" />：宏同步决策矩阵（UPM 交由 versionDefines 不干预 /
    /// 程序集在场补宏 / 不在场移除）、适配程序集装配不变量
    /// （Runestone.AesirModules.UniTask 程序集是否加载 ⟺ AESIR_MODULES_UNITASK 存在）
    /// 与命名守卫（检测白名单与 asmdef 引用必须命中 UniTask 的真实程序集名——
    /// UniTask 的命名空间名 <c>Cysharp.Threading.Tasks</c> 不是程序集名，二者混用曾致检测恒空与 CS0246）。
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
                .Any(assembly => AesirUniTaskDefineKeeper.UniTaskAssemblyNames
                    .Contains(assembly.GetName().Name));
            Assert.AreEqual(expected, AesirUniTaskDefineKeeper.IsUniTaskAssemblyLoaded(),
                "程序集检测应与域内实际状态一致（白名单任一命中）");
        }

        #endregion

        #region 命名守卫（锁「命名空间名误当程序集名」的回归）

        [Test]
        public void UniTaskAssemblyNames_ContainRealUniTaskAssemblyName()
        {
            Assert.Contains("UniTask", AesirUniTaskDefineKeeper.UniTaskAssemblyNames,
                "检测白名单必须含 com.cysharp.unitask 包内 UniTask.asmdef 的真实程序集名 UniTask——" +
                "命名空间名 Cysharp.Threading.Tasks 不是程序集名，仅按它检测会让 unitypackage / DLL " +
                "安装形态下的已装工程被误删 AESIR_MODULES_UNITASK 宏、集成静默失效");
        }

        [Test]
        public void CoreAsmdefReferences_UseRealUniTaskAssemblyName()
        {
            foreach (var asmdefName in new[] { "Runestone.AesirModules", "Runestone.AesirModules.UniTask" })
            {
                var asset = LoadAsmdefAsset(asmdefName);
                Assert.IsNotNull(asset, $"未按文件名定位到 {asmdefName}.asmdef");
                var references = JsonUtility.FromJson<AsmdefReferences>(asset.text).references;
                Assert.Contains("UniTask", references,
                    $"{asmdefName}.asmdef 的 references 必须按程序集名引用 UniTask（com.cysharp.unitask 包内 UniTask.asmdef）");
                Assert.IsFalse(references.Contains("Cysharp.Threading.Tasks"),
                    $"{asmdefName}.asmdef 不得以命名空间名 Cysharp.Threading.Tasks 作程序集引用——" +
                    "该名字解析不到任何程序集，含 UniTask 的消费工程刷新即报 CS0246（Cysharp / UniTaskVoid 找不到）");
            }
        }

        #endregion

        /// <summary>
        /// 按文件名经 AssetDatabase 定位程序集定义资产（Assets / UPM 安装形态自适应）；
        /// asmdef 文本经 <see cref="TextAsset" /> 基类加载（派生的 AssemblyDefinitionAsset 在引擎内部命名空间，勿直接引用）。
        /// </summary>
        static TextAsset LoadAsmdefAsset(string asmdefName)
        {
            return AssetDatabase.FindAssets("t:AssemblyDefinitionAsset")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => System.IO.Path.GetFileNameWithoutExtension(path) == asmdefName)
                .Select(path => AssetDatabase.LoadAssetAtPath<TextAsset>(path))
                .FirstOrDefault(asset => asset != null);
        }

        /// <summary>asmdef JSON 的引用列表子集（供 <see cref="JsonUtility" /> 解析）。</summary>
        [System.Serializable]
        class AsmdefReferences
        {
            public string[] references;
        }

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
