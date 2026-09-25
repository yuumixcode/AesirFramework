using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Codely.Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;

#if UNITY_EDITOR
using TJGenerators;
using TJGenerators.PostProcessing;
using TJGenerators.Utils;
using Unity.EditorCoroutines.Editor;
#endif

namespace UnityTcp.Editor.Tools
{
    /// <summary>
    /// 视频导入任务追踪（持久化，域重载后可幂等重跑：URL 永久、目标路径确定性）。
    /// </summary>
    public static class VideoImportTaskTracker
    {
#if UNITY_EDITOR
        [Serializable]
        private class PersistedTask
        {
            public string taskId;
            public string sessionId;
            public string videoUrl;
            public string outputPath;
            public string prompt;
            public string status;
            public int    progress;
            public bool   isChromaKey;
            public string placeholderPath;
            public string videoPath;
            public string materialPath;
            public string historyTaskId;
            public string errorMessage;
            public long   startTimeTicks;
            public long   endTimeTicks;
        }

        public class VideoImportTaskInfo : IGenerationTaskInfo
        {
            public string TaskId        { get; set; }
            public string SessionId     { get; set; }
            public string VideoUrl      { get; set; }
            public string OutputPath    { get; set; }
            public string Prompt        { get; set; }
            public string Status        { get; set; }
            public int    Progress      { get; set; }
            public bool   IsChromaKey   { get; set; }
            public string PlaceholderPath { get; set; }
            public string VideoPath     { get; set; }
            public string MaterialPath  { get; set; }
            public string HistoryTaskId { get; set; }
            public string ErrorMessage  { get; set; }
            public DateTime StartTime   { get; set; }
            public DateTime? EndTime    { get; set; }
            public string BackendTaskId { get; set; }
            public string PreviewUrl   { get; set; }
        }

        private static readonly GenerationTaskTrackerStore<VideoImportTaskInfo, PersistedTask> Store =
            new GenerationTaskTrackerStore<VideoImportTaskInfo, PersistedTask>(
                "TJGen_ImportVideo", BuildPersisted, FromPersisted);

        private static PersistedTask BuildPersisted(VideoImportTaskInfo info) => new PersistedTask
        {
            taskId          = info.TaskId ?? "",
            sessionId       = info.SessionId ?? "",
            videoUrl        = info.VideoUrl ?? "",
            outputPath      = info.OutputPath ?? "",
            prompt          = info.Prompt ?? "",
            status          = info.Status ?? "",
            progress        = info.Progress,
            isChromaKey     = info.IsChromaKey,
            placeholderPath = info.PlaceholderPath ?? "",
            videoPath       = info.VideoPath ?? "",
            materialPath    = info.MaterialPath ?? "",
            historyTaskId   = info.HistoryTaskId ?? "",
            errorMessage    = info.ErrorMessage ?? "",
            startTimeTicks  = info.StartTime.Ticks,
            endTimeTicks    = info.EndTime?.Ticks ?? 0
        };

        private static VideoImportTaskInfo FromPersisted(PersistedTask p) => new VideoImportTaskInfo
        {
            TaskId          = p.taskId,
            SessionId       = p.sessionId,
            VideoUrl        = p.videoUrl,
            OutputPath      = p.outputPath,
            Prompt          = p.prompt,
            Status          = p.status,
            Progress        = p.progress,
            IsChromaKey     = p.isChromaKey,
            PlaceholderPath = p.placeholderPath,
            VideoPath       = p.videoPath,
            MaterialPath    = p.materialPath,
            HistoryTaskId   = p.historyTaskId,
            ErrorMessage    = p.errorMessage,
            StartTime       = new DateTime(p.startTimeTicks),
            EndTime         = p.endTimeTicks > 0 ? (DateTime?)new DateTime(p.endTimeTicks) : null,
            // 运行时派生字段不持久化：完成时由 CompleteTask 重算，防陈旧值泄漏
            BackendTaskId   = "",
            PreviewUrl      = ""
        };

        internal static void ApplyTaskUpdate(VideoImportTaskInfo task, Action<VideoImportTaskInfo> mutate) =>
            Store.ApplyTaskUpdate(task, mutate);

        internal static void RegisterTask(VideoImportTaskInfo task) =>
            Store.RegisterTask(task.TaskId, task);

        internal static string AllocateTaskId() => Store.AllocateTaskId("import_video");

        public static VideoImportTaskInfo GetTask(string taskId) => Store.GetTask(taskId);
        public static List<VideoImportTaskInfo> GetAllTasks() => Store.GetAllTasks();
#endif
    }

    /// <summary>
    /// 音频导入任务追踪（持久化，域重载后可幂等重跑）。
    /// </summary>
    public static class AudioImportTaskTracker
    {
#if UNITY_EDITOR
        [Serializable]
        private class PersistedTask
        {
            public string taskId;
            public string sessionId;
            public string audioUrl;
            public string outputPath;
            public string prompt;
            public string status;
            public int    progress;
            public bool   isBgm;
            public bool   playOnAwake;
            public string placeholderPath;
            public string audioPath;
            public int    updatedAudioSources;
            public bool   createdBgmPlayer;
            public string historyTaskId;
            public string errorMessage;
            public long   startTimeTicks;
            public long   endTimeTicks;
        }

        public class AudioImportTaskInfo : IGenerationTaskInfo
        {
            public string TaskId        { get; set; }
            public string SessionId     { get; set; }
            public string AudioUrl      { get; set; }
            public string OutputPath    { get; set; }
            public string Prompt        { get; set; }
            public string Status        { get; set; }
            public int    Progress      { get; set; }
            public bool   IsBgm         { get; set; }
            public bool   PlayOnAwake   { get; set; }
            public string PlaceholderPath { get; set; }
            public string AudioPath     { get; set; }
            public int    UpdatedAudioSources { get; set; }
            public bool   CreatedBgmPlayer    { get; set; }
            public string HistoryTaskId { get; set; }
            public string ErrorMessage  { get; set; }
            public DateTime StartTime   { get; set; }
            public DateTime? EndTime    { get; set; }
            public string BackendTaskId { get; set; }
            public string PreviewUrl   { get; set; }
        }

