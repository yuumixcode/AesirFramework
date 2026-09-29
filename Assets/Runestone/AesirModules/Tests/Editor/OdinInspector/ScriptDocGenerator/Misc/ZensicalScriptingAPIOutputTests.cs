using System;
using NUnit.Framework;
using Runestone.AesirModules.ScriptDocGenerator;
using Runestone.AesirModules.ScriptDocGenerator.Editor;
using UnityEngine;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>夹具基类：提供继承组成员。</summary>
    public class ZensicalFixtureBase
    {
        public int BaseField;

        public int BaseProperty { get; set; }

        public void BaseMethod() { }
    }

    /// <summary>夹具类型：覆盖常量/声明/继承/运算符分组与参数/返回值表。</summary>
    public class ZensicalFixture : ZensicalFixtureBase
    {
        [Summary("最大数量常量")]
        public const int MaxCount = 10;

        [Summary("当前数量")]
        public int Count;

        [Summary("两数相加")]
        public int Add(int a, int b) => a + b;

#pragma warning disable 67
        [Summary("数量变更事件")]
        public event Action Changed;
#pragma warning restore 67

        [Summary("加法运算符")]
        public static ZensicalFixture operator +(ZensicalFixture x, ZensicalFixture y) => x;
    }

    /// <summary>
    /// ZensicalScriptingAPISettingsSO 输出结构回归（此前 508 行零测试）：
    /// Front Matter、元信息 note 块、声明块、成员章节顺序、分组标签、
    /// md_in_html 表格包裹、详情锚点、参数/返回值表格。
    /// 断言锁定结构标记而非逐字输出（说明列等依赖源码解析链的内容不在本测试范围）。
    /// </summary>
    public class ZensicalScriptingAPIOutputTests
    {
        static ZensicalScriptingAPISettingsSO CreateSettings() =>
            ScriptableObject.CreateInstance<ZensicalScriptingAPISettingsSO>();

        static string GenerateDoc(Type fixtureType) =>
            CreateSettings().GetGeneratedDocumentation(new TypeData(fixtureType));

        [Test]
        public void FrontMatter_AndTypeHeader_Present()
        {
            var doc = GenerateDoc(typeof(ZensicalFixture));

            StringAssert.Contains("---\ntitle: ZensicalFixture\n", doc);
            StringAssert.Contains("!!! note \"\"", doc);
            StringAssert.Contains("**种类:** `class`", doc);
            StringAssert.Contains("**命名空间:** `Runestone.AesirModules.Tests.Editor.ScriptDocGenerator`", doc);
            StringAssert.Contains("**程序集:**", doc);
            StringAssert.Contains("## 声明", doc);
            StringAssert.Contains("``` csharp", doc);
        }

        [Test]
        public void MemberSections_AppearInFixedOrder()
        {
            var doc = GenerateDoc(typeof(ZensicalFixture));

            var ctorIndex = doc.IndexOf("## 构造方法", StringComparison.Ordinal);
            var fieldIndex = doc.IndexOf("## 字段", StringComparison.Ordinal);
            var propertyIndex = doc.IndexOf("## 属性", StringComparison.Ordinal);
            var eventIndex = doc.IndexOf("## 事件", StringComparison.Ordinal);
            var methodIndex = doc.IndexOf("## 方法", StringComparison.Ordinal);

            Assert.GreaterOrEqual(ctorIndex, 0, "缺构造方法章节");
            Assert.Greater(fieldIndex, ctorIndex, "章节顺序应为 构造方法→字段");
            Assert.Greater(propertyIndex, fieldIndex, "章节顺序应为 字段→属性");
            Assert.Greater(eventIndex, propertyIndex, "章节顺序应为 属性→事件");
            Assert.Greater(methodIndex, eventIndex, "章节顺序应为 事件→方法");
        }

        [Test]
        public void FieldGroups_ConstantDeclaredInherited_WithDeclaringTypeColumn()
        {
            var doc = GenerateDoc(typeof(ZensicalFixture));

            StringAssert.Contains("**常量字段**", doc);
            StringAssert.Contains("**声明的字段**", doc);
            StringAssert.Contains("**继承的字段**", doc);
            StringAssert.Contains("| 名称 | 描述 | 声明类型 |", doc);
            StringAssert.Contains("MaxCount", doc);
            StringAssert.Contains("Count", doc);
            StringAssert.Contains("BaseField", doc);
        }

        [Test]
        public void MethodGroups_OperatorGroupPresent()
        {
            var doc = GenerateDoc(typeof(ZensicalFixture));

            StringAssert.Contains("**运算符方法**", doc);
            StringAssert.Contains("operator +", doc);
        }

        [Test]
        public void SummaryTables_WrappedInMdInHtmlDivs()
        {
            var doc = GenerateDoc(typeof(ZensicalFixture));

            StringAssert.Contains("<div class=\"api-summary-table\" markdown=\"1\">", doc);
        }

        [Test]
        public void MethodDetail_AnchorAndParamsAndReturns_Present()
        {
            var doc = GenerateDoc(typeof(ZensicalFixture));

            // 详情小节带显式锚点（api 类别前缀 + 成员展示名），概览表名称列链接到同一锚点
            StringAssert.Contains("{#method-", doc);
            StringAssert.Contains("[`Add(", doc);

            // 参数表（结构化 IParameterData）与返回值表
            StringAssert.Contains("<div class=\"api-params-table\" markdown=\"1\">", doc);
            StringAssert.Contains("| 名称 | 类型 | 说明 |", doc);
            StringAssert.Contains("`a`", doc);
            StringAssert.Contains("`b`", doc);
            StringAssert.Contains("**返回值**", doc);
            StringAssert.Contains("<div class=\"api-returns-table\" markdown=\"1\">", doc);
        }

        [Test]
        public void SummaryAttribute_UsedInDescriptionColumn()
        {
            var doc = GenerateDoc(typeof(ZensicalFixture));

            StringAssert.Contains("最大数量常量", doc);
            StringAssert.Contains("当前数量", doc);
        }
    }
}
