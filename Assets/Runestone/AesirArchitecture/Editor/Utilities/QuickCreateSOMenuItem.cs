#if !AESIR_INSPECTOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// 右键快捷生成 ScriptableObject 资源文件。
    /// <para>
    /// 复刻自 Aesir Inspector 的同名工具（<c>Runestone.AesirInspector.Editor.QuickCreateSOMenuItem</c>）。
    /// 当项目同时安装 Aesir Inspector（即存在 <c>AESIR_INSPECTOR</c> 宏定义）时本类整体不参与编译，
    /// 由 Aesir Inspector 版本提供该菜单，避免重复菜单项与功能分裂。
    /// </para>
    /// </summary>
    /// <remarks>
    /// 条件编译的失效机制依赖 Aesir Inspector 的 <c>EnsureAesirInspectorDefine</c> 在编辑器加载时自动写入
    /// <c>AESIR_INSPECTOR</c> 宏定义；未安装 Aesir Inspector 时该宏不存在，本类生效。
    /// <para>
    /// 菜单优先级 80：实测 <c>Assets/Create/C# Script</c> 与 <c>Assets/Create/2D</c> 同为 81，
    /// 同段按注册顺序紧邻，无法用整数优先级插在两者之间；80 使该项位于 Folder（18）与 C# Script（81）之间。
    /// </para>
    /// </remarks>
    public static class QuickCreateSOMenuItem
    {
        const string MenuName = "Assets/Create/Create SO Asset From Selected";

        [MenuItem(MenuName, true, 80)]
        static bool CanCreateScriptableObjectFromSelected()
        {
            var selectedObject = Selection.activeObject;
            if (!selectedObject)
            {
                return false;
            }

            foreach (var obj in Selection.objects)
            {
                if (obj is not MonoScript script)
                {
                    continue;
                }

                var scriptClass = script.GetClass();
                if (scriptClass == null)
                {
                    continue;
                }

                if (!scriptClass.IsAbstract && scriptClass.IsSubclassOf(typeof(ScriptableObject)))
                {
                    return true;
                }
            }

            return false;
        }

        [MenuItem(MenuName, false, 80)]
        static void CreateScriptableObjectFromSelected()
        {
            if (Selection.objects.Length == 1)
            {
                SingleSelectCreateSO();
            }
            else
            {
                MultiSelectCreateSO();
            }
        }

        #region Internal

        static void SingleSelectCreateSO()
        {
            if (Selection.activeObject is not MonoScript script)
            {
                return;
            }

            var instance = ScriptableObject.CreateInstance(script.GetClass());

            var defaultName = script.name;
            if (defaultName.EndsWith("SO"))
            {
                defaultName = defaultName[..^2];
            }

            ProjectWindowUtil.CreateAsset(instance, $"{defaultName}.asset");
            Selection.activeObject = instance;
        }

        static void MultiSelectCreateSO()
        {
            foreach (var guid in Selection.assetGUIDs)
            {
                var objAssetPath = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<Object>(objAssetPath);
                if (obj is not MonoScript script)
                {
                    continue;
                }

                var scriptClass = script.GetClass();
                if (scriptClass == null)
                {
                    continue;
                }

                if (!scriptClass.IsSubclassOf(typeof(ScriptableObject)) || scriptClass.IsAbstract)
                {
                    continue;
                }

                if (Path.GetExtension(objAssetPath) != "")
                {
                    objAssetPath = Path.GetDirectoryName(objAssetPath);
                }

                var defaultName = script.name;
                if (defaultName.EndsWith("SO"))
                {
                    defaultName = defaultName[..^2];
                }

                var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{objAssetPath}/{defaultName}.asset");
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance(scriptClass), assetPath);
                AssetDatabase.SaveAssets();
                AesirArchitectureDebug.Log($"生成一个 SO 资源，路径为: {assetPath}");
            }

            AssetDatabase.Refresh();
        }

        #endregion
    }
}
#endif