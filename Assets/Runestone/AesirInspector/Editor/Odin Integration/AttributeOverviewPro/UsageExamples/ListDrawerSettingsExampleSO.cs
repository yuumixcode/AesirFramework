using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class ListDrawerSettingsExampleSO : AttributeExampleSO<ListDrawerSettingsExampleSO>
    {
        [Title("Parameter: IsReadOnly")]
        [ListDrawerSettings(IsReadOnly = true)]
        public int[] readOnlyList = { 1, 2, 3 };

        [Title("Parameter: NumberOfItemsPerPage")]
        [ListDrawerSettings(NumberOfItemsPerPage = 5, ShowItemCount = true)]
        public List<int> pagedList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        [Title("Parameter: ListElementLabelName")]
        [ListDrawerSettings(ListElementLabelName = "name", ShowIndexLabels = true)]
        public List<SomeStruct> namedElements = new List<SomeStruct>
        {
            new SomeStruct { name = "First" },
            new SomeStruct { name = "Second" }
        };

        [Title("Parameter: DraggableItems, HideRemoveButton")]
        [ListDrawerSettings(DraggableItems = false, HideRemoveButton = true)]
        public List<int> restrictedList = new List<int> { 1, 2, 3 };

        [Title("Parameter: ElementColor")]
        [ListDrawerSettings(ElementColor = "lightblue")]
        public List<int> coloredList = new List<int> { 1, 2, 3 };

        public override void AesirInspectorReset()
        {
            readOnlyList = new[] { 1, 2, 3 };
            pagedList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            namedElements = new List<SomeStruct>
            {
                new SomeStruct { name = "First" },
                new SomeStruct { name = "Second" }
            };
            restrictedList = new List<int> { 1, 2, 3 };
            coloredList = new List<int> { 1, 2, 3 };
        }

        [Serializable]
        public struct SomeStruct
        {
            public string name;
            public int value;
        }
    }
}
