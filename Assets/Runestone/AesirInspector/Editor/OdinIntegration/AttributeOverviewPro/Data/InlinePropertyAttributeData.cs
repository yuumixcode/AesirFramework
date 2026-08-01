namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// InlineProperty 特性的介绍数据。
    /// </summary>
    internal class InlinePropertyAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("InlineProperty", "InlineProperty",
                "InlineProperty 特性用于将类或结构体的内容直接显示在其父级属性的同一行（或紧凑地显示），而不是显示为折叠组。",
                "The InlineProperty attribute is used to display the content of a class or struct inline with its parent property, instead of as a foldout group.",
                OdinInspectorDocumentationLinks.InlinePropertyUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("通常直接作用于类或结构体的定义上。",
                "Usually applied directly to the class or struct definition."),
            new BilingualData("配合 [HorizontalGroup] 和 [HideLabel] 可以实现类似于 Vector2 的单行紧凑显示效果。",
                "Works well with [HorizontalGroup] and [HideLabel] to create compact single-line layouts similar to Vector2."),
            new BilingualData("可以自定义内联属性的标签宽度（LabelWidth）。",
                "Supports customizing the label width (LabelWidth) for the inline properties.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(float).FullName, "LabelWidth",
                new BilingualData("内联属性的标签宽度。", "The label width of the inline properties."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                InlinePropertyExampleSO.Instance)
        };
    }
}
