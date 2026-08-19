#if ODIN_INSPECTOR
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor.OdinIntegration
{
    /// <summary>
    /// Context 调试窗口 —— Odin 版（V2）。
    /// </summary>
    /// <remarks>
    /// 三版试做之一：样式基准线（Inspector 富样式开箱即用）。
    /// <para><b>核心优势</b>：<see cref="PropertyTree"/> 直接绑定模块实例——Odin 序列化协议自动处理
    /// <see cref="ObservableValue{T}"/>（<see cref="ObservableValueAttributeProcessor{T}"/> 已生效：
    /// InlineProperty 内联 + OnValueChanged 调 InvokeEvent）、非公有字段、多态列表。</para>
    /// <para>菜单：Tools → Aesir → Architecture → Debugger (Odin)。</para>
    /// </remarks>
    public sealed class OdinContextDebuggerWindow : OdinEditorWindow
    {
        [MenuItem("Tools/Aesir/Architecture/Debugger (Odin)")]
        static void Open()
        {
            var window = GetWindow<OdinContextDebuggerWindow>("Context Debugger (Odin)");
            window.minSize = new Vector2(560, 320);
            window.Show();
        }

        [EnumToggleButtons]
        [LabelText("Context")]
        [ShowInInspector]
        [PropertyOrder(-1)]
        [OnValueChanged(nameof(OnContextSelectionChanged))]
        string _selectedContextName;

        [ShowInInspector]
        [PropertyOrder(0)]
        [LabelText(" ")]
        [HideLabel]
        [InfoBox("请在上方选择 Context", InfoMessageType.Info, nameof(HasNoSelection))]
        ContextRegistryScanner.Entry _selectedEntry;

        [ShowInInspector]
        [PropertyOrder(1)]
        [LabelText("Models")]
        [ListDrawerSettings(ShowFoldout = true, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        [ReadOnly]
        List<object> _models = new List<object>();

        [ShowInInspector]
        [PropertyOrder(2)]
        [LabelText("Services")]
        [ListDrawerSettings(ShowFoldout = true, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        [ReadOnly]
        List<object> _services = new List<object>();

        List<ContextRegistryScanner.Entry> _contexts = new List<ContextRegistryScanner.Entry>();
        List<string> _contextNames = new List<string>();

        bool HasNoSelection => _selectedEntry == null;

        protected override void OnEnable()
        {
            base.OnEnable();
            Refresh();
        }

        void Refresh()
        {
            _contexts = ContextRegistryScanner.Scan();
            _contextNames = _contexts.Select(c => c.DisplayName).ToList();
        }

        [Button("刷新", ButtonSizes.Medium)]
        [PropertyOrder(-2)]
        void RefreshButton()
        {
            Refresh();
            OnContextSelectionChanged();
        }

        IEnumerable<string> GetContextNames() => _contextNames;

        void OnContextSelectionChanged()
        {
            _selectedEntry = _contexts.FirstOrDefault(c => c.DisplayName == _selectedContextName);
            _models.Clear();
            _services.Clear();

            if (_selectedEntry?.Instance == null)
            {
                return;
            }

            _models.AddRange(_selectedEntry.Instance.GetAllModels());
            _services.AddRange(_selectedEntry.Instance.GetAllServices());
        }

        [Button("初始化", ButtonSizes.Small)]
        [PropertyOrder(-1)]
        [ShowIf(nameof(ShowInitializeButton))]
        void InitializeSelected()
        {
            if (_selectedEntry == null)
            {
                return;
            }

            ContextRegistryScanner.EnsureInitialized(_selectedEntry.ContextType);
            OnContextSelectionChanged();
        }

        bool ShowInitializeButton => _selectedEntry != null && !_selectedEntry.Initialized;
    }
}
#endif
