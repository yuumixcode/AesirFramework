namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// OnStateUpdate 特性介绍面板。
    /// </summary>
    [Summary("OnStateUpdate 特性介绍面板，展示 OnStateUpdate 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class OnStateUpdateAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new OnStateUpdateAttributeData());
        }
    }
}
