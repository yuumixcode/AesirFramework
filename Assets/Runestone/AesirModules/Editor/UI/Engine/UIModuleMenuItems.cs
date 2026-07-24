using System.IO;
using Runestone.AesirArchitecture;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirModules.Editor
{
    public static class UIModuleMenuItems
    {
        [MenuItem("GameObject/Aesir Modules/UI/Create UIRoot", false, -99)]
        static void CreateUIRoot(MenuCommand command)
        {
            var go = new GameObject("UIRoot");
            var uiRoot = go.AddComponent<UIRoot>();
            uiRoot.Build();
            Undo.RegisterCreatedObjectUndo(go, "Create UIRoot");
            Selection.activeGameObject = go;
        }

        [MenuItem("Assets/Create/Aesir Modules/UI/Default UICanvasConfig", false, -99)]
        static void CreateUICanvasConfigAsset()
        {
            const string defaultPath = UIRoot.DefaultCanvasConfigPath;
            var fileIsExist = File.Exists(defaultPath);
            if (fileIsExist)
            {
                AesirModulesDebug.LogWarning(AesirModulesDebug.UIModuleTag,
                    "默认的 UICanvasConfig 已存在，不能重复创建。路径为：" + defaultPath);
                return;
            }

            var directoryPath = Path.GetDirectoryName(defaultPath);
            if (directoryPath != null)
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                AssetDatabase.Refresh();
            }

            var asset = ScriptableObject.CreateInstance<UICanvasConfigSO>();
            AssetDatabase.CreateAsset(asset, defaultPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AesirModulesDebug.Log(AesirModulesDebug.UIModuleTag,
                "成功创建默认的 UICanvasConfig 资产，路径为：" + defaultPath);
        }
    }
}
