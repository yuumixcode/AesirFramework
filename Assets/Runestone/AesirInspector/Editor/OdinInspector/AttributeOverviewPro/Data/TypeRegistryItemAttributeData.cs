using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    internal class TypeRegistryItemAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("TypeRegistryItem", "TypeRegistryItem",
                "TypeRegistryItem 特性用于自定义类型在 Odin 的类型选择器中的样式。",
                "The TypeRegistryItem attribute customizes the appearance of types in Odin's type selector.",
                "https://odininspector.com/attributes/type-registry-item-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("主要是用于修改类型选择器中的样式，属于编辑器美化类型。",
                "Primarily used to style types in the type selector for editor aesthetics.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "name",
                new BilingualData("类型名称，用于在类型选择器中显示。", "The type name displayed in the type selector.")),
            new ParameterValue(typeof(string).FullName, "categoryPath",
                new BilingualData("类型在类型选择器中的路径，如 GameObject/UI。",
                    "The category path in the type selector, e.g., GameObject/UI.")),
            new ParameterValue(typeof(SdfIconType).FullName, "Icon",
                new BilingualData("图标类型，默认为 SdfIconType.None。",
                    "The icon type. Defaults to SdfIconType.None.")),
            new ParameterValue(typeof(Color).FullName, "LightIconColor",
                new BilingualData("Light 皮肤下的颜色。", "The icon color in the Light editor skin.")),
            new ParameterValue(typeof(Color).FullName, "DarkIconColor",
                new BilingualData("Dark 皮肤下的颜色。", "The icon color in the Dark editor skin.")),
            new ParameterValue(typeof(int).FullName, "Priority",
                new BilingualData("类型在类型选择器中的优先级，默认为 0。",
                    "The priority in the type selector. Defaults to 0."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                TypeRegistryItemExampleSO.Instance)
        };
    }
}
