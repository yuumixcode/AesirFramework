using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// Aesir 包更新窗口（Odin Inspector 版）— 检测远程最新版本、展示「本地 → 远程」更新日志、
    /// 确认后一键更新 <see cref="AesirUpdateService.InstallRootRelativePath" /> 下的本地安装包。
    /// <para>
    /// 全部编排逻辑（检测 / 更新日志 / 更新执行 / 忙碌门禁）在共享控制器
    /// <see cref="AesirUpdateController" /> 中与 IMGUI 兜底窗口共用，本类只做状态序列化、标题区手绘、
    /// 行视图模型与 Odin 特性绘制。编辑器加载时经 <see cref="RegisterOpener" /> 把打开方式注册进
    /// 菜单入口 <see cref="AesirUpdateWindow" />；未安装 Odin Inspector 时本程序集整体不参与编译，
    /// 菜单自动回退到 IMGUI 兜底窗口。
    /// </para>
    /// <para>
    /// 状态设计：远程版本 / 检测结果 / 更新日志均为序列化字段，更新导入触发域重载后窗口内容不丢失；
    /// 行视图模型（<see cref="PackageRow" />）在状态变化时一次性重建并重算显示文本与颜色，
    /// OnGUI 期间零 LINQ、零字符串拼接、零磁盘 IO。
    /// </para>
    /// </summary>
    public class AesirUpdateWindowOdin : OdinEditorWindow
    {
        #region 行视图模型

        /// <summary>
        /// 单个本地安装包的行视图模型。显示文本 / 颜色 / 可更新标记在
        /// <see cref="AesirUpdateWindowOdin.RebuildRows" /> 时一次性算好，绘制期只读。
        /// </summary>
        [Serializable]
        public sealed class PackageRow
        {
            /// <summary>对应的本地安装包数据。</summary>
            [HideInInspector]
            public AesirUpdateService.InstalledPackage Model;

            /// <summary>状态文本着色。</summary>
            [HideInInspector]
            public Color StatusColor;

            /// <summary>检测到的远程版本（仅待更新时有值）。</summary>
            [HideInInspector]
            public string RemoteVersion;

            /// <summary>本地版本落后于远程版本。</summary>
            [HideInInspector]
            public bool Outdated;

            [HorizontalGroup("Row", 0.42f)]
            [DisplayAsString(false, 13)]
            [HideLabel]
            public string Name;

            [HorizontalGroup("Row", Width = 100)]
            [DisplayAsString]
            [HideLabel]
            public string Local;

            [HorizontalGroup("Row", Width = 130)]
            [DisplayAsString]
            [HideLabel]
            [GUIColor(nameof(StatusColor))]
            public string Remote;

            /// <summary>待更新提示文本（不提供单包更新按钮——统一走「全部更新」，防版本撕裂）。</summary>
            [HorizontalGroup("Row", Width = 120)]
            [DisplayAsString]
            [HideLabel]
            [ShowIf(nameof(Outdated))]
            public string UpdateHint = "请用「全部更新」";
        }

        #endregion

        #region 常量

        const string WindowTitle = "Aesir Updater";

        const string InfoText = "更新范围：本地安装的 Aesir 包（复制 / unitypackage 导入，默认位置 Assets/Runestone，" +
                                "可自由移动到项目任意文件夹）。\n" +
                                "经 Package Manager（Git URL）安装的副本不在本工具管辖内，请使用 Package Manager 更新。\n" +
                                "版本检测经 CDN，最新发布最长约 12 小时后才会被检测到（可点「打开 Releases 页面」确认）。";

        const string GitWarningText = "检测到当前项目存在 .git 目录。若这是 AesirFramework 开发仓库，请勿执行更新——Release 内容会覆盖本地源码。";

        /// <summary>未扫到包的提示（显示实际扫描根——经锚点定位，默认 Assets/Runestone）。</summary>
        static readonly string NoPackageText = $"未在 {AesirUpdateService.PrimaryInstallRoot} 下扫描到 Aesir 包。" +
                                               "请通过 GitHub Releases 导入 unitypackage 安装，或确认安装目录正确。";

        const string HeaderTitleText = "Aesir 包更新器";

        /// <summary>副标题（显示实际扫描根）。</summary>
        static readonly string HeaderSubtitleText = $"检测并更新 {AesirUpdateService.PrimaryInstallRoot} 下的本地安装包";

        /// <summary>待更新状态的提示色（暖黄）。</summary>
        static readonly Color OutdatedColor = new Color(0.95f, 0.72f, 0.2f);

        /// <summary>已是最新状态的提示色（绿色）。</summary>
        static readonly Color UpToDateColor = new Color(0.4f, 0.85f, 0.45f);

        #endregion

        #region 标题样式

        static GUIStyle _headerTitleStyle;

        /// <summary>
        /// 窗口主标题样式（正常亮度粗体）。不使用 Odin [Title]：其 BoldTitle / Subtitle 样式灰暗、
        /// 观感如禁用文本；手绘样式与 Getting Started 窗口的 BoldLabel 派生先例一致。
        /// </summary>
        static GUIStyle HeaderTitleStyle =>
            _headerTitleStyle ??= new GUIStyle(SirenixGUIStyles.BoldLabel) { fontSize = 15 };

        static GUIStyle _headerSubtitleStyle;

        /// <summary>窗口副标题样式（正常文本色小号字；Odin Subtitle 样式自带 alpha 0.7 削减，不用）。</summary>
        static GUIStyle HeaderSubtitleStyle =>
            _headerSubtitleStyle ??= new GUIStyle(SirenixGUIStyles.Label) { fontSize = 11 };

        #endregion

        #region 打开方式注册

        /// <summary>
        /// 编辑器加载时把 Odin 窗口打开方式注册给菜单入口（域重载后静态委托清空，每次重载重新注册）。
        /// </summary>
        [InitializeOnLoadMethod]
        static void RegisterOpener() => AesirUpdateWindow.RegisterOdinWindowOpener(OpenWindow);

        /// <summary>打开窗口（菜单路由到此）。</summary>
        public static void OpenWindow()
        {
            var window = GetWindow<AesirUpdateWindowOdin>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(660, 480);
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(680, 560);
            window.Show();
        }

        #endregion

        #region 状态

        /// <summary>更新器状态（序列化载体；编排与写入全部在共享控制器）。</summary>
        [SerializeField]
        [HideInInspector]
        AesirUpdateController.UpdateState _state = new AesirUpdateController.UpdateState();

        /// <summary>共享编排控制器（非序列化，OnEnable 重建并接管 _state）。</summary>
        AesirUpdateController _controller;

        Vector2 _scrollPosition;

        #endregion

        #region 面板内容

        [InfoBox(InfoText)]
        [InfoBox(GitWarningText, InfoMessageType.Warning, VisibleIf = nameof(IsGitRepository))]
        [InfoBox("$" + nameof(NoPackageText), InfoMessageType.Warning, VisibleIf = nameof(HasNoPackages))]
        [ShowInInspector]
        [ReadOnly]
        [LabelText("本地安装")]
        [ListDrawerSettings(ShowFoldout = false, DraggableItems = false, IsReadOnly = true,
            ShowItemCount = false, ShowPaging = false, ShowIndexLabels = false)]
        List<PackageRow> _rows = new List<PackageRow>();

        [HorizontalGroup("Actions")]
        [PropertySpace(8, 0)]
        [Button("检查更新", ButtonSizes.Medium)]
        [EnableIf(nameof(NotBusy))]
        void CheckForUpdatesButton() => _controller.CheckForUpdates();

        [HorizontalGroup("Actions")]
        [PropertySpace(8, 0)]
        [Button("打开 Releases 页面", ButtonSizes.Medium)]
        void OpenReleasesPage() => Application.OpenURL(AesirUpdateService.ReleasesPageUrl);

        [PropertySpace(4, 0)]
        [Button("$" + nameof(UpdateAllLabel), ButtonSizes.Large)]
        [GUIColor(0.45f, 0.85f, 0.45f)]
        [ShowIf(nameof(HasOutdated))]
        [EnableIf(nameof(NotBusy))]
        void UpdateAllButton() => _controller.RequestUpdate(_controller.OutdatedPackages());

        [FoldoutGroup("更新日志（本地 → 远程变更）", VisibleIf = nameof(HasChangelog))]
        [PropertySpace(8, 0)]
        [ShowInInspector]
        [HideLabel]
        [MultiLineProperty(14)]
        [ReadOnly]
        string ChangelogText => _state.ChangelogText;

        [ShowInInspector]
        [HideLabel]
        [ProgressBar(0, 1)]
        [ShowIf(nameof(Busy))]
        [PropertySpace(8, 0)]
        float _progress01;

        [ShowInInspector]
        [HideLabel]
        [DisplayAsString(false)]
        [PropertySpace(8, 4)]
        string StatusText => _state.Status;

        #endregion

        #region 生命周期

        protected override void OnEnable()
        {
            base.OnEnable();
            _controller = new AesirUpdateController(_state, WindowTitle, OnViewChanged,
                progress01 => _progress01 = progress01);
            _controller.Initialize();
        }

        protected override void DrawEditor(int index)
        {
            // 域重载后行视图模型的窗口引用若失效，重建（行数据在 OnViewChanged 中保持同步）
            if (_rows.Count != _state.Packages.Count)
            {
                RebuildRows();
            }

            DrawHeader();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            base.DrawEditor(index);
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 手绘窗口标题区（固定于滚动区外，内容滚动时保持可见）：正常亮度主标题 + 副标题 + 1px 分隔线
        /// （横线对齐 SirenixEditorGUI.Title 的 HorizontalLine 行为）。
        /// </summary>
        void DrawHeader()
        {
            EditorGUILayout.LabelField(HeaderTitleText, HeaderTitleStyle);
            EditorGUILayout.LabelField(HeaderSubtitleText, HeaderSubtitleStyle);
            var lineRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(1f));
            EditorGUI.DrawRect(lineRect, SirenixGUIStyles.LightBorderColor);
            EditorGUILayout.Space(3f);
        }

        /// <summary>状态变化回调（重扫 / 忙碌切换 / 状态文本变更）：重建行视图模型并重绘。</summary>
        void OnViewChanged()
        {
            RebuildRows();
            Repaint();
        }

        /// <summary>按当前本地安装与检测结果重建行视图模型（仅状态变化时调用）。</summary>
        void RebuildRows()
        {
            _rows.Clear();
            foreach (var pkg in _state.Packages)
            {
                var row = new PackageRow
                {
                    Model = pkg,
                    Name = pkg.DirName,
                    Local = "本地 v" + pkg.Version
                };

                if (string.IsNullOrEmpty(_state.RemoteVersion))
                {
                    row.Remote = "远程未检查";
                    row.StatusColor = Color.gray;
                }
                else
                {
                    var cmp = AesirUpdateService.CompareVersion(pkg.Version, _state.RemoteVersion);
                    if (cmp < 0)
                    {
                        row.Remote = $"远程 {_state.RemoteVersion}";
                        row.StatusColor = OutdatedColor;
                        row.Outdated = true;
                        row.RemoteVersion = _state.RemoteVersion;
                    }
                    else if (cmp == 0)
                    {
                        row.Remote = "已是最新";
                        row.StatusColor = UpToDateColor;
                    }
                    else
                    {
                        row.Remote = "本地高于远程";
                        row.StatusColor = Color.gray;
                    }
                }

                _rows.Add(row);
            }

            HasOutdated = _rows.Any(row => row.Outdated);
        }

        #endregion

        #region 状态判定（Odin 特性绑定）

        /// <summary>是否处于忙碌状态（据此禁用更新按钮）。</summary>
        public bool Busy => _state.Busy;

        bool NotBusy => !_state.Busy;

        bool IsGitRepository => _state.IsGitRepository;

        bool HasNoPackages => _state.Packages.Count == 0;

        bool HasOutdated { get; set; }

        bool HasChangelog => !string.IsNullOrEmpty(_state.ChangelogText);

        string UpdateAllLabel => $"全部更新到 {_state.RemoteVersion}";

        #endregion
    }
}
