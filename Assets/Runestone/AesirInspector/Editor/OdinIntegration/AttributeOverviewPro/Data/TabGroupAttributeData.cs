using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// TabGroup 特性的介绍数据。
    /// </summary>
    internal class TabGroupAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("TabGroup", "TabGroup", "TabGroup 特性用于将多个属性组织在不同的页签（Tabs）中。",
                "The TabGroup attribute is used to organize multiple properties into different tabs.",
                OdinInspectorDocumentationLinks.TabGroupUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("通过组名和页签名称来组织页签。具有相同组名的属性会被放在同一个页签栏中。",
                "Organizes tabs by group name and tab name. Properties with the same group name will be placed in the same tab bar."),
            new BilingualData("支持设置页签图标、文字颜色以及是否使用固定高度。",
                "Supports setting tab icons, text colors, and whether to use fixed heights for all tabs in the group."),
            new BilingualData("可以通过 TabLayouting 参数控制页签的排列方式（如多行排列或收缩排列）。",
                "The TabLayouting parameter controls how tabs are laid out (e.g., MultiRow or Shrink).")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "tab",
                new BilingualData("页签名称。", "The name of the tab.")),
            new ParameterValue(typeof(bool).FullName, "useFixedHeight",
                new BilingualData("是否为所有页签使用固定高度。默认值为 true。",
                    "Whether to use a fixed height for all tabs in the group. Default is true.")),
            new ParameterValue(typeof(SdfIconType).FullName, "icon",
                new BilingualData("页签显示的图标。", "The icon to display on the tab.")),
            new ParameterValue(typeof(string).FullName, "TextColor",
                new BilingualData("页签文本颜色。", "The text color of the tab."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                TabGroupExampleSO.Instance)
        };
    }
}
