namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class AssetSelectorAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("AssetSelector", "AssetSelector",
                "AssetSelector 特性可以作用于单个字段或者列表，在字段选择框旁边增加一个小按钮，可以弹出一个下拉选择框。",
                "The AssetSelector attribute adds a small button next to the field selector to show a dropdown picker, supporting both single fields and lists.",
                "https://odininspector.com/attributes/asset-selector-attribute");

        public override BilingualData[] UsageTips { get; set; } = { };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "IsUniqueList",
                new BilingualData("如果为 true，则列表中不允许有重复的元素，默认为 true。",
                    "If true, duplicate elements are not allowed in the list. Defaults to true.")),
            new ParameterValue(typeof(bool).FullName, "DrawDropdownForListElements",
                new BilingualData("如果为 true，为列表中的每个元素增加一个 AssetSelector 功能，默认为 true。",
                    "If true, adds an AssetSelector feature to each element in the list. Defaults to true.")),
            new ParameterValue(typeof(bool).FullName, "DisableListAddButtonBehaviour",
                new BilingualData("如果为 true，则列表添加按钮的行为被禁用，默认为 false。",
                    "If true, the list add button behavior is disabled. Defaults to false.")),
            new ParameterValue(typeof(bool).FullName, "ExcludeExistingValuesInList",
                new BilingualData(
                    "当 IsUniqueList 和 ExcludeExistingValuesInList 都为 true，则列表中不允许有重复的元素，且直接剔除已经存在的，不会出现勾选框。",
                    "When both IsUniqueList and ExcludeExistingValuesInList are true, existing values are excluded from the dropdown without checkboxes.")),
            new ParameterValue(typeof(bool).FullName, "ExpandAllMenuItems",
                new BilingualData("展开所有可选项，默认为 true。", "Expands all menu items. Defaults to true.")),
            new ParameterValue(typeof(bool).FullName, "FlattenTreeView",
                new BilingualData("默认情况下，下拉选择框具有树结构，如果为 true，将舍弃树结构的绘制。",
                    "By default the dropdown has a tree structure; set to true to flatten it.")),
            new ParameterValue(typeof(int).FullName, "DropdownWidth",
                new BilingualData("下拉选择框的宽度。", "The width of the dropdown.")),
            new ParameterValue(typeof(int).FullName, "DropdownHeight",
                new BilingualData("整个下拉选择框的高度。", "The height of the dropdown.")),
            new ParameterValue(typeof(string).FullName, "DropdownTitle",
                new BilingualData("下拉选择框的标题。", "The title of the dropdown.")),
            new ParameterValue(typeof(string[]).FullName, "SearchInFolders",
                new BilingualData("在特定的文件夹中选择，相对路径，以 Assets/ 开头。",
                    "Search in specific folders. Relative path starting with Assets/.")),
            new ParameterValue(typeof(string).FullName, "Filter",
                new BilingualData("使用 AssetDatabase.FindAssets() 的参数进行过滤。",
                    "Filter using AssetDatabase.FindAssets() parameters.")),
            new ParameterValue(typeof(string).FullName, "Paths",
                new BilingualData("根据相对路径进行筛选，类似 SearchInFolders 参数，可以使用 | 符号分隔多条路径。",
                    "Filter by relative paths, similar to SearchInFolders. Multiple paths can be separated by |."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                AssetSelectorExampleSO.Instance)
        };
    }
}
