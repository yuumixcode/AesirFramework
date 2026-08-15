using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class DisableInInlineEditorExampleSO : AttributeExampleSO<DisableInInlineEditorExampleSO>
    {
        [Title("No Parameters")]
        [DisableInInlineEditors]
        public Material material;

        [DisableInInlineEditors]
        public Mesh mesh;

        public override void AesirInspectorReset()
        {
            material = null;
            mesh = null;
        }
    }
}
