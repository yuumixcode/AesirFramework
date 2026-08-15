using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// OnStateUpdate 特性案例。
    /// </summary>
    [AesirExample]
    internal class OnStateUpdateExampleSO : AttributeExampleSO<OnStateUpdateExampleSO>
    {
        [Title("Controls")]
        public bool ToggleField;

        [Title("Expression (@)")]
        [OnStateUpdate("@$property.State.Visible = ToggleField")]
        public string VisibleIfToggled;

        [Title("Parameter: Action (InspectorProperty property)")]
        [OnStateUpdate("UpdateState")]
        public int DisabledIfZero;

        void UpdateState(InspectorProperty property)
        {
            property.State.Enabled = DisabledIfZero != 0;
        }

        public override void AesirInspectorReset()
        {
            ToggleField = true;
            VisibleIfToggled = "Hello";
            DisabledIfZero = 1;
        }
    }
}
