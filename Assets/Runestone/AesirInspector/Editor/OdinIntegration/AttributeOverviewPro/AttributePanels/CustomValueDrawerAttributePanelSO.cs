namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// CustomValueDrawer 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class CustomValueDrawerAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new CustomValueDrawerAttributeData());
        }
    }
}
