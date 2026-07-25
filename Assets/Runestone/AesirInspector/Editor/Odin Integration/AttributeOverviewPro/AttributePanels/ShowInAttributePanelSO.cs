namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AttributeCategory(AesirAttributeCategory.Conditionals)]
    internal class ShowInAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new ShowInAttributeData());
        }
    }
}
