using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class
        ColorPaletteExampleWithPaletteNameSO : AttributeExampleSO<ColorPaletteExampleWithPaletteNameSO>
    {
        [Title("Field Name Example")]
        [ColorPalette("$SepiaPaletteName")]
        public Color fieldNameExample;

        [Title("Attribute Expression Example")]
        [ColorPalette("@UseTropical ? TropicalPaletteName : SepiaPaletteName")]
        public Color attributeExpressionExample;

        [Title("Property Name Example")]
        [ColorPalette("$PaletteNameProperty")]
        public Color propertyNameExample;

        [Title("Method Name Example")]
        [ColorPalette("$GetPaletteName")]
        public Color methodNameExample;

        public string SepiaPaletteName = "Sepia";
        public string TropicalPaletteName = "Tropical";
        public bool UseTropical;

        public string PaletteNameProperty => UseTropical ? TropicalPaletteName : SepiaPaletteName;

        public override void AesirInspectorReset()
        {
            SepiaPaletteName = "Sepia";
            TropicalPaletteName = "Tropical";
            UseTropical = false;
            fieldNameExample = Color.white;
            attributeExpressionExample = Color.white;
            propertyNameExample = Color.white;
            methodNameExample = Color.white;
        }

        string GetPaletteName() => UseTropical ? TropicalPaletteName : SepiaPaletteName;
    }
}
