namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// FilePath 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class FilePathAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new FilePathAttributeData());
        }
    }
}
