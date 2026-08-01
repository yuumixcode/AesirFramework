namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// MaxValue 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Numbers)]
    public class MaxValueAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new MaxValueAttributeData());
        }
    }
}
