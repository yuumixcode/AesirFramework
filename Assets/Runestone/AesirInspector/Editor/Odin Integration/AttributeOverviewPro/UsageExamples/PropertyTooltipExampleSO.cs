using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class PropertyTooltipExampleSO : AttributeExampleSO<PropertyTooltipExampleSO>
    {
        [Title("No Parameters")]
        [PropertyTooltip("This is a simple tooltip.")]
        public int simpleTooltip;

        [Title("Member Reference ($)")]
        [PropertyTooltip("Supports $ reference: $TooltipText")]
        public int referencedTooltip;

        [Title("Expression (@)")]
        [PropertyTooltip("@\"Current Time: \" + DateTime.Now.ToString()")]
        public int expressionTooltip;

        string TooltipText = "This is a tooltip from a member variable.";

        public override void AesirInspectorReset()
        {
            simpleTooltip = 0;
            referencedTooltip = 0;
            expressionTooltip = 0;
        }
    }
}
