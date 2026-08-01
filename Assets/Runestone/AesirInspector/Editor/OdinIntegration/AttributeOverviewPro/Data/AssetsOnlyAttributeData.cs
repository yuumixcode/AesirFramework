namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// AssetsOnly 特性的介绍数据，包含标题和案例预览项。
    /// </summary>
    internal class AssetsOnlyAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("AssetsOnly", "AssetsOnly",
                "AssetsOnly 用于 UnityEngine.Object 类型，并将 Property 限制为项目 Asset，而不是场景对象。\n" +
                "当您想要确保对象来自项目而不是场景时，请使用此项。",
                "AssetsOnly is used on object properties, and restricts the property to project assets, and not scene objects.\n" +
                "Use this when you want to ensure an object is from the project, and not from the scene.",
                OdinInspectorDocumentationLinks.AssetsOnlyUrl);

        public override BilingualData[] UsageTips { get; set; } = null;
        public override ParameterValue[] AttributeParameters { get; set; } = null;
        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = null;

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("No Parameters",
                AssetsOnlyExampleSO.Instance)
        };
    }
}
