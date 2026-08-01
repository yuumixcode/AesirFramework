using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class DisplayAsStringExampleSO : AttributeExampleSO<DisplayAsStringExampleSO>
    {
        [Title("No Parameters")]
        [DisplayAsString]
        public string label = "This is a string displayed as a label";

        [Title("Parameter: FontSize")]
        [DisplayAsString(16)]
        public string largeLabel = "Large font text";

        [Title("Parameter: EnableRichText")]
        [DisplayAsString(true)]
        public string richTextLabel = "<color=red>Red</color> <color=green>Green</color> <b>Bold</b>";

        [Title("Parameter: Alignment")]
        [DisplayAsString(TextAlignment.Center)]
        public string centerLabel = "Center aligned text";

        [Title("Parameter: Overflow")]
        [DisplayAsString(false)]
        [BoxGroup("Overflow False")]
        public string longLabel = "This is a very very very very very very very long text";

        public override void AesirInspectorReset()
        {
            label = "This is a string displayed as a label";
            largeLabel = "Large font text";
            richTextLabel = "<color=red>Red</color> <color=green>Green</color> <b>Bold</b>";
            centerLabel = "Center aligned text";
            longLabel = "This is a very very very very very very very long text";
        }
    }
}
