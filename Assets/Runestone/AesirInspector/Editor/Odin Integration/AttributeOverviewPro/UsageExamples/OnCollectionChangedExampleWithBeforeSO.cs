using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class
        OnCollectionChangedExampleWithBeforeSO : AttributeExampleSO<OnCollectionChangedExampleWithBeforeSO>
    {
        [Title("Member Reference ($)")]
        public string beforeMethodName = "BeforeChange";

        [OnCollectionChanged(Before = "$beforeMethodName")]
        public List<string> memberReferenceExample = new List<string> { "A", "B", "C" };

        [Title("Expression (@)")]
        [OnCollectionChanged(Before = "@Debug.Log(\"A change is about to occur\", this)")]
        public List<string> expressionExample = new List<string> { "X", "Y", "Z" };

        void BeforeChange()
        {
            Debug.Log("Before change handler called");
        }

        public override void AesirInspectorReset()
        {
            beforeMethodName = "BeforeChange";
            memberReferenceExample = new List<string> { "A", "B", "C" };
            expressionExample = new List<string> { "X", "Y", "Z" };
        }
    }
}
