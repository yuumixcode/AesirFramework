using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class InfoBoxExampleWithMessageSO : AttributeExampleSO<InfoBoxExampleWithMessageSO>
    {
        [Title("Member Reference ($)")]
        public string messageField = "Dynamic message from field";

        [InfoBox("$messageField")]
        public int referenceExample;

        [Title("Expression (@)")]
        [InfoBox("@\"Current Time: \" + System.DateTime.Now.ToString(\"HH:mm:ss\")")]
        public int expressionExample;

        public override void AesirInspectorReset()
        {
            messageField = "Dynamic message from field";
            referenceExample = 0;
            expressionExample = 0;
        }
    }
}
