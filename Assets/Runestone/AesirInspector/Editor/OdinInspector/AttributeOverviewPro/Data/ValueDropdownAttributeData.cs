using System.Collections;
using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ValueDropdown 特性的介绍数据。
    /// </summary>
    internal class ValueDropdownAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ValueDropdown", "ValueDropdown",
                "ValueDropdown 特性用于在属性上提供一个自定义的下拉选择列表。",
                "The ValueDropdown attribute provides a custom dropdown list for selecting values for a property.",
                OdinInspectorDocumentationLinks.ValueDropdownUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("可以引用字段、属性、方法或使用 Odin 表达式来获取可选值列表。",
                "Can reference a field, property, method, or use an Odin expression to get the list of values."),
            new BilingualData("支持树状结构显示，通过在值前面增加路径字符串实现（如 'Group/Item'）。",
                "Supports tree-view structures by adding path strings to labels (e.g., 'Group/Item')."),
            new BilingualData("可以使用 ValueDropdownList<T> 来方便地定义带有标签和实际值的选项列表。",
                "Use ValueDropdownList<T> to easily define options with custom labels and their corresponding values."),
            new BilingualData("支持多选（作用于列表/集合上），并可以配置是否排除已选值。",
                "Supports multi-selection when applied to collections and can be configured to exclude already selected values.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "values",
                new BilingualData("获取可选值列表的成员名或表达式。",
                    "The member name or expression to get the list of values.")),
            new ParameterValue(typeof(bool).FullName, "AppendNextDrawer",
                new BilingualData("是否在原有的绘制方式后附加下拉按钮，而不是直接替换。",
                    "Whether to append a dropdown button next to the original drawer instead of replacing it.")),
            new ParameterValue(typeof(bool).FullName, "DisableGUIInAppendedDrawer",
                new BilingualData("配合 AppendNextDrawer 使用，是否禁用原有绘制的交互。",
                    "If true, the original drawer will be disabled when using AppendNextDrawer.")),
            new ParameterValue(typeof(bool).FullName, "ExpandAllMenuItems",
                new BilingualData("如果显示为树状图，是否默认展开所有项。",
                    "Whether to expand all menu items by default in tree-view mode.")),
            new ParameterValue(typeof(bool).FullName, "IsUniqueList",
                new BilingualData("当作用于列表时，是否保证列表项唯一。", "Whether to ensure items in the list are unique.")),
            new ParameterValue(typeof(int).FullName, "NumberOfItemsBeforeEnablingSearch",
                new BilingualData("当列表项达到多少个时显示搜索框。",
                    "The number of items required before search is enabled in the dropdown."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Values", ResolverType.ValueResolver,
                typeof(IEnumerable).FullName, "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ValueDropdownExampleSO.Instance)
        };
    }
}
