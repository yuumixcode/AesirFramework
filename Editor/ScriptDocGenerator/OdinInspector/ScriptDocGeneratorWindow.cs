using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// 脚本文档生成器窗口，直接展示 ScriptDocGeneratorSO 单面板。
    /// </summary>
    public class ScriptDocGeneratorWindow : OdinEditorWindow
    {
        const string WindowName = "Script Doc Generator";

        ScriptDocGeneratorPanelSO _panelSO;

        PropertyTree _soTree;

        protected override void OnEnable()
        {
            base.OnEnable();

            _panelSO = ScriptDocGeneratorPanelSO.Instance;

            ScriptDocGeneratorPanelSO.ToastRequested -= ShowToast;
            ScriptDocGeneratorPanelSO.ToastRequested += ShowToast;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ScriptDocGeneratorPanelSO.ToastRequested -= ShowToast;
            // 域重载 / 窗口销毁时释放 PropertyTree——不 Dispose 会在下次 GC 时报
            // "An Odin PropertyTree instance is being garbage collected without first having been disposed"
            _soTree?.Dispose();
            _soTree = null;
        }

        [MenuItem(ScriptDocGeneratorMenuPaths.ScriptDocGenerator, false,
            ScriptDocGeneratorMenuPaths.ScriptDocGeneratorOrder)]
        public static void OpenWindow()
        {
            if (!ScriptDocGeneratorUtility.EnsureInitialized())
            {
                return;
            }

            var window = GetWindow<ScriptDocGeneratorWindow>();
            window.titleContent = new GUIContent(WindowName);
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1000, 800);
            window.Show();
        }

        protected override void DrawEditor(int index)
        {
            // 绘制回调只做空值保护：单例解析在资产缺失时会执行 CreateAsset 与
            // AssetDatabase.Refresh，必须收敛在 OnEnable 等非绘制路径
            if (_panelSO == null)
            {
                return;
            }

            _soTree ??= PropertyTree.Create(_panelSO);
            _soTree.Draw(false);
        }
    }
}
