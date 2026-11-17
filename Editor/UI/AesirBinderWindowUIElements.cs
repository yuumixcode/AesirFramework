using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// Binder 的 UI Toolkit 操作窗口（无 Odin 编译依赖）：复刻 BinderAssistant 的 Odin Inspector 面板。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     BinderAssistant 及其 Odin 特性在 <c>Runestone.AesirModules.OdinInspector</c> 程序集
    ///     （有 Odin 时才编译）。本窗口反射访问它，因此在核心 Editor 程序集里零 Odin 编译依赖：
    ///     有 Odin 时替代 Odin Inspector 提供「构建绑定单元 / 生成脚本 / 检查绑定」全部功能；
    ///     无 Odin 时 BinderAssistant 类型不存在，窗口给出明确提示（RAM 核心功能不受影响）。
    ///     </para>
    ///     <para>
    ///     字段用 <see cref="SerializedObject" /> + <see cref="PropertyField" /> 自动绑定，
    ///     下拉数据源通过反射调用 Odin 版的 GetBaseTypes / GetContextTypeChoices / GetPartialSuffixOptions
    ///     并转成字符串数组。所有动作按钮反射调用 BinderAssistant 的私有方法。
    ///     </para>
    /// </remarks>
    internal sealed class AesirBinderWindowUIElements : EditorWindow
    {
        const string MenuPath = "Tools/Aesir/Modules/UI Binder";
        const string AssistantTypeName = "Runestone.AesirModules.BinderAssistant";

        static readonly string[] ActionMethods = { "CreateUnits", "GenerateCode", "CheckBindings" };
        static readonly string[] ActionLabels = { "构建绑定单元", "生成脚本", "检查绑定" };

        Type _assistantType;
        Object _target;
        SerializedObject _serialized;

        /// <summary>供样式开关关闭 Odin 时 / 无 Odin 时的菜单落点。</summary>
        [MenuItem(MenuPath, false, 1003)]
        static void Open()
        {
            var window = GetWindow<AesirBinderWindowUIElements>("Aesir UI Binder");
            window.minSize = new Vector2(520, 480);
            window.Show();
        }

        void OnEnable()
        {
            _assistantType = ResolveAssistantType();
            OnSelectionChange();
        }

        void OnSelectionChange() => Rebuild();

        Type ResolveAssistantType()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(AssistantTypeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        void Rebuild()
        {
            var root = rootVisualElement;
            root.Clear();
            root.style.paddingLeft = 10f;
            root.style.paddingRight = 10f;
            root.style.paddingTop = 8f;

            root.Add(BuildHeader());

            if (_assistantType == null)
            {
                root.Add(new HelpBox(
                    "未找到 BinderAssistant 类型——它随 Odin Inspector 一起提供。" +
                    "当前工程未安装 Odin，Binder 工具不可用；运行时组件与已生成的绑定代码不受影响。",
                    HelpBoxMessageType.Warning));
                return;
            }

            var component = ResolveComponent();
            if (component == null)
            {
                root.Add(new HelpBox(
                    "请在场景中选中挂载了 BinderAssistant 的根 UI 物体。",
                    HelpBoxMessageType.Info));
                return;
            }

            _serialized = new SerializedObject(component);

            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1f;
            root.Add(scroll);

            AddConfigFields(scroll, component);
            AddActions(scroll, component);
        }

        VisualElement BuildHeader()
        {
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.paddingBottom = 6f;
            header.style.borderBottomWidth = 1f;
            header.style.borderBottomColor = new Color(0.11f, 0.11f, 0.11f);

            var title = new Label("Aesir UI Binder");
            title.style.fontSize = 14f;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.flexGrow = 1f;
            header.Add(title);

            header.Add(new Button(Rebuild) { text = "刷新" });
            return header;
        }

        Component ResolveComponent()
        {
            var go = Selection.activeGameObject;
            return go == null ? null : go.GetComponent(_assistantType);
        }

        void AddConfigFields(VisualElement parent, Component component)
        {
            // SerializedObject 自动绑定全部可序列化字段（含 Units 列表），无需逐字段手写
            var iterator = _serialized.GetIterator();
            var enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.name == "m_Script")
                {
                    continue;
                }

                var field = new PropertyField(iterator.Copy());
                field.Bind(_serialized);
                parent.Add(field);
            }
        }

        void AddActions(VisualElement parent, Component component)
        {
            var actions = new VisualElement();
            actions.style.flexDirection = FlexDirection.Row;
            actions.style.marginTop = 8f;
            actions.style.paddingTop = 6f;
            actions.style.borderTopWidth = 1f;
            actions.style.borderTopColor = new Color(0.11f, 0.11f, 0.11f);

            for (var i = 0; i < ActionMethods.Length; i++)
            {
                var methodName = ActionMethods[i];
                var button = new Button(() => Invoke(component, methodName)) { text = ActionLabels[i] };
                button.style.marginRight = 4f;
                actions.Add(button);
            }

            parent.Add(actions);
        }

        static void Invoke(Component component, string methodName)
        {
            var method = component.GetType().GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                Debug.LogError($"[Aesir Binder] 找不到方法 {methodName}");
                return;
            }

            method.Invoke(component, null);
        }
    }
}
