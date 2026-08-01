namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ProgressBar 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Numbers)]
    public class ProgressBarAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new ProgressBarAttributeData());
        }
    }
}
