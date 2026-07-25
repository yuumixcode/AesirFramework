using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class InfoBoxExampleSO : AttributeExampleSO<InfoBoxExampleSO>
    {
        [Title("No Parameters")]
        [InfoBox("This is a default info box.")]
        public int defaultInfoBox;

        [Title("Parameter: MessageType (Warning)")]
        [InfoBox("This is a warning info box.", InfoMessageType.Warning)]
        public int warningInfoBox;

        [Title("Parameter: MessageType (Error)")]
        [InfoBox("This is an error info box.", InfoMessageType.Error)]
        public int errorInfoBox;

        [Title("Parameter: MessageType (None)")]
        [InfoBox("This info box has no icon.", InfoMessageType.None)]
        public int noIconInfoBox;

        [Title("Parameter: GUIAlwaysEnabled")]
        [ReadOnly]
        [InfoBox("This info box is always enabled, even if the property is read-only.",
            GUIAlwaysEnabled = true)]
        public int guiAlwaysEnabled;

        [Title("Parameter: Icon & IconColor")]
        [InfoBox("Custom icon and color.", Icon = SdfIconType.InfoCircle, IconColor = "cyan")]
        public int customIcon;

        public override void AesirInspectorReset()
        {
            defaultInfoBox = 0;
            warningInfoBox = 0;
            errorInfoBox = 0;
            noIconInfoBox = 0;
            guiAlwaysEnabled = 0;
            customIcon = 0;
        }
    }
}
