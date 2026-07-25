// ----------------------------------------------------------------------------
// MIT License
//
// Copyright (c) 2026 Runestone - Yuumix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ----------------------------------------------------------------------------

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [Summary("TableColumnWidth 特性的介绍数据，包含标题、参数说明和案例预览项")]
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
