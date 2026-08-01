using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class MultiLinePropertyExampleSO : AttributeExampleSO<MultiLinePropertyExampleSO>
    {
        [Title("No Parameters")]
        [MultiLineProperty]
        public string defaultMultiLine = "Line 1\nLine 2\nLine 3";

        [Title("Parameter: LineCount = 10")]
        [MultiLineProperty(10)]
        public string tallMultiLine = "This text area spans 10 lines";

        [Title("Parameter: ShowLabel = false")]
        [HideLabel]
        [MultiLineProperty(5)]
        public string hiddenLabelMultiLine = "Without label and 5 lines tall";

        public override void AesirInspectorReset()
        {
            defaultMultiLine = "Line 1\nLine 2\nLine 3";
            tallMultiLine = "This text area spans 10 lines";
            hiddenLabelMultiLine = "Without label and 5 lines tall";
        }
    }
}
