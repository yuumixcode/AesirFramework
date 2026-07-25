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

using System;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [Summary("TypeDrawerSettings 特性的介绍数据，包含标题、参数说明和案例预览项")]
    internal class TypeDrawerSettingsAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("TypeDrawerSettings", "TypeDrawerSettings",
                "TypeDrawerSettings 特性用于设置 Type 类型的绘制样式。",
                "The TypeDrawerSettings attribute configures the drawing style for Type fields.",
                "https://odininspector.com/attributes/type-drawer-settings-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("Type 类型 Unity 无法直接序列化，可以使用 Odin 序列化，通常 Type 类型需要显示在 Inspector 面板上时是用于编辑器工具的。",
                "Unity cannot directly serialize Type. Use Odin serialization. Type fields in the Inspector are typically for editor tools."),
            new BilingualData("该 Example 采用了 Odin 序列化。", "This example uses Odin serialization.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(Type).FullName, "BaseType",
                new BilingualData("基类类型，用于限制可选择的类型范围。",
                    "The base type to filter which types are selectable.")),
            new ParameterValue(typeof(TypeInclusionFilter).FullName, "Filter",
                new BilingualData("过滤器，默认为 TypeInclusionFilter.IncludeAll。",
                    "The type inclusion filter. Defaults to TypeInclusionFilter.IncludeAll."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                TypeDrawerSettingsExampleSO.Instance)
        };
    }
}
