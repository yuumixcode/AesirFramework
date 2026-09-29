using System.Text.RegularExpressions;
using NUnit.Framework;
using Runestone.AesirModules.ScriptDocGenerator.Editor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    public class XmlSummaryToolTests
    {
        const string TypeSummaryCode = @"using System;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>
    /// 测试类级别（包括结构体，接口等）的 Summary，
    /// 以 class 为例
    /// </summary>
    [Serializable]
    public class TestClassSummary { }
}
";

        const string SpecialCharsCode = @"using System;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>
    /// 成员 "" Summary 注释 ????
    /// &lt;para&gt;aaa&lt;/para&gt;
    /// <para>aaa</para>
    /// </summary>
    /// <remarks>AAAAA</remarks>>
    [Obsolete(""临时方法"")] public struct TestStructSummary { }
}
";

        const string MethodSummaryCode = @"using System;
using UnityEngine;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    public class TestMemberSummary : MonoBehaviour
    {
        // 两个 // 的简单注释
        /// <summary>
        /// AAA
        /// </summary>
        /// <param name=""filePath"">以 Assets 开头的相对路径即可</param>
        [Obsolete(""临时方法"")] public static void MethodA(string filePath)
        {
            // 方法体
            Debug.Log(""测试成员Summary注释"");
        }
    }
}
";

        const string MultiLineAttrCode = @"using System;
using UnityEngine;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>
    /// 测试移除 ChineseSummary
    /// </summary>
    [Obsolete(""临时方法"")]
    [Summary(""测试"" +
             ""移除多行的"" +
             "" ChineseSummary"")]
    public class TestRemoveSummaryB
    {
        /// <summary>
        /// BBB
        /// </summary>
        [Obsolete(""临时方法"")] [Summary(""AAA"")] public void Method()
        {
            Debug.Log(""测试移除多行的 ChineseSummary"");
        }
    }
}
";

        const string NoSummaryCode = @"using System;
