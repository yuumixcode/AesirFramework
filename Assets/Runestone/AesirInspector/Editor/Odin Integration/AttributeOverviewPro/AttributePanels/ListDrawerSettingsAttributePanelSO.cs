namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ListDrawerSettings 特性介绍面板。
    /// </summary>
    [Summary("ListDrawerSettings 特性介绍面板，展示 ListDrawerSettings 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Collections)]
    public class ListDrawerSettingsAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new ListDrawerSettingsAttributeData());
        }
    }
}
