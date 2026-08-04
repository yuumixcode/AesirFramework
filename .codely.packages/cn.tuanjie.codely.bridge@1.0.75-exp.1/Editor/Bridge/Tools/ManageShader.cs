using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Codely.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityTcp.Editor.Helpers;

namespace UnityTcp.Editor.Tools
{
    /// <summary>
    /// Handles CRUD operations for shader files within the Unity project.
    /// </summary>
    public static class ManageShader
    {
        /// <summary>
        /// Main handler for shader management actions.
        /// </summary>
        public static object HandleCommand(JObject @params)
        {
            // Extract parameters
            string action = @params["action"]?.ToString().ToLower();
            string name = @params["name"]?.ToString();
            string path = @params["path"]?.ToString(); // Relative to Assets/
            string contents = null;
            // Check if we have base64 encoded contents
            bool contentsEncoded = @params["contentsEncoded"]?.ToObject<bool>() ?? false;
            if (contentsEncoded && @params["encodedContents"] != null)
            {
                try
                {
                    contents = DecodeBase64(@params["encodedContents"].ToString());
                }
                catch (Exception e)
                {
                    return Response.Error($"Failed to decode shader contents: {e.Message}");
                }
            }
            else
            {
                contents = @params["contents"]?.ToString();
            }

            // Validate required parameters
            if (string.IsNullOrEmpty(action))
            {
                return Response.Error("Action parameter is required.");
            }

