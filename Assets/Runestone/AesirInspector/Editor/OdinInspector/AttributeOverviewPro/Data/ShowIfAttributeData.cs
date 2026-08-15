using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ShowIf 特性的介绍数据。
    /// </summary>
    internal class ShowIfAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ShowIf", "ShowIf", "ShowIf 特性用于根据条件动态显示或隐藏属性。",
                "The ShowIf attribute is used to dynamically show or hide properties based on a condition.",
                OdinInspectorDocumentationLinks.ShowIfUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("可以根据 bool 成员、方法或表达式来控制属性的显示。",
                "Properties can be shown or hidden based on a bool member, method, or expression."),
            new BilingualData("配合 OptionalValue 参数，可以根据枚举或其他值进行匹配显示。",
                "With the OptionalValue parameter, properties can be shown based on matches with enums or other values."),
            new BilingualData("ShowIf 仅控制显示，不影响序列化或逻辑，通常与 HideIf 成对使用。",
                "ShowIf only controls visibility and does not affect serialization or logic; it's often used as the opposite of HideIf.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "condition",
                new BilingualData("控制显示的成员名或表达式。",
                    "The member name or expression that controls visibility.")),
            new ParameterValue(typeof(object).FullName, "optionalValue",
                new BilingualData("可选值。如果提供此参数，只有当 condition 的值等于此值时，属性才会显示。",
                    "Optional value. If provided, the property will only be shown if the condition's value equals this value."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Condition", ResolverType.ValueResolver, typeof(bool).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ShowIfExampleSO.Instance)
        };
    }
}
