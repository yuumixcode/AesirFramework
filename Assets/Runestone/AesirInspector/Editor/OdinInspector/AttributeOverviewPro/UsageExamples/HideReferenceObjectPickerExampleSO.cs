using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class HideReferenceObjectPickerExampleSO : AttributeExampleSO<HideReferenceObjectPickerExampleSO>
    {
        [Title("No Parameters")]
        [HideReferenceObjectPicker]
        public string hiddenPickerField;

        [Title("Shown Reference Picker (Default)")]
        public string shownPickerField;

        public override void AesirInspectorReset()
        {
            hiddenPickerField = string.Empty;
            shownPickerField = string.Empty;
        }
    }
}
