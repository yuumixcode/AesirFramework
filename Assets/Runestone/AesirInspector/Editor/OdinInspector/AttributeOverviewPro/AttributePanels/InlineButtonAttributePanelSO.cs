namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// InlineButton 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Buttons)]
    public class InlineButtonAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new InlineButtonAttributeData());
        }
    }
}
