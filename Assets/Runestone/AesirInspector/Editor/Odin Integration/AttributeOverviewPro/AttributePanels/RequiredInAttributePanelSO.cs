namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// RequiredIn 特性介绍面板。
    /// </summary>
    [Summary("RequiredIn 特性介绍面板，展示 RequiredIn 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Validation)]
    public class RequiredInAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new RequiredInAttributeData());
        }
    }
}
