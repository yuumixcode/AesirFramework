namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// LabelWidth 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class LabelWidthAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new LabelWidthAttributeData());
        }
    }
}
