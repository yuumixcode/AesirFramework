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

        [MenuItem(AesirInspectorMenuItems.AddScriptToTargetType, false,
            AesirInspectorMenuItems.AddScriptToTargetTypeOrder)]
        public static void AddScriptToTargetType()
        {
            var monoScript = SelectionMonoScripts.First();
            var targetType = monoScript.GetClass();
            ScriptDocGeneratorSO.Instance.TargetType = targetType;
            ScriptDocGeneratorSO.Instance.TypeSourceProperty = ScriptDocGeneratorSO.TypeSource.SingleType;
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
            var temporaryTypes = ScriptDocGeneratorSO.Instance.TemporaryTypes;
            temporaryTypes.AddRange(types);
            var distinctTypes = temporaryTypes.Distinct().ToList();
            ScriptDocGeneratorSO.Instance.TemporaryTypes = distinctTypes;
            ScriptDocGeneratorSO.Instance.TypeSourceProperty = ScriptDocGeneratorSO.TypeSource.MultipleTypes;
            foreach (var type in types)
            {
                Debug.Log("添加到 Script Doc Generator 的 Temporary Types：" + type.FullName);
            }
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
            {
                return false;
            }

            var monoScript = SelectionMonoScripts[0];
            var targetType = monoScript.GetClass();
            return targetType != null;
        }

        [MenuItem(AesirInspectorMenuItems.AddScriptToTargetTypeAndOpenWindow, true)]
        public static bool AddScriptToTargetTypeAndOpenWindowValidate() =>
            AddScriptToTargetTypeValidate();

        [MenuItem(AesirInspectorMenuItems.AddScriptsToTemporaryTypes, true)]
        public static bool AddScriptsToTargetTypesValidate()
        {
            var length = SelectionMonoScripts.Length;
            if (length < 1)
            {
                return false;
            }

            foreach (var monoScript in SelectionMonoScripts)
            {
                var targetType = monoScript.GetClass();
                if (targetType == null)
                {
                    return false;
                }
            }

            return true;
        }

        [MenuItem(AesirInspectorMenuItems.AddScriptsToTemporaryTypesAndOpenWindow, true)]
        public static bool AddScriptsToTemporaryTypesAndOpenWindowValidate() =>
            AddScriptsToTargetTypesValidate();
    }
}
