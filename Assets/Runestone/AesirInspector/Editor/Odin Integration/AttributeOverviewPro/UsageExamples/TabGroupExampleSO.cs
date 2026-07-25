using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class TabGroupExampleSO : AttributeExampleSO<TabGroupExampleSO>
    {
        [Title("No Parameters")]
        [TabGroup("Tabs", "General")]
        public int a;

        [TabGroup("Tabs", "General")]
        public int b;

        [TabGroup("Tabs", "Settings")]
        public bool c;

        [TabGroup("Tabs", "Settings")]
        public float d;

        [Title("Parameter: SdfIcon")]
        [TabGroup("Icons", "Player", SdfIconType.PersonFill)]
        public string playerName;

        [TabGroup("Icons", "Inventory", SdfIconType.BriefcaseFill)]
        public int inventorySize;

        [Title("Parameter: UseAdaptiveHeight")]
        [TabGroup("Height", "Short")]
        public int shortTab;

        [TabGroup("Height", "Tall")]
        [DisplayAsString]
        public string tallTab = "\n\n\n\nTall Content";

        public override void AesirInspectorReset()
        {
            a = 0;
            b = 0;
            c = false;
            d = 0f;
            playerName = "";
            inventorySize = 0;
            shortTab = 0;
        }
    }
}
