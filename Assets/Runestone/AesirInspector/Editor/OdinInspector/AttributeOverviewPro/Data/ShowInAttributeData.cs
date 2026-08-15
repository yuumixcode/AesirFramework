using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class ShowInAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ShowIn", "ShowIn", "ShowIn 特性用于仅在指定编辑器模式下显示属性。",
                "The ShowIn attribute is used to display a property only in a specified editor mode.",
                OdinInspectorDocumentationLinks.ShowInUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("使用了 ShowIn 特性的 Property 所在的脚本，需要是和 Prefab 有关的，可以是预制体物体或者子物体。",
                "The script containing a property with ShowIn must be related to a Prefab, either a prefab object or a child object."),
            new BilingualData("当脚本所在的预制体是某一种特定类型(PrefabKind)时，被标记的 Property 将会显示。",
                "When the prefab containing the script is of a specific type (PrefabKind), the property will be shown.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(PrefabKind).FullName, "kind",
                new BilingualData(
                    "Prefab 当前的类型，可以同时为多种类型，用 | 分隔，如: PrefabKind.InstanceInScene | PrefabKind.InstanceInPrefab",
                    "The current type of Prefab, multiple types can be combined using |, e.g.: PrefabKind.InstanceInScene | PrefabKind.InstanceInPrefab")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.None",
                new BilingualData("此时将不会显示，因为无法满足这个条件。",
                    "The property will not be shown because this condition cannot be met.")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.InstanceInScene",
                new BilingualData("表示当前脚本挂载的物体是 Prefab，并且是场景中的实例时生效。",
                    "Applies when the object is a Prefab instance in a scene.")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.InstanceInPrefab",
                new BilingualData("表示当前脚本挂载的物体是 Prefab，并且是嵌套在其他预制体中的物体时生效。",
                    "Applies when the object is a Prefab nested inside another prefab.")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.Regular",
                new BilingualData("表示当前脚本挂载的物体是 Regular Prefab 时生效。",
                    "Applies when the object is a regular Prefab.")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.Variant",
                new BilingualData("表示当前脚本挂载的物体是 Prefab Variant (变体) 时生效。",
                    "Applies when the object is a Prefab Variant.")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.NonPrefabInstance",
                new BilingualData("表示当前脚本挂载的物体是场景中的非 Prefab 实例时生效。",
                    "Applies when the object is a non-Prefab instance in the scene.")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.PrefabInstance",
                new BilingualData("PrefabInstance = InstanceInPrefab | InstanceInScene",
                    "PrefabInstance = InstanceInPrefab | InstanceInScene")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.PrefabAsset",
                new BilingualData("PrefabAsset = Variant | Regular", "PrefabAsset = Variant | Regular")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.PrefabInstanceAndNonPrefabInstance",
                new BilingualData(
                    "PrefabInstanceAndNonPrefabInstance = InstanceInPrefab | InstanceInScene | NonPrefabInstance",
                    "PrefabInstanceAndNonPrefabInstance = InstanceInPrefab | InstanceInScene | NonPrefabInstance")),
            new ParameterValue(">>> PrefabKind", "PrefabKind.All",
                new BilingualData("All = PrefabInstanceAndNonPrefabInstance | PrefabAsset",
                    "All = PrefabInstanceAndNonPrefabInstance | PrefabAsset"))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ShowInExampleSO.Instance)
        };
    }
}
