namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// GUIColor 特性介绍面板，展示 GUIColor 各参数用法及案例预览。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class GUIColorAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize()
        {
            SetData(new GUIColorAttributeData());
        }
    }
}
