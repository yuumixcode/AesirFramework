namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ShowInInspector 特性介绍面板。
    /// </summary>
    [Summary("ShowInInspector 特性介绍面板，展示 ShowInInspector 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class ShowInInspectorAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new ShowInInspectorAttributeData());
        }
    }
}
