namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// FolderPath 特性介绍面板。
    /// </summary>
    [Summary("FolderPath 特性介绍面板，展示 FolderPath 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class FolderPathAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new FolderPathAttributeData());
        }
    }
}
