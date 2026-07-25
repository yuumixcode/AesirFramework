namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ShowIfGroup 特性介绍面板。
    /// </summary>
    [Summary("ShowIfGroup 特性介绍面板，展示 ShowIfGroup 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Conditionals)]
    public class ShowIfGroupAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new ShowIfGroupAttributeData());
        }
    }
}
