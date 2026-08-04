using System;
using System.IO;
using Codely.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityTcp.Editor.Helpers;

namespace UnityTcp.Editor.Tools
{
    /// <summary>
    /// [EXPERIMENTAL] Handles UI Toolkit operations (UXML, USS, PanelSettings).
    /// </summary>
    public static class ManageUIToolkit
    {
        public static object HandleCommand(JObject @params)
        {
            string action = @params["action"]?.ToString().ToLower();
            if (string.IsNullOrEmpty(action))
            {
                return Response.Error("Action parameter is required.");
            }

            try
            {
                switch (action)
                {
                    case "ensure_panel_settings_asset":
                        return EnsurePanelSettingsAsset(@params);
                    case "link_uss_to_uxml":
                        return LinkUssToUxml(@params);
                    case "create_uxml":
                        return CreateUxml(@params);
                    case "create_uss":
                        return CreateUss(@params);
                    default:
                        return Response.Error(
                            $"Unknown action: '{action}'. Valid actions: ensure_panel_settings_asset, link_uss_to_uxml, create_uxml, create_uss."
                        );
                }
            }
            catch (Exception e)
            {
                CodelyLogger.LogError($"[ManageUIToolkit] Action '{action}' failed: {e}");
                return Response.Error($"[EXPERIMENTAL] UI Toolkit operation failed: {e.Message}");
            }
        }

        private static object EnsurePanelSettingsAsset(JObject @params)
        {
#if !UNITY_2021_2_OR_NEWER
            return Response.Error("[EXPERIMENTAL] ensure_panel_settings_asset requires Unity 2021.2+ (PanelSettings is part of the runtime UI Toolkit not present in this Editor version).");
#else
            try
            {
                var writeCheck = WriteGuard.CheckWriteAllowed("ensure_panel_settings_asset");
                if (writeCheck != null) return writeCheck;

                string path = @params["path"]?.ToString();
                if (string.IsNullOrEmpty(path))
                    return Response.Error("'path' parameter required.");

                if (!path.EndsWith(".asset"))
                    path += ".asset";

                // Check if already exists
                var existingAsset = AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
                if (existingAsset != null)
                {
                    return new
                    {
                        success = true,
                        message = "[EXPERIMENTAL] PanelSettings asset already exists.",
                        data = new { path = path, alreadyExists = true }
                    };
                }

                // Create directory if needed
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // Create PanelSettings asset
                var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(panelSettings, path);
                AssetDatabase.SaveAssets();

                return new
                {
                    success = true,
                    message = "[EXPERIMENTAL] PanelSettings asset created.",
                    data = new { path = path, alreadyExists = false }
                };
            }
            catch (Exception e)
            {
                return Response.Error($"[EXPERIMENTAL] Failed to ensure PanelSettings asset: {e.Message}");
            }
#endif
        }

        private static object LinkUssToUxml(JObject @params)
        {
            try
            {
                var writeCheck = WriteGuard.CheckWriteAllowed("link_uss_to_uxml");
                if (writeCheck != null) return writeCheck;

                // Support both parameter naming conventions: uxml/uss and uxml_path/uss_path,
                // as well as GUID-based references via uxml_guid/uss_guid.
                string uxmlShorthand = @params["uxml"]?.ToString();
                string uxmlPathParam = @params["uxml_path"]?.ToString();
                string uxmlGuidParam = @params["uxml_guid"]?.ToString();

                string ussShorthand = @params["uss"]?.ToString();
                string ussPathParam = @params["uss_path"]?.ToString();
                string ussGuidParam = @params["uss_guid"]?.ToString();

                bool hasUxmlIdentifier =
                    !string.IsNullOrEmpty(uxmlShorthand)
                    || !string.IsNullOrEmpty(uxmlPathParam)
                    || !string.IsNullOrEmpty(uxmlGuidParam);
                bool hasUssIdentifier =
                    !string.IsNullOrEmpty(ussShorthand)
                    || !string.IsNullOrEmpty(ussPathParam)
                    || !string.IsNullOrEmpty(ussGuidParam);

                if (!hasUxmlIdentifier)
                    return Response.Error("Either 'uxml', 'uxml_path', or 'uxml_guid' parameter is required.");
                if (!hasUssIdentifier)
                    return Response.Error("Either 'uss', 'uss_path', or 'uss_guid' parameter is required.");

                string uxmlGuidUsed;
                string ussGuidUsed;

                string uxmlPath = ResolveAssetPath(uxmlShorthand, uxmlPathParam, uxmlGuidParam, out uxmlGuidUsed);
                string ussPath = ResolveAssetPath(ussShorthand, ussPathParam, ussGuidParam, out ussGuidUsed);

                if (string.IsNullOrEmpty(uxmlPath))
                {
                    if (!string.IsNullOrEmpty(uxmlGuidUsed))
                    {
                        return Response.Error($"UXML asset not found for GUID: {uxmlGuidUsed}");
                    }

                    return Response.Error("UXML path could not be resolved.");
                }

                if (string.IsNullOrEmpty(ussPath))
                {
                    if (!string.IsNullOrEmpty(ussGuidUsed))
                    {
                        return Response.Error($"USS asset not found for GUID: {ussGuidUsed}");
                    }

                    return Response.Error("USS path could not be resolved.");
                }

                var uxmlAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
                if (uxmlAsset == null)
                    return Response.Error($"UXML not found at: {uxmlPath}");

                var ussAsset = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
                if (ussAsset == null)
                    return Response.Error($"USS not found at: {ussPath}");

                // Read UXML file content
                string uxmlContent = File.ReadAllText(uxmlPath);

                // Check if USS is already linked
                string ussFileName = Path.GetFileName(ussPath);
                if (uxmlContent.Contains($"src=\"{ussFileName}\""))
                {
                    return new
                    {
                        success = true,
                        message = "[EXPERIMENTAL] USS already linked to UXML.",
                        data = new { uxml = uxmlPath, uss = ussPath, alreadyLinked = true }
                    };
                }

                // Add USS reference to UXML
                // Insert after <ui:UXML> tag
                int insertPos = uxmlContent.IndexOf("<ui:UXML");
                if (insertPos >= 0)
                {
                    insertPos = uxmlContent.IndexOf('>', insertPos) + 1;
                    string styleTag = $"\n    <Style src=\"{ussFileName}\" />";
                    uxmlContent = uxmlContent.Insert(insertPos, styleTag);
                    File.WriteAllText(uxmlPath, uxmlContent);
                    AssetDatabase.ImportAsset(uxmlPath);

                    return new
                    {
                        success = true,
                        message = "[EXPERIMENTAL] USS linked to UXML.",
                        data = new { uxml = uxmlPath, uss = ussPath, alreadyLinked = false }
                    };
                }
                else
                {
                    return Response.Error("Failed to parse UXML file.");
                }
            }
            catch (Exception e)
            {
                return Response.Error($"[EXPERIMENTAL] Failed to link USS to UXML: {e.Message}");
            }
        }

