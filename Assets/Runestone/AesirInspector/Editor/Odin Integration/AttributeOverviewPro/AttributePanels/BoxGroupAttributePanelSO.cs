namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// BoxGroup 特性介绍面板。
    /// </summary>
    [Summary("BoxGroup 特性介绍面板，展示 BoxGroup 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Groups)]
    public class BoxGroupAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new BoxGroupAttributeData());
        }
    }
}
