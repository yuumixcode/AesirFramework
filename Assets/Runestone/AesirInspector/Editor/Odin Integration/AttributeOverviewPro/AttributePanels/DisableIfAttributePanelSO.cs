namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// DisableIf 特性介绍面板。
    /// </summary>
    [Summary("DisableIf 特性介绍面板，展示 DisableIf 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Conditionals)]
    public class DisableIfAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new DisableIfAttributeData());
        }
    }
}
