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
    [Summary("RequiredListLength 特性的介绍数据，包含标题、参数说明和案例预览项")]
    internal class RequiredListLengthAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("RequiredListLength", "RequiredListLength",
                "RequiredListLength 特性用于限制列表的最小和/或最大长度。",
                "The RequiredListLength attribute is used to restrict the minimum and/or maximum length of a list.");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("可以只设置最小长度，或同时设置最小和最大长度。",
                "Can set only a minimum length, or both minimum and maximum length."),
            new BilingualData("支持使用成员变量（$ 符号）或表达式（@ 符号）解析字符串参数。",
                "Supports resolving string parameters using member references ($) or expressions (@).")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(int).FullName, "ListLength",
                new BilingualData("列表必须满足的最小/最大长度。", "The minimum/maximum length the list must satisfy.")),
            new ParameterValue(typeof(string).FullName, "Message",
                new BilingualData("验证失败时显示的自定义消息。", "Custom message displayed when validation fails."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = null;

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                RequiredListLengthExampleSO.Instance)
        };
    }
}
