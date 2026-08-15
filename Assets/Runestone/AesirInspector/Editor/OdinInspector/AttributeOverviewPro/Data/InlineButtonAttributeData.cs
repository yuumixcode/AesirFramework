using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// InlineButton 特性的介绍数据。
    /// </summary>
    internal class InlineButtonAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("InlineButton", "InlineButton", "InlineButton 特性用于在属性值的右侧绘制一个按钮。",
                "The InlineButton attribute draws a button to the right of the property value.",
                OdinInspectorDocumentationLinks.InlineButtonUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("可以在一个属性上应用多个 InlineButton，它们会按顺序排列。",
                "You can apply multiple InlineButton attributes to a single property; they will be arranged in order."),
            new BilingualData("支持指定图标、自定义文本以及根据条件显示按钮。",
                "Supports specifying icons, custom labels, and conditional visibility."),
            new BilingualData("可以通过 ButtonColor 和 TextColor 自定义按钮的颜色。",
                "Button and text colors can be customized via ButtonColor and TextColor parameters.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "action",
                new BilingualData("点击时触发的方法名或表达式。", "The method name or expression to trigger on click.")),
            new ParameterValue(typeof(string).FullName, "label",
                new BilingualData("按钮显示的文本。默认使用方法名。",
                    "The label to display on the button. Defaults to the method name.")),
            new ParameterValue(typeof(SdfIconType).FullName, "icon",
                new BilingualData("按钮显示的图标。", "The icon to display on the button.")),
            new ParameterValue(typeof(string).FullName, "ShowIf",
                new BilingualData("控制按钮显示的条件表达式。",
                    "The condition expression that controls the visibility of the button.")),
            new ParameterValue(typeof(string).FullName, "ButtonColor",
                new BilingualData("按钮的背景颜色。", "The background color of the button.")),
            new ParameterValue(typeof(string).FullName, "TextColor",
                new BilingualData("按钮的文字颜色。", "The text color of the button."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Action", ResolverType.ActionResolver, "void", "None",
                new List<ParameterValue>()),
            new ResolvedStringParameterValue("ShowIf", ResolverType.ValueResolver, typeof(bool).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                InlineButtonExampleSO.Instance)
        };
    }
}
