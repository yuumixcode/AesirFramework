namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// HideIf 特性介绍面板。
    /// </summary>
    [Summary("HideIf 特性介绍面板，展示 HideIf 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Conditionals)]
    public class HideIfAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new HideIfAttributeData());
        }
    }
}