        private static readonly GenerationTaskTrackerStore<AudioImportTaskInfo, PersistedTask> Store =
            new GenerationTaskTrackerStore<AudioImportTaskInfo, PersistedTask>(
                "TJGen_ImportAudio", BuildPersisted, FromPersisted);

        private static PersistedTask BuildPersisted(AudioImportTaskInfo info) => new PersistedTask
        {
            taskId          = info.TaskId ?? "",
            sessionId       = info.SessionId ?? "",
            audioUrl        = info.AudioUrl ?? "",
            outputPath      = info.OutputPath ?? "",
            prompt          = info.Prompt ?? "",
            status          = info.Status ?? "",
            progress        = info.Progress,
            isBgm           = info.IsBgm,
            playOnAwake     = info.PlayOnAwake,
            placeholderPath = info.PlaceholderPath ?? "",
            audioPath       = info.AudioPath ?? "",
            updatedAudioSources = info.UpdatedAudioSources,
            createdBgmPlayer    = info.CreatedBgmPlayer,
            historyTaskId   = info.HistoryTaskId ?? "",
            errorMessage    = info.ErrorMessage ?? "",
            startTimeTicks  = info.StartTime.Ticks,
            endTimeTicks    = info.EndTime?.Ticks ?? 0
        };

        private static AudioImportTaskInfo FromPersisted(PersistedTask p) => new AudioImportTaskInfo
        {
            TaskId          = p.taskId,
            SessionId       = p.sessionId,
            AudioUrl        = p.audioUrl,
            OutputPath      = p.outputPath,
            Prompt          = p.prompt,
            Status          = p.status,
            Progress        = p.progress,
            IsBgm           = p.isBgm,
            PlayOnAwake     = p.playOnAwake,
            PlaceholderPath = p.placeholderPath,
            AudioPath       = p.audioPath,
            UpdatedAudioSources = p.updatedAudioSources,
            CreatedBgmPlayer    = p.createdBgmPlayer,
            HistoryTaskId   = p.historyTaskId,
            ErrorMessage    = p.errorMessage,
            StartTime       = new DateTime(p.startTimeTicks),
            EndTime         = p.endTimeTicks > 0 ? (DateTime?)new DateTime(p.endTimeTicks) : null,
            BackendTaskId   = "",
            PreviewUrl      = ""
        };

        internal static void ApplyTaskUpdate(AudioImportTaskInfo task, Action<AudioImportTaskInfo> mutate) =>
            Store.ApplyTaskUpdate(task, mutate);

        internal static void RegisterTask(AudioImportTaskInfo task) =>
            Store.RegisterTask(task.TaskId, task);

        internal static string AllocateTaskId() => Store.AllocateTaskId("import_audio");

        public static AudioImportTaskInfo GetTask(string taskId) => Store.GetTask(taskId);
        public static List<AudioImportTaskInfo> GetAllTasks() => Store.GetAllTasks();
#endif
    }

    /// <summary>
    /// Replaces the removed Unity-side video/audio generation tools (generate_video /
    /// generate_effect_video / generate_audio_clip / generate_sound_effect / generate_tts / voice_clone):
    /// generation now runs on the backend MCP (generate_video / generate_effect_video / generate_music /
    /// generate_sound_effect / generate_tts + file_upload + check_task; voice_clone needs no landing —
    /// its result is a custom_voice_id string consumed by generate_tts). This tool owns download +
    /// import + post-processing for permanent CDN media URLs.
    /// </summary>
    public static class ImportMediaFromUrlTool
    {
        private const string VideoToolName = "import_video_from_url";
        private const string AudioToolName = "import_audio_from_url";

