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
    /// 菜单经 <see cref="OdinWindowOpener" /> 路由到 Odin 版窗口（AesirGetStartedWindowOdin：页面栈导航、
    /// 示例卡片与滑动动效）；否则打开本 UI Toolkit 窗口。数据层两版共用 <see cref="AesirGetStartedService" />。
    /// </para>
    /// </summary>
    public class AesirGetStartedWindow : EditorWindow
    {
        #region 菜单与 Odin 窗口路由

        const string MenuPath = "Tools/Aesir/Getting Started";
        const string WindowTitle = "Aesir Getting Started";

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

        /// <summary>扫描产物（包 → 分组 → 示例 的视图模型；扫描时重建）。</summary>
        List<AesirGetStartedService.AesirPackageInfo> _packages = new();
        ScrollView _scroll;

        #endregion

        #region 生命周期

        void OnEnable() => BuildUI();

        void BuildUI()
        {
            var root = rootVisualElement;
            root.Clear();

            root.Add(BuildToolbar());
            _scroll = new ScrollView(ScrollViewMode.Vertical);
            _scroll.style.flexGrow = 1f;
            root.Add(_scroll);

            Scan();
        }

        /// <summary>扫描包与示例并重建 UI（刷新 / 导入 Sample 后调用）。</summary>
        void Scan()
        {
            _packages = AesirGetStartedService.ScanPackages();
            if (_scroll == null)
            {
                return;
            }

            _scroll.Clear();
            if (_packages.Count == 0)
            {
                var box = new HelpBox(
                    "未扫描到 Aesir 包。请经 GitHub Releases 导入 unitypackage" +
                    "（默认安装到 Assets/Runestone，可自由移动到项目任意文件夹），" +
                    "或经 Package Manager 用 Git URL 安装后从 Samples 标签页导入示例。",
                    HelpBoxMessageType.Warning);
                var open = new Button(() => Application.OpenURL(AesirUpdateService.ReleasesPageUrl))
                {
                    text = "打开 GitHub Releases 页面"
                };
                _scroll.Add(box);
                _scroll.Add(open);
                return;
            }

            foreach (var pkg in _packages)
            {
                _scroll.Add(BuildPackage(pkg));
            }
        }

        #endregion

        #region UI

        VisualElement BuildToolbar()
        {
            var toolbar = new VisualElement();
            toolbar.AddToClassList("aesir-getstarted-toolbar");
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.alignItems = Align.Center;
            toolbar.style.paddingBottom = 6f;
            toolbar.style.borderBottomWidth = 1f;

            var hint = new Label(
                "Aesir Framework 示例导航 — 打开场景 / 定位 / 导入 Sample。" +
                "点击事件卡片上的按钮完成对应操作。");
            hint.style.flexGrow = 1f;
            hint.style.whiteSpace = WhiteSpace.Normal;
            toolbar.Add(hint);

            toolbar.Add(new Button(Scan) { text = "刷新" });
            return toolbar;
        }

        VisualElement BuildPackage(AesirGetStartedService.AesirPackageInfo pkg)
        {
            var container = new VisualElement();
            container.AddToClassList("aesir-getstarted-package");

            var installNote = pkg.InstallType == AesirGetStartedService.AesirInstallType.AssetsCopy
                ? "Assets 安装"
                : "UPM 安装 · 未导入示例可一键导入";
            var header = new Label(
                $"{pkg.DisplayName}   v{pkg.Version} · {pkg.Samples.Count} 个示例 · {installNote}");
            header.AddToClassList("aesir-getstarted-package-header");
            container.Add(header);

            foreach (var group in AesirGetStartedService.GroupSamples(pkg))
            {
                var groupTitle = new Label(group.Title);
                groupTitle.AddToClassList("aesir-getstarted-group-title");
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
            card.AddToClassList("aesir-getstarted-sample");

            var info = new VisualElement();
            info.style.flexGrow = 1f;
            var title = new Label(sample.DisplayName);
            title.AddToClassList("aesir-getstarted-sample-title");
            info.Add(title);
            if (!string.IsNullOrEmpty(sample.Description))
            {
                var desc = new Label(sample.Description);
                desc.AddToClassList("aesir-getstarted-sample-desc");
                desc.style.whiteSpace = WhiteSpace.Normal;
                info.Add(desc);
            }

            card.Add(info);

            // 动作按钮：已导入且有场景 → 打开场景；其余形态降级为定位 / 引导导入
            if (sample.IsImported && sample.HasScene)
            {
                card.Add(new Button(() => OpenScene(sample)) { text = "打开场景" });
                card.Add(new Button(() => Ping(sample)) { text = "定位" });
            }
            else if (sample.IsImported)
            {
                var noScene = new Label("代码示例 · 无独立场景");
                noScene.style.fontSize = 10f;
                card.Add(noScene);
                card.Add(new Button(() => Ping(sample)) { text = "定位" });
            }
            else
            {
                card.Add(new Button(() => Import(sample)) { text = "导入 Sample" });
            }

            return card;
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
                    Scan(); // 卡片即时切换为已导入态
                    AesirArchitectureDebug.Log("AesirGetStarted", message);
                    break;

                case AesirGetStartedService.AesirSampleImportResult.Cancelled:
                    break; // 用户在确认框取消，无需提示

                default:
                    EditorUtility.DisplayDialog(AesirGetStartedService.ImportConfirmTitle, message, "确定");
                    break;
            }
        }

        #endregion
    }
}
