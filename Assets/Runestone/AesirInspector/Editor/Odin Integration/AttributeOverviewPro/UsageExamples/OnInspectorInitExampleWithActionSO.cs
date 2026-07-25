using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class OnInspectorInitExampleWithActionSO : AttributeExampleSO<OnInspectorInitExampleWithActionSO>
    {
        [Title("Member Reference ($)")]
        public string initMethodName = "CustomInitMethod";

        [OnInspectorInit("$initMethodName")]
        public string memberReferenceExample;

        [Title("Expression (@)")]
        [OnInspectorInit("@Debug.Log(\"OnInspectorInit Action invoked via expression\", this)")]
        public string expressionExample;

        void CustomInitMethod()
        {
            Debug.Log("Custom init method called via member reference");
        }

        public override void AesirInspectorReset()
        {
            initMethodName = "CustomInitMethod";
            memberReferenceExample = string.Empty;
            expressionExample = string.Empty;
        }
    }
}
