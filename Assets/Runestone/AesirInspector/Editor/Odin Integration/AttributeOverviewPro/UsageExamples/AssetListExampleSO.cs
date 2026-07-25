using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class AssetListExampleSO : AttributeExampleSO<AssetListExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        [AssetList]
        public Texture2D singleObject;

        [FoldoutGroup("Parameter: Path")]
        [AssetList(Path = "/Plugins/Sirenix/")]
        public List<ScriptableObject> assetList;

        [FoldoutGroup("Parameter: AutoPopulate + Path")]
        [AssetList(AutoPopulate = true, Path = "Plugins/Sirenix/")]
        public List<ScriptableObject> autoPopulatedWhenInspected;

        [FoldoutGroup("Parameter: Tags")]
        [AssetList(Tags = "EditorOnly,Respawn")]
        public List<GameObject> gameObjectsWithTag;

        [FoldoutGroup("Parameter: LayerNames")]
        [AssetList(LayerNames = "Water")]
        public GameObject[] gameObjectsWithLayerNames;

        [FoldoutGroup("Parameter: AssetNamePrefix")]
        [AssetList(AssetNamePrefix = "AesirInspector_")]
        public List<GameObject> gameObjectsWithNamePrefix;

        public override void AesirInspectorReset()
        {
            singleObject = null;
            assetList = null;
            autoPopulatedWhenInspected = null;
            gameObjectsWithTag = null;
            gameObjectsWithLayerNames = null;
            gameObjectsWithNamePrefix = null;
        }
    }
}
