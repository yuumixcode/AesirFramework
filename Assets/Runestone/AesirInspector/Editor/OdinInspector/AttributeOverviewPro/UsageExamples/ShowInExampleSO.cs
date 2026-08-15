using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class ShowInExampleSO : AttributeExampleSO<ShowInExampleSO>
    {
        [Title("No Parameters")]
        [ShowIn(PrefabKind.All)]
        public GameObject defaultPrefab;

        [Title("Parameter: PrefabKind")]
        [ShowIn(PrefabKind.PrefabAsset)]
        public GameObject showInPrefab;

        [ShowIn(PrefabKind.InstanceInScene)]
        public GameObject showInSceneInstance;

        [ShowIn(PrefabKind.NonPrefabInstance)]
        public GameObject showInNonPrefab;

        public override void AesirInspectorReset()
        {
            defaultPrefab = null;
            showInPrefab = null;
            showInSceneInstance = null;
            showInNonPrefab = null;
        }
    }
}
