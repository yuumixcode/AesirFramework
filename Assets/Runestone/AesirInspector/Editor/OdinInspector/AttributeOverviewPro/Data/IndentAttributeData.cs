namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Indent 特性的介绍数据。
    /// </summary>
    internal class IndentAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Indent", "缩进", "Indent 特性允许在 Inspector 中对属性进行缩进。你可以指定缩进的层级。",
                "The Indent attribute allows for indenting properties in the inspector. You can specify the level of indentation.",
                OdinInspectorDocumentationLinks.IndentUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("Indent 特性对于在 Inspector 中组织属性非常有用，尤其是当某些属性在逻辑上属于上一个属性时。",
                "The Indent attribute is useful for organizing properties in the inspector, especially when some properties logically belong to a previous property."),
            new BilingualData("你可以使用负值来减少缩进。", "You can use negative values to decrease indentation.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[1]
        {
            new ParameterValue(typeof(int).FullName, "IndentLevel",
                new BilingualData("缩进的层级。默认值为 1。", "The level of indentation. Default value is 1."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = null;

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                IndentExampleSO.Instance)
        };
    }
}
