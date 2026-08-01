using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// MaxValue 特性的介绍数据。
    /// </summary>
    internal class MaxValueAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Max Value", "最大值", "MaxValue 特性为数字属性设置一个最大值。如果值大于该值，它将被限制在该值。",
                "The MaxValue attribute sets a maximum value for a numeric property. If the value becomes greater than the specified maximum, it will be clamped to it.",
                OdinInspectorDocumentationLinks.MaxValueUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("你可以指定一个固定的最大值，或者引用另一个成员作为动态最大值。",
                "You can specify a fixed maximum value, or reference another member as a dynamic maximum value.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[1]
        {
            new ParameterValue(typeof(double).FullName, "MaxValue",
                new BilingualData("允许的最大值。支持使用 $ 引用成员。",
                    "The maximum value allowed. Supports $ for member reference."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("MaxValue", ResolverType.ValueResolver, "double", "100",
                new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                MaxValueExampleSO.Instance)
        };
    }
}
