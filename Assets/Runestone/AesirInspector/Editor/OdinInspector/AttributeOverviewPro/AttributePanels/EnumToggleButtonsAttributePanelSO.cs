namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// EnumToggleButtons 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.TypeSpecifics)]
    public class EnumToggleButtonsAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new EnumToggleButtonsAttributeData());
        }
    }
}
