using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class HideLabelExampleSO : AttributeExampleSO<HideLabelExampleSO>
    {
        [Title("Standard Property")]
        public int normalProperty;

        [Title("No Parameters")]
        [HideLabel]
        public int hiddenLabelProperty;

        [Title("Usage in Groups")]
        [HorizontalGroup("Group")]
        [HideLabel]
        public int a;

        [HorizontalGroup("Group")]
        [HideLabel]
        public int b;

        public override void AesirInspectorReset()
        {
            normalProperty = 0;
            hiddenLabelProperty = 0;
            a = 0;
            b = 0;
        }
    }
}
