namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// InfoBox 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class InfoBoxAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new InfoBoxAttributeData());
        }
    }
}
