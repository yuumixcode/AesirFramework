namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// AssetList 特性介绍面板，展示 AssetList 各参数用法及案例预览。
    /// </summary>
    [AttributeCategory(AesirAttributeCategory.TypeSpecifics)]
    public class AssetListAttributePanelSO : AbstractAttributePanelSO
    {
        /// <summary>
        /// 初始化面板数据。
        /// </summary>
        public override void Initialize()
        {
            SetData(new AssetListAttributeData());
        }
    }
}
