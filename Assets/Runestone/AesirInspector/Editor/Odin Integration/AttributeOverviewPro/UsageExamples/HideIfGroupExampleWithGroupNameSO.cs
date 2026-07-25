using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class HideIfGroupExampleWithGroupNameSO : AttributeExampleSO<HideIfGroupExampleWithGroupNameSO>
    {
        [Title("Controls")]
        public bool toggle;

        public string groupName = "DynamicGroup";

        [Title("Member Reference ($)")]
        [HideIfGroup("$groupName", Condition = "toggle")]
        [BoxGroup("$groupName/Content")]
        public string content;

        [BoxGroup("$groupName/Content")]
        public int value;

        public override void AesirInspectorReset()
        {
            toggle = true;
            groupName = "DynamicGroup";
            content = string.Empty;
            value = 0;
        }
    }
}
