using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// 包更新窗口的共享编排控制器——检测 / 更新日志 / 更新执行 / 忙碌门禁 / 进度的唯一真源。
    /// <para>
    /// IMGUI 兜底窗口与 Odin 版窗口经构造注入同一 <see cref="UpdateState" /> 与视图回调复用本类，
    /// 消除两窗口各自维护一份编排逻辑的改一漏一风险（单包撕裂防呆、确认框、忙碌门禁只此一份）。
    /// 状态以 <see cref="UpdateState" /> 为载体挂在窗口的 <c>[SerializeField]</c> 字段上跨域重载保留；
    /// <see cref="UpdateState.Busy" /> 与 <see cref="UpdateState.IsGitRepository" /> 为 <c>[NonSerialized]</c>
    /// ——忙碌标志在更新导入触发的域重载后重置为 false（原协程已死，不重置会永久卡忙碌），
    /// .git 检测每次 <see cref="Initialize" /> 重跑。
    /// </para>
    /// </summary>
    /// <remarks>
    /// 本类与两个窗口同属更新器内部实现，非对外 API；窗口层只保留绘制与路由。
    /// </remarks>
    public sealed class AesirUpdateController
    {
        /// <summary>
        /// 更新器状态。窗口以 <c>[SerializeField]</c> 持有以跨域重载保留远程检测结果与更新日志；
        /// <see cref="AesirUpdateController" /> 是唯一写入者，窗口层只读。
        /// </summary>
        [Serializable]
        public sealed class UpdateState
        {
            /// <summary>本地扫描到的安装包（<see cref="Rescan" /> 时重建）。</summary>
            public List<AesirUpdateService.InstalledPackage> Packages =
                new List<AesirUpdateService.InstalledPackage>();

            /// <summary>远程 Release 快照（检测成功后有值）。</summary>
            public AesirUpdateService.ReleaseSnapshot Snapshot;

            /// <summary>远程最新版本号。</summary>
            public string RemoteVersion;

            /// <summary>远程版本的检测来源。</summary>
            public string RemoteSource;

            /// <summary>「本地 → 远程」更新日志摘要。</summary>
            public string ChangelogText = "";

            /// <summary>状态栏文本。</summary>
            public string Status = "点击「检查更新」获取远程最新版本。";

            /// <summary>忙碌标志（域重载后重置，见类注释）。</summary>
            [NonSerialized] public bool Busy;

            /// <summary>项目根是否存在 .git 目录（每次 <see cref="Initialize" /> 重跑）。</summary>
            [NonSerialized] public bool IsGitRepository;
        }

        readonly UpdateState _state;
        readonly string _progressTitle;
        readonly Action _viewChanged;
        readonly Action<float> _progressChanged;

        /// <summary>受控状态（窗口层只读）。</summary>
        public UpdateState State => _state;

        /// <summary>
        /// 构造编排控制器。
        /// </summary>
        /// <param name="state">状态载体（由窗口的 <c>[SerializeField]</c> 字段传入）。</param>
        /// <param name="progressTitle">进度对话框标题（两个窗口各自的标题文案）。</param>
        /// <param name="viewChanged">状态变化后的视图刷新回调（重扫 / 忙碌切换 / 状态文本变更时触发）。</param>
        /// <param name="progressChanged">进度变化回调（0-1；Odin 版据此更新窗口内进度条，IMGUI 版可忽略）。</param>
        public AesirUpdateController(UpdateState state, string progressTitle, Action viewChanged,
            Action<float> progressChanged)
        {
            _state = state;
            _progressTitle = progressTitle;
            _viewChanged = viewChanged;
            _progressChanged = progressChanged;
        }

        /// <summary>
        /// 初始化：磁盘 .git 检测 + 首次重扫。窗口 OnEnable 调用（域重载后重跑两项 NonSerialized 检测）。
        /// </summary>
        public void Initialize()
        {
            _state.IsGitRepository = AesirUpdateService.IsGitRepository();
            Rescan();
        }

        /// <summary>重新扫描本地安装（远程信息保留，更新导入触发域重载后继续展示）。</summary>
        public void Rescan()
        {
            _state.Packages = AesirUpdateService.ScanInstalledPackages();
            _viewChanged?.Invoke();
        }

        /// <summary>取全部待更新包（委托 <see cref="AesirUpdateService.ComputeOutdatedPackages" />，按包 id 排序保证依赖顺序）。</summary>
        public List<AesirUpdateService.InstalledPackage> OutdatedPackages() =>
            AesirUpdateService.ComputeOutdatedPackages(_state.Packages, _state.RemoteVersion);

        /// <summary>检查远程最新版本并拉取更新日志（忙碌中重复调用直接返回）。</summary>
        public async void CheckForUpdates()
        {
            if (!BeginBusy())
            {
                return;
            }

            try
            {
                SetProgress("正在检测远程最新版本 ...", 0.05f);
                _state.Snapshot = await AesirUpdateService.FetchLatestReleaseSnapshotAsync();
                _state.RemoteVersion = _state.Snapshot.Tag;
                _state.RemoteSource = _state.Snapshot.Source;
                Rescan();

                SetProgress("正在拉取更新日志 ...", 0.3f);
                await RefreshChangelog();

                SetStatus($"远程最新版本 {_state.RemoteVersion}（来源：{_state.RemoteSource}）。");
                Debug.Log($"[Aesir Updater] 远程最新版本 {_state.RemoteVersion}（来源：{_state.RemoteSource}）");
            }
            catch (Exception e)
            {
                _state.Snapshot = null;
                _state.RemoteVersion = null;
                _state.ChangelogText = "";
                Rescan();
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
            if (_state.Snapshot == null || outdated.Count == 0)
            {
                _state.ChangelogText = "";
                return;
            }

            _state.ChangelogText = await AesirUpdateService.BuildChangelogDigestAsync(_state.Snapshot.Tag, outdated);
        }

        /// <summary>
        /// 更新入口（「全部更新」按钮）：先弹确认框防误操作，确认后执行
        /// 备份 → 逐包下载导入 → 登记清单。取消或忙碌中直接返回。
        /// 两包同 Release 发布且 Modules 依赖 Architecture——任何调用都会被扩展为全部待更新包，
        /// 从入口杜绝单包更新造成的版本撕裂。
        /// </summary>
        public void RequestUpdate(List<AesirUpdateService.InstalledPackage> targets)
        {
            if (_state.Busy || _state.Snapshot == null || targets == null || targets.Count == 0)
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
                AesirUpdateService.BuildUpdateConfirmation(targets, _state.RemoteVersion, _state.IsGitRepository),
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
                    await AesirUpdateService.UpdatePackagesAsync(_state.Snapshot, targets, SetProgress);
                SetStatus($"更新完成（{_state.RemoteVersion}）。备份：{backupPath}");
                Debug.Log($"[Aesir Updater] {_state.Status}");
                EditorUtility.DisplayDialog(_progressTitle,
                    $"已更新到 {_state.RemoteVersion}。\n\n本地修改已备份至：\n{backupPath}", "好");
            }
            catch (Exception e)
            {
                SetStatus("更新失败：" + e.Message);
                Debug.LogError($"[Aesir Updater] 更新失败：{e.Message}\n{e}");
                EditorUtility.DisplayDialog(_progressTitle, _state.Status, "好");
            }
            finally
            {
                EndBusy();
                Rescan();
                // 编译可能在 Refresh 内同步触发域重载；之后的日志不保证执行，重要信息已在其前输出
                AssetDatabase.Refresh();
            }
        }

        bool BeginBusy()
        {
            if (_state.Busy)
            {
                return false;
            }

            _state.Busy = true;
            return true;
        }

        void EndBusy()
        {
            EditorUtility.ClearProgressBar();
            _state.Busy = false;
            _viewChanged?.Invoke();
        }

        void SetProgress(string message, float progress)
        {
            _state.Status = message;
            var progress01 = Mathf.Clamp01(progress);
            _progressChanged?.Invoke(progress01);
            EditorUtility.DisplayProgressBar(_progressTitle, message, progress01);
            _viewChanged?.Invoke();
        }

        void SetStatus(string message)
        {
            _state.Status = message;
            _viewChanged?.Invoke();
        }
    }
}
