namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Indent 特性介绍面板。
    /// </summary>
    [Summary("Indent 特性介绍面板，展示 Indent 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class IndentAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new IndentAttributeData());
        }
    }
}
