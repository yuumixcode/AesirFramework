namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// HideLabel 特性的介绍数据。
    /// </summary>
    internal class HideLabelAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("HideLabel", "HideLabel",
                "HideLabel 特性用于隐藏属性在 Inspector 中默认显示的标签（Label）。",
                "The HideLabel attribute is used to hide the default label of a property in the Inspector.",
                OdinInspectorDocumentationLinks.HideLabelUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("常用于 HorizontalGroup 中，以便让输入框占据整行或特定列的全部宽度。",
                "Commonly used in HorizontalGroups to allow input fields to occupy the entire width of a row or column."),
            new BilingualData("适用于本身就具有清晰视觉含义的属性（如颜色块、预览图等）。",
                "Ideal for properties that have clear visual meaning on their own, such as color blocks or previews."),
            new BilingualData("该特性不仅隐藏标签，还会移除标签所占用的水平空间。",
                "This attribute not only hides the label but also removes the horizontal space it would have occupied.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                HideLabelExampleSO.Instance)
        };
    }
}
