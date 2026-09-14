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
    /// 项目同时安装 Aesir Inspector（独立包，写入 <c>AESIR_INSPECTOR</c> 宏）时本类整体不参与编译，
    /// 由其提供同名菜单，避免重复菜单项。
    /// </para>
    /// </summary>
    /// <remarks>
    /// 菜单优先级 80：实测 <c>Assets/Create/C# Script</c> 与 <c>Assets/Create/2D</c> 同为 81，
    /// 同段按注册顺序紧邻，无法用整数优先级插在两者之间；80 使该项位于 Folder（18）与 C# Script（81）之间。
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

                if (IsCreatableSoClass(script.GetClass()))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>类型是否可创建 SO 资源（非抽象且继承 ScriptableObject）。</summary>
        internal static bool IsCreatableSoClass(System.Type type) =>
            type != null && !type.IsAbstract && type.IsSubclassOf(typeof(ScriptableObject));

        /// <summary>
        /// 生成 SO 资源的默认显示名：去掉尾部 "SO" 后缀；
        /// 脚本名恰好为 "SO" 时保留原名（避免产生空资源名）。
        /// </summary>
        internal static string GetDefaultAssetName(string scriptName)
        {
            if (scriptName.EndsWith("SO") && scriptName.Length > 2)
            {
                return scriptName[..^2];
            }

            return scriptName;
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

            var defaultName = GetDefaultAssetName(script.name);

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
                if (!IsCreatableSoClass(scriptClass))
                {
                    continue;
                }

                if (Path.GetExtension(objAssetPath) != "")
                {
                    // GetDirectoryName 在 Windows 返回反斜杠——归一化为 Unity 资产路径分隔符
                    objAssetPath = Path.GetDirectoryName(objAssetPath)?.Replace('\\', '/');
                }

                var defaultName = GetDefaultAssetName(script.name);

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
