namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class ToggleLeftAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ToggleLeft", "ToggleLeft",
                "ToggleLeft 特性用于将 bool 字段的开关绘制在左侧，类似于 Unity 原生的 Toggle 行为。",
                "The ToggleLeft attribute is used to draw the toggle of a bool field on the left side, similar to Unity's native Toggle behavior.",
                OdinInspectorDocumentationLinks.ToggleLeftUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("ToggleLeft 使开关绘制在标签左侧，而非默认的右侧。",
                "ToggleLeft draws the toggle on the left side of the label, instead of the default right side."),
            new BilingualData("常与 EnableIf、DisableIf 等条件特性组合使用，以提供更直观的交互体验。",
                "Often combined with conditional attributes like EnableIf and DisableIf for a more intuitive interaction experience.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = null;
        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = null;

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("No Parameters",
                ToggleLeftExampleSO.Instance)
        };
    }
}
