using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class ShowIfExampleSO : AttributeExampleSO<ShowIfExampleSO>
    {
        [Title("Controls")]
        public bool showFields;

        public InfoMessageType messageType;

        [Title("Parameter: Condition (bool)")]
        [ShowIf("showFields")]
        public int visibleWhenToggled;

        [Title("Parameter: Condition (Enum) & OptionalValue")]
        [ShowIf("messageType", InfoMessageType.Warning)]
        public int visibleWhenWarning;

        [Title("Expression (@)")]
        [ShowIf("@this.showFields && this.messageType == InfoMessageType.Error")]
        public int visibleWithExpression;

        public override void AesirInspectorReset()
        {
            showFields = false;
            messageType = InfoMessageType.Info;
            visibleWhenToggled = 0;
            visibleWhenWarning = 0;
            visibleWithExpression = 0;
        }
    }
}
