using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class DetailedInfoBoxAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("DetailedInfoBox", "DetailedInfoBox",
                "DetailedInfoBox 特性用于在属性上方绘制一个带有详细信息的可折叠消息框。",
                "The DetailedInfoBox attribute is used to draw a collapsible message box with detailed information above a property.",
                OdinInspectorDocumentationLinks.DetailedInfoBoxUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("支持双语显示主消息和详细内容。",
                "Supports bilingual display of the main message and detailed content."),
            new BilingualData("详细内容区域可以手动展开或折叠。",
                "The detailed content area can be manually expanded or collapsed."),
            new BilingualData("支持根据成员名或表达式动态解析消息和详细内容。",
                "Supports dynamic resolution of messages and detailed content based on member names or expressions.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "chinese",
                new BilingualData("默认（中文）消息框中显示的文本。",
                    "Text displayed in the default (Chinese) message box.")),
            new ParameterValue(typeof(string).FullName, "english",
                new BilingualData("英文模式下消息框中显示的文本。", "Text displayed in the message box in English mode.")),
            new ParameterValue(typeof(string).FullName, "detailsChinese",
                new BilingualData("默认（中文）详细内容文本。", "Default (Chinese) detailed content text.")),
            new ParameterValue(typeof(string).FullName, "detailsEnglish",
                new BilingualData("英文模式下详细内容文本。", "Detailed content text in English mode.")),
            new ParameterValue("InfoMessageType", "infoMessageType",
                new BilingualData("消息框的类型（Info, Warning, Error, None）。",
                    "The type of the message box (Info, Warning, Error, None).")),
            new ParameterValue(typeof(string).FullName, "visibleIf",
                new BilingualData("可选成员名或表达式，用于控制消息框是否显示。",
                    "An optional member name or expression used to control whether the message box is displayed.")),
            new ParameterValue(typeof(bool).FullName, "guiAlwaysEnabled",
                new BilingualData("即使属性被禁用，是否也始终启用消息框。",
                    "Whether the message box is always enabled even if the property is disabled."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Message", ResolverType.ValueResolver, typeof(string).FullName,
                "None", new List<ParameterValue>()),
            new ResolvedStringParameterValue("Details", ResolverType.ValueResolver, typeof(string).FullName,
                "None", new List<ParameterValue>()),
            new ResolvedStringParameterValue("Visible If", ResolverType.ValueResolver, typeof(bool).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Bilingual Usage",
                DetailInfoBoxBilingualExampleSO.Instance)
        };
    }
}
