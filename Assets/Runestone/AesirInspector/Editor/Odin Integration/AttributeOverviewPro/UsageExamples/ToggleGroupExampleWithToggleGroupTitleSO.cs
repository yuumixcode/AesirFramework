using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class
        ToggleGroupExampleWithToggleGroupTitleSO : AttributeExampleSO<
        ToggleGroupExampleWithToggleGroupTitleSO>
    {
        [Title("Member Reference ($)")]
        public string toggleTitleField = "Dynamic Toggle Title";

        [ToggleGroup(nameof(Toggle1), "$toggleTitleField")]
        public bool Toggle1;

        [ToggleGroup(nameof(Toggle1))]
        public int referenceExample;

        [Title("Expression (@)")]
        [ToggleGroup(nameof(Toggle2), "@\"Dynamic_\" + System.DateTime.Now.DayOfWeek")]
        public bool Toggle2;

        [ToggleGroup(nameof(Toggle2))]
        public int expressionExample;

        public override void AesirInspectorReset()
        {
            toggleTitleField = "Dynamic Toggle Title";
            Toggle1 = false;
            Toggle2 = false;
            referenceExample = 0;
            expressionExample = 0;
        }
    }
}
