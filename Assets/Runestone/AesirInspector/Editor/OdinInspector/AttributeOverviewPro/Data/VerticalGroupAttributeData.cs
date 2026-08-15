namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// VerticalGroup 特性的介绍数据。
    /// </summary>
    internal class VerticalGroupAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("VerticalGroup", "VerticalGroup", "VerticalGroup 特性用于将多个属性垂直排列在一个组中。",
                "The VerticalGroup attribute is used to group multiple properties vertically.",
                OdinInspectorDocumentationLinks.VerticalGroupUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("通常用于在 HorizontalGroup 中创建子列。",
                "Commonly used to create sub-columns within a HorizontalGroup."),
            new BilingualData("可以设置 Padding 来调整组内的上下边距。",
                "Supports setting Padding to adjust top and bottom margins within the group."),
            new BilingualData("与 HorizontalGroup 配合使用可以构建复杂的布局。",
                "Used in conjunction with HorizontalGroup to build complex layouts.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(float).FullName, "PaddingTop",
                new BilingualData("顶边距。", "The top padding of the group.")),
            new ParameterValue(typeof(float).FullName, "PaddingBottom",
                new BilingualData("底边距。", "The bottom padding of the group."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                VerticalGroupExampleSO.Instance)
        };
    }
}
