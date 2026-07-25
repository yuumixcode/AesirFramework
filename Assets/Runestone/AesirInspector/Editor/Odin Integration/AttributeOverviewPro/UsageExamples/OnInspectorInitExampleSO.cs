using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class OnInspectorInitExampleSO : AttributeExampleSO<OnInspectorInitExampleSO>
    {
        [Title("No Parameters")]
        [OnInspectorInit]
        public int initializedField;

        [Title("Parameter: Action (Method Name)")]
        [OnInspectorInit(nameof(InitializeMethod))]
        public string methodNameField;

        [Title("Parameter: Action (Expression)")]
        [OnInspectorInit("@fieldSetByExpression = \"Set by expression on init\"")]
        public string fieldSetByExpression;

        void InitializeMethod()
        {
            Debug.Log("OnInspectorInit: Initialize method called");
        }

        public override void AesirInspectorReset()
        {
            initializedField = 0;
            methodNameField = string.Empty;
            fieldSetByExpression = string.Empty;
        }
    }
}
