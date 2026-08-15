namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// SceneObjectsOnly 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Validation)]
    public class SceneObjectsOnlyAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new SceneObjectsOnlyAttributeData());
        }
    }
}
