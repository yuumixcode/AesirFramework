namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// RequiredIn 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Validation)]
    public class RequiredInAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new RequiredInAttributeData());
        }
    }
}
