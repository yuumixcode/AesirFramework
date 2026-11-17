using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// Aesir Getting Started 窗口（UI Toolkit 版）— 框架示例导航：列出本机安装的 Aesir 包及其示例，
    /// 按教学分组展示，一键打开示例场景或定位示例目录。
    /// <para>
    /// 安装 Odin Inspector 且样式开关 <see cref="AesirInspectorStyleSettings.UseOdin" /> 为 true 时，
    /// 菜单经 <see cref="OdinWindowOpener" /> 路由到 Odin 版窗口（AesirGetStartedWindowOdin）；
    /// 否则打开本窗口。外观由 <c>AesirGetStarted.uss</c> 统一控制，复刻 Odin 深色卡片风格。
    /// </para>
    /// </summary>
    public class AesirGetStartedWindow : EditorWindow
    {
        #region 菜单与 Odin 窗口路由

        const string MenuPath = "Tools/Aesir/Getting Started";
        const string WindowTitle = "Aesir Getting Started";
        const string StyleSheetPath =
            "Assets/Runestone/AesirArchitecture/Editor/GetStarted/AesirGetStarted.uss";

        /// <summary>
        /// Odin 版窗口的打开委托（由 Odin 程序集经 [InitializeOnLoadMethod] 注册；
        /// 未安装 Odin Inspector 或样式开关关闭时为 null，菜单打开本 UI Toolkit 窗口）。
        /// </summary>
        public static Action OdinWindowOpener { get; private set; }

        /// <summary>注册 Odin 版窗口的打开方式（域重载清空静态委托后由 Odin 程序集重新注册）。</summary>
        public static void RegisterOdinWindowOpener(Action opener) => OdinWindowOpener = opener;

        // priority -980：菜单排序键（越小越靠上）。父菜单 priority 由子项最小值决定，本项即
        // Tools/Aesir 组的最小值——-980 大于 Tools/Odin 组的 -1000，故 Aesir 组稳定排在 Odin 组之后；
        // 与组内次小 priority 差值超过 10，Unity 自动在其间插入独立分割线（置顶 + 独立分割线）
        [MenuItem(MenuPath, false, -980)]
        static void Open()
        {
            if (AesirInspectorStyleSettings.UseOdin && OdinWindowOpener != null)
            {
                OdinWindowOpener();
                return;
            }

            var window = GetWindow<AesirGetStartedWindow>(WindowTitle);
            window.minSize = new Vector2(520, 480);
            window.Show();
        }

        #endregion

        #region 状态

        List<AesirGetStartedService.AesirPackageInfo> _packages = new();
        ScrollView _scroll;

        #endregion

        #region 生命周期

        void OnEnable() => BuildUI();

        void BuildUI()
        {
            var root = rootVisualElement;
            root.Clear();
            root.AddToClassList("aesir-root");

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }

            root.Add(BuildToolbar());
            _scroll = new ScrollView(ScrollViewMode.Vertical);
            _scroll.style.flexGrow = 1f;
            root.Add(_scroll);

            Scan();
        }

        VisualElement BuildToolbar()
        {
            var toolbar = new VisualElement();
            toolbar.AddToClassList("aesir-toolbar");

            var title = new Label("Aesir Framework — 示例导航");
            title.AddToClassList("aesir-toolbar-title");
            toolbar.Add(title);

            toolbar.Add(new Button(() => Application.OpenURL(AesirUpdateService.ReleasesPageUrl))
            {
                text = "Releases"
            }.Also(b => b.AddToClassList("aesir-button")));

            toolbar.Add(new Button(Scan) { text = "刷新" }
                .Also(b => b.AddToClassList("aesir-button")));

            return toolbar;
        }

        /// <summary>扫描包与示例并重建 UI（刷新 / 导入 Sample 后调用）。</summary>
        void Scan()
        {
            _packages = AesirGetStartedService.ScanPackages();
            if (_scroll == null)
            {
                return;
            }

            var scrollOffset = _scroll.scrollOffset;
            _scroll.Clear();

            if (_packages.Count == 0)
            {
                var box = new HelpBox(
                    "未扫描到 Aesir 包。请经 GitHub Releases 导入 unitypackage" +
                    "（默认安装到 Assets/Runestone，可自由移动到项目任意文件夹），" +
                    "或经 Package Manager 用 Git URL 安装后从 Samples 标签页导入示例。",
                    HelpBoxMessageType.Warning);
                _scroll.Add(box);
                _scroll.Add(new Button(() => Application.OpenURL(AesirUpdateService.ReleasesPageUrl))
                {
                    text = "打开 GitHub Releases 页面"
                }.Also(b => b.AddToClassList("aesir-button")));
                return;
            }

            foreach (var pkg in _packages)
            {
                _scroll.Add(BuildPackage(pkg));
            }

            _scroll.scrollOffset = scrollOffset;
        }

        #endregion

        #region UI

        VisualElement BuildPackage(AesirGetStartedService.AesirPackageInfo pkg)
        {
            var container = new VisualElement();
            container.AddToClassList("aesir-package");

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;

            var name = new Label(pkg.DisplayName);
            name.AddToClassList("aesir-package-header");
            name.style.flexGrow = 1f;
            header.Add(name);

            var installNote = pkg.InstallType == AesirGetStartedService.AesirInstallType.AssetsCopy
                ? $"v{pkg.Version} · {pkg.Samples.Count} 示例 · Assets 安装"
                : $"v{pkg.Version} · {pkg.Samples.Count} 示例 · UPM 安装";
            var meta = new Label(installNote);
            meta.AddToClassList("aesir-package-meta");
            header.Add(meta);
            container.Add(header);

            foreach (var group in AesirGetStartedService.GroupSamples(pkg))
            {
                var groupTitle = new Label($"{group.Title}    {group.Samples.Count} 个示例");
                groupTitle.AddToClassList("aesir-group-title");
                container.Add(groupTitle);

                foreach (var sample in group.Samples)
                {
                    container.Add(BuildSample(sample));
                }
            }

            return container;
        }

        VisualElement BuildSample(AesirGetStartedService.AesirSampleInfo sample)
        {
            var card = new VisualElement();
            card.AddToClassList("aesir-sample");

            var icon = new Label(GetSampleIcon(sample));
            icon.AddToClassList("aesir-sample-icon");
            card.Add(icon);

            var info = new VisualElement();
            info.AddToClassList("aesir-sample-info");

            var title = new Label(sample.DisplayName);
            title.AddToClassList("aesir-sample-title");
            info.Add(title);

            if (!string.IsNullOrEmpty(sample.Description))
            {
                var desc = new Label(sample.Description);
                desc.AddToClassList("aesir-sample-desc");
                info.Add(desc);
            }

            var badge = AesirGetStartedService.GetSampleBadge(sample);
            if (!string.IsNullOrEmpty(badge))
            {
                var badgeLabel = new Label(badge);
                badgeLabel.AddToClassList("aesir-badge");
                info.Add(badgeLabel);
            }

            card.Add(info);

            var actions = new VisualElement();
            actions.AddToClassList("aesir-sample-actions");
            if (sample.IsImported && sample.HasScene)
            {
                actions.Add(new Button(() => OpenScene(sample)) { text = "打开场景" }
                    .Also(b => b.AddToClassList("aesir-button")));
                actions.Add(new Button(() => Ping(sample)) { text = "定位" }
                    .Also(b => b.AddToClassList("aesir-button")));
            }
            else if (sample.IsImported)
            {
                var hint = new Label("无独立场景");
                hint.AddToClassList("aesir-empty");
                actions.Add(hint);
                actions.Add(new Button(() => Ping(sample)) { text = "定位" }
                    .Also(b => b.AddToClassList("aesir-button")));
            }
            else
            {
                actions.Add(new Button(() => Import(sample)) { text = "导入 Sample" }
                    .Also(b => b.AddToClassList("aesir-button")));
            }

            card.Add(actions);
            return card;
        }

        /// <summary>示例图标：按分组 / 目录映射为 Unicode 符号（近似 Odin 的 SDF 图标，避免引入图标资产）。</summary>
        static string GetSampleIcon(AesirGetStartedService.AesirSampleInfo sample)
        {
            switch (AesirGetStartedService.GetSampleGroup(sample))
            {
                case "MVC 计数器 · 渐进式": return "▦";
                case "MVP 计数器 · 渐进式": return "☰";
                case "事件模块": return "◈";
                case "音频模块": return "♪";
            }

            var dir = sample.RelativeDir ?? string.Empty;
            if (dir.StartsWith("MiniEvent", StringComparison.Ordinal)) return "⚡";
            if (dir.StartsWith("ObservableCollections", StringComparison.Ordinal)) return "▤";
            if (dir.StartsWith("ObservableValue", StringComparison.Ordinal)) return "◉";
            if (dir.StartsWith("RuntimeInitializeLoadType", StringComparison.Ordinal)) return "◷";
            return "★";
        }

        static void OpenScene(AesirGetStartedService.AesirSampleInfo sample)
        {
            if (AesirGetStartedService.OpenSampleScene(sample))
            {
                AesirArchitectureDebug.Log("AesirGetStarted",
                    AesirGetStartedService.BuildOpenSceneToastMessage(sample));
            }
        }

        static void Ping(AesirGetStartedService.AesirSampleInfo sample)
        {
            if (AesirGetStartedService.PingSample(sample))
            {
                AesirArchitectureDebug.Log("AesirGetStarted",
                    AesirGetStartedService.BuildPingToastMessage(sample));
            }
        }

        void Import(AesirGetStartedService.AesirSampleInfo sample)
        {
            var result = AesirGetStartedService.ConfirmAndImportUpmSample(sample, out var message);
            switch (result)
            {
                case AesirGetStartedService.AesirSampleImportResult.Imported:
                    Scan();
                    AesirArchitectureDebug.Log("AesirGetStarted", message);
                    break;
                case AesirGetStartedService.AesirSampleImportResult.Cancelled:
                    break;
                default:
                    EditorUtility.DisplayDialog(AesirGetStartedService.ImportConfirmTitle, message, "确定");
                    break;
            }
        }

        #endregion
    }

    static class AesirVisualElementExtensions
    {
        internal static T Also<T>(this T element, Action<T> action)
        {
            action(element);
            return element;
        }
    }
}
