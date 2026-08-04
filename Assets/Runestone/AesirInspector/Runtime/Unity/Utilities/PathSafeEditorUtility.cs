using System.Diagnostics;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 关于 Path 路径的编辑器安全工具类。仅在编辑器阶段可用，打包后调用自动剔除。
    /// </summary>
    public static class PathSafeEditorUtility
    {
        /// <summary>
        /// 确保 Assets 目录下的相对路径的文件夹存在，如果不存在则递归创建。仅在编辑器阶段可用，打包后自动剔除。
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void EnsureDirectoryExists(string relativePath)
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(relativePath))
            {
                return;
            }

            var fullPath = PathUtility.ToUnityPath(relativePath.Trim());
            if (!Directory.Exists(fullPath))
            {
                // Directory 默认递归创建
                Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh();
            }
#endif
        }
    }
}
