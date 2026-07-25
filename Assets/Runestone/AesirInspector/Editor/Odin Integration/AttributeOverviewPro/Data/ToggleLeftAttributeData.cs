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
    [Summary("ToggleLeft 特性的介绍数据，包含标题和案例预览项")]
    internal class ToggleLeftAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ToggleLeft", "ToggleLeft",
                "ToggleLeft 特性用于将 bool 字段的开关绘制在左侧，类似于 Unity 原生的 Toggle 行为。",
                "The ToggleLeft attribute is used to draw the toggle of a bool field on the left side, similar to Unity's native Toggle behavior.",
                OdinInspectorDocumentationLinks.ToggleLeftUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("ToggleLeft 使开关绘制在标签左侧，而非默认的右侧。",
                "ToggleLeft draws the toggle on the left side of the label, instead of the default right side."),
            new BilingualData("常与 EnableIf、DisableIf 等条件特性组合使用，以提供更直观的交互体验。",
                "Often combined with conditional attributes like EnableIf and DisableIf for a more intuitive interaction experience.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = null;
        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = null;

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("No Parameters",
                ToggleLeftExampleSO.Instance)
        };
    }
}
