namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class ShowPropertyResolverAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ShowPropertyResolver", "ShowPropertyResolver",
                "ShowPropertyResolver 特性用于在检查器中显示属性的解析器信息，便于调试。",
                "The ShowPropertyResolver attribute is used to display the property's resolver information in the inspector for debugging purposes.");

        public override BilingualData[] UsageTips { get; set; } = { };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = null;

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ShowPropertyResolverExampleSO.Instance)
        };
    }
}
