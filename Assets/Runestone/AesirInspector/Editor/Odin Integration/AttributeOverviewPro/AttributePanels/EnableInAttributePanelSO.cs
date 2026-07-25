namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AttributeCategory(AesirAttributeCategory.Conditionals)]
    internal class EnableInAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new EnableInAttributeData());
        }
    }
}
