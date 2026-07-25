namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// VerticalGroup 特性介绍面板。
    /// </summary>
    [Summary("VerticalGroup 特性介绍面板，展示 VerticalGroup 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Groups)]
    public class VerticalGroupAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new VerticalGroupAttributeData());
        }
    }
}
