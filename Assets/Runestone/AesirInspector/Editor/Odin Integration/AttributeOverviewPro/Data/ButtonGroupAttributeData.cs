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
    [Summary("ButtonGroup 特性的介绍数据，包含标题、参数说明、解析字符串参数和案例预览项")]
    internal class ButtonGroupAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ButtonGroup", "ButtonGroup", "ButtonGroup 特性用于将多个按钮分组并排显示在同一行中。",
                "The ButtonGroup attribute is used to group multiple buttons side-by-side in the same row.",
                "https://odininspector.com/attributes/button-group-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("通过指定相同的组名来将多个按钮归入同一组中。",
                "Group multiple buttons together by specifying the same group name."),
            new BilingualData("默认组名为 \"_DefaultGroup\"，不指定组名时自动使用。",
                "The default group name is \"_DefaultGroup\", used automatically when no group name is specified."),
            new BilingualData("Order 参数控制组内按钮的显示顺序，数值越小越靠左。",
                "The Order parameter controls the display order of buttons within a group; smaller values appear further to the left."),
            new BilingualData("ButtonHeight 参数可以自定义按钮的高度。",
                "The ButtonHeight parameter customizes the button height."),
            new BilingualData("GroupName 参数支持 $ 和 @ 字符串解析。",
                "The GroupName parameter supports $ and @ string resolution.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "groupName",
                new BilingualData("组的名称。不指定时默认使用 \"_DefaultGroup\"。",
                    "The name of the group. Defaults to \"_DefaultGroup\" when not specified.")),
            new ParameterValue(typeof(float).FullName, "order",
                new BilingualData("组内按钮的显示顺序。", "The display order of buttons within the group.")),
            new ParameterValue(typeof(int).FullName, "buttonHeight",
                new BilingualData("按钮的高度（像素）。", "The height of the button (in pixels)."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("GroupName", ResolverType.ValueResolver, typeof(string).FullName,
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
                ButtonGroupExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("GroupName",
                ButtonGroupExampleWithGroupNameSO.Instance)
        };
    }
}
