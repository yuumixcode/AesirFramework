using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// <see cref="AesirEventArgsSO" /> 自定义 Inspector：
    /// 运行模式（Play Mode）限定的事件触发按钮 + 事件参数配置字段。
    /// </summary>
    [CustomEditor(typeof(AesirEventArgsSO))]
    public class AesirEventArgsSOEditor : UnityEditor.Editor
    {
        Button _raiseButton;

        void OnEnable() => EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        void OnDisable() => EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

        void OnPlayModeStateChanged(PlayModeStateChange change) => RefreshRaiseButton();

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            _raiseButton = new Button(Raise)
            {
                text = "触发事件（Raise）",
                tooltip = "仅运行模式（Play Mode）下可触发"
            };
            root.Add(_raiseButton);
            RefreshRaiseButton();

            // PropertyField(SerializedProperty) 构造即自绑定，经 SubclassSelectorDrawer 渲染类型下拉
            var argsProperty = serializedObject.FindProperty("eventArgs");
            root.Add(new PropertyField(argsProperty) { label = "事件参数（Event Args）" });

            return root;
        }

        void Raise()
        {
            ((AesirEventArgsSO)target).Raise();
        }

        void RefreshRaiseButton()
        {
            // 编辑器尚未创建 Inspector GUI 时（如模式切换早于绘制）按钮为 null
            if (_raiseButton != null)
            {
                _raiseButton.SetEnabled(Application.isPlaying);
            }
        }
    }
}
