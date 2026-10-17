using System;
using System.Collections;
using System.Collections.Generic;
using Codely.Newtonsoft.Json.Linq;
using UnityEditor;

#if UNITY_EDITOR
using TJGenerators;
using TJGenerators.Generators;
using TJGenerators.Config;
using TJGenerators.Pipeline;
using TJGenerators.Utils;
using Unity.EditorCoroutines.Editor;
#endif

namespace UnityTcp.Editor.Tools
{
    public static class ImageTaskTracker
    {
#if UNITY_EDITOR
        [Serializable]
        private class PersistedTask
        {
            public string taskId;
            public string generatorId;
            public string prompt;
            public string imagePath;
            public string status;
            public int    progress;
            public string resultPath;
            public string errorMessage;
            public long   startTimeTicks;
            public long   endTimeTicks;
            public string previewUrl;
            public string placeholderPath;
            public string backendTaskId;
        }

        public class ImageTaskInfo : IGenerationTaskInfo
        {
            public string TaskId { get; set; }
            public string GeneratorId { get; set; }
            public string Prompt { get; set; }
            public string ImagePath { get; set; }
            public string Status { get; set; }
            public int Progress { get; set; }
            public string ResultPath { get; set; }
            public string ErrorMessage { get; set; }
            public string PreviewUrl { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public string PlaceholderPath { get; set; }
            public string BackendTaskId { get; set; }
        }

        private static readonly GenerationTaskTrackerStore<ImageTaskInfo, PersistedTask> Store =
            new GenerationTaskTrackerStore<ImageTaskInfo, PersistedTask>(
                "TJGen_Image", BuildPersisted, FromPersisted);

        private static PersistedTask BuildPersisted(ImageTaskInfo info) => new PersistedTask
        {
            taskId          = info.TaskId,
            generatorId     = info.GeneratorId,
            prompt          = info.Prompt ?? "",
            imagePath       = info.ImagePath ?? "",
            status          = info.Status,
            progress        = info.Progress,
            resultPath      = info.ResultPath ?? "",
            errorMessage    = info.ErrorMessage ?? "",
            startTimeTicks  = info.StartTime.Ticks,
            endTimeTicks    = info.EndTime?.Ticks ?? 0,
            previewUrl      = info.PreviewUrl ?? "",
            placeholderPath = info.PlaceholderPath ?? "",
            backendTaskId   = info.BackendTaskId ?? ""
        };

        private static ImageTaskInfo FromPersisted(PersistedTask p) => new ImageTaskInfo
        {
            TaskId          = p.taskId,
            GeneratorId     = p.generatorId,
            Prompt          = p.prompt,
            ImagePath       = p.imagePath,
            Status          = p.status,
            Progress        = p.progress,
            ResultPath      = p.resultPath,
            ErrorMessage    = p.errorMessage,
            PreviewUrl      = p.previewUrl,
            StartTime       = new DateTime(p.startTimeTicks),
            EndTime         = p.endTimeTicks > 0 ? (DateTime?)new DateTime(p.endTimeTicks) : null,
            PlaceholderPath = p.placeholderPath,
            BackendTaskId   = p.backendTaskId
        };

        internal static void ApplyTaskUpdate(ImageTaskInfo task, Action<ImageTaskInfo> mutate) =>
            Store.ApplyTaskUpdate(task, mutate);

        public static string CreateTask(string generatorId, string prompt, string imagePath = null, string placeholderPath = null, string backendTaskId = null)
        {
            string taskId = Store.AllocateTaskId("image");
            var task = new ImageTaskInfo
            {
                TaskId          = taskId,
                GeneratorId     = generatorId,
                Prompt          = prompt ?? "",
                ImagePath       = imagePath ?? "",
                Status          = "generating",
                StartTime       = DateTime.Now,
                PlaceholderPath = placeholderPath,
                BackendTaskId   = backendTaskId
            };
            Store.RegisterTask(taskId, task);
            return taskId;
        }

