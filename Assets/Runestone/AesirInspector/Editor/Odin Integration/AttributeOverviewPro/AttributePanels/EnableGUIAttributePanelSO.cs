namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// EnableGUI 特性介绍面板。
    /// </summary>
    [Summary("EnableGUI 特性介绍面板，展示 EnableGUI 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class EnableGUIAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new EnableGUIAttributeData());
        }
    }
}
