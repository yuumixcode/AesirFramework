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
        /// 创建后自动调用 <see cref="UIRoot.Build" /> 构建分层 Canvas。
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
        /// 在 Project 窗口的 Create 菜单中创建默认的 <see cref="UICanvasConfigSO" /> 资产。
        /// 资产固定创建在 <see cref="UIRoot.DefaultCanvasConfigPath" /> 路径下，已存在时不重复创建。
        /// </summary>
        [MenuItem("Assets/Create/Aesir Modules/UI/Default UICanvasConfig", false, -99)]
        static void CreateUICanvasConfigAsset()
        {
            // 幂等确保资产存在（已存在则加载复用），实现与 UIRoot 的 Inspector 按钮共用同一入口，
            // 不在此处重建目录/资产创建逻辑，避免两份等价实现产生行为漂移
            UIRoot.EnsureDefaultCanvasConfigAsset();
            AesirModulesDebug.Log(AesirModulesDebug.UIModuleTag,
                "已确保默认的 UICanvasConfig 资产存在，路径为：" + UIRoot.DefaultCanvasConfigPath);
        }
    }
}
