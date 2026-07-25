namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// PropertySpace 特性介绍面板。
    /// </summary>
    [Summary("PropertySpace 特性介绍面板，展示 PropertySpace 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class PropertySpaceAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new PropertySpaceAttributeData());
        }
    }
}
