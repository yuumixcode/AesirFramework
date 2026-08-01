using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class ToggleLeftExampleSO : AttributeExampleSO<ToggleLeftExampleSO>
    {
        [Title("No Parameters")]
        [InfoBox("Draws the checkbox toggle aligned to the left side of the label")]
        [ToggleLeft]
        public bool leftToggled;

        [EnableIf("leftToggled")]
        public int A;

        [EnableIf("leftToggled")]
        public bool B;

        public override void AesirInspectorReset()
        {
            leftToggled = false;
            A = 0;
            B = false;
        }
    }
}
