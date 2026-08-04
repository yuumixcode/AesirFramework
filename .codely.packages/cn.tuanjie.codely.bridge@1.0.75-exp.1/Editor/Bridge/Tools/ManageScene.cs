using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codely.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityTcp.Editor.Helpers; // For Response class

namespace UnityTcp.Editor.Tools
{
    /// <summary>
    /// Handles scene management operations like loading, saving, creating, and querying hierarchy.
    /// </summary>
    public static class ManageScene
    {
        private static readonly string[] AllowedSceneExtensions = new[] { ".unity", ".scene" };

#if TUANJIE_1_OR_NEWER || TUANJIE_1
        // Tuanjie Editor defines TUANJIE_* version macros (e.g. TUANJIE_1, TUANJIE_1_1, TUANJIE_1_1_2, TUANJIE_X_Y_OR_NEWER).
        private const bool IsTuanjieEditor = true;
#else
        // Unity Editor builds do NOT define TUANJIE_* macros.
        private const bool IsTuanjieEditor = false;
#endif

        private static bool HasAllowedSceneExtension(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return AllowedSceneExtensions.Any(ext =>
                path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)
            );
        }

        private static string GetSceneExtensionFromPathOrName(string path, string name)
        {
            // If caller provided an explicit scene file path, respect its extension.
            if (!string.IsNullOrEmpty(path) && HasAllowedSceneExtension(path))
            {
                return Path.GetExtension(path).ToLowerInvariant();
            }

            // If caller (incorrectly) included an extension in name, respect it as a hint.
            if (!string.IsNullOrEmpty(name) && HasAllowedSceneExtension(name))
            {
                return Path.GetExtension(name).ToLowerInvariant();
            }

            // Default depends on editor:
            // - Unity:   .unity
            // - Tuanjie: .scene
            return IsTuanjieEditor ? ".scene" : ".unity";
        }

