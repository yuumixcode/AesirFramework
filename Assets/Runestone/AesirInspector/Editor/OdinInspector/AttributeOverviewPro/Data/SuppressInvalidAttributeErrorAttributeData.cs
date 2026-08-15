namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class SuppressInvalidAttributeErrorAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("SuppressInvalidAttributeError", "SuppressInvalidAttributeError",
                "SuppressInvalidAttributeError 特性用于抑制不适用特性产生的错误消息。",
                "The SuppressInvalidAttributeError attribute is used to suppress error messages from incompatible attributes.");

        public override BilingualData[] UsageTips { get; set; } = { };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = null;

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                SuppressInvalidAttributeErrorExampleSO.Instance)
        };
    }
}
