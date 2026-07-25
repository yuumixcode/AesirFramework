using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// LabelText 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class LabelTextExampleSO : AttributeExampleSO<LabelTextExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        [LabelText("Custom Label")]
        public int customLabel = 1;

        [FoldoutGroup("Member Reference ($)")]
        [LabelText("$dynamicLabel")]
        public string labelFromMember = "The label above is dynamic";

        public string dynamicLabel = "Dynamic Label Text";

        [FoldoutGroup("Expression (@)")]
        [LabelText("@\"Current Time: \" + DateTime.Now.ToString(\"HH:mm:ss\")")]
        public string expressionLabel;

        [FoldoutGroup("Parameter: NicifyText")]
        [LabelText("m_myField", true)]
        public int nicifiedField = 10;

        [FoldoutGroup("Parameter: SdfIcon")]
        [LabelText("Heart Icon", SdfIconType.HeartFill, IconColor = "red")]
        public int iconLabel = 100;

        public override void AesirInspectorReset()
        {
            customLabel = 1;
            labelFromMember = "The label above is dynamic";
            dynamicLabel = "Dynamic Label Text";
            expressionLabel = "";
            nicifiedField = 10;
            iconLabel = 100;
        }
    }
}
