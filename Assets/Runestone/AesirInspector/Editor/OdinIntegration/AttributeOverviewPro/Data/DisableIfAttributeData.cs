using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// DisableIf 特性的介绍数据。
    /// </summary>
    internal class DisableIfAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("DisableIf", "DisableIf", "DisableIf 特性用于根据条件控制属性是否在检查器中禁用。",
                "The DisableIf attribute is used to control whether a property is disabled in the inspector based on a condition.",
                OdinInspectorDocumentationLinks.DisableIfUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("支持根据 bool 成员、属性、方法或 C# 表达式来动态控制禁用状态。",
                "Supports dynamically controlling the disabled state based on bool members, properties, methods, or C# expressions."),
            new BilingualData("支持通过 optionalValue 参数将成员值与特定值进行比较。",
                "Supports comparing a member value with a specific value via the optionalValue parameter."),
            new BilingualData("当条件满足时，属性在检查器中将显示为禁用（灰掉）状态，且不可编辑。",
                "When the condition is met, the property will appear disabled (greyed out) in the inspector and will not be editable.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "condition",
                new BilingualData("用于判断的成员名、方法名或表达式。",
                    "The member name, method name, or expression used for judgment.")),
            new ParameterValue(typeof(object).FullName, "optionalValue",
                new BilingualData("可选的比较值。如果提供，则当 condition 成员的值等于此值时，属性被禁用。",
                    "An optional comparison value. If provided, the property is disabled when the value of the condition member equals this value."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Condition", ResolverType.ValueResolver, typeof(bool).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Usage Examples",
                DisableIfExampleSO.Instance)
        };
    }
}
