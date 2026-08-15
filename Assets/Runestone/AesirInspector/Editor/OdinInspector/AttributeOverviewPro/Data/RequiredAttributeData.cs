using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Required 特性的介绍数据。
    /// </summary>
    internal class RequiredAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Required", "Required", "Required 特性用于标记关键字段，如果字段为空，将在检查器中显示错误消息。",
                "The Required attribute is used to mark critical fields. If a field is empty, an error message will be displayed in the inspector.",
                OdinInspectorDocumentationLinks.RequiredUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("用于标记关键的值，使其在运行前不能为空。",
                "Used to mark critical values so they cannot be empty before running."),
            new BilingualData("自定义错误信息可以引用成员（使用 $ 符号）或使用表达式（使用 @ 符号）。",
                "Custom error messages can reference members (using $) or use expressions (using @)."),
            new BilingualData("优先使用构造函数参数而不是 ErrorMessage 命名参数，Rider 能提供更好的字符串解析支持。",
                "Prefer using constructor arguments over the ErrorMessage named parameter for better string resolution support in Rider.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "errorMessage",
                new BilingualData("字段为空时显示的自定义错误消息。",
                    "Custom error message displayed when the field is empty.")),
            new ParameterValue("InfoMessageType", "infoMessageType",
                new BilingualData("消息的类型，控制左侧显示的图标（None, Info, Warning, Error）。",
                    "The type of the message, controlling the icon displayed on the left (None, Info, Warning, Error)."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Error Message", ResolverType.ValueResolver,
                typeof(string).FullName, "None", new List<ParameterValue>
                {
                    new ParameterValue("T", "$value",
                        new BilingualData("应用此特性的成员的值（通常为空）。",
                            "The value of the member that has the attribute applied to it (usually empty)."))
                })
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                RequiredExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("ErrorMessage Expression",
                RequiredExampleWithErrorMessageSO.Instance)
        };
    }
}
