using System;
using Runestone.AesirArchitecture.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// 事件测试窗口（UI Toolkit 版）：在编辑器中选择事件类型、配置载荷并手动触发，
    /// 用于不写触发代码即可复现事件、验证订阅链路。
    /// <para>
    /// 触发经由公开 API <see cref="AesirEventArgsSO.Raise" />，发布者为临时 SO 资产。
    /// 载荷配置复用已有的 <c>SubclassSelector</c> Drawer（经 <see cref="PropertyField" /> 绑定 SerializedProperty）。
    /// 仅在 Play Mode 可触发：分发走非创建式单例，编辑模式无实例、发布无事发生。
    /// </para>
    /// </summary>
    public class AesirEventTesterWindow : EditorWindow
    {
        const string MenuPath = "Tools/Aesir/Modules/Event Tester";
        Label _hint;
        Button _raiseButton;
        SerializedObject _serialized;

        AesirEventArgsSO _testAsset;

        /// <summary>Odin 版窗口的打开委托。</summary>
        public static Action OdinWindowOpener { get; private set; }

        void OnEnable()
        {
            BuildUI();
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            if (_testAsset != null)
            {
                DestroyImmediate(_testAsset);
                _testAsset = null;
            }
        }

        /// <summary>由 Odin 程序集在域加载期注册打开方式。</summary>
        public static void RegisterOdinWindowOpener(Action opener) => OdinWindowOpener = opener;

        [MenuItem(MenuPath)]
        static void Open()
        {
            if (AesirInspectorStyleSettings.UseOdin && OdinWindowOpener != null)
            {
                OdinWindowOpener();
                return;
            }

            var window = GetWindow<AesirEventTesterWindow>();
            window.titleContent = new GUIContent("Event Tester");
            window.minSize = new Vector2(420f, 340f);
            window.Show();
        }

        void OnPlayModeChanged(PlayModeStateChange change) => RefreshRaiseButton();

        void BuildUI()
        {
            var root = rootVisualElement;
            root.Clear();
            root.style.paddingLeft = 10f;
            root.style.paddingRight = 10f;
            root.style.paddingTop = 8f;

            _hint = EventDiagnosticsWindowHelper.Row("选择具体事件参数类型并填写载荷，点击「触发事件」发布。需进入 Play Mode。",
                EventDiagnosticsWindowHelper.MutedColor);
            _hint.style.whiteSpace = WhiteSpace.Normal;
            root.Add(_hint);

            if (_testAsset == null)
            {
                _testAsset = CreateInstance<AesirEventArgsSO>();
                _testAsset.name = "Aesir Event Tester";
                _serialized = new SerializedObject(_testAsset);
            }

            var argsProperty = _serialized.FindProperty("eventArgs");
            root.Add(new PropertyField(argsProperty) { label = "事件参数" });

            _raiseButton = new Button(Raise)
            {
                text = "触发事件（Raise）"
            };
            root.Add(_raiseButton);
            RefreshRaiseButton();
        }

        void Raise()
        {
            if (_serialized != null)
            {
                _serialized.ApplyModifiedProperties();
            }

            _testAsset.Raise();
        }

        void RefreshRaiseButton()
        {
            if (_raiseButton != null)
            {
                _raiseButton.SetEnabled(Application.isPlaying);
            }
        }
    }
}
