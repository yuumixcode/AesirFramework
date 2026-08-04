#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Diagnostics;
using Object = UnityEngine.Object;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 关于 Project 操作的编辑器安全工具类。仅在编辑器阶段可用，打包后自动剔除。
    /// </summary>
    public static class ProjectSafeEditorUtility
    {
        /// <summary>
        /// Ping 项目中的任何资源，可以是文件夹路径。传入相对路径。仅在编辑器阶段可用，打包后自动剔除。
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void PingAndSelectAsset(string relativePath)
        {
#if UNITY_EDITOR
            if (!relativePath.StartsWith("Assets"))
            {
                AesirInspectorDebug.Error("相对路径必须以 Assets 开头");
                return;
            }

            var asset = AssetDatabase.LoadAssetAtPath<Object>(relativePath);
            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
#endif
        }
    }
}
