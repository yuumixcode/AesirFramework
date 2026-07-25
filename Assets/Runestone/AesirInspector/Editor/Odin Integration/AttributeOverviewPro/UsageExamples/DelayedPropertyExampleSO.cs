using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class DelayedPropertyExampleSO : AttributeExampleSO<DelayedPropertyExampleSO>
    {
        [Title("No Parameters")]
        [DelayedProperty]
        [OnValueChanged("OnValueChanged")]
        public int delayedInt;

        [Title("Standard Property")]
        [OnValueChanged("OnValueChanged")]
        public int normalInt;

        void OnValueChanged()
        {
            Debug.Log("Value changed!");
        }

        public override void AesirInspectorReset()
        {
            delayedInt = 0;
            normalInt = 0;
        }
    }
}
