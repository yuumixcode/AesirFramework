namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// DictionaryDrawerSettings 特性介绍面板。
    /// </summary>
    [Summary("DictionaryDrawerSettings 特性介绍面板，展示 DictionaryDrawerSettings 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Collections)]
    public class DictionaryDrawerSettingsAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new DictionaryDrawerSettingsAttributeData());
        }
    }
}