        private static string StripSceneExtension(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            foreach (var ext in AllowedSceneExtensions)
            {
                if (name.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                {
                    return name.Substring(0, name.Length - ext.Length);
                }
            }
            return name;
        }

        private sealed class SceneCommand
        {
            public string action { get; set; } = string.Empty;
            public string name { get; set; } = string.Empty;
            public string path { get; set; } = string.Empty;
            public int? buildIndex { get; set; }
        }

        private static SceneCommand ToSceneCommand(JObject p)
        {
            if (p == null) return new SceneCommand();
            int? BI(JToken t)
            {
                if (t == null || t.Type == JTokenType.Null) return null;
                var s = t.ToString().Trim();
                if (s.Length == 0) return null;
                if (int.TryParse(s, out var i)) return i;
                if (double.TryParse(s, out var d)) return (int)d;
                return t.Type == JTokenType.Integer ? t.Value<int>() : (int?)null;
            }
            return new SceneCommand
            {
                action = (p["action"]?.ToString() ?? string.Empty).Trim().ToLowerInvariant(),
                name = p["name"]?.ToString() ?? string.Empty,
                path = p["path"]?.ToString() ?? string.Empty,
                buildIndex = BI(p["buildIndex"] ?? p["build_index"])
            };
        }

        /// <summary>
        /// Main handler for scene management actions.
        /// </summary>
        public static object HandleCommand(JObject @params)
        {
            try { CodelyLogger.Verbose("[ManageScene] HandleCommand: start"); } catch { }
            var cmd = ToSceneCommand(@params);
            string action = cmd.action;
            string name = string.IsNullOrEmpty(cmd.name) ? null : cmd.name;
            string path = string.IsNullOrEmpty(cmd.path) ? null : cmd.path; // Relative to Assets/
            int? buildIndex = cmd.buildIndex;
            // bool loadAdditive = @params["loadAdditive"]?.ToObject<bool>() ?? false; // Example for future extension
            // Ensure path is relative to Assets/, removing any leading "Assets/"
            string relativeDir = path ?? string.Empty;
            if (!string.IsNullOrEmpty(relativeDir))
            {
                relativeDir = relativeDir.Replace('\\', '/').Trim('/');
                if (relativeDir.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                {
                    relativeDir = relativeDir.Substring("Assets/".Length).TrimStart('/');
                }
            }

            // Apply default *after* sanitizing, using the original path variable for the check
            if (string.IsNullOrEmpty(path) && action == "create") // Check original path for emptiness
            {
                relativeDir = "Scenes"; // Default relative directory
            }

            if (string.IsNullOrEmpty(action))
            {
                return Response.Error("Action parameter is required.");
            }

            // Normalize name (strip extension if provided)
            name = StripSceneExtension(name);

            // Determine extension for scene file operations (Unity: .unity, Tuanjie: .scene)
            string sceneExt = GetSceneExtensionFromPathOrName(path, cmd.name);

            string sceneFileName = string.IsNullOrEmpty(name) ? null : $"{name}{sceneExt}";
            // Construct full system path correctly: ProjectRoot/Assets/relativeDir/sceneFileName
            string fullPathDir = Path.Combine(Application.dataPath, relativeDir); // Combine with Assets path (Application.dataPath ends in Assets)
            string fullPath = string.IsNullOrEmpty(sceneFileName)
                ? null
                : Path.Combine(fullPathDir, sceneFileName);
            // Ensure relativePath always starts with "Assets/" and uses forward slashes
            string relativePath = string.IsNullOrEmpty(sceneFileName)
                ? null
                : Path.Combine("Assets", relativeDir, sceneFileName).Replace('\\', '/');

            // Ensure directory exists for 'create'
            if (action == "create" && !string.IsNullOrEmpty(fullPathDir))
            {
                try
                {
                    Directory.CreateDirectory(fullPathDir);
                }
                catch (Exception e)
                {
                    return Response.Error(
                        $"Could not create directory '{fullPathDir}': {e.Message}"
                    );
                }
            }

            // Route action
            try { CodelyLogger.Verbose($"[ManageScene] Route action='{action}' name='{name}' path='{path}' buildIndex={(buildIndex.HasValue ? buildIndex.Value.ToString() : "null")}"); } catch { }
            switch (action)
            {
                // Ensure operations (idempotent)
                case "ensure_scene_open":
                    // For ensure_scene_open, the path parameter is the full scene path (e.g., "Assets/Scenes/MyScene.unity")
                    // NOT the directory path like other actions
                    if (string.IsNullOrEmpty(path))
                        return Response.Error("'path' parameter is required for 'ensure_scene_open' action.");
                    // Normalize the path - ensure it starts with Assets/ and uses forward slashes
                    string ensureScenePath = path.Replace('\\', '/');
                    if (!ensureScenePath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                        ensureScenePath = "Assets/" + ensureScenePath.TrimStart('/');
                    if (!HasAllowedSceneExtension(ensureScenePath))
                        return Response.Error("'path' must end with '.unity' or '.scene' for scene files.");
                    return EnsureSceneOpen(ensureScenePath);
                case "ensure_scene_saved":
                    return EnsureSceneSaved();

                // Regular operations
                case "create":
                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(relativePath))
                        return Response.Error(
                            "'name' and 'path' parameters are required for 'create' action."
                        );
                    return CreateScene(fullPath, relativePath);
                case "load":
                    // Loading can be done by path/name or build index
                    if (!string.IsNullOrEmpty(relativePath))
                        return LoadScene(relativePath);
                    else if (buildIndex.HasValue)
                        return LoadScene(buildIndex.Value);
                    else
                        return Response.Error(
                            "Either 'name'/'path' or 'buildIndex' must be provided for 'load' action."
                        );
                case "save":
                    // Save current scene, optionally to a new path
                    return SaveScene(fullPath, relativePath);
                case "get_hierarchy":
                    try { CodelyLogger.Verbose("[ManageScene] get_hierarchy: entering"); } catch { }
                    var gh = GetSceneHierarchy();
                    try { CodelyLogger.Verbose("[ManageScene] get_hierarchy: exiting"); } catch { }
                    return gh;
                case "get_active":
                    try { CodelyLogger.Verbose("[ManageScene] get_active: entering"); } catch { }
                    var ga = GetActiveSceneInfo();
                    try { CodelyLogger.Verbose("[ManageScene] get_active: exiting"); } catch { }
                    return ga;
                case "get_build_settings":
                    return GetBuildSettingsScenes();
                // Add cases for modifying build settings, additive loading, unloading etc.
                default:
                    return Response.Error(
                        $"Unknown action: '{action}'. Valid actions: ensure_scene_open, ensure_scene_saved, create, load, save, get_hierarchy, get_active, get_build_settings."
                    );
            }
        }

        private static object CreateScene(string fullPath, string relativePath)
        {
            if (File.Exists(fullPath))
            {
                return Response.Error($"Scene already exists at '{relativePath}'.");
            }

            try
            {
                // Create a new empty scene
                Scene newScene = EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Single
                );
                // Save it to the specified path (creation is an authoring operation)
                bool saved = EditorSceneManager.SaveScene(newScene, relativePath);

                if (saved)
                {
                    AssetDatabase.Refresh(); // Ensure Unity sees the new scene file
                    return Response.Success(
                        $"Scene '{Path.GetFileName(relativePath)}' created successfully at '{relativePath}'.",
                        new { path = relativePath }
                    );
                }
                else
                {
                    // If SaveScene fails, it might leave an untitled scene open.
                    // Optionally try to close it, but be cautious.
                    return Response.Error($"Failed to save new scene to '{relativePath}'.");
                }
            }
            catch (Exception e)
            {
                return Response.Error($"Error creating scene '{relativePath}': {e.Message}");
            }
        }

        private static object LoadScene(string relativePath)
        {
            if (
                !File.Exists(
                    Path.Combine(
                        Application.dataPath.Substring(
                            0,
                            Application.dataPath.Length - "Assets".Length
                        ),
                        relativePath
                    )
                )
            )
            {
                return Response.Error($"Scene file not found at '{relativePath}'.");
            }

            // Check for unsaved changes in the current scene
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                // Optionally prompt the user or save automatically before loading
                return Response.Error(
                    "Current scene has unsaved changes. Please save or discard changes before loading a new scene."
                );
                // Example: bool saveOK = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                // if (!saveOK) return Response.Error("Load cancelled by user.");
            }

            try
            {
                EditorSceneManager.OpenScene(relativePath, OpenSceneMode.Single);
                return Response.Success(
                    $"Scene '{relativePath}' loaded successfully.",
                    new
                    {
                        path = relativePath,
                        name = Path.GetFileNameWithoutExtension(relativePath),
                    }
                );
            }
            catch (Exception e)
            {
                return Response.Error($"Error loading scene '{relativePath}': {e.Message}");
            }
        }

