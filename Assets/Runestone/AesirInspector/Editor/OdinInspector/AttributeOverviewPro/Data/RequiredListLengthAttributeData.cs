namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class RequiredListLengthAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("RequiredListLength", "RequiredListLength",
                "RequiredListLength 特性用于限制列表的最小和/或最大长度。",
                "The RequiredListLength attribute is used to restrict the minimum and/or maximum length of a list.");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("可以只设置最小长度，或同时设置最小和最大长度。",
                "Can set only a minimum length, or both minimum and maximum length."),
            new BilingualData("支持使用成员变量（$ 符号）或表达式（@ 符号）解析字符串参数。",
                "Supports resolving string parameters using member references ($) or expressions (@).")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(int).FullName, "ListLength",
                new BilingualData("列表必须满足的最小/最大长度。", "The minimum/maximum length the list must satisfy.")),
            new ParameterValue(typeof(string).FullName, "Message",
                new BilingualData("验证失败时显示的自定义消息。", "Custom message displayed when validation fails."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = null;

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                RequiredListLengthExampleSO.Instance)
        };
    }
}
