namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Required 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Validation)]
    public class RequiredAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new RequiredAttributeData());
        }
    }
}
