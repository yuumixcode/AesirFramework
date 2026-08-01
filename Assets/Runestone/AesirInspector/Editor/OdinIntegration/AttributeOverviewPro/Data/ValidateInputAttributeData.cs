using System.Collections.Generic;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ValidateInput 特性的介绍数据。
    /// </summary>
    internal class ValidateInputAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ValidateInput", "ValidateInput",
                "ValidateInput 特性用于在检查器中对属性值进行自定义验证，并在验证失败时显示消息。",
                "The ValidateInput attribute is used to perform custom validation on property values in the inspector and display a message when validation fails.",
                OdinInspectorDocumentationLinks.ValidateInputUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("支持使用方法名、成员变量或 C# 表达式作为验证逻辑。",
                "Supports using method names, member variables, or C# expressions as validation logic."),
            new BilingualData("验证方法可以包含参数，如属性的值、属性本身，甚至可以通过 ref 参数动态修改错误消息和消息类型。",
                "Validation methods can include parameters such as the value of the property, the property itself, and even dynamically modify the error message and message type via ref parameters."),
            new BilingualData("可以设置 ContinuousValidationCheck 选项来强制持续验证（每帧检查）。",
                "The ContinuousValidationCheck option can be set to force continuous validation (per-frame checking).")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "condition",
                new BilingualData("验证逻辑的方法名或表达式。返回 true 表示验证通过。",
                    "The method name or expression for validation logic. Returning true indicates validation passed.")),
            new ParameterValue(typeof(string).FullName, "defaultMessage",
                new BilingualData("验证失败时显示的默认消息。支持字符串解析。",
                    "The default message displayed when validation fails. Supports string resolution.")),
            new ParameterValue("InfoMessageType", "messageType",
                new BilingualData("消息的类型（Info, Warning, Error, None）。默认为 Error。",
                    "The type of the message (Info, Warning, Error, None). Defaults to Error.")),
            new ParameterValue(typeof(bool).FullName, "IncludeChildren",
                new BilingualData("子字段修改时是否也触发验证。默认为 true。",
                    "Whether to trigger validation when sub-fields are modified. Defaults to true.")),
            new ParameterValue(typeof(bool).FullName, "ContinuousValidationCheck",
                new BilingualData("是否每帧都进行验证，而不仅是在值改变时。默认为 false。",
                    "Whether to perform validation every frame, not just when the value changes. Defaults to false."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Condition", ResolverType.ValueResolver, typeof(bool).FullName,
                "None", new List<ParameterValue>
                {
                    new ParameterValue("ref string", "message",
                        new BilingualData("可通过 ref 修改的错误消息。", "Error message that can be modified via ref.")),
                    new ParameterValue("ref InfoMessageType", "messageType",
                        new BilingualData("可通过 ref 修改的消息类型。", "Message type that can be modified via ref.")),
                    new ParameterValue("T", "$value",
                        new BilingualData("当前属性的值。", "The current value of the property.")),
                    new ParameterValue("InspectorProperty", "$property",
                        new BilingualData("当前的 InspectorProperty 对象。",
                            "The current InspectorProperty object."))
                }),
            new ResolvedStringParameterValue("Default Message", ResolverType.ValueResolver,
                typeof(string).FullName, "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Usage Examples",
                ValidateInputExampleSO.Instance)
        };
    }
}
