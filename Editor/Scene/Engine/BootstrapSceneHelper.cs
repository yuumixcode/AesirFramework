using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// 启动场景帮助类，自动查找项目中的启动场景，并将其设置为第一个加载的场景
    /// </summary>
    [InitializeOnLoad]
    public static class BootstrapSceneHelper
    {
        static readonly string[] PresetBootstrapSceneNames =
        {
            "Bootstrap", "BootstrapScene", "Bootstrapper", "BootstrapperScene", "bootstrap_scene",
            "bootstrap", "bootstrapper_scene", "bootstrapper"
        };

        /// <summary>
        /// 静态构造函数配合 [InitializeOnLoad] 特性，在编译后立刻执行一次。
        /// 没有标记特性时曾导致重新编译后的第一次进入 Play Mode 无法强制启动 Bootstrapper
        /// </summary>
        static BootstrapSceneHelper()
        {
            ResetEvent();
        }

        static SceneEditorSettings SceneEditorSettings => SceneEditorSettings.instance;

        /// <summary>
        /// 兼容 Enter Play Mode 时的事件重置。防御性编程。
        /// </summary>
        [InitializeOnEnterPlayMode]
        static void ResetEvent()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static void SetupBootstrapScene(bool silent = false)
        {
            if (!silent)
            {
                Debug.Log("BootstrapSceneHelper 执行 SetupBootstrapScene()");
            }

            var scenes = EditorBuildSettings.scenes;
            var bootstrapperIndex = -1;
            var currentBootstrapSceneName = string.Empty;
            for (var i = 0; i < scenes.Length; i++)
            {
                for (var j = 0; j < PresetBootstrapSceneNames.Length; j++)
                {
                    if (Path.GetFileNameWithoutExtension(scenes[i].path) != PresetBootstrapSceneNames[j])
                    {
                        continue;
                    }

                    bootstrapperIndex = i;
                    currentBootstrapSceneName = PresetBootstrapSceneNames[j];
                    break;
                }

                if (bootstrapperIndex != -1)
                {
                    break;
                }
            }

            if (bootstrapperIndex != -1)
            {
                var bootstrapperScene = scenes[bootstrapperIndex];
                if (bootstrapperIndex > 0)
                {
                    var sceneList = new List<EditorBuildSettingsScene>(scenes);
                    sceneList.RemoveAt(bootstrapperIndex);
                    sceneList.Insert(0, bootstrapperScene);
                    EditorBuildSettings.scenes = sceneList.ToArray();
                    if (!silent)
                    {
                        Debug.Log($"[BootstrapSceneHelper] 移动 {currentBootstrapSceneName} 场景，修改其序号为 0 ！");
                    }
                }

                SceneEditorSettings.instance.BootstrapperScenePath = bootstrapperScene.path;
            }
            else
            {
                var guids = Array.Empty<string>();

                for (var i = 0; i < PresetBootstrapSceneNames.Length; i++)
                {
                    guids = AssetDatabase.FindAssets($"{PresetBootstrapSceneNames[i]} t:Scene");
                    if (guids.Length > 0)
                    {
                        currentBootstrapSceneName = PresetBootstrapSceneNames[i];
                        break;
                    }
                }

                if (guids.Length <= 0)
                {
                    return;
                }

                var path = "";
                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetFileNameWithoutExtension(assetPath) != currentBootstrapSceneName)
                    {
                        continue;
                    }

                    path = assetPath;
                    break;
                }

                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                var sceneList = new List<EditorBuildSettingsScene>(scenes);
                sceneList.Insert(0, new EditorBuildSettingsScene(path, true));
                SceneEditorSettings.instance.BootstrapperScenePath = path;
                EditorBuildSettings.scenes = sceneList.ToArray();
                if (!silent)
                {
                    Debug.Log($"[BootstrapSceneHelper] 添加 {path} 到 Build Settings，且序号设置为 0 ！");
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void AutoSetupBootstrapScene()
        {
            if (!SceneEditorSettings.instance.SetupBootstrapper)
            {
                return;
            }

            SetupBootstrapScene();
        }

        static void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (!SceneEditorSettings.FirstLoadBootstrapScene)
            {
                return;
            }

            SetupBootstrapScene(true);
            switch (playModeStateChange)
            {
                case PlayModeStateChange.ExitingEditMode:
                    SceneEditorSettings.PreviousScenePath = SceneManager.GetActiveScene().path;
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() &&
                        IsSceneInBuildSettings(SceneEditorSettings.instance.BootstrapperScenePath))
                    {
                        EditorSceneManager.OpenScene(SceneEditorSettings.instance.BootstrapperScenePath);
                    }

                    break;

                case PlayModeStateChange.EnteredEditMode:
                    if (!string.IsNullOrEmpty(SceneEditorSettings.instance.PreviousScenePath))
                    {
                        EditorSceneManager.OpenScene(SceneEditorSettings.instance.PreviousScenePath);
                    }

                    break;
            }
        }

        static bool IsSceneInBuildSettings(string scenePath)
        {
            return !string.IsNullOrEmpty(scenePath) &&
                   EditorBuildSettings.scenes.Any(scene => scene.path == scenePath);
        }
    }
}
