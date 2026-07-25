namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// PropertyRange 特性介绍面板。
    /// </summary>
    [Summary("PropertyRange 特性介绍面板，展示 PropertyRange 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Numbers)]
    public class PropertyRangeAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new PropertyRangeAttributeData());
        }
    }
}
