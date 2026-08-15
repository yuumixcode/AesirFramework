using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class
        SuppressInvalidAttributeErrorExampleSO : AttributeExampleSO<SuppressInvalidAttributeErrorExampleSO>
    {
        [Title("Without Suppression")]
        [Range(0, 10)]
        public string unsuppressedError = "This will show an error";

        [Title("With Suppression")]
        [Range(0, 10)]
        [SuppressInvalidAttributeError]
        public string suppressedError = "Error suppressed";

        public override void AesirInspectorReset()
        {
            unsuppressedError = "This will show an error";
            suppressedError = "Error suppressed";
        }
    }
}
