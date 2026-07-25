namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// SceneObjectsOnly 特性介绍面板。
    /// </summary>
    [Summary("SceneObjectsOnly 特性介绍面板，展示 SceneObjectsOnly 特性的用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Validation)]
    public class SceneObjectsOnlyAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new SceneObjectsOnlyAttributeData());
        }
    }
}
