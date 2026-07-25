using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class ChildGameObjectOnlyExampleSO : AttributeExampleSO<ChildGameObjectOnlyExampleSO>
    {
        [Title("No Parameters")]
        [ChildGameObjectsOnly]
        public Transform childObject;

        public override void AesirInspectorReset()
        {
            childObject = null;
        }
    }
}
