namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// HorizontalGroup 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Groups)]
    public class HorizontalGroupAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new HorizontalGroupAttributeData());
        }
    }
}
