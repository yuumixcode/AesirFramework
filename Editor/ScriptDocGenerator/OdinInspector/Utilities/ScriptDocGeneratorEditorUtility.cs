using System.IO;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// Script Doc Generator 的编辑器工具方法，仅供编辑器程序集内部使用。
    /// </summary>
    public static class ScriptDocGeneratorEditorUtility
    {
        /// <summary>
        /// 确保 Assets 目录下的相对路径的文件夹存在，如果不存在则递归创建。
        /// </summary>
        public static void EnsureDirectoryExists(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return;
            }

            var normalizedPath = relativePath.Trim().Replace("\\", "/");
            if (!Directory.Exists(normalizedPath))
            {
                // Directory 默认递归创建
                Directory.CreateDirectory(normalizedPath);
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Ping 项目中的任何资源，可以是文件夹路径。传入相对路径。
        /// </summary>
        public static void PingAndSelectAsset(string relativePath)
        {
            if (!relativePath.StartsWith("Assets"))
            {
                Debug.LogError("相对路径必须以 Assets 开头");
                return;
            }

            var asset = AssetDatabase.LoadAssetAtPath<Object>(relativePath);
            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
        }

        /// <summary>
        /// 根据配置名称获取或创建编辑器 ScriptableObject 资源。
        /// 如果资源不存在则自动创建并保存到指定路径，同时将资源注册到 EditorBuildSettings 中。
        /// </summary>
        public static T GetOrCreateEditorScriptableObject<T>(string configName,
            string folderPath,
            string assetName) where T : ScriptableObject
        {
            if (EditorBuildSettings.TryGetConfigObject(configName, out T instance))
            {
                return instance;
            }

            EnsureDirectoryExists(folderPath);
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
    }
}
