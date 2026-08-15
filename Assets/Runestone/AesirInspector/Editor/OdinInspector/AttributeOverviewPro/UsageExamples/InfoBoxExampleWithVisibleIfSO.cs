using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class InfoBoxExampleWithVisibleIfSO : AttributeExampleSO<InfoBoxExampleWithVisibleIfSO>
    {
        [Title("Member Reference ($)")]
        public bool toggleInfoBox;

        [InfoBox("This box is only visible when toggleInfoBox is true.", "toggleInfoBox")]
        public int referenceExample;

        [Title("Expression (@)")]
        [InfoBox("Visible when current second is even.", "@DateTime.Now.Second % 2 == 0")]
        public int expressionExample;

        public override void AesirInspectorReset()
        {
            toggleInfoBox = false;
            referenceExample = 0;
            expressionExample = 0;
        }
    }
}
