namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// EnableGUI 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class EnableGUIAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new EnableGUIAttributeData());
        }
    }
}
