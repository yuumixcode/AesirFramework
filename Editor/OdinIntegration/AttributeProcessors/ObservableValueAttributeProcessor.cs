using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirArchitecture.Editor.OdinIntegration
{
    /// <summary>
    /// 为泛型 ObservableValue 提供的 Odin Inspector 属性处理器，用于优化其在面板上的展示效果。
    /// </summary>
    /// <typeparam name="T">ObservableValue 所包装的值类型</typeparam>
    /// <remarks>
    /// 通过 Odin AttributeProcessor 机制，在不修改 <see cref="ObservableValue{T}"/> 类代码的前提下，
    /// 为其在 Inspector 中自动添加展示与响应特性：
    /// <para>
    /// <see cref="ProcessSelfAttributes"/>：添加 <c>[HideLabel]</c> 隐藏默认标签并配合 <c>[InlineProperty]</c> 内联展示，
    /// 使 <see cref="ObservableValue{T}"/> 在 Inspector 中以紧凑形式呈现，避免不必要的嵌套层级。
    /// </para>
    /// <para>
    /// <see cref="ProcessChildMemberAttributes"/>：通过成员名匹配定位到内部 value 字段后，
    /// 添加 <c>[OnValueChanged]</c> 特性使其在 Inspector 编辑时自动调用 <c>InvokeEvent()</c> 触发变更通知，
    /// 实现 Inspector 中直接编辑值即可触发响应式更新，无需运行代码。
    /// </para>
    /// <para>
    /// 成员名匹配使用 <see cref="ObservableValue{T}.PrivateValueFieldName"/> 和 <see cref="ObservableValue{T}.InvokeMethodName"/> 常量，
    /// 避免硬编码字符串导致重构时不一致的风险。
    /// </para>
    /// <para>
    /// <c>[LabelText]</c> 使用 Odin 表达式 <c>@$property.Parent.Name</c> 动态显示所属属性名，
    /// 使每个 ObservableValue 字段在 Inspector 中都有语义化的标签。
    /// </para>
    /// </remarks>
    /// <seealso cref="ObservableValue{T}"/>
    public class ObservableValueAttributeProcessor<T> : OdinAttributeProcessor<ObservableValue<T>>
    {
        /// <summary>
        /// 处理类自身的特性，隐藏标签并使其内联展示
        /// </summary>
        /// <param name="property">Odin Inspector 正在构建的属性节点，代表被处理的 ObservableValue 实例</param>
        /// <param name="attributes">该属性节点当前已附加的特性列表，处理器可向其中添加新特性</param>
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new HideLabelAttribute());
            attributes.Add(new InlinePropertyAttribute());
        }

        /// <summary>
        /// 处理子成员特性，当值在 Inspector 中被修改时自动触发变更通知事件
        /// </summary>
        /// <param name="parentProperty">所属父属性节点，即 ObservableValue 实例对应的 InspectorProperty</param>
        /// <param name="member">正在处理的目标成员信息，用于判断是否为 value 字段</param>
        /// <param name="attributes">该成员当前已附加的特性列表，处理器可向其中添加新特性</param>
        /// <remarks>
        /// 通过比较成员名与 <see cref="ObservableValue{T}.PrivateValueFieldName"/> 常量来定位内部 value 字段，
        /// 避免硬编码字段名字符串。一旦匹配成功，即为该字段添加变更回调与动态标签特性。
        /// </remarks>
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            if (member.MemberType == MemberTypes.Field &&
                member.Name == ObservableValue<T>.PrivateValueFieldName)
            {
                attributes.Add(new OnValueChangedAttribute(ObservableValue<T>.InvokeMethodName, true));
                attributes.Add(new LabelTextAttribute("@$property.Parent.Name", true, SdfIconType.EyeFill));
            }
        }
    }
}
