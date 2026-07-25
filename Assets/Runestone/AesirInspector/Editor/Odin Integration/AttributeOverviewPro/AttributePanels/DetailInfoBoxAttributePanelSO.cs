namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// DetailInfoBox 特性介绍面板。
    /// </summary>
    [Summary("DetailInfoBox 特性介绍面板，展示 DetailInfoBox 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class DetailInfoBoxAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new DetailedInfoBoxAttributeData());
        }
    }
}
