namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// EnableIf 特性介绍面板。
    /// </summary>
    [Summary("EnableIf 特性介绍面板，展示 EnableIf 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Conditionals)]
    public class EnableIfAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new EnableIfAttributeData());
        }
    }
}
