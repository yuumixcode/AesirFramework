using System.IO;
using UnityEngine;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    public static class ScriptDocGeneratorPaths
    {
        /// <summary>
        /// Script Doc Generator 编辑器资源的根路径。
        /// </summary>
        public const string EditorDefaultResourcesPath = "Assets/Editor Default Resources/Script Doc Generator";

        /// <summary>
        /// Script Doc Generator 模块资源的存放路径
        /// </summary>
        public const string ScriptDocGeneratorAssetsFolderPath = EditorDefaultResourcesPath;

        /// <summary>
        /// Panels 模块资源存放路径。
        /// </summary>
        public const string PanelsPath = ScriptDocGeneratorAssetsFolderPath + "/Panels";

        /// <summary>
        /// 面板配置资源的存放路径。
        /// </summary>
        public const string PanelConfigFolderPath = EditorDefaultResourcesPath + "/Panel";

        /// <summary>
        /// 文档生成器设置资源的存放路径。
        /// </summary>
        public const string GeneratorSettingsFolderPath = PanelConfigFolderPath + "/GeneratorSettings";

        /// <summary>
        /// 默认文档输出路径（绝对路径）。
        /// 以项目根目录为基准推导，避免 Assets 相对路径在 Assets 外目录（如文档站仓库）不可用的问题。
        /// </summary>
        public static readonly string DefaultDocFolderPath = Path.Combine(
            Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty,
            EditorDefaultResourcesPath, "Documents");
    }
}
