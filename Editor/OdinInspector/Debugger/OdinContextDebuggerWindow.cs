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
    /// Context 调试窗口 —— Odin 版（最终选定方案，2026-08-19 用户选型）。
    /// </summary>
    /// <remarks>
    /// 三版试做的选定版本。<b>仅显示已初始化的 Context</b>——未初始化的不显示、不操作
    /// （调试器定位：运行时观察已注册的架构实例，而非初始化入口）。
    /// <para><b>核心优势</b>：<see cref="PropertyTree"/> 直接绑定模块实例——Odin 序列化协议自动处理
    /// <see cref="ObservableValue{T}"/>（<see cref="ObservableValueAttributeProcessor{T}"/> 已生效：
    /// InlineProperty 内联 + OnValueChanged 调 InvokeEvent），可直观看到 Model 的值并拖拽调试。</para>
    /// <para>菜单：Tools → Aesir → Architecture → Context Debugger。</para>
    /// </remarks>
    public sealed class OdinContextDebuggerWindow : OdinEditorWindow
    {
        [MenuItem("Tools/Aesir/Architecture/Context Debugger")]
        static void Open()
        {
            var window = GetWindow<OdinContextDebuggerWindow>("Context Debugger");
            window.minSize = new Vector2(560, 320);
            window.Show();
        }

        [ValueDropdown(nameof(GetInitializedContextNames))]
        [LabelText("Context")]
        [ShowInInspector]
        [PropertyOrder(-1)]
        [OnValueChanged(nameof(OnContextSelectionChanged))]
        string _selectedContextName;

        [ShowInInspector]
        [PropertyOrder(0)]
        [HideLabel]
        [InfoBox("请在上方选择一个已初始化的 Context", InfoMessageType.Info, nameof(HasNoSelection))]
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

        List<ContextRegistryScanner.Entry> _initializedContexts = new List<ContextRegistryScanner.Entry>();

        bool HasNoSelection => _selectedEntry == null;

        protected override void OnEnable()
        {
            base.OnEnable();
            Refresh();
        }

        [Button("刷新", ButtonSizes.Medium)]
        [PropertyOrder(-2)]
        void Refresh()
        {
            // 仅保留已初始化的 Context（未初始化不显示、不操作）
            _initializedContexts = ContextRegistryScanner.Scan()
                .Where(c => c.Initialized)
                .ToList();

            // 当前选中项若已不再初始化，清空选择
            if (_selectedEntry != null && !_initializedContexts.Any(c => c.DisplayName == _selectedEntry.DisplayName))
            {
                _selectedEntry = null;
                _selectedContextName = null;
                _models.Clear();
                _services.Clear();
            }
            else
            {
                OnContextSelectionChanged();
            }
        }

        IEnumerable<string> GetInitializedContextNames() =>
            _initializedContexts.Select(c => c.DisplayName);

        void OnContextSelectionChanged()
        {
            _selectedEntry = _initializedContexts.FirstOrDefault(c => c.DisplayName == _selectedContextName);
            _models.Clear();
            _services.Clear();

            if (_selectedEntry?.Instance == null)
            {
                return;
            }

            _models.AddRange(_selectedEntry.Instance.GetAllModels());
            _services.AddRange(_selectedEntry.Instance.GetAllServices());
        }
    }
}
#endif
