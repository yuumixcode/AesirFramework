using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Button 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class ButtonExampleSO : AttributeExampleSO<ButtonExampleSO>
    {
        public string dynamicButtonName = "Click Me!";

        [Title("No Parameters")]
        [Button("Simple Button")]
        void SimpleButton() => Debug.Log("Button Clicked!");

        [Title("Member Reference ($)")]
        [Button("$dynamicButtonName")]
        void DynamicButton() => Debug.Log("Dynamic Button Clicked!");

        [Title("Parameter: ButtonSize")]
        [Button(ButtonSizes.Small)]
        void SmallButton() { }

        [Button(ButtonSizes.Large)]
        void LargeButton() { }

        [Button(50)]
        void CustomHeightButton() { }

        [Title("Parameter: SdfIcon")]
        [Button(SdfIconType.HeartFill, IconAlignment.LeftOfText)]
        void HeartButton() { }

        [Title("Method Parameters")]
        [Button(ButtonStyle.Box)]
        void MethodWithParameters(string text, int count)
        {
            Debug.Log(string.Format("Text: {0}, Count: {1}", text, count));
        }

        public override void AesirInspectorReset()
        {
            dynamicButtonName = "Click Me!";
        }
    }
}
