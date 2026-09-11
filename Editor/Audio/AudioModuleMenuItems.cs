using UnityEditor;
using UnityEngine;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// 音频模块编辑器菜单项。提供在场景中预放置 AudioModule 的快捷入口。
    /// </summary>
    public static class AudioModuleMenuItems
    {
        /// <summary>
        /// 在 Hierarchy 窗口右键菜单中创建预放置的 AudioModule GameObject。
        /// 预放置实例可在 Inspector 中配置 SFX 音源数量与配置资产，并在 Awake 按需创建音源组件。
        /// </summary>
        [MenuItem("GameObject/Aesir Modules/Audio/Create AudioModule", false, -99)]
        static void CreateAudioModule(MenuCommand command)
        {
            var go = new GameObject("AudioModule");
            go.AddComponent<AudioModule>();
            Undo.RegisterCreatedObjectUndo(go, "Create AudioModule");
            Selection.activeGameObject = go;
        }
    }
}
