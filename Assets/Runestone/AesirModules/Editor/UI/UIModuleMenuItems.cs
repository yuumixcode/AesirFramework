using System.IO;
using Runestone.AesirArchitecture;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// UI 模块编辑器菜单项。提供快捷创建 UIRoot 和默认 Canvas 配置资产的入口。
    /// </summary>
    public static class UIModuleMenuItems
    {
        /// <summary>
        /// 在 Hierarchy 窗口右键菜单中创建一个带完整层级结构的 UIRoot GameObject。
        /// 创建后自动调用 <see cref="UIRoot.Build"/> 构建分层 Canvas。
        /// </summary>
        [MenuItem("GameObject/Aesir Modules/UI/Create UIRoot", false, -99)]
        static void CreateUIRoot(MenuCommand command)
        {
            var go = new GameObject("UIRoot");
            var uiRoot = go.AddComponent<UIRoot>();
            uiRoot.Build();
            Undo.RegisterCreatedObjectUndo(go, "Create UIRoot");
            Selection.activeGameObject = go;
        }

        /// <summary>
        /// 在 Project 窗口的 Create 菜单中创建默认的 <see cref="UICanvasConfigSO"/> 资产。
        /// 资产固定创建在 <see cref="UIRoot.DefaultCanvasConfigPath"/> 路径下，已存在时不重复创建。
        /// </summary>
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
