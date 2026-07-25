namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// FilePath 特性介绍面板。
    /// </summary>
    [Summary("FilePath 特性介绍面板，展示 FilePath 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class FilePathAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new FilePathAttributeData());
        }
    }
}
