namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// FoldoutGroup 特性的介绍数据。
    /// </summary>
    internal class FoldoutGroupAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("FoldoutGroup", "FoldoutGroup", "FoldoutGroup 特性用于将多个属性组织在一个可折叠的组中。",
                "The FoldoutGroup attribute is used to group multiple properties inside a collapsible foldout.",
                OdinInspectorDocumentationLinks.FoldoutGroupUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("通过组名将属性归入同一个折叠组。支持路径嵌套（如 'Parent/Child'）。",
                "Groups properties into a foldout by group name. Supports nested paths (e.g., 'Parent/Child')."),
            new BilingualData("可以设置默认是否展开（expanded 参数）。",
                "Can be configured to be initially expanded or collapsed via the expanded parameter."),
            new BilingualData("常用于收纳不常用或次要的属性，保持 Inspector 面板整洁。",
                "Commonly used to hide less frequently used or secondary properties to keep the Inspector clean.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "expanded",
                new BilingualData("组是否在初始状态下展开。", "Whether the group should be expanded by default.")),
            new ParameterValue(typeof(bool).FullName, "HasDefinedExpanded",
                new BilingualData("内部使用，标识是否显式设置了展开状态。",
                    "Internal use, indicates if the expanded state has been explicitly defined."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                FoldoutGroupExampleSO.Instance)
        };
    }
}
