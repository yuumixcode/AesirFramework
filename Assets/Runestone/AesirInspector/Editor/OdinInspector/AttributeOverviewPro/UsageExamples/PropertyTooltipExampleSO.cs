using Sirenix.OdinInspector;

#pragma warning disable CS0414 // 字段已被赋值，但它的值从未被使用
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

        string _tooltipText = "This is a tooltip from a member variable.";

        public override void AesirInspectorReset()
        {
            simpleTooltip = 0;
            referencedTooltip = 0;
            expressionTooltip = 0;
        }
    }
}
