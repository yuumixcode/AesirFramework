namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class WrapAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Wrap", "Wrap", "Wrap 特性为数字类型的属性设置数值循环范围，当数值超出范围时会自动从另一端开始。",
                "The Wrap attribute sets a looping range for numeric properties, automatically wrapping values from one end to the other when they exceed the range.",
                "https://odininspector.com/attributes/wrap-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("支持 int、float、Vector3 等数字类型。",
                "Supports int, float, Vector3, and other numeric types."),
            new BilingualData("当数值调整超出范围时会自动循环回另一端的值。",
                "When adjusted beyond the range, values automatically wrap around to the other end."),
            new BilingualData("适用于角度、弧度等数值需要进行循环处理的场景。",
                "Ideal for angles, radians, and other values that need cyclic behavior.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(float).FullName, "min",
                new BilingualData("范围的最小值。", "The minimum value of the range.")),
            new ParameterValue(typeof(float).FullName, "max",
                new BilingualData("范围的最大值。", "The maximum value of the range."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                WrapExampleSO.Instance)
        };
    }
}
