namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// MinMaxSlider 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Numbers)]
    public class MinMaxSliderAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new MinMaxSliderAttributeData());
        }
    }
}
