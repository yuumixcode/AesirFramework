using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class DisableInExampleSO : AttributeExampleSO<DisableInExampleSO>
    {
        [Title("No Parameters")]
        [DisableIn(PrefabKind.All)]
        public GameObject defaultDisabled;

        [Title("Parameter: PrefabKind")]
        [DisableIn(PrefabKind.PrefabAsset)]
        public GameObject disableInPrefab;

        [DisableIn(PrefabKind.InstanceInScene)]
        public GameObject disableInSceneInstance;

        [DisableIn(PrefabKind.NonPrefabInstance)]
        public GameObject disableInNonPrefab;

        public override void AesirInspectorReset()
        {
            defaultDisabled = null;
            disableInPrefab = null;
            disableInSceneInstance = null;
            disableInNonPrefab = null;
        }
    }
}
