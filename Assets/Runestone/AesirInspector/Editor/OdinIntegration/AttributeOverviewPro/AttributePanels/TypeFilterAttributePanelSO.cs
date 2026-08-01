namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// TypeFilter 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class TypeFilterAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new TypeFilterAttributeData());
        }
    }
}