        private static object LoadScene(int buildIndex)
        {
            if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
            {
                return Response.Error(
                    $"Invalid build index: {buildIndex}. Must be between 0 and {SceneManager.sceneCountInBuildSettings - 1}."
                );
            }

            // Check for unsaved changes
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                return Response.Error(
                    "Current scene has unsaved changes. Please save or discard changes before loading a new scene."
                );
            }

            try
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(buildIndex);
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                return Response.Success(
                    $"Scene at build index {buildIndex} ('{scenePath}') loaded successfully.",
                    new
                    {
                        path = scenePath,
                        name = Path.GetFileNameWithoutExtension(scenePath),
                        buildIndex = buildIndex,
                    }
                );
            }
            catch (Exception e)
            {
                return Response.Error(
                    $"Error loading scene with build index {buildIndex}: {e.Message}"
                );
            }
        }

        private static object SaveScene(string fullPath, string relativePath)
        {
            try
            {
                Scene currentScene = EditorSceneManager.GetActiveScene();
                if (!currentScene.IsValid())
                {
                    return Response.Error("No valid scene is currently active to save.");
                }

                bool saved;
                string finalPath = currentScene.path; // Path where it was last saved or will be saved

                if (!string.IsNullOrEmpty(relativePath) && currentScene.path != relativePath)
                {
                    // Save As...
                    // Ensure directory exists
                    string dir = Path.GetDirectoryName(fullPath);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    saved = EditorSceneManager.SaveScene(currentScene, relativePath);
                    finalPath = relativePath;
                }
                else
                {
                    // Save (overwrite existing or save untitled)
                    if (string.IsNullOrEmpty(currentScene.path))
                    {
                        // Scene is untitled, needs a path
                        return Response.Error(
                            "Cannot save an untitled scene without providing a 'name' and 'path'. Use Save As functionality."
                        );
                    }
                    saved = EditorSceneManager.SaveScene(currentScene);
                }

                if (saved)
                {
                    AssetDatabase.Refresh();
                    return Response.Success(
                        $"Scene '{currentScene.name}' saved successfully to '{finalPath}'.",
                        new { path = finalPath, name = currentScene.name }
                    );
                }
                else
                {
                    return Response.Error($"Failed to save scene '{currentScene.name}'.");
                }
            }
            catch (Exception e)
            {
                return Response.Error($"Error saving scene: {e.Message}");
            }
        }

        private static object GetActiveSceneInfo()
        {
            try
            {
                try { CodelyLogger.Verbose("[ManageScene] get_active: querying EditorSceneManager.GetActiveScene"); } catch { }
                Scene activeScene = EditorSceneManager.GetActiveScene();
                try { CodelyLogger.Verbose($"[ManageScene] get_active: got scene valid={activeScene.IsValid()} loaded={activeScene.isLoaded} name='{activeScene.name}'"); } catch { }
                if (!activeScene.IsValid())
                {
                    return Response.Error("No active scene found.");
                }

                var sceneInfo = new
                {
                    name = activeScene.name,
                    path = activeScene.path,
                    buildIndex = activeScene.buildIndex, // -1 if not in build settings
                    isDirty = activeScene.isDirty,
                    isLoaded = activeScene.isLoaded,
                    rootCount = activeScene.rootCount,
                };

                return Response.Success("Retrieved active scene information.", sceneInfo);
            }
            catch (Exception e)
            {
                try { CodelyLogger.LogError($"[ManageScene] get_active: exception {e.Message}"); } catch { }
                return Response.Error($"Error getting active scene info: {e.Message}");
            }
        }

        private static object GetBuildSettingsScenes()
        {
            try
            {
                var scenes = new List<object>();
                for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                {
                    var scene = EditorBuildSettings.scenes[i];
                    scenes.Add(
                        new
                        {
                            path = scene.path,
                            guid = scene.guid.ToString(),
                            enabled = scene.enabled,
                            buildIndex = i, // Actual build index considering only enabled scenes might differ
                        }
                    );
                }
                return Response.Success("Retrieved scenes from Build Settings.", scenes);
            }
            catch (Exception e)
            {
                return Response.Error($"Error getting scenes from Build Settings: {e.Message}");
            }
        }

        private static object GetSceneHierarchy()
        {
            try
            {
                try { CodelyLogger.Verbose("[ManageScene] get_hierarchy: querying EditorSceneManager.GetActiveScene"); } catch { }
                Scene activeScene = EditorSceneManager.GetActiveScene();
                try { CodelyLogger.Verbose($"[ManageScene] get_hierarchy: got scene valid={activeScene.IsValid()} loaded={activeScene.isLoaded} name='{activeScene.name}'"); } catch { }
                if (!activeScene.IsValid() || !activeScene.isLoaded)
                {
                    return Response.Error(
                        "No valid and loaded scene is active to get hierarchy from."
                    );
                }

                try { CodelyLogger.Verbose("[ManageScene] get_hierarchy: fetching root objects"); } catch { }
                GameObject[] rootObjects = activeScene.GetRootGameObjects();
                try { CodelyLogger.Verbose($"[ManageScene] get_hierarchy: rootCount={rootObjects?.Length ?? 0}"); } catch { }
                
                // Count total GameObjects to avoid massive responses
                int totalObjectCount = 0;
                foreach (var rootObj in rootObjects)
                {
                    totalObjectCount += CountGameObjectsRecursive(rootObj);
                }
                
                // If too many objects would be serialized, return a shallow root-only tree with hints.
                if (totalObjectCount > 500)
                {
                    var roots = rootObjects
                        .Where(go => go != null)
                        .Select(go => new Dictionary<string, object>
                        {
                            { "name", go.name },
                            { "instanceID", go.GetStableInstanceId() },
                            { "activeSelf", go.activeSelf },
                            { "activeInHierarchy", go.activeInHierarchy },
                            { "tag", go.tag },
                            { "layer", go.layer },
                            { "isStatic", go.isStatic },
                            { "childCount", go.transform != null ? go.transform.childCount : 0 },
                            { "children", new List<object>() }, // Keep tree shape (shallow)
                        })
                        .ToList();

                    return Response.Success(
                        $"Scene hierarchy is large ({totalObjectCount} GameObjects). Returned root objects only (children omitted).",
                        new
                        {
                            partial = true,
                            mode = "roots_only",
                            totalObjectCount = totalObjectCount,
                            rootCount = roots.Count,
                            roots = roots,
                            hints = new[]
                            {
                                "Use unity_gameobject action='list_children' on a specific root GameObject to expand its subtree with a depth limit.",
                                "Use unity_gameobject action='find' with searchMethod='by_path' to locate a specific object (e.g. 'Root/Child') before listing children.",
                                "If you only need a specific area, pick a root from this list and then drill down incrementally (depth=1..N)."
                            }
                        }
                    );
                }
                
                var hierarchy = rootObjects.Select(go => BuildGameObjectTree(go)).ToList();

                var resp = Response.Success(
                    $"Retrieved hierarchy for scene '{activeScene.name}'.",
                    hierarchy
                );
                try { CodelyLogger.Verbose("[ManageScene] get_hierarchy: success"); } catch { }
                return resp;
            }
            catch (Exception e)
            {
                try { CodelyLogger.LogError($"[ManageScene] get_hierarchy: exception {e.Message}"); } catch { }
                return Response.Error($"Error getting scene hierarchy: {e.Message}");
            }
        }

        // --- Ensure Methods (Idempotent Operations) ---

        /// <summary>
        /// Ensures a scene is open. Idempotent.
        /// </summary>
        private static object EnsureSceneOpen(string scenePath)
        {
            try
            {
                var writeCheck = WriteGuard.CheckWriteAllowed($"ensure_scene_open({scenePath})");
                if (writeCheck != null) return writeCheck;

                if (!File.Exists(scenePath))
                    return Response.Error($"Scene file not found: {scenePath}");

                Scene activeScene = SceneManager.GetActiveScene();
                if (activeScene.path.Equals(scenePath, StringComparison.OrdinalIgnoreCase))
                {
                    return new
                    {
                        success = true,
                        message = $"Scene is already active.",
                        data = new { scenePath = activeScene.path, dirty = activeScene.isDirty }
                    };
                }

                Scene loadedScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                if (!loadedScene.IsValid())
                    return Response.Error($"Failed to open scene: {scenePath}");

                return new
                {
                    success = true,
                    message = $"Scene opened.",
                    data = new { scenePath = loadedScene.path, dirty = loadedScene.isDirty }
                };
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to ensure scene open: {e.Message}");
            }
        }

        /// <summary>
        /// Ensures the active scene is saved. Idempotent.
        /// </summary>
        private static object EnsureSceneSaved()
        {
            try
            {
                Scene activeScene = SceneManager.GetActiveScene();
                if (!activeScene.IsValid())
                    return Response.Error("No active scene.");

                if (!activeScene.isDirty)
                {
                    return new
                    {
                        success = true,
                        message = "Scene already saved.",
                        data = new { scenePath = activeScene.path, dirty = false }
                    };
                }

                // An untitled scene has no path; SaveScene would open a native
                // "Save Scene" file dialog that blocks the editor thread and hangs
                // automated/yolo mode. Fail fast instead of triggering the dialog.
                if (string.IsNullOrEmpty(activeScene.path))
                    return Response.Error(
                        "Cannot ensure-save an untitled scene (no path). Use the 'save' action with a 'name' and 'path' first."
                    );

                var writeCheck = WriteGuard.CheckWriteAllowed($"ensure_scene_saved");
                if (writeCheck != null) return writeCheck;

                bool saved = EditorSceneManager.SaveScene(activeScene);
                if (!saved)
                    return Response.Error($"Failed to save scene.");

                return new
                {
                    success = true,
                    message = "Scene saved.",
                    data = new { scenePath = activeScene.path, dirty = false }
                };
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to ensure scene saved: {e.Message}");
            }
        }

        /// <summary>
        /// Counts total GameObjects in a hierarchy. Uses an iterative traversal to avoid stack overflows on deep hierarchies.
        /// </summary>
        private static int CountGameObjectsRecursive(GameObject go)
        {
            if (go == null) return 0;
            int count = 0;
            var stack = new Stack<Transform>();
            if (go.transform != null)
            {
                stack.Push(go.transform);
            }

            while (stack.Count > 0)
            {
                var tr = stack.Pop();
                if (tr == null) continue;
                count++;
                int cc = tr.childCount;
                for (int i = 0; i < cc; i++)
                {
                    var child = tr.GetChild(i);
                    if (child != null) stack.Push(child);
                }
            }

            return count;
        }

        /// <summary>
        /// Builds a tree representation of a GameObject and its descendants iteratively,
        /// so deeply nested hierarchies (e.g. UI layouts hundreds of levels deep) cannot
        /// blow the managed stack regardless of total object count.
        /// </summary>
        private static object BuildGameObjectTree(GameObject root)
        {
            if (root == null) return null;
            var rootTransform = root.transform;
            if (rootTransform == null) return null;

            var nodes = new Dictionary<Transform, Dictionary<string, object>>();
            var stack = new Stack<Transform>();
            stack.Push(rootTransform);

            while (stack.Count > 0)
            {
                var tr = stack.Pop();
                if (tr == null || nodes.ContainsKey(tr)) continue;
                nodes[tr] = BuildGameObjectNode(tr.gameObject);
                int cc = tr.childCount;
                for (int i = 0; i < cc; i++)
                {
                    var child = tr.GetChild(i);
                    if (child != null) stack.Push(child);
                }
            }

            // Second pass: link children in original sibling order.
            foreach (var kv in nodes)
            {
                var tr = kv.Key;
                var children = (List<object>)kv.Value["children"];
                int cc = tr.childCount;
                for (int i = 0; i < cc; i++)
                {
                    var child = tr.GetChild(i);
                    if (child != null && nodes.TryGetValue(child, out var childNode))
                        children.Add(childNode);
                }
            }

            return nodes[rootTransform];
        }

        private static Dictionary<string, object> BuildGameObjectNode(GameObject go)
        {
            var tr = go.transform;
            return new Dictionary<string, object>
            {
                { "name", go.name },
                { "activeSelf", go.activeSelf },
                { "activeInHierarchy", go.activeInHierarchy },
                { "tag", go.tag },
                { "layer", go.layer },
                { "isStatic", go.isStatic },
                { "instanceID", go.GetStableInstanceId() },
                {
                    "transform",
                    new
                    {
                        position = new
                        {
                            x = tr.localPosition.x,
                            y = tr.localPosition.y,
                            z = tr.localPosition.z,
                        },
                        rotation = new
                        {
                            x = tr.localRotation.eulerAngles.x,
                            y = tr.localRotation.eulerAngles.y,
                            z = tr.localRotation.eulerAngles.z,
                        },
                        scale = new
                        {
                            x = tr.localScale.x,
                            y = tr.localScale.y,
                            z = tr.localScale.z,
                        },
                    }
                },
                { "children", new List<object>() },
            };
        }
    }
}

