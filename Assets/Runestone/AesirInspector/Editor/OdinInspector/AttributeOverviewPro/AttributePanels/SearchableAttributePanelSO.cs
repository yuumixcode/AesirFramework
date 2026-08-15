namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Searchable 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Collections)]
    public class SearchableAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new SearchableAttributeData());
        }
    }
}
