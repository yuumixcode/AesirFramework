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
        /// <summary>
        /// 预设的启动场景名称列表（单一事实来源：与运行时 SceneModule 共用 <see cref="SceneModule.PresetBootstrapSceneNames" />）
        /// </summary>
        static readonly string[] PresetBootstrapSceneNames = SceneModule.PresetBootstrapSceneNames.ToArray();

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
                    // FindAssets/GetSceneByName 均为大小写不敏感匹配，此处保持一致，
                    // 否则 "bootstrap.unity" 会被 "Bootstrap" 预设漏检
                    if (!string.Equals(Path.GetFileNameWithoutExtension(scenes[i].path),
                            PresetBootstrapSceneNames[j], StringComparison.OrdinalIgnoreCase))
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
                // 每个预设名独立做"FindAssets → 精确文件名过滤"：FindAssets 是大小写不敏感的
                // 子串匹配（Foo_Bootstrapper 也会命中 Bootstrapper），命中子串但精确过滤
                // 落空时必须继续尝试下一个预设名，不能提前断定——否则项目里只有小写
                // bootstrap.unity 时会被先命中的子串结果吞掉真实启动场景的注册
                var path = "";
                for (var i = 0; i < PresetBootstrapSceneNames.Length; i++)
                {
                    var presetName = PresetBootstrapSceneNames[i];
                    var guids = AssetDatabase.FindAssets($"{presetName} t:Scene");
                    foreach (var guid in guids)
                    {
                        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        if (!string.Equals(Path.GetFileNameWithoutExtension(assetPath), presetName,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        path = assetPath;
                        currentBootstrapSceneName = presetName;
                        break;
                    }

                    if (!string.IsNullOrEmpty(path))
                    {
                        break;
                    }
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
