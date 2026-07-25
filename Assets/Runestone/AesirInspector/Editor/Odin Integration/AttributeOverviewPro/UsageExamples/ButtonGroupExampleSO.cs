using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class ButtonGroupExampleSO : AttributeExampleSO<ButtonGroupExampleSO>
    {
        [Title("No Parameters")]
        [ButtonGroup]
        void ButtonA() => Debug.Log("Button A Clicked");

        [ButtonGroup]
        void ButtonB() => Debug.Log("Button B Clicked");

        [ButtonGroup]
        void ButtonC() => Debug.Log("Button C Clicked");

        [Title("Parameter: Order")]
        [ButtonGroup("Ordered", Order = 20)]
        void LateButton() => Debug.Log("Late Button (Order=20)");

        [ButtonGroup("Ordered", Order = 10)]
        void EarlyButton() => Debug.Log("Early Button (Order=10)");

        [Title("Parameter: ButtonHeight")]
        [ButtonGroup(ButtonHeight = 40)]
        void TallButton() => Debug.Log("Tall Button (ButtonHeight=40)");

        public override void AesirInspectorReset() { }
    }
}
