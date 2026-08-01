using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class OnCollectionChangedAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("OnCollectionChanged", "OnCollectionChanged",
                "OnCollectionChanged 特性用于在集合（如 List、Dictionary 等）的内容发生更改时触发方法。",
                "The OnCollectionChanged attribute is used to trigger methods when the contents of a collection (such as List, Dictionary, etc.) are changed.",
                OdinInspectorDocumentationLinks.OnCollectionChangedUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("你可以指定在集合更改前和更改后分别执行的方法。",
                "You can specify methods to execute before and after the collection changes."),
            new BilingualData("更改方法可以接收 CollectionChangeInfo 参数来获取更改详情。",
                "The change methods can receive a CollectionChangeInfo parameter to get details about the change.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[2]
        {
            new ParameterValue(typeof(string).FullName, "Before",
                new BilingualData("集合更改前要执行的操作或方法名。",
                    "The action or method name to execute before the collection changes.")),
            new ParameterValue(typeof(string).FullName, "After",
                new BilingualData("集合更改后要执行的操作或方法名。",
                    "The action or method name to execute after the collection changes."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Before", ResolverType.ActionResolver, typeof(void).FullName,
                "None", new List<ParameterValue>()),
            new ResolvedStringParameterValue("After", ResolverType.ActionResolver, typeof(void).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                OnCollectionChangedExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Before",
                OnCollectionChangedExampleWithBeforeSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("After",
                OnCollectionChangedExampleWithAfterSO.Instance)
        };
    }
}
