using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// MinMaxSlider 特性案例。
    /// </summary>
    [AesirExample]
    internal class MinMaxSliderExampleSO : AttributeExampleSO<MinMaxSliderExampleSO>
    {
        [Title("No Parameters")]
        [MinMaxSlider(-10f, 10f, true)]
        public Vector2 Vector2Range;

        [MinMaxSlider(0, 100)]
        public Vector2Int Vector2IntRange;

        [Title("Member Reference ($)")]
        public float DynamicMin;

        public float DynamicMax = 10;

        [MinMaxSlider("$DynamicMin", "$DynamicMax", true)]
        public Vector2 DynamicRange;

        public override void AesirInspectorReset()
        {
            Vector2Range = new Vector2(-2, 2);
            Vector2IntRange = new Vector2Int(20, 80);
            DynamicMin = 0;
            DynamicMax = 10;
            DynamicRange = new Vector2(3, 7);
        }
    }
}
