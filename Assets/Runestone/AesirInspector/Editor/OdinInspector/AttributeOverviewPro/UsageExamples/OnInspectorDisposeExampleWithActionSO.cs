using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class
        OnInspectorDisposeExampleWithActionSO : AttributeExampleSO<OnInspectorDisposeExampleWithActionSO>
    {
        [Title("Member Reference ($)")]
        public string disposeMethodName = "CustomDisposeMethod";

        [OnInspectorDispose("$disposeMethodName")]
        public string memberReferenceExample;

        [Title("Expression (@)")]
        [OnInspectorDispose("@Debug.Log(\"OnInspectorDispose invoked via expression\", this)")]
        public string expressionExample;

        void CustomDisposeMethod()
        {
            Debug.Log("Custom dispose method called via member reference");
        }

        public override void AesirInspectorReset()
        {
            disposeMethodName = "CustomDisposeMethod";
            memberReferenceExample = string.Empty;
            expressionExample = string.Empty;
        }
    }
}
