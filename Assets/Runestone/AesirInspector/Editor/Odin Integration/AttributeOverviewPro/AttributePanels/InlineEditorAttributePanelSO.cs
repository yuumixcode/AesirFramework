namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// InlineEditor 特性介绍面板。
    /// </summary>
    [Summary("InlineEditor 特性介绍面板，展示 InlineEditor 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class InlineEditorAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new InlineEditorAttributeData());
        }
    }
}
