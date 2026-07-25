namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// OnValueChanged 特性介绍面板。
    /// </summary>
    [Summary("OnValueChanged 特性介绍面板，展示 OnValueChanged 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class OnValueChangedAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new OnValueChangedAttributeData());
        }
    }
}
