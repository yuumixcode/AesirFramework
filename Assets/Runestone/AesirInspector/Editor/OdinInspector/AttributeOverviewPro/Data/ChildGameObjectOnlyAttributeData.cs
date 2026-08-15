namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class ChildGameObjectOnlyAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ChildGameObjectOnly", "ChildGameObjectOnly",
                "ChildGameObjectOnly 特性作用于继承 Component 或者 GameObject 的字段上，在面板上绘制一个小按钮，用于选择当前物体的子物体。",
                "The ChildGameObjectOnly attribute draws a button to select a child GameObject for fields inheriting Component or GameObject.",
                "https://odininspector.com/attributes/child-game-object-only-attribute");

        public override BilingualData[] UsageTips { get; set; } = { };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "IncludeSelf",
                new BilingualData("是否包含当前物体，默认为 true。",
                    "Whether to include the current object. Defaults to true.")),
            new ParameterValue(typeof(bool).FullName, "IncludeInactive",
                new BilingualData("是否包含非激活的物体，默认为 false。",
                    "Whether to include inactive objects. Defaults to false."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ChildGameObjectOnlyExampleSO.Instance)
        };
    }
}
