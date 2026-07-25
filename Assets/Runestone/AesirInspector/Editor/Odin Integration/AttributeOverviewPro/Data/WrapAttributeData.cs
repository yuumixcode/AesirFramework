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
    [Summary("Wrap 特性的介绍数据，包含标题、参数说明和案例预览项")]
    internal class WrapAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Wrap", "Wrap", "Wrap 特性为数字类型的属性设置数值循环范围，当数值超出范围时会自动从另一端开始。",
                "The Wrap attribute sets a looping range for numeric properties, automatically wrapping values from one end to the other when they exceed the range.",
                "https://odininspector.com/attributes/wrap-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("支持 int、float、Vector3 等数字类型。",
                "Supports int, float, Vector3, and other numeric types."),
            new BilingualData("当数值调整超出范围时会自动循环回另一端的值。",
                "When adjusted beyond the range, values automatically wrap around to the other end."),
            new BilingualData("适用于角度、弧度等数值需要进行循环处理的场景。",
                "Ideal for angles, radians, and other values that need cyclic behavior.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(float).FullName, "min",
                new BilingualData("范围的最小值。", "The minimum value of the range.")),
            new ParameterValue(typeof(float).FullName, "max",
                new BilingualData("范围的最大值。", "The maximum value of the range."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                WrapExampleSO.Instance)
        };
    }
}
