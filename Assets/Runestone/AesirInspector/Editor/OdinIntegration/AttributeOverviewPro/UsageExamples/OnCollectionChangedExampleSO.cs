using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class OnCollectionChangedExampleSO : AttributeExampleSO<OnCollectionChangedExampleSO>
    {
        [Title("Parameter: Before & After")]
        [InfoBox("Edit the list to see OnCollectionChanged in effect")]
        [OnCollectionChanged("Before", "After")]
        public List<string> list = new List<string> { "Item 1", "Item 2", "Item 3" };

        void Before()
        {
            Debug.Log("OnCollectionChanged: Before change");
        }

        void After()
        {
            Debug.Log("OnCollectionChanged: After change");
        }

        public override void AesirInspectorReset()
        {
            list = new List<string> { "Item 1", "Item 2", "Item 3" };
        }
    }
}
