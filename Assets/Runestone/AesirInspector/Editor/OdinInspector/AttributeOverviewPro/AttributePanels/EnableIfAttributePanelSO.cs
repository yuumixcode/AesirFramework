namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// EnableIf 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Conditionals)]
    public class EnableIfAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new EnableIfAttributeData());
        }
    }
}
