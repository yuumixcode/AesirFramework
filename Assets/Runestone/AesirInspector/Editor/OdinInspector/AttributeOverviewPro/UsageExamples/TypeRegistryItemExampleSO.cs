using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class TypeRegistryItemExampleSO : AttributeExampleSO<TypeRegistryItemExampleSO>
    {
        const string CATEGORY_PATH = "Sirenix.TypeSelector.Demo";
        const string BASE_ITEM_NAME = "Painting Tools";
        const string PATH = CATEGORY_PATH + "/" + BASE_ITEM_NAME;

        [Title("Default Style")]
        [ShowInInspector]
        [PolymorphicDrawerSettings(ShowBaseType = true)]
        public BasicClass BasicItem;

        [Title("Using TypeRegistryItem Attribute")]
        [ShowInInspector]
        [PolymorphicDrawerSettings(ShowBaseType = true)]
        public Base PaintingItem;

        public override void AesirInspectorReset()
        {
            BasicItem = null;
            PaintingItem = null;
        }

        public abstract class BasicClass { }

        public class MyClassA : BasicClass
        {
            public string Name;
        }

        public class MyClassB : BasicClass
        {
            public int Number;
        }

        public class MyClassC : BasicClass
        {
            public float Number;
        }

        [TypeRegistryItem(Name = BASE_ITEM_NAME, Icon = SdfIconType.Tools, CategoryPath = CATEGORY_PATH,
            Priority = int.MinValue)]
        public abstract class Base { }

        [TypeRegistryItem(Name = "Brush", CategoryPath = PATH, Icon = SdfIconType.BrushFill,
            Priority = int.MinValue)]
        public class InheritorA : Base
        {
            public Color Color = Color.red;
            public float PaintRemaining = 0.4f;
        }

        [TypeRegistryItem(Name = "Paint Bucket", CategoryPath = PATH, Icon = SdfIconType.PaintBucket,
            Priority = int.MinValue)]
        public class InheritorB : Base
        {
            public Color Color = Color.green;
            public float PaintRemaining = 0.8f;
        }

        [TypeRegistryItem(Name = "Palette", CategoryPath = PATH, Icon = SdfIconType.PaletteFill,
            Priority = int.MinValue)]
        public class InheritorC : Base
        {
            public Color[] Colors = { Color.blue, Color.red, Color.green, Color.white };
        }
    }
}
