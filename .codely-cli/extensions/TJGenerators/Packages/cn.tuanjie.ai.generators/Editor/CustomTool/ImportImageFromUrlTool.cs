using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    /// 图片导入任务追踪（持久化，域重载后可幂等重跑：URL 永久、目标路径确定性）。
    /// </summary>
    public static class ImportImageTaskTracker
    {
#if UNITY_EDITOR
        [Serializable]
        private class PersistedTask
        {
            public string taskId;
            public string sessionId;
            public string imageUrlsJoined;   // '\n' joined
            public string namesJoined;       // '\n' joined
            public string outputPath;
            public string importType;
            public string materialPath;
            public string prompt;
            public string status;
            public int    progress;
            public string placeholderPath;
            public string imagePathsJoined;  // '\n' joined (results)
            public string historyTaskId;
            public string errorMessage;
            public long   startTimeTicks;
            public long   endTimeTicks;
        }

        public class ImportImageTaskInfo : IGenerationTaskInfo
        {
            public string TaskId        { get; set; }
            public string SessionId     { get; set; }
            public List<string> ImageUrls { get; set; } = new List<string>();
            public List<string> Names     { get; set; } = new List<string>();
            public string OutputPath     { get; set; }
            public string ImportType     { get; set; } = "image";
            public string MaterialPath   { get; set; }
            public string Prompt         { get; set; }
            public string Status         { get; set; }
            public int    Progress       { get; set; }
            public string PlaceholderPath { get; set; }
            public List<string> ImagePaths { get; set; } = new List<string>();
            public string HistoryTaskId  { get; set; }
            public string ErrorMessage   { get; set; }
            public DateTime StartTime    { get; set; }
            public DateTime? EndTime     { get; set; }
            public string BackendTaskId  { get; set; }
            public string PreviewUrl     { get; set; }
        }

        private static readonly GenerationTaskTrackerStore<ImportImageTaskInfo, PersistedTask> Store =
            new GenerationTaskTrackerStore<ImportImageTaskInfo, PersistedTask>(
                "TJGen_ImportImage",
                BuildPersisted,
                FromPersisted);

        private const string JoinSep = "\n";

        private static string Join(List<string> list) => list == null ? "" : string.Join(JoinSep, list);
        private static List<string> Split(string joined) =>
            string.IsNullOrEmpty(joined) ? new List<string>() : joined.Split('\n').ToList();

        private static PersistedTask BuildPersisted(ImportImageTaskInfo info) => new PersistedTask
        {
            taskId            = info.TaskId ?? "",
            sessionId         = info.SessionId ?? "",
            imageUrlsJoined   = Join(info.ImageUrls),
            namesJoined       = Join(info.Names),
            outputPath        = info.OutputPath ?? "",
            importType        = info.ImportType ?? "image",
            materialPath      = info.MaterialPath ?? "",
            prompt            = info.Prompt ?? "",
            status            = info.Status ?? "",
            progress          = info.Progress,
            placeholderPath   = info.PlaceholderPath ?? "",
            imagePathsJoined  = Join(info.ImagePaths),
            historyTaskId     = info.HistoryTaskId ?? "",
            errorMessage      = info.ErrorMessage ?? "",
            startTimeTicks    = info.StartTime.Ticks,
            endTimeTicks      = info.EndTime?.Ticks ?? 0
        };

        private static ImportImageTaskInfo FromPersisted(PersistedTask p) => new ImportImageTaskInfo
        {
            TaskId          = p.taskId,
            SessionId       = p.sessionId,
            ImageUrls       = Split(p.imageUrlsJoined),
            Names           = Split(p.namesJoined),
            OutputPath      = p.outputPath,
            ImportType      = string.IsNullOrEmpty(p.importType) ? "image" : p.importType,
            MaterialPath    = p.materialPath ?? "",
            Prompt          = p.prompt,
            Status          = p.status,
            Progress        = p.progress,
            PlaceholderPath = p.placeholderPath,
            ImagePaths      = Split(p.imagePathsJoined),
            HistoryTaskId   = p.historyTaskId,
            ErrorMessage    = p.errorMessage,
            StartTime       = new DateTime(p.startTimeTicks),
            EndTime         = p.endTimeTicks > 0 ? (DateTime?)new DateTime(p.endTimeTicks) : null,
            BackendTaskId   = "",
            PreviewUrl      = p.imageUrlsJoined
        };

        internal static void ApplyTaskUpdate(ImportImageTaskInfo task, Action<ImportImageTaskInfo> mutate) =>
            Store.ApplyTaskUpdate(task, mutate);

        internal static void RegisterTask(ImportImageTaskInfo task) =>
            Store.RegisterTask(task.TaskId, task);

        internal static string AllocateTaskId() => Store.AllocateTaskId("import_image");

        public static ImportImageTaskInfo GetTask(string taskId) => Store.GetTask(taskId);
        public static List<ImportImageTaskInfo> GetAllTasks() => Store.GetAllTasks();
#endif
    }

    /// <summary>
    /// Replaces the removed Unity-side image generation tools (generate_image /
    /// generate_image_layers / upscale_image): generation now runs on the backend MCP
    /// (generate_image / generate_image_layers / upscale_image + file_upload + check_task);
    /// this tool owns download + import + post-processing for permanent CDN image URLs.
    /// </summary>
    public static class ImportImageFromUrlTool
    {
        private const string ToolName = "import_image_from_url";

        [ExecuteCustomTool.CustomTool(ToolName,
            "Import already-generated image(s) from permanent CDN URL(s) into Unity, with the standard image " +
            "post-processing chain (file-level in-place replacement with a stable GUID placeholder for single-image " +
            "mode, TextureImporterType.Default + alphaIsTransparency, TuanjieAI/Session labels, History record). " +
            "URLs come from the MCP backend image flow (generate_image / generate_image_layers / upscale_image -> " +
            "check_task -> image URL), NOT from a local path. " +
            "Key parameters: image_url (single image) OR image_urls (comma-separated, e.g. image layers: base first, " +
            "then layers), output_path (optional: file path for single image mode / folder path for multi-image mode; " +
            "default Assets/TJGenerators/History/<ImportImage/<url-hash>.png | ImportLayers_<timestamp>>), " +
            "names (optional comma-separated file names for multi-image mode, e.g. layer_0,layer_1), " +
            "prompt (optional display text for History), session_id. import_type defaults to 'image'; use 'skybox' for one color panorama " +
            "from MCP generate_skybox: imports as Cubemap and creates/reuses a Skybox/Cubemap material, returning material_path. " +
            "Skybox mode does not change the scene; apply the material after completion if requested. Do not import depth maps as skyboxes. " +
            "Supported extensions: .png / .jpg / .jpeg (Unity cannot import .webp — regenerate with output_format png|jpeg). " +
            "Existing files at the target path are overwritten in place. " +
            "RELIABLE COMPLETION FLOW (the Unity-side <bg_task_done> push may never reach the session): after placing " +
            "the placeholder, run a foreground shell wait of at most 30 seconds in this subagent, then " +
            "call query_local_task ONCE with task_id. Do not finish the subagent while importing; " +
            "if it still reports 'importing', run another bounded foreground wait and query again. " +
            "Domain reload safe: in-flight imports are automatically re-launched (URL is permanent, target path deterministic).")]
        public static object ImportImageFromUrl(JObject parameters)
        {
#if UNITY_EDITOR
            try
            {
                TJLog.Log($"[ImportImageFromUrlTool] Import with parameters: {parameters}");

                if (TJGeneratorsPlayModeGuard.IsActive)
                    return Fail(TJGeneratorsPlayModeGuard.Message);

                List<string> imageUrls = ParseUrlList(parameters["image_url"]?.ToString(), parameters["image_urls"]?.ToString());
                if (imageUrls.Count == 0)
                    return Fail("'image_url' or 'image_urls' is required. Get it from the MCP backend: " +
                                "generate_image / generate_image_layers / upscale_image -> check_task -> image URL (.png/.jpg/.jpeg).");

                foreach (string url in imageUrls)
                {
                    string ext = Path.GetExtension(new Uri(url).AbsolutePath)?.ToLowerInvariant();
                    if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
                        return Fail($"Unsupported image URL extension '{ext}'. Supported: .png / .jpg / .jpeg. " +
                                    "(If the URL is .webp, regenerate via MCP with output_format 'png' or 'jpeg'.) URL: {url}");
                }

                bool isMulti = imageUrls.Count > 1;
                string importType = parameters["import_type"]?.ToString() ?? "image";
                if (importType != "image" && importType != "skybox")
                    return Fail("import_type must be 'image' or 'skybox'.");
                bool isSkybox = importType == "skybox";
                if (isSkybox && isMulti) return Fail("Skybox import requires one color panorama URL.");
                string outputPath = parameters["output_path"]?.ToString();
                string prompt     = parameters["prompt"]?.ToString();
                string sessionId  = parameters["session_id"]?.ToString() ?? "";
                List<string> names = ParseNameList(parameters["names"]?.ToString(), imageUrls, isMulti);

                string placeholderPath = "";
                string historyTaskId = "";
                string materialPath = "";

                if (!isMulti)
                {
                    // 单图：文件级占位（GUID 稳定，下载完成后原地覆盖文件内容）
                    placeholderPath = string.IsNullOrEmpty(outputPath)
                        ? (isSkybox ? "Assets/TJGenerators/History/ImportSkybox/" : "Assets/TJGenerators/History/ImportImage/") +
                          Import3DModelFromUrlTool.BuildUrlHashFileName(imageUrls[0], Path.GetExtension(new Uri(imageUrls[0]).AbsolutePath))
                        : NormalizeAssetFilePath(outputPath);
                    if (isSkybox)
                    {
                        if (!PathUtils.TryNormalizeOutputAssetPath(outputPath ?? placeholderPath, out placeholderPath, out string pathError))
                            return Fail(pathError);
                        if (string.IsNullOrEmpty(Path.GetExtension(placeholderPath)))
                            placeholderPath += Path.GetExtension(new Uri(imageUrls[0]).AbsolutePath);
                        string ext = Path.GetExtension(placeholderPath).ToLowerInvariant();
                        if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
                            return Fail("Skybox output_path must be a .png/.jpg/.jpeg image, not a material path.");
                        materialPath = SkyboxTaskTracker.DeriveMaterialPath(placeholderPath);
                        ValidateSkyboxTarget(placeholderPath, materialPath);
                    }
                    EnsureAssetFolder(Path.GetDirectoryName(placeholderPath));
                    // Preserve an existing image until its replacement has downloaded successfully.
                    if (!File.Exists(PathUtils.ToAbsoluteAssetPath(placeholderPath)) &&
                        !WritePlaceholderImage(placeholderPath, isSkybox))
                        return Fail($"Failed to create placeholder at: {placeholderPath}");
                    AssetDatabase.ImportAsset(placeholderPath, ImportAssetOptions.ForceUpdate);
                    // Existing skyboxes remain untouched until a valid replacement has downloaded.
                    if (isSkybox && AssetDatabase.LoadAssetAtPath<Cubemap>(placeholderPath) == null)
                        ConfigureSkybox(placeholderPath, materialPath, sessionId);
                    TJGeneratorsGenerationLabel.EnableLabel(TJGeneratorsAssetReference.FromPath(placeholderPath));
                    TJGeneratorsGenerationLabel.EnableSessionLabel(TJGeneratorsAssetReference.FromPath(placeholderPath), sessionId);
                    historyTaskId = TJGeneratorsHistoryManager.AddGeneratingPlaceholder(
                        prompt ?? "", "", ToolName, true,
                        CustomToolHistoryBindings.HistoryGuidFromPlaceholderAssetPath(placeholderPath),
                        prompt, sessionId);
                }
                else
                {
                    // 多图：批量导入到目标文件夹，无占位；History 以 layer_0 完成后逐张补录
                    if (string.IsNullOrEmpty(outputPath))
                        outputPath = "Assets/TJGenerators/History/ImportLayers_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    EnsureAssetFolder(outputPath.EndsWith("/") ? outputPath : outputPath + "/");
                }

                var task = new ImportImageTaskTracker.ImportImageTaskInfo
                {
                    TaskId         = ImportImageTaskTracker.AllocateTaskId(),
                    SessionId      = sessionId,
                    ImageUrls      = imageUrls,
                    Names          = names,
                    OutputPath     = outputPath ?? "",
                    ImportType     = importType,
                    MaterialPath   = materialPath,
                    Prompt         = prompt ?? "",
                    Status         = "importing",
                    Progress       = 0,
                    PlaceholderPath = placeholderPath,
                    HistoryTaskId  = historyTaskId,
                    StartTime      = DateTime.Now
                };
                ImportImageTaskTracker.RegisterTask(task);

                LaunchImport(task);

                var result = new Dictionary<string, object>
                {
                    { "success",              true },
                    { "submission_success",   true },
                    { "task_id",              task.TaskId },
                    { "status",               "importing" },
                    { "image_count",          imageUrls.Count },
                    { "estimated_wait_seconds", 30 },
                    { "notification_mode",    "query_status" },
                    { "message",
                        (string.IsNullOrEmpty(placeholderPath)
                            ? "Import started (multi-image mode — no placeholder; all images land in output_path folder). "
                            : "Import started. STEP 1 (do now): The placeholder asset is at placeholder_path (a small gray image, GUID-stable) — you may place/reference it right away. ") +
                        "STEP 2 (do now): Wait in this subagent with run_shell_command(run_in_background=false), using Start-Sleep / sleep for at most 30 seconds. " +
                        "Keep ownership of this import; a timer finishing is not import completion, and the Unity-side push may never arrive. " +
                        "STEP 3: Do not call complete_task or return final text while the import is pending. " +
                        "STEP 4 (after the foreground wait returns): Call query_local_task ONCE with this task_id to fetch image_path / layer fields; " +
                        "if it still reports 'importing', run another bounded foreground wait before querying again. " +
                        "The placeholder file content is replaced in place (GUID stable) — do NOT place again. " +
                        "*** Never wait indefinitely for a Unity-side notification; never loop query_local_task within one wake-up. ***" }
                };
                if (!string.IsNullOrEmpty(placeholderPath))
                {
                    result["placeholder_path"] = placeholderPath;
                    result["image_url"] = imageUrls[0];
                    if (isSkybox)
                    {
                        result["import_type"] = "skybox";
                        if (AssetDatabase.LoadAssetAtPath<Material>(materialPath) != null)
                            result["placeholder_material_path"] = materialPath;
                        result["expected_material_path"] = materialPath;
                    }
                }
                else
                {
                    result["layers_folder"] = (outputPath ?? "").TrimEnd('/');
                    result["image_urls"] = string.Join(",", imageUrls);
                }
                return result;
            }
            catch (Exception e)
            {
                TJLog.LogError($"[ImportImageFromUrlTool] Error: {e}");
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

                var task = ImportImageTaskTracker.GetTask(taskId);
                if (task == null)
                    return Fail($"Task '{taskId}' not found. It may have been cleaned up or Unity was fully restarted.");

                var result = new Dictionary<string, object>
                {
                    { "success",    true },
                    { "task_id",    task.TaskId },
                    { "status",     task.Status },
                    { "progress",   task.Progress },
                    { "image_count", task.ImageUrls.Count },
                    { "start_time", task.StartTime.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                if (task.ImagePaths.Count == 1)
                {
                    result["image_path"]  = task.ImagePaths[0];
                    result["preview_url"] = task.PreviewUrl ?? "";
                }
                else if (task.ImagePaths.Count > 1)
                {
                    result["layer_0_path"] = task.ImagePaths[0];
                    result["layers_folder"] = task.OutputPath?.TrimEnd('/') ?? "";
                    result["layer_count"]   = task.ImagePaths.Count;
                    result["layer_paths"]   = task.ImagePaths.ToArray();
                    result["preview_url"]   = task.PreviewUrl ?? "";
                }

                if (!string.IsNullOrEmpty(task.PlaceholderPath)) result["placeholder_path"] = task.PlaceholderPath;
                if (task.ImportType == "skybox")
                {
                    result["import_type"] = "skybox";
                    if (task.Status == "completed")
                    {
                        result["texture_path"] = task.ImagePaths.FirstOrDefault() ?? "";
                        result["material_path"] = task.MaterialPath ?? "";
                    }
                }
                if (!string.IsNullOrEmpty(task.ErrorMessage))    result["error"]            = task.ErrorMessage;
                if (task.EndTime.HasValue)
                {
                    result["end_time"]         = task.EndTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    result["duration_seconds"] = (int)(task.EndTime.Value - task.StartTime).TotalSeconds;
                }

                if (task.Status == "recovering")
                    result["hint"] = "The import was interrupted by a domain reload and is being re-launched automatically. Wait in this subagent for up to 30 seconds, then check this same task again.";

                if (task.Status == "completed" && task.ImagePaths.Count > 0)
                    result["result_summary"] = task.ImagePaths.Count == 1
                        ? $"Import completed. Image: {task.ImagePaths[0]}."
                        : $"Import completed. {task.ImagePaths.Count} images under: {task.OutputPath} (layer_0_path is the base image).";

                return result;
            }
            catch (Exception e)
            {
                TJLog.LogError($"[ImportImageFromUrlTool] Query error: {e}");
                return Fail($"Error: {e.Message}");
            }
#else
            return Fail("This tool only works in Unity Editor.");
#endif
        }

        // ------------------------------------------------------------------
        // import pipeline
        // ------------------------------------------------------------------
#if UNITY_EDITOR
        internal static void LaunchImport(ImportImageTaskTracker.ImportImageTaskInfo task)
        {
            if (task == null || task.ImageUrls == null || task.ImageUrls.Count == 0) return;
            EditorCoroutineUtility.StartCoroutineOwnerless(DownloadAndImport(task));
        }

        private static IEnumerator DownloadAndImport(ImportImageTaskTracker.ImportImageTaskInfo task)
        {
            var imported = new List<string>();
            for (int i = 0; i < task.ImageUrls.Count; i++)
            {
                string url = task.ImageUrls[i];
                string targetPath = ResolveTargetPath(task, i);

                using (var request = UnityWebRequest.Get(url))
                {
                    request.timeout = 300;
                    yield return request.SendWebRequest();

                    bool hasError = UnityWebRequestCompat.IsNotSuccess(request);
                    if (hasError)
                    {
                        ImportImageTaskTracker.ApplyTaskUpdate(task, t =>
                        {
                            t.Status        = "failed";
                            t.ErrorMessage  = $"Download failed ({request.error}): {url}";
                            t.EndTime       = DateTime.Now;
                        });
                        GenerationNotifier.NotifyFailed(ToolName, task.TaskId, "", task.ErrorMessage,
                            new JObject { ["session_id"] = task.SessionId ?? "", ["prompt"] = task.Prompt ?? "" });
                        TJLog.LogError($"[ImportImageFromUrlTool] {task.ErrorMessage}");
                        yield break;
                    }

                    try
                    {
                        if (task.ImportType == "skybox") ValidateSkyboxBytes(request.downloadHandler.data);
                        string absPath = PathUtils.ToAbsoluteAssetPath(targetPath);
                        string dir = Path.GetDirectoryName(absPath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        File.WriteAllBytes(absPath, request.downloadHandler.data);
                    }
                    catch (Exception e)
                    {
                        ImportImageTaskTracker.ApplyTaskUpdate(task, t =>
                        {
                            t.Status       = "failed";
                            t.ErrorMessage = $"Save failed: {e.Message}";
                            t.EndTime      = DateTime.Now;
                        });
                        GenerationNotifier.NotifyFailed(ToolName, task.TaskId, "", task.ErrorMessage,
                            new JObject { ["session_id"] = task.SessionId ?? "", ["prompt"] = task.Prompt ?? "" });
                        yield break;
                    }
                }

                Exception importError = null;
                try
                {
                    AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
                    if (task.ImportType == "skybox")
                        ConfigureSkybox(targetPath, task.MaterialPath, task.SessionId);
                    else
                        GeneratedTextureImportUtils.ConfigureImportedTexture(targetPath, TextureImporterType.Default, alphaIsTransparency: true);
                }
                catch (Exception e) { importError = e; }
                if (importError != null)
                {
                    ImportImageTaskTracker.ApplyTaskUpdate(task, t =>
                    {
                        t.Status = "failed";
                        t.ErrorMessage = "Import failed: " + importError.Message;
                        t.EndTime = DateTime.Now;
                    });
                    GenerationNotifier.NotifyFailed(ToolName, task.TaskId, "", task.ErrorMessage,
                        new JObject { ["session_id"] = task.SessionId ?? "" });
                    yield break;
                }
                TJGeneratorsGenerationLabel.EnableLabel(TJGeneratorsAssetReference.FromPath(targetPath));
                TJGeneratorsGenerationLabel.EnableSessionLabel(TJGeneratorsAssetReference.FromPath(targetPath), task.SessionId ?? "");

                imported.Add(targetPath);
                ImportImageTaskTracker.ApplyTaskUpdate(task, t =>
                {
                    t.ImagePaths = new List<string>(imported);
                    t.Progress   = (int)((i + 1) * 100.0f / task.ImageUrls.Count);
                });
            }

            CompleteTask(task, imported);
        }

        private static void CompleteTask(ImportImageTaskTracker.ImportImageTaskInfo task, List<string> imported)
        {
            ImportImageTaskTracker.ApplyTaskUpdate(task, t =>
            {
                t.Status     = "completed";
                t.Progress   = 100;
                t.ImagePaths = imported;
                t.PreviewUrl = task.ImageUrls.Count > 0 ? task.ImageUrls[0] : "";
                t.EndTime    = DateTime.Now;
            });

            // History：单图完成占位；多图首张完成占位、其余逐张补录（一图一格）。
            // 多图模式无前置占位资产：以 layer_0 落盘后的 GUID 事后建 History 并立即完成。
            string historyTaskId = task.HistoryTaskId;
            if (string.IsNullOrEmpty(historyTaskId) && imported.Count > 1)
            {
                historyTaskId = TJGeneratorsHistoryManager.AddGeneratingPlaceholder(
                    task.Prompt ?? "", "", ToolName, true,
                    CustomToolHistoryBindings.HistoryGuidFromPlaceholderAssetPath(imported[0]),
                    task.Prompt, task.SessionId ?? "");
                ImportImageTaskTracker.ApplyTaskUpdate(task, t => t.HistoryTaskId = historyTaskId);
            }
            if (!string.IsNullOrEmpty(historyTaskId))
            {
                if (imported.Count == 1)
                    TJGeneratorsHistoryManager.CompletePlaceholder(task.HistoryTaskId, imported[0], task.PreviewUrl, null);
                else
                    TJGeneratorsHistoryManager.CompletePlaceholderMultiImage(task.HistoryTaskId, imported, task.ImageUrls.ToArray(), null);
            }

            var payload = new JObject
            {
                ["session_id"]       = task.SessionId ?? "",
                ["prompt"]           = task.Prompt ?? "",
                ["preview_url"]      = task.PreviewUrl ?? "",
                ["progress"]         = 100,
                ["start_time"]       = task.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                ["end_time"]         = task.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                ["duration_seconds"] = task.EndTime.HasValue
                    ? (int)(task.EndTime.Value - task.StartTime).TotalSeconds
                    : 0
            };
            if (imported.Count == 1)
            {
                payload["image_path"] = imported[0];
                if (task.ImportType == "skybox")
                {
                    payload["import_type"] = "skybox";
                    payload["texture_path"] = imported[0];
                    payload["material_path"] = task.MaterialPath;
                }
            }
            else
            {
                payload["layer_0_path"] = imported[0];
                payload["layers_folder"] = task.OutputPath?.TrimEnd('/') ?? "";
                payload["layer_count"]   = imported.Count;
                payload["layer_paths"]   = new JArray(imported.ToArray());
            }

            GenerationNotifier.NotifyCompleted(ToolName, task.TaskId, "", payload);
            TJLog.Log($"[ImportImageFromUrlTool] Import completed: {imported.Count} image(s), first: {(imported.Count > 0 ? imported[0] : "-")}");
        }

        // ------------------------------------------------------------------
        // helpers
        // ------------------------------------------------------------------

        private static void ValidateSkyboxTarget(string texturePath, string materialPath)
        {
            if (Shader.Find("Skybox/Cubemap") == null)
                throw new InvalidOperationException("Skybox/Cubemap shader is unavailable in this project.");
            var existing = AssetDatabase.LoadMainAssetAtPath(texturePath);
            if (existing != null && !(existing is Cubemap))
                throw new InvalidOperationException("Existing output is not a Cubemap. Choose a new skybox output_path.");
            var asset = AssetDatabase.LoadMainAssetAtPath(materialPath);
            if (asset != null && (!(asset is Material material) || material.shader == null || material.shader.name != "Skybox/Cubemap"))
                throw new InvalidOperationException("Existing material is not a Skybox/Cubemap material. Choose a new output_path.");
        }

        internal static void ValidateSkyboxBytes(byte[] bytes)
        {
            var image = new Texture2D(2, 2);
            try
            {
                if (!image.LoadImage(bytes) || image.width < 2 || image.width != image.height * 2)
                    throw new InvalidOperationException("Skybox requires a decodable 2:1 color panorama; existing assets were not overwritten.");
            }
            finally { UnityEngine.Object.DestroyImmediate(image); }
        }

        internal static void ConfigureSkybox(string texturePath, string materialPath, string sessionId)
        {
            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Skybox texture importer not found.");
            importer.textureType = TextureImporterType.Default;
            importer.textureShape = TextureImporterShape.TextureCube;
            importer.generateCubemap = TextureImporterGenerateCubemap.AutoCubemap;
            importer.SaveAndReimport();
            var cube = AssetDatabase.LoadAssetAtPath<Cubemap>(texturePath);
            if (cube == null) throw new InvalidOperationException("Skybox did not import as a Cubemap.");
            var shader = Shader.Find("Skybox/Cubemap");
            if (shader == null) throw new InvalidOperationException("Skybox/Cubemap shader not found.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }
            material.SetTexture("_Tex", cube);
            EditorUtility.SetDirty(material);
#if UNITY_2021_2_OR_NEWER
            AssetDatabase.SaveAssetIfDirty(material);
#else
            // Legacy Unity has no single-asset save API (same fallback as the existing skybox pipeline).
            AssetDatabase.SaveAssets();
#endif
            TJGeneratorsGenerationLabel.EnableLabel(TJGeneratorsAssetReference.FromPath(materialPath));
            TJGeneratorsGenerationLabel.EnableSessionLabel(TJGeneratorsAssetReference.FromPath(materialPath), sessionId ?? "");
        }

        private static List<string> ParseUrlList(string single, string multi)
        {
            var urls = new List<string>();
            if (!string.IsNullOrWhiteSpace(multi))
                foreach (string u in multi.Split(','))
                    if (!string.IsNullOrWhiteSpace(u)) urls.Add(u.Trim());
            if (urls.Count == 0 && !string.IsNullOrWhiteSpace(single))
                urls.Add(single.Trim());
            return urls;
        }

        private static List<string> ParseNameList(string namesJoined, List<string> urls, bool isMulti)
        {
            var names = new List<string>();
            if (!string.IsNullOrWhiteSpace(namesJoined))
                foreach (string n in namesJoined.Split(','))
                    if (!string.IsNullOrWhiteSpace(n)) names.Add(PathUtils.SanitizeAssetFolderName(n.Trim()));
            while (names.Count < urls.Count)
                names.Add(isMulti ? $"layer_{names.Count}" : "image");
            return names;
        }

        private static string NormalizeAssetFilePath(string path)
        {
            path = path.Replace("\\", "/");
            if (!path.StartsWith("Assets/"))
                path = "Assets/" + path.TrimStart('/');
            return path;
        }

        private static void EnsureAssetFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder)) return;
            string abs = PathUtils.ToAbsoluteAssetPath(folder);
            if (!string.IsNullOrEmpty(abs) && !Directory.Exists(abs))
                Directory.CreateDirectory(abs);
        }

        private static string ResolveTargetPath(ImportImageTaskTracker.ImportImageTaskInfo task, int index)
        {
            string url = task.ImageUrls[index];
            string ext = Path.GetExtension(new Uri(url).AbsolutePath);
            if (string.IsNullOrEmpty(ext)) ext = ".png";

            if (task.ImageUrls.Count == 1)
            {
                // 单图：占位路径即最终路径（原地覆盖，GUID 稳定）
                if (!string.IsNullOrEmpty(task.PlaceholderPath)) return task.PlaceholderPath;
                return "Assets/TJGenerators/History/ImportImage/" +
                       Import3DModelFromUrlTool.BuildUrlHashFileName(url, ext);
            }

            // 多图：目标文件夹 + names[i] 或 URL 哈希
            string folder = (task.OutputPath ?? "Assets/TJGenerators/History/ImportLayers").TrimEnd('/');
            string name = (index < task.Names.Count && !string.IsNullOrEmpty(task.Names[index]))
                ? Path.ChangeExtension(task.Names[index], ext)
                : Import3DModelFromUrlTool.BuildUrlHashFileName(url, ext);
            return (folder + "/" + name).Replace("\\", "/");
        }

        /// <summary>写入小灰图占位文件（单图模式：真实图片下载后原地覆盖，GUID 不变）。</summary>
        private static bool WritePlaceholderImage(string assetPath, bool skybox = false)
        {
            try
            {
                string absPath = PathUtils.ToAbsoluteAssetPath(assetPath);
                if (string.IsNullOrEmpty(absPath)) return false;
                string dir = Path.GetDirectoryName(absPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var tex = new Texture2D(skybox ? 16 : 8, 8, TextureFormat.RGBA32, false);
                var pixels = new Color32[tex.width * tex.height];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(160, 160, 160, 255);
                tex.SetPixels32(pixels);
                tex.Apply();

                string ext = Path.GetExtension(assetPath)?.ToLowerInvariant();
                byte[] bytes = (ext == ".jpg" || ext == ".jpeg") ? tex.EncodeToJPG(80) : tex.EncodeToPNG();
                UnityEngine.Object.DestroyImmediate(tex);

                File.WriteAllBytes(absPath, bytes);
                return File.Exists(absPath);
            }
            catch (Exception e)
            {
                TJLog.LogError($"[ImportImageFromUrlTool] WritePlaceholderImage failed: {e.Message}");
                return false;
            }
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

#if UNITY_EDITOR
    /// <summary>
    /// Re-launches in-flight image import tasks after a domain reload.
    /// The image URL is a permanent CDN URL and the target path is deterministic
    /// (placeholder path / output folder), so re-running the import simply overwrites in place.
    /// </summary>
    [InitializeOnLoad]
    public static class ImportImageDomainReloadRecovery
    {
        static ImportImageDomainReloadRecovery()
        {
            CustomToolDomainReloadRecovery.Schedule(ResumeInterruptedImports);
        }

        private static void ResumeInterruptedImports()
        {
            foreach (var task in ImportImageTaskTracker.GetAllTasks())
            {
                if (task == null) continue;
                if (task.Status != "importing" && task.Status != "recovering") continue;
                if (task.ImageUrls == null || task.ImageUrls.Count == 0) continue;

                TJLog.Log($"[ImportImageDomainReloadRecovery] Resuming image import task: {task.TaskId}");
                ImportImageTaskTracker.ApplyTaskUpdate(task, t => t.Status = "recovering");
                ImportImageFromUrlTool.LaunchImport(task);
            }
        }
    }
#endif
}
