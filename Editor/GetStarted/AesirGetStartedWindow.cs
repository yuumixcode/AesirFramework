using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// Aesir Getting Started 窗口（IMGUI 兜底版）— 框架示例导航：列出本机安装的 Aesir 包及其示例，
    /// 按教学分组展示，一键打开示例场景或定位示例目录。
    /// <para>
    /// 安装了 Odin Inspector 时，菜单入口经 <see cref="OdinWindowOpener" /> 路由到 Odin 版窗口
    /// （AesirGetStartedWindowOdin：页面栈导航、示例卡片与滑动动效）；未安装时本窗口为菜单落点。
    /// 数据层两版共用 <see cref="AesirGetStartedService" />，本类只做展示与动作接线。
    /// </para>
    /// <para>
    /// 扫描结果（包 + 分组视图模型）在 OnEnable / 手动刷新时重建，OnGUI 期间零磁盘 IO、零 LINQ。
    /// </para>
    /// </summary>
    public class AesirGetStartedWindow : EditorWindow
    {
        #region 菜单与 Odin 窗口路由

        const string MenuPath = "Tools/Aesir/Getting Started";

        /// <summary>
        /// Odin 版窗口的打开委托（由 Odin 程序集经 [InitializeOnLoadMethod] 注册；
        /// 未安装 Odin Inspector 时为 null，菜单打开本 IMGUI 兜底窗口）。
        /// </summary>
        public static Action OdinWindowOpener { get; private set; }

        /// <summary>注册 Odin 版窗口的打开方式（域重载清空静态委托后由 Odin 程序集重新注册）。</summary>
        public static void RegisterOdinWindowOpener(Action opener) => OdinWindowOpener = opener;

        // priority -1000：菜单排序键（越小越靠上），使本项居 Tools/Aesir 顶部；
        // 与后续菜单项（995 / 999 / 1000）差值超过 10，Unity 自动在其间插入独立分割线
        [MenuItem(MenuPath, false, -1000)]
        static void Open()
        {
            if (OdinWindowOpener != null)
            {
                OdinWindowOpener();
                return;
            }

            var window = GetWindow<AesirGetStartedWindow>("Aesir Getting Started Window");
            window.minSize = new Vector2(520, 480);
            window.Show();
        }

        #endregion

        #region 状态

        /// <summary>扫描产物（包 → 分组 → 示例 的视图模型；扫描时重建）。</summary>
        List<AesirGetStartedService.AesirPackageInfo> _packages =
            new List<AesirGetStartedService.AesirPackageInfo>();

        /// <summary>各包的分组视图（与 _packages 平行；避免 OnGUI 每帧重算分组）。</summary>
        readonly List<List<AesirGetStartedService.SampleGroup>> _groups =
            new List<List<AesirGetStartedService.SampleGroup>>();

        Vector2 _scrollPosition;

        #endregion

        #region 生命周期

        void OnEnable()
        {
            Scan();
        }

        /// <summary>扫描包与示例并预计算分组视图模型（OnGUI 期间只读消费）。</summary>
        void Scan()
        {
            _packages = AesirGetStartedService.ScanPackages();
            _groups.Clear();
            foreach (var pkg in _packages)
            {
                _groups.Add(AesirGetStartedService.GroupSamples(pkg));
            }

            Repaint();
        }

        #endregion

        #region UI

        void OnGUI()
        {
            DrawToolbar();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if (_packages.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "未扫描到 Aesir 包。请经 GitHub Releases 导入 unitypackage" +
                    "（默认安装到 Assets/Runestone，可自由移动到项目任意文件夹），" +
                    "或经 Package Manager 用 Git URL 安装后从 Samples 标签页导入示例。", MessageType.Warning);
                if (GUILayout.Button("打开 GitHub Releases 页面"))
                {
                    Application.OpenURL(AesirUpdateService.ReleasesPageUrl);
                }
            }
            else
            {
                for (var i = 0; i < _packages.Count; i++)
                    DrawPackage(_packages[i], _groups[i]);
            }

            EditorGUILayout.EndScrollView();
        }

        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Aesir Framework 示例导航 — 点击「打开场景」保存当前场景并进入示例；「定位」在 Project 窗口选中示例文件夹。",
                EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                Scan();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);
        }

        void DrawPackage(AesirGetStartedService.AesirPackageInfo pkg,
            List<AesirGetStartedService.SampleGroup> groups)
        {
            var installNote = pkg.InstallType == AesirGetStartedService.AesirInstallType.AssetsCopy
                ? "Assets 安装"
                : "UPM 安装 · 示例经 Package Manager → Samples 导入";

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(pkg.DisplayName, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"v{pkg.Version} · {pkg.Samples.Count} 个示例 · {installNote}",
                EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            foreach (var group in groups)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField(group.Title, EditorStyles.boldLabel);
                foreach (var sample in group.Samples)
                {
                    DrawSample(sample);
                }
            }

            EditorGUILayout.Space(10);
        }

        void DrawSample(AesirGetStartedService.AesirSampleInfo sample)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(sample.DisplayName, EditorStyles.boldLabel);
            if (!string.IsNullOrEmpty(sample.Description))
            {
                EditorGUILayout.LabelField(sample.Description, EditorStyles.wordWrappedMiniLabel);
            }

            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // 动作按钮：已导入且有场景 → 打开场景；其余形态降级为定位 / 引导导入
            if (sample.IsImported && sample.HasScene)
            {
                if (GUILayout.Button("打开场景", GUILayout.Width(80)))
                {
                    AesirGetStartedService.OpenSampleScene(sample);
                }

                if (GUILayout.Button("定位", GUILayout.Width(52)))
                {
                    AesirGetStartedService.PingSample(sample);
                }
            }
            else if (sample.IsImported)
            {
                EditorGUILayout.LabelField("代码示例 · 无独立场景", EditorStyles.miniLabel, GUILayout.Width(110));
                if (GUILayout.Button("定位", GUILayout.Width(52)))
                {
                    AesirGetStartedService.PingSample(sample);
                }
            }
            else
            {
                if (GUILayout.Button("去导入", GUILayout.Width(70)))
                {
                    AesirGetStartedService.OpenPackageManager();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        #endregion
    }
}
