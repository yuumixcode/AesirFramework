namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AttributeCategory(AesirAttributeCategory.Conditionals)]
    internal class HideInEditorModeAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new HideInEditorModeAttributeData());
        }
    }
}
