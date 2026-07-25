namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// MaxValue 特性介绍面板。
    /// </summary>
    [Summary("MaxValue 特性介绍面板，展示 MaxValue 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Numbers)]
    public class MaxValueAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new MaxValueAttributeData());
        }
    }
}
