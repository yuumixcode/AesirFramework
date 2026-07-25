namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Required 特性介绍面板。
    /// </summary>
    [Summary("Required 特性介绍面板，展示 Required 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Validation)]
    public class RequiredAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new RequiredAttributeData());
        }
    }
}
