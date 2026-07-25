namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// TabGroup 特性介绍面板。
    /// </summary>
    [Summary("TabGroup 特性介绍面板，展示 TabGroup 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Groups)]
    public class TabGroupAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new TabGroupAttributeData());
        }
    }
}
