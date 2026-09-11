using System.IO;
using UnityEngine;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    public static class ScriptDocGeneratorPaths
    {
        /// <summary>
        /// Script Doc Generator 编辑器资源的根路径。
        /// </summary>
        public const string EditorDefaultResourcesPath =
            "Assets/Editor Default Resources/Script Doc Generator";

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
        /// 默认文档输出路径（项目根目录下，Assets 外）。
        /// 输出在 Assets 外可避免为每个生成 .md 产生 .meta 与 AssetDatabase 刷新；
        /// 需要随包分发时可在面板中改回 Assets 内路径（支持绝对路径，如文档站仓库）。
        /// </summary>
        public static readonly string DefaultDocFolderPath = Path.Combine(
            Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty, "ScriptDocGenerator");
    }
}
