namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// LabelText 特性介绍面板。
    /// </summary>
    [Summary("LabelText 特性介绍面板，展示 LabelText 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class LabelTextAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new LabelTextAttributeData());
        }
    }
}
