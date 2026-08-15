using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class ColorPaletteExampleSO : AttributeExampleSO<ColorPaletteExampleSO>
    {
        [Title("No Parameters")]
        [ColorPalette]
        public Color color1;

        [Title("Parameter: PaletteName")]
        [ColorPalette(PaletteName = "Color3")]
        public Color color2;

        [Title("Parameter: ShowAlpha")]
        [ColorPalette(ShowAlpha = true)]
        public Color color3;

        public override void AesirInspectorReset()
        {
            color1 = Color.white;
            color2 = Color.white;
            color3 = Color.white;
        }
    }
}
