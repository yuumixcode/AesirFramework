namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// TableList 特性介绍面板。
    /// </summary>
    [Summary("TableList 特性介绍面板，展示 TableList 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Collections)]
    public class TableListAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new TableListAttributeData());
        }
    }
}
