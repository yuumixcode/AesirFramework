using Runestone.AesirInspector.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    public class ScriptDocGeneratorWindow : OdinEditorWindow
    {
        const string ScriptDocGenWindowName = "Script Doc Generator";

        [SerializeField]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        ScriptDocGeneratorSO asset;

        protected override void OnEnable()
        {
            base.OnEnable();
            WindowPadding = new Vector4(10, 10, 10, 10);
            asset = ScriptDocGeneratorSO.Instance;
            ScriptDocGeneratorSO.ToastRequested -= ShowToast;
            ScriptDocGeneratorSO.ToastRequested += ShowToast;
        }

        protected override void OnDestroy()
        {
            ScriptDocGeneratorSO.ToastRequested -= ShowToast;
            base.OnDestroy();
        }

        [MenuItem(AesirInspectorMenuItems.ScriptDocGenerator, false,
            AesirInspectorMenuItems.ScriptDocGeneratorOrder)]
        public static void OpenWindow()
        {
            var window = GetWindow<ScriptDocGeneratorWindow>();
            window.titleContent = new GUIContent(ScriptDocGenWindowName);
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1000, 800);
            window.Show();
        }
    }
}
