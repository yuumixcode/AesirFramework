using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class ShowIfGroupExampleSO : AttributeExampleSO<ShowIfGroupExampleSO>
    {
        [Title("Controls")]
        public bool toggle;

        public InfoMessageType messageType;

        [Title("No Parameters")]
        [ShowIfGroup("toggle")]
        [BoxGroup("toggle/Shown Box")]
        public int a;

        [BoxGroup("toggle/Shown Box")]
        public int b;

        [Title("Parameter: Value")]
        [ShowIfGroup("toggle/messageType", Value = InfoMessageType.Info)]
        [BoxGroup("toggle/messageType/Border", ShowLabel = false)]
        public string fieldName;

        [Title("Parameter: Condition")]
        [ShowIfGroup("DemoGroup", Condition = "toggle")]
        public GameObject gameObject;

        public override void AesirInspectorReset()
        {
            toggle = true;
            messageType = InfoMessageType.Info;
            a = 0;
            b = 0;
            fieldName = string.Empty;
            gameObject = null;
        }
    }
}
