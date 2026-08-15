using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Runestone.AesirInspector;
using Runestone.AesirInspector.OdinIntegration.Editor;

namespace Runestone.AesirInspector.Editor.Tests
{
    /// <summary>
    /// OdinSourceFileHelper 与 SourceSummaryParser 的局限性测试。
    /// 覆盖会话中分析过的场景：块注释中的假 ///、全限定键、命名空间跟踪、
    /// 多类型同文件、单行 summary、嵌套类型、预处理指令、多文件合并。
    /// </summary>
    public class SourceParsingTests
    {
        /// <summary>
        /// 将源代码字符串转为 SourceFileEntry（模拟文件读取，路径为虚拟值）。
        /// </summary>
        static SourceFileEntry MakeEntry(string source) =>
            new("TestFile.cs", source.Split('\n'));

        /// <summary>
        /// 解析单个源代码字符串，返回 summary 字典。
        /// </summary>
        static Dictionary<string, string> Parse(string source) =>
            SourceSummaryParser.ParseSummaries(new[] { MakeEntry(source) });

        // ─────────────────────────────────────────────────────────────
        // 1. 块注释中的假 /// — 核心改进验证
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 块注释跨行且某行以 /// 开头时，不应被误判为 XML 文档注释。
        /// </summary>
        [Test]
        public void BlockComment_FakeXmlDoc_NotParsed()
        {
            const string source = @"namespace TestNS
{
    /*
    /// <summary>这是块注释内的假 summary</summary>
    */
    /// <summary>这是真正的 summary</summary>
    public class FakeDocClass { }
}
";
            var result = Parse(source);

            Assert.IsTrue(result.ContainsKey("TestNS.FakeDocClass"),
                "真实 summary 应被解析");
            Assert.AreEqual("这是真正的 summary", result["TestNS.FakeDocClass"],
                "假 summary 不应覆盖真实 summary");
            Assert.IsFalse(result.ContainsValue("这是块注释内的假 summary"),
                "块注释内的假 /// 不应被解析");
        }

        /// <summary>
        /// 同一行内 */ 和 /// 共存时，先退出块注释再识别 ///。
        /// 注意：*/ 和 /// 在同一行是极端边缘情况，当前实现将 */ 单独一行作为常见模式处理。
        /// </summary>
        [Test]
        public void BlockComment_SameLineClose_ThenXmlDoc_Parsed()
        {
            const string source = @"namespace TestNS
{
    /*
    some code
    */
    /// <summary>退出块注释后的真实 summary</summary>
    public class SameLineCloseClass { }
}
";
            var result = Parse(source);

            Assert.IsTrue(result.ContainsKey("TestNS.SameLineCloseClass"),
                "退出块注释后的 /// 应被识别");
            Assert.AreEqual("退出块注释后的真实 summary", result["TestNS.SameLineCloseClass"]);
        }

        /// <summary>
        /// 连续多个块注释段落，中间穿插真实 /// 注释。
        /// </summary>
        [Test]
        public void BlockComment_MultipleSegments_OnlyRealXmlDocParsed()
        {
            const string source = @"namespace TestNS
{
    /* block 1
    /// <summary>假 summary 1</summary>
    */
    /// <summary>真实 summary A</summary>
    public class MultiBlockA { }

    /* block 2
    /// <summary>假 summary 2</summary>
    */
    /// <summary>真实 summary B</summary>
    public class MultiBlockB { }
}
";
            var result = Parse(source);

            Assert.AreEqual("真实 summary A", result["TestNS.MultiBlockA"]);
            Assert.AreEqual("真实 summary B", result["TestNS.MultiBlockB"]);
            Assert.IsFalse(result.ContainsValue("假 summary 1"));
            Assert.IsFalse(result.ContainsValue("假 summary 2"));
        }

        // ─────────────────────────────────────────────────────────────
        // 2. 全限定键 — 避免跨类型同名成员冲突
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 同一文件中两个类有同名字段，summary 应通过全限定键区分。
        /// </summary>
        [Test]
        public void FullQualifiedKey_SameMemberName_DifferentTypes()
        {
            const string source = @"namespace TestNS
{
    /// <summary>类 A 的 summary</summary>
    public class ClassA
    {
        /// <summary>A 的 Count 字段</summary>
        public int Count;
    }

    /// <summary>类 B 的 summary</summary>
    public class ClassB
    {
        /// <summary>B 的 Count 字段</summary>
        public int Count;
    }
}
";
            var result = Parse(source);

            Assert.AreEqual("A 的 Count 字段", result["TestNS.ClassA.Count"]);
            Assert.AreEqual("B 的 Count 字段", result["TestNS.ClassB.Count"]);
        }

