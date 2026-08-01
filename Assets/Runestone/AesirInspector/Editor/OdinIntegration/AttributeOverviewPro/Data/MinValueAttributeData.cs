using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// MinValue 特性的介绍数据。
    /// </summary>
    internal class MinValueAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Min Value", "最小值", "MinValue 特性为数字属性设置一个最小值。如果值小于该值，它将被限制在该值。",
                "The MinValue attribute sets a minimum value for a numeric property. If the value becomes less than the specified minimum, it will be clamped to it.",
                OdinInspectorDocumentationLinks.MinValueUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("你可以指定一个固定的最小值，或者引用另一个成员作为动态最小值。",
                "You can specify a fixed minimum value, or reference another member as a dynamic minimum value.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[1]
        {
            new ParameterValue(typeof(double).FullName, "MinValue",
                new BilingualData("允许的最小值。支持使用 $ 引用成员。",
                    "The minimum value allowed. Supports $ for member reference."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("MinValue", ResolverType.ValueResolver, "double", "0",
                new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                MinValueExampleSO.Instance)
        };
    }
}
