using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class BoxGroupExampleSO : AttributeExampleSO<BoxGroupExampleSO>
    {
        [Title("No Parameters")]
        [BoxGroup("My Group")]
        public int a;

        [BoxGroup("My Group")]
        public int b;

        [Title("Parameter: ShowLabel (False)")]
        [BoxGroup("No Label", false)]
        public int c;

        [Title("Parameter: CenterLabel")]
        [BoxGroup("Centered Label", centerLabel: true)]
        public int d;

        [Title("Parameter: LabelText")]
        [BoxGroup("Custom Title", LabelText = "This is a Box Group")]
        public int e;

        public override void AesirInspectorReset()
        {
            a = 0;
            b = 0;
            c = 0;
            d = 0;
            e = 0;
        }
    }
}
