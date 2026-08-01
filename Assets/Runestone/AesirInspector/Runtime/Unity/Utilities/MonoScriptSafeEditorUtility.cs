using System.Diagnostics;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 关于 MonoScript 的编辑器安全工具类。仅在编辑器阶段可用，打包后调用返回默认值。
    /// </summary>
    public static class MonoScriptSafeEditorUtility
    {
        /// <summary>
        /// 在项目中根据脚本文件名称查找脚本文件，并在编辑器中选择。仅在编辑器阶段可用，打包后自动剔除。
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void SelectMonoScript(string scriptName)
        {
#if UNITY_EDITOR
            Selection.activeObject = GetMonoScript(scriptName);
#endif
        }
#if UNITY_EDITOR
        /// <summary>
        /// 在项目中根据脚本文件名称查找脚本文件，返回找到的 MonoScript。仅在编辑器阶段可用，打包后返回 null。
        /// </summary>
        public static MonoScript GetMonoScript(string scriptName)
        {
            MonoScript foundMonoScript = null;
            var scriptAssetPath = FindScriptPath(scriptName);
            if (!string.IsNullOrWhiteSpace(scriptAssetPath))
            {
                foundMonoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptAssetPath);
            }

            return foundMonoScript;
        }
#endif
        /// <summary>
        /// 在项目中根据脚本文件名称查找脚本文件，返回脚本文件路径。仅在编辑器阶段可用，打包后返回 string.Empty
        /// </summary>
        public static string FindScriptPath(string scriptName)
        {
#if UNITY_EDITOR
            var scriptAssetPath = AssetDatabase.FindAssets("t:MonoScript " + scriptName)
                .Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();
            return !string.IsNullOrWhiteSpace(scriptAssetPath) ? scriptAssetPath : null;
#else
            return string.Empty;
#endif
        }
    }
}
