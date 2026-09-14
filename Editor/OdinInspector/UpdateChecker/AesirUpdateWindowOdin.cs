using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    /// 全部逻辑（检测 / 下载 / 备份 / 清单 / 日志解析）都在 <see cref="AesirUpdateService" />，
    /// 本类只做状态持有与交互编排。编辑器加载时经 <see cref="RegisterOpener" /> 把打开方式注册进
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
        #region 常量

        const string WindowTitle = "Aesir Updater";

        const string InfoText =
            "更新范围：Assets/Runestone 下的本地安装（复制 / unitypackage 导入）。\n" +
            "经 Package Manager（Git URL）安装的副本不在本工具管辖内，请使用 Package Manager 更新。\n" +
            "版本检测经 CDN，最新发布最长约 12 小时后才会被检测到（可点「打开 Releases 页面」确认）。";

        const string GitWarningText =
            "检测到当前项目存在 .git 目录。若这是 AesirFramework 开发仓库，请勿执行更新——Release 内容会覆盖本地源码。";

        const string NoPackageText =
            "未在 Assets/Runestone 下扫描到 Aesir 包。请通过 GitHub Releases 导入 unitypackage 安装，或确认安装目录正确。";

        /// <summary>待更新状态的提示色（暖黄）。</summary>
        static readonly Color OutdatedColor = new Color(0.95f, 0.72f, 0.2f);

        /// <summary>已是最新状态的提示色（绿色）。</summary>
        static readonly Color UpToDateColor = new Color(0.4f, 0.85f, 0.45f);

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

        #region 序列化状态（跨域重载保留）

        [SerializeField, HideInInspector]
        List<AesirUpdateService.InstalledPackage> _packages = new List<AesirUpdateService.InstalledPackage>();

        [SerializeField, HideInInspector]
        AesirUpdateService.ReleaseSnapshot _snapshot;

        [SerializeField, HideInInspector]
        string _remoteVersion;

        [SerializeField, HideInInspector]
        string _remoteSource;

        #endregion

        #region 会话状态（域重载后重建）

        [HideInInspector]
        bool _busy;

        [HideInInspector]
        bool _isGitRepository;

        [HideInInspector]
        bool _hasOutdated;

        Vector2 _scrollPosition;

        #endregion

        #region 面板内容

        [Title("Aesir 包更新器", "检测并更新 Assets/Runestone 下的本地安装包")]
        [InfoBox(InfoText, InfoMessageType.Info)]
        [InfoBox(GitWarningText, InfoMessageType.Warning, VisibleIf = nameof(IsGitRepository))]
        [InfoBox(NoPackageText, InfoMessageType.Warning, VisibleIf = nameof(HasNoPackages))]
        [ShowInInspector, ReadOnly, LabelText("本地安装")]
        [ListDrawerSettings(ShowFoldout = false, DraggableItems = false, IsReadOnly = true,
            ShowItemCount = false, ShowPaging = false, ShowIndexLabels = false)]
        List<PackageRow> _rows = new List<PackageRow>();

        [HorizontalGroup("Actions"), PropertySpace(8, 0)]
        [Button("检查更新", ButtonSizes.Medium), EnableIf(nameof(NotBusy))]
        void CheckForUpdatesButton() => CheckForUpdates();

        [HorizontalGroup("Actions"), PropertySpace(8, 0)]
        [Button("打开 Releases 页面", ButtonSizes.Medium)]
        void OpenReleasesPage() => Application.OpenURL(AesirUpdateService.ReleasesPageUrl);

        [PropertySpace(4, 0)]
        [Button("$" + nameof(UpdateAllLabel), ButtonSizes.Large), GUIColor(0.45f, 0.85f, 0.45f),
         ShowIf(nameof(HasOutdated)), EnableIf(nameof(NotBusy))]
        void UpdateAllButton() => RequestUpdate(OutdatedPackages());

        [FoldoutGroup("更新日志（本地 → 远程变更）", VisibleIf = nameof(HasChangelog)), PropertySpace(8, 0)]
        [ShowInInspector, HideLabel, MultiLineProperty(14), ReadOnly]
        [SerializeField]
        string _changelogText = "";

        [ShowInInspector, HideLabel, ProgressBar(0, 1), ShowIf(nameof(Busy)), PropertySpace(8, 0)]
        float _progress01;

        [ShowInInspector, HideLabel, DisplayAsString(false), PropertySpace(8, 4)]
        [SerializeField]
        string _status = "点击「检查更新」获取远程最新版本。";

        #endregion

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

            /// <summary>所属窗口（域重载后由窗口绘制前惰性补回）。</summary>
            [HideInInspector, NonSerialized]
            public AesirUpdateWindowOdin Window;

            /// <summary>状态文本着色。</summary>
            [HideInInspector]
            public Color StatusColor;

            /// <summary>检测到的远程版本（仅待更新时有值）。</summary>
            [HideInInspector]
            public string RemoteVersion;

            /// <summary>本地版本落后于远程版本。</summary>
            [HideInInspector]
            public bool Outdated;

            [HorizontalGroup("Row", 0.42f), DisplayAsString(false, 13), HideLabel]
            public string Name;

            [HorizontalGroup("Row", Width = 100), DisplayAsString, HideLabel]
            public string Local;

            [HorizontalGroup("Row", Width = 130), DisplayAsString, HideLabel, GUIColor(nameof(StatusColor))]
            public string Remote;

            /// <summary>待更新提示文本（不提供单包更新按钮——统一走「全部更新」，防版本撕裂）。</summary>
            [HorizontalGroup("Row", Width = 120), DisplayAsString, HideLabel, ShowIf(nameof(Outdated))]
            public string UpdateHint = "请用「全部更新」";
        }

        #endregion

        #region 生命周期

        protected override void OnEnable()
        {
            base.OnEnable();
            _isGitRepository = AesirUpdateService.IsGitRepository();
            Rescan();
        }

        protected override void DrawEditor(int index)
        {
            // 域重载后行视图模型的窗口引用丢失（NonSerialized），绘制前惰性补回
            foreach (var row in _rows)
            {
                if (row.Window == null)
                {
                    row.Window = this;
                }
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            base.DrawEditor(index);
            EditorGUILayout.EndScrollView();
        }

        /// <summary>重新扫描本地安装（远程信息保留，便于更新导入触发域重载后继续展示）。</summary>
        void Rescan()
        {
            _packages = AesirUpdateService.ScanInstalledPackages();
            RebuildRows();
            Repaint();
        }

        /// <summary>按当前本地安装与检测结果重建行视图模型（仅状态变化时调用）。</summary>
        void RebuildRows()
        {
            _rows.Clear();
            foreach (var pkg in _packages)
            {
                var row = new PackageRow
                {
                    Model = pkg,
                    Window = this,
                    Name = pkg.DirName,
                    Local = "本地 v" + pkg.Version
                };

                if (string.IsNullOrEmpty(_remoteVersion))
                {
                    row.Remote = "远程未检查";
                    row.StatusColor = Color.gray;
                }
                else
                {
                    var cmp = AesirUpdateService.CompareVersion(pkg.Version, _remoteVersion);
                    if (cmp < 0)
                    {
                        row.Remote = $"远程 {_remoteVersion}";
                        row.StatusColor = OutdatedColor;
                        row.Outdated = true;
                        row.RemoteVersion = _remoteVersion;
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

            _hasOutdated = _rows.Any(row => row.Outdated);
        }

        #endregion

        #region 检查更新

        async void CheckForUpdates()
        {
            if (!BeginBusy())
            {
                return;
            }

            try
            {
                SetProgress("正在检测远程最新版本 ...", 0.05f);
                _snapshot = await AesirUpdateService.FetchLatestReleaseSnapshotAsync();
                _remoteVersion = _snapshot.Tag;
                _remoteSource = _snapshot.Source;
                RebuildRows();

                SetProgress("正在拉取更新日志 ...", 0.3f);
                await RefreshChangelog();

                SetStatus($"远程最新版本 {_remoteVersion}（来源：{_remoteSource}）。");
                Debug.Log($"[Aesir Updater] 远程最新版本 {_remoteVersion}（来源：{_remoteSource}）");
            }
            catch (Exception e)
            {
                _snapshot = null;
                _remoteVersion = null;
                _changelogText = "";
                RebuildRows();
                SetStatus("检查更新失败：" + e.Message);
                Debug.LogWarning($"[Aesir Updater] 检查更新失败：{e.Message}\n{e}");
            }
            finally
            {
                EndBusy();
            }
        }

        /// <summary>拉取并生成全部待更新包的更新日志摘要（无待更新包时清空）。</summary>
        async Task RefreshChangelog()
        {
            var outdated = OutdatedPackages();
            if (_snapshot == null || outdated.Count == 0)
            {
                _changelogText = "";
                return;
            }

            _changelogText = await AesirUpdateService.BuildChangelogDigestAsync(_snapshot.Tag, outdated);
        }

        #endregion

        #region 执行更新

        /// <summary>
        /// 更新入口（「全部更新」按钮）：先弹确认框防误操作，确认后执行
        /// 备份 → 逐包下载导入 → 登记清单。取消或忙碌中直接返回。
        /// 两包同 Release 发布且 Modules 依赖 Architecture——任何调用都会被扩展为全部待更新包，
        /// 从入口杜绝单包更新造成的版本撕裂。
        /// </summary>
        public void RequestUpdate(List<AesirUpdateService.InstalledPackage> targets)
        {
            if (_busy || _snapshot == null || targets.Count == 0)
            {
                return;
            }

            // 统一扩展为全部待更新包（已按依赖顺序排列），忽略传入的子集
            targets = OutdatedPackages();
            if (targets.Count == 0)
            {
                return;
            }

            var confirmed = EditorUtility.DisplayDialog("确认更新",
                AesirUpdateService.BuildUpdateConfirmation(targets, _remoteVersion, _isGitRepository),
                "开始更新", "取消");
            if (!confirmed)
            {
                return;
            }

            UpdatePackages(targets);
        }

        async void UpdatePackages(List<AesirUpdateService.InstalledPackage> targets)
        {
            if (!BeginBusy())
            {
                return;
            }

            try
            {
                var backupPath =
                    await AesirUpdateService.UpdatePackagesAsync(_snapshot, targets, SetProgress);
                SetStatus($"更新完成（{_remoteVersion}）。备份：{backupPath}");
                Debug.Log($"[Aesir Updater] {_status}");
                EditorUtility.DisplayDialog(WindowTitle,
                    $"已更新到 {_remoteVersion}。\n\n本地修改已备份至：\n{backupPath}", "好");
            }
            catch (Exception e)
            {
                SetStatus("更新失败：" + e.Message);
                Debug.LogError($"[Aesir Updater] 更新失败：{e.Message}\n{e}");
                EditorUtility.DisplayDialog(WindowTitle, _status, "好");
            }
            finally
            {
                EndBusy();
                Rescan();
                // 编译可能在 Refresh 内同步触发域重载；之后的日志不保证执行，重要信息已在其前输出
                AssetDatabase.Refresh();
            }
        }

        #endregion

        #region 状态判定与辅助

        /// <summary>是否处于忙碌状态（行视图模型据此禁用更新按钮）。</summary>
        public bool Busy => _busy;

        bool NotBusy => !_busy;

        bool IsGitRepository => _isGitRepository;

        bool HasNoPackages => _packages.Count == 0;

        bool HasOutdated => _hasOutdated;

        bool HasChangelog => !string.IsNullOrEmpty(_changelogText);

        string UpdateAllLabel => $"全部更新到 {_remoteVersion}";

        /// <summary>取全部待更新包（委托 <see cref="AesirUpdateService.ComputeOutdatedPackages" />，按包 id 排序保证依赖顺序）。</summary>
        List<AesirUpdateService.InstalledPackage> OutdatedPackages() =>
            AesirUpdateService.ComputeOutdatedPackages(_packages, _remoteVersion);

        bool BeginBusy()
        {
            if (_busy)
            {
                return false;
            }

            _busy = true;
            return true;
        }

        void EndBusy()
        {
            EditorUtility.ClearProgressBar();
            _busy = false;
            Repaint();
        }

        void SetProgress(string message, float progress)
        {
            _status = message;
            _progress01 = Mathf.Clamp01(progress);
            EditorUtility.DisplayProgressBar(WindowTitle, message, _progress01);
            Repaint();
        }

        void SetStatus(string message)
        {
            _status = message;
            Repaint();
        }

        #endregion
    }
}
