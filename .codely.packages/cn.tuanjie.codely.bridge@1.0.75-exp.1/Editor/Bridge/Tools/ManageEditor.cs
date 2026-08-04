using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Codely.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditorInternal; // Required for tag management
using UnityEngine;
using UnityTcp.Editor.Helpers; // For Response class
using UnityTcp.Editor.Tools.Jobs;

namespace UnityTcp.Editor.Tools
{
    /// <summary>
    /// Handles operations related to controlling and querying the Unity Editor state,
    /// including managing Tags and Layers, and compilation workflow.
    /// Compatible with Unity 2022.3 LTS.
    /// </summary>
    public static class ManageEditor
    {
        // Constant for starting user layer index
        private const int FirstUserLayerIndex = 8;

        // Constant for total layer count
        private const int TotalLayerCount = 32;

        // Compilation event tracking
        private static bool _compilationCallbackRegistered = false;
        private static readonly object _compilationLock = new object();

        // Compiling and play-mode transitions both reload the domain, so these budgets have to
        // cover the reload itself — the step job's clock keeps running through it.
        private const int DefaultCompileTimeoutSeconds = 300;
        private const int DefaultPlayModeTimeoutSeconds = 120;

        /// <summary>
        /// Starts <paramref name="job"/> on the current request and returns its context, which
        /// tells the command loop to hold the response open. The job answers on this same request
        /// once it reaches its end state — even if a domain reload happens in between, which is why
        /// these are <see cref="StepJob"/>s and not coroutines.
        /// </summary>
        private static object RunToCompletion(StepJob job, int timeoutSeconds)
            => StepJobRunner.Start(
                CommandContext.RequestId, CommandContext.CommandType, job, timeoutSeconds);

        /// <summary>
        /// Main handler for editor management actions.
        /// </summary>
        public static object HandleCommand(JObject @params)
        {
            string action = @params["action"]?.ToString().ToLower();
            // Parameters for specific actions
            string tagName = @params["tagName"]?.ToString();
            string layerName = @params["layerName"]?.ToString();
            bool waitForCompletion = @params["waitForCompletion"]?.ToObject<bool>() ?? false; // Example - not used everywhere

            if (string.IsNullOrEmpty(action))
            {
                return Response.Error("Action parameter is required.");
            }

            EnsureCompilationCallbacksRegistered();

