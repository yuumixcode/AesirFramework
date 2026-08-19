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
    /// <para><b>核心实现</b>：为每个已初始化的 Model/Service 显式创建 <see cref="PropertyTree"/> 并 <c>tree.Draw()</c>——
    /// 这样 Odin 的完整序列化协议（含 <see cref="ObservableValueAttributeProcessor{T}"/> 的 InlineProperty 内联 +
    /// OnValueChanged 调 InvokeEvent）才会生效：ObservableValue 的值内联显示、可拖拽编辑、实时触发通知链。</para>
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

        List<ContextRegistryScanner.Entry> _initializedContexts = new List<ContextRegistryScanner.Entry>();
        int _selectedIndex = -1;
        Vector2 _scroll;

        // 每个模块实例一棵 PropertyTree（key = 模块实例引用）
        readonly Dictionary<object, PropertyTree> _trees = new Dictionary<object, PropertyTree>();
        readonly Dictionary<object, bool> _foldouts = new Dictionary<object, bool>();

        protected override void OnEnable()
        {
            base.OnEnable();
            Refresh();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            DisposeAllTrees();
        }

        void DisposeAllTrees()
        {
            foreach (var tree in _trees.Values)
            {
                tree?.Dispose();
            }

            _trees.Clear();
        }

        void Refresh()
        {
            // 仅保留已初始化的 Context（未初始化不显示、不操作）
            _initializedContexts = ContextRegistryScanner.Scan()
                .Where(c => c.Initialized)
                .ToList();

            // 当前选中项若已不再初始化，清空选择
            if (_selectedIndex >= _initializedContexts.Count)
            {
                _selectedIndex = -1;
            }

            // 清理已失效模块的 Tree（Context 重建后旧实例已 Dispose）
            var aliveModules = new HashSet<object>();
            foreach (var ctx in _initializedContexts)
            {
                if (ctx.Instance == null)
                {
                    continue;
                }

                foreach (var m in ctx.Instance.GetAllModels())
                {
                    aliveModules.Add(m);
                }

                foreach (var s in ctx.Instance.GetAllServices())
                {
                    aliveModules.Add(s);
                }
            }

            var deadKeys = _trees.Keys.Where(k => !aliveModules.Contains(k)).ToList();
            foreach (var key in deadKeys)
            {
                _trees[key]?.Dispose();
                _trees.Remove(key);
                _foldouts.Remove(key);
            }
        }

        protected override void OnImGUI()
        {
            DrawToolbar();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            if (_initializedContexts.Count == 0)
            {
                EditorGUILayout.HelpBox("当前没有已初始化的 Context。\n请进入 Play 模式，或在场景中放置框架根物体后刷新。",
                    MessageType.Info);
            }
            else
            {
                DrawContextSelector();
                EditorGUILayout.Space(6);
                DrawSelectedContext();
            }

            EditorGUILayout.EndScrollView();
        }

        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                Refresh();
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label($"已初始化 Context: {_initializedContexts.Count}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        void DrawContextSelector()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Context", GUILayout.Width(60));

            var names = _initializedContexts.Select(c => c.DisplayName).ToArray();
            var newIndex = EditorGUILayout.Popup(_selectedIndex, names);
            if (newIndex != _selectedIndex)
            {
                _selectedIndex = newIndex;
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawSelectedContext()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _initializedContexts.Count)
            {
                EditorGUILayout.HelpBox("请在上方选择一个已初始化的 Context", MessageType.None);
                return;
            }

            var ctx = _initializedContexts[_selectedIndex];
            if (ctx.Instance == null)
            {
                return;
            }

            DrawModules("Models", ctx.Instance.GetAllModels());
            EditorGUILayout.Space(8);
            DrawModules("Services", ctx.Instance.GetAllServices());
        }

        void DrawModules(string title, System.Collections.Generic.IEnumerable<object> modules)
        {
            var list = modules.ToList();
            EditorGUILayout.LabelField($"{title} ({list.Count})", EditorStyles.boldLabel);

            if (list.Count == 0)
            {
                EditorGUILayout.LabelField("  （空）", EditorStyles.miniLabel);
                return;
            }

            foreach (var module in list)
            {
                DrawModule(module);
            }
        }

        void DrawModule(object module)
        {
            if (module == null)
            {
                return;
            }

            if (!_foldouts.ContainsKey(module))
            {
                _foldouts[module] = true;
            }

            EditorGUILayout.BeginVertical("Box");
            _foldouts[module] = EditorGUILayout.Foldout(_foldouts[module], module.GetType().Name, true,
                EditorStyles.foldoutHeader);

            if (_foldouts[module])
            {
                EditorGUI.indentLevel++;

                // 核心：为模块实例显式创建 PropertyTree 并 Draw()，
                // 让 Odin 完整序列化协议（含 ObservableValueAttributeProcessor 的内联 + OnValueChanged）生效
                if (!_trees.TryGetValue(module, out var tree) || tree == null)
                {
                    tree = PropertyTree.Create(module);
                    _trees[module] = tree;
                }

                // 每帧强制同步目标值（Odin 会轮询 ObservableValue 的 Value getter）
                tree.UpdateTree();
                tree.Draw(true);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }
    }
}
#endif
