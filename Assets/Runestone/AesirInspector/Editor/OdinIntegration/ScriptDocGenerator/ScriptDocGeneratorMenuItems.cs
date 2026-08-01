using System.Linq;
using Runestone.AesirInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    public static class ScriptDocGeneratorMenuItems
    {
        static MonoScript[] SelectionMonoScripts => Selection
            .GetFiltered(typeof(MonoScript), SelectionMode.Assets).Cast<MonoScript>().ToArray();

        static readonly string PanelsPath =
            AesirInspectorPaths.EditorDefaultResourcesPath + "/ScriptDocGenerator/Panels";

        static SingleScriptPanelSO GetSingleScriptPanel() =>
            ScriptableObjectSafeEditorUtility.GetOrCreateEditorScriptableObject<SingleScriptPanelSO>(
                "SingleScriptPanel", PanelsPath, "SingleScriptPanel");

        static MultipleScriptsPanelSO GetMultipleScriptsPanel() =>
            ScriptableObjectSafeEditorUtility.GetOrCreateEditorScriptableObject<MultipleScriptsPanelSO>(
                "MultipleScriptsPanel", PanelsPath, "MultipleScriptsPanel");

        [MenuItem(AesirInspectorMenuItems.AddScriptToTargetType, false,
            AesirInspectorMenuItems.AddScriptToTargetTypeOrder)]
        public static void AddScriptToTargetType()
        {
            var monoScript = SelectionMonoScripts.First();
            var targetType = monoScript.GetClass();
            var panel = GetSingleScriptPanel();
            panel.TargetType = targetType;
            Debug.Log("设置 Script Doc Generator 的 Target Type 为：" + targetType.FullName);
        }

        [MenuItem(AesirInspectorMenuItems.AddScriptToTargetTypeAndOpenWindow, false,
            AesirInspectorMenuItems.AddScriptToTargetTypeAndOpenWindowOrder)]
        public static void AddScriptToTargetTypeAndOpenWindow()
        {
            AddScriptToTargetType();
            ScriptDocGeneratorWindow.OpenWindow();
        }

        [MenuItem(AesirInspectorMenuItems.AddScriptsToTemporaryTypes, false,
            AesirInspectorMenuItems.AddScriptsToTemporaryTypesOrder)]
        public static void AddScriptsToTargetTypes()
        {
            var monoScripts = SelectionMonoScripts.ToList();
            var types = monoScripts.Select(x => x.GetClass()).ToList();
            var panel = GetMultipleScriptsPanel();
            var temporaryTypes = panel.TemporaryTypes;
            temporaryTypes.AddRange(types);
            panel.TemporaryTypes = temporaryTypes.Distinct().ToList();
            foreach (var type in types)
                Debug.Log("添加到 Script Doc Generator 的 Temporary Types：" + type.FullName);
        }

        [MenuItem(AesirInspectorMenuItems.AddScriptsToTemporaryTypesAndOpenWindow, false,
            AesirInspectorMenuItems.AddScriptsToTemporaryTypesAndOpenWindowOrder)]
        public static void AddScriptsToTemporaryTypesAndOpenWindow()
        {
            AddScriptsToTargetTypes();
            ScriptDocGeneratorWindow.OpenWindow();
        }

        [MenuItem(AesirInspectorMenuItems.AddScriptToTargetType, true)]
        public static bool AddScriptToTargetTypeValidate()
        {
            var length = SelectionMonoScripts.Length;
            if (length != 1)
                return false;

            var monoScript = SelectionMonoScripts[0];
            return monoScript.GetClass() != null;
        }

        [MenuItem(AesirInspectorMenuItems.AddScriptToTargetTypeAndOpenWindow, true)]
        public static bool AddScriptToTargetTypeAndOpenWindowValidate() =>
            AddScriptToTargetTypeValidate();

        [MenuItem(AesirInspectorMenuItems.AddScriptsToTemporaryTypes, true)]
        public static bool AddScriptsToTargetTypesValidate()
        {
            var length = SelectionMonoScripts.Length;
            if (length < 1)
                return false;

            foreach (var monoScript in SelectionMonoScripts)
            {
                if (monoScript.GetClass() == null)
                    return false;
            }

            return true;
        }

        [MenuItem(AesirInspectorMenuItems.AddScriptsToTemporaryTypesAndOpenWindow, true)]
        public static bool AddScriptsToTemporaryTypesAndOpenWindowValidate() =>
            AddScriptsToTargetTypesValidate();
    }
}
