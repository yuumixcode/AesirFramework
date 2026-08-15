namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// CustomValueDrawer 特性介绍面板，展示 CustomValueDrawer 用法及案例预览。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class CustomValueDrawerPanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize()
        {
            SetData(new CustomValueDrawerAttributeData());
        }
    }
}
