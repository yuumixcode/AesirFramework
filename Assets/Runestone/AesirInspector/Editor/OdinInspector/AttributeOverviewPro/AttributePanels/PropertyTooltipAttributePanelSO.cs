namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// PropertyTooltip 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class PropertyTooltipAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new PropertyTooltipAttributeData());
        }
    }
}
