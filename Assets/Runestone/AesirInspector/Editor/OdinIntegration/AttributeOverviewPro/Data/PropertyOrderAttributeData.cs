namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// PropertyOrder 特性的介绍数据。
    /// </summary>
    internal class PropertyOrderAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("PropertyOrder", "PropertyOrder",
                "PropertyOrder 特性用于自定义检查器中属性和方法的绘制顺序。",
                "The PropertyOrder attribute is used to customize the drawing order of properties and methods in the inspector.",
                OdinInspectorDocumentationLinks.PropertyOrderUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("默认的绘制顺序（Order）通常是 0。数字越小越优先绘制（可以在最上方），数字越大越靠后绘制。",
                "The default drawing order is usually 0. Smaller numbers are drawn first (at the top), while larger numbers are drawn later."),
            new BilingualData("可以使用负数来确保某些属性始终在最上方显示。",
                "Negative numbers can be used to ensure certain properties always appear at the top."),
            new BilingualData("不仅适用于字段和属性，也适用于 [Button] 方法和 [OnInspectorGUI] 方法。",
                "It applies not only to fields and properties but also to [Button] methods and [OnInspectorGUI] methods.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(float).FullName, "order",
                new BilingualData("绘制顺序的数值。默认为 0。", "The numeric value of the drawing order. Defaults to 0."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Usage Examples",
                PropertyOrderExampleSO.Instance)
        };
    }
}
