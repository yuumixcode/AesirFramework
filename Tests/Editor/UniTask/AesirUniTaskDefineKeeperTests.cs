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
    ///     <para>
    ///     决策为纯函数，不触碰 PlayerSettings；装配不变量在三种健康状态下均成立
    ///     （无 UniTask → 宏不存在、适配程序集被 defineConstraints 排除；
    ///     unitypackage / DLL 形态 → 维护器写全局宏、适配程序集参与编译加载；
    ///     UPM 形态（<c>com.cysharp.unitask</c> 在注册表）→ 维护器有意不写全局宏，宏由各程序集自身的
    ///     versionDefines 提供），断言的实质是「宏来源与程序集装配必须一致」。
    ///     </para>
    ///     <para>
    ///     门控一致性守卫：versionDefines 产生的宏**只对声明它的程序集可见**（不会成为全局宏），
    ///     故每个带 <c>AESIR_MODULES_UNITASK</c> 约束的程序集都必须自己声明一份 versionDefines ——
    ///     漏声明会被静默排除出编译管线（用例整体消失、总数不变，极易漏判）。
    ///     </para>
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
            var upmInstalled = IsUpmPackageRegistered();

            if (adapterLoaded)
            {
                Assert.IsTrue(definePresent || upmInstalled,
                    "适配程序集已加载，但 AESIR_MODULES_UNITASK 既无全局宏、com.cysharp.unitask 也不在 UPM 注册表——" +
                    "宏来源不可解释（versionDefines / 宏维护器链路断裂）");
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

        #region 门控一致性（versionDefines 的宏只对声明它的程序集可见）

        /// <summary>
        /// 凡以 <c>AESIR_MODULES_UNITASK</c> 作 <c>defineConstraints</c> 的程序集，必须各自声明
        /// <c>com.cysharp.unitask</c> 的 versionDefines：该宏不会成为全局宏，只对本程序集可见，
        /// 漏声明的程序集会被 Unity 静默排除出编译管线（测试用例整体消失、总数不变）。
        /// </summary>
        [Test]
        public void GatedAsmdefs_DeclareOwnUniTaskVersionDefines()
        {
            var gated = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => new { Path = path, Asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path) })
                .Where(item => item.Asset != null)
                .Select(item => new { item.Path, Asmdef = JsonUtility.FromJson<AsmdefGating>(item.Asset.text) })
                .Where(item => item.Asmdef.defineConstraints != null &&
                               item.Asmdef.defineConstraints.Contains(AesirUniTaskDefineKeeper.DefineName))
                .ToArray();

            Assert.IsNotEmpty(gated,
                $"应至少存在一个以 {AesirUniTaskDefineKeeper.DefineName} 作约束的程序集（适配程序集 / 门控测试程序集）");

            foreach (var item in gated)
            {
                Assert.IsTrue(
                    item.Asmdef.versionDefines != null && item.Asmdef.versionDefines.Any(versionDefine =>
                        versionDefine.name == "com.cysharp.unitask" &&
                        versionDefine.define == AesirUniTaskDefineKeeper.DefineName),
                    $"{System.IO.Path.GetFileName(item.Path)} 以 {AesirUniTaskDefineKeeper.DefineName} 作 defineConstraints，" +
                    "但未声明 com.cysharp.unitask 的 versionDefines——versionDefines 只对声明它的程序集生效" +
                    "（不会成为全局宏），漏声明会被静默排除出编译管线（用例整体消失、总数不变）");
            }
        }

        /// <summary>
        /// 行为侧同款守卫：适配程序集在场 ⟺ 门控 PlayMode 测试程序集在场。
        /// 两者共用同一套门控条件（仅约束来自不同来源），任一侧断裂都会让用例静默消失。
        /// </summary>
        [Test]
        public void GatedPlayModeTestAssembly_LoadedState_MatchesAdapter()
        {
            var adapterLoaded = IsAdapterAssemblyLoaded();
            var testAssemblyLoaded = IsAssemblyLoaded("Runestone.AesirModules.Tests.UniTask");

            Assert.AreEqual(adapterLoaded, testAssemblyLoaded,
                "适配程序集与 UniTask 门控 PlayMode 测试程序集的加载状态必须一致——" +
                "不一致意味着某一侧的门控来源断裂（测试侧缺 versionDefines 时用例会静默消失）");
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

        /// <summary>asmdef JSON 的门控子集（<c>defineConstraints</c> + <c>versionDefines</c>）。</summary>
        [System.Serializable]
        class AsmdefGating
        {
            public string[] defineConstraints;
            public VersionDefine[] versionDefines;
        }

        /// <summary>asmdef JSON 的单条 <c>versionDefines</c> 项。</summary>
        [System.Serializable]
        class VersionDefine
        {
            public string name;
            public string expression;
            public string define;
        }

        /// <summary>指定名称的程序集是否已加载进当前域。</summary>
        static bool IsAssemblyLoaded(string assemblyName)
        {
            return System.AppDomain.CurrentDomain.GetAssemblies()
                .Any(assembly => assembly.GetName().Name == assemblyName);
        }

        /// <summary>适配程序集（Runestone.AesirModules.UniTask）是否已加载进当前域。</summary>
        static bool IsAdapterAssemblyLoaded() => IsAssemblyLoaded("Runestone.AesirModules.UniTask");

        /// <summary>
        /// UPM 注册表中是否存在 <c>com.cysharp.unitask</c>——UPM 形态由各程序集的 versionDefines 提供宏、
        /// 维护器有意不写全局宏，故全局宏缺席在此形态下属预期。
        /// </summary>
        static bool IsUpmPackageRegistered()
        {
            return UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages()
                .Any(package => package.name == "com.cysharp.unitask");
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
