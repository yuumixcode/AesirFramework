namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// SceneObjectsOnly 特性的介绍数据。
    /// </summary>
    internal class SceneObjectsOnlyAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("SceneObjectsOnly", "SceneObjectsOnly",
                "SceneObjectsOnly 特性用于限制对象引用仅能选择场景中的对象，而不能选择项目资源（Prefab 等）。",
                "The SceneObjectsOnly attribute restricts object references to only allow scene objects, preventing the selection of project assets like prefabs.",
                OdinInspectorDocumentationLinks.SceneObjectsOnlyUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("常用于需要引用当前场景中特定实例的字段。",
                "Commonly used for fields that must reference a specific instance in the current scene."),
            new BilingualData("如果尝试将项目资源拖入该字段，Odin 会显示验证错误。",
                "Odin will display a validation error if you try to drag a project asset into the field."),
            new BilingualData("与 AssetsOnly 特性相反。", "The opposite of the AssetsOnly attribute.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                SceneObjectsOnlyExampleSO.Instance)
        };
    }
}
