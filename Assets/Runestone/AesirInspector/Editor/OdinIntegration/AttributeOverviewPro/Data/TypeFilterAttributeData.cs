using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// TypeFilter 特性的介绍数据。
    /// </summary>
    internal class TypeFilterAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Type Filter", "类型过滤器",
                "TypeFilter 特性为属性提供了一个下拉列表，用于选择并实例化不同的类型。这在处理多态性时非常有用。",
                "The TypeFilter attribute provides a dropdown for a property to select and instantiate different types. This is very useful when dealing with polymorphism.",
                OdinInspectorDocumentationLinks.TypeFilterUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("你可以指定一个方法或字段名，该成员应返回一个可选类型的列表。",
                "You can specify a method or field name that returns a list of selectable types."),
            new BilingualData("此特性通常与接口或抽象类一起使用，以便在运行时选择具体的实现类。",
                "This attribute is often used with interfaces or abstract classes to select concrete implementation classes at runtime.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[1]
        {
            new ParameterValue(typeof(string).FullName, "FilterGetter",
                new BilingualData("返回可选类型列表的方法或字段名。",
                    "The name of the method or field that returns the list of selectable types."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("FilterGetter", ResolverType.ValueResolver, "IEnumerable<Type>",
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                TypeFilterExampleSO.Instance)
        };
    }
}
