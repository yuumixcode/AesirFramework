namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Searchable 特性介绍面板。
    /// </summary>
    [Summary("Searchable 特性介绍面板，展示 Searchable 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Collections)]
    public class SearchableAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new SearchableAttributeData());
        }
    }
}
