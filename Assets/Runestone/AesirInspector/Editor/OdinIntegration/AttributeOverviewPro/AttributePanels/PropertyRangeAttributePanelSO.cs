namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// PropertyRange 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Numbers)]
    public class PropertyRangeAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new PropertyRangeAttributeData());
        }
    }
}
