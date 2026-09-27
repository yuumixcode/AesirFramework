using UnityEditor;
using UnityEngine;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// Scene 模块设置窗口（原生 IMGUI 兜底）：展示并编辑 <see cref="SceneEditorSettings" /> 单例。
    /// </summary>
    /// <remarks>
    /// 双窗口模式的兜底侧（与包内更新器同款模式）：安装 Odin Inspector 时，本窗口持有的菜单项经
    /// <see cref="OdinWindowOpener" /> 静态委托路由到 <see cref="SceneModuleSettingsWindowOdin" />
    /// （Odin 程序集在域加载期注册打开方式）；未安装 Odin 时菜单直接打开本窗口——展示的信息量与
    /// Odin 版等价（四个设置字段 + 手动搜集按钮）。数据层 <see cref="SceneEditorSettings" />
    /// 为两窗口共用的单一真源，写入即时 <c>Save</c> 落盘。
    /// </remarks>
    public class SceneModuleSettingsWindow : EditorWindow
    {
        // 归入 Tools/Aesir/Modules/ 子菜单（Aesir Modules 包专属工具）；默认 priority 1000，
        // 组内位于 Script Doc Generator（999）之后，差值 ≤ 10 不产生分割线
        const string MenuPath = "Tools/Aesir/Modules/Scene Module Settings";

        /// <summary>
        /// Odin 版窗口的打开委托（由 Odin 程序集经 [InitializeOnLoadMethod] 注册；
        /// 未安装 Odin Inspector 时为 null，菜单打开本原生兜底窗口）。
        /// </summary>
        public static System.Action OdinWindowOpener { get; private set; }

        /// <summary>注册 Odin 版窗口的打开方式（域重载清空静态委托后由 Odin 程序集重新注册）。</summary>
        public static void RegisterOdinWindowOpener(System.Action opener) => OdinWindowOpener = opener;

        [MenuItem(MenuPath)]
        static void Open()
        {
            if (OdinWindowOpener != null)
            {
                OdinWindowOpener();
                return;
            }

            var window = GetWindow<SceneModuleSettingsWindow>();
            window.titleContent = new GUIContent("Scene Module Settings");
            window.minSize = new Vector2(500f, 300f);
            window.Show();
        }

        void OnGUI()
        {
            var settings = SceneEditorSettings.instance;

            // 对齐 Odin 版 LabelWidth(300)：标签占 300 宽，长文案不折行
            var prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 300f;

            EditorGUILayout.BeginVertical(EditorStyles.inspectorFullWidthMargins);

            EditorGUI.BeginChangeCheck();
            var setupBootstrapper = EditorGUILayout.Toggle(
                new GUIContent("是否自动搜集项目中的 Bootstrapper 场景并注册"),
                settings.SetupBootstrapper);
            if (EditorGUI.EndChangeCheck())
            {
                settings.SetupBootstrapper = setupBootstrapper;
            }

            EditorGUI.BeginChangeCheck();
            var firstLoad = EditorGUILayout.Toggle(
                new GUIContent("是否强制优先加载 Bootstrapper 场景"),
                settings.FirstLoadBootstrapScene);
            if (EditorGUI.EndChangeCheck())
            {
                settings.FirstLoadBootstrapScene = firstLoad;
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Bootstrapper 场景", EditorStyles.miniBoldLabel);
            EditorGUILayout.SelectableLabel(NullOrEmpty(settings.BootstrapperScenePath));

            EditorGUILayout.LabelField("上一个场景", EditorStyles.miniBoldLabel);
            EditorGUILayout.SelectableLabel(NullOrEmpty(settings.PreviousScenePath));

            EditorGUILayout.Space();

            if (GUILayout.Button("手动搜集 Bootstrapper 场景并注册"))
            {
                BootstrapSceneHelper.SetupBootstrapScene();
            }

            EditorGUILayout.EndVertical();

            EditorGUIUtility.labelWidth = prevLabelWidth;
        }

        static string NullOrEmpty(string path) =>
            string.IsNullOrEmpty(path) ? "（未设置）" : path;
    }
}
