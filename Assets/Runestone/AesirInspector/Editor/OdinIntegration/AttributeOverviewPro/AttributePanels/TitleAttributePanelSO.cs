namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Title 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class TitleAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new TitleAttributeData());
        }
    }
}
