using System.Collections.Generic;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// GUIColor 特性的介绍数据，包含标题、参数说明、解析字符串参数和案例预览项。
    /// </summary>
    internal class GUIColorAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("GUIColor", "GUIColor", "GUIColor 特性用于改变 GUI 元素的颜色。它可以用于突出特殊的、重要的字段。",
                "GUIColor is used to change the color of GUI elements. It can be used to highlight special or important fields.",
                OdinInspectorDocumentationLinks.GuiColorUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("支持多种颜色定义方式，包括颜色名、Hex 码、RGBA 格式和表达式。",
                "Supports various color definition methods, including color names, hex codes, RGBA format, and expressions."),
            new BilingualData("使用 $ 符号引用成员名时，Rider 可以提供智能补全和高亮。",
                "When referencing member names with the $ symbol, Rider provides autocomplete and highlighting."),
            new BilingualData("颜色表达式可以是三元表达式，用于根据条件动态改变颜色。",
                "Color expressions can be ternary expressions, used to dynamically change colors based on conditions.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[5]
        {
            new ParameterValue(typeof(float).FullName, "r",
                new BilingualData("红色通道 (0-1)。", "Red channel (0-1).")),
            new ParameterValue(typeof(float).FullName, "g",
                new BilingualData("绿色通道 (0-1)。", "Green channel (0-1).")),
            new ParameterValue(typeof(float).FullName, "b",
                new BilingualData("蓝色通道 (0-1)。", "Blue channel (0-1).")),
            new ParameterValue(typeof(float).FullName, "a",
                new BilingualData("Alpha 通道 (0-1)。", "Alpha channel (0-1).")),
            new ParameterValue(typeof(string).FullName, "getColor",
                new BilingualData(
                    "支持多种颜色格式，包括命名颜色（例如 \"red\"、\"orange\"、\"green\"、\"blue\"）、十六进制代码（例如 \"#FF0000\" 和 \"#FF0000FF\"）以及 RGBA（例如 \"RGBA(1,1,1,1)\"）或 RGB（例如 \"RGB(1,1,1)\"），包括 Odin 特性表达式（例如 \"@this.MyColor\"）。以下是可用的命名颜色：black, blue, clear, cyan, gray, green, grey, magenta, orange, purple, red, transparent, transparentBlack, transparentWhite, white, yellow, lightblue, lightcyan, lightgray, lightgreen, lightgrey, lightmagenta, lightorange, lightpurple, lightred, lightyellow, darkblue, darkcyan, darkgray, darkgreen, darkgrey, darkmagenta, darkorange, darkpurple, darkred, darkyellow。",
                    "Supports a variety of color formats, including named colors (e.g. \"red\", \"orange\", \"green\", \"blue\"), hex codes (e.g. \"#FF0000\" and \"#FF0000FF\"), and RGBA (e.g. \"RGBA(1,1,1,1)\") or RGB (e.g. \"RGB(1,1,1)\"), including Odin attribute expressions (e.g \"@this.MyColor\"). Here are the available named colors: black, blue, clear, cyan, gray, green, grey, magenta, orange, purple, red, transparent, transparentBlack, transparentWhite, white, yellow, lightblue, lightcyan, lightgray, lightgreen, lightgrey, lightmagenta, lightorange, lightpurple, lightred, lightyellow, darkblue, darkcyan, darkgray, darkgreen, darkgrey, darkmagenta, darkorange, darkpurple, darkred, darkyellow."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Get Color", ResolverType.ValueResolver, typeof(Color).FullName,
                "None", new List<ParameterValue>
                {
                    new ParameterValue("T", "$value",
                        new BilingualData("应用此特性的成员的值。",
                            "The value of the member that has the attribute applied to it."))
                })
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Parameters",
                GUIColorExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("GetColor",
                GUIColorExampleWithColorSO.Instance)
        };
    }
}
