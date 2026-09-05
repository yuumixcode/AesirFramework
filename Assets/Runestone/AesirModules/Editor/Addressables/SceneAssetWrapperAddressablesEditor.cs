using UnityEditor;
using UnityEditor.AddressableAssets;

namespace Runestone.AesirModules.Editor.Addressables
{
    /// <summary>
    /// Addressables 编辑器胶水：把场景寻址能力注册进
    /// <see cref="SceneAssetWrapperAddressablesBridge" />，供 SceneAssetWrapper 的
    /// Inspector 工具（Addressable 着色、"加入 Addressables"按钮、地址实时核验）使用。
    /// <para>
    /// 本程序集仅在项目安装了 com.unity.addressables 包（宏 AESIR_MODULES_ADDRESSABLES，
    /// 由核心运行时程序集的 versionDefines 声明）时参与编译——
    /// 卸载包后 defineConstraints 不满足，本程序集整体不编译，不产生任何错误；
    /// 桥未注册时 SceneAssetWrapper 的所有 Addressables 编辑器功能自动隐藏。
    /// </para>
    /// </summary>
    [InitializeOnLoad]
    public static class SceneAssetWrapperAddressablesEditor
    {
        static SceneAssetWrapperAddressablesEditor()
        {
            Register();
        }

        /// <summary>
        /// 注册桥接能力（域重载后由 [InitializeOnLoad] 自动调用）。
        /// 公开为手动入口：单测清理桥状态后可用它恢复 Inspector 功能。
        /// </summary>
        public static void Register()
        {
            SceneAssetWrapperAddressablesBridge.Register(GetAddress, MakeAddressable);
        }

        /// <summary>
        /// 查询场景资产在 Addressables 中的地址；不可寻址或 Addressables 未初始化时返回 null。
        /// </summary>
        static string GetAddress(string scenePath)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null || string.IsNullOrEmpty(scenePath))
            {
                return null;
            }

            var guid = AssetDatabase.AssetPathToGUID(scenePath);
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            var entry = settings.FindAssetEntry(guid);
            return entry == null ? null : entry.address;
        }

        /// <summary>
        /// 把场景资产加入 Addressables 默认组并返回其地址；失败时返回 null。
        /// </summary>
        static string MakeAddressable(string scenePath)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null || string.IsNullOrEmpty(scenePath))
            {
                return null;
            }

            var guid = AssetDatabase.AssetPathToGUID(scenePath);
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            var group = settings.DefaultGroup;
            if (group == null)
            {
                return null;
            }

            var entry = settings.CreateOrMoveEntry(guid, group);
            return entry == null ? null : entry.address;
        }
    }
}
