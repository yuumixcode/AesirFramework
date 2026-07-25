namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [Summary("TitleGroup 特性介绍面板，展示 TitleGroup 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Groups)]
    public class TitleGroupAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new TitleGroupAttributeData());
        }
    }
}