        public static void MarkTaskCompleted(string taskId, string resultPath, string previewUrl = null)
        {
            var task = Store.GetTask(taskId);
            if (task == null) return;
            Store.ApplyTaskUpdate(task, t =>
            {
                t.Status     = "completed";
                t.Progress   = 100;
                t.ResultPath = resultPath;
                t.PreviewUrl = previewUrl;
                t.EndTime    = DateTime.Now;
            });
        }

        public static void MarkTaskFailed(string taskId, string errorMessage)
        {
            var task = Store.GetTask(taskId);
            if (task == null) return;
            Store.ApplyTaskUpdate(task, t =>
            {
                t.Status       = "failed";
                t.ErrorMessage = errorMessage;
                t.EndTime      = DateTime.Now;
            });
        }

        public static ImageTaskInfo GetTask(string taskId) => Store.GetTask(taskId);

        public static List<ImageTaskInfo> GetAllTasks() => Store.GetAllTasks();

        public static ImageTaskInfo GetTaskByBackendId(string backendTaskId) =>
            Store.GetTaskByBackendId(backendTaskId);

        public static ImageTaskInfo CreateRecoveredTask(
            string backendTaskId, string prompt, string placeholderPath, long timestampMs, string generatorId = null)
        {
            return Store.CreateRecoveredTask(backendTaskId, () => new ImageTaskInfo
            {
                TaskId          = $"recovered_{backendTaskId}",
                BackendTaskId   = backendTaskId,
                GeneratorId     = generatorId ?? "",
                Prompt          = prompt ?? "",
                PlaceholderPath = placeholderPath ?? "",
                Status          = "recovering",
                Progress        = 0,
                StartTime       = timestampMs > 0
                                    ? DateTimeOffset.FromUnixTimeMilliseconds(timestampMs).LocalDateTime
                                    : DateTime.Now
            });
        }

        public static void CleanupCompletedTasks() => Store.CleanupCompletedTasks();
#endif
    }

