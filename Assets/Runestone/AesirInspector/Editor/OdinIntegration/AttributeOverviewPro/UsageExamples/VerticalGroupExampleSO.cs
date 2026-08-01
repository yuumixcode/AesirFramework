using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class VerticalGroupExampleSO : AttributeExampleSO<VerticalGroupExampleSO>
    {
        [Title("No Parameters")]
        [HorizontalGroup("Split")]
        [VerticalGroup("Split/Left")]
        public int left1;

        [VerticalGroup("Split/Left")]
        public int left2;

        [VerticalGroup("Split/Right")]
        public int right1;

        [VerticalGroup("Split/Right")]
        public int right2;

        [Title("Parameter: PaddingTop, PaddingBottom")]
        [VerticalGroup("Padded", PaddingTop = 10, PaddingBottom = 10)]
        public int padded1;

        [VerticalGroup("Padded")]
        public int padded2;

        public override void AesirInspectorReset()
        {
            left1 = 0;
            left2 = 0;
            right1 = 0;
            right2 = 0;
            padded1 = 0;
            padded2 = 0;
        }
    }
}
