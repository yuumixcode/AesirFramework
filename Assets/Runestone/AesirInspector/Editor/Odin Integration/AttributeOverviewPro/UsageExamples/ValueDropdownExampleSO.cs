using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class ValueDropdownExampleSO : AttributeExampleSO<ValueDropdownExampleSO>
    {
        static readonly int[] TextureSizes = { 256, 512, 1024 };

        static readonly IEnumerable FriendlyTextureSizes = new ValueDropdownList<int>
        {
            { "Small (256)", 256 },
            { "Medium (512)", 512 },
            { "Large (1024)", 1024 }
        };

        static readonly IEnumerable TreeViewOfInts = new ValueDropdownList<int>
        {
            { "Group A/Node 1", 1 },
            { "Group A/Node 2", 2 },
            { "Group B/SubGroup/Node 3", 3 },
            { "Group C/Node 4", 4 },
            { "Group C/Node 5", 5 },
            { "Node 6", 6 }
        };

        [Title("No Parameters")]
        [ValueDropdown("TextureSizes")]
        public int someSize;

        [ValueDropdown("FriendlyTextureSizes")]
        public int friendlySize;

        [Title("Parameter: ExpandAllMenuItems")]
        [ValueDropdown("TreeViewOfInts", ExpandAllMenuItems = true)]
        public List<int> intTreeView = new List<int> { 1, 6 };

        [Title("Parameter: IsUniqueList")]
        [ValueDropdown("GetAllSceneObjects", IsUniqueList = true)]
        public List<GameObject> uniqueSceneObjects;

        [Title("Parameter: AppendNextDrawer")]
        [ValueDropdown("FriendlyTextureSizes", AppendNextDrawer = true, DisableGUIInAppendedDrawer = true)]
        public int appendedSize;

        IEnumerable GetAllSceneObjects()
        {
            var objects =
                FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var result = new ValueDropdownList<GameObject>();
            foreach (var go in objects)
            {
                result.Add(go.name, go);
            }

            return result;
        }

        public override void AesirInspectorReset()
        {
            someSize = 256;
            friendlySize = 512;
            intTreeView = new List<int> { 1, 6 };
            uniqueSceneObjects = new List<GameObject>();
            appendedSize = 1024;
        }
    }
}
