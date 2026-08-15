namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class EnumPagingAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("EnumPaging", "EnumPaging", "EnumPaging 特性作用于枚举类型，绘制一个可循环的枚举按钮。",
                "The EnumPaging attribute draws a looping button for enum fields.",
                "https://odininspector.com/attributes/enum-paging-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("可以和其他特性结合使用，比如可以改变 Unity 编辑器当前选择的工具。",
                "Can be combined with other attributes, such as changing the currently selected Unity Editor tool.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                EnumPagingExampleSO.Instance)
        };
    }
}
