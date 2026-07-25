using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class TitleGroupExampleWithSubtitleSO : AttributeExampleSO<TitleGroupExampleWithSubtitleSO>
    {
        [Title("Member Reference ($)")]
        public string subtitleField = "Subtitle from Field";

        [TitleGroup("Main Title", "$subtitleField")]
        public int referenceExample;

        [Title("Expression (@)")]
        [TitleGroup("Main Title", "@\"Current Time: \" + System.DateTime.Now.ToString(\"HH:mm:ss\")")]
        public int expressionExample;

        public override void AesirInspectorReset()
        {
            subtitleField = "Subtitle from Field";
            referenceExample = 0;
            expressionExample = 0;
        }
    }
}
