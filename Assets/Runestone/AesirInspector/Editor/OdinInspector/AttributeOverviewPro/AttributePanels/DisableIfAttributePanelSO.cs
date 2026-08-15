namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// DisableIf 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Conditionals)]
    public class DisableIfAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new DisableIfAttributeData());
        }
    }
}
