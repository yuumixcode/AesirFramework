namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// SuffixLabel 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class SuffixLabelAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new SuffixLabelAttributeData());
        }
    }
}
