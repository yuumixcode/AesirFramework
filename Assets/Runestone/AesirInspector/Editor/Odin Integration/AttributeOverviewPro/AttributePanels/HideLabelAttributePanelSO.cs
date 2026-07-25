namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// HideLabel 特性介绍面板。
    /// </summary>
    [Summary("HideLabel 特性介绍面板，展示 HideLabel 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class HideLabelAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new HideLabelAttributeData());
        }
    }
}
