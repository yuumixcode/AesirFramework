namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// EnumToggleButtons 特性介绍面板。
    /// </summary>
    [Summary("EnumToggleButtons 特性介绍面板，展示 EnumToggleButtons 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.TypeSpecifics)]
    public class EnumToggleButtonsAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new EnumToggleButtonsAttributeData());
        }
    }
}
