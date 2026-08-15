namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class TypeInfoBoxAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("TypeInfoBox", "TypeInfoBox", "TypeInfoBox 特性在类的内部的最上方绘制一个 InfoBox。",
                "The TypeInfoBox attribute draws an InfoBox at the top of a class.",
                "https://odininspector.com/attributes/type-info-box-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("快速绘制一个顶部的 InfoBox，不需要使用 PropertyOrder 和 OnInspectorGUI 特性。",
                "Quickly draw a top InfoBox without needing PropertyOrder and OnInspectorGUI attributes.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "message",
                new BilingualData("顶部 InfoBox 的消息内容。", "The message content of the top InfoBox."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                TypeInfoBoxExampleSO.Instance)
        };
    }
}
