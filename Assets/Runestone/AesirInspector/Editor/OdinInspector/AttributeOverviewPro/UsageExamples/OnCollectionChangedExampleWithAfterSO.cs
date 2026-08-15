using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class
        OnCollectionChangedExampleWithAfterSO : AttributeExampleSO<OnCollectionChangedExampleWithAfterSO>
    {
        [Title("Member Reference ($)")]
        public string afterMethodName = "AfterChange";

        [OnCollectionChanged(After = "$afterMethodName")]
        public List<string> memberReferenceExample = new List<string> { "A", "B", "C" };

        [Title("Expression (@)")]
        [OnCollectionChanged(After = "@Debug.Log(\"A change occurred\", this)")]
        public List<string> expressionExample = new List<string> { "X", "Y", "Z" };

        void AfterChange()
        {
            Debug.Log("After change handler called");
        }

        public override void AesirInspectorReset()
        {
            afterMethodName = "AfterChange";
            memberReferenceExample = new List<string> { "A", "B", "C" };
            expressionExample = new List<string> { "X", "Y", "Z" };
        }
    }
}
