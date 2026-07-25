namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [Summary("ToggleGroup 特性介绍面板，展示 ToggleGroup 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Groups)]
    public class ToggleGroupAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new ToggleGroupAttributeData());
        }
    }
}
