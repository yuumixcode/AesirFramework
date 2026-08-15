using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// InlineEditor 特性的介绍数据。
    /// </summary>
    internal class InlineEditorAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("InlineEditor", "InlineEditor",
                "InlineEditor 特性用于在当前属性下方直接嵌入另一个对象的编辑器面板。",
                "The InlineEditor attribute is used to embed the editor of another object directly below the property.",
                OdinInspectorDocumentationLinks.InlineEditorUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("支持多种显示模式，如仅 GUI、带标题、带预览或完整编辑器。",
                "Supports multiple display modes, such as GUI only, with header, with preview, or full editor."),
            new BilingualData("可以控制对象选择字段（ObjectField）的显示方式，如折叠模式或隐藏模式。",
                "Controls how the object selection field (ObjectField) is drawn, such as foldout or hidden mode."),
            new BilingualData("非常适合用于 ScriptableObject 或 Material 的内联编辑，减少窗口切换。",
                "Ideal for inline editing of ScriptableObjects or Materials, reducing window switching.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(InlineEditorModes).FullName, "inlineEditorMode",
                new BilingualData("编辑器显示模式。默认值为 GUIOnly。",
                    "The mode in which the editor should be drawn. Default is GUIOnly.")),
            new ParameterValue(typeof(InlineEditorObjectFieldModes).FullName, "objectFieldMode",
                new BilingualData("对象字段的绘制模式。默认值为 Boxed。",
                    "The mode in which the object field should be drawn. Default is Boxed."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                InlineEditorExampleSO.Instance)
        };
    }
}
