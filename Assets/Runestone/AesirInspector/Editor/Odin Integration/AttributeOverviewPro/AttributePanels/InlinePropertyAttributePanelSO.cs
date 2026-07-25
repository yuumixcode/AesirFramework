namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// InlineProperty 特性介绍面板。
    /// </summary>
    [Summary("InlineProperty 特性介绍面板，展示 InlineProperty 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class InlinePropertyAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new InlinePropertyAttributeData());
        }
    }
}