    /// <summary>
    /// Shared Unity-side image task surface used by generate_game_ui_kit and LocalTaskTools.
    /// Plain image generation / sprite-sequence CustomTools have moved to MCP
    /// (generate_image / generate_sprite_animation / generate_eight_dir_sprite).
    /// Keeps ImageTaskTracker, QueryImageStatus/ListImageTasks, and ImagePipelineHost.
    /// </summary>
    public static class GenerateImageTool
    {
        public static object QueryImageStatus(JObject parameters)
        {
#if UNITY_EDITOR
            try
            {
                string taskId = parameters["task_id"]?.ToString();

                if (string.IsNullOrEmpty(taskId))
                {
                    return new Dictionary<string, object>
                    {
                        { "success", false },
                        { "message", "'task_id' parameter is required" }
                    };
                }

                var task = ImageTaskTracker.GetTask(taskId);

                if (task == null)
                {
                    return new Dictionary<string, object>
                    {
                        { "success", false },
                        { "message", $"Task '{taskId}' not found. It may have been completed and cleaned up." }
                    };
                }

                var result = new Dictionary<string, object>
                {
                    { "success",      true },
                    { "task_id",      task.TaskId },
                    { "generator_id", task.GeneratorId },
                    { "status",       task.Status },
                    { "progress",     task.Progress },
                    { "prompt",       task.Prompt },
                    { "start_time",   task.StartTime.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                if (!string.IsNullOrEmpty(task.ImagePath))  result["input_image_path"] = task.ImagePath;
                if (!string.IsNullOrEmpty(task.ResultPath)) result["image_path"]        = task.ResultPath;
                result["preview_url"] = PreviewUrlHelper.GetPreviewUrl(task.PreviewUrl, task.BackendTaskId);
                if (!string.IsNullOrEmpty(task.ErrorMessage)) result["error"]           = task.ErrorMessage;

                if (task.EndTime.HasValue)
                {
                    result["end_time"]         = task.EndTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    result["duration_seconds"]  = (int)(task.EndTime.Value - task.StartTime).TotalSeconds;
                }

                if (task.Status == "generating")
                {
                    if (!string.IsNullOrEmpty(task.PlaceholderPath))
                        result["placeholder_path"] = task.PlaceholderPath;
                }

                return result;
            }
            catch (Exception e)
            {
                TJLog.LogError($"[GenerateImageTool] Query error: {e}");
                return new Dictionary<string, object>
                {
                    { "success", false },
                    { "message", $"Error querying task status: {e.Message}" }
                };
            }
#else
            return new Dictionary<string, object>
            {
                { "success", false },
                { "message", "This tool only works in Unity Editor." }
            };
#endif
        }

        public static object ListImageTasks(JObject parameters)
        {
#if UNITY_EDITOR
            try
            {
                var tasks    = ImageTaskTracker.GetAllTasks();
                var taskList = new List<Dictionary<string, object>>();

                foreach (var task in tasks)
                {
                    var taskData = new Dictionary<string, object>
                    {
                        { "task_id",      task.TaskId },
                        { "generator_id", task.GeneratorId },
                        { "status",       task.Status },
                        { "progress",     task.Progress },
                        { "prompt",       task.Prompt },
                        { "start_time",   task.StartTime.ToString("yyyy-MM-dd HH:mm:ss") }
                    };

                    if (!string.IsNullOrEmpty(task.ImagePath))    taskData["input_image_path"] = task.ImagePath;
                    if (!string.IsNullOrEmpty(task.ResultPath))   taskData["image_path"]        = task.ResultPath;
                    taskData["preview_url"] = PreviewUrlHelper.GetPreviewUrl(task.PreviewUrl, task.BackendTaskId);
                    if (!string.IsNullOrEmpty(task.ErrorMessage)) taskData["error"]             = task.ErrorMessage;
                    if (task.EndTime.HasValue) taskData["end_time"] = task.EndTime.Value.ToString("yyyy-MM-dd HH:mm:ss");

                    taskList.Add(taskData);
                }

                return new Dictionary<string, object>
                {
                    { "success", true },
                    { "count",   taskList.Count },
                    { "tasks",   taskList }
                };
            }
            catch (Exception e)
            {
                TJLog.LogError($"[GenerateImageTool] List error: {e}");
                return new Dictionary<string, object>
                {
                    { "success", false },
                    { "message", $"Error listing tasks: {e.Message}" }
                };
            }
#else
            return new Dictionary<string, object>
            {
                { "success", false },
                { "message", "This tool only works in Unity Editor." }
            };
#endif
        }

    }

#if UNITY_EDITOR
    /// <summary>
    /// Automatically resumes interrupted generate_image tasks after domain reload.
    /// </summary>
    [InitializeOnLoad]
    public static class ImageDomainReloadRecovery
    {
        static ImageDomainReloadRecovery()
        {
            CustomToolDomainReloadRecovery.Schedule(ResumeInterruptedTasks);
        }

        private static void ResumeInterruptedTasks()
        {
            CustomToolDomainReloadRecovery.Resume(
                "GenerateImageTool",
                ConfigType.Image,
                t => t.toolName == "generate_image",
                () => ImageTaskTracker.GetAllTasks(),
                (interrupted, _, generator) =>
                {
                    var trackerTask = ImageTaskTracker.GetTaskByBackendId(interrupted.backendTaskId);
                    if (trackerTask != null)
                    {
                        CustomToolDomainReloadRecovery.MarkTrackerRecoveringIfNeeded(trackerTask.Status, () =>
                        {
                            ImageTaskTracker.ApplyTaskUpdate(trackerTask, t => t.Status = "recovering");
                        });
                    }
                    else
                    {
                        string placeholderPath = CustomToolDomainReloadRecovery.ResolveAssetPath(interrupted.targetAssetGuid);
                        trackerTask = ImageTaskTracker.CreateRecoveredTask(
                            interrupted.backendTaskId, interrupted.prompt, placeholderPath, interrupted.timestamp,
                            interrupted.modelVersion);
                    }

                    string placeholderPathForHost = trackerTask.PlaceholderPath ?? "";
                    if (string.IsNullOrEmpty(placeholderPathForHost))
                        placeholderPathForHost = CustomToolDomainReloadRecovery.ResolveAssetPath(interrupted.targetAssetGuid);

                    var host = new ImageRecoveryHost(
                        placeholderPathForHost, interrupted.backendTaskId, interrupted.sessionId, generator);
                    CustomToolDomainReloadRecovery.StartPolling(
                        "GenerateImageTool", host, ConfigType.Image,
                        interrupted.sessionId, "generate_image", generator, interrupted.backendTaskId);
                });
        }
    }

