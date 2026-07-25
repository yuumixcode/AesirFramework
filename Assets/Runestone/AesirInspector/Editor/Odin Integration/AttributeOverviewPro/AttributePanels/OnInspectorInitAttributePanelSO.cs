namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [Summary("OnInspectorInit 特性介绍面板，展示 OnInspectorInit 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Meta)]
    public class OnInspectorInitAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new OnInspectorInitAttributeData());
        }
    }
}
