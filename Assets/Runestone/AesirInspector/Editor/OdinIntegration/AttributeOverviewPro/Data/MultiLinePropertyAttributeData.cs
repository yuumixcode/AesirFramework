namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// MultiLineProperty 特性的介绍数据。
    /// </summary>
    internal class MultiLinePropertyAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("MultiLineProperty", "MultiLineProperty",
                "MultiLineProperty 特性用于创建多行文本输入区域，适用于较长的字符串编辑。",
                "The MultiLineProperty attribute creates a multi-line text input area for editing longer strings.",
                "https://odininspector.com/attributes/multi-line-property-attribute");

        public override BilingualData[] UsageTips { get; set; } = { };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                MultiLinePropertyExampleSO.Instance)
        };
    }
}
