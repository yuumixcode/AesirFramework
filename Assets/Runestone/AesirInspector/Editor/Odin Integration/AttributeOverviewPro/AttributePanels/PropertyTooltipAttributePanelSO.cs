namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// PropertyTooltip 特性介绍面板。
    /// </summary>
    [Summary("PropertyTooltip 特性介绍面板，展示 PropertyTooltip 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class PropertyTooltipAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new PropertyTooltipAttributeData());
        }
    }
}
