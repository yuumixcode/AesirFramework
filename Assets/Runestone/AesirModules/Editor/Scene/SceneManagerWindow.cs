using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirModules.Editor
{
    public class SceneManagerWindow : OdinEditorWindow
    {
        [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        [SerializeField]
        SceneEditorSettings settings;

        protected override void OnEnable()
        {
            base.OnEnable();
            settings = SceneEditorSettings.instance;
        }

        [MenuItem("Tools/Aesir/Scene Editor Settings")]
        static void Open()
        {
            var window = GetWindow<SceneManagerWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenterXY(500f, 600f);
            window.titleContent = new GUIContent("Scene Editor Settings");
            window.Show();
        }
    }
}
