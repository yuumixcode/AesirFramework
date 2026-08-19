using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// Context 调试窗口 —— IMGUI 版（V1）。
    /// </summary>
    /// <remarks>
    /// 三版试做之一：朴素可靠的基线版。样式用 EditorStyles + GUI.Box 做基础美化。
    /// <para><b>性能控制</b>：手动「刷新」按钮重扫，不每帧轮询；折叠节点不展开子树反射。</para>
    /// <para>菜单：Tools → Aesir → Architecture → Debugger (IMGUI)。</para>
    /// </remarks>
    public sealed class ImguiContextDebuggerWindow : EditorWindow
    {
        List<ContextRegistryScanner.Entry> _contexts = new List<ContextRegistryScanner.Entry>();
        int _selectedIndex = -1;
        Vector2 _contextScroll;
        Vector2 _moduleScroll;
        readonly Dictionary<string, bool> _foldouts = new Dictionary<string, bool>();
        bool _showInternalFields;

        [MenuItem("Tools/Aesir/Architecture/Debugger (IMGUI)")]
        static void Open()
        {
            var window = GetWindow<ImguiContextDebuggerWindow>("Context Debugger (IMGUI)");
            window.minSize = new Vector2(560, 320);
            window.Show();
        }

        void OnEnable()
        {
            Refresh();
        }

        void Refresh()
        {
            _contexts = ContextRegistryScanner.Scan();
            Repaint();
        }

        void OnGUI()
        {
            DrawToolbar();

            EditorGUILayout.BeginHorizontal();

            // 左列：Context 列表
            EditorGUILayout.BeginVertical(GUILayout.Width(220));
            _contextScroll = EditorGUILayout.BeginScrollView(_contextScroll);
            for (var i = 0; i < _contexts.Count; i++)
            {
                var ctx = _contexts[i];
                var style = i == _selectedIndex ? EditorStyles.whiteLabel : EditorStyles.label;
                var bg = i == _selectedIndex ? new Color(0.24f, 0.48f, 0.9f, 0.4f) : Color.clear;
                var rect = EditorGUILayout.BeginHorizontal();
                EditorGUI.DrawRect(rect, bg);
                var badge = ctx.Initialized ? "●" : "○";
                var badgeColor = ctx.Initialized ? new Color(0.3f, 0.85f, 0.4f) : Color.gray;
                var oldColor = GUI.color;
                GUI.color = badgeColor;
                GUILayout.Label(badge, GUILayout.Width(16));
                GUI.color = oldColor;
                if (GUILayout.Button(ctx.DisplayName, EditorStyles.label))
                {
                    _selectedIndex = i;
                    _foldouts.Clear();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            // 分隔线
            GUILayout.Box(string.Empty, GUILayout.Width(1), GUILayout.ExpandHeight(true));

            // 右列：模块详情
            EditorGUILayout.BeginVertical();
            _moduleScroll = EditorGUILayout.BeginScrollView(_moduleScroll);
            DrawSelectedContext();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                Refresh();
            }

            _showInternalFields = GUILayout.Toggle(_showInternalFields, "显示内部字段", EditorStyles.toolbarButton,
                GUILayout.Width(90));
            GUILayout.FlexibleSpace();
            GUILayout.Label("IMGUI 版", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        void DrawSelectedContext()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _contexts.Count)
            {
                EditorGUILayout.HelpBox("请在左侧选择一个 Context", MessageType.Info);
                return;
            }

            var ctx = _contexts[_selectedIndex];
            EditorGUILayout.Space(6);

            // Context 头
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(ctx.DisplayName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("状态", ctx.Initialized ? "已初始化" : "未初始化", EditorStyles.miniLabel);
            if (!ctx.Initialized && GUILayout.Button("初始化", GUILayout.Width(80)))
            {
                ContextRegistryScanner.EnsureInitialized(ctx.ContextType);
                Refresh();
                return;
            }

            EditorGUILayout.EndVertical();

            if (!ctx.Initialized || ctx.Instance == null)
            {
                return;
            }

            EditorGUILayout.Space(4);
            DrawModules("Models", ctx.Instance.GetAllModels());
            EditorGUILayout.Space(4);
            DrawModules("Services", ctx.Instance.GetAllServices());
        }

        void DrawModules(string title, System.Collections.Generic.IEnumerable<object> modules)
        {
            var list = modules.ToList();
            EditorGUILayout.LabelField($"{title} ({list.Count})", EditorStyles.boldLabel);

            foreach (var module in list)
            {
                var moduleName = module.GetType().Name;
                var key = $"{title}/{moduleName}";
                if (!_foldouts.ContainsKey(key))
                {
                    _foldouts[key] = false;
                }

                EditorGUILayout.BeginVertical("Box");
                _foldouts[key] = EditorGUILayout.Foldout(_foldouts[key], moduleName, true,
                    EditorStyles.foldoutHeader);

                if (_foldouts[key])
                {
                    EditorGUI.indentLevel++;
                    var fields = ModuleFieldReader.ReadFields(module, _showInternalFields);
                    if (fields.Count == 0)
                    {
                        EditorGUILayout.LabelField("（无可展示字段）", EditorStyles.miniLabel);
                    }

                    foreach (var field in fields)
                    {
                        DrawField(field);
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
        }

        void DrawField(ModuleFieldReader.FieldEntry field)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(field.DisplayName, GUILayout.Width(160));

            var valueType = ModuleFieldReader.GetEditableValueType(field);
            var current = field.ReadValue();
            object newValue = current;
            var canEdit = Application.isPlaying;

            EditorGUI.BeginDisabledGroup(!canEdit);
            if (valueType == typeof(int))
            {
                newValue = EditorGUILayout.IntField((int)(current ?? 0));
            }
            else if (valueType == typeof(float))
            {
                newValue = EditorGUILayout.FloatField((float)(current ?? 0f));
            }
            else if (valueType == typeof(string))
            {
                newValue = EditorGUILayout.TextField((string)(current ?? string.Empty));
            }
            else if (valueType == typeof(bool))
            {
                newValue = EditorGUILayout.Toggle((bool)(current ?? false));
            }
            else if (valueType == typeof(Vector2))
            {
                newValue = EditorGUILayout.Vector2Field(GUIContent.none, (Vector2)(current ?? Vector2.zero));
            }
            else if (valueType == typeof(Vector3))
            {
                newValue = EditorGUILayout.Vector3Field(GUIContent.none, (Vector3)(current ?? Vector3.zero));
            }
            else if (valueType.IsEnum)
            {
                newValue = EditorGUILayout.EnumPopup((System.Enum)(current ?? System.Enum.GetValues(valueType).GetValue(0)));
            }
            else
            {
                EditorGUILayout.LabelField(current?.ToString() ?? "null", EditorStyles.miniLabel);
            }

            EditorGUI.EndDisabledGroup();

            if (canEdit && !Equals(newValue, current))
            {
                field.WriteValue(newValue);
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
