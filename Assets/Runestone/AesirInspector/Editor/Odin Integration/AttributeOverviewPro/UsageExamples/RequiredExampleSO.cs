using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class RequiredExampleSO : AttributeExampleSO<RequiredExampleSO>
    {
        [Title("No Parameters")]
        [Required]
        public GameObject defaultRequired;

        [Title("Parameter: InfoMessageType (Info)")]
        [Required("This is an info message.", InfoMessageType.Info)]
        public Rigidbody infoRequired;

        [Title("Parameter: InfoMessageType (Warning)")]
        [Required("This is a warning message.", InfoMessageType.Warning)]
        public ScriptableObject warningRequired;

        [Title("Parameter: InfoMessageType (Error)")]
        [Required("This is an error message.", InfoMessageType.Error)]
        public GameObject errorRequired;

        public override void AesirInspectorReset()
        {
            defaultRequired = null;
            infoRequired = null;
            warningRequired = null;
            errorRequired = null;
        }
    }
}
