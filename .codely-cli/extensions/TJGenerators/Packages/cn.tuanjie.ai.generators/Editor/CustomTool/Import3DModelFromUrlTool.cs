using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Codely.Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
using TJGenerators;
using TJGenerators.Generators;
using TJGenerators.Config;
using TJGenerators.Pipeline;
using TJGenerators.PostProcessing;
using TJGenerators.Utils;
using Unity.EditorCoroutines.Editor;
#endif

namespace UnityTcp.Editor.Tools
{
    /// <summary>
    /// Tracks 3D model import tasks started by import_3d_model_from_url.
    /// Generation itself is done by the MCP backend (generate_3d_model + check_task);
    /// this tracker only covers the Unity-side download/import/post-process stage.
    /// Import source URLs are permanent CDN URLs, so after a domain reload an
    /// in-flight import is simply re-launched with the same parameters (idempotent).
    /// </summary>
    public static class ImportModelTaskTracker
    {
#if UNITY_EDITOR
        [Serializable]
        private class PersistedTask
        {
            public string taskId;
            public string sessionId;
            public string provider;
            public string outputType;
            public string modelUrl;
            public string renderedImageUrl;
            public string prompt;
            public string status;
            public int    progress;

            public string prefabPath;
            public string modelPath;
            public string sourceModelPath;
            public string riggedModelPath;
            public string targetPrefabPath;
            public string controllerPath;

            public bool   addMotion;
            public string motionDescription;
            public bool   loopTime;

            public string errorMessage;
            public long   startTimeTicks;
            public long   endTimeTicks;
        }

        public class ImportTaskInfo : IGenerationTaskInfo
        {
            public string TaskId          { get; set; }
            public string SessionId       { get; set; }
            public string Provider        { get; set; }
            public string OutputType      { get; set; }
            public string ModelUrl         { get; set; }
            public string RenderedImageUrl { get; set; }
            public string Prompt           { get; set; }
            public string Status           { get; set; }
            public int    Progress        { get; set; }

            public string PrefabPath       { get; set; }
            public string ModelPath         { get; set; }
            public string SourceModelPath  { get; set; }
            public string RiggedModelPath  { get; set; }
            public string TargetPrefabPath { get; set; }
            public string ControllerPath   { get; set; }

            public bool   AddMotion        { get; set; }
            public string MotionDescription { get; set; }
            public bool   LoopTime         { get; set; } = true;

            public string ErrorMessage    { get; set; }
            public DateTime StartTime     { get; set; }
            public DateTime? EndTime      { get; set; }
            public string BackendTaskId   { get; set; }
            public string PreviewUrl      { get; set; }
        }

        private static readonly GenerationTaskTrackerStore<ImportTaskInfo, PersistedTask> Store =
            new GenerationTaskTrackerStore<ImportTaskInfo, PersistedTask>(
                "TJGen_ImportModel",
                BuildPersisted,
                FromPersisted);

        private static PersistedTask BuildPersisted(ImportTaskInfo info) => new PersistedTask
        {
            taskId             = info.TaskId ?? "",
            sessionId          = info.SessionId ?? "",
            provider           = info.Provider ?? "",
            outputType         = info.OutputType ?? "static",
            modelUrl           = info.ModelUrl ?? "",
            renderedImageUrl   = info.RenderedImageUrl ?? "",
            prompt             = info.Prompt ?? "",
            status             = info.Status ?? "",
            progress           = info.Progress,
            prefabPath         = info.PrefabPath ?? "",
            modelPath          = info.ModelPath ?? "",
            sourceModelPath    = info.SourceModelPath ?? "",
            riggedModelPath    = info.RiggedModelPath ?? "",
            targetPrefabPath   = info.TargetPrefabPath ?? "",
            controllerPath     = info.ControllerPath ?? "",
            addMotion          = info.AddMotion,
            motionDescription  = info.MotionDescription ?? "",
            loopTime           = info.LoopTime,
            errorMessage       = info.ErrorMessage ?? "",
            startTimeTicks     = info.StartTime.Ticks,
            endTimeTicks       = info.EndTime?.Ticks ?? 0
        };

        private static ImportTaskInfo FromPersisted(PersistedTask p) => new ImportTaskInfo
        {
            TaskId             = p.taskId,
            SessionId          = p.sessionId,
            Provider           = p.provider,
            OutputType         = string.IsNullOrEmpty(p.outputType) ? "static" : p.outputType,
            ModelUrl           = p.modelUrl,
            RenderedImageUrl   = p.renderedImageUrl,
            Prompt             = p.prompt,
            Status             = p.status,
            Progress           = p.progress,
            PrefabPath         = p.prefabPath,
            ModelPath          = p.modelPath,
            SourceModelPath    = p.sourceModelPath,
            RiggedModelPath    = p.riggedModelPath,
            TargetPrefabPath   = p.targetPrefabPath,
            ControllerPath     = p.controllerPath,
            AddMotion          = p.addMotion,
            MotionDescription  = p.motionDescription,
            LoopTime           = p.loopTime,
            ErrorMessage       = p.errorMessage,
            StartTime          = new DateTime(p.startTimeTicks),
            EndTime            = p.endTimeTicks > 0 ? (DateTime?)new DateTime(p.endTimeTicks) : null
        };

