using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// DisplayAsString 特性的介绍数据。
    /// </summary>
    internal class DisplayAsStringAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("DisplayAsString", "DisplayAsString",
                "DisplayAsString 特性将属性值绘制为简单的文本标签，而不是可编辑的输入框。",
                "The DisplayAsString attribute draws the property value as a simple text label instead of an editable input field.",
                OdinInspectorDocumentationLinks.DisplayAsStringUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("适用于只需要展示而不需要在 Inspector 中修改的字符串、数字或其他对象。",
                "Ideal for displaying strings, numbers, or other objects that should be visible but not editable in the Inspector."),
            new BilingualData("支持富文本、自定义字体大小以及对齐方式。",
                "Supports rich text, custom font sizes, and text alignment."),
            new BilingualData("配合 overflow 参数可以控制长文本的截断或换行显示。",
                "Use the overflow parameter to control whether long text should be truncated or wrapped.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "enableRichText",
                new BilingualData("是否启用富文本渲染。", "Whether to enable rich text rendering.")),
            new ParameterValue(typeof(int).FullName, "fontSize",
                new BilingualData("字体大小。", "The font size of the text.")),
            new ParameterValue(typeof(TextAlignment).FullName, "alignment",
                new BilingualData("文本对齐方式。", "The alignment of the text.")),
            new ParameterValue(typeof(bool).FullName, "overflow",
                new BilingualData("文本过长时是否允许溢出显示。如果为 false，则会进行裁剪。",
                    "Whether the text should overflow if it's too long. If false, it will be clipped."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                DisplayAsStringExampleSO.Instance)
        };
    }
}
