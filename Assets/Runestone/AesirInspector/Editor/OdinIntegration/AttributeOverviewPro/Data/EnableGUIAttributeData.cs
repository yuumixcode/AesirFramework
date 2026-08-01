namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// EnableGUI 特性的介绍数据。
    /// </summary>
    internal class EnableGUIAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("EnableGUI", "EnableGUI", "强制启用 property，使其正常显示。",
                "Forces a property to be enabled so it displays normally.",
                OdinInspectorDocumentationLinks.EnableGuiUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("部分 property 是灰色显示无法修改，可以使用 [EnableGUI] 来恢复正常，仅优化显示样式。",
                "Some properties are grayed out and cannot be modified. Use [EnableGUI] to restore normal display, optimizing the appearance only.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                EnableGUIExampleSO.Instance)
        };
    }
}
