namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AttributeCategory(AesirAttributeCategory.Conditionals)]
    internal class DisableInAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new DisableInAttributeData());
        }
    }
}
