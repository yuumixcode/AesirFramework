using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class DetailedInfoBoxExampleWithDetailsSO : AttributeExampleSO<DetailedInfoBoxExampleWithDetailsSO>
    {
        public string detailsField = "Dynamic details from field";

        [Title("Member Reference ($)")]
        [DetailedInfoBox("Message with field details", "$detailsField")]
        public int fieldReferenceExample;

        [Title("Expression (@)")]
        [DetailedInfoBox("Message with expression details", "@\"Details: \" + System.DateTime.Now.DayOfWeek")]
        public int expressionExample;

        [Title("Method Name ($)")]
        [DetailedInfoBox("Message with method details", "$GetDetails")]
        public int methodNameExample;

        public string GetDetails() => "Details from method";

        public override void AesirInspectorReset()
        {
            detailsField = "Dynamic details from field";
            fieldReferenceExample = 0;
            expressionExample = 0;
            methodNameExample = 0;
        }
    }
}
