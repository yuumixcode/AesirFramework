using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Codely.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityTcp.Editor.Helpers; // For Response class
using static UnityTcp.Editor.Tools.ManageGameObject;

#if UNITY_6000_0_OR_NEWER
using PhysicsMaterialType = UnityEngine.PhysicsMaterial;
using PhysicsMaterialCombine = UnityEngine.PhysicsMaterialCombine;  
#else
using PhysicsMaterialType = UnityEngine.PhysicMaterial;
using PhysicsMaterialCombine = UnityEngine.PhysicMaterialCombine;
#endif

namespace UnityTcp.Editor.Tools
{
    /// <summary>
    /// Handles asset management operations within the Unity project.
    /// </summary>
    public static class ManageAsset
    {
        // --- Main Handler ---

        private static readonly Dictionary<string, Func<JObject, object>> ActionHandlers =
            new Dictionary<string, Func<JObject, object>>
            {
                { "ensure_has_meta", p => EnsureHasMeta(p["path"]?.ToString()) },
                { "ensure_meta_integrity", p => EnsureMetaIntegrity(p["path"]?.ToString()) },
                { "import", p => ReimportAsset(p["path"]?.ToString(), p["properties"] as JObject) },
                { "create", CreateAsset },
                { "modify", p => ModifyAsset(p["path"]?.ToString(), p["properties"] as JObject) },
                { "delete", p => DeleteAsset(p["path"]?.ToString()) },
                { "duplicate", p => DuplicateAsset(p["path"]?.ToString(), p["destination"]?.ToString()) },
                { "move", p => MoveOrRenameAsset(p["path"]?.ToString(), p["destination"]?.ToString()) },
                { "search", SearchAssets },
                { "get_info", p => GetAssetInfo(
                    p["path"]?.ToString(),
                    p["generatePreview"]?.ToObject<bool>() ?? false) },
                { "create_folder", p => CreateFolder(p["path"]?.ToString()) },
                { "get_components", p => GetComponentsFromAsset(p["path"]?.ToString()) },
            };

        public static object HandleCommand(JObject @params)
            => ActionRouter.Route(@params, ActionHandlers);

        // --- Action Implementations ---

        private static object ReimportAsset(string path, JObject properties)
        {
            if (string.IsNullOrEmpty(path))
                return Response.Error("'path' is required for reimport.");
            string fullPath = SanitizeAssetPath(path);
            bool ghostDesync;
            if (!AssetExists(fullPath, out ghostDesync))
                return BuildAssetNotFoundResponse($"Asset not found at path: {fullPath}", fullPath, ghostDesync);

            try
            {
                if (properties != null && properties.HasValues)
                {
                    return Response.Error(
                        "Modifying importer properties is not supported. Call import without properties to force reimport."
                    );
                }

                AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);
                // AssetDatabase.Refresh(); // Usually ImportAsset handles refresh
                return Response.Success($"Asset '{fullPath}' reimported.", GetAssetData(fullPath));
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to reimport asset '{fullPath}': {e.Message}");
            }
        }

        private static object CreateAsset(JObject @params)
        {
            string path = @params["path"]?.ToString();
            string assetType = @params["assetType"]?.ToString();
            JObject properties = @params["properties"] as JObject;

            if (string.IsNullOrEmpty(path))
                return Response.Error("'path' is required for create.");
            if (string.IsNullOrEmpty(assetType))
                return Response.Error("'assetType' is required for create.");

            string fullPath = SanitizeAssetPath(path);
            string directory = Path.GetDirectoryName(fullPath);

            EnsureDirectoryExists(directory);

            if (AssetExists(fullPath))
                return Response.Error($"Asset already exists at path: {fullPath}");

            try
            {
                UnityEngine.Object newAsset = null;
                string lowerAssetType = assetType.ToLowerInvariant();

                // Handle common asset types
                if (lowerAssetType == "folder")
                {
                    return CreateFolder(path); // Use dedicated method
                }
                else if (lowerAssetType == "material")
                {
                    // Prefer provided shader; fall back to common pipelines
                    var requested = properties?["shader"]?.ToString();
                    Shader shader =
                        (!string.IsNullOrEmpty(requested) ? Shader.Find(requested) : null)
                        ?? Shader.Find("Universal Render Pipeline/Lit")
                        ?? Shader.Find("HDRP/Lit")
                        ?? Shader.Find("Standard")
                        ?? Shader.Find("Unlit/Color");
                    if (shader == null)
                        return Response.Error($"Could not find a suitable shader (requested: '{requested ?? "none"}').");

                    var mat = new Material(shader);
                    if (properties != null)
                        ApplyMaterialProperties(mat, properties);
                    AssetDatabase.CreateAsset(mat, fullPath);
                    newAsset = mat;
                }
                else if (lowerAssetType == "physicsmaterial")
                {
                    PhysicsMaterialType pmat = new PhysicsMaterialType();
                    if (properties != null)
                        ApplyPhysicsMaterialProperties(pmat, properties);
                    AssetDatabase.CreateAsset(pmat, fullPath);
                    newAsset = pmat;
                }
                else if (lowerAssetType == "scriptableobject")
                {
                    string scriptClassName = properties?["scriptClass"]?.ToString();
                    if (string.IsNullOrEmpty(scriptClassName))
                        return Response.Error(
                            "'scriptClass' property required when creating ScriptableObject asset."
                        );

                    // NOTE:
                    // Previously this used ComponentResolver.TryResolve, which is intentionally limited to
                    // Component/MonoBehaviour types. That meant any ScriptableObject type (including custom ones)
                    // would always fail to resolve, even after a successful compilation / domain reload.
                    //
                    // Here we use a dedicated resolver that searches for ScriptableObject-derived types instead.
                    string resolveError;
                    Type scriptType = ResolveScriptableObjectType(scriptClassName, out resolveError);
                    if (scriptType == null)
                    {
                        var reason = string.IsNullOrEmpty(resolveError)
                            ? "Type not found."
                            : resolveError;
                        return Response.Error(
                            $"Script class '{scriptClassName}' invalid: {reason}"
                        );
                    }

                    ScriptableObject so = ScriptableObject.CreateInstance(scriptType);
                    // TODO: Apply properties from JObject to the ScriptableObject instance?
                    AssetDatabase.CreateAsset(so, fullPath);
                    newAsset = so;
                }
                else if (lowerAssetType == "prefab")
                {
                    // Creating prefabs usually involves saving an existing GameObject hierarchy.
                    // A common pattern is to create an empty GameObject, configure it, and then save it.
                    return Response.Error(
                        "Creating prefabs programmatically usually requires a source GameObject. Use manage_gameobject to create/configure, then save as prefab via a separate mechanism or future enhancement."
                    );
                }
                // TODO: Add more asset types (Animation Controller, Scene, etc.)
                else
                {
                    return Response.Error(
                        $"Creation for asset type '{assetType}' is not explicitly supported yet. Supported: Folder, Material, PhysicsMaterial, ScriptableObject."
                    );
                }

                if (
                    newAsset == null
                    && !Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), fullPath))
                ) // Check if it wasn't a folder and asset wasn't created
                {
                    return Response.Error(
                        $"Failed to create asset '{assetType}' at '{fullPath}'. See logs for details."
                    );
                }

                AssetDatabase.SaveAssets();
                // AssetDatabase.Refresh(); // CreateAsset often handles refresh
                return Response.Success(
                    $"Asset '{fullPath}' created successfully.",
                    GetAssetData(fullPath)
                );
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to create asset at '{fullPath}': {e.Message}");
            }
        }

        private static object CreateFolder(string path)
        {
            if (string.IsNullOrEmpty(path))
                return Response.Error("'path' is required for create_folder.");
            string fullPath = SanitizeAssetPath(path);
            string parentDir = Path.GetDirectoryName(fullPath);
            string folderName = Path.GetFileName(fullPath);

            if (AssetExists(fullPath))
            {
                // Check if it's actually a folder already
                if (AssetDatabase.IsValidFolder(fullPath))
                {
                    return Response.Success(
                        $"Folder already exists at path: {fullPath}",
                        GetAssetData(fullPath)
                    );
                }
                else
                {
                    return Response.Error(
                        $"An asset (not a folder) already exists at path: {fullPath}"
                    );
                }
            }

            try
            {
                EnsureDirectoryExists(parentDir);

                string guid = AssetDatabase.CreateFolder(parentDir, folderName);
                if (string.IsNullOrEmpty(guid))
                {
                    return Response.Error(
                        $"Failed to create folder '{fullPath}'. Check logs and permissions."
                    );
                }

                // AssetDatabase.Refresh(); // CreateFolder usually handles refresh
                return Response.Success(
                    $"Folder '{fullPath}' created successfully.",
                    GetAssetData(fullPath)
                );
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to create folder '{fullPath}': {e.Message}");
            }
        }

        /// <summary>
        /// Resolve a ScriptableObject type by short or fully-qualified name.
        /// Searches loaded assemblies and ensures the type derives from ScriptableObject.
        /// Does NOT rely on ComponentResolver (which is Component/MonoBehaviour-specific).
        /// </summary>
        private static Type ResolveScriptableObjectType(string nameOrFullName, out string error)
        {
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(nameOrFullName))
            {
                error = "scriptClass cannot be null or empty.";
                return null;
            }

            // 1) Direct Type.GetType lookup (works for fully-qualified names with assembly, or some common cases)
            Type type = Type.GetType(nameOrFullName, throwOnError: false);
            if (IsValidScriptableObject(type))
            {
                return type;
            }

            // 2) Search all loaded assemblies, preferring Player (runtime) assemblies when available
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

