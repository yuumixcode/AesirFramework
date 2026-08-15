namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ShowInInspector 特性的介绍数据。
    /// </summary>
    internal class ShowInInspectorAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Show In Inspector", "在检查器中显示",
                "ShowInInspector 特性用于在 Inspector 中显示非序列化的成员，如私有字段、属性或方法返回值。",
                "The ShowInInspector attribute is used to display non-serialized members in the inspector, such as private fields, properties, or method return values.",
                OdinInspectorDocumentationLinks.ShowInInspectorUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("通常 Unity 只显示公有的序列化字段。使用此特性，你可以查看那些通常隐藏的状态，而无需将它们设为公有或添加 [SerializeField]。",
                "Normally, Unity only displays public serialized fields. With this attribute, you can view states that are normally hidden without having to make them public or adding [SerializeField]."),
            new BilingualData("请注意，仅使用 [ShowInInspector] 不会使成员变得可序列化；它只是在 Inspector 中显示它。",
                "Note that using [ShowInInspector] alone does not make a member serializable; it only displays it in the inspector.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = null;

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = null;

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ShowInInspectorExampleSO.Instance)
        };
    }
}
