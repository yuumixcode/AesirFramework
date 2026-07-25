namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// PropertyOrder 特性介绍面板。
    /// </summary>
    [Summary("PropertyOrder 特性介绍面板，展示 PropertyOrder 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class PropertyOrderAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new PropertyOrderAttributeData());
        }
    }
}
