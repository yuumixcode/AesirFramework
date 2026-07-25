namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// AssetsOnly 特性介绍面板，展示 AssetsOnly 用法及案例预览。
    /// </summary>
    [Summary("AssetsOnly 特性介绍面板，展示 AssetsOnly 用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.Essentials)]
    public class AssetsOnlyAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        [Summary("初始化面板数据")]
        public override void Initialize()
        {
            SetData(new AssetsOnlyAttributeData());
        }
    }
}
