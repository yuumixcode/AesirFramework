using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// MinValue 特性案例。
    /// </summary>
    [AesirExample]
    internal class MinValueExampleSO : AttributeExampleSO<MinValueExampleSO>
    {
        [Title("No Parameters")]
        [MinValue(0)]
        public int MinimumZero;

        [Title("Member Reference ($)")]
        [MinValue("$DynamicMin")]
        public float DynamicMinimum;

        public float DynamicMin = 10;

        public override void AesirInspectorReset()
        {
            MinimumZero = 0;
            DynamicMinimum = 10;
            DynamicMin = 10;
        }
    }
}
