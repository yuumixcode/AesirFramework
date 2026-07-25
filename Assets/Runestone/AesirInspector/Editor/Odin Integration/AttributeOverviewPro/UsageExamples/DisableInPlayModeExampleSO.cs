using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class DisableInPlayModeExampleSO : AttributeExampleSO<DisableInPlayModeExampleSO>
    {
        [Title("No Parameters")]
        [DisableInPlayMode]
        public GameObject gameObject;

        [DisableInPlayMode]
        public Material material;

        [DisableInPlayMode]
        public int someValue;

        public override void AesirInspectorReset()
        {
            gameObject = null;
            material = null;
            someValue = 0;
        }
    }
}
