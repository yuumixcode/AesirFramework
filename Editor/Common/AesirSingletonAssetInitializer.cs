using UnityEditor;
using UnityEngine;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// 模块配置资产（单例 <see cref="ScriptableObject" />）的自动创建共用实现。
    /// </summary>
    /// <remarks>
    /// 各模块的 <c>XxxModuleConfigAssetInitializer</c> 负责在 <c>[InitializeOnLoadMethod]</c> 里注册
    /// <c>EditorApplication.delayCall</c>（域加载期 AssetDatabase 未就绪，此刻 CreateAsset 会报
    /// "Unable to import newly created asset"），实际创建推迟到编辑器空闲首帧并由本类执行——
    /// UI / Scene 两个配置资产此前各持一份等价实现，此处收敛为单一真源。
    /// </remarks>
    internal static class AesirSingletonAssetInitializer
    {
        /// <summary>
        /// 确保指定单例配置资产存在。
        /// </summary>
        /// <remarks>
        /// 满足以下任一条件即返回：Resources 兜底路径命中（资产就绪），或项目中已存在同类型资产
        /// （含被移出 Resources 的形态——视为用户主动放弃 Resources 加载，尊重现状、不搬回也不重复创建）。
        /// </remarks>
        /// <typeparam name="T">配置资产类型。</typeparam>
        /// <param name="assetPath">资产固定创建路径（相对项目根，含扩展名）。</param>
        /// <param name="resourcePath">Resources 兜底路径（不含扩展名）。</param>
        /// <param name="assetDirectory">资产固定创建目录（相对项目根）。</param>
        /// <param name="logTag">创建成功后的门面日志前缀（按模块归属，如 <c>UIModuleTag</c> / <c>SceneModuleTag</c>）。</param>
        internal static void EnsureSingletonAsset<T>(string assetPath, string resourcePath,
            string assetDirectory, string logTag) where T : ScriptableObject
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (Resources.Load<T>(resourcePath) != null)
            {
                return;
            }

            if (AssetDatabase.FindAssets("t:" + typeof(T).Name).Length > 0)
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder(assetDirectory))
            {
                // CreateFolder 只接收"父目录 + 新目录名"，故取待建目录的最后一段
                AssetDatabase.CreateFolder("Assets/Resources",
                    assetDirectory.Substring(assetDirectory.LastIndexOf('/') + 1));
            }

            var config = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            AesirModulesDebug.Log(logTag,
                $"已自动创建 {typeof(T).Name} 配置资产（Resources 兜底路径）：{assetPath}");
        }
    }
}