#if UNITY_EDITOR
            var playerAsmNames = new HashSet<string>(
                UnityEditor.Compilation.CompilationPipeline
                    .GetAssemblies(UnityEditor.Compilation.AssembliesType.Player)
                    .Select(a => a.name),
                StringComparer.Ordinal
            );

            IEnumerable<System.Reflection.Assembly> playerAsms =
                loadedAssemblies.Where(a => playerAsmNames.Contains(a.GetName().Name));
            IEnumerable<System.Reflection.Assembly> editorAsms =
                loadedAssemblies.Except(playerAsms);
#else
            IEnumerable<System.Reflection.Assembly> playerAsms = loadedAssemblies;
            IEnumerable<System.Reflection.Assembly> editorAsms =
                Array.Empty<System.Reflection.Assembly>();
#endif

            IEnumerable<Type> SafeGetTypes(System.Reflection.Assembly a)
            {
                try
                {
                    return a.GetTypes();
                }
                catch (System.Reflection.ReflectionTypeLoadException rtle)
                {
                    return rtle.Types.Where(t => t != null);
                }
            }

            bool isShortName = !nameOrFullName.Contains(".");
            Func<Type, bool> match;
            if (isShortName)
                match = t => t.Name.Equals(nameOrFullName, StringComparison.Ordinal);
            else
                match = t => t.FullName != null
                          && t.FullName.Equals(nameOrFullName, StringComparison.Ordinal);

            var fromPlayer = playerAsms
                .SelectMany(SafeGetTypes)
                .Where(IsValidScriptableObject)
                .Where(match);

            var fromEditor = editorAsms
                .SelectMany(SafeGetTypes)
                .Where(IsValidScriptableObject)
                .Where(match);

            var candidates = new List<Type>(fromPlayer);
            if (candidates.Count == 0)
            {
                candidates.AddRange(fromEditor);
            }

            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            if (candidates.Count > 1)
            {
                var lines = candidates.Select(
                    t => $"{t.FullName} (assembly {t.Assembly.GetName().Name})"
                );
                error =
                    $"Multiple ScriptableObject types matched '{nameOrFullName}':\n - "
                    + string.Join("\n - ", lines)
                    + "\nProvide a fully qualified type name (Namespace.TypeName) to disambiguate.";
                return null;
            }

            error =
                $"ScriptableObject type '{nameOrFullName}' not found in loaded assemblies. "
                + "Use a fully-qualified name (Namespace.TypeName) and ensure the script compiled.";
            return null;
        }

        private static bool IsValidScriptableObject(Type t) =>
            t != null && typeof(ScriptableObject).IsAssignableFrom(t);

        private static object ModifyAsset(string path, JObject properties)
        {
            if (string.IsNullOrEmpty(path))
                return Response.Error("'path' is required for modify.");
            if (properties == null || !properties.HasValues)
                return Response.Error("'properties' are required for modify.");

            string fullPath = SanitizeAssetPath(path);
            bool ghostDesync;
            if (!AssetExists(fullPath, out ghostDesync))
                return BuildAssetNotFoundResponse($"Asset not found at path: {fullPath}", fullPath, ghostDesync);

            try
            {
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(
                    fullPath
                );
                if (asset == null)
                    return Response.Error($"Failed to load asset at path: {fullPath}");

                bool modified = false; // Flag to track if any changes were made

                // --- NEW: Handle GameObject / Prefab Component Modification ---
                if (asset is GameObject gameObject)
                {
                    // Iterate through the properties JSON: keys are component names, values are properties objects for that component
                    foreach (var prop in properties.Properties())
                    {
                        string componentName = prop.Name; // e.g., "Collectible"
                        // Check if the value associated with the component name is actually an object containing properties
                        if (
                            prop.Value is JObject componentProperties
                            && componentProperties.HasValues
                        ) // e.g., {"bobSpeed": 2.0}
                        {
                            // Resolve component type via ComponentResolver, then fetch by Type
                            Component targetComponent = null;
                            bool resolved = ComponentResolver.TryResolve(componentName, out var compType, out var compError);
                            if (resolved)
                            {
                                targetComponent = gameObject.GetComponent(compType);
                            }
                            
                            // Only warn about resolution failure if component also not found
                            if (targetComponent == null && !resolved)
                            {
                                CodelyLogger.LogWarning(
                                    $"[ManageAsset.ModifyAsset] Failed to resolve component '{componentName}' on '{gameObject.name}': {compError}"
                                );
                            }

                            if (targetComponent != null)
                            {
                                // Apply the nested properties (e.g., bobSpeed) to the found component instance
                                // Use |= to ensure 'modified' becomes true if any component is successfully modified
                                modified |= ApplyObjectProperties(
                                    targetComponent,
                                    componentProperties
                                );
                            }
                            else
                            {
                                // Log a warning if a specified component couldn't be found
                                CodelyLogger.LogWarning(
                                    $"[ManageAsset.ModifyAsset] Component '{componentName}' not found on GameObject '{gameObject.name}' in asset '{fullPath}'. Skipping modification for this component."
                                );
                            }
                        }
                        else
                        {
                            // Log a warning if the structure isn't {"ComponentName": {"prop": value}}
                            // We could potentially try to apply this property directly to the GameObject here if needed,
                            // but the primary goal is component modification.
                            CodelyLogger.LogWarning(
                                $"[ManageAsset.ModifyAsset] Property '{prop.Name}' for GameObject modification should have a JSON object value containing component properties. Value was: {prop.Value.Type}. Skipping."
                            );
                        }
                    }
                    // Note: 'modified' is now true if ANY component property was successfully changed.
                }
                // --- End NEW ---

                // --- Existing logic for other asset types (now as else-if) ---
                // Example: Modifying a Material
                else if (asset is Material material)
                {
                    // Apply properties directly to the material. If this modifies, it sets modified=true.
                    // Use |= in case the asset was already marked modified by previous logic (though unlikely here)
                    modified |= ApplyMaterialProperties(material, properties);
                }
                // Example: Modifying a ScriptableObject
                else if (asset is ScriptableObject so)
                {
                    // Apply properties directly to the ScriptableObject.
                    modified |= ApplyObjectProperties(so, properties); // General helper
                }
                // Example: Modifying TextureImporter settings
                else if (asset is Texture)
                {
                    AssetImporter importer = AssetImporter.GetAtPath(fullPath);
                    if (importer is TextureImporter textureImporter)
                    {
                        bool importerModified = ApplyObjectProperties(textureImporter, properties);
                        if (importerModified)
                        {
                            // Importer settings need saving and reimporting
                            AssetDatabase.WriteImportSettingsIfDirty(fullPath);
                            AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate); // Reimport to apply changes
                            modified = true; // Mark overall operation as modified
                        }
                    }
                    else
                    {
                        CodelyLogger.LogWarning($"Could not get TextureImporter for {fullPath}.");
                    }
                }
                // TODO: Add modification logic for other common asset types (Models, AudioClips importers, etc.)
                else // Fallback for other asset types OR direct properties on non-GameObject assets
                {
                    // This block handles non-GameObject/Material/ScriptableObject/Texture assets.
                    // Attempts to apply properties directly to the asset itself.
                    CodelyLogger.LogWarning(
                        $"[ManageAsset.ModifyAsset] Asset type '{asset.GetType().Name}' at '{fullPath}' is not explicitly handled for component modification. Attempting generic property setting on the asset itself."
                    );
                    modified |= ApplyObjectProperties(asset, properties);
                }
                // --- End Existing Logic ---

                // Check if any modification happened (either component or direct asset modification)
                if (modified)
                {
                    // Mark the asset as dirty (important for prefabs/SOs) so Unity knows to save it.
                    EditorUtility.SetDirty(asset);
                    if (asset is GameObject prefabRoot)
                    {
                        PrefabUtility.SavePrefabAsset(prefabRoot);
                    }
                    // Save all modified assets to disk.
                    AssetDatabase.SaveAssets();
                    // Refresh might be needed in some edge cases, but SaveAssets usually covers it.
                    // AssetDatabase.Refresh();
                    return Response.Success(
                        $"Asset '{fullPath}' modified successfully.",
                        GetAssetData(fullPath)
                    );
                }
                else
                {
                    // If no changes were made (e.g., component not found, property names incorrect, value unchanged), return a success message indicating nothing changed.
                    return Response.Success(
                        $"No applicable or modifiable properties found for asset '{fullPath}'. Check component names, property names, and values.",
                        GetAssetData(fullPath)
                    );
                    // Previous message: return Response.Success($"No applicable properties found to modify for asset '{fullPath}'.", GetAssetData(fullPath));
                }
            }
            catch (Exception e)
            {
                // Log the detailed error internally
                CodelyLogger.LogError($"[ManageAsset] Action 'modify' failed for path '{path}': {e}");
                // Return a user-friendly error message
                return Response.Error($"Failed to modify asset '{fullPath}': {e.Message}");
            }
        }

        private static object DeleteAsset(string path)
        {
            if (string.IsNullOrEmpty(path))
                return Response.Error("'path' is required for delete.");
            string fullPath = SanitizeAssetPath(path);
            bool ghostDesync;
            if (!AssetExists(fullPath, out ghostDesync))
                return BuildAssetNotFoundResponse($"Asset not found at path: {fullPath}", fullPath, ghostDesync);

            try
            {
                bool success = AssetDatabase.DeleteAsset(fullPath);
                if (success)
                {
                    // AssetDatabase.Refresh(); // DeleteAsset usually handles refresh
                    return Response.Success($"Asset '{fullPath}' deleted successfully.");
                }
                else
                {
                    // This might happen if the file couldn't be deleted (e.g., locked)
                    return Response.Error(
                        $"Failed to delete asset '{fullPath}'. Check logs or if the file is locked."
                    );
                }
            }
            catch (Exception e)
            {
                return Response.Error($"Error deleting asset '{fullPath}': {e.Message}");
            }
        }

        private static object DuplicateAsset(string path, string destinationPath)
        {
            if (string.IsNullOrEmpty(path))
                return Response.Error("'path' is required for duplicate.");

            string sourcePath = SanitizeAssetPath(path);
            bool ghostDesync;
            if (!AssetExists(sourcePath, out ghostDesync))
                return BuildAssetNotFoundResponse($"Source asset not found at path: {sourcePath}", sourcePath, ghostDesync);

            string destPath;
            if (string.IsNullOrEmpty(destinationPath))
            {
                // Generate a unique path if destination is not provided
                destPath = AssetDatabase.GenerateUniqueAssetPath(sourcePath);
            }
            else
            {
                destPath = SanitizeAssetPath(destinationPath);
                if (AssetExists(destPath))
                    return Response.Error($"Asset already exists at destination path: {destPath}");
                // Ensure destination directory exists
                EnsureDirectoryExists(Path.GetDirectoryName(destPath));
            }

            try
            {
                bool success = AssetDatabase.CopyAsset(sourcePath, destPath);
                if (success)
                {
                    // AssetDatabase.Refresh();
                    return Response.Success(
                        $"Asset '{sourcePath}' duplicated to '{destPath}'.",
                        GetAssetData(destPath)
                    );
                }
                else
                {
                    return Response.Error(
                        $"Failed to duplicate asset from '{sourcePath}' to '{destPath}'."
                    );
                }
            }
            catch (Exception e)
            {
                return Response.Error($"Error duplicating asset '{sourcePath}': {e.Message}");
            }
        }

        private static object MoveOrRenameAsset(string path, string destinationPath)
        {
            if (string.IsNullOrEmpty(path))
                return Response.Error("'path' is required for move/rename.");
            if (string.IsNullOrEmpty(destinationPath))
                return Response.Error("'destination' path is required for move/rename.");

            string sourcePath = SanitizeAssetPath(path);
            string destPath = SanitizeAssetPath(destinationPath);

            bool ghostDesync;
            if (!AssetExists(sourcePath, out ghostDesync))
                return BuildAssetNotFoundResponse($"Source asset not found at path: {sourcePath}", sourcePath, ghostDesync);
            if (AssetExists(destPath))
                return Response.Error(
                    $"An asset already exists at the destination path: {destPath}"
                );

            // Ensure destination directory exists
            EnsureDirectoryExists(Path.GetDirectoryName(destPath));

            try
            {
                // Validate will return an error string if failed, empty string if successful
                string validateError = AssetDatabase.ValidateMoveAsset(sourcePath, destPath);
                if (!string.IsNullOrEmpty(validateError))
                {
                    return Response.Error(
                        $"Failed to move/rename asset from '{sourcePath}' to '{destPath}': {validateError}"
                    );
                }

                // MoveAsset returns an empty string on success, or an error message on failure
                string moveError = AssetDatabase.MoveAsset(sourcePath, destPath);
                if (string.IsNullOrEmpty(moveError))
                {
                    // AssetDatabase.Refresh(); // MoveAsset usually handles refresh
                    return Response.Success(
                        $"Asset moved/renamed from '{sourcePath}' to '{destPath}'.",
                        GetAssetData(destPath)
                    );
                }
                else
                {
                    return Response.Error(
                        $"Failed to move/rename asset from '{sourcePath}' to '{destPath}': {moveError}"
                    );
                }
            }
            catch (Exception e)
            {
                return Response.Error($"Error moving/renaming asset '{sourcePath}': {e.Message}");
            }
        }

        private static object SearchAssets(JObject @params)
        {
            string searchPattern = @params["searchPattern"]?.ToString();
            string filterType = @params["filterType"]?.ToString();
            string pathScope = @params["path"]?.ToString(); // Use path as folder scope
            string filterDateAfterStr = @params["filterDateAfter"]?.ToString();
            int pageSize = @params["pageSize"]?.ToObject<int?>() ?? 50; // Default page size
            int pageNumber = @params["pageNumber"]?.ToObject<int?>() ?? 1; // Default page number (1-based)
            bool generatePreview = @params["generatePreview"]?.ToObject<bool>() ?? false;

            List<string> searchFilters = new List<string>();
            if (!string.IsNullOrEmpty(searchPattern))
                searchFilters.Add(searchPattern);
            if (!string.IsNullOrEmpty(filterType))
                searchFilters.Add($"t:{filterType}");

            string[] folderScope = null;
            if (!string.IsNullOrEmpty(pathScope))
            {
                folderScope = new string[] { SanitizeAssetPath(pathScope) };
                if (!AssetDatabase.IsValidFolder(folderScope[0]))
                {
                    return Response.Error($"Search path '{folderScope[0]}' is not a valid folder.");
                }
            }

            DateTime? filterDateAfter = null;
            if (!string.IsNullOrEmpty(filterDateAfterStr))
            {
                if (
                    DateTime.TryParse(
                        filterDateAfterStr,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out DateTime parsedDate
                    )
                )
                {
                    filterDateAfter = parsedDate;
                }
                else
                {
                    CodelyLogger.LogWarning(
                        $"Could not parse filterDateAfter: '{filterDateAfterStr}'. Expected ISO 8601 format."
                    );
                }
            }

            try
            {
                string[] guids = AssetDatabase.FindAssets(
                    string.Join(" ", searchFilters),
                    folderScope
                );

                // Collect matching paths first (no GetAssetData yet) so pagination
                // is applied before any expensive preview rendering.
                List<string> matchedPaths = new List<string>();
                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(assetPath))
                        continue;

                    if (filterDateAfter.HasValue)
                    {
                        DateTime lastWriteTime = File.GetLastWriteTimeUtc(
                            Path.Combine(Directory.GetCurrentDirectory(), assetPath)
                        );
                        if (lastWriteTime <= filterDateAfter.Value)
                            continue;
                    }

                    matchedPaths.Add(assetPath);
                }

                int totalFound = matchedPaths.Count;
                int startIndex = (pageNumber - 1) * pageSize;
                var pagedPaths = matchedPaths.Skip(startIndex).Take(pageSize).ToList();
                var pagedResults = pagedPaths
                    .Select(p => GetAssetData(p, generatePreview))
                    .ToList<object>();

                return Response.Success(
                    $"Found {totalFound} asset(s). Returning page {pageNumber} ({pagedResults.Count} assets).",
                    new
                    {
                        totalAssets = totalFound,
                        pageSize = pageSize,
                        pageNumber = pageNumber,
                        assets = pagedResults,
                    }
                );
            }
            catch (Exception e)
            {
                return Response.Error($"Error searching assets: {e.Message}");
            }
        }

        private static object GetAssetInfo(string path, bool generatePreview)
        {
            if (string.IsNullOrEmpty(path))
                return Response.Error("'path' is required for get_info.");
            string fullPath = SanitizeAssetPath(path);
            bool ghostDesync;
            if (!AssetExists(fullPath, out ghostDesync))
                return BuildAssetNotFoundResponse($"Asset not found at path: {fullPath}", fullPath, ghostDesync);

            try
            {
                return Response.Success(
                    "Asset info retrieved.",
                    GetAssetData(fullPath, generatePreview)
                );
            }
            catch (Exception e)
            {
                return Response.Error($"Error getting info for asset '{fullPath}': {e.Message}");
            }
        }

        /// <summary>
        /// Retrieves components attached to a GameObject asset (like a Prefab).
        /// </summary>
        /// <param name="path">The asset path of the GameObject or Prefab.</param>
        /// <returns>A response object containing a list of component type names or an error.</returns>
        private static object GetComponentsFromAsset(string path)
        {
            // 1. Validate input path
            if (string.IsNullOrEmpty(path))
                return Response.Error("'path' is required for get_components.");

            // 2. Sanitize and check existence
            string fullPath = SanitizeAssetPath(path);
            bool ghostDesync;
            if (!AssetExists(fullPath, out ghostDesync))
                return BuildAssetNotFoundResponse($"Asset not found at path: {fullPath}", fullPath, ghostDesync);

            try
            {
                // 3. Load the asset
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(
                    fullPath
                );
                if (asset == null)
                    return Response.Error($"Failed to load asset at path: {fullPath}");

                // 4. Check if it's a GameObject (Prefabs load as GameObjects)
                GameObject gameObject = asset as GameObject;
                if (gameObject == null)
                {
                    // Also check if it's *directly* a Component type (less common for primary assets)
                    Component componentAsset = asset as Component;
                    if (componentAsset != null)
                    {
                        // If the asset itself *is* a component, maybe return just its info?
                        // This is an edge case. Let's stick to GameObjects for now.
                        return Response.Error(
                            $"Asset at '{fullPath}' is a Component ({asset.GetType().FullName}), not a GameObject. Components are typically retrieved *from* a GameObject."
                        );
                    }
                    return Response.Error(
                        $"Asset at '{fullPath}' is not a GameObject (Type: {asset.GetType().FullName}). Cannot get components from this asset type."
                    );
                }

                // 5. Get components
                Component[] components = gameObject.GetComponents<Component>();

                // 6. Format component data
                List<object> componentList = components
                    .Select(comp => new
                    {
                        typeName = comp.GetType().FullName,
                        instanceID = comp.GetStableInstanceId(),
                        // TODO: Add more component-specific details here if needed in the future?
                        //       Requires reflection or specific handling per component type.
                    })
                    .ToList<object>(); // Explicit cast for clarity if needed

                // 7. Return success response
                return Response.Success(
                    $"Found {componentList.Count} component(s) on asset '{fullPath}'.",
                    componentList
                );
            }
            catch (Exception e)
            {
                CodelyLogger.LogError(
                    $"[ManageAsset.GetComponentsFromAsset] Error getting components for '{fullPath}': {e}"
                );
                return Response.Error(
                    $"Error getting components for asset '{fullPath}': {e.Message}"
                );
            }
        }

        // --- Internal Helpers ---

        /// <summary>
        /// Ensures the asset path starts with "Assets/".
        /// </summary>
        private static string SanitizeAssetPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            path = path.Replace('\\', '/'); // Normalize separators
            if (!path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                return "Assets/" + path.TrimStart('/');
            }
            return path;
        }

        /// <summary>
        /// Checks if an asset exists at the given path (file or folder).
        /// 
        /// NOTE:
        /// We intentionally require a *real* backing asset on disk for non-folder assets.
        /// Relying solely on AssetDatabase.AssetPathToGUID can report "ghost" assets
        /// where a GUID/meta still exists in Unity's database but the actual asset file
        /// has been deleted.
        /// 
        /// If we detect a GUID in AssetDatabase but the corresponding file is missing
        /// on disk, we trigger a one-off AssetDatabase.Refresh() to give Unity a chance
        /// to heal the desync before returning "not found".
        /// </summary>
        private static bool AssetExists(string path, out bool ghostDesyncDetected)
        {
            ghostDesyncDetected = false;

            if (string.IsNullOrEmpty(path))
                return false;

            // Normalise path (adds "Assets/" prefix if missing, normalises slashes)
            string sanitizedPath = SanitizeAssetPath(path);
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), sanitizedPath);

            // --- Folder case: require BOTH AssetDatabase and filesystem directory ---
            bool isFolder = AssetDatabase.IsValidFolder(sanitizedPath);
            bool dirExists = Directory.Exists(fullPath);

            if (isFolder)
            {
                if (!dirExists)
                {
                    // Ghost folder: AssetDatabase thinks folder exists but the directory is gone.
                    ghostDesyncDetected = true;

                    CodelyLogger.LogWarning(
                        $"[ManageAsset.AssetExists] Detected valid folder '{sanitizedPath}' in AssetDatabase " +
                        $"but no directory found at '{fullPath}'. Triggering AssetDatabase.Refresh() to resync."
                    );

                    AssetDatabase.Refresh();

                    // Re-evaluate after refresh
                    isFolder = AssetDatabase.IsValidFolder(sanitizedPath);
                    dirExists = Directory.Exists(fullPath);
                }

                // Only treat as existing if both ADB 和 FS 都确认存在
                return isFolder && dirExists;
            }

            // --- Non-folder assets: look at both AssetDatabase (GUID) and filesystem file ---
            string guid = AssetDatabase.AssetPathToGUID(sanitizedPath);
            bool fileExists = File.Exists(fullPath);

            // Ghost case: AssetDatabase still has a GUID for this path, but the backing file
            // is gone from disk. Trigger a refresh once to let Unity heal its cache.
            if (!fileExists && !string.IsNullOrEmpty(guid))
            {
                ghostDesyncDetected = true;

                CodelyLogger.LogWarning(
                    $"[ManageAsset.AssetExists] Detected GUID '{guid}' for '{sanitizedPath}' in AssetDatabase " +
                    $"but no asset file found at '{fullPath}'. Triggering AssetDatabase.Refresh() to resync."
                );

                AssetDatabase.Refresh();

                // Re-evaluate after refresh
                guid = AssetDatabase.AssetPathToGUID(sanitizedPath);
                fileExists = File.Exists(fullPath);
            }

            // Non-folder assets: require that the main asset file exists on disk
            // *and* that AssetDatabase knows about it (has a GUID). This prevents
            // "ghost" assets that only have a stale GUID/meta entry.
            if (!fileExists)
            {
                return false;
            }

            return !string.IsNullOrEmpty(guid);
        }

        /// <summary>
        /// Convenience overload when ghost-desync information is not needed.
        /// </summary>
        private static bool AssetExists(string path)
        {
            return AssetExists(path, out _);
        }

        /// <summary>
        /// Creates a standardized "asset not found" error with an extra hint for LLMs
        /// about potential AssetDatabase / filesystem desync.
        /// </summary>
        private static object AssetNotFoundError(string message, string path)
        {
            return Response.Error(
                message,
                new
                {
                    path = path,
                    llm_hint =
                        "The requested asset could not be found on disk. If this asset should exist (for example it was " +
                        "recently renamed, moved, or deleted outside the Unity Editor), Unity's AssetDatabase may be out of " +
                        "sync with the filesystem. Ask the user to refresh the AssetDatabase in the Unity Editor (for example " +
                        "via 'Assets → Reimport All' or by reopening the project) and then retry this tool call."
                }
            );
        }

        /// <summary>
        /// Builds an "asset not found" response, only upgrading to AssetNotFoundError (with
        /// LLM hint about AssetDatabase desync) when we have actually detected a ghost asset
        /// scenario (GUID present in AssetDatabase but file missing on disk).
        /// </summary>
        private static object BuildAssetNotFoundResponse(string message, string path, bool ghostDesyncDetected)
        {
            if (ghostDesyncDetected)
            {
                // Ghost asset case: surface the richer error with LLM hint.
                return AssetNotFoundError(message, path);
            }

            // Normal "not found" case (e.g., bad path, never existed): keep error simple.
            return Response.Error(
                message,
                new
                {
                    path = path
                }
            );
        }

        /// <summary>
        /// Recursively ensures an Assets/ folder chain exists via AssetDatabase.CreateFolder.
        /// </summary>
        private static void EnsureDirectoryExists(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath))
                return;

            string folderPath = directoryPath.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(folderPath))
                return;

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string name = Path.GetFileName(folderPath);
            if (string.IsNullOrEmpty(name))
                return;

            EnsureDirectoryExists(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        /// <summary>
        /// Applies properties from JObject to a Material.
        /// </summary>
        private static bool ApplyMaterialProperties(Material mat, JObject properties)
        {
            if (mat == null || properties == null)
                return false;
            bool modified = false;

            // Example: Set shader
            if (properties["shader"]?.Type == JTokenType.String)
            {
                Shader newShader = Shader.Find(properties["shader"].ToString());
                if (newShader != null && mat.shader != newShader)
                {
                    mat.shader = newShader;
                    modified = true;
                }
            }
            // Example: Set color property
            if (properties["color"] is JObject colorProps)
            {
                string propName = colorProps["name"]?.ToString() ?? "_Color"; // Default main color
                if (colorProps["value"] is JArray colArr && colArr.Count >= 3)
                {
                    try
                    {
                        Color newColor = new Color(
                            colArr[0].ToObject<float>(),
                            colArr[1].ToObject<float>(),
                            colArr[2].ToObject<float>(),
                            colArr.Count > 3 ? colArr[3].ToObject<float>() : 1.0f
                        );
                        if (mat.HasProperty(propName) && mat.GetColor(propName) != newColor)
                        {
                            mat.SetColor(propName, newColor);
                            modified = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        CodelyLogger.LogWarning(
                            $"Error parsing color property '{propName}': {ex.Message}"
                        );
                    }
                }
            } else if (properties["color"] is JArray colorArr)
            {
                // Array form: {"color":[r,g,b,a]}. Try _BaseColor (URP/HDRP) first, fall back to _Color (Standard).
                try {
                    if (colorArr.Count >= 3)
                    {
                        Color newColor = new Color(
                            colorArr[0].ToObject<float>(),
                            colorArr[1].ToObject<float>(), 
                            colorArr[2].ToObject<float>(), 
                            colorArr.Count > 3 ? colorArr[3].ToObject<float>() : 1.0f
                        );
                        string[] colorPropNames = { "_BaseColor", "_Color" };
                        foreach (var propName in colorPropNames)
                        {
                            if (mat.HasProperty(propName))
                            {
                                if (mat.GetColor(propName) != newColor)
                                {
                                    mat.SetColor(propName, newColor);
                                    modified = true;
                                }
                                break;
                            }
                        }
                    }
                } 
                catch (Exception ex) {
                    CodelyLogger.LogWarning(
                        $"Error parsing color array property: {ex.Message}"
                    );
                }
            }
            // Example: Set float property
            if (properties["float"] is JObject floatProps)
            {
                string propName = floatProps["name"]?.ToString();
                if (
                    !string.IsNullOrEmpty(propName) &&
                    (floatProps["value"]?.Type == JTokenType.Float || floatProps["value"]?.Type == JTokenType.Integer)
                )
                {
                    try
                    {
                        float newVal = floatProps["value"].ToObject<float>();
                        if (mat.HasProperty(propName) && mat.GetFloat(propName) != newVal)
                        {
                            mat.SetFloat(propName, newVal);
                            modified = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        CodelyLogger.LogWarning(
                            $"Error parsing float property '{propName}': {ex.Message}"
                        );
                    }
                }
            }
            // Example: Set texture property
            if (properties["texture"] is JObject texProps)
            {
                string propName = texProps["name"]?.ToString() ?? "_MainTex"; // Default main texture
                string texPath = texProps["path"]?.ToString();
                if (!string.IsNullOrEmpty(texPath))
                {
                    Texture newTex = AssetDatabase.LoadAssetAtPath<Texture>(
                        SanitizeAssetPath(texPath)
                    );
                    if (
                        newTex != null
                        && mat.HasProperty(propName)
                        && mat.GetTexture(propName) != newTex
                    )
                    {
                        mat.SetTexture(propName, newTex);
                        modified = true;
                    }
                    else if (newTex == null)
                    {
                        CodelyLogger.LogWarning($"Texture not found at path: {texPath}");
                    }
                }
            }

            // Handle common Standard/URP shader properties directly by name
            // metallic -> _Metallic
            if (properties["metallic"]?.Type == JTokenType.Float || properties["metallic"]?.Type == JTokenType.Integer)
            {
                try
                {
                    float newVal = properties["metallic"].ToObject<float>();
                    string propName = "_Metallic";
                    if (mat.HasProperty(propName) && mat.GetFloat(propName) != newVal)
                    {
                        mat.SetFloat(propName, newVal);
                        modified = true;
                    }
                }
                catch (Exception ex)
                {
                    CodelyLogger.LogWarning($"Error parsing metallic property: {ex.Message}");
                }
            }
            // smoothness -> _Smoothness or _Glossiness (Standard shader uses _Glossiness)
            if (properties["smoothness"]?.Type == JTokenType.Float || properties["smoothness"]?.Type == JTokenType.Integer)
            {
                try
                {
                    float newVal = properties["smoothness"].ToObject<float>();
                    // Try both property names
                    string[] propNames = { "_Smoothness", "_Glossiness" };
                    foreach (var propName in propNames)
                    {
                        if (mat.HasProperty(propName) && mat.GetFloat(propName) != newVal)
                        {
                            mat.SetFloat(propName, newVal);
                            modified = true;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    CodelyLogger.LogWarning($"Error parsing smoothness property: {ex.Message}");
                }
            }

            // TODO: Add handlers for other property types (Vectors, Ints, Keywords, RenderQueue, etc.)
            return modified;
        }

        /// <summary>
        ///  Applies properties from JObject to a PhysicsMaterial.
        /// </summary>
        private static bool ApplyPhysicsMaterialProperties(PhysicsMaterialType pmat, JObject properties)
        {
            if (pmat == null || properties == null)
                return false;
            bool modified = false;

            // Helper to check if a token is a number (Float or Integer)
            bool IsNumber(JToken token) => token?.Type == JTokenType.Float || token?.Type == JTokenType.Integer;

            // Set dynamic friction
            if (IsNumber(properties["dynamicFriction"]))
            {
                float dynamicFriction = properties["dynamicFriction"].ToObject<float>();
                pmat.dynamicFriction = dynamicFriction;
                modified = true;
            }

            // Set static friction
            if (IsNumber(properties["staticFriction"]))
            {
                float staticFriction = properties["staticFriction"].ToObject<float>();
                pmat.staticFriction = staticFriction;
                modified = true;
            }

            // Set bounciness
            if (IsNumber(properties["bounciness"]))
            {
                float bounciness = properties["bounciness"].ToObject<float>();
                pmat.bounciness = bounciness;
                modified = true;
            }

            List<String> averageList = new List<String> { "ave", "Ave", "average", "Average" };
            List<String> multiplyList = new List<String> { "mul", "Mul", "mult", "Mult", "multiply", "Multiply" };
            List<String> minimumList = new List<String> { "min", "Min", "minimum", "Minimum" };
            List<String> maximumList = new List<String> { "max", "Max", "maximum", "Maximum" };

            // Example: Set friction combine
            if (properties["frictionCombine"]?.Type == JTokenType.String)
            {
                string frictionCombine = properties["frictionCombine"].ToString();
                if (averageList.Contains(frictionCombine))
                    { pmat.frictionCombine = PhysicsMaterialCombine.Average; modified = true; }
                else if (multiplyList.Contains(frictionCombine))
                    { pmat.frictionCombine = PhysicsMaterialCombine.Multiply; modified = true; }
                else if (minimumList.Contains(frictionCombine))
                    { pmat.frictionCombine = PhysicsMaterialCombine.Minimum; modified = true; }
                else if (maximumList.Contains(frictionCombine))
                    { pmat.frictionCombine = PhysicsMaterialCombine.Maximum; modified = true; }
                else
                    CodelyLogger.LogWarning(
                        $"[ApplyPhysicsMaterialProperties] Unknown frictionCombine value: '{frictionCombine}'. " +
                        "Expected: Average, Multiply, Minimum, Maximum."
                    );
            }

            // Example: Set bounce combine
            if (properties["bounceCombine"]?.Type == JTokenType.String)
            {
                string bounceCombine = properties["bounceCombine"].ToString();
                if (averageList.Contains(bounceCombine))
                    { pmat.bounceCombine = PhysicsMaterialCombine.Average; modified = true; }
                else if (multiplyList.Contains(bounceCombine))
                    { pmat.bounceCombine = PhysicsMaterialCombine.Multiply; modified = true; }
                else if (minimumList.Contains(bounceCombine))
                    { pmat.bounceCombine = PhysicsMaterialCombine.Minimum; modified = true; }
                else if (maximumList.Contains(bounceCombine))
                    { pmat.bounceCombine = PhysicsMaterialCombine.Maximum; modified = true; }
                else
                    CodelyLogger.LogWarning(
                        $"[ApplyPhysicsMaterialProperties] Unknown bounceCombine value: '{bounceCombine}'. " +
                        "Expected: Average, Multiply, Minimum, Maximum."
                    );
            }

            return modified;
        }

        /// <summary>
        /// Generic helper to set properties on any UnityEngine.Object using reflection.
        /// </summary>
        private static bool ApplyObjectProperties(UnityEngine.Object target, JObject properties)
        {
            if (target == null || properties == null)
                return false;
            bool modified = false;
            Type type = target.GetType();

            foreach (var prop in properties.Properties())
            {
                string propName = prop.Name;
                JToken propValue = prop.Value;
                if (SetPropertyOrField(target, propName, propValue, type))
                {
                    modified = true;
                }
            }
            return modified;
        }

        /// <summary>
        /// Helper to set a property or field via reflection, handling basic types and Unity objects.
        /// </summary>
        private static bool SetPropertyOrField(
            object target,
            string memberName,
            JToken value,
            Type type = null
        )
        {
            type = type ?? target.GetType();
            System.Reflection.BindingFlags flags =
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.IgnoreCase;

            try
            {
                System.Reflection.PropertyInfo propInfo = type.GetProperty(memberName, flags);
                if (propInfo != null && propInfo.CanWrite)
                {
                    object convertedValue = ConvertJTokenToType(value, propInfo.PropertyType);
                    if (
                        convertedValue != null
                        && !object.Equals(propInfo.GetValue(target), convertedValue)
                    )
                    {
                        propInfo.SetValue(target, convertedValue);
                        return true;
                    }
                }
                else
                {
                    System.Reflection.FieldInfo fieldInfo = type.GetField(memberName, flags);
                    if (fieldInfo != null)
                    {
                        object convertedValue = ConvertJTokenToType(value, fieldInfo.FieldType);
                        if (
                            convertedValue != null
                            && !object.Equals(fieldInfo.GetValue(target), convertedValue)
                        )
                        {
                            fieldInfo.SetValue(target, convertedValue);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CodelyLogger.LogWarning(
                    $"[SetPropertyOrField] Failed to set '{memberName}' on {type.Name}: {ex.Message}"
                );
            }
            return false;
        }

        /// <summary>
        /// Simple JToken to Type conversion for common Unity types and primitives.
        /// </summary>
        private static object ConvertJTokenToType(JToken token, Type targetType)
        {
            try
            {
                if (token == null || token.Type == JTokenType.Null)
                    return null;

                if (targetType == typeof(string))
                    return token.ToObject<string>();
                if (targetType == typeof(int))
                    return token.ToObject<int>();
                if (targetType == typeof(float))
                    return token.ToObject<float>();
                if (targetType == typeof(bool))
                    return token.ToObject<bool>();
                if (targetType == typeof(Vector2) && token is JArray arrV2 && arrV2.Count == 2)
                    return new Vector2(arrV2[0].ToObject<float>(), arrV2[1].ToObject<float>());
                if (targetType == typeof(Vector3) && token is JArray arrV3 && arrV3.Count == 3)
                    return new Vector3(
                        arrV3[0].ToObject<float>(),
                        arrV3[1].ToObject<float>(),
                        arrV3[2].ToObject<float>()
                    );
                if (targetType == typeof(Vector4) && token is JArray arrV4 && arrV4.Count == 4)
                    return new Vector4(
                        arrV4[0].ToObject<float>(),
                        arrV4[1].ToObject<float>(),
                        arrV4[2].ToObject<float>(),
                        arrV4[3].ToObject<float>()
                    );
                if (targetType == typeof(Quaternion) && token is JArray arrQ && arrQ.Count == 4)
                    return new Quaternion(
                        arrQ[0].ToObject<float>(),
                        arrQ[1].ToObject<float>(),
                        arrQ[2].ToObject<float>(),
                        arrQ[3].ToObject<float>()
                    );
                if (targetType == typeof(Color) && token is JArray arrC && arrC.Count >= 3) // Allow RGB or RGBA
                    return new Color(
                        arrC[0].ToObject<float>(),
                        arrC[1].ToObject<float>(),
                        arrC[2].ToObject<float>(),
                        arrC.Count > 3 ? arrC[3].ToObject<float>() : 1.0f
                    );
                if (targetType.IsEnum)
                    return Enum.Parse(targetType, token.ToString(), true); // Case-insensitive enum parsing

                // Handle loading Unity Objects (Materials, Textures, etc.) by path
                if (
                    typeof(UnityEngine.Object).IsAssignableFrom(targetType)
                    && token.Type == JTokenType.String
                )
                {
                    string assetPath = SanitizeAssetPath(token.ToString());
                    UnityEngine.Object loadedAsset = AssetDatabase.LoadAssetAtPath(
                        assetPath,
                        targetType
                    );
                    if (loadedAsset == null)
                    {
                        CodelyLogger.LogWarning(
                            $"[ConvertJTokenToType] Could not load asset of type {targetType.Name} from path: {assetPath}"
                        );
                    }
                    return loadedAsset;
                }

                // Fallback: Try direct conversion (might work for other simple value types)
                return token.ToObject(targetType);
            }
            catch (Exception ex)
            {
                CodelyLogger.LogWarning(
                    $"[ConvertJTokenToType] Could not convert JToken '{token}' (type {token.Type}) to type '{targetType.Name}': {ex.Message}"
                );
                return null;
            }
        }


        // --- Data Serialization ---

        /// <summary>
        /// Creates a serializable representation of an asset.
        /// </summary>
        private static object GetAssetData(string path, bool generatePreview = false)
        {
            if (string.IsNullOrEmpty(path) || !AssetExists(path))
                return null;

            string guid = AssetDatabase.AssetPathToGUID(path);
            Type assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            string previewBase64 = null;
            int previewWidth = 0;
            int previewHeight = 0;

            if (generatePreview && asset != null)
            {
                Texture2D preview = AssetPreview.GetAssetPreview(asset);

                if (preview != null)
                {
                    try
                    {
                        // Ensure texture is readable for EncodeToPNG
                        // Creating a temporary readable copy is safer
                        RenderTexture rt = null;
                        Texture2D readablePreview = null;
                        RenderTexture previous = RenderTexture.active;
                        try
                        {
                            rt = RenderTexture.GetTemporary(preview.width, preview.height);
                            Graphics.Blit(preview, rt);
                            RenderTexture.active = rt;
                            readablePreview = new Texture2D(preview.width, preview.height, TextureFormat.RGB24, false);
                            readablePreview.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                            readablePreview.Apply();

                            var pngData = readablePreview.EncodeToPNG();
                            if (pngData != null && pngData.Length > 0)
                            {
                                previewBase64 = Convert.ToBase64String(pngData);
                                previewWidth = readablePreview.width;
                                previewHeight = readablePreview.height;
                            }
                        }
                        finally
                        {
                            RenderTexture.active = previous;
                            if (rt != null) RenderTexture.ReleaseTemporary(rt);
                            if (readablePreview != null) UnityEngine.Object.DestroyImmediate(readablePreview);
                        }
                    }
                    catch (Exception ex)
                    {
                        CodelyLogger.LogWarning(
                            $"Failed to generate readable preview for '{path}': {ex.Message}. Preview might not be readable."
                        );
                        // Fallback: Try getting static preview if available?
                        // Texture2D staticPreview = AssetPreview.GetMiniThumbnail(asset);
                    }
                }
                else
                {
                    CodelyLogger.LogWarning(
                        $"Could not get asset preview for {path} (Type: {assetType?.Name}). Is it supported?"
                    );
                }
            }

            return new
            {
                path = path,
                guid = guid,
                assetType = assetType?.FullName ?? "Unknown",
                name = Path.GetFileNameWithoutExtension(path),
                fileName = Path.GetFileName(path),
                isFolder = AssetDatabase.IsValidFolder(path),
                instanceID = asset?.GetStableInstanceId() ?? 0,
                lastWriteTimeUtc = File.GetLastWriteTimeUtc(
                        Path.Combine(Directory.GetCurrentDirectory(), path)
                    )
                    .ToString("o"), // ISO 8601
                // --- Preview Data ---
                previewBase64 = previewBase64, // PNG data as Base64 string
                previewWidth = previewWidth,
                previewHeight = previewHeight,
                // TODO: Add more metadata? Importer settings? Dependencies?
            };
        }
        // --- Ensure Methods (Idempotent Operations) ---

        /// <summary>
        /// Ensures an asset has a .meta file. Idempotent - safe if .meta already exists.
        /// </summary>
        private static object EnsureHasMeta(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return Response.Error("'path' is required for ensure_has_meta.");

                string fullPath = SanitizeAssetPath(path);
                bool ghostDesync;
                if (!AssetExists(fullPath, out ghostDesync))
                    return BuildAssetNotFoundResponse($"Asset not found at path: {fullPath}", fullPath, ghostDesync);

                string metaPath = fullPath + ".meta";
                bool metaExists = File.Exists(metaPath);

                if (metaExists)
                {
                    return new
                    {
                        success = true,
                        message = "Asset .meta file already exists.",
                        data = new { path = fullPath, hasMeta = true, alreadyExists = true }
                    };
                }

                // Meta doesn't exist - trigger reimport to generate it safely
                AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.SaveAssets();

                metaExists = File.Exists(metaPath);
                if (!metaExists)
                {
                    return Response.Error($"Failed to generate .meta file for: {fullPath}");
                }

                return new
                {
                    success = true,
                    message = "Asset .meta file generated.",
                    data = new { path = fullPath, hasMeta = true, alreadyExists = false }
                };
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to ensure .meta file: {e.Message}");
            }
        }

        /// <summary>
        /// Checks .meta file integrity and consistency with asset.
        /// Read-only check - provides recommendations without auto-fixing.
        /// </summary>
        private static object EnsureMetaIntegrity(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return Response.Error("'path' is required for ensure_meta_integrity.");

                string fullPath = SanitizeAssetPath(path);
                bool ghostDesync;
                if (!AssetExists(fullPath, out ghostDesync))
                    return BuildAssetNotFoundResponse($"Asset not found at path: {fullPath}", fullPath, ghostDesync);

                string metaPath = fullPath + ".meta";
                if (!File.Exists(metaPath))
                {
                    return new
                    {
                        success = false,
                        message = "Asset .meta file is missing.",
                        data = new
                        {
                            path = fullPath,
                            hasMeta = false,
                            issues = new[] { "meta_file_missing" },
                            recommendation = "Use 'ensure_has_meta' action to generate .meta file"
                        }
                    };
                }

                var issues = new List<string>();
                var recommendations = new List<string>();

                // Check GUID
                string guid = AssetDatabase.AssetPathToGUID(fullPath);
                if (string.IsNullOrEmpty(guid))
                {
                    issues.Add("guid_invalid");
                    recommendations.Add("Reimport asset to regenerate GUID");
                }

                // Check importer settings exist
                AssetImporter importer = AssetImporter.GetAtPath(fullPath);
                if (importer == null)
                {
                    issues.Add("importer_not_found");
                    recommendations.Add("Reimport asset to fix importer");
                }

                // Check file timestamp consistency
                DateTime assetModified = File.GetLastWriteTimeUtc(fullPath);
                DateTime metaModified = File.GetLastWriteTimeUtc(metaPath);

                if (assetModified > metaModified.AddSeconds(5)) // 5 second grace period
                {
                    issues.Add("meta_outdated");
                    recommendations.Add("Reimport asset to update .meta file");
                }

                bool isHealthy = issues.Count == 0;

                return new
                {
                    success = true,
                    message = isHealthy ? "Asset .meta file is healthy." : "Asset .meta file has issues.",
                    data = new
                    {
                        path = fullPath,
                        hasMeta = true,
                        guid = guid,
                        healthy = isHealthy,
                        issues = issues.ToArray(),
                        recommendations = recommendations.ToArray(),
                        timestamps = new
                        {
                            asset = assetModified.ToString("o"),
                            meta = metaModified.ToString("o")
                        }
                    }
                };
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to check .meta integrity: {e.Message}");
            }
        }
    }
}

