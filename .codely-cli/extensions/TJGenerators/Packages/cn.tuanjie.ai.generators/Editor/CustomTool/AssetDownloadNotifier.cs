using Codely.Newtonsoft.Json.Linq;

#if UNITY_EDITOR
using System;
using TJGenerators.AssetSearch;
using UnityEditor;
#endif

namespace UnityTcp.Editor.Tools
{
    /// <summary>
    /// 订阅 AssetDownloadService.TaskUpdated，在任务进入终态时
    /// 通过 GenerationNotifier 向 Codely CLI 推送 bg_task_done 通知。
    /// Skipped 状态跳过：download_asset 响应体已同步携带 prefab_path，无需异步通知。
    /// </summary>
    internal static class AssetDownloadNotifier
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            AssetDownloadService.TaskUpdated += OnTaskUpdated;
        }

        private static void OnTaskUpdated(DownloadTaskInfo task)
        {
            if (task == null) return;

            switch (task.Status)
            {
                case DownloadTaskStatus.Completed:
                    SendCompleted(task);
                    break;
                case DownloadTaskStatus.Failed:
                    SendFailed(task, task.ErrorMessage ?? "Unknown error");
                    break;
                case DownloadTaskStatus.Interrupted:
                    SendFailed(task,
                        "interrupted: domain reload killed the download — " +
                        "retry download_asset with the same query_id + asset_ids (dedup is safe)");
                    break;
                // Downloading / Importing / Skipped：不推送通知
            }
        }

        private static void SendCompleted(DownloadTaskInfo task)
        {
            var extra = new JObject
            {
                ["asset_id"]         = task.AssetId      ?? "",
                ["name"]             = task.AssetName    ?? "",
                ["prefab_path"]      = task.PrefabPath   ?? "",
                ["metadata_path"]    = task.MetadataPath ?? "",
                ["category"]         = task.Category     ?? "",
                ["source"]           = task.Source       ?? "",
                ["start_time"]       = task.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                ["end_time"]         = (task.EndTime ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"),
                ["duration_seconds"] = task.EndTime.HasValue
                    ? (int)(task.EndTime.Value - task.StartTime).TotalSeconds : 0,
            };
            GenerationNotifier.NotifyCompleted(
                toolName:      "download_asset",
                taskId:        task.TaskId,
                backendTaskId: task.AssetId ?? "",
                extraData:     extra);
        }

        private static void SendFailed(DownloadTaskInfo task, string errorMessage)
        {
            var extra = new JObject
            {
                ["asset_id"]         = task.AssetId   ?? "",
                ["name"]             = task.AssetName ?? "",
                ["start_time"]       = task.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                ["end_time"]         = (task.EndTime ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"),
                ["duration_seconds"] = task.EndTime.HasValue
                    ? (int)(task.EndTime.Value - task.StartTime).TotalSeconds : 0,
            };
            GenerationNotifier.NotifyFailed(
                toolName:      "download_asset",
                taskId:        task.TaskId,
                backendTaskId: task.AssetId ?? "",
                errorMessage:  errorMessage,
                extraData:     extra);
        }
#endif
    }
}
