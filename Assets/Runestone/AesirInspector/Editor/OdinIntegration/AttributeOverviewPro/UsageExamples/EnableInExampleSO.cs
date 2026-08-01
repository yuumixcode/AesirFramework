using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class EnableInExampleSO : AttributeExampleSO<EnableInExampleSO>
    {
        [Title("No Parameters")]
        [EnableIn(PrefabKind.All)]
        public GameObject defaultEnabled;

        [Title("Parameter: PrefabKind")]
        [EnableIn(PrefabKind.PrefabAsset)]
        public GameObject enableInPrefab;

        [EnableIn(PrefabKind.InstanceInScene)]
        public GameObject enableInSceneInstance;

        [EnableIn(PrefabKind.NonPrefabInstance)]
        public GameObject enableInNonPrefab;

        public override void AesirInspectorReset()
        {
            defaultEnabled = null;
            enableInPrefab = null;
            enableInSceneInstance = null;
            enableInNonPrefab = null;
        }
    }
}
