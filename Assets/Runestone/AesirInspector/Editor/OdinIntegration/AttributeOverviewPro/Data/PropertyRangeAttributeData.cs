using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// PropertyRange 特性的介绍数据。
    /// </summary>
    internal class PropertyRangeAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Property Range", "属性范围",
                "PropertyRange 特性与 Unity 的 Range 特性类似，但它支持使用 $ 符号来引用成员作为动态范围。",
                "The PropertyRange attribute is similar to Unity's Range attribute, but it supports using the $ symbol to reference members for a dynamic range.",
                OdinInspectorDocumentationLinks.PropertyRangeUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("你可以指定滑动条的最小值和最大值。如果使用 $ 引用成员，滑动条的范围将随成员的值变化而动态更新。",
                "You can specify the minimum and maximum values of the slider. If you use $ to reference members, the range of the slider will be dynamically updated as the values of those members change.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[2]
        {
            new ParameterValue(typeof(double).FullName, "Min",
                new BilingualData("滑动条的最小值。支持使用 $ 引用成员。",
                    "The minimum value of the slider. Supports $ for member reference.")),
            new ParameterValue(typeof(double).FullName, "Max",
                new BilingualData("滑动条的最大值。支持使用 $ 引用成员。",
                    "The maximum value of the slider. Supports $ for member reference."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Min", ResolverType.ValueResolver, "double", "0",
                new List<ParameterValue>()),
            new ResolvedStringParameterValue("Max", ResolverType.ValueResolver, "double", "100",
                new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                PropertyRangeExampleSO.Instance)
        };
    }
}
