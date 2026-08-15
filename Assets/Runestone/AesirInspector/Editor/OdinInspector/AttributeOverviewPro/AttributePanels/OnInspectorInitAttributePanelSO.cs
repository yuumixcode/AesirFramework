namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AttributeCategory(AesirAttributeCategory.Meta)]
    public class OnInspectorInitAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new OnInspectorInitAttributeData());
        }
    }
}
