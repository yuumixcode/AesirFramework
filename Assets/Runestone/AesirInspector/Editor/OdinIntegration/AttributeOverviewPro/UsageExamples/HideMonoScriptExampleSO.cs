using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    [HideMonoScript]
    public class HideMonoScriptExampleSO : AttributeExampleSO<HideMonoScriptExampleSO>
    {
        [Title("No Parameters")]
        public string sampleField;

        public override void AesirInspectorReset()
        {
            sampleField = string.Empty;
        }
    }
}
