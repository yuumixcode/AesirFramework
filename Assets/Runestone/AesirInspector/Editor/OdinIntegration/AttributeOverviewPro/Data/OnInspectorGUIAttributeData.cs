using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// OnInspectorGUI 特性的介绍数据。
    /// </summary>
    internal class OnInspectorGUIAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("On Inspector GUI", "在检查器 GUI 时",
                "OnInspectorGUI 特性允许你在 Inspector 中执行自定义的 GUI 代码。你可以将其应用于字段、属性或方法。",
                "The OnInspectorGUI attribute allows you to execute custom GUI code in the inspector. You can apply it to fields, properties, or methods.",
                OdinInspectorDocumentationLinks.OnInspectorGuiUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("你可以指定一个方法名，或者直接编写 C# 表达式。",
                "You can specify a method name or write a C# expression directly."),
            new BilingualData("当应用于字段或属性时，你可以选择在成员之前或之后绘制自定义 GUI。",
                "When applied to a field or property, you can choose to draw custom GUI before or after the member.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[1]
        {
            new ParameterValue(typeof(string).FullName, "Action",
                new BilingualData("要执行的 GUI 操作或方法名。", "The GUI action or method name to execute."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Action", ResolverType.ActionResolver, "void", "None",
                new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                OnInspectorGUIExampleSO.Instance)
        };
    }
}