        internal static void ApplyTaskUpdate(ImportTaskInfo task, Action<ImportTaskInfo> mutate) =>
            Store.ApplyTaskUpdate(task, mutate);

        internal static void AddTask(ImportTaskInfo task)
        {
            if (task == null || string.IsNullOrEmpty(task.TaskId)) return;
            Store.RegisterTask(task.TaskId, task);
        }

        public static ImportTaskInfo GetTask(string taskId) => Store.GetTask(taskId);

        public static List<ImportTaskInfo> GetAllTasks() => Store.GetAllTasks();

        public static void RemoveTask(string taskId) => Store.RemoveTask(taskId);
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// Re-launches in-flight import tasks after a domain reload.
    /// The model URL is a permanent CDN URL and the download path is deterministic
    /// (md5 of the URL), so re-running the import simply overwrites in place.
    /// </summary>
    [InitializeOnLoad]
    public static class ImportModelDomainReloadRecovery
    {
        static ImportModelDomainReloadRecovery()
        {
            CustomToolDomainReloadRecovery.Schedule(ResumeInterruptedImports);
        }

        private static void ResumeInterruptedImports()
        {
            foreach (var task in ImportModelTaskTracker.GetAllTasks())
            {
                if (task == null) continue;
                if (task.Status != "importing" && task.Status != "recovering" && task.Status != "initializing")
                    continue;
                if (string.IsNullOrEmpty(task.ModelUrl)) continue;

                TJLog.Log($"[ImportModelDomainReloadRecovery] Resuming import task: {task.TaskId}");
                ImportModelTaskTracker.ApplyTaskUpdate(task, t => t.Status = "recovering");
                Import3DModelFromUrlTool.LaunchImport(task);
            }
        }
    }

    /// <summary>
    /// Headless pipeline host for import_3d_model_from_url.
    /// Deterministic model download path (md5 of URL) so domain-reload re-runs
    /// overwrite in place instead of allocating new History slots.
    /// </summary>
    internal class ImportModelPipelineHost : HeadlessPipelineHostBase, IModelDownloadPathProvider
    {
        private readonly ImportModelTaskTracker.ImportTaskInfo _task;

        internal ImportModelPipelineHost(ImportModelTaskTracker.ImportTaskInfo task)
        {
            _task = task;
        }

        protected override string DialogLogTag => "Import3DModelFromUrl";

        public override TJGeneratorsAssetReference GetTargetAsset()
        {
            // motion FBX is imported as animation source; the prefab keeps the rigged model
            if (IsMotion) return null;
            if (string.IsNullOrEmpty(_task?.PrefabPath)) return null;
            return TJGeneratorsAssetReference.FromPath(_task.PrefabPath);
        }

        private bool IsMotion =>
            string.Equals(_task?.OutputType, "motion", StringComparison.OrdinalIgnoreCase);

        public string GetModelDownloadPath(string resolvedSavePath)
        {
            if (_task == null || string.IsNullOrEmpty(_task.ModelUrl)) return null;

            string ext = Path.GetExtension(resolvedSavePath);
            if (string.IsNullOrEmpty(ext)) ext = Path.GetExtension(_task.ModelUrl);
            if (string.IsNullOrEmpty(ext)) ext = ".fbx";

            if (IsMotion)
            {
                // 与 generate_model_motion 产线一致：动作 FBX 落在绑骨模型同目录
                if (string.IsNullOrEmpty(_task.RiggedModelPath)) return null;
                string dir = Path.GetDirectoryName(_task.RiggedModelPath)?.Replace("\\", "/") ?? "";
                string baseName = Path.GetFileNameWithoutExtension(_task.RiggedModelPath);
                if (baseName.EndsWith("_rigged", StringComparison.OrdinalIgnoreCase))
                    baseName = baseName.Substring(0, baseName.Length - "_rigged".Length);
                return Path.Combine(dir, baseName + "_motion" + ext).Replace("\\", "/");
            }

            // 静态/绑骨导入：按 Prefab 名分组 + URL 哈希文件名，域重载重跑原位覆盖
            string groupBase = string.IsNullOrEmpty(_task.PrefabPath)
                ? "Import"
                : Path.GetFileNameWithoutExtension(_task.PrefabPath);
            string group = PathUtils.SanitizeAssetFolderName(groupBase);
            if (string.IsNullOrEmpty(group)) group = "Import";
            return ("Assets/TJGenerators/History/" + group + "/" +
                    Import3DModelFromUrlTool.BuildUrlHashFileName(_task.ModelUrl, ext))
                .Replace("\\", "/");
        }

