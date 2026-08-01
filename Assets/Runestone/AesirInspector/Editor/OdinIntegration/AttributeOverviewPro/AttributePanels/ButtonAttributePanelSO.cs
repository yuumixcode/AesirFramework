namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Button 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Buttons)]
    public class ButtonAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new ButtonAttributeData());
        }
    }
}
