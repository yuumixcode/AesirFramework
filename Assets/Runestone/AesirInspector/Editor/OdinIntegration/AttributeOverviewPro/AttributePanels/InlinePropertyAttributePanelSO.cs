namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// InlineProperty 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class InlinePropertyAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new InlinePropertyAttributeData());
        }
    }
}