        public override void OnGenerationCompleted(string modelPath)
        {
            if (_task == null) return;

            string controllerPath = _task.ControllerPath ?? "";

            if (IsMotion)
            {
                // 1) 配置动作 FBX 导入（Humanoid 动画剪辑，可循环）
                RiggedModelPostProcess.SetupAnimationImport(modelPath, _task.LoopTime);

                // 2) 重新导入使动画剪辑可提取，控制器与剪辑使用相同的循环设置
                PathUtils.SafeRefresh();
                if (!string.IsNullOrEmpty(_task.RiggedModelPath))
                {
                    string riggedDir = Path.GetDirectoryName(_task.RiggedModelPath)?.Replace("\\", "/") ?? "";
                    string riggedBase = Path.GetFileNameWithoutExtension(_task.RiggedModelPath);
                    controllerPath = RiggedModelPostProcess.CreateSingleClipLoopAnimatorControllerFromMotionClip(
                        riggedDir, riggedBase, modelPath, _task.LoopTime) ?? "";

                    // 3) 把控制器 + Avatar 绑回目标 Prefab
                    if (!string.IsNullOrEmpty(_task.TargetPrefabPath))
                        ReplaceAnimatedCharacterModelTool.AssignAnimatorControllerIfMissing(
                            _task.TargetPrefabPath, _task.RiggedModelPath);
                }
            }
            else if (string.Equals(_task.OutputType, "rigged", StringComparison.OrdinalIgnoreCase))
            {
                // 绑骨 FBX：DownloadModel 的 rigged 分支已完成 Humanoid 导入收尾，
                // 这里补上 Prefab 的 Animator/Avatar 绑定
                if (!string.IsNullOrEmpty(_task.PrefabPath))
                    ReplaceAnimatedCharacterModelTool.AssignAnimatorControllerIfMissing(
                        _task.PrefabPath, modelPath);
            }

            ImportModelTaskTracker.ApplyTaskUpdate(_task, t =>
            {
                t.Status         = "completed";
                t.Progress       = 100;
                t.ModelPath      = modelPath;
                t.ControllerPath = controllerPath;
                t.EndTime        = DateTime.Now;
            });

            GenerationNotifier.NotifyCompleted(
                toolName:      "import_3d_model_from_url",
                taskId:        _task.TaskId,
                backendTaskId: "",
                extraData: new JObject
                {
                    ["session_id"]       = _task.SessionId ?? "",
                    ["provider"]         = _task.Provider ?? "",
                    ["output_type"]      = _task.OutputType ?? "static",
                    ["model_url"]        = _task.ModelUrl ?? "",
                    ["model_path"]       = modelPath ?? "",
                    // motion 模式下模型即动作 FBX，同时按 SKILL 文档暴露 motion_fbx_path
                    ["motion_fbx_path"]  = IsMotion ? (modelPath ?? "") : "",
                    ["prefab_path"]      = _task.PrefabPath ?? "",
                    ["target_prefab_path"] = _task.TargetPrefabPath ?? "",
                    ["rigged_model_path"] = _task.RiggedModelPath ?? "",
                    ["source_model_path"] = _task.SourceModelPath ?? "",
                    ["controller_path"]  = controllerPath ?? "",
                    ["add_motion"]       = _task.AddMotion,
                    ["motion_description"] = _task.MotionDescription ?? "",
                    ["progress"]         = 100,
                    ["start_time"]       = _task.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["end_time"]         = _task.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                    ["duration_seconds"] = _task.EndTime.HasValue
                        ? (int)(_task.EndTime.Value - _task.StartTime).TotalSeconds
                        : 0
                });

            TJLog.Log($"[ImportModelPipelineHost] Import completed: {modelPath}");
        }

        public override void ShowDialog(string title, string message)
        {
            base.ShowDialog(title, message);

            if (ErrorDialogUtils.IsErrorDialog(title) && _task != null)
            {
                var friendly = ErrorDialogUtils.ConvertToUserFriendlyError(title, message);
                ImportModelTaskTracker.ApplyTaskUpdate(_task, t =>
                {
                    t.Status       = "failed";
                    t.ErrorMessage = friendly.TechnicalMessage;
                    t.EndTime      = DateTime.Now;
                });
                GenerationNotifier.NotifyFailed(
                    toolName:      "import_3d_model_from_url",
                    taskId:        _task.TaskId,
                    backendTaskId: "",
                    errorMessage:  friendly.TechnicalMessage,
                    extraData: new JObject
                    {
                        ["session_id"]  = _task.SessionId ?? "",
                        ["provider"]    = _task.Provider ?? "",
                        ["output_type"] = _task.OutputType ?? "static",
                        ["model_url"]   = _task.ModelUrl ?? ""
                    });
            }
        }
    }
#endif

    /// <summary>
    /// CustomTool importing an already-generated 3D model from a permanent CDN URL
    /// (produced by the MCP backend: generate_3d_model + check_task).
    /// Replaces the removed Unity-side generation tools (generate_3d_model_by_rodin /
    /// generate_3d_model_by_tripo_p1 / generate_texture_model_by_tripo /
    /// generate_rigged_model / generate_model_motion / generate_animated_character):
    /// generation now runs on MCP, this tool owns download + import + post-processing
    /// + prefab binding + scene-ready output.
    /// </summary>
    public static class Import3DModelFromUrlTool
    {
        private const string ToolName = "import_3d_model_from_url";

