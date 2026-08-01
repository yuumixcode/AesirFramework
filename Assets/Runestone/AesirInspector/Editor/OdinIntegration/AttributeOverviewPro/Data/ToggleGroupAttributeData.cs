using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class ToggleGroupAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ToggleGroup", "ToggleGroup",
                "ToggleGroup 特性用于为 Toggle 类型属性创建一个可折叠的组，切换开关时将展开或折叠组内容。",
                "The ToggleGroup attribute is used to create a collapsible group for toggle-type properties, expanding or collapsing content when the toggle is switched.",
                "https://odininspector.com/attributes/toggle-group-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("ToggleGroup 需要指定一个布尔类型的属性作为开关。",
                "ToggleGroup requires specifying a boolean property as the toggle control."),
            new BilingualData("支持自定义组的标题、标题对齐方式、粗体和水平分割线。",
                "Supports customizing the group title, title alignment, bold text, and horizontal line."),
            new BilingualData("CollapseOthersOnExpand 参数可使展开当前组时自动折叠其他组。",
                "The CollapseOthersOnExpand parameter automatically collapses other groups when expanding the current one."),
            new BilingualData("Order 参数控制多个 Toggle 组之间的排序。",
                "The Order parameter controls the sorting order among multiple Toggle groups.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "groupPath",
                new BilingualData("组的路径名称。", "The path name of the group.")),
            new ParameterValue(typeof(string).FullName, "toggleGroupTitle",
                new BilingualData("组的标题文本（默认与字段名相同）。",
                    "The title text of the group (defaults to the field name).")),
            new ParameterValue("TitleAlignments", "toggleGroupTitleAlignment",
                new BilingualData("标题的对齐方式。", "The alignment of the title.")),
            new ParameterValue(typeof(bool).FullName, "toggleGroupTitleBold",
                new BilingualData("标题是否加粗。", "Whether the title is bold.")),
            new ParameterValue(typeof(bool).FullName, "toggleGroupTitleHorizontalLine",
                new BilingualData("标题下方是否显示水平线。", "Whether to show a horizontal line below the title.")),
            new ParameterValue(typeof(float).FullName, "order",
                new BilingualData("组在 Inspector 中的显示顺序。",
                    "The display order of the group in the Inspector.")),
            new ParameterValue(typeof(bool).FullName, "collapseOthersOnExpand",
                new BilingualData("展开时是否自动折叠其他组。", "Whether to collapse other groups when expanding."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("ToggleGroupTitle", ResolverType.ValueResolver,
                typeof(string).FullName, "None", new List<ParameterValue>
                {
                    new ParameterValue("T", "$value",
                        new BilingualData("应用此特性的成员的值。",
                            "The value of the member that has the attribute applied to it."))
                })
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ToggleGroupExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("ToggleGroupTitle",
                ToggleGroupExampleWithToggleGroupTitleSO.Instance)
        };
    }
}
