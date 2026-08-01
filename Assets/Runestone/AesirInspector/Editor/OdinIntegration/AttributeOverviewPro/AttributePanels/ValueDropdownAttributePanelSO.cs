namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ValueDropdown 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class ValueDropdownAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new ValueDropdownAttributeData());
        }
    }
}