        [ExecuteCustomTool.CustomTool(ToolName,
            "Import an already-generated 3D model from a permanent CDN URL into Unity, with the full TJGenerators " +
            "post-processing chain (texture extraction, material remap, normal-map fix, default white material fallback, " +
            "mesh validation, auto-fit normalization to ~1m, placeholder replacement, History record + TuanjieAI/Session labels). " +
            "The URL comes from the MCP backend generation flow (generate_3d_model -> check_task -> model URL), NOT from a local path. " +
            "Supports .fbx / .obj / .zip model URLs and an optional rendered_image_url (applied as main texture, Tripo outputs usually have one). " +
            "Modes via output_type: " +
            "'static' (default) — plain model import bound to a Cube placeholder prefab; " +
            "'rigged' — import a Humanoid rigged FBX (e.g. from MCP unirig_rig); pass source_model_path to restore textures from the source model; " +
            "'motion' — import a motion FBX (e.g. from MCP generate_motion); pass rigged_model_path (required) and optional target_prefab_path; " +
            "an AnimatorController is created and assigned automatically; no placeholder prefab is created for motion. " +
            "static also supports add_motion=true + motion_description (same as UI 添加动作): after the mesh lands, the pipeline " +
            "runs UniRig + HunyuanMotion automatically and the prefab ends up rigged + animated in ONE call (~5-15 min). " +
            "Key parameters: model_url (required), rendered_image_url, provider (rodin|tripo|tripo-texture|hunyuan — selects the " +
            "post-processing preset; default rodin; static only), output_type (static|rigged|motion), prefab_output_path, force_overwrite, " +
            "add_motion, motion_description, source_model_path (rigged), rigged_model_path + target_prefab_path + loop (motion), " +
            "prompt (optional display text for History), session_id. " +
            "Returns immediately with task_id + prefab_output_path (a Cube/Capsule placeholder). " +
            "Instantiate the placeholder prefab right away. " +
            "RELIABLE COMPLETION FLOW (the Unity-side <bg_task_done> push may never reach the session): after instantiating, " +
            "run a foreground shell wait of at most 30 seconds in this subagent (e.g. 'Start-Sleep -Seconds 30' / 'sleep 30'), " +
            "keep the subagent active; after the wait returns, call query_local_task ONCE with task_id to fetch the result; " +
            "if it still reports 'importing', run another bounded foreground wait and query again. " +
            "Do NOT loop query_local_task within one wake-up, and do NOT wait indefinitely for a Unity-side notification. " +
            "Domain reload safe: in-flight imports are automatically re-launched (URL is permanent, import is idempotent).")]
        public static object ImportModelFromUrl(JObject parameters)
        {
#if UNITY_EDITOR
            try
            {
                TJLog.Log($"[Import3DModelFromUrlTool] Import with parameters: {parameters}");

                if (TJGeneratorsPlayModeGuard.IsActive)
                    return Fail(TJGeneratorsPlayModeGuard.Message);

                string modelUrl          = parameters["model_url"]?.ToString();
                string renderedImageUrl  = parameters["rendered_image_url"]?.ToString();
                string provider          = parameters["provider"]?.ToString();
                string outputType        = parameters["output_type"]?.ToString();
                string prompt            = parameters["prompt"]?.ToString();
                string prefabOutputPath  = parameters["prefab_output_path"]?.ToString();
                bool   forceOverwrite    = parameters["force_overwrite"]?.ToObject<bool>() ?? false;
                string sessionId         = parameters["session_id"]?.ToString() ?? "";
                GenerationRequestOrigin.SetWorkspaceName(parameters["workspace_name"]?.ToString());

                if (string.IsNullOrEmpty(outputType))
                    outputType = "static";
                outputType = outputType.Trim().ToLowerInvariant();
                if (outputType != "static" && outputType != "rigged" && outputType != "motion")
                    return Fail($"Unsupported output_type '{outputType}'. Supported: static / rigged / motion.");

                if (string.IsNullOrEmpty(modelUrl))
                    return Fail("'model_url' is required. Get it from the MCP backend: generate_3d_model -> check_task -> model URL (.fbx/.obj/.zip).");

                // 自行解析扩展名（GenerationAssetFormatUtils.GetExtensionFromUrl 对不认识的扩展名返回 null，
                // 无法区分 "不支持" 与 "没有扩展名"，这里直接从 URL path 段取）
                string urlPath = modelUrl;
                int queryIdx = urlPath.IndexOf('?');
                if (queryIdx > 0)
                    urlPath = urlPath.Substring(0, queryIdx);
                string ext = Path.GetExtension(urlPath);
                if (string.IsNullOrEmpty(ext))
                    return Fail("model_url has no recognizable extension. Supported: .fbx / .obj / .zip " +
                                "(use the model URL from check_task; a .glb URL means the generation did not enable with_fbx).");
                ext = ext.ToLowerInvariant();
                if (ext != ".fbx" && ext != ".obj" && ext != ".zip")
                    return Fail($"Unsupported model_url extension '{ext}'. Supported: .fbx / .obj / .zip. " +
                                "If the URL is .glb, regenerate via MCP with with_fbx=true.");

                bool addMotion = parameters["add_motion"]?.ToObject<bool>() ?? false;
                string motionDescription = parameters["motion_description"]?.ToString();
                if (addMotion && outputType != "static")
                    return Fail("add_motion is only supported with output_type=static (the mesh is rigged after import).");
                if (addMotion && string.IsNullOrWhiteSpace(motionDescription))
                    return Fail("add_motion is true but 'motion_description' is required (same as UI 添加动作). Example: 'a walking cycle'.");

                if (string.IsNullOrEmpty(provider))
                    provider = "rodin";

                string sourceModelPath = parameters["source_model_path"]?.ToString();
                if (outputType == "rigged" && string.IsNullOrEmpty(sourceModelPath))
                    return Fail("'source_model_path' is required for output_type=rigged (used to restore textures onto the rigged FBX).");
                if (outputType == "rigged" && !File.Exists(PathUtils.ToAbsoluteAssetPath(sourceModelPath)))
                    return Fail($"Source model not found: {sourceModelPath}");

                string riggedModelPath = parameters["rigged_model_path"]?.ToString();
                if (outputType == "motion" && string.IsNullOrEmpty(riggedModelPath))
                    return Fail("'rigged_model_path' is required for output_type=motion (the Humanoid FBX the motion clips belong to).");
                if (outputType == "motion" && !File.Exists(PathUtils.ToAbsoluteAssetPath(riggedModelPath)))
                    return Fail($"Rigged model not found: {riggedModelPath}");

                string targetPrefabPath = parameters["target_prefab_path"]?.ToString();
                bool loopTime = parameters["loop"]?.ToObject<bool>() ?? true;

                string providerConfigId = ResolveProviderConfigId(outputType == "static" ? provider : null);
                if (outputType == "static" && providerConfigId == null)
                    return Fail($"Unsupported provider '{provider}'. Supported: rodin / tripo / tripo-texture / hunyuan.");
                var config = ConfigManager.GetGeneratorConfig(
                    ConfigType.Generator, ResolveGeneratorConfigId(outputType, providerConfigId));
                if (config == null)
                    return Fail($"Cannot find generator config for import (output_type={outputType}). Ensure cn.tuanjie.ai.generators package is installed.");

                string createdPrefabPath = null;
                if (outputType != "motion")
                {
                    prefabOutputPath = ResolvePrefabPath(prefabOutputPath, prompt, modelUrl, outputType, forceOverwrite);
                    if (prefabOutputPath == null)
                        return Fail("Failed to resolve prefab output path");

                    createdPrefabPath = outputType == "rigged"
                        ? ReplaceAnimatedCharacterModelTool.CreateBlankPrefab(prefabOutputPath)
                        : CreateBlankPrefab(prefabOutputPath);
                    if (string.IsNullOrEmpty(createdPrefabPath))
                        return Fail($"Failed to create prefab at: {prefabOutputPath}");

                    TJGeneratorsGenerationLabel.EnableSessionLabel(
                        TJGeneratorsAssetReference.FromPath(createdPrefabPath), sessionId);
                }

                var task = new ImportModelTaskTracker.ImportTaskInfo
                {
                    TaskId            = $"import_model_{DateTime.Now.Ticks}",
                    SessionId         = sessionId,
                    Provider          = outputType == "static" ? provider : "",
                    OutputType        = outputType,
                    ModelUrl          = modelUrl,
                    RenderedImageUrl  = renderedImageUrl ?? "",
                    Prompt            = prompt ?? "",
                    Status            = "importing",
                    Progress          = 0,
                    PrefabPath        = createdPrefabPath ?? "",
                    SourceModelPath   = sourceModelPath ?? "",
                    RiggedModelPath   = riggedModelPath ?? "",
                    TargetPrefabPath = targetPrefabPath ?? "",
                    AddMotion         = addMotion,
                    MotionDescription = motionDescription ?? "",
                    LoopTime          = loopTime,
                    StartTime         = DateTime.Now
                };
                ImportModelTaskTracker.AddTask(task);

                LaunchImport(task);

                bool hasPlaceholder = !string.IsNullOrEmpty(createdPrefabPath);
                // static 实测通常 3-8s 完成；agent 按 estimated_wait_seconds 起后台定时器，未完成会自动续
                int estimatedWait = addMotion ? 900 : 60;
                var result = new Dictionary<string, object>
                {
                    { "success",              true },
                    { "submission_success",   true },
                    { "task_id",              task.TaskId },
                    { "status",               "importing" },
                    { "output_type",          outputType },
                    { "model_url",            modelUrl },
                    { "add_motion",           addMotion },
                    { "estimated_wait_seconds", estimatedWait },
                    { "notification_mode",    "query_status" },
                    { "message",
                        (hasPlaceholder
                            ? "Import started. STEP 1 (do now): Instantiate the prefab at prefab_output_path — it contains a placeholder. "
                            : "Import started (motion mode — no placeholder prefab; the controller is assigned to target_prefab_path automatically). ") +
                        "STEP 2 (do now): Wait in this subagent with run_shell_command(run_in_background=false), using Start-Sleep / sleep for at most 30 seconds. " +
                        "Keep ownership of this import; a timer finishing is not import completion, and the Unity-side push may never arrive. " +
                        "STEP 3: Do not call complete_task or return final text while the import is pending. " +
                        "STEP 4 (after the foreground wait returns): Call query_local_task ONCE with this task_id to fetch model_path / prefab_path; " +
                        "if it still reports 'importing', run another bounded foreground wait before querying again. " +
                        "The placeholder child is replaced in place — do NOT place again. " +
                        "*** Never wait indefinitely for a Unity-side notification; never loop query_local_task within one wake-up. ***" }
                };
                if (hasPlaceholder) result["prefab_output_path"] = createdPrefabPath;
                if (outputType == "static") result["provider"] = provider;
                if (addMotion) result["motion_description"] = motionDescription;
                if (outputType == "rigged") result["source_model_path"] = sourceModelPath;
                if (outputType == "motion")
                {
                    result["rigged_model_path"]  = riggedModelPath;
                    result["target_prefab_path"] = targetPrefabPath ?? "";
                }
                return result;
            }
            catch (Exception e)
            {
                TJLog.LogError($"[Import3DModelFromUrlTool] Error: {e}");
                return Fail($"Error: {e.Message}");
            }
#else
            return Fail("This tool only works in Unity Editor.");
#endif
        }

