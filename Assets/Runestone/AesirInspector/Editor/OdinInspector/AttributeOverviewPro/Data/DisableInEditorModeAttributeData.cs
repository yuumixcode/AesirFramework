namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class DisableInEditorModeAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("DisableInEditorMode", "DisableInEditorMode",
                "DisableInEditorMode 特性使属性在 Editor 模式下禁用。",
                "The DisableInEditorMode attribute disables a property while in Editor mode.",
                OdinInspectorDocumentationLinks.DisableInEditorModeUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("适合运行时需要调试的数据。", "Suitable for data that needs debugging at runtime.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                DisableInEditorModeExampleSO.Instance)
        };
    }
}
