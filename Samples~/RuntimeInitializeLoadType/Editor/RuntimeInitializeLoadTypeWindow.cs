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
        [Title("RuntimeInitializeLoadType", "五个初始化时机的执行顺序与最佳实践示例", TitleAlignments.Left)]
        [InfoBox("官方文档：https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RuntimeInitializeOnLoadMethodAttribute.html",
            InfoMessageType.None)]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public RuntimeInitializeLoadTypeSettings runtimeInitializeLoadTypeSettings;

        protected override void OnEnable()
        {
            base.OnEnable();
            runtimeInitializeLoadTypeSettings = RuntimeInitializeLoadTypeSettings.instance;
        }

        [MenuItem("Tools/Aesir/Architecture/Samples/RuntimeInitializeLoadType")]
        public static void ShowWindow()
        {
            var window = GetWindow<RuntimeInitializeLoadTypeWindow>();
            window.titleContent = new GUIContent("RuntimeInitializeLoadType");
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 700);
            window.Show();
        }
    }
}
