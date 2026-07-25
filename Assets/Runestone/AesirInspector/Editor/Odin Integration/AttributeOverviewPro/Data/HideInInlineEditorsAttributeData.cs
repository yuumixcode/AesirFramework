namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [Summary("HideInInlineEditors 特性的介绍数据，包含标题、参数说明和案例预览项")]
    internal class HideInInlineEditorsAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("HideInInlineEditors", "HideInInlineEditors",
                "HideInInlineEditors 特性使属性在 InlineEditor 中隐藏。",
                "The HideInInlineEditors attribute hides a property when displayed within an InlineEditor.",
                OdinInspectorDocumentationLinks.HideInPlayModeUrl); // 暂时借用，链接不带s

        public override BilingualData[] UsageTips { get; set; } = { };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                HideInInlineEditorsExampleSO.Instance)
        };
    }
}
