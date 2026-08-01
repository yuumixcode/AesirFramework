using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class TitleGroupExampleWithGroupNameSO : AttributeExampleSO<TitleGroupExampleWithGroupNameSO>
    {
        [Title("Member Reference ($)")]
        public string groupNameField = "Dynamic Group";

        [TitleGroup("$groupNameField")]
        public int referenceExample;

        [Title("Expression (@)")]
        [TitleGroup("@\"Dynamic_\" + System.DateTime.Now.ToString(\"HH:mm\")")]
        public int expressionExample;

        public override void AesirInspectorReset()
        {
            groupNameField = "Dynamic Group";
            referenceExample = 0;
            expressionExample = 0;
        }
    }
}
