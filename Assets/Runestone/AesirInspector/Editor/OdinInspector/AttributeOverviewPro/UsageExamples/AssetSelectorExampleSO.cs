using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class AssetSelectorExampleSO : AttributeExampleSO<AssetSelectorExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        [AssetSelector]
        public ScriptableObject example;

        [FoldoutGroup("FlattenTreeView")]
        [AssetSelector(FlattenTreeView = true)]
        public ScriptableObject example2;

        [FoldoutGroup("Path")]
        [AssetSelector(Paths = "Assets/Plugins/OdinToolkits")]
        public GameObject gameObject;

        [FoldoutGroup("IsUniqueList")]
        [AssetSelector(IsUniqueList = false)]
        public List<GameObject> gameObjects;

        [FoldoutGroup("DrawDropdownForListElements")]
        [AssetSelector(DrawDropdownForListElements = false)]
        public List<GameObject> gameObjects2;

        [FoldoutGroup("DisableListAddButtonBehaviour")]
        [AssetSelector(DisableListAddButtonBehaviour = true)]
        public List<GameObject> gameObjects3;

        [FoldoutGroup("ExcludeExistingValuesInList")]
        [AssetSelector(ExcludeExistingValuesInList = true)]
        public List<GameObject> gameObjects4;

        [FoldoutGroup("ExpandAllMenuItems")]
        [AssetSelector(ExpandAllMenuItems = false)]
        public List<GameObject> gameObjects5;

        [FoldoutGroup("DropdownSettings")]
        [AssetSelector(DropdownWidth = 600, DropdownHeight = 300, DropdownTitle = "Dropdown Title")]
        public GameObject gameObject6;

        public override void AesirInspectorReset()
        {
            example = null;
            example2 = null;
            gameObject = null;
            gameObjects = null;
            gameObjects2 = null;
            gameObjects3 = null;
            gameObjects4 = null;
            gameObjects5 = null;
            gameObject6 = null;
        }
    }
}