        /// <summary>
        /// 类型自身的 summary 使用 Namespace.TypeName 键。
        /// </summary>
        [Test]
        public void FullQualifiedKey_TypeLevelSummary()
        {
            const string source = @"namespace My.Namespace
{
    /// <summary>类型级 summary</summary>
    public class TypeLevelTest { }
}
";
            var result = Parse(source);

            Assert.IsTrue(result.ContainsKey("My.Namespace.TypeLevelTest"));
            Assert.AreEqual("类型级 summary", result["My.Namespace.TypeLevelTest"]);
        }

        // ─────────────────────────────────────────────────────────────
        // 3. 命名空间上下文跟踪
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 命名空间中的成员键应包含命名空间前缀。
        /// </summary>
        [Test]
        public void NamespaceContext_NestedNamespace_IncludedInKey()
        {
            const string source = @"namespace Outer.Inner
{
    /// <summary>嵌套命名空间的类</summary>
    public class DeepClass
    {
        /// <summary>嵌套命名空间的方法</summary>
        public void DeepMethod() { }
    }
}
";
            var result = Parse(source);

            Assert.IsTrue(result.ContainsKey("Outer.Inner.DeepClass"));
            Assert.IsTrue(result.ContainsKey("Outer.Inner.DeepClass.DeepMethod()"));
            Assert.AreEqual("嵌套命名空间的类", result["Outer.Inner.DeepClass"]);
            Assert.AreEqual("嵌套命名空间的方法", result["Outer.Inner.DeepClass.DeepMethod()"]);
        }

        /// <summary>
        /// 无命名空间的类型，键不含命名空间前缀。
        /// </summary>
        [Test]
        public void NamespaceContext_NoNamespace_KeyWithoutNamespace()
        {
            const string source = @"/// <summary>无命名空间的类</summary>
public class NoNamespaceClass
{
    /// <summary>无命名空间的方法</summary>
    public void NoNamespaceMethod() { }
}
";
            var result = Parse(source);

            Assert.IsTrue(result.ContainsKey("NoNamespaceClass"));
            Assert.IsTrue(result.ContainsKey("NoNamespaceClass.NoNamespaceMethod()"));
        }

        // ─────────────────────────────────────────────────────────────
        // 4. 单行 XML summary
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 单行 summary（/// &lt;summary&gt;text&lt;/summary&gt;）应被正确解析。
        /// </summary>
        [Test]
        public void SingleLineSummary_ParsedCorrectly()
        {
            const string source = @"namespace TestNS
{
    /// <summary>单行 summary 测试</summary>
    public class SingleLineClass { }
}
";
            var result = Parse(source);

            Assert.AreEqual("单行 summary 测试", result["TestNS.SingleLineClass"]);
        }

