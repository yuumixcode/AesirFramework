namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// FoldoutGroup 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Groups)]
    public class FoldoutGroupAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new FoldoutGroupAttributeData());
        }
    }
}
