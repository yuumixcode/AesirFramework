using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class ShowPropertyResolverExampleSO : OdinAttributeExampleSO<ShowPropertyResolverExampleSO>
    {
        [Title("No Parameters")]
        [ShowPropertyResolver]
        public Dictionary<int, Vector3> MyDictionary;

        [ShowPropertyResolver]
        public List<int> myList = new List<int>();

        [ShowPropertyResolver]
        public string myString = "Hello";

        public override void AesirInspectorReset()
        {
            MyDictionary = new Dictionary<int, Vector3>();
            myList = new List<int>();
            myString = "Hello";
        }
    }
}
