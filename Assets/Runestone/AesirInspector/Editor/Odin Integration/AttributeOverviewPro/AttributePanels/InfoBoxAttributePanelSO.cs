namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// InfoBox 特性介绍面板。
    /// </summary>
    [Summary("InfoBox 特性介绍面板，展示 InfoBox 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class InfoBoxAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new InfoBoxAttributeData());
        }
    }
}
