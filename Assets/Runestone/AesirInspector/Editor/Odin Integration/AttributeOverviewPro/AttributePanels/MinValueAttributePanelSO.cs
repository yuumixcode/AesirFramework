namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// MinValue 特性介绍面板。
    /// </summary>
    [Summary("MinValue 特性介绍面板，展示 MinValue 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Numbers)]
    public class MinValueAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new MinValueAttributeData());
        }
    }
}