        // ------------------------------------------------------------------
        // import_video_from_url
        // ------------------------------------------------------------------
        [ExecuteCustomTool.CustomTool(VideoToolName,
            "Import an already-generated video from a permanent CDN URL into Unity as a VideoClip, with the " +
            "standard video post-processing chain (blank-mp4 placeholder with a stable GUID, in-place file " +
            "overwrite, TuanjieAI/Session labels, History record; optional ChromaKey material for green-screen " +
            "effect videos). URLs come from the MCP backend video flow (generate_video / generate_effect_video " +
            "-> check_task -> video URL .mp4), NOT from a local path. " +
            "Key parameters: video_url (permanent CDN URL, .mp4 only), output_path (optional asset save path; " +
            "default Assets/TJGenerators/History/ImportVideo/<url-hash>.mp4), prompt (optional History display " +
            "text), session_id, chroma_key (optional bool, default false — set true for green-screen effect " +
            "videos: after import, creates the ChromaKey material and sets up the effect player in scene; " +
            "completion then also returns material_path). " +
            "Existing files at the target path are overwritten in place (placeholder skipped to protect the file). " +
            "RELIABLE COMPLETION FLOW (the Unity-side <bg_task_done> push may never reach the session): after " +
            "placing the placeholder, run a foreground shell wait of at most 30 seconds in this subagent, " +
            "then call query_local_task ONCE with task_id. Do not finish the subagent while importing; " +
            "if it still reports 'importing', run another bounded foreground wait and query again. " +
            "Domain reload safe: in-flight imports are automatically re-launched (URL is permanent, target path deterministic).")]
        public static object ImportVideoFromUrl(JObject parameters)
        {
#if UNITY_EDITOR
            try
            {
                TJLog.Log($"[ImportMediaFromUrlTool] Import video with parameters: {parameters}");

                if (TJGeneratorsPlayModeGuard.IsActive)
                    return Fail(VideoToolName, TJGeneratorsPlayModeGuard.Message);

                string videoUrl = parameters["video_url"]?.ToString();
                if (string.IsNullOrWhiteSpace(videoUrl))
                    return Fail(VideoToolName, "'video_url' is required. Get it from the MCP backend: " +
                                "generate_video / generate_effect_video -> check_task -> video URL (.mp4).");

                string ext = GetUrlExtension(videoUrl);
                if (ext != ".mp4")
                    return Fail(VideoToolName, $"Unsupported video URL extension '{ext}'. Supported: .mp4 only. " +
                                $"URL: {videoUrl}");

                string outputPath = NormalizeOutputPath(parameters["output_path"]?.ToString(), videoUrl, ext);
                string prompt     = parameters["prompt"]?.ToString();
                string sessionId  = parameters["session_id"]?.ToString() ?? "";
                bool   chromaKey   = parameters["chroma_key"] != null && parameters["chroma_key"].ToObject<bool>();

                // 单视频：文件级占位（GUID 稳定，下载完成后原地覆盖文件内容）。
                // 目标文件已存在时不写占位，避免下载失败把原视频毁成空白片；下载完成后仍原位覆盖。
                string placeholderPath = outputPath;
                EnsureAssetFolder(Path.GetDirectoryName(placeholderPath));
                string absPlaceholder = PathUtils.ToAbsoluteAssetPath(placeholderPath);
                if (string.IsNullOrEmpty(absPlaceholder) || !File.Exists(absPlaceholder))
                {
                    string created = TJGeneratorsVideoUtils.CreateBlankVideoClip(placeholderPath);
                    if (string.IsNullOrEmpty(created) || !File.Exists(PathUtils.ToAbsoluteAssetPath(created)))
                        return Fail(VideoToolName, $"Failed to create placeholder at: {placeholderPath}");
                    placeholderPath = created;
                    AssetDatabase.ImportAsset(placeholderPath, ImportAssetOptions.ForceUpdate);
                    TJGeneratorsGenerationLabel.EnableLabel(TJGeneratorsAssetReference.FromPath(placeholderPath));
                    TJGeneratorsGenerationLabel.EnableSessionLabel(TJGeneratorsAssetReference.FromPath(placeholderPath), sessionId);
                }

                string historyTaskId = TJGeneratorsHistoryManager.AddGeneratingPlaceholder(
                    prompt ?? "", "", VideoToolName, true,
                    CustomToolHistoryBindings.HistoryGuidFromPlaceholderAssetPath(placeholderPath),
                    prompt, sessionId);

                var task = new VideoImportTaskTracker.VideoImportTaskInfo
                {
                    TaskId         = VideoImportTaskTracker.AllocateTaskId(),
                    SessionId      = sessionId,
                    VideoUrl       = videoUrl,
                    OutputPath     = outputPath,
                    Prompt         = prompt ?? "",
                    Status         = "importing",
                    Progress       = 0,
                    IsChromaKey    = chromaKey,
                    PlaceholderPath = placeholderPath,
                    HistoryTaskId  = historyTaskId,
                    StartTime      = DateTime.Now
                };
                VideoImportTaskTracker.RegisterTask(task);

                LaunchVideoImport(task, chromaKey);

                var result = new Dictionary<string, object>
                {
                    { "success",              true },
                    { "submission_success",   true },
                    { "task_id",              task.TaskId },
                    { "status",               "importing" },
                    { "chroma_key",           chromaKey },
                    { "estimated_wait_seconds", 60 },
                    { "notification_mode",    "query_status" },
                    { "message",
                        "Import started. STEP 1 (do now): The placeholder asset is at placeholder_path (a tiny blank MP4, GUID-stable) — you may reference it right away. " +
                        "STEP 2 (do now): Wait in this subagent with run_shell_command(run_in_background=false), using Start-Sleep / sleep for at most 30 seconds. " +
                        "Keep ownership of this import; a timer finishing is not import completion, and the Unity-side push may never arrive. " +
                        "STEP 3: Do not call complete_task or return final text while the import is pending. " +
                        "STEP 4 (after the foreground wait returns): Call query_local_task ONCE with this task_id to fetch video_path (and material_path when chroma_key=true); " +
                        "if it still reports 'importing', run another bounded foreground wait before querying again. " +
                        "The placeholder file content is replaced in place (GUID stable) — do NOT place again. " +
                        "*** Never wait indefinitely for a Unity-side notification; never loop query_local_task within one wake-up. ***" }
                };
                result["placeholder_path"] = placeholderPath;
                result["video_url"] = videoUrl;
                return result;
            }
            catch (Exception e)
            {
                TJLog.LogError($"[ImportMediaFromUrlTool] Video import error: {e}");
                return Fail(VideoToolName, $"Error: {e.Message}");
            }
#else
            return Fail(VideoToolName, "This tool only works in Unity Editor.");
#endif
        }

