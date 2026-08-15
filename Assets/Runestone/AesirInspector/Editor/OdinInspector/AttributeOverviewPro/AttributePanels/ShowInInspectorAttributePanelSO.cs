namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ShowInInspector 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class ShowInInspectorAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new ShowInInspectorAttributeData());
        }
    }
}
