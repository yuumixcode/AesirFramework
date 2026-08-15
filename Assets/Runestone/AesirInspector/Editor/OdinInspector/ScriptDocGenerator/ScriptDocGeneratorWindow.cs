using Runestone.AesirInspector.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
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

            AesirInspectorLanguageSettingsSO.LanguageChanged -= Repaint;
            AesirInspectorLanguageSettingsSO.LanguageChanged += Repaint;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ScriptDocGeneratorPanelSO.ToastRequested -= ShowToast;
            AesirInspectorLanguageSettingsSO.LanguageChanged -= Repaint;
        }

        [MenuItem(AesirInspectorMenuItems.ScriptDocGenerator, false,
            AesirInspectorMenuItems.ScriptDocGeneratorOrder)]
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
            if (_panelSO == null)
            {
                _panelSO = ScriptDocGeneratorPanelSO.Instance;
                if (_panelSO == null)
                {
                    return;
                }
            }

            _soTree ??= PropertyTree.Create(_panelSO);
            _soTree.Draw(false);
        }

        new void ShowToast(ToastPosition position,
            SdfIconType icon,
            string message,
            Color color,
            float duration)
        {
            ShowNotification(new GUIContent(message), duration);
        }
    }
}
