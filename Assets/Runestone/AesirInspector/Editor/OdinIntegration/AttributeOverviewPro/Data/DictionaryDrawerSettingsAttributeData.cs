using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// DictionaryDrawerSettings 特性的介绍数据。
    /// </summary>
    internal class DictionaryDrawerSettingsAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("DictionaryDrawerSettings", "DictionaryDrawerSettings",
                "DictionaryDrawerSettings 特性用于自定义字典（Dictionary）在 Inspector 中的绘制方式。",
                "The DictionaryDrawerSettings attribute is used to customize how dictionaries are drawn in the Inspector.",
                OdinInspectorDocumentationLinks.DictionaryDrawerSettingsUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("可以自定义键（Key）和值（Value）列的标签文本。",
                "Customizes the label text for Key and Value columns."),
            new BilingualData("支持多种显示模式，如单行显示（OneLine）、折叠显示（Foldout）等。",
                "Supports multiple display modes, such as OneLine or Foldout."),
            new BilingualData("可以设置键列的固定宽度（KeyColumnWidth）。",
                "Allows setting a fixed width for the Key column."),
            new BilingualData("可以控制字典是否只读（IsReadOnly），禁止在面板中添加或删除项。",
                "Can make the dictionary read-only, preventing additions or removals in the Inspector.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "KeyLabel",
                new BilingualData("键列的标签文本。", "The label for the key column.")),
            new ParameterValue(typeof(string).FullName, "ValueLabel",
                new BilingualData("值列的标签文本。", "The label for the value column.")),
            new ParameterValue(typeof(float).FullName, "KeyColumnWidth",
                new BilingualData("键列的宽度。", "The width of the key column.")),
            new ParameterValue(typeof(DictionaryDisplayOptions).FullName, "DisplayMode",
                new BilingualData("字典的显示模式。", "The display mode for the dictionary.")),
            new ParameterValue(typeof(bool).FullName, "IsReadOnly",
                new BilingualData("是否为只读模式。", "Whether the dictionary is read-only."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeOdinSerializedExample("Basic Usage",
                DictionaryDrawerSettingsExampleSO.Instance)
        };
    }
}
