namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// DictionaryDrawerSettings 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Collections)]
    public class DictionaryDrawerSettingsAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new DictionaryDrawerSettingsAttributeData());
        }
    }
}
