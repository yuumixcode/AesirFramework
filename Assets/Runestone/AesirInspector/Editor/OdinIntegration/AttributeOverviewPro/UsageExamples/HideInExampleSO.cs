using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class HideInExampleSO : AttributeExampleSO<HideInExampleSO>
    {
        [Title("No Parameters")]
        [HideIn(PrefabKind.All)]
        public GameObject defaultPrefab;

        [Title("Parameter: PrefabKind")]
        [HideIn(PrefabKind.PrefabAsset)]
        public GameObject hideInPrefab;

        [HideIn(PrefabKind.InstanceInScene)]
        public GameObject hideInSceneInstance;

        [HideIn(PrefabKind.NonPrefabInstance)]
        public GameObject hideInNonPrefab;

        public override void AesirInspectorReset()
        {
            defaultPrefab = null;
            hideInPrefab = null;
            hideInSceneInstance = null;
            hideInNonPrefab = null;
        }
    }
}
