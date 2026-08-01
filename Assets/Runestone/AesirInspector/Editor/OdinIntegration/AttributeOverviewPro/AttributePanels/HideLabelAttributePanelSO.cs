namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// HideLabel 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class HideLabelAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new HideLabelAttributeData());
        }
    }
}
