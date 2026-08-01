namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// VerticalGroup 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Groups)]
    public class VerticalGroupAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new VerticalGroupAttributeData());
        }
    }
}
