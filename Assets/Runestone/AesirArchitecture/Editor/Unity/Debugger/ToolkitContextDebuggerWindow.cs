using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// Context 调试窗口 —— UI Toolkit 纯代码版（V3）。
    /// </summary>
    /// <remarks>
    /// 三版试做之一：现代感最强的一版（卡片式布局 + 状态徽标配色 + retained mode 性能）。
    /// <para><b>纯代码构建</b>：不用 UXML/UIBuilder，全部经 <c>VisualElement</c> + 内联 USS 风格构建——
    /// 样式可被 AI 读写与 diff，契合 G10 Inspector 精简原则。</para>
    /// <para><b>性能</b>：retained mode，仅数据变更时局部刷新，不每帧轮询。</para>
    /// <para>菜单：Tools → Aesir → Architecture → Debugger (UI Toolkit)。</para>
    /// </remarks>
    public sealed class ToolkitContextDebuggerWindow : EditorWindow
    {
        static readonly Color BadgeGreen = new Color(0.3f, 0.85f, 0.4f);
        static readonly Color BadgeGray = new Color(0.55f, 0.55f, 0.55f);
        static readonly Color CardBg = new Color(0.22f, 0.22f, 0.22f, 1f);
        static readonly Color CardBorder = new Color(0.35f, 0.35f, 0.35f, 1f);
        static readonly Color Accent = new Color(0.24f, 0.48f, 0.9f);

        List<ContextRegistryScanner.Entry> _contexts = new List<ContextRegistryScanner.Entry>();
        int _selectedIndex = -1;
        bool _showInternalFields;

        ListView _contextList;
        VisualElement _detailPane;
        Label _statusLabel;

        [MenuItem("Tools/Aesir/Architecture/Debugger (UI Toolkit)")]
        static void Open()
        {
            var window = GetWindow<ToolkitContextDebuggerWindow>("Context Debugger (UI Toolkit)");
            window.minSize = new Vector2(600, 340);
            window.Show();
        }

        void OnEnable()
        {
            BuildUI();
            Refresh();
        }

        void BuildUI()
        {
            var root = rootVisualElement;
            root.Clear();
            root.style.flexDirection = FlexDirection.Column;

            // ── 工具栏 ──
            var toolbar = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    paddingLeft = 8, paddingRight = 8, paddingTop = 4, paddingBottom = 4,
                    backgroundColor = new Color(0.18f, 0.18f, 0.18f),
                    borderBottomWidth = 1,
                    borderBottomColor = CardBorder,
                }
            };
            var refreshBtn = new Button(Refresh) { text = "刷新" };
            refreshBtn.style.marginRight = 8;
            toolbar.Add(refreshBtn);

            var internalToggle = new Toggle("显示内部字段") { value = _showInternalFields };
            internalToggle.RegisterValueChangedCallback(e =>
            {
                _showInternalFields = e.newValue;
                RebuildDetail();
            });
            toolbar.Add(internalToggle);

            var spacer = new VisualElement { style = { flexGrow = 1 } };
            toolbar.Add(spacer);
            toolbar.Add(new Label("UI Toolkit 版") { style = { fontSize = 10, color = BadgeGray } });
            root.Add(toolbar);

            // ── 主体：左列表 + 右详情 ──
            var body = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1 } };

            // 左列
            var leftPane = new VisualElement
            {
                style =
                {
                    width = 220,
                    borderRightWidth = 1,
                    borderRightColor = CardBorder,
                    backgroundColor = new Color(0.16f, 0.16f, 0.16f),
                }
            };
            _contextList = new ListView
            {
                style = { flexGrow = 1 },
                selectionType = SelectionType.Single,
                makeItem = MakeContextItem,
                bindItem = BindContextItem,
            };
            _contextList.selectionChanged += OnContextSelected;
            leftPane.Add(_contextList);
            body.Add(leftPane);

            // 右列
            _detailPane = new ScrollView { style = { flexGrow = 1, paddingLeft = 10, paddingRight = 10, paddingTop = 8 } };
            body.Add(_detailPane);
            root.Add(body);
        }

        VisualElement MakeContextItem()
        {
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    paddingLeft = 8, paddingTop = 5, paddingBottom = 5,
                }
            };
            var badge = new Label("●") { name = "badge", style = { fontSize = 11, marginRight = 6, width = 14 } };
            var name = new Label { name = "name", style = { flexGrow = 1 } };
            row.Add(badge);
            row.Add(name);
            return row;
        }

        void BindContextItem(VisualElement element, int index)
        {
            var ctx = _contexts[index];
            var badge = element.Q<Label>("badge");
            var name = element.Q<Label>("name");
            badge.text = "●";
            badge.style.color = ctx.Initialized ? BadgeGreen : BadgeGray;
            name.text = ctx.DisplayName;
            element.style.backgroundColor = index == _selectedIndex
                ? new Color(Accent.r, Accent.g, Accent.b, 0.3f)
                : Color.clear;
        }

        void OnContextSelected(IEnumerable<object> selection)
        {
            var selected = selection.FirstOrDefault() as ContextRegistryScanner.Entry;
            _selectedIndex = selected == null ? -1 : _contexts.IndexOf(selected);
            _contextList.RefreshItems();
            RebuildDetail();
        }

        void Refresh()
        {
            _contexts = ContextRegistryScanner.Scan();
            _contextList.itemsSource = _contexts;
            _contextList.RefreshItems();
            RebuildDetail();
        }

        void RebuildDetail()
        {
            _detailPane.Clear();

            if (_selectedIndex < 0 || _selectedIndex >= _contexts.Count)
            {
                _detailPane.Add(new Label("请在左侧选择一个 Context")
                {
                    style = { color = BadgeGray, marginTop = 12, unityFontStyleAndWeight = FontStyle.Italic }
                });
                return;
            }

            var ctx = _contexts[_selectedIndex];

            // Context 卡片
            var header = MakeCard();
            var titleRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            titleRow.Add(new Label(ctx.DisplayName) { style = { fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1 } });
            var badge = new Label(ctx.Initialized ? "● 已初始化" : "○ 未初始化")
            {
                style = { color = ctx.Initialized ? BadgeGreen : BadgeGray, fontSize = 11 }
            };
            titleRow.Add(badge);
            header.Add(titleRow);

            if (!ctx.Initialized)
            {
                var initBtn = new Button(() =>
                {
                    ContextRegistryScanner.EnsureInitialized(ctx.ContextType);
                    Refresh();
                })
                { text = "初始化", style = { marginTop = 8, alignSelf = Align.FlexStart } };
                header.Add(initBtn);
            }

            _detailPane.Add(header);

            if (!ctx.Initialized || ctx.Instance == null)
            {
                return;
            }

            _detailPane.Add(BuildModuleSection("Models", ctx.Instance.GetAllModels().Cast<object>().ToList()));
            _detailPane.Add(BuildModuleSection("Services", ctx.Instance.GetAllServices().Cast<object>().ToList()));
        }

        VisualElement BuildModuleSection(string title, List<object> modules)
        {
            var section = new VisualElement { style = { marginTop = 10 } };
            section.Add(new Label($"{title} ({modules.Count})")
            {
                style = { fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 4 }
            });

            foreach (var module in modules)
            {
                var card = MakeCard();
                var moduleName = module.GetType().Name;
                var foldout = new Foldout { text = moduleName, value = false };
                foldout.style.marginBottom = 2;

                var fields = ModuleFieldReader.ReadFields(module, _showInternalFields);
                if (fields.Count == 0)
                {
                    foldout.Add(new Label("（无可展示字段）")
                    {
                        style = { color = BadgeGray, fontSize = 10, marginLeft = 8 }
                    });
                }

                foreach (var field in fields)
                {
                    foldout.Add(BuildFieldRow(field));
                }

                card.Add(foldout);
                section.Add(card);
            }

            return section;
        }

        VisualElement BuildFieldRow(ModuleFieldReader.FieldEntry field)
        {
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginLeft = 8, marginTop = 2, marginBottom = 2,
                }
            };
            row.Add(new Label(field.DisplayName) { style = { width = 160, fontSize = 11 } });

            var valueType = ModuleFieldReader.GetEditableValueType(field);
            var current = field.ReadValue();
            var canEdit = Application.isPlaying;

            VisualElement editor = null;
            if (valueType == typeof(int))
            {
                var f = new IntegerField { value = (int)(current ?? 0), style = { flexGrow = 1 } };
                f.SetEnabled(canEdit);
                f.RegisterValueChangedCallback(e => field.WriteValue(e.newValue));
                editor = f;
            }
            else if (valueType == typeof(float))
            {
                var f = new FloatField { value = (float)(current ?? 0f), style = { flexGrow = 1 } };
                f.SetEnabled(canEdit);
                f.RegisterValueChangedCallback(e => field.WriteValue(e.newValue));
                editor = f;
            }
            else if (valueType == typeof(string))
            {
                var f = new TextField { value = (string)(current ?? string.Empty), style = { flexGrow = 1 } };
                f.SetEnabled(canEdit);
                f.RegisterValueChangedCallback(e => field.WriteValue(e.newValue));
                editor = f;
            }
            else if (valueType == typeof(bool))
            {
                var f = new Toggle { value = (bool)(current ?? false), style = { flexGrow = 1 } };
                f.SetEnabled(canEdit);
                f.RegisterValueChangedCallback(e => field.WriteValue(e.newValue));
                editor = f;
            }
            else if (valueType == typeof(Vector2))
            {
                var f = new Vector2Field { value = (Vector2)(current ?? Vector2.zero), style = { flexGrow = 1 } };
                f.SetEnabled(canEdit);
                f.RegisterValueChangedCallback(e => field.WriteValue(e.newValue));
                editor = f;
            }
            else if (valueType == typeof(Vector3))
            {
                var f = new Vector3Field { value = (Vector3)(current ?? Vector3.zero), style = { flexGrow = 1 } };
                f.SetEnabled(canEdit);
                f.RegisterValueChangedCallback(e => field.WriteValue(e.newValue));
                editor = f;
            }
            else if (valueType.IsEnum)
            {
                var f = new EnumField((System.Enum)(current ?? System.Enum.GetValues(valueType).GetValue(0)))
                {
                    style = { flexGrow = 1 }
                };
                f.SetEnabled(canEdit);
                f.RegisterValueChangedCallback(e => field.WriteValue(e.newValue));
                editor = f;
            }
            else
            {
                editor = new Label(current?.ToString() ?? "null")
                {
                    style = { color = BadgeGray, fontSize = 10, flexGrow = 1 }
                };
            }

            row.Add(editor);
            return row;
        }

        static VisualElement MakeCard()
        {
            return new VisualElement
            {
                style =
                {
                    backgroundColor = CardBg,
                    borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1,
                    borderTopColor = CardBorder, borderBottomColor = CardBorder,
                    borderLeftColor = CardBorder, borderRightColor = CardBorder,
                    borderTopLeftRadius = 6, borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6, borderBottomRightRadius = 6,
                    paddingLeft = 10, paddingRight = 10, paddingTop = 8, paddingBottom = 8,
                    marginBottom = 6,
                }
            };
        }
    }
}
