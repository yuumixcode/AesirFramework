namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// OnValueChanged 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class OnValueChangedAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new OnValueChangedAttributeData());
        }
    }
}
