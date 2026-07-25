namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class DelayedPropertyAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("DelayedProperty", "DelayedProperty",
                "DelayedProperty 特性延迟属性值的更新，直到用户按下回车键或输入框失去焦点。",
                "The DelayedProperty attribute delays the update of a property value until the user presses enter or the input field loses focus.",
                OdinInspectorDocumentationLinks.DelayedUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("适用于需要通过 OnValueChanged 触发昂贵操作的场景，避免在打字过程中频繁触发。",
                "Ideal for scenarios where OnValueChanged triggers expensive operations, avoiding frequent triggers while typing."),
            new BilingualData("该特性与 Unity 自带的 [Delayed] 特性功能一致，但 Odin 对其提供了更好的集成支持。",
                "This attribute functions similarly to Unity's built-in [Delayed] attribute but offers better integration with Odin.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                DelayedPropertyExampleSO.Instance)
        };
    }
}
