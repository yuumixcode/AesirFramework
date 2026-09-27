using System;
using System.Collections.Generic;
using System.Text;
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
        /// 忙碌标记的 SessionState 键（跨域重载存活）。用途只有一个：域重载后判断上一轮流程是否被打断，
        /// 据此清理残留进度条与程序集重载锁（见 <see cref="RecoverFromInterruptedRun" />）。
        /// </summary>
        const string BusySessionKey = "AesirUpdater.Busy";

        readonly Action<float> _progressChanged;
        readonly string _progressTitle;

        readonly Action _viewChanged;

        /// <summary>
        /// 用户在进度条上点了「取消」（<see cref="SetProgress" /> 置位、单向保持）。
        /// 仅更新执行阶段把进度条渲染为可取消并探测此标记；下载循环每帧评估并中止请求。
        /// </summary>
        bool _cancelRequested;

        /// <summary>
        /// 构造编排控制器。
        /// </summary>
        /// <param name="state">状态载体（由窗口的 <c>[SerializeField]</c> 字段传入）。</param>
        /// <param name="progressTitle">进度对话框标题（两个窗口各自的标题文案）。</param>
        /// <param name="viewChanged">状态变化后的视图刷新回调（重扫 / 忙碌切换 / 状态文本变更时触发）。</param>
        /// <param name="progressChanged">进度变化回调（0-1；Odin 版据此更新窗口内进度条，IMGUI 版可忽略）。</param>
        public AesirUpdateController(UpdateState state,
            string progressTitle,
            Action viewChanged,
            Action<float> progressChanged)
        {
            State = state;
            _progressTitle = progressTitle;
            _viewChanged = viewChanged;
            _progressChanged = progressChanged;
        }

        /// <summary>受控状态（窗口层只读）。</summary>
        public UpdateState State { get; }

        /// <summary>
        /// 初始化：磁盘 .git 检测 + 首次重扫。窗口 OnEnable 调用（域重载后重跑两项 NonSerialized 检测）。
        /// </summary>
        public void Initialize()
        {
            State.IsGitRepository = AesirUpdateService.IsGitRepository();
            Rescan();
        }

        /// <summary>
        /// 重新扫描本地安装（远程信息保留，更新导入触发域重载后继续展示）。
        /// 扫描根经锚点资产定位，Runestone 移动到项目任意文件夹后照常找到。
        /// </summary>
        public void Rescan()
        {
            State.Packages = AesirUpdateService.ScanInstalledPackagesFromAllRoots();
            _viewChanged?.Invoke();
        }

        /// <summary>取全部待更新包（委托 <see cref="AesirUpdateService.ComputeOutdatedPackages" />，按包 id 排序保证依赖顺序）。</summary>
        public List<AesirUpdateService.InstalledPackage> OutdatedPackages() =>
            AesirUpdateService.ComputeOutdatedPackages(State.Packages, State.RemoteVersion);

        /// <summary>
        /// 取「全部更新」的执行目标：全部待更新包 + 缺失的已知包（补装），
        /// 委托 <see cref="AesirUpdateService.ComputeUpdateTargets" />——「全部更新」按钮的可见性也以此判断。
        /// </summary>
        public List<AesirUpdateService.InstalledPackage> UpdateTargets() =>
            AesirUpdateService.ComputeUpdateTargets(State.Packages, State.RemoteVersion);

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
                var check = await AesirUpdateService.CheckLatestReleaseAsync();
                State.Snapshot = check.Snapshot;
                State.RemoteVersion = check.Snapshot.Tag;
                State.RemoteSource = check.Snapshot.Source;
                State.RemoteRouteKind = check.RouteKind;
                State.GitHubDirectAvailable = check.GitHubDirectAvailable;
                State.DetectionDetail = check.BuildAttemptsLog();
                Rescan();

                SetProgress("正在拉取更新日志 ...", 0.3f);
                await RefreshChangelog();

                SetStatus($"远程最新版本 {State.RemoteVersion}。" +
                          AesirUpdateService.BuildRouteText(State.RemoteSource, State.RemoteRouteKind));
                Debug.Log($"[Aesir Updater] 远程最新版本 {State.RemoteVersion}\n" +
                          $"{AesirUpdateService.BuildDetectionSummary(State.RemoteSource, State.RemoteRouteKind, State.GitHubDirectAvailable)}\n" +
                          $"检测详情：\n{State.DetectionDetail}");
            }
            catch (Exception e)
            {
                State.Snapshot = null;
                State.RemoteVersion = null;
                State.ChangelogText = "";
                State.RemoteSource = "";
                State.DetectionDetail = "";
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
            if (State.Snapshot == null || outdated.Count == 0)
            {
                State.ChangelogText = "";
                return;
            }

            State.ChangelogText =
                await AesirUpdateService.BuildChangelogDigestAsync(State.Snapshot.Tag, outdated);
        }

        /// <summary>
        /// 「全部更新」入口（工具栏按钮）：目标 = 全部待更新包 + 缺失的已知包（补装，
        /// <see cref="AesirUpdateService.ComputeUpdateTargets" />）——「全部更新」的语义是让整个框架
        /// 到达远程版本（旧的更新、缺的安装）。确认框含补装说明（缺失的包会标为「新安装」并单独提示，
        /// 用户可选择取消后仅单独更新已安装的包）。取消或忙碌中直接返回。
        /// </summary>
        public void RequestUpdateAll()
        {
            if (State.Busy || State.Snapshot == null)
            {
                return;
            }

            var targets = UpdateTargets();
            if (targets.Count == 0)
            {
                return;
            }

            var confirmed = EditorUtility.DisplayDialog("确认更新",
                AesirUpdateService.BuildUpdateAllConfirmation(targets, State.RemoteVersion, State.IsGitRepository),
                "开始更新", "取消");
            if (!confirmed)
            {
                return;
            }

            UpdatePackages(targets);
        }

        /// <summary>
        /// 「单包更新」入口（包列表行内按钮）：仅更新指定包，供只需要其中一个包的用户使用。
        /// 另一已知包在场且落后于远程版本时，确认框前置配套版本警告（两包按同版本配套发布，
        /// 仅更新其一可能造成版本撕裂——提示但不阻止，决定权在用户）。
        /// </summary>
        public void RequestUpdateSingle(AesirUpdateService.InstalledPackage package)
        {
            if (State.Busy || State.Snapshot == null || package == null)
            {
                return;
            }

            // 单包也须低于远程版本才有意义（窗口层只在待更新行渲染按钮，此处兜底）
            if (AesirUpdateService.CompareVersion(package.Version, State.RemoteVersion) >= 0)
            {
                return;
            }

            var confirmed = EditorUtility.DisplayDialog("确认更新",
                AesirUpdateService.BuildSingleUpdateConfirmation(package, State.RemoteVersion, State.Packages,
                    State.IsGitRepository),
                "仅更新此包", "取消");
            if (!confirmed)
            {
                return;
            }

            UpdatePackages(new List<AesirUpdateService.InstalledPackage> { package });
        }

        async void UpdatePackages(List<AesirUpdateService.InstalledPackage> targets)
        {
            if (!BeginBusy())
            {
                return;
            }

            // 锁定程序集重载：导入 unitypackage 会带来脚本变更，若中途发生域重载，
            // 本异步链会随旧域一起消失——第二个包永远等不到、进度条停在上一包的导入文案上，
            // 而 Busy 是 [NonSerialized]（域重载后重置为 false）会让按钮又能点，交互错乱。
            // 锁到流程收尾（finally 解锁）→ 全程只在最后重载一次，两个包走完同一条链路。
            // reloadLocked 是 Lock/Unlock 的配平标志：任何异常路径下恰好解锁一次、绝不重复解锁；
            // 加锁放在 try 内部——加锁本身失败时无锁可解（不解锁），忙碌标记则无论如何都会在 finally 清理
            var reloadLocked = false;
            try
            {
                EditorApplication.LockReloadAssemblies();
                reloadLocked = true;
                _cancelRequested = false;

                var result = await AesirUpdateService.UpdatePackagesAsync(State.Snapshot, targets,
                    (message, progress) => SetProgress(message, progress, cancellable: true),
                    () => _cancelRequested);

                if (result.Cancelled)
                {
                    // 用户取消：温和收尾，如实区分已导入（保持有效）与未更新的包
                    SetStatus(BuildCancelledStatus(result));
                    Debug.Log($"[Aesir Updater] {State.Status}");
                    EditorUtility.DisplayDialog(_progressTitle,
                        State.Status + "\n\n取消发生在下载阶段，项目文件未受影响，可稍后重新执行更新。", "好");
                }
                else
                {
                    SetStatus($"更新完成（{State.RemoteVersion}）");
                    Debug.Log($"[Aesir Updater] {State.Status}");
                    EditorUtility.DisplayDialog(_progressTitle,
                        $"已更新到 {State.RemoteVersion}。", "好");
                }
            }
            catch (Exception e)
            {
                SetStatus("更新失败：" + e.Message);
                Debug.LogError($"[Aesir Updater] 更新失败：{e.Message}\n{e}");
                EditorUtility.DisplayDialog(_progressTitle, State.Status, "好");
            }
            finally
            {
                // 收尾三步（清进度条 → 重扫 → 刷新导入）包在独立的 try/finally 中：
                // 任一步抛异常，解锁与忙碌标记清理仍必然执行，异常原样向上传播（fail-fast 不吞）
                try
                {
                    // 先收进度条（此刻仍持锁）：避免与 Unity 自带导入进度条互相覆盖
                    EditorUtility.ClearProgressBar();
                    Rescan();
                    // 刷新触发新脚本编译；此刻仍持锁，编译完成后的域重载被推迟到解锁之后
                    AssetDatabase.Refresh();
                }
                finally
                {
                    // 顺序铁律：先解锁、再清忙碌标记。解锁前的任何一步若被异常/强杀中断，
                    // 「AesirUpdater.Busy」仍在，域重载后 RecoverFromInterruptedRun 能兜底解锁；
                    // 若反过来先清标记再解锁，两步之间一旦中断，兜底判断即失效，
                    // 重载锁会泄漏整个会话——此后一切需要域重载的操作都阻塞，只能重启编辑器
                    if (reloadLocked)
                    {
                        EditorApplication.UnlockReloadAssemblies();
                        reloadLocked = false;
                    }

                    EndBusy();
                }
            }
        }

        /// <summary>用户取消后的状态文案：如实区分已导入（保持有效）与未更新的包。</summary>
        static string BuildCancelledStatus(AesirUpdateService.UpdateResult result)
        {
            var builder = new StringBuilder("更新已取消。");
            if (result.CompletedDirNames.Count > 0)
            {
                builder.Append("已完成导入：").Append(string.Join("、", result.CompletedDirNames))
                    .Append("（内容保持有效）。");
            }
            else
            {
                builder.Append("尚无包完成导入。");
            }

            builder.Append("未更新：").Append(string.Join("、", result.SkippedDirNames)).Append("。");
            return builder.ToString();
        }

        bool BeginBusy()
        {
            if (State.Busy)
            {
                return false;
            }

            // 清掉可能残留的进度条（上一轮流程被强杀/超时中断时会留下），再进入忙碌
            EditorUtility.ClearProgressBar();
            State.Busy = true;
            SessionState.SetBool(BusySessionKey, true);
            return true;
        }

        void EndBusy()
        {
            EditorUtility.ClearProgressBar();
            State.Busy = false;
            SessionState.SetBool(BusySessionKey, false);
            _viewChanged?.Invoke();
        }

        /// <summary>
        /// 域重载兜底收尾：上一次流程若被域重载/编辑器强杀打断（<see cref="BusySessionKey" /> 标记仍在），
        /// 这里清掉残留进度条并释放可能残留的程序集重载锁——否则进度条会一直挂在屏幕上（用户实测反馈过）。
        /// </summary>
        [InitializeOnLoadMethod]
        static void RecoverFromInterruptedRun()
        {
            if (!SessionState.GetBool(BusySessionKey, false))
            {
                return;
            }

            SessionState.SetBool(BusySessionKey, false);
            EditorUtility.ClearProgressBar();
            EditorApplication.UnlockReloadAssemblies();
            Debug.LogWarning("[Aesir Updater] 上一次更新/检测流程未正常收尾（域重载或编辑器中断），" +
                             "已清理残留进度条与程序集重载锁。");
        }

        /// <summary>
        /// 上报进度并刷新全局进度条。<paramref name="cancellable" /> 为 true 时渲染为带「取消」按钮的进度条，
        /// 用户点取消即置位 <see cref="_cancelRequested" />（下载循环每帧探测并中止请求——
        /// 卡在慢速线路时用户不必等待超时判据触发）。检测阶段进度条短暂且无法中途探测取消，保持不可取消。
        /// </summary>
        void SetProgress(string message, float progress, bool cancellable = false)
        {
            State.Status = message;
            var progress01 = Mathf.Clamp01(progress);
            _progressChanged?.Invoke(progress01);
            if (cancellable)
            {
                if (EditorUtility.DisplayCancelableProgressBar(_progressTitle, message, progress01))
                {
                    _cancelRequested = true;
                }
            }
            else
            {
                EditorUtility.DisplayProgressBar(_progressTitle, message, progress01);
            }

            _viewChanged?.Invoke();
        }

        void SetStatus(string message)
        {
            State.Status = message;
            _viewChanged?.Invoke();
        }

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

            /// <summary>远程版本的检测来源（源展示名，如 "GitHub API" / "jsDelivr (cdn.jsdelivr.net)"）。</summary>
            public string RemoteSource;

            /// <summary>检测来源所属线路类别（直连 GitHub / 镜像站 / CDN 中转）。</summary>
            public AesirUpdateService.ReleaseRouteKind RemoteRouteKind;

            /// <summary>本次检测中直连 GitHub 是否可用（可用即结果 100% 实时）。</summary>
            public bool GitHubDirectAvailable;

            /// <summary>本次检测各层尝试的可读记录（界面「检测详情」与故障定位用）。</summary>
            public string DetectionDetail = "";

            /// <summary>「本地 → 远程」更新日志摘要。</summary>
            public string ChangelogText = "";

            /// <summary>状态栏文本。</summary>
            public string Status = "点击「检查更新」获取远程最新版本。";

            /// <summary>忙碌标志（域重载后重置，见类注释）。</summary>
            [NonSerialized]
            public bool Busy;

            /// <summary>项目根是否存在 .git 目录（每次 <see cref="Initialize" /> 重跑）。</summary>
            [NonSerialized]
            public bool IsGitRepository;
        }
    }
}
