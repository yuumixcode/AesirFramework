using System;
using NUnit.Framework;
using Runestone.AesirModules.ScriptDocGenerator;
using Runestone.AesirModules.ScriptDocGenerator.Editor;
using UnityEngine;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>
    /// DefaultScriptingAPISettingsSO 输出回归：锁定锐评修复的三处输出 bug
    /// （单事件/单方法阈值、常量表过滤布尔、继承属性 flag 守卫）。
    /// </summary>
    public class DefaultScriptingAPISettingsOutputTests
    {
        static DefaultScriptingAPISettingsSO CreateSettings() =>
            ScriptableObjectInMemory.Create<DefaultScriptingAPISettingsSO>();

        [Test]
        public void SingleEvent_Class_ProducesEventSection()
        {
            var settings = CreateSettings();
            var typeData = new TypeData(typeof(SingleEventClass));
            var doc = settings.GetGeneratedDocumentation(typeData);
            StringAssert.Contains("## 事件", doc);
            StringAssert.Contains("OnlyEvent", doc);
        }

        [Test]
        public void SingleMethod_Interface_ProducesMethodSection()
        {
            var settings = CreateSettings();
            var typeData = new TypeData(typeof(ISingleMethodInterface));
            var doc = settings.GetGeneratedDocumentation(typeData);
            StringAssert.Contains("## 方法", doc);
            StringAssert.Contains("OnlyMethod", doc);
        }

        [Test]
        public void PlainPublicField_NotInConstSection()
        {
            var settings = CreateSettings();
            var typeData = new TypeData(typeof(ConstAndPlainFieldClass));
            var doc = settings.GetGeneratedDocumentation(typeData);
            StringAssert.Contains("### 常量字段", doc);
            StringAssert.Contains("MaxCount", doc);
            StringAssert.Contains("### 声明的普通字段", doc);

            // 常量区块内不应出现非 const 字段：以章节标题切片后断言
            var constSection = doc.Substring(doc.IndexOf("### 常量字段"));
            var constSectionEnd = constSection.IndexOf("### 声明的普通字段");
            if (constSectionEnd > 0)
            {
                constSection = constSection.Substring(0, constSectionEnd);
            }

            StringAssert.DoesNotContain("plainField", constSection);
        }

        [Test]
        public void PrivateOverrideProperty_NoEmptyInheritedSection()
        {
            var settings = CreateSettings();
            var typeData = new TypeData(typeof(PrivateOverridePropertyClass));
            var doc = settings.GetGeneratedDocumentation(typeData);

            // 私有 new 属性非 API 成员，基类 public 属性被遮蔽——不应出现空"继承的属性"表
            var hasInheritedHeader = doc.Contains("### 继承的属性");
            Assert.IsFalse(hasInheritedHeader, "仅存在非 API 的继承属性时不应输出空的'继承的属性'章节");
        }

        /// <summary>恰好一个公共事件的类——object 无事件，这是"单事件即丢章节"的现实受害形状</summary>
        public class SingleEventClass
        {
#pragma warning disable 67
            public event Action OnlyEvent;
#pragma warning restore 67
        }

        /// <summary>恰好一个方法的接口——接口无继承方法，单方法接口曾整体丢"方法"章节</summary>
        public interface ISingleMethodInterface
        {
            int OnlyMethod(int count);
        }

        /// <summary>public 非 const 字段 + public const 字段——常量表过滤布尔 bug 的回归形状</summary>
        public class ConstAndPlainFieldClass
        {
            public const int MaxCount = 10;
            public int plainField = 5;
        }

        /// <summary>仅私有继承属性（继承 public 属性并 new 为 private）——曾产生空"继承的属性"章节</summary>
        public class PrivateOverridePropertyClass : PublicPropertyBase
        {
            new int InheritedProperty { get; set; }
        }

        public class PublicPropertyBase
        {
            public int InheritedProperty { get; set; }
        }

        /// <summary>ScriptableObject.CreateInstance 的浅封装，测试内不落盘</summary>
        static class ScriptableObjectInMemory
        {
            public static T Create<T>() where T : ScriptableObject =>
                ScriptableObject.CreateInstance<T>();
        }
    }
}
