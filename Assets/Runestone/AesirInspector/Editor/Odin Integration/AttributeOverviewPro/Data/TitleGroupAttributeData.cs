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

using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [Summary("TitleGroup 特性的介绍数据，包含标题、参数说明、解析字符串参数和案例预览项")]
    internal class TitleGroupAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("TitleGroup", "TitleGroup",
                "TitleGroup 特性用于创建一个带标题的组，可以将多个属性组织在同一个标题下。",
                "The TitleGroup attribute is used to create a titled group that organizes multiple properties under a single title header.",
                "https://odininspector.com/attributes/title-group-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("通过指定相同的组名将多个属性归入同一个标题组。支持路径嵌套（如 'Parent/Child'）。",
                "Group multiple properties under the same titled group by specifying the same group name. Supports path-based nesting (e.g., 'Parent/Child')."),
            new BilingualData("支持子标题、对齐方式、粗体、水平分割线以及缩进等显示样式控制。",
                "Supports subtitle, alignment, bold text, horizontal line, and indent display style options."),
            new BilingualData("可以使用 order 参数控制多个标题组之间的显示顺序。",
                "The order parameter controls the display order among multiple title groups.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "title",
                new BilingualData("主标题文本。", "The main title text.")),
            new ParameterValue(typeof(string).FullName, "subtitle",
                new BilingualData("子标题文本，显示在主标题下方。", "The subtitle text displayed below the main title.")),
            new ParameterValue("TitleAlignments", "alignment",
                new BilingualData("标题的对齐方式（Left, Centered, Right, Split）。",
                    "The alignment of the title (Left, Centered, Right, Split).")),
            new ParameterValue(typeof(bool).FullName, "horizontalLine",
                new BilingualData("是否在标题下方显示水平分割线。",
                    "Whether to display a horizontal line below the title.")),
            new ParameterValue(typeof(bool).FullName, "boldTitle",
                new BilingualData("标题是否加粗显示。", "Whether the title should be displayed in bold.")),
            new ParameterValue(typeof(bool).FullName, "indent",
                new BilingualData("标题内容是否缩进。", "Whether the title content should be indented.")),
            new ParameterValue(typeof(float).FullName, "order",
                new BilingualData("标题组在检查器中的显示顺序。", "The display order of the title group in the inspector."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Group Name", ResolverType.ValueResolver,
                typeof(string).FullName, "None", new List<ParameterValue>
                {
                    new ParameterValue("T", "$value",
                        new BilingualData("应用此特性的成员的值。",
                            "The value of the member that has the attribute applied to it."))
                }),
            new ResolvedStringParameterValue("Subtitle", ResolverType.ValueResolver, typeof(string).FullName,
                "None", new List<ParameterValue>
                {
                    new ParameterValue("T", "$value",
                        new BilingualData("应用此特性的成员的值。",
                            "The value of the member that has the attribute applied to it."))
                })
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                TitleGroupExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("GroupName",
                TitleGroupExampleWithGroupNameSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Subtitle",
                TitleGroupExampleWithSubtitleSO.Instance)
        };
    }
}
