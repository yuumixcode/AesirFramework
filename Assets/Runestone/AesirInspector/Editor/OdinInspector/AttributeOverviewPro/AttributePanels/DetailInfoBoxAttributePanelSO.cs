namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// DetailInfoBox 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class DetailInfoBoxAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new DetailedInfoBoxAttributeData());
        }
    }
}
