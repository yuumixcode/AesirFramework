using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Search;
#endif

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 关于 Hierarchy 的编辑器安全工具类，不包括预制体的 Stage 场景。仅在编辑器阶段可用，打包后调用返回默认值。
    /// </summary>
    public static class HierarchySafeEditorUtility
    {
        /// <summary>
        /// 获取 GameObject 的绝对路径。仅在编辑器阶段可用，打包后返回 string.Empty
        /// </summary>
        public static string GetAbsolutePath(GameObject obj) => GetAbsolutePath(obj.transform);

        /// <summary>
        /// 获取 Transform 的绝对路径。仅在编辑器阶段可用，打包后返回 string.Empty
        /// </summary>
        public static string GetAbsolutePath(Transform trans)
        {
#if UNITY_EDITOR
            return SearchUtils.GetHierarchyPath(trans.gameObject, false).TrimStart('/');
#else
            return string.Empty;
#endif
        }
    }
}
