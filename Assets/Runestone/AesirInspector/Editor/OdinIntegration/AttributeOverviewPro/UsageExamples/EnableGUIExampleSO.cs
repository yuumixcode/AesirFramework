using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class EnableGUIExampleSO : AttributeExampleSO<EnableGUIExampleSO>
    {
        [Title("No Parameters")]
        public string normalField = "Normal editable field";

        [ReadOnly]
        public string readOnlyField = "Read-only field (grayed out)";

        [ReadOnly]
        [EnableGUI]
        public string enabledReadOnlyField = "Can receive focus despite being read-only";

        public override void AesirInspectorReset()
        {
            normalField = "Normal editable field";
            readOnlyField = "Read-only field (grayed out)";
            enabledReadOnlyField = "Can receive focus despite being read-only";
        }
    }
}
