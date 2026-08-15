using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// PropertyRange 特性案例。
    /// </summary>
    [AesirExample]
    internal class PropertyRangeExampleSO : AttributeExampleSO<PropertyRangeExampleSO>
    {
        [Title("No Parameters")]
        [PropertyRange(0, 100)]
        public int StaticRange = 50;

        [Title("Member Reference ($)")]
        [PropertyRange("$Min", "$Max")]
        public float DynamicRange = 5;

        public float Min;
        public float Max = 10;

        public override void AesirInspectorReset()
        {
            StaticRange = 50;
            DynamicRange = 5;
            Min = 0;
            Max = 10;
        }
    }
}
