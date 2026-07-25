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
    [Summary("TypeSelectorSettings 特性的介绍数据，包含标题、参数说明和案例预览项")]
    internal class TypeSelectorSettingsAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("TypeSelectorSettings", "TypeSelectorSettings",
                "TypeSelectorSettings 特性为使用 Odin 绘制的类型选择器提供选项。",
                "The TypeSelectorSettings attribute provides options for Odin's type selector.",
                "https://odininspector.com/attributes/type-selector-settings-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("Odin 对于 TypeSelectorSettings 有全局设置，如果另外设置覆盖，则将使用全局设置。",
                "Odin has global TypeSelectorSettings. If additional overrides are set, the global settings are used.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "ShowCategories",
                new BilingualData("是否显示类型分组。", "Whether to show type categories.")),
            new ParameterValue(typeof(bool).FullName, "PreferNamespaces",
                new BilingualData("指定是否优先使用命名空间而不是程序集类别名称。",
                    "Whether to prefer namespaces over assembly category names.")),
            new ParameterValue(typeof(bool).FullName, "ShowNoneItem",
                new BilingualData("指定是否显示 '<none>' 项。", "Whether to show the '<none>' item.")),
            new ParameterValue(typeof(string).FullName, "FilterTypesFunction",
                new BilingualData("自定义类型过滤函数，Func<Type, bool>，参数为类型，返回值为 bool，表示是否显示该类型。",
                    "Custom type filter function, Func<Type, bool>, parameter is the type, return value indicates whether to show the type."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                TypeSelectorSettingsExampleSO.Instance)
        };
    }
}
