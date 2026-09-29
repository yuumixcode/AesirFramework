using System.Text.RegularExpressions;
using NUnit.Framework;
using Runestone.AesirArchitecture.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// ScriptingSymbolEditorUtility 的 EditMode 测试：符号字符串的增删判定（纯逻辑）与查询路径。
    /// <para>
    /// 用例**有意不写入真实宏定义符号**：<c>PlayerSettings.SetScriptingDefineSymbols</c> 会请求
    /// "Define symbols changed" 全量脚本重编译并触发域重载（Editor.log 实证：脚本重编译 →
    /// Reloading assemblies → TestRunner: Unexpected assembly reload happened while running tests），
    /// 使本轮测试在域重载后失去运行上下文。故宏字符串的增删语义经纯函数
    /// （<c>ContainsSymbol</c> / <c>AddSymbol</c> / <c>RemoveSymbol</c>）覆盖，
    /// <see cref="PlayerSettings" /> 侧只做只读断言；真实写入路径属手动验证项。
    /// </para>
    /// </summary>
    public class ScriptingSymbolEditorUtilityTests
    {
        const string TestSymbol = "AESIR_TEST_SYMBOL_XYZ";

        /// <summary>
        /// 读取当前选中构建目标的宏定义符号字符串（只读，用于断言"未发生写入"）。
        /// </summary>
        static string GetSelectedTargetSymbols()
        {
            var target = NamedBuildTarget.FromBuildTargetGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup);
            return PlayerSettings.GetScriptingDefineSymbols(target);
        }

        #region 纯逻辑：ContainsSymbol

        [Test]
        public void ContainsSymbol_ExactEntry_ReturnsTrue()
        {
            Assert.IsTrue(ScriptingSymbolEditorUtility.ContainsSymbol("A;B;C", "B"));
        }

        [Test]
        public void ContainsSymbol_IgnoresSurroundingWhitespace()
        {
            Assert.IsTrue(ScriptingSymbolEditorUtility.ContainsSymbol("A; B ;C", "B"));
        }

        [Test]
        public void ContainsSymbol_PartialEntry_ReturnsFalse()
        {
            // 必须整项相等："AESIR_TEST" 不是 "AESIR_TEST_SYMBOL_XYZ"，反之亦然
            Assert.IsFalse(ScriptingSymbolEditorUtility.ContainsSymbol("AESIR_TEST", TestSymbol));
            Assert.IsFalse(ScriptingSymbolEditorUtility.ContainsSymbol(TestSymbol, "AESIR_TEST"));
        }

        [Test]
        public void ContainsSymbol_NullOrEmptySource_ReturnsFalse()
        {
            Assert.IsFalse(ScriptingSymbolEditorUtility.ContainsSymbol(null, TestSymbol));
            Assert.IsFalse(ScriptingSymbolEditorUtility.ContainsSymbol("", TestSymbol));
        }

        #endregion

        #region 纯逻辑：AddSymbol

        [Test]
        public void AddSymbol_EmptyOrNullSource_ReturnsSymbolItself()
        {
            Assert.AreEqual(TestSymbol, ScriptingSymbolEditorUtility.AddSymbol("", TestSymbol));
            Assert.AreEqual(TestSymbol, ScriptingSymbolEditorUtility.AddSymbol(null, TestSymbol));
        }

        [Test]
        public void AddSymbol_AppendsAtEnd_KeepingExistingOrder()
        {
            Assert.AreEqual("A;B;" + TestSymbol, ScriptingSymbolEditorUtility.AddSymbol("A;B", TestSymbol));
        }

        [Test]
        public void AddSymbol_AlreadyPresent_ReturnsSameReference()
        {
            // 返回同一引用是"无需写回 PlayerSettings"的判据（调用方按引用/值相等跳过写入）
            const string symbols = "A;B";
            Assert.AreSame(symbols, ScriptingSymbolEditorUtility.AddSymbol(symbols, "A"),
                "符号已存在时不应产生新字符串，否则会触发一次多余的宏写回");
        }

        [Test]
        public void AddSymbol_WhitespaceVariantPresent_IsIdempotent()
        {
            const string symbols = "A; B";
            Assert.AreSame(symbols, ScriptingSymbolEditorUtility.AddSymbol(symbols, "B"));
        }

        [Test]
        public void AddSymbol_RepeatedCalls_DoNotDuplicateEntry()
        {
            var once = ScriptingSymbolEditorUtility.AddSymbol("A", TestSymbol);
            var twice = ScriptingSymbolEditorUtility.AddSymbol(once, TestSymbol);
            Assert.AreEqual(once, twice, "重复添加不应产生重复的分号分隔条目");
        }

        #endregion

        #region 纯逻辑：RemoveSymbol

        [Test]
        public void RemoveSymbol_MiddleEntry_KeepsRemainingOrder()
        {
            Assert.AreEqual("A;C", ScriptingSymbolEditorUtility.RemoveSymbol("A;B;C", "B"));
        }

        [Test]
        public void RemoveSymbol_TrimsWhitespaceOfRemainingEntries()
        {
            Assert.AreEqual("A;C", ScriptingSymbolEditorUtility.RemoveSymbol("A ; B ;C", "B"));
        }

        [Test]
        public void RemoveSymbol_OnlyEntry_ReturnsEmptyString()
        {
            Assert.AreEqual(string.Empty, ScriptingSymbolEditorUtility.RemoveSymbol(TestSymbol, TestSymbol));
        }

        [Test]
        public void RemoveSymbol_Absent_ReturnsSameReference()
        {
            const string symbols = "A;B";
            Assert.AreSame(symbols, ScriptingSymbolEditorUtility.RemoveSymbol(symbols, TestSymbol));
        }

        [Test]
        public void AddThenRemove_RestoresOriginalSymbols()
        {
            const string symbols = "A;B";
            var added = ScriptingSymbolEditorUtility.AddSymbol(symbols, TestSymbol);
            Assert.AreEqual(symbols, ScriptingSymbolEditorUtility.RemoveSymbol(added, TestSymbol));
        }

        #endregion

        #region 查询路径（只读，不写宏）

        [Test]
        public void HasScriptingDefineSymbol_MatchesSelectedTargetEntries()
        {
            var symbols = GetSelectedTargetSymbols();
            Assert.IsNotEmpty(symbols, "当前构建目标应至少含一个宏定义符号（AESIR_ARCHITECTURE 由 EnsureAesirArchitectureDefine 确保）");

            var first = symbols.Split(';')[0].Trim();
            Assert.IsTrue(ScriptingSymbolEditorUtility.HasScriptingDefineSymbol(first),
                $"PlayerSettings 中已存在的符号「{first}」应被查询命中");
        }

        [Test]
        public void HasScriptingDefineSymbol_UnknownSymbol_ReturnsFalse()
        {
            Assert.IsFalse(ScriptingSymbolEditorUtility.HasScriptingDefineSymbol(TestSymbol));
        }

        [Test]
        public void HasScriptingDefineSymbol_EmptySymbol_ReturnsFalse()
        {
            Assert.IsFalse(ScriptingSymbolEditorUtility.HasScriptingDefineSymbol(""));
        }

        [Test]
        public void EmptyOrNullSymbol_IsIgnored_WithoutTouchingSettings()
        {
            var before = GetSelectedTargetSymbols();

            LogAssert.Expect(LogType.Warning, new Regex("symbol 不能为空"));
            LogAssert.Expect(LogType.Warning, new Regex("symbol 不能为空"));
            LogAssert.Expect(LogType.Warning, new Regex("symbol 不能为空"));
            LogAssert.Expect(LogType.Warning, new Regex("symbol 不能为空"));
            Assert.DoesNotThrow(() =>
            {
                ScriptingSymbolEditorUtility.EnsureScriptingDefineSymbol("");
                ScriptingSymbolEditorUtility.RemoveScriptingDefineSymbol("");
                ScriptingSymbolEditorUtility.EnsureScriptingDefineSymbol(null);
                ScriptingSymbolEditorUtility.RemoveScriptingDefineSymbol(null);
            });

            Assert.AreEqual(before, GetSelectedTargetSymbols(),
                "空 / null 符号调用应早退，不写入 PlayerSettings（否则触发全量重编译与域重载）");
        }

        #endregion
    }
}
