using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Aesir Inspector 模块资产初始化完成标识。每个工具首次初始化后创建对应的标识资产，
    /// 后续打开工具时通过检查标识是否存在来判断是否已完成初始化。
    /// </summary>
    public class AesirInspectorModuleAssetMarkerSO : ScriptableObject
    {
        [DisplayAsString]
        public string Description =>
            $"Aesir Inspector 模块标识资产，本资产标识的是 {ScriptDocGeneratorAssetMarkerName}，不要移动或者删除本资产。";

        const string ScriptDocGeneratorAssetMarkerName = "ScriptDocGeneratorAssetMarker";

        [ReadOnly]
        [SerializeField]
        string toolName;

        /// <summary>
        /// 快捷创建 ScriptDocGenerator 标识资产
        /// </summary>
        public static void CreateScriptDocGeneratorMarkerAsset()
        {
            CreateMarkerAsset(ScriptDocGeneratorAssetMarkerName,
                ScriptDocGeneratorPaths.ScriptDocGeneratorAssetsFolderPath);
        }

        /// <summary>
        /// 检查 Script Doc Generator 模块的标识资产是否已初始化。
        /// 用于判断模块相关资源是否已创建并准备就绪。
        /// </summary>
        /// <returns>如果标识资产已存在，则返回 true；否则返回 false。</returns>
        public static bool IsScriptDocGeneratorAssetsInitialized() =>
            IsModuleInitialized(ScriptDocGeneratorAssetMarkerName,
                ScriptDocGeneratorPaths.ScriptDocGeneratorAssetsFolderPath);

        /// <summary>
        /// 检查指定工具的标识资产是否已存在。
        /// </summary>
        static bool IsModuleInitialized(string toolName, string folderPath)
        {
            if (!toolName.EndsWith(".asset"))
            {
                toolName += ".asset";
            }

            var path = PathUtility.CombinePath(folderPath, toolName);
            return AssetDatabase.LoadAssetAtPath<AesirInspectorModuleAssetMarkerSO>(path) != null;
        }

        /// <summary>
        /// 创建指定工具的标识资产。应在工具的所有资产初始化完成后调用。
        /// </summary>
        static void CreateMarkerAsset(string toolName, string folderPath)
        {
            PathSafeEditorUtility.EnsureDirectoryExists(folderPath);
            if (!toolName.EndsWith(".asset"))
            {
                toolName += ".asset";
            }

            var assetPath = Path.Combine(folderPath, toolName);
            var marker = CreateInstance<AesirInspectorModuleAssetMarkerSO>();
            marker.toolName = toolName;
            AssetDatabase.CreateAsset(marker, assetPath);
            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
