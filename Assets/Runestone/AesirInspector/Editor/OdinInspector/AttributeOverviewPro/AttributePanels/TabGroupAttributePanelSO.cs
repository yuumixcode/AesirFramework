namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// TabGroup 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Groups)]
    public class TabGroupAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new TabGroupAttributeData());
        }
    }
}
