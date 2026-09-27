using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// RuntimeInitializeLoadType 示例窗口 — 可视化调整各时机开关与 Configurable Enter Play Mode 设置。
    /// </summary>
    public class RuntimeInitializeLoadTypeWindow : OdinEditorWindow
    {
        [Title("RuntimeInitializeLoadType", "五个初始化时机的执行顺序与最佳实践示例")]
        [InfoBox(
            "官方文档：https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RuntimeInitializeOnLoadMethodAttribute.html",
            InfoMessageType.None)]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public RuntimeInitializeLoadTypeSettings runtimeInitializeLoadTypeSettings;

        protected override void OnEnable()
        {
            base.OnEnable();
            runtimeInitializeLoadTypeSettings = RuntimeInitializeLoadTypeSettings.instance;
        }

        // priority 995：承担 Architecture 组的组级排序锚点（父菜单 priority 由子项最小值决定），
        // 使 Architecture 组位于 Modules 组（998）之前；差值 ≤10 不产生分割线
        [MenuItem("Tools/Aesir/Architecture/Samples/RuntimeInitializeLoadType", false, 995)]
        public static void ShowWindow()
        {
            var window = GetWindow<RuntimeInitializeLoadTypeWindow>();
            window.titleContent = new GUIContent("RuntimeInitializeLoadType");
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 700);
            window.Show();
        }
    }
}