            // Route action
            switch (action)
            {
                // Deprecated actions — answer with the replacement instead of the generic
                // "unknown action" error so an AI client knows what to call next.
                case "publish_dirty_state_if_needed":
                    return Deprecated(action, "get_state");

                // Every command runs to completion on its own request — compilation and
                // play-mode transitions are already done when their call returns — so these
                // wait actions have nothing left to wait for.
                case "wait_for_compile":
                case "wait_for_stop":
                    return Response.ErrorWithCode(
                        "action_deprecated",
                        $"Action '{action}' is deprecated. There is no need to wait: every " +
                        "command returns only after its work is done (compilation, play-mode " +
                        "transitions, imports included).");

                // Compilation Management — runs to the real end state rather than answering
                // "pending" and making the client poll; see RunToCompletion.
                // request_compile is an alias kept for older clients.
                case "request_compile":
                case "start_compilation_pipeline":
                    return RunToCompletion(
                        new CompilePipelineJob
                        {
                            ClearCache = @params["clearCache"]?.ToObject<bool?>() ?? false,
                        },
                        @params["timeoutSeconds"]?.ToObject<int?>() ?? DefaultCompileTimeoutSeconds);
                case "get_compilation_summary":
                    return CompilationHelper.GetCompilationSummary();

                // Waits until every other in-flight job (async task, step, coroutine) has
                // finished and the editor is neither compiling nor importing. Enrolled under
                // its own name — not the command type — so the job can exclude itself (and
                // concurrent waiters) from the registry it watches.
                case "wait_for_idle":
                    return StepJobRunner.Start(
                        CommandContext.RequestId, WaitForIdleJob.CommandName,
                        new WaitForIdleJob(),
                        @params["timeoutSeconds"]?.ToObject<int?>() ?? 600);

                // Play Mode Control — each answers once the editor has actually reached the
                // requested state, which is generally a domain reload away.
                case "play":
                    return RunToCompletion(
                        new PlayJob(),
                        @params["timeoutSeconds"]?.ToObject<int?>() ?? DefaultPlayModeTimeoutSeconds);
                case "pause":
                    return RunToCompletion(
                        new PauseJob(),
                        @params["timeoutSeconds"]?.ToObject<int?>() ?? DefaultPlayModeTimeoutSeconds);
                case "stop":
                    return RunToCompletion(
                        new StopPlayModeJob(),
                        @params["timeoutSeconds"]?.ToObject<int?>() ?? DefaultPlayModeTimeoutSeconds);
                case "step":
                    try
                    {
                        if (!EditorApplication.isPlaying)
                        {
                            // Auto-enter play mode and pause before stepping. Keep ticking when
                            // the editor is unfocused so stepping proceeds without a window focus.
                            Application.runInBackground = true;
                            EditorApplication.isPlaying = true;
                            EditorApplication.isPaused = true;
                        }
                        else if (!EditorApplication.isPaused)
                        {
                            EditorApplication.isPaused = true;
                        }
                        EditorApplication.Step();
                        return Response.Success("Stepped one frame.", new { playMode = "paused" });
                    }
                    catch (Exception e)
                    {
                        return Response.Error($"Error stepping frame: {e.Message}");
                    }

                // Editor State/Info — get_current_state is an alias kept for older clients.
                case "get_state":
                case "get_current_state":
                    return GetEditorState();
                case "get_project_root":
                    return GetProjectRoot();
                case "get_windows":
                    return GetEditorWindows();
                case "get_active_tool":
                    return GetActiveTool();
                case "get_selection":
                    return GetSelection();
                // Called by the embedded frontend once its page is fully loaded, to
                // collect agent-input messages sent while it was closed or loading.
                case "drain_agent_input":
                    var pendingAgentInputs = Notifier.AgentInputNotifier.DrainPending();
                    return Response.Success(
                        $"Drained {pendingAgentInputs.Count} pending agent input message(s).",
                        pendingAgentInputs);
                case "set_active_tool":
                    string toolName = @params["toolName"]?.ToString();
                    if (string.IsNullOrEmpty(toolName))
                        return Response.Error("'toolName' parameter required for set_active_tool.");
                    return SetActiveTool(toolName);

                // Tag Management
                case "ensure_tag":
                    if (string.IsNullOrEmpty(tagName))
                        return Response.Error("'tagName' parameter required for ensure_tag.");
                    return EnsureTag(tagName);
                case "add_tag":
                    if (string.IsNullOrEmpty(tagName))
                        return Response.Error("'tagName' parameter required for add_tag.");
                    return AddTag(tagName);
                case "remove_tag":
                    if (string.IsNullOrEmpty(tagName))
                        return Response.Error("'tagName' parameter required for remove_tag.");
                    return RemoveTag(tagName);
                case "get_tags":
                    return GetTags(); // Helper to list current tags

                // Layer Management
                case "ensure_layer":
                    if (string.IsNullOrEmpty(layerName))
                        return Response.Error("'layerName' parameter required for ensure_layer.");
                    return EnsureLayer(layerName);
                case "add_layer":
                    if (string.IsNullOrEmpty(layerName))
                        return Response.Error("'layerName' parameter required for add_layer.");
                    return AddLayer(layerName);
                case "remove_layer":
                    if (string.IsNullOrEmpty(layerName))
                        return Response.Error("'layerName' parameter required for remove_layer.");
                    return RemoveLayer(layerName);
                case "get_layers":
                    return GetLayers(); // Helper to list current layers

                // Window Focus
                case "focus_window":
                    string windowType = @params["windowType"]?.ToString();
                    if (string.IsNullOrEmpty(windowType))
                        return Response.Error("'windowType' parameter required for focus_window.");
                    return FocusWindow(windowType);

                default:
                    return Response.Error(
                        $"Unknown action: '{action}'. Supported actions include: start_compilation_pipeline (alias: request_compile), get_compilation_summary, wait_for_idle, play, pause, stop, step, get_state (alias: get_current_state), get_project_root, get_windows, get_active_tool, get_selection, set_active_tool, ensure_tag, add_tag, remove_tag, get_tags, ensure_layer, add_layer, remove_layer, get_layers, focus_window."
                    );
            }
        }

        private static object Deprecated(string action, string replacement)
            => Response.ErrorWithCode(
                "action_deprecated",
                $"Action '{action}' is deprecated. Use '{replacement}' instead.");

