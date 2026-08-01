namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// BoxGroup 特性的介绍数据。
    /// </summary>
    internal class BoxGroupAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("BoxGroup", "BoxGroup", "BoxGroup 特性用于将多个属性包裹在一个带有边框和可选标题的盒子中。",
                "The BoxGroup attribute is used to group multiple properties inside a box with a border and an optional title.",
                OdinInspectorDocumentationLinks.BoxGroupUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("通过组名将属性归入同一个盒子中。如果组名包含路径（如 'Parent/Child'），则会创建嵌套组。",
                "Groups properties into a box by group name. Path-based names (e.g., 'Parent/Child') create nested groups."),
            new BilingualData("可以控制是否显示标题、标题是否居中显示。",
                "Supports showing or hiding the title, and centering the title."),
            new BilingualData("常用于将功能相关的属性进行视觉上的归类。", "Commonly used to visually group related properties.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "showLabel",
                new BilingualData("是否显示组标题。默认值为 true。", "Whether to show the group label. Default is true.")),
            new ParameterValue(typeof(bool).FullName, "centerLabel",
                new BilingualData("标题是否居中显示。", "Whether to center the label.")),
            new ParameterValue(typeof(string).FullName, "LabelText",
                new BilingualData("自定义显示的标题文本。", "Custom label text to display."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                BoxGroupExampleSO.Instance)
        };
    }
}
