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
    public class ObservableValueAttributeProcessor<T> : OdinAttributeProcessor<ObservableValue<T>>
    {
        /// <summary>
        /// 处理类自身的特性，隐藏标签并使其内联展示
        /// </summary>
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new HideLabelAttribute());
            attributes.Add(new InlinePropertyAttribute());
        }

        /// <summary>
        /// 处理子成员特性，当值在 Inspector 中被修改时自动触发变更通知事件
        /// </summary>
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
