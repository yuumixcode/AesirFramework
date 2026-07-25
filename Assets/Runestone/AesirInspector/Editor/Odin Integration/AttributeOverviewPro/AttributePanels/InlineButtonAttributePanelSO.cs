namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// InlineButton 特性介绍面板。
    /// </summary>
    [Summary("InlineButton 特性介绍面板，展示 InlineButton 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Buttons)]
    public class InlineButtonAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new InlineButtonAttributeData());
        }
    }
}
