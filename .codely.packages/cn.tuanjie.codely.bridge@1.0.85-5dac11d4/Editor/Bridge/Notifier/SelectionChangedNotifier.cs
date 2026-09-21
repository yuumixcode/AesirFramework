using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codely.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityTcp.Editor.Helpers;

namespace UnityTcp.Editor.Notifier
{
    /// <summary>
    /// Broadcasts the current Editor selection to notifier clients whenever it
    /// changes. The emitted payload mirrors the structure returned by
    /// <see cref="UnityTcp.Editor.Tools.ManageEditor"/>'s get_selection action.
    /// </summary>
    [InitializeOnLoad]
    public static class SelectionChangedNotifier
    {
        private const string EventType = "selection_changed";

        private static bool _flushScheduled;

        static SelectionChangedNotifier()
        {
            Selection.selectionChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged() => ScheduleFlush();

        private static void ScheduleFlush()
        {
            if (_flushScheduled) return;
            _flushScheduled = true;
            EditorApplication.delayCall += Flush;
        }

        private static void Flush()
        {
            _flushScheduled = false;
            try
            {
                if (!UnityTcpBridge.IsRunning) return;

                JObject payload = BuildSelectionPayload();
                if (payload == null) return;

                UnityTcpBridge.NotifyAll(EventType, payload);
            }
            catch (Exception e)
            {
                try { CodelyLogger.LogError($"[SelectionChangedNotifier] flush failed: {e.Message}"); } catch { }
            }
        }

        private static JObject BuildSelectionPayload()
        {
            var assetGUIDs = Selection.assetGUIDs;
            var assetPaths = assetGUIDs?.Select(AssetDatabase.GUIDToAssetPath).ToArray();

            string activeAssetPath = Selection.activeObject != null
                ? AssetDatabase.GetAssetPath(Selection.activeObject)
                : null;
            string activeAsset = string.IsNullOrEmpty(activeAssetPath)
                ? null
                : Path.GetFullPath(activeAssetPath).Replace('\\', '/');

            var payload = new
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
                objects = Selection.objects
                    .Where(obj => obj != null)
                    .Select(obj => new
                    {
                        name = obj.name,
                        type = obj.GetType().FullName,
                        instanceID = obj.GetStableInstanceId(),
                    })
                    .ToList(),
                gameObjects = Selection.gameObjects
                    .Where(go => go != null)
                    .Select(go => new
                    {
                        name = go.name,
                        path = GetGameObjectHierarchyPath(go),
                        instanceID = go.GetStableInstanceId(),
                    })
                    .ToList(),
                assetGUIDs = assetGUIDs,
                assetPaths = assetPaths,
            };

            return JObject.FromObject(payload);
        }

        private static string GetGameObjectHierarchyPath(GameObject obj)
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
    }
}