public class NoSummaryClass { }";

        const string StringLiteralCode = @"using System;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>
    /// 包含字符串常量的测试类
    /// </summary>
    [Summary(""真实特性"")]
    public class TestStringLiteral
    {
        public void Method()
        {
            string s = ""这里有一个伪造的特性：[Summary(\""伪造特性\"")]"";
            UnityEngine.Debug.Log(s);
        }
    }
}
";

        const string PreprocessorCode = @"using System;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>
    /// 编辑器工具类
    /// </summary>
    public class TestPreprocessor
    {
        /// <summary>
        /// 编辑器专用方法
        /// </summary>
#if UNITY_EDITOR
        [Summary(""旧内容"")]
        public void EditorMethod() { }
#endif
    }
}
";

        const string SingleLineSummaryCode = @"using System;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>单行 summary 测试</summary>
    [Serializable]
    public class TestSingleLineSummary { }
}
";

        const string MixedSingleMultiLineCode = @"using System;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>类级别单行</summary>
    public class TestMixed
    {
        /// <summary>
        /// 多行 summary
        /// </summary>
        public void MultiLineMethod() { }

        /// <summary>单行方法</summary>
        public void SingleLineMethod() { }
    }
}
";

        static void ProcessAndAssert(string source, XmlSummaryTool.ProcessMode mode, string expected)
        {
            var result = new XmlSummaryTool(source).ParseSourceScript().GetProcessedSourceScript(mode);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TypeLevelSummary_SyncAddsAttribute()
        {
            ProcessAndAssert(TypeSummaryCode, XmlSummaryTool.ProcessMode.SyncSummary,
                @"using Runestone.AesirModules.ScriptDocGenerator;
using System;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>
    /// 测试类级别（包括结构体，接口等）的 Summary，
    /// 以 class 为例
    /// </summary>
    [Summary(""测试类级别（包括结构体，接口等）的 Summary， 以 class 为例"")]
    [Serializable]
    public class TestClassSummary { }
}
");
        }

        [Test]
        public void TypeLevelSummary_ReplaceReplacesTagWithAttribute()
        {
            ProcessAndAssert(TypeSummaryCode, XmlSummaryTool.ProcessMode.ReplaceSummary,
                @"using Runestone.AesirModules.ScriptDocGenerator;
using System;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    [Summary(""测试类级别（包括结构体，接口等）的 Summary， 以 class 为例"")]
    [Serializable]
    public class TestClassSummary { }
}
");
        }

        [Test]
        public void SpecialCharsSummary_SyncHandlesCorrectly()
        {
            // summary 含双引号：生成的特性文本必须转义，否则是非法 C#；
            // XML 实体（&lt;para&gt;）进特性前解码为字面文本（双重转义修复）
            ProcessAndAssert(SpecialCharsCode, XmlSummaryTool.ProcessMode.SyncSummary,
                @"using Runestone.AesirModules.ScriptDocGenerator;
using System;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>
    /// 成员 "" Summary 注释 ????
    /// &lt;para&gt;aaa&lt;/para&gt;
    /// <para>aaa</para>
    /// </summary>
    /// <remarks>AAAAA</remarks>>
    [Summary(""成员 \"" Summary 注释 ???? <para>aaa</para> aaa"")]
    [Obsolete(""临时方法"")] public struct TestStructSummary { }
}
");
        }

        [Test]
        public void SpecialCharsSummary_ReplaceHandlesCorrectly()
        {
            ProcessAndAssert(SpecialCharsCode, XmlSummaryTool.ProcessMode.ReplaceSummary,
                @"using Runestone.AesirModules.ScriptDocGenerator;
using System;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <remarks>AAAAA</remarks>>
    [Summary(""成员 \"" Summary 注释 ???? <para>aaa</para> aaa"")]
    [Obsolete(""临时方法"")] public struct TestStructSummary { }
}
");
        }

        [Test]
        public void MethodSummary_SyncAddsAttribute()
        {
            ProcessAndAssert(MethodSummaryCode, XmlSummaryTool.ProcessMode.SyncSummary,
                @"using Runestone.AesirModules.ScriptDocGenerator;
using System;
using UnityEngine;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    public class TestMemberSummary : MonoBehaviour
    {
        // 两个 // 的简单注释
        /// <summary>
        /// AAA
        /// </summary>
        /// <param name=""filePath"">以 Assets 开头的相对路径即可</param>
        [Summary(""AAA"")]
        [Obsolete(""临时方法"")] public static void MethodA(string filePath)
        {
            // 方法体
            Debug.Log(""测试成员Summary注释"");
        }
    }
}
");
        }

        [Test]
        public void MethodSummary_ReplaceReplacesTagWithAttribute()
        {
            ProcessAndAssert(MethodSummaryCode, XmlSummaryTool.ProcessMode.ReplaceSummary,
                @"using Runestone.AesirModules.ScriptDocGenerator;
using System;
using UnityEngine;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    public class TestMemberSummary : MonoBehaviour
    {
        // 两个 // 的简单注释
        /// <param name=""filePath"">以 Assets 开头的相对路径即可</param>
        [Summary(""AAA"")]
        [Obsolete(""临时方法"")] public static void MethodA(string filePath)
        {
            // 方法体
            Debug.Log(""测试成员Summary注释"");
        }
    }
}
");
        }

        [Test]
        public void ExistingMultiLineAttribute_SyncAttributePriority()
        {
            // 双向对齐（特性优先）：可解析的 [Summary("AAA")] 是权威内容，
            // XML summary 与特性不一致时以特性文本回写 XML；类级的拼接字符串实参特性
            // 无法安全解析，按"无特性"回退 XML 内容重新生成
            ProcessAndAssert(MultiLineAttrCode, XmlSummaryTool.ProcessMode.SyncSummary, @"using System;
using UnityEngine;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>
    /// 测试移除 ChineseSummary
    /// </summary>
    [Summary(""测试移除 ChineseSummary"")]
    [Obsolete(""临时方法"")]
    public class TestRemoveSummaryB
    {
        /// <summary>AAA</summary>
        [Obsolete(""临时方法"")] [Summary(""AAA"")] public void Method()
        {
            Debug.Log(""测试移除多行的 ChineseSummary"");
        }
    }
}
");
        }

        [Test]
        public void ExistingMultiLineAttribute_ReplaceAttributePriority()
        {
            // Replace：内容取特性优先——可解析特性 "AAA" 保持；类级拼接特性回退 XML 文本
            ProcessAndAssert(MultiLineAttrCode, XmlSummaryTool.ProcessMode.ReplaceSummary, @"using System;
using UnityEngine;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    [Summary(""测试移除 ChineseSummary"")]
    [Obsolete(""临时方法"")]
    public class TestRemoveSummaryB
    {
        [Summary(""AAA"")]
        [Obsolete(""临时方法"")] public void Method()
        {
            Debug.Log(""测试移除多行的 ChineseSummary"");
        }
    }
}
");
        }

        [Test]
        public void NoXmlComment_SyncOnlyAddsUsing()
        {
            var result = new XmlSummaryTool(NoSummaryCode).ParseSourceScript()
                .GetProcessedSourceScript(XmlSummaryTool.ProcessMode.SyncSummary);
            Assert.IsTrue(result.Contains("using Runestone.AesirModules.ScriptDocGenerator;"));
            Assert.IsTrue(result.Contains("public class NoSummaryClass { }"));
        }

        [Test]
        public void RemoveAllSummaryAttributes()
        {
            ProcessAndAssert(MultiLineAttrCode, XmlSummaryTool.ProcessMode.RemoveSummary, @"using System;
using UnityEngine;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>
    /// 测试移除 ChineseSummary
    /// </summary>
    [Obsolete(""临时方法"")]
    public class TestRemoveSummaryB
    {
        /// <summary>
        /// BBB
        /// </summary>
        [Obsolete(""临时方法"")] public void Method()
        {
            Debug.Log(""测试移除多行的 ChineseSummary"");
        }
    }
}
");
        }

        [Test]
        public void StringLiteralSummary_NotRemoved()
        {
            var result = new XmlSummaryTool(StringLiteralCode).ParseSourceScript()
                .GetProcessedSourceScript(XmlSummaryTool.ProcessMode.RemoveSummary);
            Assert.IsTrue(result.Contains(@"[Summary(\""伪造特性\"")]"),
                "String literal containing [Summary] should not be removed");
            Assert.IsFalse(result.Contains("[Summary(\"真实特性\")]"),
                "Real [Summary] attribute should be removed");
        }

        [Test]
        public void Preprocessor_SyncAlignsXmlAttributeInsideBlock()
        {
            // 特性优先：可解析的 [Summary("旧内容")] 为权威内容，XML 被对齐回写
            ProcessAndAssert(PreprocessorCode, XmlSummaryTool.ProcessMode.SyncSummary, @"using System;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>
    /// 编辑器工具类
    /// </summary>
    [Summary(""编辑器工具类"")]
    public class TestPreprocessor
    {
        /// <summary>旧内容</summary>
#if UNITY_EDITOR
        [Summary(""旧内容"")]
        public void EditorMethod() { }
#endif
    }
}
");
        }

        [Test]
        public void Preprocessor_ReplaceAttributePriorityInsideBlock()
        {
            // Replace：特性可解析 → 内容取特性（"旧内容"），XML summary 标签移除
            ProcessAndAssert(PreprocessorCode, XmlSummaryTool.ProcessMode.ReplaceSummary, @"using System;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    [Summary(""编辑器工具类"")]
    public class TestPreprocessor
    {
#if UNITY_EDITOR
        [Summary(""旧内容"")]
        public void EditorMethod() { }
#endif
    }
}
");
        }

        [Test]
        public void Preprocessor_RemoveDeletesAttributeInsideBlock()
        {
            ProcessAndAssert(PreprocessorCode, XmlSummaryTool.ProcessMode.RemoveSummary, @"using System;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>
    /// 编辑器工具类
    /// </summary>
    public class TestPreprocessor
    {
        /// <summary>
        /// 编辑器专用方法
        /// </summary>
#if UNITY_EDITOR
        public void EditorMethod() { }
#endif
    }
}
");
        }

        [Test]
        public void SingleLineSummary_SyncAddsAttribute()
        {
            ProcessAndAssert(SingleLineSummaryCode, XmlSummaryTool.ProcessMode.SyncSummary,
                @"using Runestone.AesirModules.ScriptDocGenerator;
using System;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>单行 summary 测试</summary>
    [Summary(""单行 summary 测试"")]
    [Serializable]
    public class TestSingleLineSummary { }
}
");
        }

        [Test]
        public void SingleLineSummary_ReplaceReplacesTagWithAttribute()
        {
            ProcessAndAssert(SingleLineSummaryCode, XmlSummaryTool.ProcessMode.ReplaceSummary,
                @"using Runestone.AesirModules.ScriptDocGenerator;
using System;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    [Summary(""单行 summary 测试"")]
    [Serializable]
    public class TestSingleLineSummary { }
}
");
        }

        [Test]
        public void MixedSingleMultiLine_SyncAddsAttributes()
        {
            ProcessAndAssert(MixedSingleMultiLineCode, XmlSummaryTool.ProcessMode.SyncSummary, @"using System;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>类级别单行</summary>
    [Summary(""类级别单行"")]
    public class TestMixed
    {
        /// <summary>
        /// 多行 summary
        /// </summary>
        [Summary(""多行 summary"")]
        public void MultiLineMethod() { }

        /// <summary>单行方法</summary>
        [Summary(""单行方法"")]
        public void SingleLineMethod() { }
    }
}
");
        }

        [Test]
        public void MixedSingleMultiLine_ReplaceReplacesTagsWithAttributes()
        {
            ProcessAndAssert(MixedSingleMultiLineCode, XmlSummaryTool.ProcessMode.ReplaceSummary,
                @"using System;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    [Summary(""类级别单行"")]
    public class TestMixed
    {
        [Summary(""多行 summary"")]
        public void MultiLineMethod() { }

        [Summary(""单行方法"")]
        public void SingleLineMethod() { }
    }
}
");
        }

        #region Sync 幂等（空白压缩对称）

        [Test]
        public void SyncAttributeWhitespaceDifference_DoesNotRewrite()
        {
            // 特性与 XML 仅空白差异时不应触发回写（非幂等修复：比较前两侧同等压缩）
            const string source = @"using System;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>hello world</summary>
    [Summary(""hello  world"")]
    public class TestWhitespace { }
}
";
            var result = new XmlSummaryTool(source).ParseSourceScript()
                .GetProcessedSourceScript(XmlSummaryTool.ProcessMode.SyncSummary);
            StringAssert.Contains("/// <summary>hello world</summary>", result);
            StringAssert.Contains("[Summary(\"hello  world\")]", result);
        }

        #endregion

        #region Remove 模式 header 区清理

        [Test]
        public void RemoveMode_HeaderAttribute_IsStripped()
        {
            // 首个 XML 注释之前（header 区）的 [Summary] 此前永不被清理
            const string source = @"using System;
using Runestone.AesirModules.ScriptDocGenerator;

[Summary(""头部类的特性"")]
public class HeaderClass { }
";
            var result = new XmlSummaryTool(source).ParseSourceScript()
                .GetProcessedSourceScript(XmlSummaryTool.ProcessMode.RemoveSummary);
            Assert.IsFalse(result.Contains("[Summary("), "header 区的 [Summary] 应被 Remove 模式清理");
            StringAssert.Contains("public class HeaderClass { }", result);
        }

        #endregion

        #region 新语义与修复回归

        const string EscapedQuoteCode = @"using System;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>
    /// 提到 ""字段"" 的注释
    /// </summary>
    public class TestEscapedQuote { }
}
";

        [Test]
        public void QuoteInSummary_GeneratedAttributeIsEscaped()
        {
            var result = new XmlSummaryTool(EscapedQuoteCode).ParseSourceScript()
                .GetProcessedSourceScript(XmlSummaryTool.ProcessMode.SyncSummary);
            // 生成的特性字符串必须可编译：双引号转义为 \"
            StringAssert.Contains("[Summary(\"提到 \\\"字段\\\" 的注释\")]", result);
        }

        const string CrLfSource =
            "using System;\r\n\r\npublic class TestCrLf\r\n{\r\n    /// <summary>CRLF 摘要</summary>\r\n    public int Value { get; set; }\r\n}\r\n";

        [Test]
        public void CrlfSource_LineEndingsPreserved()
        {
            var result = new XmlSummaryTool(CrLfSource).ParseSourceScript()
                .GetProcessedSourceScript(XmlSummaryTool.ProcessMode.SyncSummary);
            StringAssert.Contains("\r\n", result);
            Assert.IsFalse(result.Contains("\n") && result.Replace("\r\n", "").Contains("\n"),
                "输出不应混入 LF 行尾");
        }

        const string QuadrupleSlashCode = @"using System;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    public class TestQuadrupleSlash
    {
        //// 四斜杠是普通注释，不是 XML 文档注释
        public void Method() { }
    }
}
";

        [Test]
        public void QuadrupleSlashComment_NotTreatedAsXmlDoc()
        {
            // //// 前无 /// 文档注释：整个文件无文档注释，分组为空，仅头部注入 using
            var result = new XmlSummaryTool(QuadrupleSlashCode).ParseSourceScript()
                .GetProcessedSourceScript(XmlSummaryTool.ProcessMode.SyncSummary);
            StringAssert.Contains("using Runestone.AesirModules.ScriptDocGenerator;", result);
            StringAssert.Contains("//// 四斜杠是普通注释，不是 XML 文档注释", result);
        }

        [Test]
        public void RemoveMode_NoAttribute_NoRewrite()
        {
            // 无 [Summary] 特性时 Remove 模式应原样返回（不重写行尾/空行）
            var result = new XmlSummaryTool(TypeSummaryCode).ParseSourceScript()
                .GetProcessedSourceScript(XmlSummaryTool.ProcessMode.RemoveSummary);
            Assert.AreEqual(TypeSummaryCode, result);
        }

        [Test]
        public void SyncAttributePriority_AttributeTextWins()
        {
            // 双向对齐：特性文本与 XML 不一致时，特性是权威内容并回写 XML（Sync）
            const string source = @"using System;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>XML 旧文本</summary>
    [Summary(""特性新文本"")]
    public class TestPriority { }
}
";
            var result = new XmlSummaryTool(source).ParseSourceScript()
                .GetProcessedSourceScript(XmlSummaryTool.ProcessMode.SyncSummary);
            StringAssert.Contains("/// <summary>特性新文本</summary>", result);
            StringAssert.Contains("[Summary(\"特性新文本\")]", result);
            Assert.IsFalse(result.Contains("XML 旧文本"), "XML 旧文本应被特性内容对齐覆盖");
        }

        [Test]
        public void ReplaceAttributePriority_AttributeTextPreserved()
        {
            // Replace：已有可解析特性时内容取特性，不再从 XML 重新生成
            const string source = @"using System;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>XML 旧文本</summary>
    [Summary(""特性保留文本"")]
    public class TestPriority2 { }
}
";
            var result = new XmlSummaryTool(source).ParseSourceScript()
                .GetProcessedSourceScript(XmlSummaryTool.ProcessMode.ReplaceSummary);
            StringAssert.Contains("[Summary(\"特性保留文本\")]", result);
            Assert.IsFalse(result.Contains("<summary>"), "Replace 模式应移除 summary 标签");
        }

        [Test]
        public void UnparseableConcatAttribute_FallsBackToXml()
        {
            // 拼接字符串实参无法安全解析 → 按"无特性"处理，回退 XML 内容重新生成
            const string source = @"using System;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>XML 权威文本</summary>
    [Summary(""前半"" + ""后半"")]
    public class TestConcat { }
}
";
            var result = new XmlSummaryTool(source).ParseSourceScript()
                .GetProcessedSourceScript(XmlSummaryTool.ProcessMode.SyncSummary);
            StringAssert.Contains("[Summary(\"XML 权威文本\")]", result);
            Assert.IsFalse(result.Contains(@"""前半"" + ""后半"""), "拼接特性无法解析时应在移除后重新生成（RemovedFirst 以宽松匹配删除）");
        }

        #endregion

        #region XML 实体解码（双重转义修复）

        const string EntitySummaryCode = @"using System;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>使用 List&lt;T&gt; 与 &amp; 符号的注释</summary>
    public class TestEntitySummary { }
}
";

        [Test]
        public void EntityInSummary_DecodedBeforeAttributeGeneration()
        {
            var result = new XmlSummaryTool(EntitySummaryCode).ParseSourceScript()
                .GetProcessedSourceScript(XmlSummaryTool.ProcessMode.SyncSummary);
            StringAssert.Contains("[Summary(\"使用 List<T> 与 & 符号的注释\")]", result);
            Assert.IsFalse(result.Contains("&amp;lt;"), "实体不应被双重转义（&lt; → 特性 → &amp;lt;）");
        }

        [Test]
        public void EntityInSummary_SyncIsIdempotentAcrossRuns()
        {
            var first = new XmlSummaryTool(EntitySummaryCode).ParseSourceScript()
                .GetProcessedSourceScript(XmlSummaryTool.ProcessMode.SyncSummary);
            var second = new XmlSummaryTool(first).ParseSourceScript()
                .GetProcessedSourceScript(XmlSummaryTool.ProcessMode.SyncSummary);
            Assert.AreEqual(first, second, "第二次 Sync 应为空操作（特性文本与 XML 实体解码后一致）");
        }

        #endregion

        #region 多成员代码块特性归属（fail-closed 跳过）

        const string NonFirstMemberAttrCode = @"using System;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>首成员的注释</summary>
    public class First { }

    [Summary(""次成员的特性"")]
    public class Second { }
}
";

        [Test]
        public void NonFirstMemberAttribute_SyncSkipsBlockWithWarning()
        {
            LogAssert.Expect(LogType.Warning, new Regex("无法安全判断归属"));
            var result = new XmlSummaryTool(NonFirstMemberAttrCode).ParseSourceScript()
                .GetProcessedSourceScript(XmlSummaryTool.ProcessMode.SyncSummary);
            StringAssert.Contains("[Summary(\"次成员的特性\")]", result);
            Assert.IsFalse(result.Contains("[Summary(\"首成员的注释\")]"), "非首成员持特性时不应把次成员内容错注到块头");
        }

        [Test]
        public void NonFirstMemberAttribute_ReplaceSkipsBlockWithWarning()
        {
            LogAssert.Expect(LogType.Warning, new Regex("无法安全判断归属"));
            var result = new XmlSummaryTool(NonFirstMemberAttrCode).ParseSourceScript()
                .GetProcessedSourceScript(XmlSummaryTool.ProcessMode.ReplaceSummary);
            StringAssert.Contains("[Summary(\"次成员的特性\")]", result);
            StringAssert.Contains("/// <summary>首成员的注释</summary>", result);
        }

        [Test]
        public void NonFirstMemberAttribute_RemoveSkipsBlockWithWarning()
        {
            LogAssert.Expect(LogType.Warning, new Regex("无法安全判断归属"));
            var result = new XmlSummaryTool(NonFirstMemberAttrCode).ParseSourceScript()
                .GetProcessedSourceScript(XmlSummaryTool.ProcessMode.RemoveSummary);
            StringAssert.Contains("[Summary(\"次成员的特性\")]", result);
        }

        const string BothMembersAttrCode = @"using System;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>首成员的注释</summary>
    [Summary(""首成员的特性"")]
    public class FirstA { }

    [Summary(""次成员的特性"")]
    public class SecondA { }
}
";

        [Test]
        public void BothMembersHaveAttribute_SyncSkipsWholeBlock()
        {
            // 保守语义：块内任何非首成员持特性即整块跳过——即使首成员特性可判定，
            // 也避免 Remove 路径误删次成员特性（fail-closed，用户手动拆分后再处理）
            LogAssert.Expect(LogType.Warning, new Regex("无法安全判断归属"));
            var result = new XmlSummaryTool(BothMembersAttrCode).ParseSourceScript()
                .GetProcessedSourceScript(XmlSummaryTool.ProcessMode.SyncSummary);
            StringAssert.Contains("[Summary(\"首成员的特性\")]", result);
            StringAssert.Contains("[Summary(\"次成员的特性\")]", result);
            StringAssert.Contains("/// <summary>首成员的注释</summary>", result);
        }

        [Test]
        public void FirstMemberAttributeWithNestedMemberAttribute_SyncSkips()
        {
            // 嵌套成员持特性同样判为非首成员（此前会被误认为类本身的特性）
            const string source = @"using System;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>类的注释</summary>
    public class Outer
    {
        [Summary(""嵌套方法的特性"")]
        public void Nested() { }
    }
}
";
            LogAssert.Expect(LogType.Warning, new Regex("无法安全判断归属"));
            var result = new XmlSummaryTool(source).ParseSourceScript()
                .GetProcessedSourceScript(XmlSummaryTool.ProcessMode.SyncSummary);
            StringAssert.Contains("[Summary(\"嵌套方法的特性\")]", result);
            Assert.IsFalse(result.Contains("[Summary(\"类的注释\")]"), "嵌套成员的特性不应被错注到类上");
        }

        #endregion
    }
}
