using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Searchable 特性的介绍数据。
    /// </summary>
    internal class SearchableAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Searchable", "Searchable", "Searchable 特性为列表、数组或类添加一个搜索框，方便快速筛选内容。",
                "The Searchable attribute adds a search field to a list, array, or class, allowing for quick content filtering.",
                OdinInspectorDocumentationLinks.SearchableUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("默认开启模糊搜索和递归搜索，可以匹配子对象的字段内容。",
                "Fuzzy search and recursive search are enabled by default, matching content in child object fields."),
            new BilingualData("可以作用于集合上，也可以直接作用于类声明上，使其所有实例都可搜索。",
                "Can be applied to collections or directly to class declarations to make all instances searchable."),
            new BilingualData("通过 FilterOptions 可以精细控制搜索匹配的范围（如仅匹配属性名、匹配值类型等）。",
                "FilterOptions allows fine-grained control over search matching (e.g., property name only, value type only).")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "FuzzySearch",
                new BilingualData("是否启用模糊搜索。默认值为 true。",
                    "Whether to enable fuzzy searching. Default is true.")),
            new ParameterValue(typeof(bool).FullName, "Recursive",
                new BilingualData("是否递归搜索子属性。默认值为 true。",
                    "Whether to search child properties recursively. Default is true.")),
            new ParameterValue(typeof(SearchFilterOptions).FullName, "FilterOptions",
                new BilingualData("搜索过滤选项。", "Options for how searching should filter properties."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                SearchableExampleSO.Instance)
        };
    }
}
