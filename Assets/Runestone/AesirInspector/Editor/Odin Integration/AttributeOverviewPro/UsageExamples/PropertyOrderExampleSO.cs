using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// PropertyOrder 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class PropertyOrderExampleSO : AttributeExampleSO<PropertyOrderExampleSO>
    {
        [Title("No Parameters")]
        [PropertyOrder(1)]
        public string second = "I am drawn second (Order = 1)";

        [PropertyOrder(-1)]
        public string first = "I am drawn first, even though I'm defined later in code (Order = -1)";

        [PropertyOrder(10)]
        public string third = "I am drawn last (Order = 10)";

        [Title("Usage with Buttons")]
        [PropertyOrder(5)]
        [Button]
        void MiddleButton() => Debug.Log("Middle Button");

        public override void AesirInspectorReset()
        {
            second = "I am drawn second (Order = 1)";
            first = "I am drawn first, even though I'm defined later in code (Order = -1)";
            third = "I am drawn last (Order = 10)";
        }
    }
}
