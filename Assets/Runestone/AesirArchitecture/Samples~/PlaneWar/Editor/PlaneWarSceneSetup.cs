using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirArchitecture.Samples.PlaneWarMono
{
    /// <summary>
    /// PlaneWar 示例场景一键修复工具。
    /// </summary>
    /// <remarks>
    /// 修复场景中 HUD 组件的 Text 引用和 Player prefab 的 bulletPrefab 引用。
    /// 场景与预制体目录不硬编码安装路径，而是按资源名全仓搜索——
    /// 包内 Samples/（开发仓库）与 UPM 导入副本（Assets/Samples/&lt;包名&gt;/&lt;版本&gt;/）两种布局均可定位。
    /// 菜单路径：Tools → Aesir → PlaneWar → Fix Scene References
    /// </remarks>
    public static class PlaneWarSceneSetup
    {
        const string SceneName = "SampleForPlaneWarMono";

        [MenuItem("Tools/Aesir/PlaneWar/Fix Scene References")]
        static void FixReferences()
        {
            var scenePath = FindAssetPath(SceneName + " t:Scene");
            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError($"[PlaneWar] 未找到场景 {SceneName}.unity，请确认 PlaneWar 示例已导入。");
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // 1. 修复 HUD 字段引用
            var hudGo = GameObject.Find("HUD");
            if (hudGo == null)
            {
                Debug.LogError("[PlaneWar] HUD not found in scene.");
                return;
            }

            var hud = hudGo.GetComponent<SamplePlaneWarMonoGameHUD>();
            if (hud == null)
            {
                Debug.LogError("[PlaneWar] SamplePlaneWarMonoGameHUD component not found.");
                return;
            }

            var so = new SerializedObject(hud);
            so.FindProperty("scoreText").objectReferenceValue = hudGo.transform.Find("ScoreText")?.GetComponent<Text>();
            so.FindProperty("timeText").objectReferenceValue = hudGo.transform.Find("TimeText")?.GetComponent<Text>();
            so.FindProperty("gameOverText").objectReferenceValue = hudGo.transform.Find("GameOverText")?.GetComponent<Text>();
            so.ApplyModifiedPropertiesWithoutUndo();

            // 2. 修复 Player prefab 的 bulletPrefab 引用
            // 场景位于 <示例根>/Scene/ 下，预制体约定在同级 <示例根>/Prefab/ 目录
            var sceneDir = Path.GetDirectoryName(scenePath)?.Replace('\\', '/');
            var exampleRootDir = Path.GetDirectoryName(sceneDir)?.Replace('\\', '/');
            var prefabDir = exampleRootDir != null ? exampleRootDir + "/Prefab" : null;
            var playerPrefab = prefabDir != null ? AssetDatabase.LoadAssetAtPath<GameObject>(prefabDir + "/Player.prefab") : null;
            var bulletPrefab = prefabDir != null ? AssetDatabase.LoadAssetAtPath<GameObject>(prefabDir + "/Bullet.prefab") : null;
            if (playerPrefab != null && bulletPrefab != null)
            {
                var playerSo = new SerializedObject(playerPrefab.GetComponent<SamplePlaneWarMonoPlayer>());
                playerSo.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab.GetComponent<SamplePlaneWarMonoBullet>();
                playerSo.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(playerPrefab);
            }
            else
            {
                Debug.LogError($"[PlaneWar] 未找到 Player/Bullet 预制体（预期目录：{prefabDir}）。");
            }

            // 3. 保存
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[PlaneWar] Scene references fixed successfully.");
        }

        /// <summary>按资源名过滤全仓搜索，返回首个命中资源的路径；无命中返回 null。</summary>
        static string FindAssetPath(string filter)
        {
            var guids = AssetDatabase.FindAssets(filter);
            return guids.Length > 0 ? AssetDatabase.GUIDToAssetPath(guids[0]) : null;
        }
    }
}
