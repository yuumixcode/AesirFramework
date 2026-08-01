using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class ToggleGroupExampleSO : AttributeExampleSO<ToggleGroupExampleSO>
    {
        [Title("No Parameters")]
        [ToggleGroup(nameof(Toggle1))]
        public bool Toggle1;

        [ToggleGroup(nameof(Toggle1))]
        public int field1;

        [ToggleGroup(nameof(Toggle1))]
        public int field2;

        [Title("Parameter: ToggleGroupTitle")]
        [ToggleGroup(nameof(Toggle2), "Custom Title")]
        public bool Toggle2;

        [ToggleGroup(nameof(Toggle2))]
        public int field3;

        [Title("Parameter: Order")]
        [ToggleGroup(nameof(Toggle3), 10)]
        public bool Toggle3;

        [ToggleGroup(nameof(Toggle3))]
        public int field4;

        [Title("Parameter: Order")]
        [ToggleGroup(nameof(Toggle4), "Toggle 4")]
        public bool Toggle4;

        [ToggleGroup(nameof(Toggle4))]
        public int field5;

        [Title("Parameter: CollapseOthersOnExpand")]
        [ToggleGroup(nameof(Toggle5), CollapseOthersOnExpand = true)]
        public bool Toggle5;

        [ToggleGroup(nameof(Toggle5))]
        public int field6;

        public override void AesirInspectorReset()
        {
            Toggle1 = false;
            Toggle2 = false;
            Toggle3 = false;
            Toggle4 = false;
            Toggle5 = false;
            field1 = 0;
            field2 = 0;
            field3 = 0;
            field4 = 0;
            field5 = 0;
            field6 = 0;
        }
    }
}
