using Runestone.AesirInspector.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.Samples.LoadType.Editor
{
    public class RuntimeInitializeLoadTypeWindow : OdinEditorWindow
    {
        [HideLabel]
        [InlineProperty]
        public BilingualHeaderControl header;

        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public RuntimeInitializeLoadTypeSettings runtimeInitializeLoadTypeSettings;

        protected override void OnEnable()
        {
            base.OnEnable();
            header = new BilingualHeaderControl("RuntimeInitializeLoadType", "RuntimeInitializeLoadType",
                "五个初始化时机的执行顺序与最佳实践示例", "Execution order and best practices for five initialization timings",
                "https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RuntimeInitializeOnLoadMethodAttribute.html");
            runtimeInitializeLoadTypeSettings = RuntimeInitializeLoadTypeSettings.instance;
        }

        [MenuItem(AesirInspectorMenuItems.SampleRuntimeInitializeOnLoad, false,
            AesirInspectorMenuItems.SampleRuntimeInitializeOnLoadOrder)]
        public static void ShowWindow()
        {
            var window = GetWindow<RuntimeInitializeLoadTypeWindow>();
            window.titleContent =
                new GUIContent(AesirInspectorMenuItems.SampleRuntimeInitializeOnLoadWindowName);
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 700);
            window.Show();
        }
    }
}
