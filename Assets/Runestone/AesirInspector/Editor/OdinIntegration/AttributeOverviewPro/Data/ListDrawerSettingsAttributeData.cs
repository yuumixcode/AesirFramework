namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ListDrawerSettings 特性的介绍数据。
    /// </summary>
    internal class ListDrawerSettingsAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ListDrawerSettings", "ListDrawerSettings",
                "ListDrawerSettings 特性用于自定义列表或数组在 Inspector 中的绘制方式。",
                "The ListDrawerSettings attribute is used to customize how lists or arrays are drawn in the Inspector.",
                OdinInspectorDocumentationLinks.ListDrawerSettingsUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("可以控制列表是否只读（禁止增删，但元素本身可能仍可编辑）。",
                "Can make the list read-only (preventing adding/removing, though elements themselves might remain editable)."),
            new BilingualData("支持分页显示（NumberOfItemsPerPage），适合处理超长列表。",
                "Supports paging (NumberOfItemsPerPage), ideal for very long lists."),
            new BilingualData("支持拖拽排序（DraggableItems）和隐藏添加/移除按钮。",
                "Supports draggable reordering (DraggableItems) and hiding add/remove buttons."),
            new BilingualData("可以使用 ListElementLabelName 来指定元素结构中的某个字段作为该元素的标签。",
                "Use ListElementLabelName to specify a field within the element structure to use as its label.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "IsReadOnly",
                new BilingualData("是否为只读模式。", "Whether the list is read-only.")),
            new ParameterValue(typeof(bool).FullName, "ShowFoldout",
                new BilingualData("是否显示折叠箭头。", "Whether to show the foldout arrow.")),
            new ParameterValue(typeof(bool).FullName, "ShowIndexLabels",
                new BilingualData("是否显示元素序号。", "Whether to show index labels for elements.")),
            new ParameterValue(typeof(string).FullName, "ListElementLabelName",
                new BilingualData("作为元素标签的成员名。", "The name of the member to use as the element label.")),
            new ParameterValue(typeof(int).FullName, "NumberOfItemsPerPage",
                new BilingualData("每页显示的元素数量。", "The number of items to show per page.")),
            new ParameterValue(typeof(bool).FullName, "DraggableItems",
                new BilingualData("是否允许拖拽排序。", "Whether items can be reordered by dragging.")),
            new ParameterValue(typeof(string).FullName, "ElementColor",
                new BilingualData("元素的背景颜色。", "The background color of the elements."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ListDrawerSettingsExampleSO.Instance)
        };
    }
}
