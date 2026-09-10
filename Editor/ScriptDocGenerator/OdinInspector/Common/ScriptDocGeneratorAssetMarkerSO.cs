using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// Script Doc Generator 模块资产初始化完成标识。首次初始化后创建标识资产，
    /// 后续打开工具时通过检查标识是否存在来判断是否已完成初始化。
    /// </summary>
    public class ScriptDocGeneratorAssetMarkerSO : ScriptableObject
    {
        const string MarkerAssetName = "ScriptDocGeneratorAssetMarker";

        [DisplayAsString]
        public string Description =>
            $"Script Doc Generator 模块标识资产，本资产标识的是 {MarkerAssetName}，不要移动或者删除本资产。";

        [ReadOnly]
        [SerializeField]
        string toolName;

        /// <summary>
        /// 检查 Script Doc Generator 模块的标识资产是否已初始化。
        /// 用于判断模块相关资源是否已创建并准备就绪。
        /// </summary>
        /// <returns>如果标识资产已存在，则返回 true；否则返回 false。</returns>
        public static bool IsAssetsInitialized() =>
            AssetDatabase.LoadAssetAtPath<ScriptDocGeneratorAssetMarkerSO>(GetMarkerAssetPath()) != null;

        /// <summary>
        /// 创建标识资产。应在工具的所有资产初始化完成后调用。
        /// </summary>
        public static void CreateMarkerAsset()
        {
            ScriptDocGeneratorEditorUtility.EnsureDirectoryExists(ScriptDocGeneratorPaths
                .ScriptDocGeneratorAssetsFolderPath);
            var assetPath = GetMarkerAssetPath();
            var marker = CreateInstance<ScriptDocGeneratorAssetMarkerSO>();
            marker.toolName = MarkerAssetName + ".asset";
            AssetDatabase.CreateAsset(marker, assetPath);
            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static string GetMarkerAssetPath() =>
            Path.Combine(ScriptDocGeneratorPaths.ScriptDocGeneratorAssetsFolderPath, MarkerAssetName + ".asset")
                .Replace("\\", "/");
    }
}
