using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class TitleExampleWithTitleSO : AttributeExampleSO<TitleExampleWithTitleSO>
    {
        [Title("Member Reference ($)")]
        public string dynamicTitle = "Title from Field";

        [Title("$dynamicTitle")]
        public int referenceExample;

        [Title("Expression (@)")]
        [Title("@\"Current Date: \" + System.DateTime.Now.ToString(\"dd:MM:yyyy\")")]
        public int expressionExample;

        public override void AesirInspectorReset()
        {
            dynamicTitle = "Title from Field";
            referenceExample = 0;
            expressionExample = 0;
        }
    }
}
