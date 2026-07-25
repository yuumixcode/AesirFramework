using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class HideInPlayModeExampleSO : AttributeExampleSO<HideInPlayModeExampleSO>
    {
        [Title("No Parameters")]
        [HideInPlayMode]
        public GameObject gameObject;

        [HideInPlayMode]
        public Material material;

        [HideInPlayMode]
        public int someValue;

        public override void AesirInspectorReset()
        {
            gameObject = null;
            material = null;
            someValue = 0;
        }
    }
}
