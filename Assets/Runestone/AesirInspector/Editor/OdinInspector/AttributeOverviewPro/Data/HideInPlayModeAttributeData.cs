namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class HideInPlayModeAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("HideInPlayMode", "HideInPlayMode",
                "HideInPlayMode 特性使属性在 Play 模式下隐藏。",
                "The HideInPlayMode attribute hides a property while in Play mode.",
                OdinInspectorDocumentationLinks.HideInPlayModeUrl);

        public override BilingualData[] UsageTips { get; set; } = { };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                HideInPlayModeExampleSO.Instance)
        };
    }
}
