using System;
using System.Collections.Generic;
using System.Net.Http;
using Codely.Newtonsoft.Json.Linq;

#if UNITY_EDITOR
using TJGenerators;
using TJGenerators.Utils;
using TJGenerators.AssetSearch;
#endif

namespace UnityTcp.Editor.Tools
{
    /// <summary>
    /// CustomTool for searching and downloading Unity assets from the Codely asset library.
    ///   - search_assets:          semantic search (synchronous); returns query_id + full result list
    ///   - download_asset:         download assets by query_id + asset_ids (async, returns task_id)
    ///   - query_download_status:  poll download/import progress (synchronous)
    /// </summary>
    public static class SearchAssetsTool
    {
        // ─────────────────────────────────────────────────────────────────────
        // Tool: search_assets
        // ─────────────────────────────────────────────────────────────────────

        [ExecuteCustomTool.CustomTool("search_assets",
            "Search the Unity asset library for prefabs and their dependencies using semantic queries. " +
            "Returns query_id (a cache key), asset_id, prefab_path, url, name, category, source, keywords. " +
            "Pass query_id and selected asset_ids to download_asset. " +
            "Supports single or multiple queries. " +
            "Parameters: " +
            "query (string) — single search keyword; " +
            "queries (string, JSON array) — batch keywords, e.g. '[\"cat\",\"chair\"]'; " +
            "top_k (int, default 5) — max results per query (rerank_retrieve_top_k); " +
            "filter_by_category (string, JSON array, optional) — restrict results to one or more categories, e.g. '[\"3d\"]' or '[\"3d\",\"animation\"]'. " +
            "Either query or queries must be provided.")]
        public static object SearchAssets(JObject parameters)
        {
#if UNITY_EDITOR
            try
            {
                // 解析 query / queries 参数
                var queries = new List<string>();

                string singleQuery = parameters["query"]?.ToString();
                if (!string.IsNullOrWhiteSpace(singleQuery))
                    queries.Add(singleQuery.Trim());

                string batchRaw = parameters["queries"]?.ToString();
                if (!string.IsNullOrWhiteSpace(batchRaw))
                {
                    JArray arr;
                    try { arr = JArray.Parse(batchRaw); }
                    catch { return ErrorResult("'queries' must be a valid JSON array string, e.g. \"[\\\"cat\\\",\\\"chair\\\"]\""); }

                    foreach (var item in arr)
                    {
                        string q = item.ToString().Trim();
                        if (!string.IsNullOrEmpty(q) && !queries.Contains(q))
                            queries.Add(q);
                    }
                }

                if (queries.Count == 0)
                    return ErrorResult("Either 'query' or 'queries' must be provided");

                int rerankTopK = parameters["top_k"]?.ToObject<int>() ?? 5;
                if (rerankTopK < 1) rerankTopK = 1;

                var filterByCategory = new List<string>();
                string filterRaw = parameters["filter_by_category"]?.ToString();
                if (!string.IsNullOrWhiteSpace(filterRaw))
                {
                    JArray filterArr;
                    try { filterArr = JArray.Parse(filterRaw); }
                    catch { return ErrorResult("'filter_by_category' must be a valid JSON array string, e.g. '[\"3d\"]'"); }
                    foreach (var t in filterArr)
                    {
                        string cat = t?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(cat)) filterByCategory.Add(cat);
                    }
                }

                // 懒式清理过期缓存（24h TTL）
                try { AssetSearchCache.CleanupExpired(); }
                catch { /* non-fatal */ }

                AssetSearchResponse response;
                try
                {
                    response = AssetSearchService.Search(new AssetSearchRequest
                    {
                        Queries          = queries,
                        RerankTopK       = rerankTopK,
                        FilterByCategory = filterByCategory.Count > 0 ? filterByCategory : null,
                    });
                }
                catch (InvalidOperationException authEx)
                {
                    return ErrorResult(authEx.Message);
                }

