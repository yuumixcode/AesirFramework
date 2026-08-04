#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor
{
    /// <summary>
    /// 保存并恢复已加载场景中 GameObject 在 Hierarchy 面板里的展开状态。
    /// 本脚本刻意保持独立：不依赖 ES、Odin、asmdef 或任何项目工具类。
    /// </summary>
    [InitializeOnLoad]
    public static class SceneHierarchyExpansionState
    {
        // 最大记录层级。默认 5 层可以覆盖多数编辑需求，同时避免深层大场景产生过多路径。
        const int MaxDepth = 5;

        // 自动恢复失败后的重试上限。Hierarchy 窗口刚创建时，内部 TreeView 可能还没初始化完成。
        const int RetryLimit = 5;

        // 单次最多保存的展开对象数量。用于限制大场景中展开节点过多导致的保存/恢复开销。
        const int MaxStoredExpandedObjects = 250;

        // 自动恢复前额外等待的 Editor Update 次数，用于避开场景刚打开时 Hierarchy 尚未稳定的阶段。
        const int RestoreDelayTicks = 2;

        // 自动保存/加载开关。需要完全手动控制时，可以把这些常量改为 false。
        const bool AutoSaveOnSceneSaving = true;
        const bool AutoSaveBeforeSceneClosing = true;
        const bool AutoLoadOnSceneOpened = true;
        const bool AutoSaveBeforeAssemblyReload = true;
        const bool AutoRestoreAfterPlayMode = true;
        const bool LogTiming = true;

        const string MenuRoot = "Tools/Scene Hierarchy Expansion/";
        const string StoragePrefix = "Standalone.SceneHierarchyExpansionState.";

        static int _restoreRetryCount;
        static int _pendingRestoreDelayTicks;
        static bool _restoreScheduled;

        static SceneHierarchyExpansionState()
        {
            if (AutoSaveOnSceneSaving)
            {
                EditorSceneManager.sceneSaving += OnSceneSaving;
            }

            if (AutoLoadOnSceneOpened)
            {
                EditorSceneManager.sceneOpened += OnSceneOpened;
            }

            if (AutoSaveBeforeSceneClosing)
            {
                EditorSceneManager.sceneClosing += OnSceneClosing;
            }

            if (AutoSaveBeforeAssemblyReload)
            {
                AssemblyReloadEvents.beforeAssemblyReload += SaveLoadedScenesExpansionState;
            }

            if (AutoRestoreAfterPlayMode)
            {
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            }

            EditorApplication.quitting += SaveLoadedScenesExpansionState;
            EditorApplication.delayCall += () => ScheduleRestoreLoadedScenes(RestoreDelayTicks);
        }

        [MenuItem(MenuRoot + "Save Loaded Scenes Expansion")]
        public static void SaveLoadedScenesExpansionState()
        {
            var totalStart = EditorApplication.timeSinceStartup;
            var readExpandedStart = EditorApplication.timeSinceStartup;

            // Unity 没有公开 Hierarchy 展开状态 API，这里通过反射读取当前展开的 InstanceID。
            var expandedIds = SceneHierarchyReflection.GetExpandedInstanceIds();
            var readExpandedMs = ToMilliseconds(EditorApplication.timeSinceStartup - readExpandedStart);
            if (expandedIds.Count == 0)
            {
                LogSaveTiming(0, 0, 0, expandedIds.Count, readExpandedMs,
                    ToMilliseconds(EditorApplication.timeSinceStartup - totalStart));
                return;
            }

            var storedSceneCount = 0;
            var scannedTransformCount = 0;
            var storedExpandedCount = 0;

            // 分场景保存，避免多场景编辑时互相污染。
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!CanStoreScene(scene))
                {
                    continue;
                }

                var data = new SceneExpansionData();
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (data.expandedTransformPaths.Count >= MaxStoredExpandedObjects)
                    {
                        break;
                    }

                    CollectExpandedPaths(root.transform, expandedIds, data.expandedTransformPaths,
                        ref scannedTransformCount);
                }

                data.expandedTransformPaths.Sort(StringComparer.Ordinal);

                // 保存到 EditorPrefs：不创建资产文件，不影响版本库，按项目和场景 GUID 隔离。
                EditorPrefs.SetString(GetStorageKey(scene), JsonUtility.ToJson(data));
                storedSceneCount++;
                storedExpandedCount += data.expandedTransformPaths.Count;
            }

            LogSaveTiming(storedSceneCount, scannedTransformCount, storedExpandedCount, expandedIds.Count,
                readExpandedMs, ToMilliseconds(EditorApplication.timeSinceStartup - totalStart));
        }

        [MenuItem(MenuRoot + "Load Loaded Scenes Expansion")]
        public static void LoadLoadedScenesExpansionState()
        {
            _restoreRetryCount = 0;
            ScheduleRestoreLoadedScenes(0);
        }

        [MenuItem(MenuRoot + "Clear Loaded Scenes Saved State")]
        public static void ClearLoadedScenesSavedState()
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (CanStoreScene(scene))
                {
                    EditorPrefs.DeleteKey(GetStorageKey(scene));
                }
            }

            Debug.Log(
                "[SceneHierarchyExpansionState] Cleared saved hierarchy expansion state for loaded scenes.");
        }

        static void OnSceneSaving(Scene scene, string path)
        {
            SaveLoadedScenesExpansionState();
        }

        static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            _restoreRetryCount = 0;
            ScheduleRestoreLoadedScenes(RestoreDelayTicks);
        }

        static void OnSceneClosing(Scene scene, bool removingScene)
        {
            SaveLoadedScenesExpansionState();
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                SaveLoadedScenesExpansionState();
                return;
            }

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                _restoreRetryCount = 0;
                ScheduleRestoreLoadedScenes(RestoreDelayTicks);
            }
        }

        static void ScheduleRestoreLoadedScenes(int delayTicks)
        {
            // 合并短时间内的重复恢复请求，并从最后一次请求后重新等待。
            _pendingRestoreDelayTicks = Mathf.Max(0, delayTicks);

            if (_restoreScheduled)
            {
                return;
            }

            _restoreScheduled = true;
            EditorApplication.update += RestoreLoadedScenesWhenReady;
        }

        static void RestoreLoadedScenesWhenReady()
        {
            if (_pendingRestoreDelayTicks > 0)
            {
                _pendingRestoreDelayTicks--;
                return;
            }

            // 编译、资源刷新、播放模式切换期间不应用，避免和 Unity 自身重建 Hierarchy 的时机冲突。
            if (!IsEditorReadyForRestore())
            {
                RetryRestore();
                return;
            }

            // 如果 Hierarchy 内部对象还没准备好，延迟到后续 editor tick 再试。
            if (!SceneHierarchyReflection.CanSetExpandedState)
            {
                RetryRestore();
                return;
            }

            EditorApplication.update -= RestoreLoadedScenesWhenReady;
            _restoreScheduled = false;

            var totalStart = EditorApplication.timeSinceStartup;
            var resolveMs = 0d;
            var applyMs = 0d;
            var loadedSceneCount = 0;
            var candidatePathCount = 0;
            var resolvedPathCount = 0;
            var restoredCount = 0;

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!CanStoreScene(scene))
                {
                    continue;
                }

                var json = EditorPrefs.GetString(GetStorageKey(scene), string.Empty);
                if (string.IsNullOrEmpty(json))
                {
                    continue;
                }

                var data = JsonUtility.FromJson<SceneExpansionData>(json);
                if (data == null || data.expandedTransformPaths == null)
                {
                    continue;
                }

                loadedSceneCount++;
                candidatePathCount += data.expandedTransformPaths.Count;

                // 先恢复浅层，再恢复深层，避免父节点未展开时子节点恢复失败或不可见。
                data.expandedTransformPaths.Sort(ComparePathDepthThenName);
                foreach (var transformPath in data.expandedTransformPaths)
                {
                    var resolveStart = EditorApplication.timeSinceStartup;
                    var transform = ResolveTransformPath(scene, transformPath);
                    resolveMs += ToMilliseconds(EditorApplication.timeSinceStartup - resolveStart);
                    if (transform == null)
                    {
                        continue;
                    }

                    resolvedPathCount++;

                    var applyStart = EditorApplication.timeSinceStartup;
                    if (SceneHierarchyReflection.SetExpanded(transform.gameObject.GetInstanceID(), true))
                    {
                        restoredCount++;
                    }

                    applyMs += ToMilliseconds(EditorApplication.timeSinceStartup - applyStart);
                }
            }

            if (restoredCount == 0 && _restoreRetryCount < RetryLimit)
            {
                RetryRestore();
                return;
            }

            EditorApplication.RepaintHierarchyWindow();
            _restoreRetryCount = 0;

            LogRestoreTiming(loadedSceneCount, candidatePathCount, resolvedPathCount, restoredCount, resolveMs,
                applyMs, ToMilliseconds(EditorApplication.timeSinceStartup - totalStart));
        }

        static void RetryRestore()
        {
            _restoreRetryCount++;
            if (_restoreRetryCount <= RetryLimit)
            {
                ScheduleRestoreLoadedScenes(RestoreDelayTicks);
            }
            else
            {
                EditorApplication.update -= RestoreLoadedScenesWhenReady;
                _restoreScheduled = false;
                _pendingRestoreDelayTicks = 0;
            }
        }

        static bool IsEditorReadyForRestore() =>
            !EditorApplication.isCompiling && !EditorApplication.isUpdating &&
            !EditorApplication.isPlayingOrWillChangePlaymode;

        static void CollectExpandedPaths(Transform transform,
            HashSet<int> expandedIds,
            List<string> paths,
            ref int scannedTransformCount)
        {
            if (transform == null)
            {
                return;
            }

            scannedTransformCount++;

            // 超过限制层级就不继续递归，控制保存和恢复成本。
            if (GetDepth(transform) > MaxDepth)
            {
                return;
            }

            if (transform.childCount > 0 && expandedIds.Contains(transform.gameObject.GetInstanceID()))
            {
                if (paths.Count >= MaxStoredExpandedObjects)
                {
                    return;
                }

                paths.Add(BuildTransformPath(transform));
            }

            for (var i = 0; i < transform.childCount; i++)
            {
                if (paths.Count >= MaxStoredExpandedObjects)
                {
                    break;
                }

                CollectExpandedPaths(transform.GetChild(i), expandedIds, paths, ref scannedTransformCount);
            }
        }

        static void LogSaveTiming(int sceneCount,
            int scannedTransformCount,
            int storedExpandedCount,
            int editorExpandedIdCount,
            double readExpandedMs,
            double totalMs)
        {
            Debug.Log(
                $"[SceneHierarchyExpansionState] Save timing: total={totalMs:F2}ms, readExpandedIds={readExpandedMs:F2}ms, " +
                $"scenes={sceneCount}, scannedObjects={scannedTransformCount}, savedExpanded={storedExpandedCount}, " +
                $"editorExpandedIds={editorExpandedIdCount}, maxSaved={MaxStoredExpandedObjects}, maxDepth={MaxDepth}.");
        }

        static void LogRestoreTiming(int sceneCount,
            int candidatePathCount,
            int resolvedPathCount,
            int restoredCount,
            double resolveMs,
            double applyMs,
            double totalMs)
        {
            Debug.Log(
                $"[SceneHierarchyExpansionState] Restore timing: total={totalMs:F2}ms, resolvePaths={resolveMs:F2}ms, applyExpanded={applyMs:F2}ms, " +
                $"scenes={sceneCount}, savedPaths={candidatePathCount}, resolvedPaths={resolvedPathCount}, restored={restoredCount}, " +
                $"retry={_restoreRetryCount}/{RetryLimit}.");
        }

        static double ToMilliseconds(double seconds) => seconds * 1000d;

        static string BuildTransformPath(Transform transform)
        {
            var segments = new List<string>(MaxDepth + 1);
            var current = transform;

            while (current != null)
            {
                segments.Add(BuildPathSegment(current));
                current = current.parent;
            }

            segments.Reverse();
            return string.Join("/", segments);
        }

        static string BuildPathSegment(Transform transform)
        {
            var sameNameIndex = GetSameNameIndex(transform);
            var siblingIndex = transform.GetSiblingIndex();

            // name + 同名序号 + siblingIndex 共同组成完整路径段。恢复时必须全部匹配，不做模糊降级。
            return Uri.EscapeDataString(transform.name) + "#" + sameNameIndex + "@" + siblingIndex;
        }

        static Transform ResolveTransformPath(Scene scene, string transformPath)
        {
            if (string.IsNullOrEmpty(transformPath))
            {
                return null;
            }

            var segments = transformPath.Split('/');
            if (segments.Length == 0 || segments.Length > MaxDepth + 1)
            {
                return null;
            }

            Transform current = null;
            var roots = scene.GetRootGameObjects();

            for (var i = 0; i < segments.Length; i++)
            {
                if (!TryParsePathSegment(segments[i], out var name, out var sameNameIndex, out var siblingIndex))
                {
                    return null;
                }

                // 严格按完整路径段匹配，避免同名对象被误展开。
                current = i == 0
                    ? FindRoot(roots, name, sameNameIndex, siblingIndex)
                    : FindChild(current, name, sameNameIndex, siblingIndex);

                if (current == null)
                {
                    return null;
                }
            }

            return current;
        }

        static Transform FindRoot(GameObject[] roots, string name, int sameNameIndex, int siblingIndex)
        {
            if (siblingIndex < 0 || siblingIndex >= roots.Length)
            {
                return null;
            }

            var rootAtSibling = roots[siblingIndex];
            if (rootAtSibling == null || rootAtSibling.name != name)
            {
                return null;
            }

            var seenSameName = 0;
            for (var i = 0; i < roots.Length; i++)
            {
                var root = roots[i];
                if (root == null || root.name != name)
                {
                    continue;
                }

                if (root == rootAtSibling)
                {
                    return seenSameName == sameNameIndex ? root.transform : null;
                }

                seenSameName++;
            }

            return null;
        }

        static Transform FindChild(Transform parent, string name, int sameNameIndex, int siblingIndex)
        {
            if (parent == null)
            {
                return null;
            }

            if (siblingIndex < 0 || siblingIndex >= parent.childCount)
            {
                return null;
            }

            var childAtSibling = parent.GetChild(siblingIndex);
            if (childAtSibling == null || childAtSibling.name != name)
            {
                return null;
            }

            var seenSameName = 0;
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name != name)
                {
                    continue;
                }

                if (child == childAtSibling)
                {
                    return seenSameName == sameNameIndex ? child : null;
                }

                seenSameName++;
            }

            return null;
        }

        static bool TryParsePathSegment(string segment,
            out string name,
            out int sameNameIndex,
            out int siblingIndex)
        {
            name = string.Empty;
            sameNameIndex = 0;
            siblingIndex = -1;

            var hashIndex = segment.LastIndexOf('#');
            var atIndex = segment.LastIndexOf('@');
            if (hashIndex <= 0 || atIndex <= hashIndex)
            {
                return false;
            }

            name = Uri.UnescapeDataString(segment.Substring(0, hashIndex));
            return int.TryParse(segment.Substring(hashIndex + 1, atIndex - hashIndex - 1), out sameNameIndex) &&
                   int.TryParse(segment.Substring(atIndex + 1), out siblingIndex);
        }

        static int GetSameNameIndex(Transform transform)
        {
            var index = 0;

            if (transform.parent == null)
            {
                var roots = transform.gameObject.scene.GetRootGameObjects();
                for (var i = 0; i < roots.Length; i++)
                {
                    var root = roots[i];
                    if (root == transform.gameObject)
                    {
                        return index;
                    }

                    if (root != null && root.name == transform.name)
                    {
                        index++;
                    }
                }

                return index;
            }

            for (var i = 0; i < transform.parent.childCount; i++)
            {
                var child = transform.parent.GetChild(i);
                if (child == transform)
                {
                    return index;
                }

                if (child.name == transform.name)
                {
                    index++;
                }
            }

            return index;
        }

        static int GetDepth(Transform transform)
        {
            var depth = 0;
            var current = transform;
            while (current.parent != null)
            {
                depth++;
                current = current.parent;
            }

            return depth;
        }

        static bool CanStoreScene(Scene scene) =>
            scene.IsValid() && scene.isLoaded && !string.IsNullOrEmpty(scene.path);

        static string GetStorageKey(Scene scene)
        {
            var sceneId = AssetDatabase.AssetPathToGUID(scene.path);
            if (string.IsNullOrEmpty(sceneId))
            {
                sceneId = scene.path;
            }

            // 同一个工程复制到不同目录时，project hash 可以避免 EditorPrefs 键冲突。
            return StoragePrefix + GetProjectHash() + "." + sceneId;
        }

        static string GetProjectHash()
        {
            using (var md5 = MD5.Create())
            {
                var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(Application.dataPath));
                var builder = new StringBuilder(bytes.Length * 2);
                foreach (var value in bytes)
                {
                    builder.Append(value.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        static int ComparePathDepthThenName(string a, string b)
        {
            var depthCompare = GetPathDepth(a).CompareTo(GetPathDepth(b));
            return depthCompare != 0 ? depthCompare : string.CompareOrdinal(a, b);
        }

        static int GetPathDepth(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return 0;
            }

            var depth = 0;
            for (var i = 0; i < path.Length; i++)
            {
                if (path[i] == '/')
                {
                    depth++;
                }
            }

            return depth;
        }

        [Serializable]
        sealed class SceneExpansionData
        {
            public List<string> expandedTransformPaths = new List<string>();
        }

        static class SceneHierarchyReflection
        {
            const int ReflectionSearchDepth = 6;

            static readonly Type HierarchyWindowType =
                typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");

            static readonly BindingFlags InstanceFlags =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            public static bool CanSetExpandedState
            {
                get
                {
                    var hierarchyObject = GetSceneHierarchyObject();
                    return FindMethodOwner(hierarchyObject, "SetExpanded", ReflectionSearchDepth,
                               new HashSet<object>(ReferenceComparer.Instance), out _, out _) ||
                           TryFindExpandedIds(hierarchyObject, ReflectionSearchDepth,
                               new HashSet<object>(ReferenceComparer.Instance), out _);
                }
            }

            public static HashSet<int> GetExpandedInstanceIds()
            {
                var result = new HashSet<int>();
                var hierarchyObject = GetSceneHierarchyObject();
                if (hierarchyObject == null)
                {
                    return result;
                }

                if (!TryFindExpandedIds(hierarchyObject, ReflectionSearchDepth,
                        new HashSet<object>(ReferenceComparer.Instance), out var expandedIds))
                {
                    return result;
                }

                foreach (var item in expandedIds)
                {
                    if (item is int id)
                    {
                        result.Add(id);
                    }
                }

                return result;
            }

            public static bool SetExpanded(int instanceId, bool expanded)
            {
                var hierarchyObject = GetSceneHierarchyObject();
                if (hierarchyObject == null)
                {
                    return false;
                }

                if (FindMethodOwner(hierarchyObject, "SetExpanded", ReflectionSearchDepth,
                        new HashSet<object>(ReferenceComparer.Instance), out var owner, out var method))
                {
                    method.Invoke(owner, new object[] { instanceId, expanded });
                    return true;
                }

                return TrySetExpandedId(hierarchyObject, instanceId, expanded);
            }

            static object GetSceneHierarchyObject()
            {
                if (HierarchyWindowType == null)
                {
                    return null;
                }

                var windows = Resources.FindObjectsOfTypeAll(HierarchyWindowType);
                if (windows == null || windows.Length == 0)
                {
                    return null;
                }

                // Unity 2022 的 SceneHierarchyWindow 内部通常持有 m_SceneHierarchy。
                // 如果字段名变化，则回退到 window 自身继续搜索，降低版本差异导致的失败概率。
                object window = windows[0];
                var field = HierarchyWindowType.GetField("m_SceneHierarchy", InstanceFlags);
                return field != null ? field.GetValue(window) : window;
            }

            static bool TryFindExpandedIds(object source,
                int depth,
                HashSet<object> visited,
                out IList expandedIds)
            {
                expandedIds = null;
                if (!CanInspect(source, depth, visited))
                {
                    return false;
                }

                var type = source.GetType();
                foreach (var field in type.GetFields(InstanceFlags))
                {
                    var value = SafeGet(() => field.GetValue(source));
                    if (IsExpandedIdsMember(field.Name, value, out expandedIds))
                    {
                        return true;
                    }

                    if (ShouldTraverseMember(field.Name) &&
                        TryFindExpandedIds(value, depth - 1, visited, out expandedIds))
                    {
                        return true;
                    }
                }

                foreach (var property in type.GetProperties(InstanceFlags))
                {
                    if (property.GetIndexParameters().Length > 0)
                    {
                        continue;
                    }

                    var value = SafeGet(() => property.GetValue(source, null));
                    if (IsExpandedIdsMember(property.Name, value, out expandedIds))
                    {
                        return true;
                    }

                    if (ShouldTraverseMember(property.Name) &&
                        TryFindExpandedIds(value, depth - 1, visited, out expandedIds))
                    {
                        return true;
                    }
                }

                return false;
            }

            static bool TrySetExpandedId(object hierarchyObject, int instanceId, bool expanded)
            {
                if (!TryFindExpandedIds(hierarchyObject, ReflectionSearchDepth,
                        new HashSet<object>(ReferenceComparer.Instance), out var expandedIds))
                {
                    return false;
                }

                var contains = false;
                foreach (var item in expandedIds)
                {
                    if (item is int id && id == instanceId)
                    {
                        contains = true;
                        break;
                    }
                }

                if (expanded)
                {
                    if (!contains)
                    {
                        expandedIds.Add(instanceId);
                    }

                    return true;
                }

                if (contains)
                {
                    expandedIds.Remove(instanceId);
                }

                return true;
            }

            static bool FindMethodOwner(object source,
                string methodName,
                int depth,
                HashSet<object> visited,
                out object owner,
                out MethodInfo method)
            {
                owner = null;
                method = null;
                if (!CanInspect(source, depth, visited))
                {
                    return false;
                }

                var type = source.GetType();
                method = type.GetMethod(methodName, InstanceFlags, null, new[] { typeof(int), typeof(bool) },
                    null);
                if (method != null)
                {
                    owner = source;
                    return true;
                }

                foreach (var field in type.GetFields(InstanceFlags))
                {
                    if (!ShouldTraverseMember(field.Name))
                    {
                        continue;
                    }

                    var value = SafeGet(() => field.GetValue(source));
                    if (FindMethodOwner(value, methodName, depth - 1, visited, out owner, out method))
                    {
                        return true;
                    }
                }

                foreach (var property in type.GetProperties(InstanceFlags))
                {
                    if (!ShouldTraverseMember(property.Name) || property.GetIndexParameters().Length > 0)
                    {
                        continue;
                    }

                    var value = SafeGet(() => property.GetValue(source, null));
                    if (FindMethodOwner(value, methodName, depth - 1, visited, out owner, out method))
                    {
                        return true;
                    }
                }

                return false;
            }

            static bool IsExpandedIdsMember(string memberName, object value, out IList expandedIds)
            {
                expandedIds = null;
                if (!string.Equals(memberName, "expandedIDs", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (value is IList list)
                {
                    expandedIds = list;
                    return true;
                }

                return false;
            }

            static bool ShouldTraverseMember(string memberName)
            {
                if (string.IsNullOrEmpty(memberName))
                {
                    return false;
                }

                // 限制反射搜索范围，只进入可能承载 TreeView 状态的成员，避免扫描整个编辑器对象图。
                var lower = memberName.ToLowerInvariant();
                return lower.Contains("scenehierarchy") || lower.Contains("treeview") ||
                       lower.Contains("state") || lower.Contains("data") || lower == "m_rootitem";
            }

            static bool CanInspect(object source, int depth, HashSet<object> visited)
            {
                if (source == null || depth < 0)
                {
                    return false;
                }

                var type = source.GetType();
                if (type.IsPrimitive || type.IsEnum || type == typeof(string))
                {
                    return false;
                }

                return visited.Add(source);
            }

            static object SafeGet(Func<object> getter)
            {
                try
                {
                    return getter();
                }
                catch
                {
                    return null;
                }
            }

            sealed class ReferenceComparer : IEqualityComparer<object>
            {
                public static readonly ReferenceComparer Instance = new ReferenceComparer();

                public new bool Equals(object x, object y) => ReferenceEquals(x, y);

                public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}
#endif
