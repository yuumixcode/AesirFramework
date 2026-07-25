namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Button 特性介绍面板。
    /// </summary>
    [Summary("Button 特性介绍面板，展示 Button 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Buttons)]
    public class ButtonAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new ButtonAttributeData());
        }
    }
}