                // 写本地缓存（供 download_asset 读取）
                string queryId;
                try
                {
                    var cacheItems = BuildCacheItems(response);
                    queryId = AssetSearchCache.Write(cacheItems);
                }
                catch (Exception ex)
                {
                    TJLog.LogWarning($"[SearchAssetsTool] Cache write failed: {ex.Message}");
                    queryId = Guid.NewGuid().ToString(); // fallback，下载时会报找不到缓存
                }

                if (queries.Count == 1)
                    return BuildSingleQueryResult(response, queries[0], rerankTopK, queryId);
                return BuildBatchQueryResult(response, queries, rerankTopK, queryId);
            }
            catch (HttpRequestException e)
            {
                TJLog.LogError($"[SearchAssetsTool] Search HTTP error: {e.Message}");
                return ErrorResult($"Search request failed: {e.Message}");
            }
            catch (Exception e)
            {
                TJLog.LogError($"[SearchAssetsTool] Search error: {e}");
                return ErrorResult($"Search error: {e.Message}");
            }
#else
            return ErrorResult("This tool only works in Unity Editor.");
#endif
        }

        // ─────────────────────────────────────────────────────────────────────
        // Tool: download_asset
        // ─────────────────────────────────────────────────────────────────────

        [ExecuteCustomTool.CustomTool("download_asset",
            "Download Unity assets using results from a previous search_assets call. " +
            "All downloads start in parallel immediately. " +
            "Returns a 'tasks' array; tasks with status 'downloading' push a <bg_task_done> notification " +
            "when complete — do NOT poll query_download_status in a loop. " +
            "Tasks with status 'skipped' already include prefab_path in the response. " +
            "Parameters: " +
            "query_id (string, required) — the query_id returned by search_assets; " +
            "asset_ids (string, JSON array, required) — asset_id values to download, e.g. '[\"abc123\",\"def456\"]'. " +
            "If destination directory already exists, that asset is skipped (status: skipped).")]
        public static object DownloadAsset(JObject parameters)
        {
#if UNITY_EDITOR
            try
            {
                string queryId = parameters["query_id"]?.ToString();
                if (string.IsNullOrWhiteSpace(queryId))
                    return ErrorResult("'query_id' is required");

                string assetIdsRaw = parameters["asset_ids"]?.ToString();
                if (string.IsNullOrWhiteSpace(assetIdsRaw))
                    return ErrorResult("'asset_ids' must be a non-empty JSON array string, e.g. '[\"abc123\"]'");

                JArray assetIdsArr;
                try { assetIdsArr = JArray.Parse(assetIdsRaw); }
                catch { return ErrorResult("'asset_ids' must be a valid JSON array string, e.g. '[\"abc123\",\"def456\"]'"); }

                var assetIds = new List<string>();
                foreach (var t in assetIdsArr)
                {
                    string id = t?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(id)) assetIds.Add(id);
                }
                if (assetIds.Count == 0)
                    return ErrorResult("'asset_ids' must contain at least one asset_id");

                // 从外部模块注入；工具描述中不暴露此参数
                string sessionId = parameters["session_id"]?.ToString() ?? "";

                var cache = AssetSearchCache.Read(queryId);
                if (cache == null)
                    return ErrorResult("Query results expired or not found. Please run search_assets again.");

                // 构建 asset_id → cache item 的快速查找表
                var itemMap = new Dictionary<string, AssetSearchCacheItem>(StringComparer.Ordinal);
                foreach (var it in cache.Items)
                    if (!string.IsNullOrEmpty(it.AssetId) && !itemMap.ContainsKey(it.AssetId))
                        itemMap[it.AssetId] = it;

