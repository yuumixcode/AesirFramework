using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// MaxValue 特性案例。
    /// </summary>
    [AesirExample]
    internal class MaxValueExampleSO : AttributeExampleSO<MaxValueExampleSO>
    {
        [Title("No Parameters")]
        [MaxValue(100)]
        public int MaximumHundred;

        [Title("Member Reference ($)")]
        [MaxValue("$DynamicMax")]
        public float DynamicMaximum;

        public float DynamicMax = 50;

        public override void AesirInspectorReset()
        {
            MaximumHundred = 100;
            DynamicMaximum = 50;
            DynamicMax = 50;
        }
    }
}
