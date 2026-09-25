using System.Linq;
using NUnit.Framework;
using Runestone.AesirArchitecture.Editor;
using UnityEditor;
using UnityEditor.Build;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// ScriptingSymbolEditorUtility 的 EditMode 测试：Ensure/Has/Remove 往返、幂等性与空符号忽略。
    /// 测试符号在 TearDown 强制清理，不污染 ProjectSettings。
    /// </summary>
    public class ScriptingSymbolEditorUtilityTests
    {
        const string TestSymbol = "AESIR_TEST_SYMBOL_XYZ";

        [TearDown]
        public void TearDown()
        {
            // 兜底清理：即使断言中途失败也不让测试符号残留到 ProjectSettings
            ScriptingSymbolEditorUtility.RemoveScriptingDefineSymbol(TestSymbol);
        }

        [Test]
        public void EnsureThenHasThenRemoveThenNotHas()
        {
            ScriptingSymbolEditorUtility.RemoveScriptingDefineSymbol(TestSymbol);
            Assert.IsFalse(ScriptingSymbolEditorUtility.HasScriptingDefineSymbol(TestSymbol));

            ScriptingSymbolEditorUtility.EnsureScriptingDefineSymbol(TestSymbol);
            Assert.IsTrue(ScriptingSymbolEditorUtility.HasScriptingDefineSymbol(TestSymbol));

            ScriptingSymbolEditorUtility.RemoveScriptingDefineSymbol(TestSymbol);
            Assert.IsFalse(ScriptingSymbolEditorUtility.HasScriptingDefineSymbol(TestSymbol));
        }

        [Test]
        public void Ensure_IsIdempotent_NoDuplicateEntries()
        {
            ScriptingSymbolEditorUtility.EnsureScriptingDefineSymbol(TestSymbol);
            ScriptingSymbolEditorUtility.EnsureScriptingDefineSymbol(TestSymbol);

            var target = NamedBuildTarget.FromBuildTargetGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup);
            var symbols = PlayerSettings.GetScriptingDefineSymbols(target);
            var count = symbols.Split(';').Count(s => s.Trim() == TestSymbol);
            Assert.AreEqual(1, count, "重复 Ensure 不应产生重复的分号分隔条目");
        }

        [Test]
        public void EmptySymbol_IsIgnored()
        {
            Assert.DoesNotThrow(() =>
            {
                ScriptingSymbolEditorUtility.EnsureScriptingDefineSymbol("");
                ScriptingSymbolEditorUtility.RemoveScriptingDefineSymbol("");
            });
            Assert.IsFalse(ScriptingSymbolEditorUtility.HasScriptingDefineSymbol(""));
        }
    }
}
