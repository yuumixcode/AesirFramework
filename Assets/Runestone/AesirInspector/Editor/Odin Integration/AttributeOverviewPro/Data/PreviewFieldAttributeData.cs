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

using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [Summary("PreviewField 特性的介绍数据，包含标题、参数说明和案例预览项")]
    internal class PreviewFieldAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("PreviewField", "PreviewField",
                "PreviewField 特性绘制一个正方形的 Preview 预览框，代替原有的 ObjectField，默认支持拖拽。",
                "The PreviewField attribute draws a square preview box instead of the default ObjectField, with drag-and-drop support.",
                "https://odininspector.com/attributes/preview-field-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("默认支持拖拽，可以使用全局快捷键：Ctrl + 点击 = 删除实例，直接拖拽 = 交换或移动，Ctrl + 拖拽并放下 = 覆盖。",
                "Supports drag-and-drop by default: Ctrl + Click = delete instance, drag = swap or move, Ctrl + drag and drop = replace.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "previewGetter",
                new BilingualData("可以渲染一个 Object 的 Preview 预览框，主要是用于渲染 Texture。",
                    "A getter that renders an Object preview, primarily used for rendering Textures.")),
            new ParameterValue(typeof(float).FullName, "height",
                new BilingualData("渲染框的高度。", "The height of the preview box.")),
            new ParameterValue(typeof(ObjectFieldAlignment).FullName, "alignment",
                new BilingualData("对齐样式。", "The alignment style.")),
            new ParameterValue(typeof(FilterMode).FullName, "filterMode",
                new BilingualData("纹理的过滤模式，有 Point、Bilinear、Trilinear。",
                    "The texture filter mode: Point, Bilinear, or Trilinear."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                PreviewFieldExampleSO.Instance)
        };
    }
}
