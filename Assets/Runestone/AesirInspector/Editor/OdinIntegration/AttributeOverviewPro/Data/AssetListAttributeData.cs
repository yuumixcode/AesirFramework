using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// AssetList 特性的介绍数据，包含标题、参数说明、解析字符串参数和案例预览项。
    /// </summary>
    internal class AssetListAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("AssetList", "AssetList",
                "AssetList 特性可以用于继承自 UnityEngine.Object 类型的单个元素以及由此类元素组成的列表、数组，它使用符合指定筛选条件的所有可能资源的列表来代替默认的列表抽屉。" +
                "使用此功能，你可以从列表或数组中过滤、包含或排除资源，而无需查看项目窗口。",
                "AssetList is used on lists and arrays and single elements of unity types, and replaces the default list drawer with a list of all possible assets with the specified filter. " +
                "Use this to both filter and include or exclude assets from a list or an array, without navigating the project window.",
                OdinInspectorDocumentationLinks.AssetListUrl);

        public override BilingualData[] UsageTips { get; set; } = null;

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[6]
        {
            new ParameterValue(typeof(string).FullName, "Path",
                new BilingualData("根据资源的相对路径筛选出可选择的资源列表，Rider 中可以智能补全路径",
                    "Filter the asset list to only include assets which is located at the specified path.")),
            new ParameterValue(typeof(bool).FullName, "AutoPopulate",
                new BilingualData("是否自动填充满足条件的所有资源到列表中",
                    "If true, all assets found and displayed by the asset list, will automatically be added to the list when inspected.")),
            new ParameterValue(typeof(string).FullName, "Tags",
                new BilingualData("根据资源的 Tag 筛选出可选择的资源列表，使用逗号分隔多个 Tag",
                    "Comma separated list of tags to filter the asset list.")),
            new ParameterValue(typeof(string).FullName, "LayerNames",
                new BilingualData("根据资源的 LayerName 筛选出可选择的资源列表，使用逗号分隔多个 LayerName",
                    "Filter the asset list to only include assets with a specified layer.")),
            new ParameterValue(typeof(string).FullName, "AssetNamePrefix",
                new BilingualData("根据资源的文件名前缀筛选出可选择的资源列表",
                    "Filter the asset list to only include assets which name begins with.")),
            new ParameterValue(typeof(string).FullName, "CustomFilterMethod",
                new BilingualData("设置自定义筛选方法，根据此方法筛选出可选择的资源列表",
                    "Filter the asset list to only include assets for which the given filter method returns true."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Custom Filter Method", ResolverType.ValueResolver,
                typeof(bool).FullName, "None", new List<ParameterValue>
                {
                    new ParameterValue("TList", "$value",
                        new BilingualData("代表应用此特性的列表成员，类型为列表类型",
                            "Representing the member that has attribute applied to it.")),
                    new ParameterValue("TElement", "$asset",
                        new BilingualData("代表列表中的一个元素，类型为元素类型", "Representing an element in the list."))
                })
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Parameters",
                AssetListExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("CustomFilterMethod",
                AssetListExampleWithCustomFilterMethodSO.Instance)
        };
    }
}
