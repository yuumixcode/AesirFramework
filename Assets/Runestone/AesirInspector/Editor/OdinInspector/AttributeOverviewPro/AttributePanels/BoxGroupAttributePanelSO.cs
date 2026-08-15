namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// BoxGroup 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Groups)]
    public class BoxGroupAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new BoxGroupAttributeData());
        }
    }
}
