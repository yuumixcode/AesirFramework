#if UNITY_EDITOR // 示例场景构建器：编辑器内编译，玩家构建整体剔除
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Runestone.AesirModules.Samples.Save.BasicUsage.Editor
{
    /// <summary>
    /// 生成 Save 基础用法示例场景：只含一个挂 <see cref="SampleSaveController" /> 的空物体
    /// （OnGUI 面板自带全部 UI，无需 Canvas / EventSystem / 相机）。
    /// </summary>
    /// <remarks>
    /// 菜单 <c>Tools/Aesir/Samples/Build Save BasicUsage Scene</c> 或 batchmode
    /// <c>-executeMethod Runestone.AesirModules.Samples.Save.BasicUsage.Editor.SaveBasicUsageSceneBuilder.Build</c>。
    /// 场景落在示例目录 Scene/ 下，随 Samples~/ 镜像分发。
    /// </remarks>
    public static class SaveBasicUsageSceneBuilder
    {
        const string ScenePath =
            "Assets/Runestone/AesirModules/Samples/Save/01_BasicUsage/Scene/SampleForSaveBasicUsage.unity";

        [MenuItem("Tools/Aesir/Samples/Build Save BasicUsage Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("SaveBasicUsageSample");
            go.AddComponent<SampleSaveController>();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Save Sample] 示例场景已生成：{ScenePath}");
        }
    }
}
#endif