using NUnit.Framework;

namespace Runestone.AesirInspector.Editor.Tests
{
    public class XmlSummaryToolTests
    {
        const string TypeSummaryCode = @"using System;

namespace Runestone.AesirInspector.Editor.Tests
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

namespace Runestone.AesirInspector.Editor.Tests
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

namespace Runestone.AesirInspector.Editor.Tests
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
using Runestone.AesirInspector;

namespace Runestone.AesirInspector.Editor.Tests
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
using Runestone.AesirInspector;

namespace Runestone.AesirInspector.Editor.Tests
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
using Runestone.AesirInspector;

namespace Runestone.AesirInspector.Editor.Tests
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

namespace Runestone.AesirInspector.Editor.Tests
{
    /// <summary>单行 summary 测试</summary>
    [Serializable]
    public class TestSingleLineSummary { }
}
";

        const string MixedSingleMultiLineCode = @"using System;
using Runestone.AesirInspector;

namespace Runestone.AesirInspector.Editor.Tests
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
                @"using Runestone.AesirInspector;
using System;

namespace Runestone.AesirInspector.Editor.Tests
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
                @"using Runestone.AesirInspector;
using System;

namespace Runestone.AesirInspector.Editor.Tests
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
            ProcessAndAssert(SpecialCharsCode, XmlSummaryTool.ProcessMode.SyncSummary,
                @"using Runestone.AesirInspector;
using System;

namespace Runestone.AesirInspector.Editor.Tests
{
    /// <summary>
    /// 成员 "" Summary 注释 ????
    /// &lt;para&gt;aaa&lt;/para&gt;
    /// <para>aaa</para>
    /// </summary>
    /// <remarks>AAAAA</remarks>>
    [Summary(""成员 "" Summary 注释 ???? &lt;para&gt;aaa&lt;/para&gt; aaa"")]
    [Obsolete(""临时方法"")] public struct TestStructSummary { }
}
");
        }

        [Test]
        public void SpecialCharsSummary_ReplaceHandlesCorrectly()
        {
            ProcessAndAssert(SpecialCharsCode, XmlSummaryTool.ProcessMode.ReplaceSummary,
                @"using Runestone.AesirInspector;
using System;

namespace Runestone.AesirInspector.Editor.Tests
{
    /// <remarks>AAAAA</remarks>>
    [Summary(""成员 "" Summary 注释 ???? &lt;para&gt;aaa&lt;/para&gt; aaa"")]
    [Obsolete(""临时方法"")] public struct TestStructSummary { }
}
");
        }

        [Test]
        public void MethodSummary_SyncAddsAttribute()
        {
            ProcessAndAssert(MethodSummaryCode, XmlSummaryTool.ProcessMode.SyncSummary,
                @"using Runestone.AesirInspector;
using System;
using UnityEngine;

namespace Runestone.AesirInspector.Editor.Tests
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
                @"using Runestone.AesirInspector;
using System;
using UnityEngine;

namespace Runestone.AesirInspector.Editor.Tests
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
        public void ExistingMultiLineAttribute_SyncUpdatesFromXml()
        {
            ProcessAndAssert(MultiLineAttrCode, XmlSummaryTool.ProcessMode.SyncSummary, @"using System;
using UnityEngine;
using Runestone.AesirInspector;

namespace Runestone.AesirInspector.Editor.Tests
{
    /// <summary>
    /// 测试移除 ChineseSummary
    /// </summary>
    [Summary(""测试移除 ChineseSummary"")]
    [Obsolete(""临时方法"")]
    public class TestRemoveSummaryB
    {
        /// <summary>
        /// BBB
        /// </summary>
        [Summary(""BBB"")]
        [Obsolete(""临时方法"")] public void Method()
        {
            Debug.Log(""测试移除多行的 ChineseSummary"");
        }
    }
}
");
        }

        [Test]
        public void ExistingMultiLineAttribute_ReplaceUpdatesFromXml()
        {
            ProcessAndAssert(MultiLineAttrCode, XmlSummaryTool.ProcessMode.ReplaceSummary, @"using System;
using UnityEngine;
using Runestone.AesirInspector;

namespace Runestone.AesirInspector.Editor.Tests
{
    [Summary(""测试移除 ChineseSummary"")]
    [Obsolete(""临时方法"")]
    public class TestRemoveSummaryB
    {
        [Summary(""BBB"")]
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
            Assert.IsTrue(result.Contains("using Runestone.AesirInspector;"));
            Assert.IsTrue(result.Contains("public class NoSummaryClass { }"));
        }

        [Test]
        public void RemoveAllSummaryAttributes()
        {
            ProcessAndAssert(MultiLineAttrCode, XmlSummaryTool.ProcessMode.RemoveSummary, @"using System;
using UnityEngine;
using Runestone.AesirInspector;

namespace Runestone.AesirInspector.Editor.Tests
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
        public void Preprocessor_SyncAddsAttributeInsideBlock()
        {
            ProcessAndAssert(PreprocessorCode, XmlSummaryTool.ProcessMode.SyncSummary, @"using System;
using Runestone.AesirInspector;

namespace Runestone.AesirInspector.Editor.Tests
{
    /// <summary>
    /// 编辑器工具类
    /// </summary>
    [Summary(""编辑器工具类"")]
    public class TestPreprocessor
    {
        /// <summary>
        /// 编辑器专用方法
        /// </summary>
#if UNITY_EDITOR
        [Summary(""编辑器专用方法"")]
        public void EditorMethod() { }
#endif
    }
}
");
        }

        [Test]
        public void Preprocessor_ReplaceReplacesTagInsideBlock()
        {
            ProcessAndAssert(PreprocessorCode, XmlSummaryTool.ProcessMode.ReplaceSummary, @"using System;
using Runestone.AesirInspector;

namespace Runestone.AesirInspector.Editor.Tests
{
    [Summary(""编辑器工具类"")]
    public class TestPreprocessor
    {
#if UNITY_EDITOR
        [Summary(""编辑器专用方法"")]
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
using Runestone.AesirInspector;

namespace Runestone.AesirInspector.Editor.Tests
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
                @"using Runestone.AesirInspector;
using System;

namespace Runestone.AesirInspector.Editor.Tests
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
                @"using Runestone.AesirInspector;
using System;

namespace Runestone.AesirInspector.Editor.Tests
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
            ProcessAndAssert(MixedSingleMultiLineCode, XmlSummaryTool.ProcessMode.SyncSummary,
                @"using System;
using Runestone.AesirInspector;

namespace Runestone.AesirInspector.Editor.Tests
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
using Runestone.AesirInspector;

namespace Runestone.AesirInspector.Editor.Tests
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
    }
}
