namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ShowIfGroup 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Conditionals)]
    public class ShowIfGroupAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new ShowIfGroupAttributeData());
        }
    }
}
