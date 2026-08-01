using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// OnStateUpdate 特性的介绍数据。
    /// </summary>
    internal class OnStateUpdateAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("On State Update", "在状态更新时",
                "OnStateUpdate 特性允许你在属性的状态（如可见性、是否禁用等）更新时执行代码。它在属性被绘制之前执行。",
                "The OnStateUpdate attribute allows you to execute code whenever the state of a property (like visibility, whether it's disabled, etc.) is updated. it runs before the property is drawn.",
                OdinInspectorDocumentationLinks.OnStateUpdateUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("这非常适合用于根据其他成员的值来动态更改属性的状态。",
                "This is perfect for dynamically changing the state of a property based on the values of other members."),
            new BilingualData("你可以通过 $property 访问当前属性，并修改其 State 属性。",
                "You can access the current property via $property and modify its State property.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[1]
        {
            new ParameterValue(typeof(string).FullName, "Action",
                new BilingualData("要执行的操作或方法名。", "The action or method name to execute."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Action", ResolverType.ActionResolver, "void", "None",
                new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                OnStateUpdateExampleSO.Instance)
        };
    }
}
