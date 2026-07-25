namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ProgressBar 特性介绍面板。
    /// </summary>
    [Summary("ProgressBar 特性介绍面板，展示 ProgressBar 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Numbers)]
    public class ProgressBarAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new ProgressBarAttributeData());
        }
    }
}
