namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// LabelWidth 特性的介绍数据。
    /// </summary>
    internal class LabelWidthAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("LabelWidth", "LabelWidth", "LabelWidth 特性用于自定义属性标签的宽度。",
                "The LabelWidth attribute is used to customize the width of property labels.",
                OdinInspectorDocumentationLinks.LabelWidthUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("可以设置具体的像素宽度，也可以设置 0 到 1 之间的比例宽度。",
                "You can set a specific pixel width or a proportional width between 0 and 1."),
            new BilingualData("常用于对齐多个属性的输入框，或者在标签文字较长时增加宽度。",
                "Commonly used to align input fields of multiple properties or to increase width for long label text."),
            new BilingualData("该特性会影响当前属性及其子属性的标签宽度。",
                "This attribute affects the label width of the current property and its children.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(float).FullName, "width",
                new BilingualData("标签的宽度。如果大于 1，则视为像素；如果小于等于 1，则视为比例。",
                    "The width of the label. If greater than 1, it's pixels; if 1 or less, it's proportional."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                LabelWidthExampleSO.Instance)
        };
    }
}