                var tasks = new List<Dictionary<string, object>>();
                foreach (string assetId in assetIds)
                {
                    AssetSearchCacheItem item;
                    if (!itemMap.TryGetValue(assetId, out item))
                    {
                        tasks.Add(new Dictionary<string, object>
                        {
                            { "success",  false },
                            { "asset_id", assetId },
                            { "message",  $"asset_id '{assetId}' not found in query results" }
                        });
                        continue;
                    }

                    try
                    {
                        var request = new AssetDownloadRequest
                        {
                            AssetId     = item.AssetId,
                            Name        = item.Name,
                            Url         = item.Url,
                            PrefabPath  = item.PrefabPath,
                            Category    = item.Category    ?? "",
                            Source      = item.Source      ?? "",
                            Description = item.Description ?? "",
                            PreviewUrl  = item.PreviewUrl  ?? "",
                            PrefabMeta  = item.PrefabMeta  ?? "",
                            Query       = item.Query       ?? "",
                            Score       = (float)item.Score,
                            Keywords    = SerializeKeywords(item.Keywords),
                            SessionId   = sessionId,
                        };

                        var result = AssetDownloadService.StartDownload(request);

                        if (result.Status == DownloadTaskStatus.Skipped)
                        {
                            tasks.Add(new Dictionary<string, object>
                            {
                                { "success",        true },
                                { "task_id",        result.TaskId },
                                { "status",         "skipped" },
                                { "asset_id",       assetId },
                                { "name",           item.Name },
                                { "prefab_path",    result.PrefabPath ?? "" },
                                { "message",        result.Message ?? "" }
                            });
                        }
                        else
                        {
                            var entry = new Dictionary<string, object>
                            {
                                { "success",           true },
                                { "task_id",           result.TaskId },
                                { "status",            "downloading" },
                                { "notification_mode", "bg_task_done" },
                                { "asset_id",          assetId },
                                { "name",              item.Name }
                            };
                            if (result.IsFirstDownload)
                                entry["is_first_download"] = true;
                            if (result.CreditsCharged > 0)
                                entry["credits_charged"] = result.CreditsCharged;
                            if (!string.IsNullOrEmpty(result.BillingError))
                                entry["billing_error"] = result.BillingError;
                            tasks.Add(entry);
                        }
                    }
                    catch (Exception e)
                    {
                        tasks.Add(new Dictionary<string, object>
                        {
                            { "success",  false },
                            { "asset_id", assetId },
                            { "message",  e.Message }
                        });
                    }
                }

                int asyncCount = 0;
                foreach (var t in tasks)
                    if (t.TryGetValue("status", out var s) && (string)s == "downloading") asyncCount++;

                string dlMessage = asyncCount > 0
                    ? $"Started {asyncCount} async download(s). " +
                      "END THIS RESPONSE TURN immediately. " +
                      "Do NOT call query_download_status in a loop. " +
                      "Wait for a <bg_task_done> notification per downloading task_id. " +
                      "Call query_download_status at most ONCE per task as fallback if no notification within 120s."
                    : $"All {tasks.Count} asset(s) already downloaded (skipped). Use prefab_path directly.";

