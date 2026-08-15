namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// TableList 特性的介绍数据。
    /// </summary>
    internal class TableListAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("TableList", "TableList", "TableList 特性将列表或数组绘制为一个表格，每个元素的字段对应表格的列。",
                "The TableList attribute draws a list or array as a table, where each field of the element corresponds to a column.",
                OdinInspectorDocumentationLinks.TableListUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("非常适合用于展示和编辑具有多个属性的结构体或类列表。",
                "Perfect for displaying and editing lists of structs or classes with multiple properties."),
            new BilingualData("配合 [TableColumnWidth] 可以自定义列宽。",
                "Use [TableColumnWidth] on element fields to customize column widths."),
            new BilingualData("支持分页、滚动视图模式以及隐藏工具栏等配置。",
                "Supports paging, scroll-view mode, hiding the toolbar, and more."),
            new BilingualData("默认会自动展开，也可以配置为可折叠状态。",
                "Expanded by default, but can be configured to be collapsible.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(int).FullName, "NumberOfItemsPerPage",
                new BilingualData("每页显示的元素数量。", "The number of items to show per page.")),
            new ParameterValue(typeof(bool).FullName, "ShowPaging",
                new BilingualData("是否启用分页。", "Whether to enable paging.")),
            new ParameterValue(typeof(bool).FullName, "DrawScrollView",
                new BilingualData("是否使用滚动视图绘制表格。", "Whether to draw the table inside a scroll view.")),
            new ParameterValue(typeof(int).FullName, "MaxScrollViewHeight",
                new BilingualData("滚动视图的最大高度。", "The maximum height of the scroll view.")),
            new ParameterValue(typeof(bool).FullName, "HideToolbar",
                new BilingualData("是否隐藏表格顶部的工具栏（包含添加按钮和搜索框）。",
                    "Whether to hide the toolbar at the top of the table.")),
            new ParameterValue(typeof(bool).FullName, "AlwaysExpanded",
                new BilingualData("表格是否始终处于展开状态。", "Whether the table should always be expanded."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                TableListExampleSO.Instance)
        };
    }
}
