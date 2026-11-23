using System;
using Runestone.AesirArchitecture.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// Scene 模块设置窗口（UI Toolkit 版）：展示并编辑 <see cref="SceneEditorSettings" /> 单例。
    /// </summary>
    /// <remarks>
    /// 安装 Odin Inspector 且样式开关 <see cref="AesirInspectorStyleSettings.UseOdin" /> 为 true 时，
    /// 菜单经 <see cref="OdinWindowOpener" /> 路由到 Odin 版窗口；否则打开本 UI Toolkit 窗口。
    /// 两版信息量等价（四个设置字段 + 手动搜集按钮），数据层 <see cref="SceneEditorSettings" />
    /// 为单一真源，写入即时 <c>Save</c> 落盘。
    /// </remarks>
    public class SceneModuleSettingsWindow : EditorWindow
    {
        // 归入 Tools/Aesir/Modules/ 子菜单（Aesir Modules 包专属工具）；默认 priority 1000，
        // 组内位于 Script Doc Generator（999）之后，差值 ≤ 10 不产生分割线
        const string MenuPath = "Tools/Aesir/Modules/Scene Module Settings";

        /// <summary>Odin 版窗口的打开委托（Odin 程序集注册；关闭 Odin 样式时为 null）。</summary>
        public static Action OdinWindowOpener { get; private set; }

        void OnEnable() => BuildUI();

        /// <summary>注册 Odin 版窗口的打开方式（域重载清空静态委托后由 Odin 程序集重新注册）。</summary>
        public static void RegisterOdinWindowOpener(Action opener) => OdinWindowOpener = opener;

        [MenuItem(MenuPath)]
        static void Open()
        {
            if (AesirInspectorStyleSettings.UseOdin && OdinWindowOpener != null)
            {
                OdinWindowOpener();
                return;
            }

            var window = GetWindow<SceneModuleSettingsWindow>();
            window.titleContent = new GUIContent("Scene Module Settings");
            window.minSize = new Vector2(500f, 300f);
            window.Show();
        }

        void BuildUI()
        {
            var settings = SceneEditorSettings.instance;
            var root = rootVisualElement;
            root.Clear();
            root.style.paddingLeft = 12f;
            root.style.paddingRight = 12f;
            root.style.paddingTop = 8f;

            var setup = new Toggle("是否自动搜集项目中的 Bootstrapper 场景并注册")
            {
                value = settings.SetupBootstrapper
            };
            setup.RegisterValueChangedCallback(evt => settings.SetupBootstrapper = evt.newValue);
            root.Add(setup);

            var firstLoad = new Toggle("是否强制优先加载 Bootstrapper 场景")
            {
                value = settings.FirstLoadBootstrapScene
            };
            firstLoad.RegisterValueChangedCallback(evt => settings.FirstLoadBootstrapScene = evt.newValue);
            root.Add(firstLoad);

            root.Add(BuildReadonlyField("Bootstrapper 场景", settings.BootstrapperScenePath));
            root.Add(BuildReadonlyField("上一个场景", settings.PreviousScenePath));

            root.Add(new Button(() => BootstrapSceneHelper.SetupBootstrapScene())
            {
                text = "手动搜集 Bootstrapper 场景并注册"
            });
        }

        static VisualElement BuildReadonlyField(string label, string value)
        {
            var row = new VisualElement();
            row.style.marginTop = 6f;

            var title = new Label(label);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            row.Add(title);

            var text = new Label(string.IsNullOrEmpty(value) ? "（未设置）" : value);
            text.style.whiteSpace = WhiteSpace.Normal;
            text.style.paddingLeft = 8f;
            row.Add(text);

            return row;
        }
    }
}
