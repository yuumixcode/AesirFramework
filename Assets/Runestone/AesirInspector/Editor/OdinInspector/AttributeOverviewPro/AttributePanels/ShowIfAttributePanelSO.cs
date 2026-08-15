namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ShowIf 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Conditionals)]
    public class ShowIfAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new ShowIfAttributeData());
        }
    }
}
