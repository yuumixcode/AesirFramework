using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class OnInspectorInitAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("OnInspectorInit", "OnInspectorInit",
                "OnInspectorInit 特性用于在属性即将在 Inspector 中首次绘制之前执行初始化代码。",
                "The OnInspectorInit attribute is used to execute initialization code just before a property is drawn in the Inspector for the first time.",
                OdinInspectorDocumentationLinks.OnInspectorInitUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("你可以指定一个方法名，或编写 C# 表达式来执行初始化逻辑。",
                "You can specify a method name or write a C# expression to execute initialization logic."),
            new BilingualData("Odin 的属性系统是延迟求值的，因此 OnInspectorInit 只在属性首次被解析时执行。",
                "Odin's property system is lazily evaluated, so OnInspectorInit only executes when the property is first resolved.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[1]
        {
            new ParameterValue(typeof(string).FullName, "Action",
                new BilingualData("初始化时要执行的操作或方法名。",
                    "The action or method name to execute on initialization."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Action", ResolverType.ActionResolver, typeof(void).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                OnInspectorInitExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Action",
                OnInspectorInitExampleWithActionSO.Instance)
        };
    }
}
