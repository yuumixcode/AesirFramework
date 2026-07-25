namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// SuffixLabel 特性介绍面板。
    /// </summary>
    [Summary("SuffixLabel 特性介绍面板，展示 SuffixLabel 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class SuffixLabelAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new SuffixLabelAttributeData());
        }
    }
}
