using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class DisableInEditorModeExampleSO : AttributeExampleSO<DisableInEditorModeExampleSO>
    {
        [Title("No Parameters")]
        [DisableInEditorMode]
        public GameObject gameObject;

        [DisableInEditorMode]
        public Material material;

        [DisableInEditorMode]
        public int someValue;

        public override void AesirInspectorReset()
        {
            gameObject = null;
            material = null;
            someValue = 0;
        }
    }
}
