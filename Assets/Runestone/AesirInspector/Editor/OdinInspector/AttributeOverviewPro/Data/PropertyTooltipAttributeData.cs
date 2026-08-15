using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// PropertyTooltip 特性的介绍数据。
    /// </summary>
    internal class PropertyTooltipAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("PropertyTooltip", "PropertyTooltip",
                "PropertyTooltip 特性用于为属性添加提示信息，当鼠标悬停在属性标签上时显示。",
                "The PropertyTooltip attribute adds a tooltip to a property, shown when the mouse hovers over the property label.",
                OdinInspectorDocumentationLinks.PropertyTooltipUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("支持静态字符串、引用成员（$）以及 Odin 表达式（@）。",
                "Supports static strings, member references ($), and Odin expressions (@)."),
            new BilingualData("与 Unity 自带的 [Tooltip] 类似，但增加了对 Odin 动态解析器的支持。",
                "Similar to Unity's built-in [Tooltip], but adds support for Odin's dynamic resolvers.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "tooltip",
                new BilingualData("提示信息文本或表达式。", "The tooltip text or expression."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Tooltip", ResolverType.ValueResolver, typeof(string).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                PropertyTooltipExampleSO.Instance)
        };
    }
}
