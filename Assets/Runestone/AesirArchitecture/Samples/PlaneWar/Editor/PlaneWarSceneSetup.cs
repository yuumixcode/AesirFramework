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
    /// 菜单路径：Tools → Aesir → PlaneWar → Fix Scene References
    /// </remarks>
    public static class PlaneWarSceneSetup
    {
        const string ScenePath = "Assets/Samples/Aesir Architecture/0.12.0/PlaneWar/Scene/SampleForPlaneWarMono.unity";
        const string PrefabDir = "Assets/Samples/Aesir Architecture/0.12.0/PlaneWar/Prefab";

        [MenuItem("Tools/Aesir/PlaneWar/Fix Scene References")]
        static void FixReferences()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

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
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/Player.prefab");
            var bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/Bullet.prefab");
            if (playerPrefab != null && bulletPrefab != null)
            {
                var playerSo = new SerializedObject(playerPrefab.GetComponent<SamplePlaneWarMonoPlayer>());
                playerSo.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab.GetComponent<SamplePlaneWarMonoBullet>();
                playerSo.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(playerPrefab);
            }

            // 3. 保存
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[PlaneWar] Scene references fixed successfully.");
        }
    }
}
