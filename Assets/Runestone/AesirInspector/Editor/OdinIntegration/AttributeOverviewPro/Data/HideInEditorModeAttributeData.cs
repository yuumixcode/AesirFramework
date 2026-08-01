namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class HideInEditorModeAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("HideInEditorMode", "HideInEditorMode",
                "HideInEditorMode 特性使属性在 Editor 模式下隐藏。",
                "The HideInEditorMode attribute hides a property while in Editor mode.",
                OdinInspectorDocumentationLinks.HideInEditorModeUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("适合运行时需要调试的数据。", "Suitable for data that needs debugging at runtime.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                HideInEditorModeExampleSO.Instance)
        };
    }
}
