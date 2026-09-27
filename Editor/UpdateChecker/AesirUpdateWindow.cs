using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// Aesir 包更新窗口（IMGUI 兜底版）— 面向"代码导入 Assets/Runestone（非 UPM）"的用户，
    /// 检查远程最新版本并更新本地安装的 Aesir 包。两种入口：
    /// 包列表行内「更新」按钮仅更新单个包（配套版本风险由确认框提示）；
    /// 工具栏「全部更新」让整个框架到达远程版本（旧的更新、缺失的已知包补装，确认框明示）。
    /// <para>
    /// 安装了 Odin Inspector 时，菜单入口经 <see cref="OdinWindowOpener" /> 路由到 Odin 版窗口
    /// （AesirUpdateWindowOdin，界面与交互更丰富）；未安装时本窗口为菜单落点。
    /// 全部编排逻辑（检测 / 更新日志 / 更新执行 / 忙碌门禁）在共享控制器
    /// <see cref="AesirUpdateController" /> 中与 Odin 版窗口共用，本类只做状态序列化与 IMGUI 展示。
    /// </para>
    /// <para>
    /// 流程：检测远程版本 → 拉取并展示「本地 → 远程」更新日志 → 确认框二次确认 →
    /// 静默导入（下载经「直连 → 镜像站」逐线路兜底，进度条可随时取消）→
    /// 按清单差集清理残留 → 逐包登记安装清单。
    /// 远程版本 / 检测结果 / 更新日志均为序列化字段，更新导入触发域重载后窗口内容不丢失；
    /// 过期包列表为缓存值，OnGUI 期间零 LINQ、零磁盘 IO。
    /// </para>
    /// <para>
    /// 菜单项经 validate 按安装形态显隐：扫描不到 Assets 形态的 Aesir 包安装时整体隐藏
    /// （UPM 安装的副本由 Package Manager 管辖，不经过本更新器）。
    /// </para>
    /// </summary>
    public class AesirUpdateWindow : EditorWindow
    {
        #region 菜单与 Odin 窗口路由

        const string MenuPath = "Tools/Aesir/Check for Updates";
        const string ProgressTitle = "Aesir 更新";

        /// <summary>
        /// Odin 版窗口的打开委托（由 Odin 程序集经 [InitializeOnLoadMethod] 注册；
        /// 未安装 Odin Inspector 时为 null，菜单打开本 IMGUI 兜底窗口）。
        /// </summary>
        public static Action OdinWindowOpener { get; private set; }

        /// <summary>注册 Odin 版窗口的打开方式（域重载清空静态委托后由 Odin 程序集重新注册）。</summary>
        public static void RegisterOdinWindowOpener(Action opener) => OdinWindowOpener = opener;

        // priority 1100：更新入口置 Tools/Aesir 最底部，与上方工具组（最大 1002）差值超过 10，
        // Unity 自动插入独立分割线（对齐 Getting Started -980 置顶配分割线的先例）

        // validate：扫描不到 Assets 形态的 Aesir 包安装时隐藏菜单——UPM 安装（Package Manager 管理）
        // 或未安装形态下更新器没有管辖对象（UPM 副本的更新应走 Package Manager），菜单不再显示避免误导；
        // Assets 形态安装（含与 UPM 混合并存）时照常显示，更新器只管辖 Assets 副本。
        // 扫描为纯本地目录 IO（锚点定位安装根 + 解析 package.json），菜单展开频率下开销可忽略
        [MenuItem(MenuPath, true)]
        static bool ValidateMenuVisible()
        {
            return AesirUpdateService.ScanInstalledPackagesFromAllRoots().Count > 0;
        }

        [MenuItem(MenuPath, false, 1100)]
        static void Open()
        {
            if (OdinWindowOpener != null)
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

        /// <summary>更新器状态（序列化载体；编排与写入全部在共享控制器）。</summary>
        [SerializeField]
        AesirUpdateController.UpdateState _state = new AesirUpdateController.UpdateState();

        /// <summary>更新日志折叠展开态。</summary>
        [SerializeField]
        bool _changelogExpanded = true;

        /// <summary>检测详情（各层尝试）折叠展开态。</summary>
        [SerializeField]
        bool _detectionDetailExpanded;

        /// <summary>共享编排控制器（非序列化，OnEnable 重建并接管 _state）。</summary>
        AesirUpdateController _controller;

        /// <summary>过期包缓存（视图回调时重算，避免 OnGUI 每帧 LINQ）。</summary>
        List<AesirUpdateService.InstalledPackage> _outdated = new List<AesirUpdateService.InstalledPackage>();

        /// <summary>「全部更新」目标缓存（待更新包 + 缺失的已知包补装；决定工具栏按钮可见性）。</summary>
        List<AesirUpdateService.InstalledPackage> _updateTargets =
            new List<AesirUpdateService.InstalledPackage>();

        Vector2 _scrollPosition;

        #endregion

        #region 生命周期

        void OnEnable()
        {
            _controller = new AesirUpdateController(_state, ProgressTitle, OnViewChanged, _ => { });
            _controller.Initialize();
        }

        /// <summary>状态变化回调（重扫 / 忙碌切换 / 状态文本变更）：重算缓存并重绘。</summary>
        void OnViewChanged()
        {
            _outdated = _controller.OutdatedPackages();
            _updateTargets = _controller.UpdateTargets();
            Repaint();
        }

        #endregion

        #region UI

        void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            DrawToolbar();
            DrawHelpBoxes();
            DrawPackageList();
            DrawChangelog();
            DrawDetectionDetail();
            DrawStatus();
            EditorGUILayout.EndScrollView();
        }

        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(_state.Busy);
            if (GUILayout.Button("检查更新"))
            {
                _controller.CheckForUpdates();
            }

            if (GUILayout.Button("打开 Releases 页面"))
            {
                Application.OpenURL(AesirUpdateService.ReleasesPageUrl);
            }

            if (_updateTargets.Count > 0 && GUILayout.Button($"全部更新到 {_state.RemoteVersion}"))
            {
                _controller.RequestUpdateAll();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        void DrawHelpBoxes()
        {
            EditorGUILayout.HelpBox(
                "更新范围：本地安装的 Aesir 包（复制 / unitypackage 导入，默认位置 Assets/Runestone，" + "可自由移动到项目任意文件夹）。\n" +
                "经 Package Manager（Git URL）安装的副本不在本工具管辖内，请使用 Package Manager 更新。\n" +
                "版本检测按「直连 GitHub → 镜像站 → CDN 中转」顺序兜底，能直连 GitHub 即为 100% 最新；" +
                "本次实际线路见下方检测结果。", MessageType.Info);

            if (_state.IsGitRepository)
            {
                EditorGUILayout.HelpBox(
                    "检测到当前项目存在 .git 目录。若这是 AesirFramework 开发仓库，请勿执行更新——Release 内容会覆盖本地源码。",
                    MessageType.Warning);
            }

            if (_state.Snapshot == null)
            {
                return;
            }

            EditorGUILayout.HelpBox(
                AesirUpdateService.BuildDetectionSummary(_state.RemoteSource, _state.RemoteRouteKind,
                    _state.GitHubDirectAvailable), MessageType.None);

            if (_state.RemoteRouteKind == AesirUpdateService.ReleaseRouteKind.CdnRelay)
            {
                EditorGUILayout.HelpBox(AesirUpdateService.BuildCdnDelayHintText(), MessageType.Warning);
            }
        }

        /// <summary>检测详情（各层尝试记录）折叠区——兜底机制可观测，便于定位网络问题。</summary>
        void DrawDetectionDetail()
        {
            if (string.IsNullOrEmpty(_state.DetectionDetail))
            {
                return;
            }

            _detectionDetailExpanded = EditorGUILayout.Foldout(_detectionDetailExpanded, "检测详情（各层尝试）", true);
            if (_detectionDetailExpanded)
            {
                // 不回写返回值：文本区可滚动浏览、内容以状态为唯一数据源
                EditorGUILayout.TextArea(_state.DetectionDetail, EditorStyles.textArea);
            }

            EditorGUILayout.Space();
        }

        void DrawPackageList()
        {
            EditorGUILayout.LabelField("本地安装", EditorStyles.boldLabel);

            if (_state.Packages.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    $"未在 {AesirUpdateService.PrimaryInstallRoot} 下扫描到 Aesir 包。" +
                    "请通过 GitHub Releases 导入 unitypackage 安装，或确认安装目录正确。", MessageType.Warning);
                return;
            }

            foreach (var pkg in _state.Packages)
            {
                EditorGUILayout.BeginHorizontal("box");

                EditorGUILayout.LabelField(pkg.DirName, EditorStyles.boldLabel, GUILayout.Width(160));
                GUILayout.FlexibleSpace();

                EditorGUILayout.LabelField($"本地 v{pkg.Version}", GUILayout.Width(100));
                EditorGUILayout.LabelField("→", GUILayout.Width(16));

                if (string.IsNullOrEmpty(_state.RemoteVersion))
                {
                    EditorGUILayout.LabelField("远程未检查", EditorStyles.miniLabel, GUILayout.Width(160));
                }
                else if (AesirUpdateService.CompareVersion(pkg.Version, _state.RemoteVersion) < 0)
                {
                    EditorGUILayout.LabelField($"远程 {_state.RemoteVersion}", GUILayout.Width(100));
                    // 行内单包更新：只需更新某一个包的用户入口（配套版本风险由确认框提示，不阻止）
                    using (new EditorGUI.DisabledScope(_state.Busy))
                    {
                        if (GUILayout.Button("更新", GUILayout.Width(90)))
                        {
                            _controller.RequestUpdateSingle(pkg);
                        }
                    }
                }
                else if (AesirUpdateService.CompareVersion(pkg.Version, _state.RemoteVersion) == 0)
                {
                    EditorGUILayout.LabelField("已是最新", EditorStyles.miniLabel, GUILayout.Width(160));
                }
                else
                {
                    EditorGUILayout.LabelField("本地高于远程", EditorStyles.miniLabel, GUILayout.Width(160));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
        }

        void DrawChangelog()
        {
            if (string.IsNullOrEmpty(_state.ChangelogText))
            {
                return;
            }

            _changelogExpanded = EditorGUILayout.Foldout(_changelogExpanded, "更新日志（本地 → 远程变更）", true);
            if (_changelogExpanded)
            {
                // 不回写返回值：文本区可滚动浏览、内容以状态为唯一数据源
                EditorGUILayout.TextArea(_state.ChangelogText, EditorStyles.textArea);
            }

            EditorGUILayout.Space();
        }

        void DrawStatus()
        {
            EditorGUILayout.HelpBox(_state.Status, MessageType.None);
        }

        #endregion
    }
}
