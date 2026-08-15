namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ListDrawerSettings 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Collections)]
    public class ListDrawerSettingsAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new ListDrawerSettingsAttributeData());
        }
    }
}