            // Skip name validation for SRP operations
            if (action != "detect_render_pipeline" && action != "ensure_material_shader_for_srp")
            {
                if (string.IsNullOrEmpty(name))
                {
                    return Response.Error("Name parameter is required.");
                }
                // Basic name validation (alphanumeric, underscores, cannot start with number)
                if (!Regex.IsMatch(name, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
                {
                    return Response.Error(
                        $"Invalid shader name: '{name}'. Use only letters, numbers, underscores, and don't start with a number."
                    );
                }
            }

            // Ensure path is relative to Assets/, removing any leading "Assets/"
            // Set default directory to "Shaders" if path is not provided
            string relativeDir = path ?? "Shaders"; // Default to "Shaders" if path is null
            if (!string.IsNullOrEmpty(relativeDir))
            {
                relativeDir = relativeDir.Replace('\\', '/').Trim('/');
                if (relativeDir.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                {
                    relativeDir = relativeDir.Substring("Assets/".Length).TrimStart('/');
                }
            }
            // Handle empty string case explicitly after processing
            if (string.IsNullOrEmpty(relativeDir))
            {
                relativeDir = "Shaders"; // Ensure default if path was provided as "" or only "/" or "Assets/"
            }

            // Construct paths
            string shaderFileName = $"{name}.shader";
            string fullPathDir = Path.Combine(Application.dataPath, relativeDir);
            string fullPath = Path.Combine(fullPathDir, shaderFileName);
            string relativePath = Path.Combine("Assets", relativeDir, shaderFileName)
                .Replace('\\', '/'); // Ensure "Assets/" prefix and forward slashes

            // Ensure the target directory exists for create/update
            if (action == "create" || action == "update")
            {
                try
                {
                    if (!Directory.Exists(fullPathDir))
                    {
                        Directory.CreateDirectory(fullPathDir);
                        // Refresh AssetDatabase to recognize new folders
                        AssetDatabase.Refresh();
                    }
                }
                catch (Exception e)
                {
                    return Response.Error(
                        $"Could not create directory '{fullPathDir}': {e.Message}"
                    );
                }
            }

            // Route to specific action handlers
            switch (action)
            {
                // SRP operations (don't require name validation)
                case "detect_render_pipeline":
                    return DetectRenderPipeline();
                case "ensure_material_shader_for_srp":
                    return EnsureMaterialShaderForSRP(@params);

                // Regular shader file operations
                case "create":
                    return CreateShader(fullPath, relativePath, name, contents);
                case "read":
                    return ReadShader(fullPath, relativePath);
                case "update":
                    return UpdateShader(fullPath, relativePath, name, contents);
                case "delete":
                    return DeleteShader(fullPath, relativePath);
                default:
                    return Response.Error(
                        $"Unknown action: '{action}'. Valid actions are: detect_render_pipeline, ensure_material_shader_for_srp, create, read, update, delete."
                    );
            }
        }

        /// <summary>
        /// Decode base64 string to normal text
        /// </summary>
        private static string DecodeBase64(string encoded)
        {
            byte[] data = Convert.FromBase64String(encoded);
            return System.Text.Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// Encode text to base64 string
        /// </summary>
        private static string EncodeBase64(string text)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(data);
        }

        private static object CreateShader(
            string fullPath,
            string relativePath,
            string name,
            string contents
        )
        {
            // Check if shader already exists
            if (File.Exists(fullPath))
            {
                return Response.Error(
                    $"Shader already exists at '{relativePath}'. Use 'update' action to modify."
                );
            }

            // Add validation for shader name conflicts in Unity
            if (Shader.Find(name) != null)
            {
                return Response.Error(
                    $"A shader with name '{name}' already exists in the project. Choose a different name."
                );
            }

            // Generate default content if none provided
            if (string.IsNullOrEmpty(contents))
            {
                contents = GenerateDefaultShaderContent(name);
            }

            try
            {
                File.WriteAllText(fullPath, contents, new System.Text.UTF8Encoding(false));
                AssetDatabase.ImportAsset(relativePath);
                AssetDatabase.Refresh(); // Ensure Unity recognizes the new shader
                return Response.Success(
                    $"Shader '{name}.shader' created successfully at '{relativePath}'.",
                    new { path = relativePath }
                );
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to create shader '{relativePath}': {e.Message}");
            }
        }

        private static object ReadShader(string fullPath, string relativePath)
        {
            if (!File.Exists(fullPath))
            {
                return Response.Error($"Shader not found at '{relativePath}'.");
            }

            try
            {
                string contents = File.ReadAllText(fullPath);

                // Return both normal and encoded contents for larger files
                //TODO: Consider a threshold for large files
                bool isLarge = contents.Length > 10000; // If content is large, include encoded version
                var responseData = new
                {
                    path = relativePath,
                    contents = contents,
                    // For large files, also include base64-encoded version
                    encodedContents = isLarge ? EncodeBase64(contents) : null,
                    contentsEncoded = isLarge,
                };

                return Response.Success(
                    $"Shader '{Path.GetFileName(relativePath)}' read successfully.",
                    responseData
                );
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to read shader '{relativePath}': {e.Message}");
            }
        }

        private static object UpdateShader(
            string fullPath,
            string relativePath,
            string name,
            string contents
        )
        {
            if (!File.Exists(fullPath))
            {
                return Response.Error(
                    $"Shader not found at '{relativePath}'. Use 'create' action to add a new shader."
                );
            }
            if (string.IsNullOrEmpty(contents))
            {
                return Response.Error("Content is required for the 'update' action.");
            }

            try
            {
                File.WriteAllText(fullPath, contents, new System.Text.UTF8Encoding(false));
                AssetDatabase.ImportAsset(relativePath);
                AssetDatabase.Refresh();
                return Response.Success(
                    $"Shader '{Path.GetFileName(relativePath)}' updated successfully.",
                    new { path = relativePath }
                );
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to update shader '{relativePath}': {e.Message}");
            }
        }

        private static object DeleteShader(string fullPath, string relativePath)
        {
            if (!File.Exists(fullPath))
            {
                return Response.Error($"Shader not found at '{relativePath}'.");
            }

            try
            {
                // Delete the asset through Unity's AssetDatabase first
                bool success = AssetDatabase.DeleteAsset(relativePath);
                if (!success)
                {
                    return Response.Error($"Failed to delete shader through Unity's AssetDatabase: '{relativePath}'");
                }

                // If the file still exists (rare case), try direct deletion
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                return Response.Success($"Shader '{Path.GetFileName(relativePath)}' deleted successfully.");
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to delete shader '{relativePath}': {e.Message}");
            }
        }

        //This is a CGProgram template
        //TODO: making a HLSL template as well?
        private static string GenerateDefaultShaderContent(string name)
        {
            return @"Shader """ + name + @"""
        {
            Properties
            {
                _MainTex (""Texture"", 2D) = ""white"" {}
            }
            SubShader
            {
                Tags { ""RenderType""=""Opaque"" }
                LOD 100

                Pass
                {
                    CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #include ""UnityCG.cginc""

                    struct appdata
                    {
                        float4 vertex : POSITION;
                        float2 uv : TEXCOORD0;
                    };

                    struct v2f
                    {
                        float2 uv : TEXCOORD0;
                        float4 vertex : SV_POSITION;
                    };

                    sampler2D _MainTex;
                    float4 _MainTex_ST;

                    v2f vert (appdata v)
                    {
                        v2f o;
                        o.vertex = UnityObjectToClipPos(v.vertex);
                        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                        return o;
                    }

                    fixed4 frag (v2f i) : SV_Target
                    {
                        fixed4 col = tex2D(_MainTex, i.uv);
                        return col;
                    }
                    ENDCG
                }
            }
        }";
        }
        // --- SRP/Shader Safety Methods ---

        /// <summary>
        /// Detects the current render pipeline in use.
        /// </summary>
        private static object DetectRenderPipeline()
        {
            try
            {
                string srp = "builtin";
                var currentRP = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;

                if (currentRP != null)
                {
                    string rpName = currentRP.GetType().Name.ToLowerInvariant();
                    string rpFullName = currentRP.GetType().FullName.ToLowerInvariant();

                    if (rpName.Contains("urp") || rpName.Contains("universal") || 
                        rpFullName.Contains("universal"))
                    {
                        srp = "urp";
                    }
                    else if (rpName.Contains("hdrp") || rpName.Contains("highdefinition") || 
                             rpFullName.Contains("highdefinition"))
                    {
                        srp = "hdrp";
                    }
                }

                return new
                {
                    success = true,
                    message = $"Current render pipeline: {srp}",
                    data = new
                    {
                        srp = srp,
                        rpAssetName = currentRP?.name,
                        rpTypeName = currentRP?.GetType().FullName
                    }
                };
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to detect render pipeline: {e.Message}");
            }
        }

        /// <summary>
        /// Ensures a material uses the appropriate shader for the current SRP. Idempotent.
        /// Supports both "material" (legacy) and "material_path"/"material_guid"
        /// as described in the Unity-Tools-Spec.
        /// </summary>
        private static object EnsureMaterialShaderForSRP(JObject @params)
        {
            try
            {
                var writeCheck = WriteGuard.CheckWriteAllowed("ensure_material_shader_for_srp");
                if (writeCheck != null) return writeCheck;

                // Accept multiple parameter shapes.
                // Primary spec uses "material_path" / "material_guid",
                // but we still accept legacy "material" for backwards compatibility.
                string materialPath = @params["material_path"]?.ToString();

                // Legacy fallback: allow "material" if material_path is not provided
                if (string.IsNullOrEmpty(materialPath))
                {
                    materialPath = @params["material"]?.ToString();
                }

                // Resolve from GUID if path not provided
                if (string.IsNullOrEmpty(materialPath))
                {
                    var guid = @params["material_guid"]?.ToString();
                    if (!string.IsNullOrEmpty(guid))
                    {
                        materialPath = AssetDatabase.GUIDToAssetPath(guid);
                    }
                }

                if (string.IsNullOrEmpty(materialPath))
                    return Response.Error("Either material_path or material_guid is required for ensure_material_shader_for_srp action");

                // Validate path format (mirror TS-side validation)
                if (!materialPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                    return Response.Error("material_path must start with \"Assets/\"");

                JObject shaderMapping = @params["shader_for_srp"] as JObject;
                if (shaderMapping == null)
                    return Response.Error("shader_for_srp is required for ensure_material_shader_for_srp action");

                if (!shaderMapping.ContainsKey("builtin") ||
                    string.IsNullOrWhiteSpace(shaderMapping["builtin"]?.ToString()))
                {
                    return Response.Error("shader_for_srp.builtin is required as fallback shader");
                }

                // Load material
                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null)
                    return Response.Error($"Material not found at: {materialPath}");

                // Detect current SRP
                string currentSrp = "builtin";
                var currentRP = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
                if (currentRP != null)
                {
                    string rpName = currentRP.GetType().Name.ToLowerInvariant();
                    if (rpName.Contains("urp") || rpName.Contains("universal"))
                        currentSrp = "urp";
                    else if (rpName.Contains("hdrp") || rpName.Contains("highdefinition"))
                        currentSrp = "hdrp";
                }

                // Get appropriate shader name
                string targetShaderName = null;
                if (currentSrp == "urp" && shaderMapping.ContainsKey("urp"))
                    targetShaderName = shaderMapping["urp"]?.ToString();
                else if (currentSrp == "hdrp" && shaderMapping.ContainsKey("hdrp"))
                    targetShaderName = shaderMapping["hdrp"]?.ToString();
                else if (shaderMapping.ContainsKey("builtin"))
                    targetShaderName = shaderMapping["builtin"]?.ToString(); // Fallback
                
                if (string.IsNullOrEmpty(targetShaderName))
                    return Response.Error($"No shader mapping provided for current SRP: {currentSrp}");

                // Find shader
                Shader targetShader = Shader.Find(targetShaderName);
                if (targetShader == null)
                    return Response.Error($"Shader not found: {targetShaderName}");

                // Check if material already uses this shader (idempotent)
                if (material.shader == targetShader)
                {
                    return new
                    {
                        success = true,
                        message = $"Material already uses appropriate shader for {currentSrp}.",
                        data = new
                        {
                            material = materialPath,
                            currentSrp = currentSrp,
                            shader = targetShaderName,
                            alreadyCorrect = true
                        }
                    };
                }

                // Cache old shader name BEFORE switching
                string oldShaderName = material.shader?.name ?? "None";

                // Switch shader
                material.shader = targetShader;
                EditorUtility.SetDirty(material);
                AssetDatabase.SaveAssets();

                return new
                {
                    success = true,
                    message = $"Material shader switched for {currentSrp}.",
                    data = new
                    {
                        material = materialPath,
                        currentSrp = currentSrp,
                        oldShader = oldShaderName,
                        newShader = targetShaderName,
                        alreadyCorrect = false
                    }
                };
            }
            catch (Exception e)
            {
                return Response.Error($"Failed to ensure material shader for SRP: {e.Message}");
            }
        }
    }
} 
