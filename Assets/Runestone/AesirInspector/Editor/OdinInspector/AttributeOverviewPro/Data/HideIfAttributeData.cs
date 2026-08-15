using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// HideIf 特性的介绍数据。
    /// </summary>
    internal class HideIfAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("HideIf", "HideIf", "HideIf 特性用于根据条件动态隐藏属性。",
                "The HideIf attribute is used to dynamically hide properties based on a condition.",
                OdinInspectorDocumentationLinks.HideIfUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("可以根据 bool 成员、方法或表达式来控制属性的隐藏。",
                "Properties can be hidden based on a bool member, method, or expression."),
            new BilingualData("配合 OptionalValue 参数，可以根据枚举或其他值进行匹配隐藏。",
                "With the OptionalValue parameter, properties can be hidden based on matches with enums or other values."),
            new BilingualData("HideIf 仅控制显示，不影响序列化或逻辑，通常与 ShowIf 成对使用。",
                "HideIf only controls visibility and does not affect serialization or logic; it's often used as the opposite of ShowIf.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "condition",
                new BilingualData("控制隐藏的成员名或表达式。",
                    "The member name or expression that controls visibility.")),
            new ParameterValue(typeof(object).FullName, "optionalValue",
                new BilingualData("可选值。如果提供此参数，只有当 condition 的值等于此值时，属性才会隐藏。",
                    "Optional value. If provided, the property will only be hidden if the condition's value equals this value."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Condition", ResolverType.ValueResolver, typeof(bool).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                HideIfExampleSO.Instance)
        };
    }
}
