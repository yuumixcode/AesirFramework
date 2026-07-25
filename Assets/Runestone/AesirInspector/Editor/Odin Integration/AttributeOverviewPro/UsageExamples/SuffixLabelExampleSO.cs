using Sirenix.OdinInspector;

#pragma warning disable CS0414 // 字段已被赋值，但它的值从未被使用
namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class SuffixLabelExampleSO : AttributeExampleSO<SuffixLabelExampleSO>
    {
        [Title("No Parameters")]
        [SuffixLabel("Unit: meters")]
        public float distance;

        [Title("Parameter: Overlay")]
        [SuffixLabel("Percentage", true)]
        public float progress;

        [Title("Member Reference ($)")]
        [SuffixLabel("$_dynamicLabel")]
        public string dynamicProperty;

        [Title("Expression (@)")]
        [SuffixLabel("@\"Current Length: \" + (dynamicProperty == null ? 0 : dynamicProperty.Length)")]
        public string expressionProperty;

        readonly string _dynamicLabel = "Dynamic Suffix";

        public override void AesirInspectorReset()
        {
            distance = 0;
            progress = 0.5f;
            dynamicProperty = "Hello";
            expressionProperty = "World";
        }
    }
}