        // --- Editor State/Info Methods ---
        private static object GetEditorState()
        {
            try
            {
                // Use StateComposer to build comprehensive state
                var fullState = StateComposer.BuildFullState();
                
                // Also include legacy fields for backward compatibility
                var legacyData = new
                {
                    isPlaying = EditorApplication.isPlaying,
                    isPaused = EditorApplication.isPaused,
                    isCompiling = EditorApplication.isCompiling,
                    isUpdating = EditorApplication.isUpdating,
                    applicationPath = EditorApplication.applicationPath,
                    applicationContentsPath = EditorApplication.applicationContentsPath,
                    timeSinceStartup = EditorApplication.timeSinceStartup,
                };

                return new
                {
                    success = true,
                    message = "Retrieved editor state.",
                    data = legacyData,
                    state = fullState // NEW: Full state snapshot
                };
            }
            catch (Exception e)
            {
                return Response.Error($"Error getting editor state: {e.Message}");
            }
        }

        private static object GetProjectRoot()
        {
            try
            {
                // Application.dataPath points to <Project>/Assets
                string assetsPath = Application.dataPath.Replace('\\', '/');
                string projectRoot = Directory.GetParent(assetsPath)?.FullName.Replace('\\', '/');
                if (string.IsNullOrEmpty(projectRoot))
                {
                    return Response.Error("Could not determine project root from Application.dataPath");
                }
                return Response.Success("Project root resolved.", new { projectRoot });
            }
            catch (Exception e)
            {
                return Response.Error($"Error getting project root: {e.Message}");
            }
        }

        private static object GetEditorWindows()
        {
            try
            {
                // Get all types deriving from EditorWindow
                var windowTypes = AppDomain
                    .CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsSubclassOf(typeof(EditorWindow)))
                    .ToList();

                var openWindows = new List<object>();

                // Find currently open instances
                // Resources.FindObjectsOfTypeAll seems more reliable than GetWindow for finding *all* open windows
                EditorWindow[] allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();

                foreach (EditorWindow window in allWindows)
                {
                    if (window == null)
                        continue; // Skip potentially destroyed windows

                    try
                    {
                        openWindows.Add(
                            new
                            {
                                title = window.titleContent.text,
                                typeName = window.GetType().FullName,
                                isFocused = EditorWindow.focusedWindow == window,
                                position = new
                                {
                                    x = window.position.x,
                                    y = window.position.y,
                                    width = window.position.width,
                                    height = window.position.height,
                                },
                                instanceID = window.GetStableInstanceId(),
                            }
                        );
                    }
                    catch (Exception ex)
                    {
                        CodelyLogger.LogWarning(
                            $"Could not get info for window {window.GetType().Name}: {ex.Message}"
                        );
                    }
                }

                return Response.Success("Retrieved list of open editor windows.", openWindows);
            }
            catch (Exception e)
            {
                return Response.Error($"Error getting editor windows: {e.Message}");
            }
        }

        /// <summary>
        /// Focuses an editor window by type name.
        /// Supports common window names like "Console", "Inspector", "Hierarchy", "Project", "Scene", "Game".
        /// </summary>
        private static object FocusWindow(string windowType)
        {
            try
            {
                // Map common names to actual EditorWindow type names
                var typeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Console", "UnityEditor.ConsoleWindow" },
                    { "Inspector", "UnityEditor.InspectorWindow" },
                    { "Hierarchy", "UnityEditor.SceneHierarchyWindow" },
                    { "Project", "UnityEditor.ProjectBrowser" },
                    { "Scene", "UnityEditor.SceneView" },
                    { "Game", "UnityEditor.GameView" },
                    { "Animator", "UnityEditor.Graphs.AnimatorControllerTool" },
                    { "Animation", "UnityEditor.AnimationWindow" },
                    { "Profiler", "UnityEditor.ProfilerWindow" },
                    { "AssetStore", "UnityEditor.AssetStoreWindow" },
                    { "PackageManager", "UnityEditor.PackageManager.UI.PackageManagerWindow" },
                    { "Build", "UnityEditor.BuildPlayerWindow" },
                    { "Lighting", "UnityEditor.LightingWindow" },
                    { "Navigation", "UnityEditor.NavMeshEditorWindow" },
                    { "Occlusion", "UnityEditor.OcclusionCullingWindow" },
                    { "FrameDebugger", "UnityEditor.FrameDebuggerWindow" },
                    { "AudioMixer", "UnityEditor.AudioMixerWindow" }
                };

