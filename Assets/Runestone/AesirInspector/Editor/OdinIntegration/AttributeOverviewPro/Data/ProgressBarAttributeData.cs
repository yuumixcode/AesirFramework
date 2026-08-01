using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ProgressBar 特性的介绍数据。
    /// </summary>
    internal class ProgressBarAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Progress Bar", "进度条",
                "ProgressBar 特性在 Inspector 中绘制一个进度条。它可以用于显示数字属性的当前进度。",
                "The ProgressBar attribute draws a progress bar in the inspector. It can be used to visualize the progress of a numeric property.",
                OdinInspectorDocumentationLinks.ProgressBarUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("你可以自定义进度条的颜色、背景颜色，并选择是否在进度条上显示数值或自定义文本。",
                "You can customize the color, background color of the progress bar, and choose whether to display the numeric value or custom text on the bar."),
            new BilingualData("进度条的最大值和最小值也可以通过引用其他成员来动态确定。",
                "The maximum and minimum values of the progress bar can also be dynamically determined by referencing other members.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[4]
        {
            new ParameterValue(typeof(double).FullName, "Min",
                new BilingualData("进度条的最小值。支持使用 $ 引用成员。",
                    "The minimum value of the progress bar. Supports $ for member reference.")),
            new ParameterValue(typeof(double).FullName, "Max",
                new BilingualData("进度条的最大值。支持使用 $ 引用成员。",
                    "The maximum value of the progress bar. Supports $ for member reference.")),
            new ParameterValue(typeof(float).FullName, "R, G, B",
                new BilingualData("进度条的颜色（0-1 范围）。", "The color of the progress bar (0-1 range).")),
            new ParameterValue(typeof(string).FullName, "CustomValueString",
                new BilingualData("显示在进度条上的自定义文本。支持使用 $ 引用成员。",
                    "Custom text to display on the progress bar. Supports $ for member reference."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Min", ResolverType.ValueResolver, "double", "0",
                new List<ParameterValue>()),
            new ResolvedStringParameterValue("Max", ResolverType.ValueResolver, "double", "100",
                new List<ParameterValue>()),
            new ResolvedStringParameterValue("CustomValueString", ResolverType.ValueResolver, "string",
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ProgressBarExampleSO.Instance)
        };
    }
}
