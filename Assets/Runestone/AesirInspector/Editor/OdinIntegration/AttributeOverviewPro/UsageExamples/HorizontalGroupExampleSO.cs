using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class HorizontalGroupExampleSO : AttributeExampleSO<HorizontalGroupExampleSO>
    {
        [Title("No Parameters")]
        [HorizontalGroup("Group 1")]
        public int a;

        [HorizontalGroup("Group 1")]
        public int b;

        [HorizontalGroup("Group 1")]
        public int c;

        [Title("Parameter: Width (Relative)")]
        [HorizontalGroup("Split", 0.5f)]
        [BoxGroup("Split/Left")]
        public int left;

        [BoxGroup("Split/Right")]
        public int right;

        [Title("Parameter: Gap")]
        [HorizontalGroup("Gap", Gap = 20)]
        public int gap1;

        [HorizontalGroup("Gap")]
        public int gap2;

        [Title("Parameter: Width (Fixed)")]
        [HorizontalGroup("Fixed", Width = 100)]
        public int fixedWidth;

        [HorizontalGroup("Fixed")]
        public int flexibleWidth;

        public override void AesirInspectorReset()
        {
            a = 0;
            b = 0;
            c = 0;
            left = 0;
            right = 0;
            gap1 = 0;
            gap2 = 0;
            fixedWidth = 0;
            flexibleWidth = 0;
        }
    }
}
