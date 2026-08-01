namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class DisableInPlayModeAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("DisableInPlayMode", "DisableInPlayMode",
                "DisableInPlayMode 特性使属性在 Play 模式下禁用。",
                "The DisableInPlayMode attribute disables a property while in Play mode.",
                OdinInspectorDocumentationLinks.DisableInPlayModeUrl);

        public override BilingualData[] UsageTips { get; set; } = { };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                DisableInPlayModeExampleSO.Instance)
        };
    }
}
