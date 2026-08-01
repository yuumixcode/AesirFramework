using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// LabelText 特性的介绍数据。
    /// </summary>
    internal class LabelTextAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("LabelText", "LabelText", "LabelText 特性用于更改属性在检查器中显示的标签名称。",
                "The LabelText attribute is used to change the label name of a property displayed in the inspector.",
                OdinInspectorDocumentationLinks.LabelTextUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("支持使用 $ 符号引用成员变量作为标签内容。",
                "Supports using the $ symbol to reference member variables as label content."),
            new BilingualData("支持使用 @ 符号编写 C# 表达式来动态生成标签内容。",
                "Supports using the @ symbol to write C# expressions to dynamically generate label content."),
            new BilingualData("可以设置 nicifyText 参数来自动优化变量名为更美观的显示方式。",
                "The nicifyText parameter can be set to automatically optimize variable names for a more aesthetic display."),
            new BilingualData("支持 SdfIconType 图标和颜色设置。", "Supports SdfIconType icons and color settings.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "text",
                new BilingualData("要在检查器中显示的标签文本。", "The label text to be displayed in the inspector.")),
            new ParameterValue(typeof(bool).FullName, "nicifyText",
                new BilingualData("是否优化文本显示（例如将 m_myField 转换为 My Field）。默认为 false。",
                    "Whether to nicify the text display (e.g., converting m_myField to My Field). Defaults to false.")),
            new ParameterValue("SdfIconType", "icon",
                new BilingualData("要在标签旁显示的图标。", "The icon to be displayed next to the label.")),
            new ParameterValue(typeof(string).FullName, "IconColor",
                new BilingualData("图标的颜色。支持命名颜色、十六进制和 RGBA 表达式。",
                    "The color of the icon. Supports named colors, hex, and RGBA expressions."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Text", ResolverType.ValueResolver, typeof(string).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Usage Examples",
                LabelTextExampleSO.Instance)
        };
    }
}