        public static object QueryImportStatus(JObject parameters)
        {
#if UNITY_EDITOR
            try
            {
                string taskId = parameters["task_id"]?.ToString();
                if (string.IsNullOrEmpty(taskId))
                    return Fail("'task_id' parameter is required");

                var task = ImportModelTaskTracker.GetTask(taskId);
                if (task == null)
                    return Fail($"Task '{taskId}' not found. It may have been cleaned up or Unity was fully restarted.");

                var result = new Dictionary<string, object>
                {
                    { "success",    true },
                    { "task_id",    task.TaskId },
                    { "status",     task.Status },
                    { "progress",   task.Progress },
                    { "output_type", task.OutputType ?? "static" },
                    { "start_time", task.StartTime.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                if (!string.IsNullOrEmpty(task.PrefabPath))       result["prefab_path"]  = task.PrefabPath;
                if (!string.IsNullOrEmpty(task.ModelPath))
                {
                    result["model_path"] = task.ModelPath;
                    // motion 模式下模型即动作 FBX，同时按 SKILL 文档暴露 motion_fbx_path
                    if (string.Equals(task.OutputType, "motion", StringComparison.OrdinalIgnoreCase))
                        result["motion_fbx_path"] = task.ModelPath;
                }
                if (!string.IsNullOrEmpty(task.RiggedModelPath))  result["rigged_model_path"] = task.RiggedModelPath;
                if (!string.IsNullOrEmpty(task.ControllerPath))    result["controller_path"] = task.ControllerPath;
                if (!string.IsNullOrEmpty(task.TargetPrefabPath)) result["target_prefab_path"] = task.TargetPrefabPath;
                if (!string.IsNullOrEmpty(task.ErrorMessage))     result["error"]        = task.ErrorMessage;
                if (task.EndTime.HasValue)
                {
                    result["end_time"]         = task.EndTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    result["duration_seconds"] = (int)(task.EndTime.Value - task.StartTime).TotalSeconds;
                }

                if (task.Status == "recovering")
                    result["hint"] = "The import was interrupted by a domain reload and is being re-launched automatically. Wait in this subagent for up to 30 seconds, then check this same task again.";

                if (task.Status == "completed" && !string.IsNullOrEmpty(task.ModelPath))
                    result["result_summary"] = $"Import completed. Model: {task.ModelPath}. Prefab: {task.PrefabPath ?? task.TargetPrefabPath ?? "N/A"}.";

                return result;
            }
            catch (Exception e)
            {
                TJLog.LogError($"[Import3DModelFromUrlTool] Query error: {e}");
                return Fail($"Error querying status: {e.Message}");
            }
#else
            return Fail("This tool only works in Unity Editor.");
#endif
        }

        public static object ListImportTasks(JObject parameters)
        {
#if UNITY_EDITOR
            try
            {
                var tasks = ImportModelTaskTracker.GetAllTasks();
                var taskList = new List<Dictionary<string, object>>();

                foreach (var t in tasks)
                {
                    var d = new Dictionary<string, object>
                    {
                        { "task_id",     t.TaskId },
                        { "status",      t.Status },
                        { "output_type", t.OutputType ?? "static" },
                        { "start_time",  t.StartTime.ToString("yyyy-MM-dd HH:mm:ss") }
                    };
                    if (!string.IsNullOrEmpty(t.PrefabPath))      d["prefab_path"] = t.PrefabPath;
                    if (!string.IsNullOrEmpty(t.ModelPath))        d["model_path"]  = t.ModelPath;
                    if (!string.IsNullOrEmpty(t.ErrorMessage))    d["error"]       = t.ErrorMessage;
                    taskList.Add(d);
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
                return Fail($"Error listing tasks: {e.Message}");
            }
#else
            return Fail("This tool only works in Unity Editor.");
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Builds the generator/host/pipeline for one import task and starts it.
        /// Shared by the initial submit and the domain-reload recovery (idempotent re-run).
        /// </summary>
        internal static void LaunchImport(ImportModelTaskTracker.ImportTaskInfo task)
        {
            string outputType = string.IsNullOrEmpty(task.OutputType) ? "static" : task.OutputType;
            string providerConfigId = ResolveProviderConfigId(outputType == "static" ? (task.Provider ?? "rodin") : null);
            string configId = ResolveGeneratorConfigId(outputType, providerConfigId);
            var config = ConfigManager.GetGeneratorConfig(ConfigType.Generator, configId);
            if (config == null)
            {
                string msg = $"Cannot find generator config '{configId}' for import (output_type={outputType}).";
                TJLog.LogError($"[Import3DModelFromUrlTool] {msg}");
                ImportModelTaskTracker.ApplyTaskUpdate(task, t =>
                {
                    t.Status       = "failed";
                    t.ErrorMessage = msg;
                    t.EndTime      = DateTime.Now;
                });
                GenerationNotifier.NotifyFailed(ToolName, task.TaskId, "", msg,
                    new JObject { ["session_id"] = task.SessionId ?? "", ["output_type"] = outputType });
                return;
            }

            var generator = new DynamicGenerator(config);

            if (outputType == "rigged")
            {
                if (!string.IsNullOrEmpty(task.SourceModelPath))
                    generator.SetFileUploadPath(task.SourceModelPath);
                if (!string.IsNullOrEmpty(task.Prompt))
                    generator.SetTextPrompt(task.Prompt);
            }
            else if (outputType == "motion")
            {
                string displayPrompt = !string.IsNullOrEmpty(task.MotionDescription)
                    ? task.MotionDescription
                    : task.Prompt;
                if (!string.IsNullOrEmpty(displayPrompt))
                    generator.SetTextPrompt(displayPrompt);
            }
            else
            {
                if (!string.IsNullOrEmpty(task.Prompt))
                    generator.SetTextPrompt(task.Prompt);
                if (task.AddMotion && !string.IsNullOrWhiteSpace(task.MotionDescription))
                    generator.SetAddMotion(true, task.MotionDescription.Trim());
            }

            string historyAssetPath = !string.IsNullOrEmpty(task.PrefabPath)
                ? task.PrefabPath
                : (!string.IsNullOrEmpty(task.TargetPrefabPath)
                    ? task.TargetPrefabPath
                    : task.RiggedModelPath);
            string historyGuid = CustomToolHistoryBindings.HistoryGuidFromPlaceholderAssetPath(historyAssetPath);

            var host = new ImportModelPipelineHost(task);
            var pipeline = new GenerationPipeline(
                host, ConfigType.Generator, GenerationRequestOrigin.Agent, task.SessionId ?? "", ToolName);
            var taskHandle = new TJGeneratorsTaskHandle();

            EditorCoroutineUtility.StartCoroutineOwnerless(
                pipeline.ImportModelFromUrl(
                    generator,
                    task.ModelUrl,
                    task.RenderedImageUrl,
                    historyGuid,
                    taskHandle));
        }

        /// <summary>静态导入的 provider 参数 → 生成器配置 id；rigged/motion 返回 null（走固定配置）。</summary>
        internal static string ResolveProviderConfigId(string provider)
        {
            if (string.IsNullOrEmpty(provider))
                return "rodin";

            switch (provider.Trim().ToLowerInvariant())
            {
                case "rodin":          return "rodin";
                case "tripo":
                case "tripo-p1":       return "tripo-p1";
                case "tripo-texture":
                case "tripo-texture-model": return "tripo-texture-model";
                case "hunyuan":        return "rodin"; // 混元 3D 生成器配置已移除；导入后处理与 rodin 静态链路一致
                default:               return null;
            }
        }

        /// <summary>output_type → 生成器配置 id（决定 DownloadModel 内的后处理分支）。</summary>
        private static string ResolveGeneratorConfigId(string outputType, string providerConfigId)
        {
            if (string.Equals(outputType, "rigged", StringComparison.OrdinalIgnoreCase))
                return "unirig";
            if (string.Equals(outputType, "motion", StringComparison.OrdinalIgnoreCase))
                return "hunyuan-motion";
            return string.IsNullOrEmpty(providerConfigId) ? "rodin" : providerConfigId;
        }

        private static string ResolvePrefabPath(
            string prefabOutputPath, string prompt, string modelUrl, string outputType, bool forceOverwrite)
        {
            if (string.IsNullOrEmpty(prefabOutputPath))
            {
                // prompt 仅作展示名来源：清洗非法路径字符并截断，避免长 prompt / 特殊字符破坏资产路径
                string baseName = BuildSafeBaseName(prompt);
                prefabOutputPath = $"Assets/TJGenerators/History/{outputType}_{baseName}.prefab";
                string dir = Path.GetDirectoryName(prefabOutputPath)?.Replace('\\', '/');
                if (!string.IsNullOrEmpty(dir))
                    PathUtils.EnsureAssetFolder(dir);
                prefabOutputPath = AssetDatabase.GenerateUniqueAssetPath(prefabOutputPath);
                if (string.IsNullOrEmpty(prefabOutputPath))
                    prefabOutputPath = $"Assets/TJGenerators/History/{outputType}_{baseName}.prefab";
            }
            else
            {
                prefabOutputPath = Path.ChangeExtension(prefabOutputPath, ".prefab");
                if (File.Exists(prefabOutputPath))
                {
                    if (forceOverwrite)
                    {
                        AssetDatabase.DeleteAsset(prefabOutputPath);
                    }
                    else
                    {
                        string dir = Path.GetDirectoryName(prefabOutputPath)?.Replace('\\', '/');
                        if (!string.IsNullOrEmpty(dir))
                            PathUtils.EnsureAssetFolder(dir);
                        prefabOutputPath = AssetDatabase.GenerateUniqueAssetPath(prefabOutputPath);
                    }
                }
            }
            return prefabOutputPath;
        }

        /// <summary>
        /// 从 prompt 派生精简资产名（默认 Prefab 与 History 分组共用）：
        /// 取首行 → 去行首冠词(a/an/the) → 标点转空格 → 空格折叠为下划线 → 限 24 字符按词边界截断。
        /// 生成 "{output_type}_<slug>.prefab" 风格的短名（如 static_volleyball_spherical）；空 prompt 回退 Import3D。
        /// </summary>
        internal static string BuildSafeBaseName(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                return "Import3D";

            string firstLine = prompt.Replace("\r", "").Split('\n')[0].Trim();
            if (string.IsNullOrEmpty(firstLine))
                return "Import3D";

            // 去行首冠词（可叠加）
            string[] articles = { "a ", "an ", "the " };
            bool stripped = true;
            while (stripped)
            {
                stripped = false;
                foreach (var art in articles)
                {
                    if (firstLine.Length > art.Length &&
                        firstLine.StartsWith(art, StringComparison.OrdinalIgnoreCase))
                    {
                        firstLine = firstLine.Substring(art.Length).TrimStart();
                        stripped = true;
                    }
                }
            }

            // 标点转空格（中文等字母数字保留）
            var sb = new StringBuilder();
            foreach (char c in firstLine)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    sb.Append(c);
                else
                    sb.Append(' ');
            }
            string joined = string.Join("_",
                sb.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            if (string.IsNullOrEmpty(joined))
                return "Import3D";

            // 限 24 字符，优先在词边界截断
            if (joined.Length > 24)
            {
                int cut = joined.LastIndexOf('_', 24);
                joined = cut > 0 ? joined.Substring(0, cut) : joined.Substring(0, 24);
            }

            joined = joined.Trim('_');
            return string.IsNullOrEmpty(joined) ? "Import3D" : joined;
        }

        /// <summary>URL 前 16 个十六进制字符 + 扩展名，作为确定性导入文件名（域重载重跑原位覆盖）。</summary>
        internal static string BuildUrlHashFileName(string url, string ext)
        {
            string source = string.IsNullOrEmpty(url) ? Guid.NewGuid().ToString("N") : url;
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(source));
                var sb = new StringBuilder();
                for (int i = 0; i < 8; i++)
                    sb.Append(hash[i].ToString("x2"));
                return sb.ToString() + (string.IsNullOrEmpty(ext) ? ".fbx" : ext.ToLowerInvariant());
            }
        }

        /// <summary>创建含 Cube 占位子节点的 Prefab（与被移除的 generate_3d_model_by_* 占位结构一致）。</summary>
        internal static string CreateBlankPrefab(string path)
        {
            path = Path.ChangeExtension(path, ".prefab").Replace("\\", "/");

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var root = new GameObject(Path.GetFileNameWithoutExtension(path));
            try
            {
                var placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                placeholder.name = "Placeholder";
                placeholder.transform.SetParent(root.transform);
                placeholder.transform.localPosition = Vector3.zero;
                placeholder.transform.localRotation = Quaternion.identity;
                placeholder.transform.localScale    = Vector3.one;

                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TJGeneratorsGenerationLabel.EnableLabel(TJGeneratorsAssetReference.FromPath(path));

            return path;
        }

        internal static Dictionary<string, object> Fail(string message)
        {
            return new Dictionary<string, object>
            {
                { "success", false },
                { "message", message }
            };
        }
#endif
    }
}
