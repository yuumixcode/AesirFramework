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

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [Summary("PolymorphicDrawerSettings 特性的介绍数据，包含标题、参数说明和案例预览项")]
    internal class PolymorphicDrawerSettingsAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("PolymorphicDrawerSettings", "PolymorphicDrawerSettings",
                "PolymorphicDrawerSettings 特性用于设置多态字段的绘制样式。",
                "The PolymorphicDrawerSettings attribute configures the drawing style of polymorphic fields.",
                "https://odininspector.com/attributes/polymorphic-drawer-settings-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("Unity 默认是无法序列化多态类型的（接口、抽象类），但是可以使用 Odin 序列化。",
                "Unity cannot serialize polymorphic types (interfaces, abstract classes) by default, but Odin serialization supports them."),
            new BilingualData("对于接口一定要注意是否采用了 Odin 序列化，如果选择了 EditorOnly 序列化，则构建时将会剔除 Odin 序列化的部分。",
                "Ensure interfaces use Odin serialization. If EditorOnly serialization is selected, Odin serialization is stripped during builds."),
            new BilingualData("该 Example 采用了 Odin 序列化。", "This example uses Odin serialization.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "ShowBaseType",
                new BilingualData("是否显示基类字段，默认为 false。",
                    "Whether to display the base type field. Defaults to false.")),
            new ParameterValue(typeof(bool).FullName, "ReadOnlyIfNotNullReference",
                new BilingualData("如果引用不为空，是否只读，默认为 false。",
                    "Whether to make the field read-only if the reference is not null. Defaults to false.")),
            new ParameterValue(typeof(string).FullName, "CreateInstanceFunction",
                new BilingualData("自定义创建实例的函数名，默认为 null。",
                    "Custom function name for creating instances. Defaults to null.")),
            new ParameterValue(typeof(NonDefaultConstructorPreference).FullName,
                "NonDefaultConstructorPreference",
                new BilingualData("没有默认构造函数的处理设置。",
                    "Preference for handling types without default constructors."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeOdinSerializedExample("Basic Usage",
                PolymorphicDrawerSettingsExampleSO.Instance)
        };
    }
}
