using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// Aesir 包更新窗口（IMGUI 兜底版）— 面向"代码导入 Assets/Runestone（非 UPM）"的用户，
    /// 检查远程最新版本并一键更新本地安装的 Aesir 包。
    /// <para>
    /// 安装了 Odin Inspector 时，菜单入口经 <see cref="OdinWindowOpener" /> 路由到 Odin 版窗口
    /// （AesirUpdateWindowOdin，界面与交互更丰富）；未安装时本窗口为菜单落点。两个窗口的全部逻辑
    /// （多源检测 / 下载 / 备份 / 清单差集 / 更新日志）都共用 <see cref="AesirUpdateService" />，
    /// 本类只做状态持有与 IMGUI 展示。
    /// </para>
    /// <para>
    /// 流程：检测远程版本 → 拉取并展示「本地 → 远程」更新日志 → 确认框二次确认 → 备份
    /// Assets/Runestone → 按清单差集清理残留 → 静默导入 → 逐包登记安装清单。
    /// 远程版本 / 检测结果 / 更新日志均为序列化字段，更新导入触发域重载后窗口内容不丢失；
    /// 过期包列表与 .git 检测为缓存值，OnGUI 期间零 LINQ、零磁盘 IO。
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

        [MenuItem(MenuPath)]
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

        [SerializeField] List<AesirUpdateService.InstalledPackage> _packages =
            new List<AesirUpdateService.InstalledPackage>();

        [SerializeField] AesirUpdateService.ReleaseSnapshot _snapshot;
        [SerializeField] string _remoteVersion;
        [SerializeField] string _remoteSource;
        [SerializeField] string _changelogText = "";
        [SerializeField] bool _changelogExpanded = true;
        [SerializeField] string _status = "点击「检查更新」获取远程最新版本。";

        bool _busy;
        bool _isGitRepository;

        /// <summary>过期包缓存（仅在重扫 / 检测完成时重算，避免 OnGUI 每帧 LINQ）。</summary>
        List<AesirUpdateService.InstalledPackage> _outdated =
            new List<AesirUpdateService.InstalledPackage>();

        Vector2 _scrollPosition;

        #endregion

        #region 生命周期

        void OnEnable()
        {
            _isGitRepository = AesirUpdateService.IsGitRepository();
            Rescan();
        }

        /// <summary>重新扫描本地安装并重算过期包缓存（远程信息保留，域重载后继续展示）。</summary>
        void Rescan()
        {
            _packages = AesirUpdateService.ScanInstalledPackages();
            _outdated = ComputeOutdated();
            Repaint();
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
                Rescan();

                SetProgress("正在拉取更新日志 ...", 0.3f);
                await RefreshChangelog();

                _status = $"远程最新版本 {_remoteVersion}（来源：{_remoteSource}）。";
                Debug.Log($"[Aesir Updater] 远程最新版本 {_remoteVersion}（来源：{_remoteSource}）");
            }
            catch (Exception e)
            {
                _snapshot = null;
                _remoteVersion = null;
                _changelogText = "";
                Rescan();
                _status = "检查更新失败：" + e.Message;
                Debug.LogWarning($"[Aesir Updater] {_status}\n{e}");
            }
            finally
            {
                EndBusy();
            }
        }

        /// <summary>拉取并生成全部待更新包的更新日志摘要（无待更新包时清空）。</summary>
        async Task RefreshChangelog()
        {
            if (_snapshot == null || _outdated.Count == 0)
            {
                _changelogText = "";
                return;
            }

            _changelogText = await AesirUpdateService.BuildChangelogDigestAsync(_snapshot.Tag, _outdated);
        }

        #endregion

        #region 执行更新

        /// <summary>
        /// 更新入口（「全部更新」按钮）：先弹确认框防误操作，确认后执行
        /// 备份 → 逐包下载导入 → 登记清单。取消或忙碌中直接返回。
        /// 两包同 Release 发布且 Modules 依赖 Architecture——任何调用都会被扩展为全部待更新包，
        /// 从入口杜绝单包更新造成的版本撕裂。
        /// </summary>
        void RequestUpdate(List<AesirUpdateService.InstalledPackage> targets)
        {
            if (_busy || _snapshot == null || targets.Count == 0)
            {
                return;
            }

            // 统一扩展为全部待更新包（已按依赖顺序排列），忽略传入的子集
            targets = _outdated;
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
                _status = $"更新完成（{_remoteVersion}）。备份：{backupPath}";
                Debug.Log($"[Aesir Updater] {_status}");
                EditorUtility.DisplayDialog(ProgressTitle,
                    $"已更新到 {_remoteVersion}。\n\n本地修改已备份至：\n{backupPath}", "好");
            }
            catch (Exception e)
            {
                _status = "更新失败：" + e.Message;
                Debug.LogError($"[Aesir Updater] {_status}\n{e}");
                EditorUtility.DisplayDialog(ProgressTitle, _status, "好");
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

        #region UI

        void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            DrawToolbar();
            DrawHelpBoxes();
            DrawPackageList();
            DrawChangelog();
            DrawStatus();
            EditorGUILayout.EndScrollView();
        }

        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(_busy);
            if (GUILayout.Button("检查更新"))
            {
                CheckForUpdates();
            }

            if (GUILayout.Button("打开 Releases 页面"))
            {
                Application.OpenURL(AesirUpdateService.ReleasesPageUrl);
            }

            if (_outdated.Count > 0 && GUILayout.Button($"全部更新到 {_remoteVersion}"))
            {
                RequestUpdate(_outdated);
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        void DrawHelpBoxes()
        {
            EditorGUILayout.HelpBox(
                "更新范围：Assets/Runestone 下的本地安装（复制 / unitypackage 导入）。\n" +
                "经 Package Manager（Git URL）安装的副本不在本工具管辖内，请使用 Package Manager 更新。\n" +
                "版本检测经 CDN，最新发布最长约 12 小时后才会被检测到（可点「打开 Releases 页面」确认）。", MessageType.Info);

            if (_isGitRepository)
            {
                EditorGUILayout.HelpBox(
                    "检测到当前项目存在 .git 目录。若这是 AesirFramework 开发仓库，请勿执行更新——Release 内容会覆盖本地源码。",
                    MessageType.Warning);
            }
        }

        void DrawPackageList()
        {
            EditorGUILayout.LabelField("本地安装", EditorStyles.boldLabel);

            if (_packages.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    $"未在 {AesirUpdateService.InstallRootRelativePath} 下扫描到 Aesir 包。" +
                    "请通过 GitHub Releases 导入 unitypackage 安装，或确认安装目录正确。", MessageType.Warning);
                return;
            }

            foreach (var pkg in _packages)
            {
                EditorGUILayout.BeginHorizontal("box");

                EditorGUILayout.LabelField(pkg.DirName, EditorStyles.boldLabel, GUILayout.Width(160));
                GUILayout.FlexibleSpace();

                EditorGUILayout.LabelField($"本地 v{pkg.Version}", GUILayout.Width(100));
                EditorGUILayout.LabelField("→", GUILayout.Width(16));

                if (string.IsNullOrEmpty(_remoteVersion))
                {
                    EditorGUILayout.LabelField("远程未检查", EditorStyles.miniLabel, GUILayout.Width(160));
                }
                else if (AesirUpdateService.CompareVersion(pkg.Version, _remoteVersion) < 0)
                {
                    EditorGUILayout.LabelField($"远程 {_remoteVersion}", GUILayout.Width(100));
                    // 不提供单包更新按钮：两包同 Release 发布且 Modules 依赖 Architecture，
                    // 单包更新会造成版本撕裂——统一走工具栏「全部更新」
                    EditorGUILayout.LabelField("请用「全部更新」", EditorStyles.miniLabel, GUILayout.Width(130));
                }
                else if (AesirUpdateService.CompareVersion(pkg.Version, _remoteVersion) == 0)
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
            if (string.IsNullOrEmpty(_changelogText))
            {
                return;
            }

            _changelogExpanded =
                EditorGUILayout.Foldout(_changelogExpanded, "更新日志（本地 → 远程变更）", true);
            if (_changelogExpanded)
            {
                // 不回写返回值：文本区可滚动浏览、内容以 _changelogText 为唯一数据源
                EditorGUILayout.TextArea(_changelogText, EditorStyles.textArea);
            }

            EditorGUILayout.Space();
        }

        void DrawStatus()
        {
            EditorGUILayout.HelpBox(_status, MessageType.None);
        }

        #endregion

        #region 状态判定与辅助

        /// <summary>取全部待更新包（委托 <see cref="AesirUpdateService.ComputeOutdatedPackages" />，按包 id 排序保证依赖顺序）。</summary>
        List<AesirUpdateService.InstalledPackage> ComputeOutdated() =>
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
            EditorUtility.DisplayProgressBar(ProgressTitle, message, Mathf.Clamp01(progress));
        }

        #endregion
    }
}
