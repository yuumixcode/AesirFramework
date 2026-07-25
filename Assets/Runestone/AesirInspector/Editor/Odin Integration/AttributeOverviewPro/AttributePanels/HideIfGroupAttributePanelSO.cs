namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [Summary("HideIfGroup 特性介绍面板，展示 HideIfGroup 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Conditionals)]
    public class HideIfGroupAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new HideIfGroupAttributeData());
        }
    }
}