        /// <summary>
        /// Resolves an asset path from either a shorthand (path or GUID), an explicit path, or a GUID.
        /// - If explicit path is provided, it is returned as-is.
        /// - If shorthand contains '/', it is treated as a path.
        /// - Otherwise shorthand / guid are treated as GUIDs and resolved via AssetDatabase.GUIDToAssetPath.
        /// Returns null if the asset path cannot be resolved.
        /// </summary>
        private static string ResolveAssetPath(string shorthand, string pathParam, string guidParam, out string guidUsed)
        {
            guidUsed = null;

            // Prefer explicit path if provided
            if (!string.IsNullOrEmpty(pathParam))
            {
                return pathParam;
            }

            // Handle shorthand: path or GUID
            if (!string.IsNullOrEmpty(shorthand))
            {
                if (shorthand.Contains("/"))
                {
                    // Looks like a path (ValidateToolParams already enforced "Assets/" prefix when appropriate)
                    return shorthand;
                }

                // Treat shorthand as GUID
                guidUsed = shorthand;
                string resolvedFromShorthand = AssetDatabase.GUIDToAssetPath(shorthand);
                if (!string.IsNullOrEmpty(resolvedFromShorthand))
                {
                    return resolvedFromShorthand;
                }

                // Fall through and let guidParam try (could be different)
            }

            if (!string.IsNullOrEmpty(guidParam))
            {
                guidUsed = guidParam;
                string resolvedFromGuid = AssetDatabase.GUIDToAssetPath(guidParam);
                if (!string.IsNullOrEmpty(resolvedFromGuid))
                {
                    return resolvedFromGuid;
                }
            }

            return null;
        }

        private static object CreateUxml(JObject @params)
        {
            try
            {
                var writeCheck = WriteGuard.CheckWriteAllowed("create_uxml");
                if (writeCheck != null) return writeCheck;

                string path = @params["path"]?.ToString();
                if (string.IsNullOrEmpty(path))
                    return Response.Error("'path' parameter required.");

                if (!path.EndsWith(".uxml"))
                    path += ".uxml";

                if (File.Exists(path))
                    return Response.Error($"UXML file already exists at: {path}");

                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string template = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ui:UXML
    xmlns:ui=""UnityEngine.UIElements""
    xmlns:uie=""UnityEditor.UIElements"">
    <ui:VisualElement name=""root"" style=""flex-grow: 1; background-color: rgb(255, 255, 255); justify-content: center; align-items: center;"">
        <ui:Label text=""Hello UI Toolkit"" name=""title-label"" style=""font-size: 48px; -unity-font-style: bold; color: rgb(0, 0, 0); padding: 16px; -unity-text-align: middle-center;"" />
    </ui:VisualElement>
</ui:UXML>";

                File.WriteAllText(path, template);
                AssetDatabase.ImportAsset(path);

                return new
                {
                    success = true,
                    message = "[EXPERIMENTAL] UXML file created.",
                    data = new { path = path }
                };
            }
            catch (Exception e)
            {
                return Response.Error($"[EXPERIMENTAL] Failed to create UXML: {e.Message}");
            }
        }

        private static object CreateUss(JObject @params)
        {
            try
            {
                var writeCheck = WriteGuard.CheckWriteAllowed("create_uss");
                if (writeCheck != null) return writeCheck;

                string path = @params["path"]?.ToString();
                if (string.IsNullOrEmpty(path))
                    return Response.Error("'path' parameter required.");

                if (!path.EndsWith(".uss"))
                    path += ".uss";

                if (File.Exists(path))
                    return Response.Error($"USS file already exists at: {path}");

                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string template = @"/* UI Toolkit Style Sheet */
.root {
    flex-grow: 1;
    background-color: rgb(56, 56, 56);
}

#title-label {
    font-size: 20px;
    -unity-font-style: bold;
    color: rgb(255, 255, 255);
    padding: 10px;
}
";

                File.WriteAllText(path, template);
                AssetDatabase.ImportAsset(path);

                return new
                {
                    success = true,
                    message = "[EXPERIMENTAL] USS file created.",
                    data = new { path = path }
                };
            }
            catch (Exception e)
            {
                return Response.Error($"[EXPERIMENTAL] Failed to create USS: {e.Message}");
            }
        }
    }
}

