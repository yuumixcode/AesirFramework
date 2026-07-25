namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// HorizontalGroup 特性介绍面板。
    /// </summary>
    [Summary("HorizontalGroup 特性介绍面板，展示 HorizontalGroup 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Groups)]
    public class HorizontalGroupAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new HorizontalGroupAttributeData());
        }
    }
}
