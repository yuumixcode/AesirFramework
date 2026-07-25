namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// FoldoutGroup 特性介绍面板。
    /// </summary>
    [Summary("FoldoutGroup 特性介绍面板，展示 FoldoutGroup 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Groups)]
    public class FoldoutGroupAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new FoldoutGroupAttributeData());
        }
    }
}
