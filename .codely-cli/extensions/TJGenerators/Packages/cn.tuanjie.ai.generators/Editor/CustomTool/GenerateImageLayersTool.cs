using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Codely.Newtonsoft.Json.Linq;
using UnityEngine;
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
    /// <summary>
    /// Tracks image-layering (generate_image_layers) tasks across domain reloads.
    /// </summary>
    public static class ImageLayersTaskTracker
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
            public int progress;
            public int layerCount;
            public string layer0Path;
            public string layersFolder;
            public string errorMessage;
            public long startTimeTicks;
            public long endTimeTicks;
            public string previewUrl;
            public string placeholderPath;
            public string backendTaskId;
        }

        public class ImageLayersTaskInfo : IGenerationTaskInfo
        {
            public string TaskId { get; set; }
            public string GeneratorId { get; set; }
            public string Prompt { get; set; }
            public string ImagePath { get; set; }
            public string Status { get; set; }
            public int Progress { get; set; }
            public int LayerCount { get; set; }
            public string Layer0Path { get; set; }
            public string LayersFolder { get; set; }
            public string ErrorMessage { get; set; }
            public string PreviewUrl { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public string PlaceholderPath { get; set; }
            public string BackendTaskId { get; set; }
        }

        private static readonly GenerationTaskTrackerStore<ImageLayersTaskInfo, PersistedTask> Store =
            new GenerationTaskTrackerStore<ImageLayersTaskInfo, PersistedTask>(
                "TJGen_ImageLayers", BuildPersisted, FromPersisted);

        private static PersistedTask BuildPersisted(ImageLayersTaskInfo info) => new PersistedTask
        {
            taskId = info.TaskId,
            generatorId = info.GeneratorId,
            prompt = info.Prompt ?? "",
            imagePath = info.ImagePath ?? "",
            status = info.Status,
            progress = info.Progress,
            layerCount = info.LayerCount,
            layer0Path = info.Layer0Path ?? "",
            layersFolder = info.LayersFolder ?? "",
            errorMessage = info.ErrorMessage ?? "",
            startTimeTicks = info.StartTime.Ticks,
            endTimeTicks = info.EndTime?.Ticks ?? 0,
            previewUrl = info.PreviewUrl ?? "",
            placeholderPath = info.PlaceholderPath ?? "",
            backendTaskId = info.BackendTaskId ?? ""
        };

        private static ImageLayersTaskInfo FromPersisted(PersistedTask p) => new ImageLayersTaskInfo
        {
            TaskId = p.taskId,
            GeneratorId = p.generatorId,
            Prompt = p.prompt,
            ImagePath = p.imagePath,
            Status = p.status,
            Progress = p.progress,
            LayerCount = p.layerCount,
            Layer0Path = p.layer0Path,
            LayersFolder = p.layersFolder,
            ErrorMessage = p.errorMessage,
            PreviewUrl = p.previewUrl,
            StartTime = new DateTime(p.startTimeTicks),
            EndTime = p.endTimeTicks > 0 ? (DateTime?)new DateTime(p.endTimeTicks) : null,
            PlaceholderPath = p.placeholderPath,
            BackendTaskId = p.backendTaskId
        };

        internal static void ApplyTaskUpdate(ImageLayersTaskInfo task, Action<ImageLayersTaskInfo> mutate) =>
            Store.ApplyTaskUpdate(task, mutate);

        public static string CreateTask(
            string generatorId,
            string prompt,
            string imagePath,
            int layerCount,
            string placeholderPath = null,
            string backendTaskId = null)
        {
            string taskId = Store.AllocateTaskId("image_layers");
            var task = new ImageLayersTaskInfo
            {
                TaskId = taskId,
                GeneratorId = generatorId,
                Prompt = prompt ?? "",
                ImagePath = imagePath ?? "",
                LayerCount = layerCount,
                Status = "generating",
                StartTime = DateTime.Now,
                PlaceholderPath = placeholderPath,
                BackendTaskId = backendTaskId
            };
            Store.RegisterTask(taskId, task);
            return taskId;
        }

        public static void MarkTaskCompleted(
            string taskId,
            string layer0Path,
            string layersFolder,
            string previewUrl = null)
        {
            var task = Store.GetTask(taskId);
            if (task == null) return;
            Store.ApplyTaskUpdate(task, t =>
            {
                t.Status = "completed";
                t.Progress = 100;
                t.Layer0Path = layer0Path;
                t.LayersFolder = layersFolder;
                t.PreviewUrl = previewUrl;
                t.EndTime = DateTime.Now;
            });
        }

        public static void MarkTaskFailed(string taskId, string errorMessage)
        {
            var task = Store.GetTask(taskId);
            if (task == null) return;
            Store.ApplyTaskUpdate(task, t =>
            {
                t.Status = "failed";
                t.ErrorMessage = errorMessage;
                t.EndTime = DateTime.Now;
            });
        }

        public static ImageLayersTaskInfo GetTask(string taskId) => Store.GetTask(taskId);

        public static List<ImageLayersTaskInfo> GetAllTasks() => Store.GetAllTasks();

        public static ImageLayersTaskInfo GetTaskByBackendId(string backendTaskId) =>
            Store.GetTaskByBackendId(backendTaskId);

        public static ImageLayersTaskInfo CreateRecoveredTask(
            string backendTaskId,
            string prompt,
            string placeholderPath,
            long timestampMs,
            int layerCount = 4,
            string generatorId = null,
            string imagePath = null)
        {
            return Store.CreateRecoveredTask(backendTaskId, () => new ImageLayersTaskInfo
            {
                TaskId = $"recovered_{backendTaskId}",
                BackendTaskId = backendTaskId,
                GeneratorId = generatorId ?? GenerateImageLayersTool.GeneratorId,
                Prompt = prompt ?? "",
                ImagePath = imagePath ?? "",
                LayerCount = layerCount > 0 ? layerCount : 4,
                PlaceholderPath = placeholderPath ?? "",
                Status = "recovering",
                Progress = 0,
                StartTime = timestampMs > 0
                    ? DateTimeOffset.FromUnixTimeMilliseconds(timestampMs).LocalDateTime
                    : DateTime.Now
            });
        }

        public static void CleanupCompletedTasks() => Store.CleanupCompletedTasks();
#endif
    }

    /// <summary>
    /// Internal machinery for splitting one image into multiple RGBA layers — CustomTool registrations
    /// moved to the MCP backend (generate_image_layers); kept because generate_game_ui_kit's seedream
    /// layer-decomposition step reuses the tracker / pipeline hosts / layer finalization, and legacy
    /// in-flight tasks are still resumed by ImageLayersDomainReloadRecovery.
    /// </summary>
    public static class GenerateImageLayersTool
    {
        public const string GeneratorId = "image-layering";
        public const string SeedreamGeneratorId = "seedream-image-layering";
        private const string ToolName = "generate_image_layers";

        // Seedream 自动分层：底图 + 最多 16 层 = 17 张；CollectIndexedSiblingPaths 遇缺口自动停止
        internal const int SeedreamMaxLayerCount = 17;

#if UNITY_EDITOR
        /// <summary>
        /// Parse seedream_pro size tier from tool params: 1K / 1.5K / 2K；空或 auto 返回空串（跟随输入图）。
        /// </summary>
        internal static string ParseSeedreamSize(JToken token)
        {
            string raw = token?.ToString()?.Trim();
            if (string.IsNullOrEmpty(raw) || string.Equals(raw, "auto", StringComparison.OrdinalIgnoreCase))
                return "";
            if (raw == "1K" || raw == "1.5K" || raw == "2K")
                return raw;
            TJLog.LogWarning($"[GenerateImageLayersTool] Invalid size '{raw}', using auto.");
            return "";
        }

        /// <summary>
        /// Collect layer_0 + {basename}_1 … paths that exist on disk / in AssetDatabase.
        /// </summary>
        internal static List<string> CollectLayerPaths(string layer0Path, int expectedCount) =>
            GeneratedTextureImportUtils.CollectIndexedSiblingPaths(layer0Path, expectedCount);

        internal static void FinalizeLayersAndNotify(
            string taskId,
            string backendTaskId,
            string sessionId,
            string layer0Path,
            int requestedLayerCount,
            string previewUrl,
            string toolName = null)
        {
            if (string.IsNullOrEmpty(taskId) || string.IsNullOrEmpty(layer0Path))
                return;

            var trackerTask = ImageLayersTaskTracker.GetTask(taskId);
            if (trackerTask != null
                && (trackerTask.Status == "completed" || trackerTask.Status == "failed"))
                return;

            int expected = requestedLayerCount > 0
                ? requestedLayerCount
                : (trackerTask?.LayerCount > 0 ? trackerTask.LayerCount : 4);

            var layerPaths = CollectLayerPaths(layer0Path, expected);
            GeneratedTextureImportUtils.ConfigureLayerTextures(layerPaths);
            for (int i = 0; i < layerPaths.Count; i++)
            {
                string path = layerPaths[i];
                if (string.IsNullOrEmpty(path))
                    continue;
                TJGeneratorsGenerationLabel.EnableLabel(TJGeneratorsAssetReference.FromPath(path));
                TJGeneratorsGenerationLabel.EnableSessionLabel(
                    TJGeneratorsAssetReference.FromPath(path), sessionId);
            }

            string layersFolder = Path.GetDirectoryName(layer0Path)?.Replace('\\', '/') ?? "";
            string effectivePreview = PreviewUrlHelper.GetPreviewUrl(previewUrl, backendTaskId);
            ImageLayersTaskTracker.MarkTaskCompleted(taskId, layer0Path, layersFolder, effectivePreview);
            var t = ImageLayersTaskTracker.GetTask(taskId);

            var layerPathsToken = new JArray();
            foreach (string p in layerPaths)
                layerPathsToken.Add(p ?? "");

            GenerationNotifier.NotifyCompleted(
                string.IsNullOrEmpty(toolName) ? ToolName : toolName,
                taskId,
                backendTaskId,
                new JObject
                {
                    ["session_id"] = sessionId ?? "",
                    ["generator_id"] = t?.GeneratorId ?? GeneratorId,
                    ["prompt"] = t?.Prompt ?? "",
                    ["input_image_path"] = t?.ImagePath ?? "",
                    ["layer_0_path"] = layer0Path ?? "",
                    ["layers_folder"] = layersFolder,
                    ["layer_count"] = expected,
                    ["layers_found"] = layerPaths.Count,
                    ["layer_paths"] = layerPathsToken,
                    ["preview_url"] = effectivePreview ?? "",
                    ["progress"] = 100,
                    ["start_time"] = t?.StartTime.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                    ["end_time"] = t?.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                    ["duration_seconds"] = (t != null && t.EndTime.HasValue)
                        ? (int)(t.EndTime.Value - t.StartTime).TotalSeconds
                        : 0
                });
        }
#endif
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
    public static class ImageLayersDomainReloadRecovery
    {
        static ImageLayersDomainReloadRecovery()
        {
            CustomToolDomainReloadRecovery.Schedule(ResumeInterruptedTasks);
        }

        private static void ResumeInterruptedTasks()
        {
            CustomToolDomainReloadRecovery.Resume(
                "GenerateImageLayersTool",
                ConfigType.Image,
                // generate_game_ui_kit 的 Step 2（seedream 图层拆分）也走本 tracker/host，一并恢复
                t => t.toolName == "generate_image_layers" || t.toolName == "generate_game_ui_kit",
                () => ImageLayersTaskTracker.GetAllTasks(),
                (interrupted, _, generator) =>
                {
                    var trackerTask = ImageLayersTaskTracker.GetTaskByBackendId(interrupted.backendTaskId);
                    if (trackerTask != null)
                    {
                        CustomToolDomainReloadRecovery.MarkTrackerRecoveringIfNeeded(trackerTask.Status, () =>
                        {
                            ImageLayersTaskTracker.ApplyTaskUpdate(trackerTask, t => t.Status = "recovering");
                        });
                    }
                    else
                    {
                        string placeholderPath = CustomToolDomainReloadRecovery.ResolveAssetPath(interrupted.targetAssetGuid);
                        int recoveredLayers = interrupted.numLayers > 0 ? interrupted.numLayers : 4;
                        trackerTask = ImageLayersTaskTracker.CreateRecoveredTask(
                            interrupted.backendTaskId,
                            interrupted.prompt,
                            placeholderPath,
                            interrupted.timestamp,
                            layerCount: recoveredLayers,
                            generatorId: interrupted.modelVersion,
                            imagePath: interrupted.imagePath);
                    }

                    string placeholderPathForHost = trackerTask.PlaceholderPath ?? "";
                    if (string.IsNullOrEmpty(placeholderPathForHost))
                        placeholderPathForHost = CustomToolDomainReloadRecovery.ResolveAssetPath(interrupted.targetAssetGuid);

                    int layerCount = trackerTask.LayerCount > 0
                        ? trackerTask.LayerCount
                        : (interrupted.numLayers > 0 ? interrupted.numLayers : 4);
                    var host = new ImageLayersRecoveryHost(
                        placeholderPathForHost,
                        interrupted.backendTaskId,
                        interrupted.sessionId,
                        layerCount,
                        generator,
                        interrupted.toolName);
                    CustomToolDomainReloadRecovery.StartPolling(
                        "GenerateImageLayersTool", host, ConfigType.Image,
                        interrupted.sessionId, interrupted.toolName, generator, interrupted.backendTaskId);
                });
        }
    }

    internal class ImageLayersRecoveryHost : HeadlessPipelineHostBase, IMediaAssetPipelineHost
    {
        private readonly string _placeholderPath;
        private readonly TJGeneratorsAssetReference _placeholderRef;
        private readonly string _backendTaskId;
        private readonly string _sessionId;
        private readonly int _layerCount;
        private readonly ModelGeneratorBase _generator;
        private readonly string _toolName;
        private string _layer0Path;
        private string _previewUrl;

        public ImageLayersRecoveryHost(
            string placeholderPath,
            string backendTaskId,
            string sessionId,
            int layerCount,
            ModelGeneratorBase generator,
            string toolName = null)
        {
            _placeholderPath = placeholderPath ?? "";
            _placeholderRef = string.IsNullOrEmpty(_placeholderPath)
                ? null
                : TJGeneratorsAssetReference.FromPath(_placeholderPath);
            _backendTaskId = backendTaskId;
            _sessionId = sessionId ?? "";
            _layerCount = layerCount > 0 ? layerCount : 4;
            _generator = generator;
            _toolName = toolName;
        }

        protected override string DialogLogTag => "ImageLayersRecovery";

        public override TJGeneratorsAssetReference GetTargetAsset() => _placeholderRef;

        public override void Repaint()
        {
            if (_generator == null) return;
            var trackerTask = ImageLayersTaskTracker.GetTaskByBackendId(_backendTaskId);
            if (trackerTask == null || !TJGeneratorsTaskRecovery.IsRecoverableTrackerStatus(trackerTask.Status)) return;

            int progress = _generator.CurrentProgress;
            if (progress <= trackerTask.Progress) return;

            ImageLayersTaskTracker.ApplyTaskUpdate(trackerTask, t =>
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
                var trackerTask = ImageLayersTaskTracker.GetTaskByBackendId(_backendTaskId);
                if (trackerTask != null)
                {
                    var friendlyError = ErrorDialogUtils.ConvertToUserFriendlyError(title, message);
                    ImageLayersTaskTracker.MarkTaskFailed(trackerTask.TaskId, friendlyError.TechnicalMessage);
                    GenerationNotifier.NotifyFailed(
                        "generate_image_layers",
                        trackerTask.TaskId,
                        _backendTaskId,
                        friendlyError.TechnicalMessage,
                        new JObject
                        {
                            ["session_id"] = _sessionId,
                            ["generator_id"] = trackerTask.GeneratorId ?? "",
                            ["prompt"] = trackerTask.Prompt ?? "",
                            ["input_image_path"] = trackerTask.ImagePath ?? ""
                        });
                }
            }
        }

        public string GetAssetSavePath(PipelineMediaType _type, ModelGeneratorBase generator) =>
            _type == PipelineMediaType.Texture ? _placeholderPath : null;

        public void OnAssetSaved(PipelineMediaType _type, string savePath, ModelGeneratorBase generator)
        {
            if (_type != PipelineMediaType.Texture) return;

            _layer0Path = savePath;
            if (!string.IsNullOrEmpty(generator?.CurrentPreviewUrl))
                _previewUrl = generator.CurrentPreviewUrl;

            GeneratedTextureImportUtils.ConfigureImportedTexture(
                savePath, TextureImporterType.Default, alphaIsTransparency: true);
            TJGeneratorsGenerationLabel.EnableLabel(TJGeneratorsAssetReference.FromPath(savePath));
            TJGeneratorsGenerationLabel.EnableSessionLabel(TJGeneratorsAssetReference.FromPath(savePath), _sessionId);
        }

        public override void OnGenerationCompleted(string assetPath)
        {
            string layer0 = !string.IsNullOrEmpty(_layer0Path) ? _layer0Path : assetPath;
            var trackerTask = ImageLayersTaskTracker.GetTaskByBackendId(_backendTaskId);
            if (trackerTask == null) return;

            int expected = trackerTask.LayerCount > 0 ? trackerTask.LayerCount : _layerCount;
            string preview = !string.IsNullOrEmpty(_previewUrl)
                ? _previewUrl
                : (_generator?.CurrentPreviewUrl ?? "");
            GenerateImageLayersTool.FinalizeLayersAndNotify(
                trackerTask.TaskId,
                _backendTaskId,
                _sessionId,
                layer0,
                expected,
                preview,
                _toolName);
        }
    }

    internal class ImageLayersPipelineHost : HeadlessPipelineHostBase, IMediaAssetPipelineHost
    {
        private readonly string _placeholderPath;
        private readonly TJGeneratorsAssetReference _placeholderRef;
        private readonly string _sessionId;
        private readonly int _layerCount;
        private readonly string _taskId;
        private readonly string _backendTaskId;
        private readonly Action<string> _onFailed;
        private readonly string _toolName;
        private string _layer0Path;
        private string _previewUrl;

        public ImageLayersPipelineHost(
            string placeholderPath,
            string sessionId,
            int layerCount,
            string taskId,
            string backendTaskId,
            Action<string> onFailed,
            string toolName = null)
        {
            _placeholderPath = placeholderPath;
            _placeholderRef = TJGeneratorsAssetReference.FromPath(placeholderPath);
            _sessionId = sessionId ?? "";
            _layerCount = layerCount > 0 ? layerCount : 4;
            _taskId = taskId;
            _backendTaskId = backendTaskId;
            _onFailed = onFailed;
            _toolName = toolName;
        }

        protected override string DialogLogTag => "GenerateImageLayersTool";
        protected override Action<string> DialogFailedCallback => errorMessage => _onFailed?.Invoke(errorMessage);

        public override TJGeneratorsAssetReference GetTargetAsset() => _placeholderRef;

        public void StartEditorCoroutine(IEnumerator coroutine)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(coroutine);
        }

        public string GetAssetSavePath(PipelineMediaType _type, ModelGeneratorBase generator) =>
            _type == PipelineMediaType.Texture ? _placeholderPath : null;

        public void OnAssetSaved(PipelineMediaType _type, string savePath, ModelGeneratorBase generator)
        {
            if (_type != PipelineMediaType.Texture) return;

            _layer0Path = savePath;
            if (!string.IsNullOrEmpty(generator?.CurrentPreviewUrl))
                _previewUrl = generator.CurrentPreviewUrl;

            TJLog.Log($"[GenerateImageLayersTool] Layer 0 saved: {savePath} (expected {_layerCount} layers; waiting for remaining downloads)");

            GeneratedTextureImportUtils.ConfigureImportedTexture(
                savePath, TextureImporterType.Default, alphaIsTransparency: true);
            TJGeneratorsGenerationLabel.EnableLabel(TJGeneratorsAssetReference.FromPath(savePath));
            TJGeneratorsGenerationLabel.EnableSessionLabel(TJGeneratorsAssetReference.FromPath(savePath), _sessionId);
        }

        public override void OnGenerationCompleted(string assetPath)
        {
            string layer0 = !string.IsNullOrEmpty(_layer0Path) ? _layer0Path : assetPath;
            GenerateImageLayersTool.FinalizeLayersAndNotify(
                _taskId,
                _backendTaskId,
                _sessionId,
                layer0,
                _layerCount,
                _previewUrl,
                _toolName);
        }
    }
#endif
}
