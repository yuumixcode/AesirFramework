namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// FolderPath 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class FolderPathAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new FolderPathAttributeData());
        }
    }
}
