using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Runestone.AesirInspector
{
    /// <summary>
    /// ScriptableObject 的编辑器安全工具类，不需要编写宏定义。仅编辑器阶段有效，打包后运行时调用，返回 null 或者其他默认值。
    /// </summary>
    public static class ScriptableObjectSafeEditorUtility
    {
        /// <summary>
        /// 获取对应类型的 SO 资源单例的相对路径。若存在多个则保留第一个并删除其余；若不存在则在指定路径自动创建。
        /// 打包后此方法将失效，返回 string.Empty。
        /// </summary>
        public static string GetSingletonAssetPathAndDeleteOther<T>(string relativeFolderPath = "")
            where T : ScriptableObject
        {
#if UNITY_EDITOR
            return Internal_GetSingletonAssetPathAndDeleteOther<T>(relativeFolderPath);
#else
            return string.Empty;
#endif
        }

        /// <summary>
        /// 获取对应类型的 SO 资源单例。若存在多个则保留第一个并删除其余；若不存在则在指定路径自动创建。
        /// 打包后此方法将失效，返回 null。
        /// </summary>
        public static T GetSingletonAssetAndDeleteOther<T>(string relativeFolderPath = "")
            where T : ScriptableObject
        {
#if UNITY_EDITOR
            return Internal_GetSingletonAssetAndDeleteOther<T>(relativeFolderPath);
#else
            return null;
#endif
        }

        /// <summary>
        /// 根据配置名称获取或创建编辑器 ScriptableObject 资源。
        /// 如果资源不存在则自动创建并保存到指定路径，同时将资源注册到 EditorBuildSettings 中。
        /// 打包后此方法将失效，返回 null。
        /// </summary>
        public static T GetOrCreateEditorScriptableObject<T>(string configName,
            string folderPath,
            string assetName) where T : ScriptableObject
        {
#if UNITY_EDITOR
            return Internal_GetOrCreateEditorScriptableObject<T>(configName, folderPath, assetName);
#else
            return null;
#endif
        }

        #region Internal

#if UNITY_EDITOR
        static string Internal_GetSingletonAssetPathAndDeleteOther<T>(string relativeFolderPath = "")
            where T : ScriptableObject
        {
            T singletonAsset = null;
            var targetPath = string.Empty;
            var guids = AssetDatabase.FindAssets("t:" + typeof(T));
            if (guids.Length > 0)
            {
                var allPaths = guids.Select(AssetDatabase.GUIDToAssetPath);
                foreach (var path in allPaths)
                {
                    if (!singletonAsset)
                    {
                        singletonAsset = AssetDatabase.LoadAssetAtPath<T>(path);
                        targetPath = path;
                    }
                    else
                    {
                        AssetDatabase.DeleteAsset(path);
                    }
                }

                AssetDatabase.Refresh();
                if (singletonAsset)
                {
                    return targetPath;
                }
            }

            if (string.IsNullOrWhiteSpace(relativeFolderPath))
            {
                relativeFolderPath = AesirInspectorPaths.EditorDefaultResourcesPath + "/SingletonAssets";
            }

            PathSafeEditorUtility.EnsureDirectoryExists(relativeFolderPath);
            singletonAsset = ScriptableObject.CreateInstance<T>();
            var fileNameWithoutExtension = typeof(T).Name.EndsWith("SO")
                ? typeof(T).Name.Remove(typeof(T).Name.Length - 2)
                : typeof(T).Name;
            var filePath = relativeFolderPath + "/" + fileNameWithoutExtension + ".asset";
            AssetDatabase.CreateAsset(singletonAsset, filePath);
            AssetDatabase.ImportAsset(filePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return filePath;
        }

        static T Internal_GetSingletonAssetAndDeleteOther<T>(string relativeFolderPath = "")
            where T : ScriptableObject
        {
            T singletonAsset = null;
            var guids = AssetDatabase.FindAssets("t:" + typeof(T));
            if (guids.Length > 0)
            {
                var allPaths = guids.Select(AssetDatabase.GUIDToAssetPath);
                foreach (var path in allPaths)
                {
                    if (!singletonAsset)
                    {
                        singletonAsset = AssetDatabase.LoadAssetAtPath<T>(path);
                    }
                    else
                    {
                        AssetDatabase.DeleteAsset(path);
                    }
                }

                AssetDatabase.Refresh();
                return singletonAsset;
            }

            if (string.IsNullOrEmpty(relativeFolderPath))
            {
                relativeFolderPath = AesirInspectorPaths.EditorDefaultResourcesPath + "/SingletonAssets";
            }

            PathSafeEditorUtility.EnsureDirectoryExists(relativeFolderPath);
            singletonAsset = ScriptableObject.CreateInstance<T>();
            var fileNameWithoutExtension = typeof(T).Name.EndsWith("SO")
                ? typeof(T).Name.Remove(typeof(T).Name.Length - 2)
                : typeof(T).Name;
            var filePath = relativeFolderPath + "/" + fileNameWithoutExtension + ".asset";
            AssetDatabase.CreateAsset(singletonAsset, filePath);
            AssetDatabase.ImportAsset(filePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ProjectSafeEditorUtility.PingAndSelectAsset(filePath);
            return singletonAsset;
        }

        static T Internal_GetOrCreateEditorScriptableObject<T>(string configName,
            string folderPath,
            string assetName) where T : ScriptableObject
        {
            if (EditorBuildSettings.TryGetConfigObject(configName, out T instance))
            {
                return instance;
            }

            PathSafeEditorUtility.EnsureDirectoryExists(folderPath);
            var assetPath = folderPath + "/" + assetName + ".asset";
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                EditorBuildSettings.AddConfigObject(configName, asset, true);
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            EditorBuildSettings.AddConfigObject(configName, asset, true);
            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return asset;
        }
#endif

        #endregion
    }
}