                string fullTypeName = windowType;
                if (typeMap.TryGetValue(windowType, out var mappedType))
                {
                    fullTypeName = mappedType;
                }

                // Find all open windows
                EditorWindow[] allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
                EditorWindow targetWindow = null;

                foreach (EditorWindow window in allWindows)
                {
                    if (window == null) continue;

                    var winTypeName = window.GetType().FullName;
                    // Match by full type name, short type name, or title
                    if (winTypeName.Equals(fullTypeName, StringComparison.OrdinalIgnoreCase) ||
                        winTypeName.EndsWith("." + windowType, StringComparison.OrdinalIgnoreCase) ||
                        window.GetType().Name.Equals(windowType, StringComparison.OrdinalIgnoreCase) ||
                        window.titleContent.text.Equals(windowType, StringComparison.OrdinalIgnoreCase))
                    {
                        targetWindow = window;
                        break;
                    }
                }

                if (targetWindow == null)
                {
                    // Try to open the window if it's a known type
                    Type windowTypeObj = null;
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        windowTypeObj = assembly.GetType(fullTypeName);
                        if (windowTypeObj != null) break;
                    }

                    if (windowTypeObj != null && typeof(EditorWindow).IsAssignableFrom(windowTypeObj))
                    {
                        // Use GetWindow to open it
                        targetWindow = EditorWindow.GetWindow(windowTypeObj);
                    }
                    else
                    {
                        return Response.Error(
                            $"Window '{windowType}' not found. Available windows can be queried with get_windows action. " +
                            $"Common window types: Console, Inspector, Hierarchy, Project, Scene, Game, Animator, Animation, Profiler."
                        );
                    }
                }

                // Bring the Unity application itself to the foreground first; otherwise
                // EditorWindow.Focus() only changes focus *within* Unity and the user
                // won't see it if another app or a minimized Unity is on top.
                NativeWindowFocus.BringUnityToFront();

                // Focus the window
                targetWindow.Focus();

                // Verify focus was successful
                bool isFocused = EditorWindow.focusedWindow == targetWindow;

