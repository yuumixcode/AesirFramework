namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class TableColumnWidthAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("TableColumnWidth", "TableColumnWidth",
                "TableColumnWidth 特性用于设置 TableList 特性标记的 List 的元素宽度。",
                "The TableColumnWidth attribute sets the width of elements in a TableList-marked list.",
                "https://odininspector.com/attributes/table-column-width-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("列表必须标记 [TableList]，列表元素封装为一个类对象，在元素内部的字段上标记 [TableColumnWidth]。",
                "The list must be marked with [TableList]. Wrap list elements in a class and mark inner fields with [TableColumnWidth].")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(int).FullName, "width",
                new BilingualData("宽度，单位为像素。", "The width in pixels.")),
            new ParameterValue(typeof(bool).FullName, "resizable",
                new BilingualData("是否允许调整宽度，默认为 true。", "Whether the width is resizable. Defaults to true."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } = { };
    }
}
