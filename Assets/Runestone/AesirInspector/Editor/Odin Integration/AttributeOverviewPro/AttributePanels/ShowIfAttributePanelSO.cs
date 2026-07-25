namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ShowIf 特性介绍面板。
    /// </summary>
    [Summary("ShowIf 特性介绍面板，展示 ShowIf 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Conditionals)]
    public class ShowIfAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new ShowIfAttributeData());
        }
    }
}
