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

        // 归入 Tools/Aesir/Modules/ 子菜单（Aesir Modules 包专属工具）；默认 priority 1000，
        // 组内位于 Script Doc Generator（999）之后，差值 ≤ 10 不产生分割线
        [MenuItem("Tools/Aesir/Modules/Scene Editor Settings")]
        static void Open()
        {
            var window = GetWindow<SceneManagerWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenterXY(500f, 600f);
            window.titleContent = new GUIContent("Scene Editor Settings");
            window.Show();
        }
    }
}
