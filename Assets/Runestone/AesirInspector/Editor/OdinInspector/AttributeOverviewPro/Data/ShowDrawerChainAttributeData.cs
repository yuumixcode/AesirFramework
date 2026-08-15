namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class ShowDrawerChainAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ShowDrawerChain", "ShowDrawerChain",
                "ShowDrawerChain 特性用于在检查器中显示属性的绘制链，便于调试。",
                "The ShowDrawerChain attribute is used to display the property's drawer chain in the inspector for debugging purposes.");

        public override BilingualData[] UsageTips { get; set; } = { };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = null;

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ShowDrawerChainExampleSO.Instance)
        };
    }
}
