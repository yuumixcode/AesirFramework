using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class DetailedInfoBoxExampleWithMessageSO : AttributeExampleSO<DetailedInfoBoxExampleWithMessageSO>
    {
        [Title("Member Reference ($)")]
        public string messageField = "Dynamic message from field";

        [DetailedInfoBox("$messageField", "Details for member reference")]
        public int fieldReferenceExample;

        [Title("Expression (@)")]
        [DetailedInfoBox("@\"Dynamic: \" + System.DateTime.Now.ToString(\"HH:mm:ss\")",
            "Expression-based message")]
        public int expressionExample;

        [Title("Method Name ($)")]
        [DetailedInfoBox("$GetMessage", "Details for method reference")]
        public int methodNameExample;

        public string GetMessage() => "Message from method";

        public override void AesirInspectorReset()
        {
            messageField = "Dynamic message from field";
            fieldReferenceExample = 0;
            expressionExample = 0;
            methodNameExample = 0;
        }
    }
}
