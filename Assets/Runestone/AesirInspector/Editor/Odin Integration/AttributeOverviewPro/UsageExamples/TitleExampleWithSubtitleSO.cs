using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class TitleExampleWithSubtitleSO : AttributeExampleSO<TitleExampleWithSubtitleSO>
    {
        [Title("Member Reference ($)")]
        public string dynamicSubtitle = "Subtitle from Field";

        [Title("Main Title", "$dynamicSubtitle")]
        public int referenceExample;

        [Title("Expression (@)")]
        [Title("Main Title", "@\"Current Time: \" + System.DateTime.Now.ToString(\"HH:mm:ss\")")]
        public int expressionExample;

        public override void AesirInspectorReset()
        {
            dynamicSubtitle = "Subtitle from Field";
            referenceExample = 0;
            expressionExample = 0;
        }
    }
}