    /// <summary>
    /// Headless pipeline host for resuming image tasks after domain reload.
    /// </summary>
    internal class ImageRecoveryHost : HeadlessPipelineHostBase, IMediaAssetPipelineHost
    {
        private readonly string _placeholderPath;
        private readonly TJGeneratorsAssetReference _placeholderRef;
        private readonly string _backendTaskId;
        private readonly string _sessionId;
        private readonly ModelGeneratorBase _generator;

        public ImageRecoveryHost(string placeholderPath, string backendTaskId, string sessionId, ModelGeneratorBase generator)
        {
            _placeholderPath = placeholderPath ?? "";
            _placeholderRef = string.IsNullOrEmpty(_placeholderPath)
                ? null
                : TJGeneratorsAssetReference.FromPath(_placeholderPath);
            _backendTaskId = backendTaskId;
            _sessionId = sessionId ?? "";
            _generator = generator;
        }

        protected override string DialogLogTag => "ImageRecovery";

        public override TJGeneratorsAssetReference GetTargetAsset() => _placeholderRef;

        public override void Repaint()
        {
            if (_generator == null) return;
            var trackerTask = ImageTaskTracker.GetTaskByBackendId(_backendTaskId);
            if (trackerTask == null || !TJGeneratorsTaskRecovery.IsRecoverableTrackerStatus(trackerTask.Status)) return;

            int progress = _generator.CurrentProgress;
            if (progress <= trackerTask.Progress) return;

            ImageTaskTracker.ApplyTaskUpdate(trackerTask, t =>
            {
                t.Status = "generating";
                t.Progress = progress;
            });
        }

        public override void ShowDialog(string title, string message)
        {
            base.ShowDialog(title, message);

            if (ErrorDialogUtils.IsErrorDialog(title))
            {
                var trackerTask = ImageTaskTracker.GetTaskByBackendId(_backendTaskId);
                if (trackerTask != null)
                {
                    var friendlyError = ErrorDialogUtils.ConvertToUserFriendlyError(title, message);
                    ImageTaskTracker.MarkTaskFailed(trackerTask.TaskId, friendlyError.TechnicalMessage);
                    GenerationNotifier.NotifyFailed(
                        "generate_image",
                        trackerTask.TaskId,
                        _backendTaskId,
                        friendlyError.TechnicalMessage,
                        new JObject
                        {
                            ["session_id"] = _sessionId,
                            ["generator_id"] = trackerTask.GeneratorId ?? "",
                            ["prompt"] = trackerTask.Prompt ?? ""
                        });
                }
            }
        }

        public string GetAssetSavePath(PipelineMediaType _type, ModelGeneratorBase generator) =>
            _type == PipelineMediaType.Texture ? _placeholderPath : null;

