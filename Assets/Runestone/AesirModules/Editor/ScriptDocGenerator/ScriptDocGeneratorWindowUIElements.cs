using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// Script Doc Generator 的 UI Toolkit 窗口（无 Odin 编译依赖）：替代 Odin 版的 PropertyTree 绘制。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Odin 版窗口与 <c>ScriptDocGeneratorPanelSO</c>（继承 SerializedScriptableObject）都在
    ///     <c>Runestone.AesirModules.Editor.OdinInspector</c> 程序集，有 Odin 时才编译。本窗口反射访问它们，
    ///     用 SerializedObject + PropertyField 递归绘制同一份面板数据，零 Odin 编译依赖。
    ///     </para>
    ///     <para>
    ///     为保证与 Odin 版菜单不冲突，本窗口使用独立菜单路径；两者可共存，用户按需打开。
    ///     无 Odin 时面板类型不存在，给出明确提示（SDG 是编辑器工具，不影响运行时功能）。
    ///     </para>
    /// </remarks>
    internal sealed class ScriptDocGeneratorWindowUIElements : EditorWindow
    {
        const string MenuPath = "Tools/Aesir/Modules/Script Doc Generator (UI Toolkit)";
        const string PanelTypeName = "Runestone.AesirModules.ScriptDocGenerator.Editor.ScriptDocGeneratorPanelSO";
        const string UtilityTypeName = "Runestone.AesirModules.ScriptDocGenerator.Editor.ScriptDocGeneratorUtility";

        Type _panelType;
        Object _panel;
        SerializedObject _serialized;

        [MenuItem(MenuPath, false, 1004)]
        static void Open()
        {
            var window = GetWindow<ScriptDocGeneratorWindowUIElements>("Script Doc Generator");
            window.minSize = new Vector2(900, 700);
            window.Show();
        }

        void OnEnable()
        {
            _panelType = FindType(PanelTypeName);
            Rebuild();
        }

        void Rebuild()
        {
            var root = rootVisualElement;
            root.Clear();
            root.style.paddingLeft = 10f;
            root.style.paddingRight = 10f;
            root.style.paddingTop = 8f;

            var title = new Label("Script Doc Generator (UI Toolkit)");
            title.style.fontSize = 14f;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.paddingBottom = 6f;
            root.Add(title);

            if (_panelType == null)
            {
                root.Add(new HelpBox(
                    "未找到 ScriptDocGeneratorPanelSO 类型——它随 Odin Inspector 一起提供。" +
                    "当前工程未安装 Odin，脚本文档生成器不可用；这不影响框架运行时功能。",
                    HelpBoxMessageType.Warning));
                return;
            }

            // 调用 EnsureInitialized()（反射），失败则中止
            var utilityType = FindType(UtilityTypeName);
            if (utilityType != null)
            {
                var ensure = utilityType.GetMethod("EnsureInitialized",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                var ok = ensure != null && ensure.Invoke(null, null) is bool result && result;
                if (!ok)
                {
                    root.Add(new HelpBox("文档生成器初始化失败，请查看 Console。", HelpBoxMessageType.Error));
                    return;
                }
            }

            _panel = GetPanelInstance();
            if (_panel == null)
            {
                root.Add(new HelpBox("无法获取 ScriptDocGeneratorPanelSO 实例。", HelpBoxMessageType.Error));
                return;
            }

            _serialized = new SerializedObject(_panel);
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1f;
            root.Add(scroll);

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
                scroll.Add(field);
            }
        }

        static Type FindType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        Object GetPanelInstance()
        {
            var instanceProp = _panelType.GetProperty("Instance",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            return instanceProp?.GetValue(null) as Object;
        }
    }
}
