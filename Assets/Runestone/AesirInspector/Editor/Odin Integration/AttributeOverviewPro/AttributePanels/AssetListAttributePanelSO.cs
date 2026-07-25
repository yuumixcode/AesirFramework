namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// AssetList 特性介绍面板，展示 AssetList 各参数用法及案例预览。
    /// </summary>
    [Summary("AssetList 特性介绍面板，展示 AssetList 各参数用法及案例预览")]
    [AttributeCategory(AesirAttributeCategory.TypeSpecifics)]
    public class AssetListAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        [Summary("初始化面板数据")]
        public override void Initialize()
        {
            SetData(new AssetListAttributeData());
        }
    }
}
