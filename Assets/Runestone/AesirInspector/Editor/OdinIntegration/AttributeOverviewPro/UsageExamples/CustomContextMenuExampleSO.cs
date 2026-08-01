using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class CustomContextMenuExampleSO : AttributeExampleSO<CustomContextMenuExampleSO>
    {
        [Title("No Parameters")]
        [CustomContextMenu("Say Hello/Twice", "SayHello")]
        public int myProperty;

        void SayHello()
        {
            Debug.Log("Hello Twice");
        }

        public override void AesirInspectorReset()
        {
            myProperty = 0;
        }
    }
}
