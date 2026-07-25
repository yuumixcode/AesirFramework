using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class InlineButtonExampleSO : AttributeExampleSO<InlineButtonExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        [InlineButton("OnButtonClick", "Click Me")]
        public int inlineButton;

        [FoldoutGroup("Multiple Buttons")]
        [InlineButton("A")]
        [InlineButton("B", "Custom Name")]
        public int multiButtons;

        [FoldoutGroup("Parameter: SdfIcon")]
        [InlineButton("C", SdfIconType.Dice6Fill, "Random")]
        public int iconButton;

        [FoldoutGroup("Advanced Usage")]
        public bool showButton;

        [FoldoutGroup("Advanced Usage")]
        [InlineButton("C", "Conditional", ShowIf = "showButton")]
        public int conditionalButton;

        [FoldoutGroup("Advanced Usage")]
        [InlineButton("C", "Colored", ButtonColor = "lightgreen", TextColor = "darkblue")]
        public int coloredButton;

        void OnButtonClick() => Debug.Log("Button Clicked!");
        void A() => Debug.Log("A Clicked!");
        void B() => Debug.Log("B Clicked!");
        void C() => Debug.Log("C Clicked!");

        public override void AesirInspectorReset()
        {
            inlineButton = 0;
            multiButtons = 0;
            iconButton = 0;
            showButton = true;
            conditionalButton = 0;
            coloredButton = 0;
        }
    }
}
