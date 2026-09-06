#if UNITY_EDITOR
using Runestone.AesirArchitecture;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// Binder 层级右键菜单快捷入口: 为选中物体快速挂载 <see cref="BinderAssistant" /> 与 <see cref="BinderTag" />，
    /// 免去 Add Component 菜单的层层查找。
    /// </summary>
    static class BinderMenuItems
    {
        const string MenuRoot = "GameObject/Aesir/";
        const int MenuPriority = 5000;

        [MenuItem(MenuRoot + "挂载 BinderAssistant", false, MenuPriority)]
        static void AttachAssistant(MenuCommand command)
        {
            foreach (var gameObject in Selection.gameObjects)
            {
                if (gameObject.GetComponent<BinderAssistant>() != null)
                {
                    continue;
                }

                Undo.AddComponent<BinderAssistant>(gameObject);
                MarkSceneDirty(gameObject);
            }
        }

        [MenuItem(MenuRoot + "挂载 BinderAssistant", true)]
        static bool ValidateAttachAssistant()
        {
            return Selection.gameObjects.Length > 0;
        }

        [MenuItem(MenuRoot + "添加 BinderTag 标记", false, MenuPriority + 1)]
        static void AttachTag(MenuCommand command)
        {
            foreach (var gameObject in Selection.gameObjects)
            {
                if (gameObject.GetComponent<BinderTag>() != null)
                {
                    continue;
                }

                Undo.AddComponent<BinderTag>(gameObject);
                MarkSceneDirty(gameObject);
            }
        }

        [MenuItem(MenuRoot + "添加 BinderTag 标记", true)]
        static bool ValidateAttachTag()
        {
            return Selection.gameObjects.Length > 0;
        }

        static void MarkSceneDirty(GameObject gameObject)
        {
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
    }
}
#endif
