using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class RequiredInExampleSO : AttributeExampleSO<RequiredInExampleSO>
    {
        [Title("Note")]
        [DisplayAsString(12)]
        [HideLabel]
        public string info =
            "RequiredIn attribute is specific to prefabs. It will only show validation errors when the object is a certain kind of prefab (e.g., Asset, Instance in Scene, etc.).";

        [RequiredIn(PrefabKind.PrefabAsset)]
        [InfoBox("This field is required if this object is a Prefab Asset.")]
        public GameObject requiredInPrefabAsset;

        [RequiredIn(PrefabKind.InstanceInScene)]
        [InfoBox("This field is required if this object is a Prefab Instance in a Scene.")]
        public GameObject requiredInPrefabInstance;

        public override void AesirInspectorReset()
        {
            requiredInPrefabAsset = null;
            requiredInPrefabInstance = null;
        }
    }
}
