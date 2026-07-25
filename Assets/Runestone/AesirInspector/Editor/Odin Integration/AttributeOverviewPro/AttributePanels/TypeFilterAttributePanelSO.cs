namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// TypeFilter 特性介绍面板。
    /// </summary>
    [Summary("TypeFilter 特性介绍面板，展示 TypeFilter 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class TypeFilterAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new TypeFilterAttributeData());
        }
    }
}