        /// <summary>
        /// 单行与多行 summary 混合在同一文件中。
        /// </summary>
        [Test]
        public void MixedSingleMultiLineSummary_AllParsed()
        {
            const string source = @"namespace TestNS
{
    /// <summary>类级别单行</summary>
    public class MixedClass
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
            var result = Parse(source);

            Assert.AreEqual("类级别单行", result["TestNS.MixedClass"]);
            Assert.AreEqual("多行 summary", result["TestNS.MixedClass.MultiLineMethod()"]);
            Assert.AreEqual("单行方法", result["TestNS.MixedClass.SingleLineMethod()"]);
        }

        // ─────────────────────────────────────────────────────────────
        // 5. 多类型同文件
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 一个文件中定义多个类型，每个类型的 summary 应被正确关联。
        /// </summary>
        [Test]
        public void MultipleTypesInFile_AllSummariesParsed()
        {
            const string source = @"namespace TestNS
{
    /// <summary>第一个类</summary>
    public class FirstClass
    {
        /// <summary>第一个类的方法</summary>
        public void FirstMethod() { }
    }

    /// <summary>第二个类</summary>
    public class SecondClass
    {
        /// <summary>第二个类的方法</summary>
        public void SecondMethod() { }
    }
}
";
            var result = Parse(source);

            Assert.AreEqual("第一个类", result["TestNS.FirstClass"]);
            Assert.AreEqual("第一个类的方法", result["TestNS.FirstClass.FirstMethod()"]);
            Assert.AreEqual("第二个类", result["TestNS.SecondClass"]);
            Assert.AreEqual("第二个类的方法", result["TestNS.SecondClass.SecondMethod()"]);
        }

        // ─────────────────────────────────────────────────────────────
        // 6. 预处理指令
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// summary 后的成员声明前有预处理指令（#if），summary 应正确关联。
        /// </summary>
        [Test]
        public void PreprocessorDirective_SummaryAssociatedCorrectly()
        {
            const string source = @"namespace TestNS
{
    public class PreprocessorClass
    {
        /// <summary>
        /// 编辑器专用方法
        /// </summary>
#if UNITY_EDITOR
        public void EditorMethod() { }
#endif
    }
}
";
            var result = Parse(source);

            Assert.IsTrue(result.ContainsKey("TestNS.PreprocessorClass.EditorMethod()"),
                "预处理指令后的成员声明应被正确关联 summary");
            Assert.AreEqual("编辑器专用方法", result["TestNS.PreprocessorClass.EditorMethod()"]);
        }

        /// <summary>
        /// summary 后有特性行再跟成员声明，summary 应正确关联。
        /// </summary>
        [Test]
        public void AttributeLine_SummaryAssociatedCorrectly()
        {
            const string source = @"namespace TestNS
{
    public class AttributeClass
    {
        /// <summary>
        /// 带特性的方法
        /// </summary>
        [System.Obsolete]
        public void AttributedMethod() { }
    }
}
";
            var result = Parse(source);

            Assert.IsTrue(result.ContainsKey("TestNS.AttributeClass.AttributedMethod()"));
            Assert.AreEqual("带特性的方法", result["TestNS.AttributeClass.AttributedMethod()"]);
        }

        // ─────────────────────────────────────────────────────────────
        // 7. 多文件合并（模拟 partial class）
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 两个源文件条目合并解析时，不同文件的 summary 应合并到同一字典。
        /// </summary>
        [Test]
        public void MultipleFiles_SummariesMerged()
        {
            var entry1 = MakeEntry(@"namespace TestNS
{
    /// <summary>文件一的类</summary>
    public partial class PartialClass
    {
        /// <summary>文件一的方法</summary>
        public void MethodOne() { }
    }
}
");
            var entry2 = MakeEntry(@"namespace TestNS
{
    public partial class PartialClass
    {
        /// <summary>文件二的方法</summary>
        public void MethodTwo() { }
    }
}
");
            var result = SourceSummaryParser.ParseSummaries(new[] { entry1, entry2 });

            Assert.AreEqual("文件一的类", result["TestNS.PartialClass"]);
            Assert.AreEqual("文件一的方法", result["TestNS.PartialClass.MethodOne()"]);
            Assert.AreEqual("文件二的方法", result["TestNS.PartialClass.MethodTwo()"]);
        }

        // ─────────────────────────────────────────────────────────────
        // 8. 无 XML 注释的成员返回 null（字典中不存在）
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 没有 XML summary 注释的成员不应出现在结果字典中。
        /// </summary>
        [Test]
        public void NoXmlComment_NotInResult()
        {
            const string source = @"namespace TestNS
{
    public class NoCommentClass
    {
        public void NoCommentMethod() { }
    }
}
";
            var result = Parse(source);

            Assert.IsFalse(result.ContainsKey("TestNS.NoCommentClass"));
            Assert.IsFalse(result.ContainsKey("TestNS.NoCommentClass.NoCommentMethod"));
        }

        // ─────────────────────────────────────────────────────────────
        // 9. ExtractMemberName 边缘场景
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// ExtractMemberName 应正确提取字段、属性、方法声明中的成员名。
        /// </summary>
        [Test]
        public void ExtractMemberName_VariousDeclarations()
        {
            Assert.AreEqual("MyField", SourceFileAnalyzerUtility.ExtractMemberName("public int MyField;"));
            Assert.AreEqual("MyField", SourceFileAnalyzerUtility.ExtractMemberName("public int MyField = 42;"));
            Assert.AreEqual("MyProperty", SourceFileAnalyzerUtility.ExtractMemberName("public int MyProperty { get; set; }"));
            Assert.AreEqual("MyMethod", SourceFileAnalyzerUtility.ExtractMemberName("public void MyMethod() { }"));
            Assert.AreEqual("MyMethod", SourceFileAnalyzerUtility.ExtractMemberName("public void MyMethod(string s, int i) { }"));
        }

        /// <summary>
        /// ExtractMemberName 应忽略常见关键字。注意：ExtractMemberName 专为成员声明设计，
        /// 非成员语句（如 using 指令）不在设计范围内。
        /// </summary>
        [Test]
        public void ExtractMemberName_Keywords_ReturnsNull()
        {
            Assert.IsNull(SourceFileAnalyzerUtility.ExtractMemberName("return null;"));
            Assert.IsNull(SourceFileAnalyzerUtility.ExtractMemberName("if (true) { }"));
        }

        /// <summary>
        /// ExtractMemberName 应处理带特性的声明行。
        /// </summary>
        [Test]
        public void ExtractMemberName_WithAttributes()
        {
            Assert.AreEqual("AttributedField",
                SourceFileAnalyzerUtility.ExtractMemberName("[SerializeField] private int AttributedField;"));
            Assert.AreEqual("AttributedMethod",
                SourceFileAnalyzerUtility.ExtractMemberName("[Obsolete] public void AttributedMethod() { }"));
        }

        /// <summary>
        /// ExtractMemberName 应处理行尾注释。
        /// </summary>
        [Test]
        public void ExtractMemberName_WithTrailingComment()
        {
            Assert.AreEqual("Count",
                SourceFileAnalyzerUtility.ExtractMemberName("public int Count; // 计数字段"));
            Assert.AreEqual("Name",
                SourceFileAnalyzerUtility.ExtractMemberName("public string Name { get; set; } // 名称属性"));
        }

        /// <summary>
        /// ExtractMemberName 应处理枚举成员。
        /// </summary>
        [Test]
        public void ExtractMemberName_EnumMember()
        {
            Assert.AreEqual("None", SourceFileAnalyzerUtility.ExtractMemberName("None = 0,"));
            Assert.AreEqual("First", SourceFileAnalyzerUtility.ExtractMemberName("First,"));
            Assert.AreEqual("Second", SourceFileAnalyzerUtility.ExtractMemberName("Second = 1 << 1,"));
        }

        /// <summary>
        /// ExtractMemberName 应处理多行属性声明（行尾无 { ; = ( 终止符）。
        /// 如 "public static IContext Interface" 后跟换行的 { get { ... } }。
        /// </summary>
        [Test]
        public void ExtractMemberName_MultiLineProperty_NoTerminator()
        {
            Assert.AreEqual("Interface",
                SourceFileAnalyzerUtility.ExtractMemberName("public static IContext Interface"));
            Assert.AreEqual("Count",
                SourceFileAnalyzerUtility.ExtractMemberName("public int Count"));
            Assert.AreEqual("Name",
                SourceFileAnalyzerUtility.ExtractMemberName("protected string Name"));
        }

        /// <summary>
        /// ExtractMemberName 应处理泛型方法声明（成员名后跟 &lt;T&gt; 而非直接跟 (。
        /// 如 "public void RegisterModel&lt;TModel&gt;(TModel model) where TModel : class, IModel"。
        /// </summary>
        [Test]
        public void ExtractMemberName_GenericMethod()
        {
            Assert.AreEqual("RegisterModel",
                SourceFileAnalyzerUtility.ExtractMemberName("public void RegisterModel<TModel>(TModel model) where TModel : class, IModel"));
            Assert.AreEqual("RegisterService",
                SourceFileAnalyzerUtility.ExtractMemberName("public void RegisterService<TService>(TService service) where TService : class, IService"));
            Assert.AreEqual("GetModel",
                SourceFileAnalyzerUtility.ExtractMemberName("public TModel GetModel<TModel>() where TModel : class, IModel"));
            Assert.AreEqual("GenericMethod",
                SourceFileAnalyzerUtility.ExtractMemberName("public void GenericMethod<T>(T param)"));
        }

        /// <summary>
        /// ExtractMemberName 应处理表达式体泛型方法声明。
        /// 表达式体 "=>" 中的 = 会被通用正则误匹配为成员终止符，
        /// 泛型方法正则必须优先执行才能正确提取成员名。
        /// 如 "public TModel GetModel&lt;TModel&gt;() where TModel : class, IModel =&gt; ..."。
        /// </summary>
        [Test]
        public void ExtractMemberName_GenericMethod_ExpressionBodied()
        {
            // 完整表达式体单行声明
            Assert.AreEqual("GetModel",
                SourceFileAnalyzerUtility.ExtractMemberName(
                    "public TModel GetModel<TModel>() where TModel : class, IModel => _modelLocator.Get<TModel>();"));
            Assert.AreEqual("GetService",
                SourceFileAnalyzerUtility.ExtractMemberName(
                    "public TService GetService<TService>() where TService : class, IService =>"));
            // 确保不会误匹配到表达式体末尾的泛型方法调用
            Assert.AreNotEqual("Get",
                SourceFileAnalyzerUtility.ExtractMemberName(
                    "public TModel GetModel<TModel>() where TModel : class, IModel => _modelLocator.Get<TModel>();"));
        }

        /// <summary>
        /// 表达式体泛型方法的 summary 应被正确解析。
        /// 验证从源代码到 summary 字典的完整流程，确保 =>
        /// 不会导致成员名被误提取为约束类型名（如 IModel）。
        /// </summary>
        [Test]
        public void GenericMethod_ExpressionBodied_SummaryParsed()
        {
            const string source = @"namespace TestNS
{
    /// <summary>
    /// 表达式体泛型方法测试类
    /// </summary>
    public class ExprBodyGenericClass
    {
        /// <summary>
        /// 获取已注册的 Model
        /// </summary>
        public TModel GetModel<TModel>() where TModel : class, IModel => _locator.Get<TModel>();

        /// <summary>
        /// 获取已注册的 Service
        /// </summary>
        public TService GetService<TService>() where TService : class, IService =>
            _locator.Get<TService>();
    }
}
";
            var result = Parse(source);

            Assert.AreEqual("表达式体泛型方法测试类", result["TestNS.ExprBodyGenericClass"]);
            Assert.AreEqual("获取已注册的 Model", result["TestNS.ExprBodyGenericClass.GetModel()"]);
            Assert.AreEqual("获取已注册的 Service", result["TestNS.ExprBodyGenericClass.GetService()"]);
        }

        /// <summary>
        /// 多行属性声明（行尾无终止符）的 summary 应被正确解析。
        /// 验证从源代码到 summary 字典的完整流程。
        /// </summary>
        [Test]
        public void MultiLineProperty_SummaryParsed()
        {
            const string source = @"namespace TestNS
{
    /// <summary>
    /// 多行属性声明的测试类
    /// </summary>
    public class MultiLinePropertyClass
    {
        /// <summary>
        /// 静态属性，声明跨多行
        /// </summary>
        public static IContext Interface
        {
            get { return null; }
        }

        /// <summary>
        /// 实例属性，声明跨多行
        /// </summary>
        public int Count
        {
            get;
            set;
        }
    }
}
";
            var result = Parse(source);

            Assert.AreEqual("多行属性声明的测试类", result["TestNS.MultiLinePropertyClass"]);
            Assert.AreEqual("静态属性，声明跨多行", result["TestNS.MultiLinePropertyClass.Interface"]);
            Assert.AreEqual("实例属性，声明跨多行", result["TestNS.MultiLinePropertyClass.Count"]);
        }

        /// <summary>
        /// 泛型方法的 summary 应被正确解析。
        /// 验证从源代码到 summary 字典的完整流程。
        /// </summary>
        [Test]
        public void GenericMethod_SummaryParsed()
        {
            const string source = @"namespace TestNS
{
    /// <summary>
    /// 泛型方法测试类
    /// </summary>
    public class GenericMethodClass
    {
        /// <summary>
        /// 注册 Model
        /// </summary>
        public void RegisterModel<TModel>(TModel model) where TModel : class, IModel
        {
        }

        /// <summary>
        /// 注册 Service
        /// </summary>
        public void RegisterService<TService>(TService service) where TService : class, IService
        {
        }
    }
}
";
            var result = Parse(source);

            Assert.AreEqual("泛型方法测试类", result["TestNS.GenericMethodClass"]);
            Assert.AreEqual("注册 Model", result["TestNS.GenericMethodClass.RegisterModel(TModel)"]);
            Assert.AreEqual("注册 Service", result["TestNS.GenericMethodClass.RegisterService(TService)"]);
        }

        // ─────────────────────────────────────────────────────────────
        // 10. 字符串中的假 /// （已知局限，记录为预期行为）
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 逐字字符串中的 /// 会被误判（已知局限）。
        /// 此测试记录该已知行为，防止回归。
        /// </summary>
        [Test]
        public void StringLiteral_WithTripleSlash_KnownLimitation()
        {
            const string source = @"namespace TestNS
{
    public class StringLimitationClass
    {
        public void Method()
        {
            string s = ""line1
/// <summary>字符串内的假 summary</summary>
line3"";
        }
    }
}
";
            var result = Parse(source);

            // 已知局限：多行逐字字符串中的 /// 会被误判。
            // 此测试仅验证不会崩溃，结果可能包含假条目。
            // 关键断言：真实成员不应被假 summary 覆盖。
            Assert.IsFalse(result.ContainsKey("TestNS.StringLimitationClass"),
                "类自身无 XML summary，不应出现在结果中");
        }

        // ─────────────────────────────────────────────────────────────
        // 11. ParseSummaryText 功能测试
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// ParseSummaryText 应正确提取 &lt;summary&gt; 标签内容并清理 XML 标签。
        /// 注意：ParseSummaryText 接收的行已移除 /// 前缀。
        /// </summary>
        [Test]
        public void ParseSummaryText_StripXmlTags()
        {
            var lines = new List<string>
            {
                "<summary>",
                "这是一个 <see cref=\"System.String\"/> 方法",
                "</summary>"
            };
            var result = SourceSummaryParser.ParseSummaryText(lines);

            Assert.AreEqual("这是一个 String 方法", result);
        }

        /// <summary>
        /// ParseSummaryText 应正确处理多行内容，合并为单行。
        /// </summary>
        [Test]
        public void ParseSummaryText_MultiLineMerged()
        {
            var lines = new List<string>
            {
                "<summary>",
                "第一行",
                "第二行",
                "第三行",
                "</summary>"
            };
            var result = SourceSummaryParser.ParseSummaryText(lines);

            Assert.AreEqual("第一行 第二行 第三行", result);
        }

        /// <summary>
        /// ParseSummaryText 对纯空行 summary 返回 null。
        /// </summary>
        [Test]
        public void ParseSummaryText_EmptyReturnsNull()
        {
            var lines = new List<string>
            {
                "<summary>",
                "",
                "</summary>"
            };
            var result = SourceSummaryParser.ParseSummaryText(lines);

            Assert.IsNull(result);
        }

        // ─────────────────────────────────────────────────────────────
        // 12. 重载方法（已知局限）
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 重载方法应通过参数类型列表区分，每个重载独立解析 summary。
        /// 键格式：AssemblyName.Namespace.TypeName.MethodName(ParamType1, ParamType2)
        /// 无参方法的键以空括号 () 结尾。
        /// </summary>
        [Test]
        public void OverloadedMethods_EachHasOwnSummary()
        {
            const string source = @"namespace TestNS
{
    public class OverloadClass
    {
        /// <summary>
        /// 无参版本
        /// </summary>
        public void DoSomething()
        {
        }

        /// <summary>
        /// 单参版本
        /// </summary>
        public void DoSomething(int count)
        {
        }

        /// <summary>
        /// 双参版本
        /// </summary>
        public void DoSomething(string name, int count)
        {
        }
    }
}
";
            var result = Parse(source);

            // 三个重载各自有独立的键（参数类型列表区分）
            Assert.AreEqual("无参版本", result["TestNS.OverloadClass.DoSomething()"]);
            Assert.AreEqual("单参版本", result["TestNS.OverloadClass.DoSomething(int)"]);
            Assert.AreEqual("双参版本", result["TestNS.OverloadClass.DoSomething(string, int)"]);
        }

        /// <summary>
        /// 非重载方法不受影响——不同方法名各自有独立的键。
        /// </summary>
        [Test]
        public void NonOverloadedMethods_EachHasOwnSummary()
        {
            const string source = @"namespace TestNS
{
    public class NonOverloadClass
    {
        /// <summary>
        /// 方法 A
        /// </summary>
        public void MethodA() { }

        /// <summary>
        /// 方法 B
        /// </summary>
        public void MethodB() { }
    }
}
";
            var result = Parse(source);

            Assert.AreEqual("方法 A", result["TestNS.NonOverloadClass.MethodA()"]);
            Assert.AreEqual("方法 B", result["TestNS.NonOverloadClass.MethodB()"]);
        }

        // ─────────────────────────────────────────────────────────────
        // 13. 嵌套类型（struct/class 声明在外层类内部）
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 嵌套结构体应有自己的 summary，不应回退到外层类的 summary。
        /// Type.FullName 对嵌套类型返回 "Namespace.OuterType+InnerType"，
        /// 查询键需去掉 OuterType+ 前缀，与解析器存储的 "Namespace.InnerType" 匹配。
        /// </summary>
        [Test]
        public void NestedType_HasOwnSummary_NotOuterClassSummary()
        {
            const string source = @"namespace TestNS
{
    /// <summary>
    /// 外层类的注释
    /// </summary>
    public class OuterClass
    {
        /// <summary>
        /// 嵌套结构体 A 的注释
        /// </summary>
        struct NestedStructA { }

        /// <summary>
        /// 嵌套结构体 B 的注释
        /// </summary>
        struct NestedStructB { }

        /// <summary>
        /// 无注释的嵌套结构体
        /// </summary>
        struct NestedStructC { }
    }
}
";
            var result = Parse(source);

            // 外层类的 summary
            Assert.AreEqual("外层类的注释", result["TestNS.OuterClass"]);

            // 嵌套结构体各自的 summary（不含外层类名）
            Assert.AreEqual("嵌套结构体 A 的注释", result["TestNS.NestedStructA"]);
            Assert.AreEqual("嵌套结构体 B 的注释", result["TestNS.NestedStructB"]);
            Assert.AreEqual("无注释的嵌套结构体", result["TestNS.NestedStructC"]);

            // 确保嵌套结构体没有错误地获取外层类的 summary
            Assert.IsFalse(result.ContainsKey("TestNS.OuterClass+NestedStructA"),
                "不应存在带 + 的嵌套类型键");
        }

        /// <summary>
        /// 无 XML 注释的嵌套类型不应返回外层类的 summary。
        /// </summary>
        [Test]
        public void NestedType_WithoutSummary_NotOuterClassSummary()
        {
            const string source = @"namespace TestNS
{
    /// <summary>
    /// 外层类的注释
    /// </summary>
    public class OuterWithNoCommentNested
    {
        struct NoCommentStruct { }
    }
}
";
            var result = Parse(source);

            // 外层类有 summary
            Assert.AreEqual("外层类的注释", result["TestNS.OuterWithNoCommentNested"]);

            // 无注释的嵌套结构体不应出现在结果中
            Assert.IsFalse(result.ContainsKey("TestNS.NoCommentStruct"),
                "无 XML 注释的嵌套类型不应出现在结果字典中");
        }

        // ─────────────────────────────────────────────────────────────
        // 14. 多行方法声明（参数跨行）
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 方法声明参数跨多行时，summary 应通过参数类型列表区分重载。
        /// </summary>
        [Test]
        public void MultiLineMethodDeclaration_SummaryParsed()
        {
            const string source = @"namespace TestNS
{
    public class MultiLineMethodClass
    {
        /// <summary>
        /// 单行声明的方法
        /// </summary>
        public void DoSomething(int count) { }

        /// <summary>
        /// 多行声明的方法（参数跨两行）
        /// </summary>
        public void DoSomething(
            int count,
            string name)
        {
        }

        /// <summary>
        /// 多行声明的方法（每参数一行）
        /// </summary>
        public void DoSomething(
            int count,
            string name,
            bool flag)
        {
        }
    }
}
";
            var result = Parse(source);

            Assert.AreEqual("单行声明的方法", result["TestNS.MultiLineMethodClass.DoSomething(int)"]);
            Assert.AreEqual("多行声明的方法（参数跨两行）", result["TestNS.MultiLineMethodClass.DoSomething(int, string)"]);
            Assert.AreEqual("多行声明的方法（每参数一行）", result["TestNS.MultiLineMethodClass.DoSomething(int, string, bool)"]);
        }
    }
}
