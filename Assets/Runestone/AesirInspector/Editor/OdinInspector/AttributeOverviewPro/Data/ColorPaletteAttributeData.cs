using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class ColorPaletteAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ColorPalette", "ColorPalette", "ColorPalette 特性为 Color 属性提供调色板样式的绘制。",
                "The ColorPalette attribute provides a palette-style drawer for Color properties.",
                "https://odininspector.com/attributes/color-palette-attribute");

        public override BilingualData[] UsageTips { get; set; } = { };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "ShowAlpha",
                new BilingualData("是否显示 Alpha 通道，默认为 true。",
                    "Whether to show the alpha channel, defaults to true.")),
            new ParameterValue(typeof(string).FullName, "PaletteName",
                new BilingualData("调色板的名称。", "The name of the palette."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("PaletteName", ResolverType.ValueResolver,
                typeof(string).FullName, "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ColorPaletteExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("PaletteName Resolved",
                ColorPaletteExampleWithPaletteNameSO.Instance)
        };
    }
}
