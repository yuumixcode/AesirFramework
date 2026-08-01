namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// HorizontalGroup 特性的介绍数据。
    /// </summary>
    internal class HorizontalGroupAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("HorizontalGroup", "HorizontalGroup",
                "HorizontalGroup 特性用于将多个属性水平排列在同一行中。",
                "The HorizontalGroup attribute is used to group multiple properties horizontally in a single row.",
                OdinInspectorDocumentationLinks.HorizontalGroupUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("通过指定相同的组名来将属性归入同一个水平组。",
                "Properties are grouped together by specifying the same group name."),
            new BilingualData("支持设置固定宽度（像素）或比例宽度（0-1）。",
                "Supports setting fixed widths (pixels) or proportional widths (0-1)."),
            new BilingualData("可以嵌套其他组特性，如在水平组中嵌套 BoxGroup。",
                "Can be nested with other group attributes, such as nesting a BoxGroup inside a HorizontalGroup."),
            new BilingualData("通过 Gap 参数可以调整组内成员之间的间距。",
                "The Gap parameter allows adjusting the spacing between members in the group.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(float).FullName, "Width",
                new BilingualData("组的宽度。如果小于等于 1，则视为比例；如果大于 1，则视为像素。",
                    "The width of the group. If 1 or less, it's proportional; if greater than 1, it's pixels.")),
            new ParameterValue(typeof(int).FullName, "Gap",
                new BilingualData("组内成员之间的间距（像素）。", "The spacing between members in the group (in pixels).")),
            new ParameterValue(typeof(float).FullName, "MarginLeft",
                new BilingualData("组的左边距。", "The left margin of the group.")),
            new ParameterValue(typeof(float).FullName, "MarginRight",
                new BilingualData("组的右边距。", "The right margin of the group.")),
            new ParameterValue(typeof(string).FullName, "Title",
                new BilingualData("组的标题。", "The title of the group."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                HorizontalGroupExampleSO.Instance)
        };
    }
}
