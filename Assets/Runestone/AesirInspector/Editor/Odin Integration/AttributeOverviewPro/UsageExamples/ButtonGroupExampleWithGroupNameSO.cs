using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class ButtonGroupExampleWithGroupNameSO : AttributeExampleSO<ButtonGroupExampleWithGroupNameSO>
    {
        [Title("Member Reference ($)")]
        public string groupNameField = "Custom Group";

        [ButtonGroup("$groupNameField")]
        void ReferenceMethod() { }

        [Title("Expression (@)")]
        [ButtonGroup("@\"Group_\" + System.DateTime.Now.DayOfWeek")]
        void ExpressionMethod() { }

        public override void AesirInspectorReset()
        {
            groupNameField = "Custom Group";
        }
    }
}
