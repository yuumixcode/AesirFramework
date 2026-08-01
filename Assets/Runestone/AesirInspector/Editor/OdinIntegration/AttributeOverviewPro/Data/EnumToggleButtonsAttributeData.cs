namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// EnumToggleButtons 特性的介绍数据。
    /// </summary>
    internal class EnumToggleButtonsAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("EnumToggleButtons", "EnumToggleButtons",
                "EnumToggleButtons 特性将枚举绘制为一排按钮，提供更直观的交互体验。",
                "The EnumToggleButtons attribute draws an enum as a set of horizontal toggle buttons, providing a more intuitive interaction.",
                OdinInspectorDocumentationLinks.EnumToggleButtonsUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("支持带有 [Flags] 特性的位掩码枚举，可以进行多选。",
                "Supports bitmask enums with the [Flags] attribute, allowing multiple selection."),
            new BilingualData("配合 [HideLabel] 可以让按钮填满整行。",
                "Works well with [HideLabel] to make the buttons fill the entire row width."),
            new BilingualData("可以使用 [LabelText] 为每个枚举项设置图标或自定义显示文本。",
                "You can use [LabelText] to set icons or custom display text for each enum member.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            // EnumToggleButtons 没有公开参数
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                EnumToggleButtonsExampleSO.Instance)
        };
    }
}
