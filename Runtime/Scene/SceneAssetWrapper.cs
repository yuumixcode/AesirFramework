using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Runestone.AesirModules
{
    /// <summary>
    /// 场景引用类，支持在编辑器中拖拽 SceneAsset 赋值。
    /// </summary>
    [Serializable]
    public class SceneAssetWrapper
    {
        /// <summary>
        /// 场景路径缓存值。编辑器下拖拽赋值或访问 ScenePath 时自动同步，运行时直接使用此缓存值。
        /// 使用 [SerializeField] 保证序列化。
        /// </summary>
        [SerializeField]
        [HideInInspector]
        string scenePath = string.Empty;

        /// <summary>
        /// 场景相对路径，包含后缀名。编辑器下每次访问从 sceneAsset 重新获取并缓存，运行时直接返回缓存值。
        /// </summary>
        public string ScenePath
        {
            get
            {
#if UNITY_EDITOR
                if (sceneAsset != null)
                {
                    scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                }
#endif
                return scenePath;
            }
        }

        /// <summary>
        /// 场景名称（不包含扩展名）
        /// </summary>
        public string SceneName
        {
            get
            {
                var path = ScenePath;
                return string.IsNullOrEmpty(path)
                    ? string.Empty
                    : Path.GetFileNameWithoutExtension(path);
            }
        }

        /// <summary>
        /// 场景是否在 BuildSettings 中，不在返回 true。
        /// </summary>
        public bool NotInBuildSettings
        {
            get
            {
                var path = ScenePath;
                if (string.IsNullOrEmpty(path))
                {
                    return false;
                }

                return SceneUtility.GetBuildIndexByScenePath(path) == -1;
            }
        }

        /// <summary>
        /// 输出场景名称
        /// </summary>
        public override string ToString() => SceneName;

#if UNITY_EDITOR
        internal const string SceneAssetPropertyName = nameof(SceneAsset);

        internal const string AddCurrentSceneToBuildSettingsMethodName =
            nameof(AddCurrentSceneToBuildSettings);

        internal const string ResetSceneMethodName = nameof(ResetScene);
        internal const string GetSceneAssetColorMethodName = nameof(GetSceneAssetColor);

        /// <summary>
        /// 编辑器中拖拽的 SceneAsset，赋值时自动同步 scenePath。
        /// </summary>
        public SceneAsset SceneAsset
        {
            get => sceneAsset;
            set
            {
                sceneAsset = value;
                scenePath = sceneAsset != null ? AssetDatabase.GetAssetPath(sceneAsset) : string.Empty;
            }
        }

        [SerializeField]
        [HideInInspector]
        SceneAsset sceneAsset;

        /// <summary>
        /// 将当前场景添加到 BuildSettings。
        /// </summary>
        void AddCurrentSceneToBuildSettings()
        {
            var buildSettings = EditorBuildSettings.scenes.ToList();
            buildSettings.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = buildSettings.ToArray();
        }

        void ResetScene()
        {
            SceneAsset = null;
        }

        Color GetSceneAssetColor() => NotInBuildSettings ? Color.red : Color.white;
#endif
    }
}
