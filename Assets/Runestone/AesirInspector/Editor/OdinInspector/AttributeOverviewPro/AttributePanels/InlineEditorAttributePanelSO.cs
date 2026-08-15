namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// InlineEditor 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class InlineEditorAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new InlineEditorAttributeData());
        }
    }
}
