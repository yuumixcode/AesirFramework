namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// DisplayAsString 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class DisplayAsStringAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new DisplayAsStringAttributeData());
        }
    }
}
