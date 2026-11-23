using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// Aesir 包更新窗口（UI Toolkit 版）— 面向"代码导入 Assets/Runestone（非 UPM）"的用户，
    /// 检查远程最新版本并更新本地安装的 Aesir 包。两种入口：
    /// 包列表行内「更新」按钮仅更新单个包；工具栏「全部更新」让整个框架到达远程版本。
    /// <para>
    /// 安装 Odin Inspector 且样式开关 <see cref="AesirInspectorStyleSettings.UseOdin" /> 为 true 时，
    /// 菜单经 <see cref="OdinWindowOpener" /> 路由到 Odin 版窗口；否则打开本 UI Toolkit 窗口。
    /// 全部编排逻辑（检测 / 更新日志 / 更新执行 / 忙碌门禁）在共享控制器
    /// <see cref="AesirUpdateController" /> 中两版共用，本类只做状态持有与 UI Toolkit 展示。
    /// </para>
    /// </summary>
    public class AesirUpdateWindow : EditorWindow
    {
        #region 菜单与 Odin 窗口路由

        const string MenuPath = "Tools/Aesir/Check for Updates";
        const string ProgressTitle = "Aesir 更新";

        /// <summary>Odin 版窗口的打开委托（Odin 程序集注册；关闭 Odin 样式时为 null）。</summary>
        public static Action OdinWindowOpener { get; private set; }

        /// <summary>注册 Odin 版窗口的打开方式（域重载清空静态委托后由 Odin 程序集重新注册）。</summary>
        public static void RegisterOdinWindowOpener(Action opener) => OdinWindowOpener = opener;

        // validate：扫描不到 Assets 形态的 Aesir 包安装时隐藏菜单——UPM 安装由 Package Manager 管辖
        [MenuItem(MenuPath, true)]
        static bool ValidateMenuVisible() => AesirUpdateService.ScanInstalledPackagesFromAllRoots().Count > 0;

        [MenuItem(MenuPath, false, 1100)]
        static void Open()
        {
            if (AesirInspectorStyleSettings.UseOdin && OdinWindowOpener != null)
            {
                OdinWindowOpener();
                return;
            }

            var window = GetWindow<AesirUpdateWindow>("Aesir Updater");
            window.minSize = new Vector2(560, 420);
            window.Show();
        }

        #endregion

        #region 状态

        [SerializeField]
        AesirUpdateController.UpdateState _state = new AesirUpdateController.UpdateState();

        [SerializeField]
        bool _changelogExpanded = true;

        [SerializeField]
        bool _detectionDetailExpanded;

        AesirUpdateController _controller;

        List<AesirUpdateService.InstalledPackage> _updateTargets =
            new List<AesirUpdateService.InstalledPackage>();

        #endregion

        #region 生命周期

        void OnEnable()
        {
            // 「仅重绘」回调：进度/状态文本变化只刷 UI，不重算列表
            _controller =
                new AesirUpdateController(_state, ProgressTitle, OnViewChanged, _ => { }, RefreshUI);
            _controller.Initialize();
            BuildUI();
        }

        /// <summary>结构变化回调（重扫 / 忙碌切换）：重算「全部更新」目标缓存并刷新视图。</summary>
        void OnViewChanged()
        {
            _updateTargets = _controller.UpdateTargets();
            RefreshUI();
        }

        #endregion

        #region UI

        void BuildUI()
        {
            _content = new ScrollView(ScrollViewMode.Vertical);
            var root = rootVisualElement;
            root.Clear();
            root.Add(_content);
            RefreshUI();
        }

        ScrollView _content;

        /// <summary>按当前状态重建内容树（状态为唯一数据源，更新窗口非高频交互，整体重建简单可靠）。</summary>
        void RefreshUI()
        {
            if (_content == null)
            {
                return;
            }

            var scroll = _content.scrollOffset;
            _content.Clear();

            _content.Add(BuildToolbar());
            AddInfoBox(
                "更新范围：本地安装的 Aesir 包（复制 / unitypackage 导入，默认位置 Assets/Runestone，可自由移动到项目任意文件夹）。\n" +
                "经 Package Manager（Git URL）安装的副本不在本工具管辖内，请使用 Package Manager 更新。\n" +
                "版本检测按「直连 GitHub → 镜像站 → CDN 中转」顺序兜底，能直连 GitHub 即为 100% 最新；本次实际线路见下方检测结果。",
                HelpBoxMessageType.Info);

            if (_state.IsGitRepository)
            {
                AddInfoBox("检测到当前项目存在 .git 目录。若这是 AesirFramework 开发仓库，请勿执行更新——Release 内容会覆盖本地源码。",
                    HelpBoxMessageType.Warning);
            }

            if (_state.Snapshot != null)
            {
                AddInfoBox(
                    AesirUpdateService.BuildDetectionSummary(_state.RemoteSource, _state.RemoteRouteKind,
                        _state.GitHubDirectAvailable), HelpBoxMessageType.None);
                if (_state.RemoteRouteKind == AesirUpdateService.ReleaseRouteKind.CdnRelay)
                {
                    AddInfoBox(AesirUpdateService.BuildCdnDelayHintText(), HelpBoxMessageType.Warning);
                }
            }

            _content.Add(BuildPackageList());
            _content.Add(BuildDetectionDetail());
            _content.Add(BuildChangelog());
            AddInfoBox(_state.Status, HelpBoxMessageType.None);

            _content.scrollOffset = scroll;
        }

        void AddInfoBox(string text, HelpBoxMessageType type)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var box = new HelpBox(text, type);
            box.style.whiteSpace = WhiteSpace.Normal;
            _content.Add(box);
        }

        VisualElement BuildToolbar()
        {
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.alignItems = Align.Center;
            toolbar.style.paddingBottom = 4f;

            var check = new Button(() => _controller.CheckForUpdates()) { text = "检查更新" };
            check.SetEnabled(!_state.Busy);
            toolbar.Add(check);

            toolbar.Add(new Button(() => Application.OpenURL(AesirUpdateService.ReleasesPageUrl))
            {
                text = "打开 Releases 页面"
            });

            if (_updateTargets.Count > 0)
            {
                var updateAll = new Button(() => _controller.RequestUpdateAll())
                {
                    text = $"全部更新到 {_state.RemoteVersion}"
                };
                updateAll.SetEnabled(!_state.Busy);
                toolbar.Add(updateAll);
            }

            return toolbar;
        }

        VisualElement BuildPackageList()
        {
            var section = new VisualElement();
            var title = new Label("本地安装");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            section.Add(title);

            if (_state.Packages.Count == 0)
            {
                section.Add(new HelpBox(
                    $"未在 {AesirUpdateService.PrimaryInstallRoot} 下扫描到 Aesir 包。" +
                    "请通过 GitHub Releases 导入 unitypackage 安装，或确认安装目录正确。", HelpBoxMessageType.Warning));
                return section;
            }

            foreach (var pkg in _state.Packages)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.paddingLeft = 4f;
                row.style.paddingRight = 4f;
                row.style.paddingTop = 2f;
                row.style.paddingBottom = 2f;

                var name = new Label(pkg.DirName) { style = { width = 160 } };
                name.style.unityFontStyleAndWeight = FontStyle.Bold;
                row.Add(name);
                row.Add(new Label($"本地 v{pkg.Version}") { style = { width = 100 } });
                row.Add(new Label("→") { style = { width = 16 } });

                if (string.IsNullOrEmpty(_state.RemoteVersion))
                {
                    row.Add(new Label("远程未检查") { style = { width = 160 } });
                }
                else if (AesirUpdateService.CompareVersion(pkg.Version, _state.RemoteVersion) < 0)
                {
                    row.Add(new Label($"远程 {_state.RemoteVersion}") { style = { width = 100 } });
                    var single = new Button(() => _controller.RequestUpdateSingle(pkg)) { text = "更新" };
                    single.style.width = 90f;
                    single.SetEnabled(!_state.Busy);
                    row.Add(single);
                }
                else if (AesirUpdateService.CompareVersion(pkg.Version, _state.RemoteVersion) == 0)
                {
                    row.Add(new Label("已是最新") { style = { width = 160 } });
                }
                else
                {
                    row.Add(new Label("本地高于远程") { style = { width = 160 } });
                }

                section.Add(row);
            }

            return section;
        }

        VisualElement BuildDetectionDetail() =>
            BuildFoldout("检测详情（各层尝试）", _state.DetectionDetail, _detectionDetailExpanded,
                expanded => _detectionDetailExpanded = expanded);

        VisualElement BuildChangelog() =>
            BuildFoldout("更新日志（本地 → 远程变更）", _state.ChangelogText, _changelogExpanded,
                expanded => _changelogExpanded = expanded);

        VisualElement BuildFoldout(string title, string text, bool expanded, Action<bool> onToggle)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new VisualElement();
            }

            var foldout = new Foldout { text = title, value = expanded };
            foldout.RegisterValueChangedCallback(evt => onToggle(evt.newValue));
            var area = new TextField { value = text, multiline = true, isReadOnly = true };
            area.style.whiteSpace = WhiteSpace.Normal;
            area.style.minHeight = 60f;
            foldout.Add(area);
            return foldout;
        }

        #endregion
    }
}
