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
    [Summary("ColorPalette 特性的介绍数据，包含标题、参数说明、解析字符串参数和案例预览项")]
    internal class ColorPaletteAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ColorPalette", "ColorPalette", "ColorPalette 特性为 Color 属性提供调色板样式的绘制。",
                "The ColorPalette attribute provides a palette-style drawer for Color properties.",
                "https://odininspector.com/attributes/color-palette-attribute");

        public override BilingualData[] UsageTips { get; set; } = { };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "ShowAlpha",
                new BilingualData("是否显示 Alpha 通道，默认为 true。",
                    "Whether to show the alpha channel, defaults to true.")),
            new ParameterValue(typeof(string).FullName, "PaletteName",
                new BilingualData("调色板的名称。", "The name of the palette."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("PaletteName", ResolverType.ValueResolver,
                typeof(string).FullName, "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ColorPaletteExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("PaletteName Resolved",
                ColorPaletteExampleWithPaletteNameSO.Instance)
        };
    }
}