        public void OnAssetSaved(PipelineMediaType _type, string savePath, ModelGeneratorBase generator)
        {
            if (_type != PipelineMediaType.Texture) return;

            TJLog.Log($"[GenerateImageTool] Recovered image saved: {savePath}");

            GeneratedTextureImportUtils.ConfigureImportedTexture(
                savePath, TextureImporterType.Default, alphaIsTransparency: true);

            TJGeneratorsGenerationLabel.EnableLabel(TJGeneratorsAssetReference.FromPath(savePath));
            TJGeneratorsGenerationLabel.EnableSessionLabel(TJGeneratorsAssetReference.FromPath(savePath), _sessionId);

            var trackerTask = ImageTaskTracker.GetTaskByBackendId(_backendTaskId);
            if (trackerTask == null) return;

            string previewUrl = generator?.CurrentPreviewUrl;
            ImageTaskTracker.MarkTaskCompleted(trackerTask.TaskId, savePath, previewUrl);
            var t = ImageTaskTracker.GetTask(trackerTask.TaskId);
            GenerationNotifier.NotifyCompleted(
                "generate_image",
                trackerTask.TaskId,
                _backendTaskId,
                new JObject
                {
                    ["session_id"] = _sessionId,
                    ["generator_id"] = t?.GeneratorId ?? "",
                    ["prompt"] = t?.Prompt ?? "",
                    ["image_path"] = savePath ?? "",
                    ["preview_url"] = previewUrl ?? "",
                    ["progress"] = 100,
                    ["start_time"] = t?.StartTime.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                    ["end_time"] = t?.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                    ["duration_seconds"] = (t != null && t.EndTime.HasValue) ? (int)(t.EndTime.Value - t.StartTime).TotalSeconds : 0
                });
        }
    }

    /// <summary>
    /// IGenerationPipelineHost implementation for headless image generation via custom tools.
    /// Keeps TextureImporterType.Default (not Sprite) to match the Image window behavior.
    /// </summary>
    internal class ImagePipelineHost : HeadlessPipelineHostBase, IMediaAssetPipelineHost
    {
        private readonly string _placeholderPath;
        private readonly TJGeneratorsAssetReference _placeholderRef;
        private readonly string _sessionId;
        private readonly Action<string, string> _onCompleted;
        private readonly Action<string> _onFailed;

        public ImagePipelineHost(string placeholderPath, string sessionId, Action<string, string> onCompleted, Action<string> onFailed)
        {
            _placeholderPath = placeholderPath;
            _placeholderRef  = TJGeneratorsAssetReference.FromPath(placeholderPath);
            _sessionId       = sessionId ?? "";
            _onCompleted     = onCompleted;
            _onFailed        = onFailed;
        }

        protected override string DialogLogTag => "GenerateImageTool";
        protected override Action<string> DialogFailedCallback => errorMessage => _onFailed?.Invoke(errorMessage);

        public override TJGeneratorsAssetReference GetTargetAsset() => _placeholderRef;

        public void StartEditorCoroutine(IEnumerator coroutine)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(coroutine);
        }

        public string GetAssetSavePath(PipelineMediaType _type, ModelGeneratorBase generator) =>
            // 返回 placeholder 路径，pipeline 直接覆盖文件内容，保持 GUID 不变
            _type == PipelineMediaType.Texture ? _placeholderPath : null;

        public void OnAssetSaved(PipelineMediaType _type, string savePath, ModelGeneratorBase generator)
        {
            if (_type != PipelineMediaType.Texture) return;

            TJLog.Log($"[GenerateImageTool] Image saved: {savePath}");

            // 保持 TextureImporterType.Default（与 ImageWindow 行为一致，不改为 Sprite）
            GeneratedTextureImportUtils.ConfigureImportedTexture(
                savePath, TextureImporterType.Default, alphaIsTransparency: true);
            TJGeneratorsGenerationLabel.EnableLabel(TJGeneratorsAssetReference.FromPath(savePath));
            TJGeneratorsGenerationLabel.EnableSessionLabel(TJGeneratorsAssetReference.FromPath(savePath), _sessionId);
            _onCompleted?.Invoke(savePath, generator.CurrentPreviewUrl);
        }
    }
#endif
}
