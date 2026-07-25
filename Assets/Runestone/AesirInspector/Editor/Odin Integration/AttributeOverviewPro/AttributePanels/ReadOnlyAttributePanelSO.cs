namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ReadOnly 特性介绍面板。
    /// </summary>
    [Summary("ReadOnly 特性介绍面板，展示 ReadOnly 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class ReadOnlyAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new ReadOnlyAttributeData());
        }
    }
}
