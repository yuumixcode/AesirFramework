namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// OnStateUpdate 特性介绍面板。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Misc)]
    public class OnStateUpdateAttributePanelSO : AbstractAttributePanelSO
    {
        public override void Initialize()
        {
            SetData(new OnStateUpdateAttributeData());
        }
    }
}
