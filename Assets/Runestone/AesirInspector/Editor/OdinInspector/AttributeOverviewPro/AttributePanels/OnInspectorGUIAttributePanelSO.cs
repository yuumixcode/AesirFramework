namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// OnInspectorGUI 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class OnInspectorGUIAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new OnInspectorGUIAttributeData());
        }
    }
}
