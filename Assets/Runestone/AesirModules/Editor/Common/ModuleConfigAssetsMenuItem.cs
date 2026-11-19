using UnityEditor;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// 模块配置资产的一键初始化入口：显式在 <c>Assets/Resources/</c> 下创建 UI / 场景两份配置 SO。
    /// </summary>
    /// <remarks>
    /// 包导入时<b>不</b>执行本菜单——第三方包不应在导入瞬间写入用户工程；这是开发者主动选择的开关。
    /// 两份资产均幂等创建（已存在或项目中已有同类型资产时跳过）。不点菜单也能运行：两个配置 SO 的
    /// <c>Instance</c> 在 Resources 未命中时会以代码默认值在内存中各建一份实例，不落盘。
    /// </remarks>
    internal static class ModuleConfigAssetsMenuItem
    {
        /// <summary>菜单路径（Modules 组内最前，现有 Install Dependencies 998 / Script Doc Generator 999 / Scene Module Settings 1000）。</summary>
        const string MenuPath = "Tools/Aesir/Modules/Create Module Config Assets";

        [MenuItem(MenuPath, false, 997)]
        static void CreateConfigAssets()
        {
            UIModuleConfigAssetInitializer.EnsureConfigAsset();
            SceneModuleConfigAssetInitializer.EnsureConfigAsset();
        }
    }
}
