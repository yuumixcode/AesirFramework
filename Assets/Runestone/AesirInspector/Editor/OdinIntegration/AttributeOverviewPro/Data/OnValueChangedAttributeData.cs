using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// OnValueChanged 特性的介绍数据。
    /// </summary>
    internal class OnValueChangedAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("OnValueChanged", "OnValueChanged",
                "OnValueChanged 特性用于在属性值在 Inspector 面板中被修改时触发一个方法。",
                "The OnValueChanged attribute is used to trigger a method whenever a property value is changed in the Inspector.",
                OdinInspectorDocumentationLinks.OnValueChangedUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("该特性仅在 Inspector 面板中修改值时触发，通过代码修改值不会触发。",
                "This attribute is only triggered when the value is changed via the Inspector; changes via code will not trigger it."),
            new BilingualData("可以引用方法名，方法可以没有参数，也可以有一个与属性类型一致的参数来接收新值。",
                "It can reference a method name. The method can have no parameters, or a single parameter matching the property's type to receive the new value."),
            new BilingualData("常用于动态创建资源、联动更新其他属性等场景。",
                "Commonly used for dynamically creating assets, linking updates to other properties, etc.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "methodName",
                new BilingualData("修改时触发的方法名。", "The name of the method to trigger on change.")),
            new ParameterValue(typeof(bool).FullName, "includeChildren",
                new BilingualData("如果为 true，则子属性的修改也会触发此方法。",
                    "If true, changes to child properties will also trigger this method."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Method", ResolverType.ActionResolver, "void", "None",
                new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                OnValueChangedExampleSO.Instance)
        };
    }
}
