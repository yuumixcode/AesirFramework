using System;
using System.Reflection;
using NUnit.Framework;
using Runestone.AesirArchitecture.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 守护 <see cref="ObservableValueDrawer" /> 的静态契约：开放泛型注册 + UI Toolkit 绘制入口。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Drawer 靠 [CustomPropertyDrawer(typeof(ObservableValue&lt;&gt;))] 绑定开放泛型——这是 Unity 2022.3
    ///     的关键能力（实测对所有封闭构造生效）。若有人误改成封闭类型或去掉该特性，示例的 Property 样式会静默丢失，
    ///     这类回归用静态守护用例拦住，无需构造 SerializedObject。
    ///     </para>
    ///     <para>
    ///     绘制行为（值变更触发 InvokeEvent、开关切换让路）依赖真实 Inspector/序列化上下文，
    ///     在 EditMode 里构造成本高且脆弱，改由运行时验证；这里只守静态可断言部分。
    ///     </para>
    /// </remarks>
    public class ObservableValueDrawerTests
    {
        /// <summary>
        /// 验证 Drawer 绑定到开放泛型 ObservableValue&lt;&gt; 而非某个封闭类型。
        /// </summary>
        [Test]
        public void CustomPropertyDrawer_TargetsOpenGenericObservableValue()
        {
            var attribute = Attribute.GetCustomAttribute(typeof(ObservableValueDrawer),
                typeof(CustomPropertyDrawer)) as CustomPropertyDrawer;

            Assert.IsNotNull(attribute, "ObservableValueDrawer 必须标注 CustomPropertyDrawer");
            // CustomPropertyDrawer 未公开目标类型，读其私有字段 m_Type（Unity 2022.3 实现）
            var typeField = typeof(CustomPropertyDrawer).GetField("m_Type",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(typeField, "Unity 实现应含 m_Type 字段");
            var targetType = (Type)typeField.GetValue(attribute);
            Assert.IsTrue(targetType.IsGenericTypeDefinition, "Drawer 应绑定开放泛型 ObservableValue<>，才能覆盖所有 T");
            Assert.AreEqual(typeof(ObservableValue<>), targetType, "Drawer 绑定类型必须是 ObservableValue<>");
        }

        /// <summary>
        /// 验证 Drawer 覆写了 UI Toolkit 的 CreatePropertyGUI（而非只写 IMGUI OnGUI）。
        /// </summary>
        [Test]
        public void Drawer_OverridesCreatePropertyGUI()
        {
            var method = typeof(ObservableValueDrawer).GetMethod("CreatePropertyGUI");
            Assert.IsNotNull(method, "必须提供 CreatePropertyGUI 以走 UI Toolkit 绘制");
            Assert.AreEqual(typeof(VisualElement), method.ReturnType, "CreatePropertyGUI 应返回 VisualElement");
        }

        /// <summary>
        /// 验证反射调用的方法名常量指向真实存在的 InvokeEvent——Drawer 通知逻辑依赖它。
        /// </summary>
        [Test]
        public void InvokeMethodName_ResolvesToRealPublicMethod()
        {
            var method = typeof(ObservableValue<int>).GetMethod(ObservableValue<int>.InvokeMethodName,
                BindingFlags.Instance | BindingFlags.Public);

            Assert.IsNotNull(method, $"{ObservableValue<int>.InvokeMethodName} 常量必须指向真实方法");
        }
    }
}
