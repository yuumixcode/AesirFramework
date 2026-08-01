using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// MinMaxSlider 特性的介绍数据。
    /// </summary>
    internal class MinMaxSliderAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Min Max Slider", "最小最大滑动条",
                "MinMaxSlider 特性为 Vector2, Vector2Int 以及相关的数字对提供了一个滑动条，用于选择一个范围。",
                "The MinMaxSlider attribute provides a slider for Vector2, Vector2Int and other numeric pairs to select a range.",
                OdinInspectorDocumentationLinks.MinMaxSliderUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("你可以指定滑动条的范围，也可以选择是否显示数值输入框。",
                "You can specify the range of the slider and whether to show numeric input fields."),
            new BilingualData("滑动条的边界值也可以通过引用其他成员来动态确定。",
                "The boundary values of the slider can also be dynamically determined by referencing other members.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[3]
        {
            new ParameterValue(typeof(float).FullName, "MinValue",
                new BilingualData("滑动条的最小值。支持使用 $ 引用成员。",
                    "The minimum value of the slider. Supports $ for member reference.")),
            new ParameterValue(typeof(float).FullName, "MaxValue",
                new BilingualData("滑动条的最大值。支持使用 $ 引用成员。",
                    "The maximum value of the slider. Supports $ for member reference.")),
            new ParameterValue(typeof(bool).FullName, "ShowFields",
                new BilingualData("是否显示数值输入框。", "Whether to show numeric input fields."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("MinValue", ResolverType.ValueResolver, "float", "0",
                new List<ParameterValue>()),
            new ResolvedStringParameterValue("MaxValue", ResolverType.ValueResolver, "float", "1",
                new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                MinMaxSliderExampleSO.Instance)
        };
    }
}
