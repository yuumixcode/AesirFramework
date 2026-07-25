namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// OnInspectorGUI 特性介绍面板。
    /// </summary>
    [Summary("OnInspectorGUI 特性介绍面板，展示 OnInspectorGUI 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class OnInspectorGUIAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new OnInspectorGUIAttributeData());
        }
    }
}
