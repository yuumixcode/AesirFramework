using System;
using System.Collections.Generic;
using System.Linq;
using Codely.Newtonsoft.Json.Linq;

namespace UnityTcp.Editor.Tools
{
    /// <summary>One public status surface over the existing Unity task stores. No MCP requests or resubmission.</summary>
    public static class LocalTaskTools
    {
#if UNITY_EDITOR
        private sealed class Source
        {
            public string Type;
            public Func<string, bool> Contains;
            public Func<IEnumerable<string>> Ids;
            public Func<JObject, object> Query;
        }

        private static readonly Source[] Sources =
        {
            new Source { Type = "image", Contains = id => ImageTaskTracker.GetTask(id) != null,
                Ids = () => ImageTaskTracker.GetAllTasks().Select(t => t.TaskId), Query = GenerateImageTool.QueryImageStatus },
            new Source { Type = "image_layers", Contains = id => ImageLayersTaskTracker.GetTask(id) != null,
                Ids = () => ImageLayersTaskTracker.GetAllTasks().Select(t => t.TaskId),
                Query = p => GenerateGameUiKitTool.BuildLayersTaskResult(ImageLayersTaskTracker.GetTask((string)p["task_id"])) },
            new Source { Type = "sprite", Contains = id => SpriteTaskTracker.GetTask(id) != null,
                Ids = () => SpriteTaskTracker.GetAllTasks().Select(t => t.TaskId), Query = GenerateSpriteTool.QuerySpriteStatus },
            new Source { Type = "material", Contains = id => MaterialTaskTracker.GetTask(id) != null,
                Ids = () => MaterialTaskTracker.GetAllTasks().Select(t => t.TaskId), Query = GenerateMaterialTool.QueryMaterialStatus },
            new Source { Type = "skybox", Contains = id => SkyboxTaskTracker.GetTask(id) != null,
                Ids = () => SkyboxTaskTracker.GetAllTasks().Select(t => t.TaskId), Query = GenerateSkyboxTool.QuerySkyboxStatus },
            new Source { Type = "sprite_sequence", Contains = id => SpriteSequenceTaskTracker.GetTask(id) != null,
                Ids = () => SpriteSequenceTaskTracker.GetAllTasks().Select(t => t.TaskId), Query = GenerateSpriteSequenceTool.QuerySpriteSequenceStatus },
            new Source { Type = "auto_sprite_sequence", Contains = id => AutoSpriteSequenceTaskTracker.GetTask(id) != null,
                Ids = () => AutoSpriteSequenceTaskTracker.GetAllTasks().Select(t => t.TaskId), Query = GenerateImageTool.Query2DSpriteSequenceAutoStatus },
            new Source { Type = "terrain", Contains = id => TerrainTaskTracker.GetTask(id) != null,
                Ids = () => TerrainTaskTracker.GetAllTasks().Select(t => t.TaskId), Query = GenerateTerrainTool.QueryTerrainStatus },
            new Source { Type = "terrain_apply", Contains = id => TerrainApplyTaskTracker.GetTask(id) != null,
                Ids = () => TerrainApplyTaskTracker.GetAllTasks().Select(t => t.ApplyTaskId),
                Query = p => GenerateTerrainTool.QueryTerrainApplyStatus(new JObject { ["apply_task_id"] = p["task_id"] }) },
            new Source { Type = "model_import", Contains = id => ImportModelTaskTracker.GetTask(id) != null,
                Ids = () => ImportModelTaskTracker.GetAllTasks().Select(t => t.TaskId), Query = Import3DModelFromUrlTool.QueryImportStatus },
            new Source { Type = "image_import", Contains = id => ImportImageTaskTracker.GetTask(id) != null,
                Ids = () => ImportImageTaskTracker.GetAllTasks().Select(t => t.TaskId), Query = ImportImageFromUrlTool.QueryImportStatus },
            new Source { Type = "audio_import", Contains = id => AudioImportTaskTracker.GetTask(id) != null,
                Ids = () => AudioImportTaskTracker.GetAllTasks().Select(t => t.TaskId), Query = ImportMediaFromUrlTool.QueryAudioImportStatus },
            new Source { Type = "video_import", Contains = id => VideoImportTaskTracker.GetTask(id) != null,
                Ids = () => VideoImportTaskTracker.GetAllTasks().Select(t => t.TaskId), Query = ImportMediaFromUrlTool.QueryVideoImportStatus }
        };

        private static JObject Read(Source source, string id, bool readOnly = false)
        {
            var result = JObject.FromObject(source.Query(new JObject { ["task_id"] = id, ["read_only"] = readOnly }));
            result["task_id"] = id; // Terrain also retains its original apply_task_id field.
            result["task_type"] = source.Type;
            if (source.Type == "sprite_sequence") result["route"] = "legacy_sprite_sequence";
            if (source.Type == "auto_sprite_sequence") result["route"] = "auto_frontier_sequence";
            return result;
        }

