using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class OnInspectorDisposeExampleSO : AttributeExampleSO<OnInspectorDisposeExampleSO>
    {
        [Title("No Parameters")]
        [OnInspectorDispose]
        public int disposedField;

        [Title("Parameter: Action (Expression)")]
        [OnInspectorDispose("@Debug.Log(\"OnInspectorDispose invoked\", this)")]
        public string expressionField;

        public override void AesirInspectorReset()
        {
            disposedField = 0;
            expressionField = "OnInspectorDispose trigger";
        }
    }
}
