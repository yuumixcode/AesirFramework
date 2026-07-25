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
    [Summary("TypeInfoBox 特性的介绍数据，包含标题、参数说明和案例预览项")]
    internal class TypeInfoBoxAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("TypeInfoBox", "TypeInfoBox", "TypeInfoBox 特性在类的内部的最上方绘制一个 InfoBox。",
                "The TypeInfoBox attribute draws an InfoBox at the top of a class.",
                "https://odininspector.com/attributes/type-info-box-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("快速绘制一个顶部的 InfoBox，不需要使用 PropertyOrder 和 OnInspectorGUI 特性。",
                "Quickly draw a top InfoBox without needing PropertyOrder and OnInspectorGUI attributes.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "message",
                new BilingualData("顶部 InfoBox 的消息内容。", "The message content of the top InfoBox."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                TypeInfoBoxExampleSO.Instance)
        };
    }
}
