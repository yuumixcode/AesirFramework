namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// PropertySpace 特性的介绍数据。
    /// </summary>
    internal class PropertySpaceAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("PropertySpace", "PropertySpace",
                "PropertySpace 特性用于在检查器中属性的前后添加间距（像素为单位）。",
                "The PropertySpace attribute is used to add spacing (in pixels) before and after a property in the inspector.",
                OdinInspectorDocumentationLinks.PropertySpaceUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("与 Unity 原生的 [Space] 不同，[PropertySpace] 可以同时设置属性上方和下方的间距。",
                "Unlike Unity's native [Space], [PropertySpace] can set spacing both above and below a property."),
            new BilingualData("间距是以像素为单位的浮点值。", "The spacing values are floats representing pixels."),
            new BilingualData("该特性可以应用于字段、属性和方法。",
                "This attribute can be applied to fields, properties, and methods.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(float).FullName, "SpaceBefore",
                new BilingualData("属性上方的间距像素值。", "The pixel value for spacing before the property.")),
            new ParameterValue(typeof(float).FullName, "SpaceAfter",
                new BilingualData("属性下方的间距像素值。", "The pixel value for spacing after the property."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Usage Examples",
                PropertySpaceExampleSO.Instance)
        };
    }
}
