using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class OnInspectorDisposeAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("OnInspectorDispose", "OnInspectorDispose",
                "OnInspectorDispose 特性用于在属性即将从 Inspector 中移除或释放时执行清理代码。",
                "The OnInspectorDispose attribute is used to execute cleanup code when a property is about to be removed or disposed from the Inspector.",
                OdinInspectorDocumentationLinks.OnInspectorDisposeUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("你可以指定一个方法名，或编写 C# 表达式来执行清理逻辑。",
                "You can specify a method name or write a C# expression to execute cleanup logic."),
            new BilingualData("常用于取消订阅事件、释放资源等清理操作。",
                "Commonly used for unsubscribing events, releasing resources, and other cleanup operations.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[1]
        {
            new ParameterValue(typeof(string).FullName, "Action",
                new BilingualData("释放时要执行的操作或方法名。", "The action or method name to execute on dispose."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Action", ResolverType.ActionResolver, typeof(void).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                OnInspectorDisposeExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Action",
                OnInspectorDisposeExampleWithActionSO.Instance)
        };
    }
}
