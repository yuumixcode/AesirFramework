namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// AssetsOnly 特性介绍面板，展示 AssetsOnly 用法及案例预览。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class AssetsOnlyAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize()
        {
            SetData(new AssetsOnlyAttributeData());
        }
    }
}
