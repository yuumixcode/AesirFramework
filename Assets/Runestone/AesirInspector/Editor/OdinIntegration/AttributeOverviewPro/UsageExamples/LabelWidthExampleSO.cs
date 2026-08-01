using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class LabelWidthExampleSO : AttributeExampleSO<LabelWidthExampleSO>
    {
        [Title("Standard Property")]
        public int defaultWidth;

        [Title("Parameter: Width (Fixed)")]
        [LabelWidth(50)]
        public int thinLabel;

        [LabelWidth(200)]
        public int wideLabel;

        [Title("Parameter: Width (Relative)")]
        [LabelWidth(0.5f)]
        public int proportionalLabel;

        public override void AesirInspectorReset()
        {
            defaultWidth = 0;
            thinLabel = 0;
            wideLabel = 0;
            proportionalLabel = 0;
        }
    }
}
