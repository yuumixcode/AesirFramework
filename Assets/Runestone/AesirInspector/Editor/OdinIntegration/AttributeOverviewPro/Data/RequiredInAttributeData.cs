namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// RequiredIn 特性的介绍数据。
    /// </summary>
    internal class RequiredInAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("RequiredIn", "RequiredIn",
                "RequiredIn 是 Required 特性的变体，专门用于预制体（Prefab）对象。它允许你指定属性在特定的预制体类型中不能为空。",
                "RequiredIn is a variant of the Required attribute specifically for Prefab objects. It allows you to specify that a property must not be null in certain prefab kinds.",
                OdinInspectorDocumentationLinks.RequiredInUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("适用于预制体资源或预制体实例中的脚本属性。",
                "Applicable to script properties in prefab assets or prefab instances."),
            new BilingualData("通过 PrefabKind 参数，你可以精确控制在哪些预制体状态下触发必填检查。",
                "Via the PrefabKind parameter, you can precisely control in which prefab states the required check is triggered."),
            new BilingualData("支持自定义错误消息，并且支持解析器（Resolvers）。",
                "Supports custom error messages and resolvers.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue("PrefabKind", "Kind",
                new BilingualData("指定该属性必须存在的预制体类型。",
                    "Specifies the prefab kinds where this property must be present.")),
            new ParameterValue(typeof(string).FullName, "ErrorMessage",
                new BilingualData("当验证失败时显示的自定义错误消息。",
                    "Custom error message to display when validation fails."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                RequiredInExampleSO.Instance)
        };
    }
}