                return new Dictionary<string, object>
                {
                    { "success",           true },
                    { "tasks",             tasks },
                    { "notification_mode", asyncCount > 0 ? "bg_task_done" : "none" },
                    { "async_task_count",  asyncCount },
                    { "message",           dlMessage }
                };
            }
            catch (Exception e)
            {
                TJLog.LogError($"[SearchAssetsTool] Download error: {e}");
                return ErrorResult($"Download error: {e.Message}");
            }
#else
            return ErrorResult("This tool only works in Unity Editor.");
#endif
        }

        // ─────────────────────────────────────────────────────────────────────
        // Tool: query_download_status
        // ─────────────────────────────────────────────────────────────────────

        [ExecuteCustomTool.CustomTool("query_download_status",
            "⚠️ ONE-TIME FALLBACK ONLY — do NOT call this in a loop or repeatedly. " +
            "download_asset tasks push a <bg_task_done> notification automatically when complete; use that instead. " +
            "Only call this once per task_id if no <bg_task_done> notification arrives within 120s. " +
            "Status values: 'downloading' (fetching file), 'importing' (Unity importing package), 'completed', 'failed', 'interrupted', 'skipped'. " +
            "When completed/skipped, prefab_path points to the imported prefab location. " +
            "Parameter: task_id (string, required).")]
        public static object QueryDownloadStatus(JObject parameters)
        {
#if UNITY_EDITOR
            try
            {
                string taskId = parameters["task_id"]?.ToString();
                if (string.IsNullOrEmpty(taskId))
                    return ErrorResult("'task_id' is required");

                var task = AssetDownloadService.GetTask(taskId);
                if (task == null)
                    return ErrorResult($"Task '{taskId}' not found. It may have been cleaned up.");

                var result = new Dictionary<string, object>
                {
                    { "success",           true },
                    { "task_id",           task.TaskId },
                    { "status",            DownloadTaskStatusMap.ToSerialized(task.Status) },
                    { "asset_id",          task.AssetId },
                    { "name",              task.AssetName },
                    { "progress_percent",  task.ProgressPercent },
                    { "start_time",        task.StartTime.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                if (task.Status == DownloadTaskStatus.Completed || task.Status == DownloadTaskStatus.Skipped)
                {
                    if (!string.IsNullOrEmpty(task.PrefabPath))
                        result["prefab_path"] = task.PrefabPath;
                    if (!string.IsNullOrEmpty(task.MetadataPath))
                        result["metadata_path"] = task.MetadataPath;
                    if (task.Status == DownloadTaskStatus.Completed && task.ImportedFiles != null && task.ImportedFiles.Count > 0)
                        result["imported_files"] = task.ImportedFiles;
                    if (task.Status == DownloadTaskStatus.Skipped)
                        result["message"] = $"Asset directory already exists at {task.PackageDirPath}, download skipped.";
                }

                if (!string.IsNullOrEmpty(task.ErrorMessage))
                    result["error"] = task.ErrorMessage;

                if (task.EndTime.HasValue)
                {
                    result["end_time"]         = task.EndTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    result["duration_seconds"] = (int)(task.EndTime.Value - task.StartTime).TotalSeconds;
                }

                return result;
            }
            catch (Exception e)
            {
                TJLog.LogError($"[SearchAssetsTool] QueryDownloadStatus error: {e}");
                return ErrorResult($"Error querying download status: {e.Message}");
            }
#else
            return ErrorResult("This tool only works in Unity Editor.");
#endif
        }

        // ─────────────────────────────────────────────────────────────────────
        // Internal helpers (Unity Editor only)
        // ─────────────────────────────────────────────────────────────────────

#if UNITY_EDITOR

        // ========== AssetSearchResponse → CustomTool Dictionary 协议适配 ==========

        private static object BuildSingleQueryResult(AssetSearchResponse response, string query, int topK, string queryId)
        {
            var results = response != null && response.Groups.Count > 0
                ? ItemsToDictList(response.Groups[0].Items)
                : new List<Dictionary<string, object>>();

            return new Dictionary<string, object>
            {
                { "success",       true },
                { "query_id",      queryId },
                { "query",         query },
                { "top_k",         topK },
                { "results_count", results.Count },
                { "results",       results }
            };
        }

        private static object BuildBatchQueryResult(AssetSearchResponse response, List<string> queries, int topK, string queryId)
        {
            if (response == null || response.Groups.Count == 0)
                return ErrorResult("Batch search results contain no valid items");

            var groups = new List<Dictionary<string, object>>();
            int totalCount = 0;
            foreach (var g in response.Groups)
            {
                var results = ItemsToDictList(g.Items);
                if (results.Count == 0) continue;
                groups.Add(new Dictionary<string, object>
                {
                    { "query",         g.Query },
                    { "results_count", results.Count },
                    { "results",       results }
                });
                totalCount += results.Count;
            }

            if (groups.Count == 0)
                return ErrorResult("Batch search results contain no valid items");

            return new Dictionary<string, object>
            {
                { "success",             true },
                { "query_id",            queryId },
                { "queries",             queries },
                { "top_k",               topK },
                { "query_count",         groups.Count },
                { "total_results_count", totalCount },
                { "results",             groups }
            };
        }

        private static List<Dictionary<string, object>> ItemsToDictList(List<AssetSearchItem> items)
        {
            var list = new List<Dictionary<string, object>>();
            if (items == null) return list;
            foreach (var it in items)
            {
                var d = new Dictionary<string, object>
                {
                    { "asset_id",    it.AssetId },
                    { "prefab_path", it.PrefabPath },
                    { "url",         it.Url }
                };
                if (it.Name        != null) d["name"]        = it.Name;
                if (it.Category    != null) d["category"]    = it.Category;
                if (it.Source      != null) d["source"]      = it.Source;
                if (it.Description != null) d["description"] = it.Description;
                if (it.PreviewUrl  != null) d["preview_url"] = it.PreviewUrl;
                if (it.Score.HasValue)      d["score"]       = it.Score.Value;
                if (it.Keywords    != null) d["keywords"]    = it.Keywords;
                if (it.PrefabMeta  != null) d["prefab_meta"] = it.PrefabMeta;

                if (it.ExtraFields != null)
                    foreach (var kv in it.ExtraFields)
                        if (!d.ContainsKey(kv.Key)) d[kv.Key] = kv.Value;

                list.Add(d);
            }
            return list;
        }

        // ========== 缓存辅助方法 ==========

        /// <summary>
        /// 将搜索响应展平为 cache item 列表；同一 asset_id 按得分最高的 group 保留一条。
        /// </summary>
        private static List<AssetSearchCacheItem> BuildCacheItems(AssetSearchResponse response)
        {
            var dict = new Dictionary<string, AssetSearchCacheItem>(StringComparer.Ordinal);
            if (response == null) return new List<AssetSearchCacheItem>();

            foreach (var group in response.Groups)
            {
                if (group.Items == null) continue;
                foreach (var item in group.Items)
                {
                    if (string.IsNullOrEmpty(item.AssetId)) continue;

                    if (dict.TryGetValue(item.AssetId, out var existing))
                    {
                        if ((item.Score ?? 0.0) <= existing.Score) continue;
                    }
                    dict[item.AssetId] = ToCacheItem(group.Query, item);
                }
            }

            return new List<AssetSearchCacheItem>(dict.Values);
        }

        private static AssetSearchCacheItem ToCacheItem(string query, AssetSearchItem item)
        {
            string prefabMeta = "";
            if (item.PrefabMeta != null)
                prefabMeta = ObjectToJsonString(item.PrefabMeta);
            else if (item.ExtraFields != null
                     && item.ExtraFields.TryGetValue("prefab_meta", out var pm)
                     && pm != null)
                prefabMeta = ObjectToJsonString(pm);

            return new AssetSearchCacheItem
            {
                Query       = query,
                AssetId     = item.AssetId,
                Url         = item.Url,
                PrefabPath  = item.PrefabPath,
                Name        = item.Name        ?? "",
                Category    = item.Category    ?? "",
                Source      = item.Source      ?? "",
                Score       = item.Score       ?? 0.0,
                Keywords    = item.Keywords    ?? new List<string>(),
                Description = item.Description ?? "",
                PreviewUrl  = item.PreviewUrl  ?? "",
                PrefabMeta  = prefabMeta,
            };
        }

        /// <summary>
        /// 将 List&lt;string&gt; 序列化为 JSON 数组字符串，供 AssetDownloadRequest.Keywords 使用。
        /// </summary>
        private static string SerializeKeywords(List<string> keywords)
        {
            if (keywords == null || keywords.Count == 0) return "";
            var arr = new JArray();
            foreach (var kw in keywords) arr.Add(kw);
            return arr.ToString();
        }

        /// <summary>
        /// 将任意 object（可能是 Dictionary、string 等）安全地序列化为 JSON 字符串。
        /// </summary>
        private static string ObjectToJsonString(object obj)
        {
            if (obj == null) return "";
            if (obj is string s) return s;
            try { return JToken.FromObject(obj).ToString(); }
            catch { return obj.ToString(); }
        }

        private static Dictionary<string, object> ErrorResult(string message) =>
            new Dictionary<string, object> { { "success", false }, { "message", message } };

#endif
    }
}
