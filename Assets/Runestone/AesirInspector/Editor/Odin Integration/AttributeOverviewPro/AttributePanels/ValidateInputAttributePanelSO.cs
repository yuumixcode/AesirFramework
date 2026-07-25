namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ValidateInput 特性介绍面板。
    /// </summary>
    [Summary("ValidateInput 特性介绍面板，展示 ValidateInput 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Validation)]
    public class ValidateInputAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new ValidateInputAttributeData());
        }
    }
}
