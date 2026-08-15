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

        [MenuItem(AddScriptToTargetTypeMenuName, false, AddScriptToTargetTypeMenuNameOrder)]
        public static void AddScriptToTargetType()
        {
            var monoScript = SelectionMonoScripts.First();
            var targetType = monoScript.GetClass();
            ScriptDocGeneratorPanelSO.Instance.TargetType = targetType;
            Debug.Log("设置 Script Doc Generator 的 Target Type 为：" + targetType.FullName);
        }

        [MenuItem(AddScriptToTargetTypeAndOpenWindowMenuName, false,
            AddScriptToTargetTypeAndOpenWindowMenuNameOrder)]
        public static void AddScriptToTargetTypeAndOpenWindow()
        {
            if (!ScriptDocGeneratorUtility.EnsureInitialized())
            {
                return;
            }

            AddScriptToTargetType();
            ScriptDocGeneratorWindow.OpenWindow();
        }

        [MenuItem(AddScriptsToTemporaryTypesMenuName, false, AddScriptsToTemporaryTypesMenuNameOrder)]
        public static void AddScriptsToTargetTypes()
        {
            var monoScripts = SelectionMonoScripts.ToList();
            var types = monoScripts.Select(x => x.GetClass()).ToList();
            var so = ScriptDocGeneratorPanelSO.Instance;
            var temporaryTypes = so.TemporaryTypes;
            temporaryTypes.AddRange(types);
            so.TemporaryTypes = temporaryTypes.Distinct().ToList();
            foreach (var type in types)
            {
                Debug.Log("添加到 Script Doc Generator 的 Temporary Types：" + type.FullName);
            }
        }

        [MenuItem(AddScriptsToTemporaryTypesAndOpenWindowMenuName, false,
            AddScriptsToTemporaryTypesAndOpenWindowMenuNameOrder)]
        public static void AddScriptsToTemporaryTypesAndOpenWindow()
        {
            if (!ScriptDocGeneratorUtility.EnsureInitialized())
            {
                return;
            }

            AddScriptsToTargetTypes();
            ScriptDocGeneratorWindow.OpenWindow();
        }

        [MenuItem(AddScriptToTargetTypeMenuName, true)]
        public static bool AddScriptToTargetTypeValidate()
        {
            var length = SelectionMonoScripts.Length;
            if (length != 1)
            {
                return false;
            }

            var monoScript = SelectionMonoScripts[0];
            return monoScript.GetClass() != null;
        }

        [MenuItem(AddScriptToTargetTypeAndOpenWindowMenuName, true)]
        public static bool AddScriptToTargetTypeAndOpenWindowValidate() =>
            AddScriptToTargetTypeValidate();

        [MenuItem(AddScriptsToTemporaryTypesMenuName, true)]
        public static bool AddScriptsToTargetTypesValidate()
        {
            var length = SelectionMonoScripts.Length;
            if (length < 1)
            {
                return false;
            }

            foreach (var monoScript in SelectionMonoScripts)
            {
                if (monoScript.GetClass() == null)
                {
                    return false;
                }
            }

            return true;
        }

        [MenuItem(AddScriptsToTemporaryTypesAndOpenWindowMenuName, true)]
        public static bool AddScriptsToTemporaryTypesAndOpenWindowValidate() =>
            AddScriptsToTargetTypesValidate();

        #region 菜单项定义

        /// <summary>
        /// 将选中脚本添加到 Target Type 的菜单路径。
        /// </summary>
        const string AddScriptToTargetTypeMenuName =
            AesirInspectorMenuItems.AssetsScriptDocGeneratorRoot + "/Add To Target Type";

        /// <summary>
        /// Add To Target Type 菜单项优先级。
        /// </summary>
        const int AddScriptToTargetTypeMenuNameOrder = -50;

        /// <summary>
        /// 将选中脚本添加到 Target Type 并打开窗口的菜单路径。
        /// </summary>
        const string AddScriptToTargetTypeAndOpenWindowMenuName =
            AesirInspectorMenuItems.AssetsScriptDocGeneratorRoot + "/Add To Target Type And Open Window";

        /// <summary>
        /// Add To Target Type And Open Window 菜单项优先级。
        /// </summary>
        const int AddScriptToTargetTypeAndOpenWindowMenuNameOrder = -48;

        /// <summary>
        /// 将选中脚本添加到 Temporary Types 的菜单路径。
        /// </summary>
        const string AddScriptsToTemporaryTypesMenuName =
            AesirInspectorMenuItems.AssetsScriptDocGeneratorRoot + "/Add To Temporary Types";

        /// <summary>
        /// Add To Temporary Types 菜单项优先级。
        /// </summary>
        const int AddScriptsToTemporaryTypesMenuNameOrder = -43;

        /// <summary>
        /// 将选中脚本添加到 Temporary Types 并打开窗口的菜单路径。
        /// </summary>
        const string AddScriptsToTemporaryTypesAndOpenWindowMenuName =
            AesirInspectorMenuItems.AssetsScriptDocGeneratorRoot + "/Add To Temporary Types And Open Window";

        /// <summary>
        /// Add To Temporary Types And Open Window 菜单项优先级。
        /// </summary>
        const int AddScriptsToTemporaryTypesAndOpenWindowMenuNameOrder = -40;

        #endregion
    }
}
