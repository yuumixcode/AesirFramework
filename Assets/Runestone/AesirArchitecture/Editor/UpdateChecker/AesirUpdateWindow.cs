using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// Aesir 包更新窗口 — 面向"代码导入 Assets/Runestone（非 UPM）"的用户，
    /// 检查远程最新版本并一键更新本地安装的 Aesir 包。
    /// <para>
    /// 版本检测面向大陆用户做多源兜底（jsDelivr 多域名 → GitHub API → 302 探测，见
    /// <see cref="AesirUpdateService.FetchLatestReleaseSnapshotAsync" />）；unitypackage 一律从
    /// GitHub Release 直链下载。流程：备份 Assets/Runestone → 按清单差集清理残留 → 静默导入 →
    /// 逐包登记安装清单。实现参考 QFramework PackageKit 的更新链路，差异点见
    /// <see cref="AesirUpdateService" /> 文档。
    /// </para>
    /// </summary>
    public class AesirUpdateWindow : EditorWindow
    {
        #region 常量与状态

        const string MenuPath = "Tools/Aesir/Check for Updates";
        const string ProgressTitle = "Aesir 更新";

        List<AesirUpdateService.InstalledPackage> _packages = new();
        AesirUpdateService.ReleaseSnapshot _snapshot;
        bool _busy;
        string _status = "点击「检查更新」获取远程最新版本。";

        #endregion

        #region 菜单与生命周期

        [MenuItem(MenuPath)]
        static void Open()
        {
            var window = GetWindow<AesirUpdateWindow>("Aesir Updater");
            window.minSize = new Vector2(560, 340);
        }

        void OnEnable() => Rescan();

        /// <summary>重新扫描本地安装（远程信息保留，便于更新导入触发域重载后继续展示）。</summary>
        void Rescan() => _packages = AesirUpdateService.ScanInstalledPackages();

        #endregion

        #region UI

        void OnGUI()
        {
            DrawToolbar();
            DrawHelpBoxes();
            DrawPackageList();
            DrawStatus();
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

            if (_packages.Count > 0 && AnyOutdated())
            {
                if (GUILayout.Button($"全部更新到 {RemoteVersion}"))
                {
                    UpdatePackages(OutdatedPackages());
                }
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
                "版本检测经 CDN，最新发布最长约 12 小时后才会被检测到（可点「打开 Releases 页面」确认）。",
                MessageType.Info);

            if (Directory.Exists(AesirUpdateService.ToAbsolutePath(".git")))
            {
                EditorGUILayout.HelpBox(
                    "检测到当前项目存在 .git 目录。若这是 AesirFramework 开发仓库，请勿执行更新——" +
                    "Release 内容会覆盖本地源码。",
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

                if (_snapshot == null)
                {
                    EditorGUILayout.LabelField("远程未检查", EditorStyles.miniLabel, GUILayout.Width(160));
                }
                else if (IsOutdated(pkg))
                {
                    EditorGUILayout.LabelField($"远程 {RemoteVersion}", GUILayout.Width(100));
                    if (GUILayout.Button($"更新到 {RemoteVersion}", GUILayout.Width(130)))
                    {
                        UpdatePackages(new List<AesirUpdateService.InstalledPackage> { pkg });
                    }
                }
                else if (AesirUpdateService.CompareVersion(pkg.Version, RemoteVersion) == 0)
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

        void DrawStatus()
        {
            EditorGUILayout.HelpBox(_status, MessageType.None);
        }

        #endregion

        #region 状态判定

        string RemoteVersion => string.IsNullOrEmpty(_snapshot?.Tag) ? null : _snapshot.Tag;

        bool IsOutdated(AesirUpdateService.InstalledPackage pkg) =>
            RemoteVersion != null && AesirUpdateService.CompareVersion(pkg.Version, RemoteVersion) < 0;

        bool AnyOutdated() => OutdatedPackages().Count > 0;

        /// <summary>取全部待更新包，按包 id 排序保证依赖顺序（Architecture 先于 Modules）。</summary>
        List<AesirUpdateService.InstalledPackage> OutdatedPackages() =>
            _packages.Where(IsOutdated).OrderBy(p => p.PackageId, StringComparer.Ordinal).ToList();

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
                SetProgress("正在检测远程最新版本 ...", 0.1f);
                _snapshot = await AesirUpdateService.FetchLatestReleaseSnapshotAsync();
                Rescan();
                _status = $"远程最新版本 {RemoteVersion}（来源：{_snapshot.Source}）。";
                Debug.Log($"[Aesir Updater] 远程最新版本 {RemoteVersion}（来源：{_snapshot.Source}）");
            }
            catch (Exception e)
            {
                _snapshot = null;
                _status = "检查更新失败：" + e.Message;
                Debug.LogWarning($"[Aesir Updater] {_status}\n{e}");
            }
            finally
            {
                EndBusy();
            }
        }

        #endregion

        #region 执行更新

        /// <summary>
        /// 更新指定的包列表：备份 → 逐包（下载 → 清残留 → 静默导入 → 登记清单）→ 刷新。
        /// 检测结果 <see cref="AesirUpdateService.ReleaseSnapshot.Info" /> 同时承载新清单；
        /// 302 重定向降级路径无清单，残留清理自动跳过。
        /// </summary>
        async void UpdatePackages(List<AesirUpdateService.InstalledPackage> targets)
        {
            if (!BeginBusy() || targets.Count == 0)
            {
                return;
            }

            try
            {
                var snapshot = _snapshot;

                // 1. 整体备份（一次，覆盖本次全部导入）
                SetProgress("备份 Assets/Runestone ...", 0.05f);
                var backupPath = AesirUpdateService.BackupRunestone(
                    $"{DateTime.Now:yyyyMMdd-HHmmss}_v{GetMaxLocalVersion()}");

                var localManifest = AesirUpdateService.LoadLocalManifest();

                // 2. 逐包下载导入（targets 已按依赖顺序排列）
                for (var i = 0; i < targets.Count; i++)
                {
                    var pkg = targets[i];
                    var assetUrl = snapshot.GetUnityPackageUrl(pkg.DirName);
                    var assetName = $"{pkg.DirName}-v{snapshot.Tag.TrimStart('v')}.unitypackage";

                    var progressBase = 0.1f + 0.8f * i / targets.Count;
                    var progressSpan = 0.8f / targets.Count;
                    SetProgress($"[{pkg.DirName}] 下载 {assetName} ...", progressBase);
                    var bytes = await AesirUpdateService.DownloadBytesAsync(assetUrl,
                        p => SetProgress($"[{pkg.DirName}] 下载 {assetName} ...", progressBase + progressSpan * p));

                    var tempDir = AesirUpdateService.ToAbsolutePath("Temp/AesirUpdate");
                    Directory.CreateDirectory(tempDir);
                    var tempFile = Path.Combine(tempDir, assetName);
                    File.WriteAllBytes(tempFile, bytes);

                    // 残留清理：仅当本次检测带清单且本地存在上次安装清单时有明确删除依据
                    var newEntry = snapshot.Info?.GetPackage(pkg.DirName);
                    if (newEntry != null)
                    {
                        var stale = AesirUpdateService.ComputeStaleFiles(
                            localManifest?.GetPackage(pkg.DirName)?.files,
                            newEntry.files,
                            pkg.AssetsPath);
                        var deleted = AesirUpdateService.DeleteStaleEntries(stale);
                        AesirUpdateService.PruneEmptyDirectories(pkg.AssetsPath);
                        if (deleted > 0)
                        {
                            Debug.Log($"[Aesir Updater] {pkg.DirName}: 清理 {deleted} 个残留条目");
                        }
                    }

                    SetProgress($"[{pkg.DirName}] 导入 {assetName} ...", progressBase + progressSpan * 0.95f);
                    AssetDatabase.ImportPackage(tempFile, false);
                    File.Delete(tempFile);

                    // 3. 逐包登记新清单：更新中途域重载时，已导入包的状态保证正确落盘
                    if (newEntry != null)
                    {
                        localManifest = AesirUpdateService.MergePackageEntry(localManifest, newEntry);
                        AesirUpdateService.SaveLocalManifest(localManifest);
                    }
                }

                _status = $"更新完成（{RemoteVersion}）。备份：{backupPath}";
                EditorUtility.DisplayDialog(ProgressTitle,
                    $"已更新到 {RemoteVersion}。\n\n本地修改已备份至：\n{backupPath}", "好");
                Debug.Log($"[Aesir Updater] {_status}");
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

        #region 辅助

        /// <summary>本地已安装包的最高版本（两包版本由 CI 强制一致，此处仍按最高取值兜底）。</summary>
        string GetMaxLocalVersion()
        {
            string max = null;
            foreach (var pkg in _packages)
            {
                if (max == null || AesirUpdateService.CompareVersion(pkg.Version, max) > 0)
                {
                    max = pkg.Version;
                }
            }

            return max ?? "0.0.0";
        }

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