        public static object QueryVideoImportStatus(JObject parameters)
        {
#if UNITY_EDITOR
            try
            {
                string taskId = parameters["task_id"]?.ToString();
                if (string.IsNullOrEmpty(taskId))
                    return Fail("query_local_task", "'task_id' parameter is required");

                var task = VideoImportTaskTracker.GetTask(taskId);
                if (task == null)
                    return Fail("query_local_task",
                        $"Task '{taskId}' not found. It may have been cleaned up or Unity was fully restarted.");

                var result = new Dictionary<string, object>
                {
                    { "success",    true },
                    { "task_id",    task.TaskId },
                    { "status",     task.Status },
                    { "progress",   task.Progress },
                    { "start_time", task.StartTime.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                // 结果路径仅在 completed 后输出，避免下载中途被误读为"已完成"
                if (task.Status == "completed" && !string.IsNullOrEmpty(task.VideoPath))
                {
                    result["video_path"]   = task.VideoPath;
                    result["preview_url"]  = task.PreviewUrl ?? "";
                    if (!string.IsNullOrEmpty(task.MaterialPath))
                        result["material_path"] = task.MaterialPath;
                }

                if (!string.IsNullOrEmpty(task.PlaceholderPath)) result["placeholder_path"] = task.PlaceholderPath;
                if (!string.IsNullOrEmpty(task.ErrorMessage))    result["error"]            = task.ErrorMessage;
                if (task.EndTime.HasValue)
                {
                    result["end_time"]         = task.EndTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    result["duration_seconds"] = (int)(task.EndTime.Value - task.StartTime).TotalSeconds;
                }

                if (task.Status == "recovering")
                    result["hint"] = "The import was interrupted by a domain reload and is being re-launched automatically. Wait in this subagent for up to 30 seconds, then check this same task again.";

                if (task.Status == "completed" && !string.IsNullOrEmpty(task.VideoPath))
                    result["result_summary"] = string.IsNullOrEmpty(task.MaterialPath)
                        ? $"Import completed. Video: {task.VideoPath}."
                        : $"Import completed. Video: {task.VideoPath}, ChromaKey material: {task.MaterialPath}.";

                return result;
            }
            catch (Exception e)
            {
                TJLog.LogError($"[ImportMediaFromUrlTool] Video query error: {e}");
                return Fail("query_local_task", $"Error: {e.Message}");
            }
#else
            return Fail("query_local_task", "This tool only works in Unity Editor.");
#endif
        }

        // ------------------------------------------------------------------
        // import_audio_from_url
        // ------------------------------------------------------------------
        [ExecuteCustomTool.CustomTool(AudioToolName,
            "Import an already-generated audio clip from a permanent CDN URL into Unity as an AudioClip, with the " +
            "standard audio post-processing chain (silent-WAV placeholder for pre-wiring, in-place file overwrite " +
            "for .wav, AudioSources referencing the placeholder are automatically re-bound to the real clip, " +
            "optional BGMPlayer auto-creation, TuanjieAI/Session labels, History record). " +
            "URLs come from the MCP backend audio flow (generate_music / generate_sound_effect / generate_tts " +
            "-> check_task -> audioUrl), NOT from a local path. " +
            "Key parameters: audio_url (permanent CDN URL, .wav / .mp3 only — Unity cannot import .aac / .flac / .m4a; " +
            "if the URL is another format, regenerate via MCP with output_format 'wav' or 'mp3'), " +
            "output_path (optional asset save path; default Assets/TJGenerators/History/ImportAudio/<url-hash>.<ext>), " +
            "prompt (optional History display text), session_id, " +
            "is_bgm (optional bool, default false — set true for background music: when no AudioSource in the scene " +
            "references the clip, auto-creates/reuses a BGMPlayer with loop + 2D mix), " +
            "play_on_awake (optional bool, defaults to is_bgm). " +
            "Existing files at the target path are overwritten in place (placeholder skipped to protect the file). " +
            "RELIABLE COMPLETION FLOW: after placing the placeholder, run a foreground shell wait of at most 30 seconds in this subagent " +
            "then call query_local_task ONCE with task_id. Do not finish the subagent while importing; " +
            "if it still reports 'importing', run another bounded foreground wait and query again. " +
            "Domain reload safe: in-flight imports are automatically re-launched.")]
        public static object ImportAudioFromUrl(JObject parameters)
        {
#if UNITY_EDITOR
            try
            {
                TJLog.Log($"[ImportMediaFromUrlTool] Import audio with parameters: {parameters}");

                if (TJGeneratorsPlayModeGuard.IsActive)
                    return Fail(AudioToolName, TJGeneratorsPlayModeGuard.Message);

                string audioUrl = parameters["audio_url"]?.ToString();
                if (string.IsNullOrWhiteSpace(audioUrl))
                    return Fail(AudioToolName, "'audio_url' is required. Get it from the MCP backend: " +
                                "generate_music / generate_sound_effect / generate_tts -> check_task -> audioUrl.");

                string ext = GetUrlExtension(audioUrl);
                if (ext != ".wav" && ext != ".mp3")
                    return Fail(AudioToolName, $"Unsupported audio URL extension '{ext}'. Supported: .wav / .mp3. " +
                                "(Unity cannot import .aac / .flac / .m4a — regenerate via MCP with output_format 'wav' or 'mp3'.) URL: {audioUrl}");

                string outputPath = NormalizeOutputPath(parameters["output_path"]?.ToString(), audioUrl, ext);
                string prompt     = parameters["prompt"]?.ToString();
                string sessionId  = parameters["session_id"]?.ToString() ?? "";
                bool   isBgm      = parameters["is_bgm"] != null && parameters["is_bgm"].ToObject<bool>();
                bool   playOnAwake = parameters["play_on_awake"] != null
                    ? parameters["play_on_awake"].ToObject<bool>()
                    : isBgm;

                // 占位静音 WAV（CreateBlankAudioClip 对 .mp3 目标会物化 .wav 同名占位，真实 mp3 下载后删除占位）。
                // 目标文件已存在时不写占位，避免下载失败把原音频毁成静音；下载完成后仍原位覆盖。
                string placeholderPath = outputPath;
                EnsureAssetFolder(Path.GetDirectoryName(outputPath));
                string absOutput = PathUtils.ToAbsoluteAssetPath(outputPath);
                if (string.IsNullOrEmpty(absOutput) || !File.Exists(absOutput))
                {
                    string created = TJGeneratorsAudioUtils.CreateBlankAudioClip(outputPath);
                    if (string.IsNullOrEmpty(created))
                        return Fail(AudioToolName, $"Failed to create placeholder at: {outputPath}");
                    placeholderPath = created;
                    AssetDatabase.ImportAsset(placeholderPath, ImportAssetOptions.ForceUpdate);
                    TJGeneratorsGenerationLabel.EnableLabel(TJGeneratorsAssetReference.FromPath(placeholderPath));
                    TJGeneratorsGenerationLabel.EnableSessionLabel(TJGeneratorsAssetReference.FromPath(placeholderPath), sessionId);
                }

                string historyTaskId = TJGeneratorsHistoryManager.AddGeneratingPlaceholder(
                    prompt ?? "", "", AudioToolName, true,
                    CustomToolHistoryBindings.HistoryGuidFromPlaceholderAssetPath(placeholderPath),
                    prompt, sessionId);

                var task = new AudioImportTaskTracker.AudioImportTaskInfo
                {
                    TaskId         = AudioImportTaskTracker.AllocateTaskId(),
                    SessionId      = sessionId,
                    AudioUrl       = audioUrl,
                    OutputPath     = outputPath,
                    Prompt         = prompt ?? "",
                    Status         = "importing",
                    Progress       = 0,
                    IsBgm          = isBgm,
                    PlayOnAwake    = playOnAwake,
                    PlaceholderPath = placeholderPath,
                    HistoryTaskId  = historyTaskId,
                    StartTime      = DateTime.Now
                };
                AudioImportTaskTracker.RegisterTask(task);

                LaunchAudioImport(task);

                var result = new Dictionary<string, object>
                {
                    { "success",              true },
                    { "submission_success",   true },
                    { "task_id",              task.TaskId },
                    { "status",               "importing" },
                    { "is_bgm",               isBgm },
                    { "play_on_awake",        playOnAwake },
                    { "estimated_wait_seconds", 30 },
                    { "notification_mode",    "query_status" },
                    { "message",
                        "Import started. STEP 1 (do now): The placeholder asset is at placeholder_path (a silent WAV, available immediately) — you may assign it to an AudioSource right away; it will be re-bound to the real clip automatically on completion. " +
                        "STEP 2 (do now): Wait in this subagent with run_shell_command(run_in_background=false), using Start-Sleep / sleep for at most 30 seconds. " +
                        "Keep ownership of this import; a timer finishing is not import completion, and the Unity-side push may never arrive. " +
                        "STEP 3: Do not call complete_task or return final text while the import is pending. " +
                        "STEP 4 (after the foreground wait returns): Call query_local_task ONCE with this task_id to fetch audio_path (+ updated_audio_sources / created_bgm_player); " +
                        "if it still reports 'importing', run another bounded foreground wait before querying again. " +
                        (string.Equals(placeholderPath, outputPath, StringComparison.OrdinalIgnoreCase)
                            ? "The placeholder file content is replaced in place (GUID stable) — do NOT place again. "
                            : "The real audio lands at output_path; the placeholder is deleted automatically. ") +
                        "*** Never wait indefinitely for a Unity-side notification; never loop query_local_task within one wake-up. ***" }
                };
                result["placeholder_path"] = placeholderPath;
                result["output_path"] = outputPath;
                result["audio_url"] = audioUrl;
                return result;
            }
            catch (Exception e)
            {
                TJLog.LogError($"[ImportMediaFromUrlTool] Audio import error: {e}");
                return Fail(AudioToolName, $"Error: {e.Message}");
            }
#else
            return Fail(AudioToolName, "This tool only works in Unity Editor.");
#endif
        }

        public static object QueryAudioImportStatus(JObject parameters)
        {
#if UNITY_EDITOR
            try
            {
                string taskId = parameters["task_id"]?.ToString();
                if (string.IsNullOrEmpty(taskId))
                    return Fail("query_local_task", "'task_id' parameter is required");

                var task = AudioImportTaskTracker.GetTask(taskId);
                if (task == null)
                    return Fail("query_local_task",
                        $"Task '{taskId}' not found. It may have been cleaned up or Unity was fully restarted.");

                var result = new Dictionary<string, object>
                {
                    { "success",    true },
                    { "task_id",    task.TaskId },
                    { "status",     task.Status },
                    { "progress",   task.Progress },
                    { "start_time", task.StartTime.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                if (task.Status == "completed" && !string.IsNullOrEmpty(task.AudioPath))
                {
                    result["audio_path"]  = task.AudioPath;
                    result["preview_url"] = task.PreviewUrl ?? "";
                    result["updated_audio_sources"] = task.UpdatedAudioSources;
                    result["created_bgm_player"]   = task.CreatedBgmPlayer;
                }

                if (!string.IsNullOrEmpty(task.PlaceholderPath)) result["placeholder_path"] = task.PlaceholderPath;
                if (!string.IsNullOrEmpty(task.ErrorMessage))    result["error"]            = task.ErrorMessage;
                if (task.EndTime.HasValue)
                {
                    result["end_time"]         = task.EndTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    result["duration_seconds"] = (int)(task.EndTime.Value - task.StartTime).TotalSeconds;
                }

                if (task.Status == "recovering")
                    result["hint"] = "The import was interrupted by a domain reload and is being re-launched automatically. Wait in this subagent for up to 30 seconds, then check this same task again.";

                if (task.Status == "completed" && !string.IsNullOrEmpty(task.AudioPath))
                    result["result_summary"] = $"Import completed. Audio: {task.AudioPath}.";

                return result;
            }
            catch (Exception e)
            {
                TJLog.LogError($"[ImportMediaFromUrlTool] Audio query error: {e}");
                return Fail("query_local_task", $"Error: {e.Message}");
            }
#else
            return Fail("query_local_task", "This tool only works in Unity Editor.");
#endif
        }

        // ------------------------------------------------------------------
        // import pipeline
        // ------------------------------------------------------------------
#if UNITY_EDITOR
        internal static void LaunchVideoImport(VideoImportTaskTracker.VideoImportTaskInfo task, bool chromaKey)
        {
            if (task == null || string.IsNullOrEmpty(task.VideoUrl)) return;
            EditorCoroutineUtility.StartCoroutineOwnerless(DownloadAndImportVideo(task, chromaKey));
        }

        private static IEnumerator DownloadAndImportVideo(VideoImportTaskTracker.VideoImportTaskInfo task, bool chromaKey)
        {
            string targetPath = ResolveTargetPath(task.VideoUrl, task.PlaceholderPath, task.OutputPath);

            using (var request = UnityWebRequest.Get(task.VideoUrl))
            {
                request.timeout = 600;
                yield return request.SendWebRequest();

                bool hasError = UnityWebRequestCompat.IsNotSuccess(request);
                if (hasError)
                {
                    MarkVideoFailed(task, $"Download failed ({request.error}): {task.VideoUrl}");
                    yield break;
                }

                try
                {
                    string absPath = PathUtils.ToAbsoluteAssetPath(targetPath);
                    string dir = Path.GetDirectoryName(absPath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    File.WriteAllBytes(absPath, request.downloadHandler.data);
                }
                catch (Exception e)
                {
                    MarkVideoFailed(task, $"Save failed: {e.Message}");
                    yield break;
                }
            }

            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            string sessionId = task.SessionId ?? "";
            TJGeneratorsGenerationLabel.EnableLabel(TJGeneratorsAssetReference.FromPath(targetPath));
            TJGeneratorsGenerationLabel.EnableSessionLabel(TJGeneratorsAssetReference.FromPath(targetPath), sessionId);

            // 绿幕特效视频：生成 ChromaKey 材质并在场景中布置特效播放器（与旧 generate_effect_video 链一致）
            string materialPath = "";
            if (chromaKey)
            {
                var postResult = GreenScreenVideoPostProcess.EnsureChromaKeyMaterial(targetPath);
                if (postResult.Success)
                {
                    materialPath = postResult.MaterialPath;
                    GreenScreenVideoPostProcess.SetupEffectVideoInScene(targetPath, materialPath);
                }
                else
                {
                    TJLog.LogError($"[ImportMediaFromUrlTool] ChromaKey material creation failed: {postResult.Error}");
                }
            }

            CompleteVideoTask(task, targetPath, materialPath);
        }

        private static void MarkVideoFailed(VideoImportTaskTracker.VideoImportTaskInfo task, string error)
        {
            VideoImportTaskTracker.ApplyTaskUpdate(task, t =>
            {
                t.Status       = "failed";
                t.ErrorMessage = error;
                t.EndTime      = DateTime.Now;
            });
            GenerationNotifier.NotifyFailed(VideoToolName, task.TaskId, "", error,
                new JObject { ["session_id"] = task.SessionId ?? "", ["prompt"] = task.Prompt ?? "" });
            TJLog.LogError($"[ImportMediaFromUrlTool] {error}");
        }

        private static void CompleteVideoTask(VideoImportTaskTracker.VideoImportTaskInfo task, string savedPath, string materialPath)
        {
            // 防重入：完成段为 at-least-once（域重载可在状态持久化前重跑），所有副作用幂等，重复调用直接返回
            if (task.Status == "completed") return;

            VideoImportTaskTracker.ApplyTaskUpdate(task, t =>
            {
                t.Status      = "completed";
                t.Progress    = 100;
                t.VideoPath   = savedPath;
                t.MaterialPath = materialPath ?? "";
                t.PreviewUrl  = task.VideoUrl;
                t.EndTime     = DateTime.Now;
            });

            if (!string.IsNullOrEmpty(task.HistoryTaskId))
                TJGeneratorsHistoryManager.CompletePlaceholder(task.HistoryTaskId, savedPath, task.PreviewUrl, null);

            var payload = new JObject
            {
                ["session_id"]       = task.SessionId ?? "",
                ["prompt"]           = task.Prompt ?? "",
                ["video_path"]       = savedPath,
                ["preview_url"]      = task.PreviewUrl ?? "",
                ["material_path"]    = materialPath ?? "",
                ["progress"]         = 100,
                ["start_time"]       = task.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                ["end_time"]         = task.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                ["duration_seconds"] = task.EndTime.HasValue
                    ? (int)(task.EndTime.Value - task.StartTime).TotalSeconds
                    : 0
            };

            GenerationNotifier.NotifyCompleted(VideoToolName, task.TaskId, "", payload);
            TJLog.Log($"[ImportMediaFromUrlTool] Video import completed: {savedPath}" +
                      (string.IsNullOrEmpty(materialPath) ? "" : $", ChromaKey material: {materialPath}"));
        }

        internal static void LaunchAudioImport(AudioImportTaskTracker.AudioImportTaskInfo task)
        {
            if (task == null || string.IsNullOrEmpty(task.AudioUrl)) return;
            EditorCoroutineUtility.StartCoroutineOwnerless(DownloadAndImportAudio(task));
        }

        private static IEnumerator DownloadAndImportAudio(AudioImportTaskTracker.AudioImportTaskInfo task)
        {
            string targetPath = ResolveTargetPath(task.AudioUrl, task.PlaceholderPath, task.OutputPath);

            using (var request = UnityWebRequest.Get(task.AudioUrl))
            {
                request.timeout = 300;
                yield return request.SendWebRequest();

                bool hasError = UnityWebRequestCompat.IsNotSuccess(request);
                if (hasError)
                {
                    MarkAudioFailed(task, $"Download failed ({request.error}): {task.AudioUrl}");
                    yield break;
                }

                try
                {
                    string absPath = PathUtils.ToAbsoluteAssetPath(targetPath);
                    string dir = Path.GetDirectoryName(absPath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    File.WriteAllBytes(absPath, request.downloadHandler.data);
                }
                catch (Exception e)
                {
                    MarkAudioFailed(task, $"Save failed: {e.Message}");
                    yield break;
                }
            }

            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            string sessionId = task.SessionId ?? "";
            TJGeneratorsGenerationLabel.EnableLabel(TJGeneratorsAssetReference.FromPath(targetPath));
            TJGeneratorsGenerationLabel.EnableSessionLabel(TJGeneratorsAssetReference.FromPath(targetPath), sessionId);

            CompleteAudioTask(task, targetPath);
        }

        private static void MarkAudioFailed(AudioImportTaskTracker.AudioImportTaskInfo task, string error)
        {
            AudioImportTaskTracker.ApplyTaskUpdate(task, t =>
            {
                t.Status       = "failed";
                t.ErrorMessage = error;
                t.EndTime      = DateTime.Now;
            });
            GenerationNotifier.NotifyFailed(AudioToolName, task.TaskId, "", error,
                new JObject { ["session_id"] = task.SessionId ?? "", ["prompt"] = task.Prompt ?? "" });
            TJLog.LogError($"[ImportMediaFromUrlTool] {error}");
        }

        private static void CompleteAudioTask(AudioImportTaskTracker.AudioImportTaskInfo task, string savedPath)
        {
            // 防重入：完成段为 at-least-once（域重载可在状态持久化前重跑），所有副作用幂等，重复调用直接返回
            if (task.Status == "completed") return;

            int updatedSources = 0;
            bool createdBgmPlayer = false;

            var newClip = TJGeneratorsAudioUtils.TryLoadAudioClip(savedPath);
            if (newClip == null)
            {
                // 导入失败不能静默吞掉：按失败上报，避免场景里留着占位静音 clip 被误当成品
                MarkAudioFailed(task, $"Cannot import '{savedPath}' as AudioClip. Verify the URL content matches its extension.");
                return;
            }

            // 与旧 AudioPipelineHost.OnAssetSaved 一致：占位与落盘同路径（.wav 原位覆盖）时按引用+按路径双匹配，
            // 因为重导入后 Unity 可能返回新的托管对象而 AudioSource 仍持有旧引用。
            updatedSources = RebindAudioSources(task.PlaceholderPath, savedPath, newClip, task.PlayOnAwake);

            if (task.IsBgm && updatedSources == 0)
                createdBgmPlayer = EnsureBgmPlayer(newClip, task.PlayOnAwake);

            // 真实音频与占位不同路径（.mp3 场景）时，删除占位资产
            bool pathsAreSame = string.Equals(task.PlaceholderPath, savedPath, StringComparison.OrdinalIgnoreCase);
            if (!pathsAreSame &&
                !string.IsNullOrEmpty(task.PlaceholderPath) &&
                AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(task.PlaceholderPath) != null)
            {
                AssetDatabase.DeleteAsset(task.PlaceholderPath);
            }

            AudioImportTaskTracker.ApplyTaskUpdate(task, t =>
            {
                t.Status              = "completed";
                t.Progress            = 100;
                t.AudioPath           = savedPath;
                t.UpdatedAudioSources = updatedSources;
                t.CreatedBgmPlayer     = createdBgmPlayer;
                t.PreviewUrl          = task.AudioUrl;
                t.EndTime             = DateTime.Now;
            });

            if (!string.IsNullOrEmpty(task.HistoryTaskId))
            {
                // MP3/OGG imports replace a WAV placeholder with a different asset GUID.
                // Rebind only this task's history; other generations may share the old target.
                var historyItem = TJGeneratorsHistoryManager.LoadHistory().Find(h => h.taskId == task.HistoryTaskId);
                if (historyItem != null)
                    TJGeneratorsHistoryManager.RewriteAssetGuid(historyItem.assetGuid,
                        AssetDatabase.AssetPathToGUID(savedPath), h => h.taskId == task.HistoryTaskId);
                TJGeneratorsHistoryManager.CompletePlaceholder(task.HistoryTaskId, savedPath, task.PreviewUrl, null);
            }

            var payload = new JObject
            {
                ["session_id"]       = task.SessionId ?? "",
                ["prompt"]           = task.Prompt ?? "",
                ["audio_path"]       = savedPath,
                ["preview_url"]      = task.PreviewUrl ?? "",
                ["is_bgm"]           = task.IsBgm,
                ["updated_audio_sources"] = updatedSources,
                ["created_bgm_player"]    = createdBgmPlayer,
                ["progress"]         = 100,
                ["start_time"]       = task.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                ["end_time"]         = task.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                ["duration_seconds"] = task.EndTime.HasValue
                    ? (int)(task.EndTime.Value - task.StartTime).TotalSeconds
                    : 0
            };

            GenerationNotifier.NotifyCompleted(AudioToolName, task.TaskId, "", payload);
            TJLog.Log($"[ImportMediaFromUrlTool] Audio import completed: {savedPath} " +
                      $"(rebound {updatedSources} AudioSource(s){(createdBgmPlayer ? ", created BGMPlayer" : "")})");
        }

        /// <summary>
        /// 把引用占位 clip 的 AudioSource 全部换绑到真实 clip（引用 + 路径双匹配）。返回更新数。
        /// </summary>
        private static int RebindAudioSources(string placeholderPath, string savedPath, AudioClip newClip, bool playOnAwake)
        {
            int updated = 0;
            var oldClip = TJGeneratorsAudioUtils.TryLoadAudioClip(placeholderPath);
            bool pathsAreSame = string.Equals(placeholderPath, savedPath, StringComparison.OrdinalIgnoreCase);

            foreach (var source in UnityObjectCompat.FindObjectsOfType<AudioSource>())
            {
                if (source.clip == null) continue;
                bool matchByRef = oldClip != null && source.clip == oldClip;
                bool matchByPath = pathsAreSame && string.Equals(
                    AssetDatabase.GetAssetPath(source.clip), placeholderPath, StringComparison.OrdinalIgnoreCase);
                if (!matchByRef && !matchByPath) continue;

                source.clip = newClip;
                source.playOnAwake = playOnAwake;
                EditorUtility.SetDirty(source);
                updated++;
            }

            if (updated > 0)
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            return updated;
        }

        /// <summary>is_bgm 且场景中无引用时，创建/复用 BGMPlayer（loop + 2D），与旧 BGM 链行为一致。</summary>
        private static bool EnsureBgmPlayer(AudioClip clip, bool playOnAwake)
        {
            var go = GameObject.Find("BGMPlayer");
            bool isNew = go == null;
            if (isNew)
            {
                go = new GameObject("BGMPlayer");
                Undo.RegisterCreatedObjectUndo(go, "Import BGM AudioSource");
            }

            var bgmSource = go.GetComponent<AudioSource>();
            if (bgmSource == null)
                bgmSource = Undo.AddComponent<AudioSource>(go);

            Undo.RecordObject(bgmSource, "Import BGM Clip");
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.spatialBlend = 0f;
            bgmSource.playOnAwake = playOnAwake;
            EditorUtility.SetDirty(bgmSource);
            EditorUtility.SetDirty(go);
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            TJLog.Log($"[ImportMediaFromUrlTool] {(isNew ? "Created" : "Reused")} BGMPlayer for: {clip.name}");
            return isNew;
        }

        // ------------------------------------------------------------------
        // helpers
        // ------------------------------------------------------------------

        private static string GetUrlExtension(string url)
        {
            try
            {
                return Path.GetExtension(new Uri(url).AbsolutePath)?.ToLowerInvariant();
            }
            catch
            {
                return "";
            }
        }

        /// <summary>目标路径：优先 output_path（扩展名强制为 URL 扩展，防内容与扩展错配），缺省 History/<媒介>/<url-hash>。</summary>
        private static string NormalizeOutputPath(string outputPath, string url, string urlExt)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                string folder = urlExt == ".mp4"
                    ? "Assets/TJGenerators/History/ImportVideo"
                    : "Assets/TJGenerators/History/ImportAudio";
                return folder + "/" + Import3DModelFromUrlTool.BuildUrlHashFileName(url, urlExt);
            }

            outputPath = outputPath.Replace("\\", "/");
            if (!outputPath.StartsWith("Assets/"))
                outputPath = "Assets/" + outputPath.TrimStart('/');
            return Path.ChangeExtension(outputPath, urlExt);
        }

        private static string ResolveTargetPath(string url, string placeholderPath, string outputPath)
        {
            string ext = GetUrlExtension(url);
            // 入口已校验扩展（.mp4 / .wav / .mp3），此处兜底仅供防御
            if (string.IsNullOrEmpty(ext)) ext = ".mp4";

            if (!string.IsNullOrEmpty(placeholderPath))
            {
                // 占位与目标同扩展（.wav 原位覆盖 / .mp4 原位覆盖）时直接用占位路径，GUID 稳定
                if (string.Equals(Path.GetExtension(placeholderPath), ext, StringComparison.OrdinalIgnoreCase))
                    return placeholderPath;
            }
            if (!string.IsNullOrEmpty(outputPath))
                return Path.ChangeExtension(outputPath, ext);

            string folder = ext == ".mp4" ? "Assets/TJGenerators/History/ImportVideo" : "Assets/TJGenerators/History/ImportAudio";
            return folder + "/" + Import3DModelFromUrlTool.BuildUrlHashFileName(url, ext);
        }

        private static void EnsureAssetFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder)) return;
            string abs = PathUtils.ToAbsoluteAssetPath(folder);
            if (!string.IsNullOrEmpty(abs) && !Directory.Exists(abs))
                Directory.CreateDirectory(abs);
        }