        private static object Fail(string message, string code = "INVALID_PARAMS") =>
            new JObject { ["success"] = false, ["error_code"] = code, ["message"] = message };

        private static Source[] SelectSources(string type) => string.IsNullOrEmpty(type)
            ? Sources
            : Sources.Where(s => string.Equals(s.Type, type, StringComparison.OrdinalIgnoreCase)).ToArray();
#endif

        [ExecuteCustomTool.CustomTool("query_local_task",
            "Query ONE Unity-local generation, import or post-processing task. Required: task_id (use the apply_task_id value for terrain application); " +
            "optional task_type disambiguates recovered IDs. Types: image, image_layers, sprite, material, skybox (legacy), sprite_sequence, " +
            "auto_sprite_sequence, terrain, terrain_apply, model_import, image_import, audio_import, video_import. " +
            "Returns original status, progress, asset paths and type-specific fields, plus task_type. UI Kit stages live in image/image_layers; " +
            "skybox MCP downloads use image_import. MCP-originated generation IDs must go to MCP check_task instead. " +
            "Wait between checks, keep the same task ID while pending, and verify real assets before reporting completion.")]
        public static object QueryLocalTask(JObject parameters)
        {
#if UNITY_EDITOR
            string id = parameters?["task_id"]?.ToString() ?? parameters?["apply_task_id"]?.ToString();
            if (parameters?["task_id"] != null && parameters?["apply_task_id"] != null &&
                parameters["task_id"].ToString() != parameters["apply_task_id"].ToString())
                return Fail("task_id and apply_task_id must match when both are supplied.");
            if (string.IsNullOrWhiteSpace(id)) return Fail("'task_id' is required.");
            var sources = SelectSources(parameters?["task_type"]?.ToString());
            if (sources.Length == 0) return Fail("Unknown task_type. Use list_local_tasks for supported types.");
            var matches = sources.Where(s => s.Contains(id)).ToArray();
            if (matches.Length == 0) return Fail("Local task not found. Use MCP check_task for MCP generation IDs; use list_local_tasks to locate local tasks.", "TASK_NOT_FOUND");
            if (matches.Length > 1) return new JObject { ["success"] = false, ["error_code"] = "AMBIGUOUS_TASK_ID",
                ["message"] = "Specify task_type to disambiguate this recovered task ID.", ["task_types"] = new JArray(matches.Select(s => s.Type)) };
            return Read(matches[0], id);
#else
            return new { success = false, message = "This tool only works in Unity Editor." };
#endif
        }

        [ExecuteCustomTool.CustomTool("list_local_tasks",
            "List Unity-local tasks across all native generation/import/post-processing stores, without resubmitting or calling MCP. " +
            "Optional filters: task_type (one of query_local_task's types), status (exact value or comma-separated values). " +
            "Pagination: offset (default 0), limit (default 100, max 1000). Returns tasks with full query fields, total, next_offset, task_types. " +
            "No duplicate router entries; each underlying task appears once per store. For assets belonging to a session use list_session_assets.")]
        public static object ListLocalTasks(JObject parameters)
        {
#if UNITY_EDITOR
            var sources = SelectSources(parameters?["task_type"]?.ToString());
            if (sources.Length == 0) return Fail("Unknown task_type.");
            int offset = 0, limit = 100;
            if (parameters?["offset"] != null && (!int.TryParse(parameters["offset"].ToString(), out offset) || offset < 0))
                return Fail("offset must be a non-negative integer.");
            if (parameters?["limit"] != null && (!int.TryParse(parameters["limit"].ToString(), out limit) || limit < 1 || limit > 1000))
                return Fail("limit must be between 1 and 1000.");
            var statuses = new HashSet<string>((parameters?["status"]?.ToString() ?? "").Split(',').Select(s => s.Trim()).Where(s => s.Length > 0), StringComparer.OrdinalIgnoreCase);
            var rows = sources.SelectMany(s => s.Ids().Distinct().Select(id => Read(s, id, readOnly: true)))
                .Where(r => statuses.Count == 0 || statuses.Contains((string)r["status"]))
                .OrderByDescending(r => (string)r["start_time"], StringComparer.Ordinal)
                .ThenBy(r => (string)r["task_type"], StringComparer.Ordinal)
                .ThenBy(r => (string)r["task_id"], StringComparer.Ordinal).ToList();
            var page = rows.Skip(offset).Take(limit).ToList();
            return new JObject { ["success"] = true, ["tasks"] = new JArray(page), ["total"] = rows.Count,
                ["offset"] = offset, ["limit"] = limit,
                ["next_offset"] = (long)offset + page.Count < rows.Count ? (JToken)(offset + page.Count) : JValue.CreateNull(),
                ["task_types"] = new JArray(Sources.Select(s => s.Type)) };
#else
            return new { success = false, message = "This tool only works in Unity Editor." };
#endif
        }
    }
}
