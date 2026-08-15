namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// PropertySpace 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class PropertySpaceAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new PropertySpaceAttributeData());
        }
    }
}