        private static Dictionary<string, object> Fail(string toolName, string message)
        {
            return new Dictionary<string, object>
            {
                { "success", false },
                { "tool",    toolName },
                { "message", message }
            };
        }
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// Re-launches in-flight video/audio import tasks after a domain reload.
    /// Media URLs are permanent CDN links and target paths are deterministic,
    /// so re-running the import simply overwrites in place.
    /// </summary>
    [InitializeOnLoad]
    public static class ImportMediaDomainReloadRecovery
    {
        static ImportMediaDomainReloadRecovery()
        {
            CustomToolDomainReloadRecovery.Schedule(ResumeInterruptedImports);
        }

        private static void ResumeInterruptedImports()
        {
            foreach (var task in VideoImportTaskTracker.GetAllTasks())
            {
                if (task == null) continue;
                if (task.Status != "importing" && task.Status != "recovering") continue;
                if (string.IsNullOrEmpty(task.VideoUrl)) continue;

                TJLog.Log($"[ImportMediaDomainReloadRecovery] Resuming video import task: {task.TaskId}");
                VideoImportTaskTracker.ApplyTaskUpdate(task, t => t.Status = "recovering");
                ImportMediaFromUrlTool.LaunchVideoImport(task, chromaKey: task.IsChromaKey);
            }

            foreach (var task in AudioImportTaskTracker.GetAllTasks())
            {
                if (task == null) continue;
                if (task.Status != "importing" && task.Status != "recovering") continue;
                if (string.IsNullOrEmpty(task.AudioUrl)) continue;

                TJLog.Log($"[ImportMediaDomainReloadRecovery] Resuming audio import task: {task.TaskId}");
                AudioImportTaskTracker.ApplyTaskUpdate(task, t => t.Status = "recovering");
                ImportMediaFromUrlTool.LaunchAudioImport(task);
            }
        }
    }
#endif
}
