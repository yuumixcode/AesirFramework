namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// DisplayAsString 特性介绍面板。
    /// </summary>
    [Summary("DisplayAsString 特性介绍面板，展示 DisplayAsString 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class DisplayAsStringAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new DisplayAsStringAttributeData());
        }
    }
}
