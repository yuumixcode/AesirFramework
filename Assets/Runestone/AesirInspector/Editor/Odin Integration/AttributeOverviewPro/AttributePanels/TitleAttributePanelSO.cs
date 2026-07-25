namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Title 特性介绍面板。
    /// </summary>
    [Summary("Title 特性介绍面板，展示 Title 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class TitleAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new TitleAttributeData());
        }
    }
}