                return Response.Success(
                    $"Focused window: {targetWindow.titleContent.text} ({targetWindow.GetType().Name})",
                    new
                    {
                        windowType = targetWindow.GetType().FullName,
                        title = targetWindow.titleContent.text,
                        isFocused = isFocused,
                        instanceID = targetWindow.GetStableInstanceId()
                    }
                );
            }
            catch (Exception e)
            {
                return Response.Error($"Error focusing window '{windowType}': {e.Message}");
            }
        }

        private static object GetActiveTool()
        {
            try
            {
                Tool currentTool = UnityEditor.Tools.current;
                string toolName = currentTool.ToString(); // Enum to string
                bool customToolActive = UnityEditor.Tools.current == Tool.Custom; // Check if a custom tool is active
                string activeToolName = customToolActive
                    ? EditorTools.GetActiveToolName()
                    : toolName; // Get custom name if needed

                // Convert Unity types to serializable arrays to avoid self-referencing loop
                var handleRot = UnityEditor.Tools.handleRotation.eulerAngles;
                var handlePos = UnityEditor.Tools.handlePosition;

                var toolInfo = new
                {
                    activeTool = activeToolName,
                    isCustom = customToolActive,
                    pivotMode = UnityEditor.Tools.pivotMode.ToString(),
                    pivotRotation = UnityEditor.Tools.pivotRotation.ToString(),
                    handleRotation = new float[] { handleRot.x, handleRot.y, handleRot.z },
                    handlePosition = new float[] { handlePos.x, handlePos.y, handlePos.z },
                };

                return Response.Success("Retrieved active tool information.", toolInfo);
            }
            catch (Exception e)
            {
                return Response.Error($"Error getting active tool: {e.Message}");
            }
        }

        private static object SetActiveTool(string toolName)
        {
            try
            {
                Tool targetTool;
                if (Enum.TryParse<Tool>(toolName, true, out targetTool)) // Case-insensitive parse
                {
                    // Check if it's a valid built-in tool
                    if (targetTool != Tool.None && targetTool <= Tool.Custom) // Tool.Custom is the last standard tool
                    {
                        UnityEditor.Tools.current = targetTool;
                        return Response.Success($"Set active tool to '{targetTool}'.");
                    }
                    else
                    {
                        return Response.Error(
                            $"Cannot directly set tool to '{toolName}'. It might be None, Custom, or invalid."
                        );
                    }
                }
                else
                {
                    // Potentially try activating a custom tool by name here if needed
                    // This often requires specific editor scripting knowledge for that tool.
                    return Response.Error(
                        $"Could not parse '{toolName}' as a standard Unity Tool (View, Move, Rotate, Scale, Rect, Transform, Custom)."
                    );
                }
            }
            catch (Exception e)
            {
                return Response.Error($"Error setting active tool: {e.Message}");
            }
        }
        /// <summary>
        /// Returns the hierarchy path of a GameObject (e.g. "Root/Child/Grandchild").
        /// Kept consistent with SelectionChangedNotifier so the `path` field has identical
        /// semantics in get_selection responses and selection_changed notifications.
        /// </summary>
        private static string GetGameObjectPath(UnityEngine.GameObject obj)
        {
            if (obj == null) return string.Empty;

            var path = obj.name;
            var parent = obj.transform != null ? obj.transform.parent : null;
            while (parent != null)
            {
                path = $"{parent.name}/{path}";
                parent = parent.parent;
            }
            return path;
        }
        private static object GetSelection()
        {
            try
            {
                string activeAssetPath = Selection.activeObject != null
                    ? AssetDatabase.GetAssetPath(Selection.activeObject)
                    : null;
                string activeAsset = string.IsNullOrEmpty(activeAssetPath)
                    ? null
                    : Path.GetFullPath(activeAssetPath).Replace('\\', '/');

                var selectionInfo = new
                {
                    activeObject = Selection.activeObject?.name,
                    activeAsset = activeAsset,
                    activeGameObject = Selection.activeGameObject?.name,
                    activeTransform = Selection.activeTransform?.name,
                    activeInstanceID = InstanceIdExtensions.ActiveSelectionInstanceId(),
#if UNITY_2020_1_OR_NEWER
                    count = Selection.count,
#else
                    count = Selection.objects?.Length ?? 0,
#endif
                    objects = Selection
                        .objects.Select(obj => new
                        {
                            name = obj?.name,
                            type = obj?.GetType().FullName,
                            instanceID = obj?.GetStableInstanceId(),
                        })
                        .ToList(),
                    gameObjects = Selection
                        .gameObjects.Select(go => new
                        {
                            name = go?.name,
                            path = GetGameObjectPath(go),
                            instanceID = go?.GetStableInstanceId(),
                        })
                        .ToList(),
                    assetGUIDs = Selection.assetGUIDs, // GUIDs for selected assets in Project view
                    assetPaths = Selection.assetGUIDs?.Select(AssetDatabase.GUIDToAssetPath).ToArray(),
                };

                return Response.Success("Retrieved current selection details.", selectionInfo);
            }
            catch (Exception e)
            {
                return Response.Error($"Error getting selection: {e.Message}");
            }
        }

        // --- Tag Management Methods ---

        private static object AddTag(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return Response.Error("Tag name cannot be empty or whitespace.");

            // Check if tag already exists
            if (InternalEditorUtility.tags.Contains(tagName))
            {
                return Response.Error($"Tag '{tagName}' already exists.");
            }

            try
            {
                // Add the tag using the internal utility
                InternalEditorUtility.AddTag(tagName);
                // Force save assets to ensure the change persists in the TagManager asset
                AssetDatabase.SaveAssets();
                return Response.Success($"Tag '{tagName}' added successfully.");
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to add tag '{tagName}': {e.Message}");
            }
        }
        
        /// <summary>
        /// Idempotent ensure tag - adds tag if not exists, returns success if already exists.
        /// </summary>
        private static object EnsureTag(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return Response.Error("Tag name cannot be empty or whitespace.");

            // Check if tag already exists
            if (InternalEditorUtility.tags.Contains(tagName))
            {
                return new
                {
                    success = true,
                    message = $"Tag '{tagName}' already exists.",
                    data = new { tagName = tagName, alreadyExists = true }
                };
            }

            // Tag doesn't exist, add it
            try
            {
                InternalEditorUtility.AddTag(tagName);
                AssetDatabase.SaveAssets();
                return new
                {
                    success = true,
                    message = $"Tag '{tagName}' created successfully.",
                    data = new { tagName = tagName, alreadyExists = false }
                };
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to ensure tag '{tagName}': {e.Message}");
            }
        }

        private static object RemoveTag(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return Response.Error("Tag name cannot be empty or whitespace.");
            if (tagName.Equals("Untagged", StringComparison.OrdinalIgnoreCase))
                return Response.Error("Cannot remove the built-in 'Untagged' tag.");

            // Check if tag exists before attempting removal
            if (!InternalEditorUtility.tags.Contains(tagName))
            {
                return Response.Error($"Tag '{tagName}' does not exist.");
            }

            try
            {
                // Remove the tag using the internal utility
                InternalEditorUtility.RemoveTag(tagName);
                // Force save assets
                AssetDatabase.SaveAssets();
                return Response.Success($"Tag '{tagName}' removed successfully.");
            }
            catch (Exception e)
            {
                // Catch potential issues if the tag is somehow in use or removal fails
                return Response.Error($"Failed to remove tag '{tagName}': {e.Message}");
            }
        }

        private static object GetTags()
        {
            try
            {
                string[] tags = InternalEditorUtility.tags;
                return Response.Success("Retrieved current tags.", tags);
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to retrieve tags: {e.Message}");
            }
        }

        // --- Layer Management Methods ---

        private static object AddLayer(string layerName)
        {
            if (string.IsNullOrWhiteSpace(layerName))
                return Response.Error("Layer name cannot be empty or whitespace.");

            // Access the TagManager asset
            SerializedObject tagManager = GetTagManager();
            if (tagManager == null)
                return Response.Error("Could not access TagManager asset.");

            SerializedProperty layersProp = tagManager.FindProperty("layers");
            if (layersProp == null || !layersProp.isArray)
                return Response.Error("Could not find 'layers' property in TagManager.");

            // Check if layer name already exists (case-insensitive check recommended)
            for (int i = 0; i < TotalLayerCount; i++)
            {
                SerializedProperty layerSP = layersProp.GetArrayElementAtIndex(i);
                if (
                    layerSP != null
                    && layerName.Equals(layerSP.stringValue, StringComparison.OrdinalIgnoreCase)
                )
                {
                    return Response.Error($"Layer '{layerName}' already exists at index {i}.");
                }
            }

            // Find the first empty user layer slot (indices 8 to 31)
            int firstEmptyUserLayer = -1;
            for (int i = FirstUserLayerIndex; i < TotalLayerCount; i++)
            {
                SerializedProperty layerSP = layersProp.GetArrayElementAtIndex(i);
                if (layerSP != null && string.IsNullOrEmpty(layerSP.stringValue))
                {
                    firstEmptyUserLayer = i;
                    break;
                }
            }

            if (firstEmptyUserLayer == -1)
            {
                return Response.Error("No empty User Layer slots available (8-31 are full).");
            }

            // Assign the name to the found slot
            try
            {
                SerializedProperty targetLayerSP = layersProp.GetArrayElementAtIndex(
                    firstEmptyUserLayer
                );
                targetLayerSP.stringValue = layerName;
                // Apply the changes to the TagManager asset
                tagManager.ApplyModifiedProperties();
                // Save assets to make sure it's written to disk
                AssetDatabase.SaveAssets();
                return Response.Success(
                    $"Layer '{layerName}' added successfully to slot {firstEmptyUserLayer}."
                );
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to add layer '{layerName}': {e.Message}");
            }
        }
        
        /// <summary>
        /// Idempotent ensure layer - adds layer if not exists, returns success if already exists.
        /// </summary>
        private static object EnsureLayer(string layerName)
        {
            if (string.IsNullOrWhiteSpace(layerName))
                return Response.Error("Layer name cannot be empty or whitespace.");

            // Access the TagManager asset
            SerializedObject tagManager = GetTagManager();
            if (tagManager == null)
                return Response.Error("Could not access TagManager asset.");

            SerializedProperty layersProp = tagManager.FindProperty("layers");
            if (layersProp == null || !layersProp.isArray)
                return Response.Error("Could not find 'layers' property in TagManager.");

            // Check if layer already exists
            for (int i = 0; i < TotalLayerCount; i++)
            {
                SerializedProperty layerSP = layersProp.GetArrayElementAtIndex(i);
                if (layerSP != null && layerName.Equals(layerSP.stringValue, StringComparison.OrdinalIgnoreCase))
                {
                    return new
                    {
                        success = true,
                        message = $"Layer '{layerName}' already exists at index {i}.",
                        data = new { layerName = layerName, layerIndex = i, alreadyExists = true }
                    };
                }
            }

            // Find first empty user layer slot
            int firstEmptyUserLayer = -1;
            for (int i = FirstUserLayerIndex; i < TotalLayerCount; i++)
            {
                SerializedProperty layerSP = layersProp.GetArrayElementAtIndex(i);
                if (layerSP != null && string.IsNullOrEmpty(layerSP.stringValue))
                {
                    firstEmptyUserLayer = i;
                    break;
                }
            }

            if (firstEmptyUserLayer == -1)
            {
                return Response.Error("No empty User Layer slots available (8-31 are full).");
            }

            // Add the layer
            try
            {
                SerializedProperty targetLayerSP = layersProp.GetArrayElementAtIndex(firstEmptyUserLayer);
                targetLayerSP.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                return new
                {
                    success = true,
                    message = $"Layer '{layerName}' created at slot {firstEmptyUserLayer}.",
                    data = new { layerName = layerName, layerIndex = firstEmptyUserLayer, alreadyExists = false }
                };
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to ensure layer '{layerName}': {e.Message}");
            }
        }

        private static object RemoveLayer(string layerName)
        {
            if (string.IsNullOrWhiteSpace(layerName))
                return Response.Error("Layer name cannot be empty or whitespace.");

            // Access the TagManager asset
            SerializedObject tagManager = GetTagManager();
            if (tagManager == null)
                return Response.Error("Could not access TagManager asset.");

            SerializedProperty layersProp = tagManager.FindProperty("layers");
            if (layersProp == null || !layersProp.isArray)
                return Response.Error("Could not find 'layers' property in TagManager.");

            // Find the layer by name (must be user layer)
            int layerIndexToRemove = -1;
            for (int i = FirstUserLayerIndex; i < TotalLayerCount; i++) // Start from user layers
            {
                SerializedProperty layerSP = layersProp.GetArrayElementAtIndex(i);
                // Case-insensitive comparison is safer
                if (
                    layerSP != null
                    && layerName.Equals(layerSP.stringValue, StringComparison.OrdinalIgnoreCase)
                )
                {
                    layerIndexToRemove = i;
                    break;
                }
            }

            if (layerIndexToRemove == -1)
            {
                return Response.Error($"User layer '{layerName}' not found.");
            }

            // Clear the name for that index
            try
            {
                SerializedProperty targetLayerSP = layersProp.GetArrayElementAtIndex(
                    layerIndexToRemove
                );
                targetLayerSP.stringValue = string.Empty; // Set to empty string to remove
                // Apply the changes
                tagManager.ApplyModifiedProperties();
                // Save assets
                AssetDatabase.SaveAssets();
                return Response.Success(
                    $"Layer '{layerName}' (slot {layerIndexToRemove}) removed successfully."
                );
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to remove layer '{layerName}': {e.Message}");
            }
        }

        private static object GetLayers()
        {
            try
            {
                var layers = new Dictionary<int, string>();
                for (int i = 0; i < TotalLayerCount; i++)
                {
                    string layerName = LayerMask.LayerToName(i);
                    if (!string.IsNullOrEmpty(layerName)) // Only include layers that have names
                    {
                        layers.Add(i, layerName);
                    }
                }
                return Response.Success("Retrieved current named layers.", layers);
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to retrieve layers: {e.Message}");
            }
        }

        // --- Compilation Management Methods ---

        /// <summary>
        /// Ensures compilation event callbacks are registered. These only log: compilation state is
        /// read straight from the editor wherever it matters (<see cref="WaitForCompileJob"/>,
        /// <see cref="StateComposer"/>), so there is no bookkeeping to keep in step.
        /// </summary>
        private static void EnsureCompilationCallbacksRegistered()
        {
            lock (_compilationLock)
            {
                if (!_compilationCallbackRegistered)
                {
                    UnityEditor.Compilation.CompilationPipeline.compilationStarted += OnCompilationStarted;
                    UnityEditor.Compilation.CompilationPipeline.compilationFinished += OnCompilationFinished;
                    _compilationCallbackRegistered = true;
                    CodelyLogger.Log("[ManageEditor] Compilation callbacks registered");
                }
            }
        }

        private static void OnCompilationStarted(object obj)
            => CodelyLogger.Log("[ManageEditor] Compilation started");

        private static void OnCompilationFinished(object obj)
            => CodelyLogger.Log("[ManageEditor] Compilation finished");

        // --- Helper Methods ---

        /// <summary>
        /// Gets the SerializedObject for the TagManager asset.
        /// </summary>
        private static SerializedObject GetTagManager()
        {
            try
            {
                // Load the TagManager asset from the ProjectSettings folder
                UnityEngine.Object[] tagManagerAssets = AssetDatabase.LoadAllAssetsAtPath(
                    "ProjectSettings/TagManager.asset"
                );
                if (tagManagerAssets == null || tagManagerAssets.Length == 0)
                {
                    CodelyLogger.LogError("[ManageEditor] TagManager.asset not found in ProjectSettings.");
                    return null;
                }
                // The first object in the asset file should be the TagManager
                return new SerializedObject(tagManagerAssets[0]);
            }
            catch (Exception e)
            {
                CodelyLogger.LogError($"[ManageEditor] Error accessing TagManager.asset: {e.Message}");
                return null;
            }
        }
    }

    /// <summary>
    /// Brings the Unity Editor application window to the OS foreground.
    /// On Windows uses Win32 (SetForegroundWindow with the AttachThreadInput trick to
    /// bypass the foreground-lock restriction). On macOS calls
    /// [[NSApplication sharedApplication] activateIgnoringOtherApps:YES] via the Obj-C runtime.
    /// </summary>
    internal static class NativeWindowFocus
    {
        public static void BringUnityToFront()
        {
#if UNITY_EDITOR_WIN
            BringToFrontWin();
#elif UNITY_EDITOR_OSX
            BringToFrontMac();
#endif
        }

#if UNITY_EDITOR_WIN
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        private const int SW_RESTORE = 9;

        private static void BringToFrontWin()
        {
            try
            {
                var hwnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                if (hwnd == IntPtr.Zero) return;

                if (IsIconic(hwnd)) ShowWindow(hwnd, SW_RESTORE);

                var foreHwnd = GetForegroundWindow();
                uint foreThread = GetWindowThreadProcessId(foreHwnd, out _);
                uint thisThread = GetCurrentThreadId();

                bool attached = false;
                if (foreThread != 0 && foreThread != thisThread)
                {
                    attached = AttachThreadInput(foreThread, thisThread, true);
                }
                SetForegroundWindow(hwnd);
                if (attached)
                {
                    AttachThreadInput(foreThread, thisThread, false);
                }
            }
            catch (Exception e)
            {
                CodelyLogger.LogWarning($"[NativeWindowFocus] Windows bring-to-front failed: {e.Message}");
            }
        }
#endif

#if UNITY_EDITOR_OSX
        private const string LibObjC = "/usr/lib/libobjc.dylib";

        [System.Runtime.InteropServices.DllImport(LibObjC)]
        private static extern IntPtr objc_getClass(string name);

        [System.Runtime.InteropServices.DllImport(LibObjC)]
        private static extern IntPtr sel_registerName(string name);

        [System.Runtime.InteropServices.DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

        [System.Runtime.InteropServices.DllImport(LibObjC, EntryPoint = "objc_msgSend")]
        private static extern void objc_msgSend_bool(IntPtr receiver, IntPtr selector, bool arg);

        private static void BringToFrontMac()
        {
            try
            {
                // Equivalent of: [[NSApplication sharedApplication] activateIgnoringOtherApps:YES];
                var nsAppClass = objc_getClass("NSApplication");
                if (nsAppClass == IntPtr.Zero) return;

                var sharedSel = sel_registerName("sharedApplication");
                var nsApp = objc_msgSend(nsAppClass, sharedSel);
                if (nsApp == IntPtr.Zero) return;

                var activateSel = sel_registerName("activateIgnoringOtherApps:");
                objc_msgSend_bool(nsApp, activateSel, true);
            }
            catch (Exception e)
            {
                CodelyLogger.LogWarning($"[NativeWindowFocus] macOS bring-to-front failed: {e.Message}");
            }
        }
#endif
    }

    // Helper class to get custom tool names (remains the same)
    internal static class EditorTools
    {
        public static string GetActiveToolName()
        {
            // This is a placeholder. Real implementation depends on how custom tools
            // are registered and tracked in the specific Unity project setup.
            // It might involve checking static variables, calling methods on specific tool managers, etc.
            if (UnityEditor.Tools.current == Tool.Custom)
            {
                // Example: Check a known custom tool manager
                // if (MyCustomToolManager.IsActive) return MyCustomToolManager.ActiveToolName;
                return "Unknown Custom Tool";
            }
            return UnityEditor.Tools.current.ToString();
        }
    }
}

