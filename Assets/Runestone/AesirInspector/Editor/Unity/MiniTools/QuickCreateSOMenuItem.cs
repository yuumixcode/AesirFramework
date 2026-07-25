// ----------------------------------------------------------------------------
// MIT License
// 
// Copyright (c) 2026 Runestone - Yuumix
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ----------------------------------------------------------------------------

using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// 右键快捷生成 ScriptableObject 资源文件
    /// </summary>
    [Summary("右键快捷生成 ScriptableObject 资源文件")]
    public static class QuickCreateSOMenuItem
    {
        const string MenuName = "Assets/Aesir Inspector/Create SO Asset From Selected";

        [MenuItem(MenuName, true)]
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

        [MenuItem(MenuName)]
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
                AesirInspectorLogger.Info($"生成一个 SO 资源，路径为: {assetPath}");
            }

            AssetDatabase.Refresh();
        }

        #endregion
    }
}
