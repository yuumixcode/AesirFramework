using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class
        DetailedInfoBoxExampleWithVisibleIfSO : AttributeExampleSO<DetailedInfoBoxExampleWithVisibleIfSO>
    {
        [Title("Member Reference ($)")]
        public bool toggleVisibility;

        [DetailedInfoBox("Visible conditionally", "Toggle the field above to show/hide.",
            VisibleIf = "toggleVisibility")]
        public int fieldReferenceExample;

        [Title("Expression (@)")]
        [DetailedInfoBox("Visible when second is even", "Expression-based visibility.",
            VisibleIf = "@System.DateTime.Now.Second % 2 == 0")]
        public int expressionExample;

        public override void AesirInspectorReset()
        {
            toggleVisibility = false;
            fieldReferenceExample = 0;
            expressionExample = 0;
        }
    }
}
