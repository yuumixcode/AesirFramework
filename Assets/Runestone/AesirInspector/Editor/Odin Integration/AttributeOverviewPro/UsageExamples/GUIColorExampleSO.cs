using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class GUIColorExampleSO : AttributeExampleSO<GUIColorExampleSO>
    {
        [Title("Parameter: r, g, b, a")]
        [GUIColor(1f, 0.8f, 0.4f)]
        public int rgbaExample;

        [Title("Parameter: Color (Hex Code)")]
        [GUIColor("#FF5512")]
        public int hexExample;

        [Title("Parameter: Color (Color Name)")]
        [GUIColor("orange")]
        public int colorNameExample;

        public override void AesirInspectorReset()
        {
            rgbaExample = 0;
            hexExample = 0;
            colorNameExample = 0;
        }
    }
}
