namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ReadOnly 特性的介绍数据。
    /// </summary>
    internal class ReadOnlyAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ReadOnly", "ReadOnly", "ReadOnly 特性使属性在检查器面板中显示为只读状态。",
                "The ReadOnly attribute makes a property appear as read-only in the inspector panel.",
                OdinInspectorDocumentationLinks.ReadOnlyUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("该特性仅影响检查器面板中的编辑，代码逻辑中仍然可以修改该变量的值。",
                "This attribute only affects editing in the inspector panel; the value can still be modified in code logic."),
            new BilingualData("可以应用于字段、属性以及集合类型（如 List 或 Array）。",
                "Can be applied to fields, properties, and collection types (such as List or Array).")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Usage Examples",
                ReadOnlyExampleSO.Instance)
        };
    }
}
