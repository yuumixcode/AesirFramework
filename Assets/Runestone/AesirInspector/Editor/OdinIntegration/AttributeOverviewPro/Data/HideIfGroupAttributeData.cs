using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// HideIfGroup 特性的介绍数据。
    /// </summary>
    internal class HideIfGroupAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("HideIfGroup", "HideIfGroup",
                "HideIfGroup 特性用于定义一个组，该组根据条件动态隐藏或显示。组路径可以作为条件。",
                "The HideIfGroup attribute is used to define a group that is dynamically hidden or shown based on a condition. The group path can serve as the condition.",
                OdinInspectorDocumentationLinks.HideIfGroupUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("组路径可以作为条件判断的成员名，无需单独设置 Condition 参数。",
                "The group path can serve as the condition member name without needing a separate Condition parameter."),
            new BilingualData("支持通过 Condition 参数指定成员名、方法名或表达式来控制组的隐藏。",
                "Supports specifying a member name, method name, or expression via the Condition parameter to control group visibility."),
            new BilingualData("配合 Value 参数，可以根据枚举或其他值进行匹配隐藏。",
                "With the Value parameter, visibility can be controlled based on matches with enums or other values.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "GroupName",
                new BilingualData("组的路径。如果没有指定 Condition，则路径也用作条件判断。",
                    "The path of the group. If no Condition is specified, the path also serves as the condition.")),
            new ParameterValue(typeof(string).FullName, "Condition",
                new BilingualData("控制组隐藏的条件成员名或表达式。",
                    "The condition member name or expression controlling group visibility.")),
            new ParameterValue(typeof(object).FullName, "Value",
                new BilingualData("可选值，当 condition 的值匹配此值时组隐藏。",
                    "Optional value; the group is hidden when condition matches this value."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("GroupName", ResolverType.ValueResolver, typeof(string).FullName,
                "None", new List<ParameterValue>()),
            new ResolvedStringParameterValue("Condition", ResolverType.ValueResolver, typeof(bool).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                HideIfGroupExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("GroupName Resolved",
                HideIfGroupExampleWithGroupNameSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Condition Resolved",
                HideIfGroupExampleWithConditionSO.Instance)
        };
    }
}
