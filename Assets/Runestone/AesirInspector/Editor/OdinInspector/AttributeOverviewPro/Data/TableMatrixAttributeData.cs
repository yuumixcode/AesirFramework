using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class TableMatrixAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("TableMatrix", "TableMatrix", "TableMatrix 特性将二维数组绘制成一个表格。",
                "The TableMatrix attribute draws a two-dimensional array as a table.",
                "https://odininspector.com/attributes/table-matrix-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("二维数组需要使用 Odin 序列化，案例继承 SerializedScriptableObject。",
                "Two-dimensional arrays require Odin serialization. Examples inherit from SerializedScriptableObject."),
            new BilingualData("默认绘制表格和代码结构的二维数组是相反的，可以使用参数 Transpose 反转。",
                "The default table layout is transposed relative to the code structure; use the Transpose parameter to invert it."),
            new BilingualData("自定义绘制元素样式要注意 UNITY_EDITOR 宏定义。",
                "When customizing element drawing styles, be mindful of UNITY_EDITOR macro definitions."),
            new BilingualData("可以拖拽更换不同行或者列的值，同时 Odin 新增了表格鼠标右键的功能。",
                "Values can be rearranged by dragging rows or columns; Odin also adds right-click table functionality.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "Transpose",
                new BilingualData("是否转置，默认为 false。", "Whether to transpose the table. Defaults to false.")),
            new ParameterValue(typeof(string).FullName, "Labels",
                new BilingualData("自定义绘制表头的方法，返回一个元组。",
                    "Custom method for drawing table headers, returning a tuple.")),
            new ParameterValue(typeof(bool).FullName, "IsReadOnly",
                new BilingualData("是否只读，默认为 false。", "Whether the table is read-only. Defaults to false.")),
            new ParameterValue(typeof(bool).FullName, "ResizableColumns",
                new BilingualData("是否可以修改列宽，默认为 true。", "Whether columns are resizable. Defaults to true.")),
            new ParameterValue(typeof(string).FullName, "HorizontalTitle",
                new BilingualData("横向标题。", "The horizontal title.")),
            new ParameterValue(typeof(string).FullName, "VerticalTitle",
                new BilingualData("纵向标题。", "The vertical title.")),
            new ParameterValue(typeof(int).FullName, "RowHeight",
                new BilingualData("行高。", "The row height.")),
            new ParameterValue(typeof(bool).FullName, "SquareCells",
                new BilingualData("是否使单元格保持正方形，默认为 false。",
                    "Whether to keep cells square. Defaults to false.")),
            new ParameterValue(typeof(bool).FullName, "HideColumnIndices",
                new BilingualData("隐藏绘制图表的列标。", "Hides column indices in the table.")),
            new ParameterValue(typeof(bool).FullName, "HideRowIndices",
                new BilingualData("隐藏绘制图表的行标。", "Hides row indices in the table.")),
            new ParameterValue(typeof(bool).FullName, "RespectIndentLevel",
                new BilingualData("绘制的表是否应遵循当前 GUI 缩进级别。",
                    "Whether the table should respect the current GUI indent level.")),
            new ParameterValue(typeof(string).FullName, "DrawElementMethod",
                new BilingualData("自定义绘制二维数组中的元素样式。", "Custom method for drawing elements in the 2D array."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("DrawElementMethod", ResolverType.ValueResolver,
                typeof(string).FullName, "None", new List<ParameterValue>()),
            new ResolvedStringParameterValue("HorizontalTitle", ResolverType.ValueResolver,
                typeof(string).FullName, "None", new List<ParameterValue>()),
            new ResolvedStringParameterValue("VerticalTitle", ResolverType.ValueResolver,
                typeof(string).FullName, "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeOdinSerializedExample("Basic Usage",
                TableMatrixExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeOdinSerializedExample("DrawElementMethod Resolved",
                TableMatrixExampleWithDrawElementMethodSO.Instance),
            new AttributeExamplePreviewItem().InitializeOdinSerializedExample("HorizontalTitle Resolved",
                TableMatrixExampleWithHorizontalTitleSO.Instance)
        };
    }
}
