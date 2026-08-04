using System;
using System.Collections.Generic;
using Codely.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityTcp.Editor.Helpers;

namespace UnityTcp.Editor.Tools
{
    /// <summary>
    /// Custom validation helpers that can be invoked via the execute_custom_tool MCP tool.
    /// Each method validates a specific scenario and returns a standard response object:
    ///
    /// {
    ///   success: bool,
    ///   message: string,
    ///   data: { ... optional extra info ... }
    /// }
    ///
    /// Tool registration is done via [ExecuteCustomTool.CustomTool] attribute.
    /// </summary>
    public static class CodelyUnityValidationTools
    {
        /// <summary>
        /// Validates current play mode against an expected value.
        /// tool_name: "codely.validate_play_mode"
        ///
        /// Parameters:
        /// {
        ///   "expected": "stopped" | "playing" | "paused"
        /// }
        /// </summary>
        [ExecuteCustomTool.CustomTool("codely.validate_play_mode", "Validate current editor play mode")]
        public static object ValidatePlayMode(JObject parameters)
        {
            var expected = parameters?["expected"]?.ToString() ?? "stopped";

            var actual = GetPlayModeString();

            if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
            {
                var msg = $"PlayMode mismatch. Expected={expected}, Actual={actual}";
                CodelyLogger.LogError($"[CodelyValidation] {msg}");
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = msg,
                    ["data"] = new Dictionary<string, object>
                    {
                        ["expected"] = expected,
                        ["actual"] = actual
                    }
                };
            }

            var okMsg = $"PlayMode OK: {actual}";
            CodelyLogger.Log($"[CodelyValidation] {okMsg}");
            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["message"] = okMsg,
                ["data"] = new Dictionary<string, object>
                {
                    ["expected"] = expected,
                    ["actual"] = actual
                }
            };
        }

        /// <summary>
        /// Validates that the latest console messages contain at least one entry
        /// matching the given filter text and type set.
        ///
        /// tool_name: "codely.validate_console_contains"
        ///
        /// Parameters:
        /// {
        ///   "filterText": "ConsoleSpamTest",
        ///   "types": ["error","warning","log","exception"],
        ///   "minCount": 1
        /// }
        /// </summary>
        [ExecuteCustomTool.CustomTool("codely.validate_console_contains", "Validate console contains messages matching filter")]
        public static object ValidateConsoleContains(JObject parameters)
        {
            var filterText = parameters?["filterText"]?.ToString() ?? string.Empty;
            var minCount = parameters?["minCount"]?.ToObject<int?>() ?? 1;

            var typesToken = parameters?["types"] as JArray;
            var types = new List<string>();
            if (typesToken != null)
            {
                foreach (var t in typesToken)
                {
                    if (t.Type == JTokenType.String)
                    {
                        types.Add(t.ToString());
                    }
                }
            }
            if (types.Count == 0)
            {
                types.AddRange(new[] { "error", "warning", "log" });
            }

            var logEntries = ReadConsoleMessages(types, filterText);

            var matchedCount = logEntries.Count;
            if (matchedCount < minCount)
            {
                var msg = $"Console validation failed. Expected at least {minCount} messages containing '{filterText}', but found {matchedCount}.";
                CodelyLogger.LogError($"[CodelyValidation] {msg}");
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = msg,
                    ["data"] = new Dictionary<string, object>
                    {
                        ["filterText"] = filterText,
                        ["types"] = types,
                        ["matchedCount"] = matchedCount,
                        ["messages"] = logEntries
                    }
                };
            }

            var okMsg = $"Console validation OK. Found {matchedCount} messages containing '{filterText}'.";
            CodelyLogger.Log($"[CodelyValidation] {okMsg}");
            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["message"] = okMsg,
                ["data"] = new Dictionary<string, object>
                {
                    ["filterText"] = filterText,
                    ["types"] = types,
                    ["matchedCount"] = matchedCount,
                    ["messages"] = logEntries
                }
            };
        }

        /// <summary>
        /// Validates that a specific Tag and Layer both exist in the project.
        ///
        /// tool_name: "codely.validate_tag_and_layer_exist"
        ///
        /// Parameters:
        /// {
        ///   "tagName": "CodelyTestTag",
        ///   "layerName": "CodelyLayer"
        /// }
        /// </summary>
        [ExecuteCustomTool.CustomTool("codely.validate_tag_and_layer_exist", "Validate that a specific Tag and Layer exist")]
        public static object ValidateTagAndLayerExist(JObject parameters)
        {
            var tagName = parameters?["tagName"]?.ToString();
            var layerName = parameters?["layerName"]?.ToString();

            var missing = new List<string>();

            if (!string.IsNullOrEmpty(tagName) && Array.IndexOf(UnityEditorInternal.InternalEditorUtility.tags, tagName) < 0)
            {
                missing.Add($"Tag '{tagName}'");
            }

            if (!string.IsNullOrEmpty(layerName) && Array.IndexOf(UnityEditorInternal.InternalEditorUtility.layers, layerName) < 0)
            {
                missing.Add($"Layer '{layerName}'");
            }

            if (missing.Count > 0)
            {
                var msg = "Missing project identifiers: " + string.Join(", ", missing);
                CodelyLogger.LogError($"[CodelyValidation] {msg}");
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = msg,
                    ["data"] = new Dictionary<string, object>
                    {
                        ["tagName"] = tagName,
                        ["layerName"] = layerName
                    }
                };
            }

            var okMsg = $"Tag/Layer validation OK. Tag='{tagName}', Layer='{layerName}'.";
            CodelyLogger.Log($"[CodelyValidation] {okMsg}");
            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["message"] = okMsg,
                ["data"] = new Dictionary<string, object>
                {
                    ["tagName"] = tagName,
                    ["layerName"] = layerName
                }
            };
        }

        /// <summary>
        /// Generic response validator - checks if a tool response contains expected fields/values.
        ///
        /// tool_name: "codely.validate_response"
        ///
        /// Parameters:
        /// {
        ///   "hasField": "fieldName",           // Check if response has this field
        ///   "fieldEquals": { "field": "value" }, // Check if field equals value
        ///   "isSuccess": true/false,           // Check success field
        ///   "messageContains": "text"          // Check if message contains text
        /// }
        /// </summary>
        [ExecuteCustomTool.CustomTool("codely.validate_response", "Generic response validator")]
        public static object ValidateResponse(JObject parameters)
        {
            // This tool is meant to be called after another tool call
            // The caller should pass the previous response data for validation
            var hasField = parameters?["hasField"]?.ToString();
            var fieldEquals = parameters?["fieldEquals"] as JObject;
            var isSuccess = parameters?["isSuccess"]?.ToObject<bool?>();
            var messageContains = parameters?["messageContains"]?.ToString();
            var responseData = parameters?["responseData"] as JObject;

            var errors = new List<string>();

            if (responseData == null)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = "No responseData provided for validation",
                    ["data"] = new Dictionary<string, object>()
                };
            }

            // Check hasField (supports nested paths like "state.project.srp" or "project.srp")
            // Also supports automatic recursive search if direct path fails
            if (!string.IsNullOrEmpty(hasField))
            {
                JToken fieldValue = null;
                string foundPath = null;
                
                if (hasField.Contains("."))
                {
                    // Handle explicit nested field paths (e.g., "state.project.srp" or "project.srp")
                    var pathParts = hasField.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    JToken current = responseData;
                    foreach (var part in pathParts)
                    {
                        if (current == null || current.Type != JTokenType.Object)
                        {
                            current = null;
                            break;
                        }
                        current = current[part];
                    }
                    fieldValue = current;
                    if (fieldValue != null && fieldValue.Type != JTokenType.Null)
                    {
                        foundPath = hasField;
                    }
                }
                else
                {
                    // First try simple top-level field check
                    fieldValue = responseData[hasField];
                    if (fieldValue != null && fieldValue.Type != JTokenType.Null)
                    {
                        foundPath = hasField;
                    }
                }
                
                // If not found and it's a simple field name (no dots), try recursive search
                if ((fieldValue == null || fieldValue.Type == JTokenType.Null) && !hasField.Contains("."))
                {
                    var recursiveResult = FindFieldRecursive(responseData, hasField);
                    if (recursiveResult.found)
                    {
                        fieldValue = recursiveResult.value;
                        foundPath = recursiveResult.path;
                    }
                }
                
                if (fieldValue == null || fieldValue.Type == JTokenType.Null)
                {
                    errors.Add($"Missing field: {hasField}" + (foundPath == null ? "" : $" (searched recursively, not found)"));
                }
                else if (foundPath != null && foundPath != hasField)
                {
                    // Log that we found it via recursive search (for debugging)
                    CodelyLogger.Log($"[CodelyValidation] Field '{hasField}' found at nested path: {foundPath}");
                }
            }

            // Check fieldEquals
            if (fieldEquals != null)
            {
                foreach (var prop in fieldEquals.Properties())
                {
                    var actualValue = responseData[prop.Name];
                    if (actualValue == null)
                    {
                        errors.Add($"Field '{prop.Name}' not found");
                    }
                    else if (!JToken.DeepEquals(actualValue, prop.Value))
                    {
                        errors.Add($"Field '{prop.Name}' expected={prop.Value}, actual={actualValue}");
                    }
                }
            }

            // Check isSuccess
            if (isSuccess.HasValue)
            {
                var actualSuccess = responseData["success"]?.ToObject<bool?>() ?? false;
                if (actualSuccess != isSuccess.Value)
                {
                    errors.Add($"success expected={isSuccess.Value}, actual={actualSuccess}");
                }
            }

            // Check messageContains
            if (!string.IsNullOrEmpty(messageContains))
            {
                var message = responseData["message"]?.ToString() ?? "";
                if (message.IndexOf(messageContains, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    errors.Add($"message does not contain '{messageContains}'");
                }
            }

            if (errors.Count > 0)
            {
                var msg = "Response validation failed: " + string.Join("; ", errors);
                CodelyLogger.LogError($"[CodelyValidation] {msg}");
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = msg,
                    ["data"] = new Dictionary<string, object> { ["errors"] = errors }
                };
            }

            var okMsg = "Response validation OK";
            CodelyLogger.Log($"[CodelyValidation] {okMsg}");
            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["message"] = okMsg,
                ["data"] = new Dictionary<string, object>()
            };
        }

        /// <summary>
        /// Validates that the active editor tool matches expected.
        ///
        /// tool_name: "codely.validate_active_tool"
        ///
        /// Parameters:
        /// {
        ///   "expected": "Move" | "Rotate" | "Scale" | "View" | "Rect" | "Transform"
        /// }
        /// </summary>
        [ExecuteCustomTool.CustomTool("codely.validate_active_tool", "Validate current active editor tool")]
        public static object ValidateActiveTool(JObject parameters)
        {
            var expected = parameters?["expected"]?.ToString();
            if (string.IsNullOrEmpty(expected))
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = "Parameter 'expected' is required",
                    ["data"] = new Dictionary<string, object>()
                };
            }

            var currentTool = UnityEditor.Tools.current;
            var actual = currentTool.ToString();

            // Map Unity's Tool enum to friendly names
            var toolNameMap = new Dictionary<UnityEditor.Tool, string>
            {
                { UnityEditor.Tool.View, "View" },
                { UnityEditor.Tool.Move, "Move" },
                { UnityEditor.Tool.Rotate, "Rotate" },
                { UnityEditor.Tool.Scale, "Scale" },
                { UnityEditor.Tool.Rect, "Rect" },
                { UnityEditor.Tool.Transform, "Transform" }
            };

            if (toolNameMap.TryGetValue(currentTool, out var friendlyName))
            {
                actual = friendlyName;
            }

            if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
            {
                var msg = $"Active tool mismatch. Expected={expected}, Actual={actual}";
                CodelyLogger.LogError($"[CodelyValidation] {msg}");
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = msg,
                    ["data"] = new Dictionary<string, object>
                    {
                        ["expected"] = expected,
                        ["actual"] = actual
                    }
                };
            }

            var okMsg = $"Active tool OK: {actual}";
            CodelyLogger.Log($"[CodelyValidation] {okMsg}");
            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["message"] = okMsg,
                ["data"] = new Dictionary<string, object>
                {
                    ["expected"] = expected,
                    ["actual"] = actual
                }
            };
        }

        /// <summary>
        /// Validates editor is not compiling.
        ///
        /// tool_name: "codely.validate_not_compiling"
        /// </summary>
        [ExecuteCustomTool.CustomTool("codely.validate_not_compiling", "Validate editor is not compiling")]
        public static object ValidateNotCompiling(JObject parameters)
        {
            var isCompiling = EditorApplication.isCompiling;

            if (isCompiling)
            {
                var msg = "Editor is still compiling";
                CodelyLogger.LogError($"[CodelyValidation] {msg}");
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = msg,
                    ["data"] = new Dictionary<string, object> { ["isCompiling"] = true }
                };
            }

            var okMsg = "Editor is not compiling";
            CodelyLogger.Log($"[CodelyValidation] {okMsg}");
            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["message"] = okMsg,
                ["data"] = new Dictionary<string, object> { ["isCompiling"] = false }
            };
        }

        /// <summary>
        /// Validates console message count within expected range.
        ///
        /// tool_name: "codely.validate_console_count"
        ///
        /// Parameters:
        /// {
        ///   "minCount": 0,
        ///   "maxCount": 100,
        ///   "types": ["error", "warning", "log"]
        /// }
        /// </summary>
        [ExecuteCustomTool.CustomTool("codely.validate_console_count", "Validate console message count")]
        public static object ValidateConsoleCount(JObject parameters)
        {
            var minCount = parameters?["minCount"]?.ToObject<int?>() ?? 0;
            var maxCount = parameters?["maxCount"]?.ToObject<int?>() ?? int.MaxValue;

            var typesToken = parameters?["types"] as JArray;
            var types = new List<string>();
            if (typesToken != null)
            {
                foreach (var t in typesToken)
                {
                    if (t.Type == JTokenType.String)
                    {
                        types.Add(t.ToString());
                    }
                }
            }
            if (types.Count == 0)
            {
                types.AddRange(new[] { "error", "warning", "log" });
            }

            var messages = ReadConsoleMessages(types, "");
            var count = messages.Count;

            if (count < minCount || count > maxCount)
            {
                var msg = $"Console count out of range. Expected [{minCount}, {maxCount}], Actual={count}";
                CodelyLogger.LogError($"[CodelyValidation] {msg}");
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = msg,
                    ["data"] = new Dictionary<string, object>
                    {
                        ["count"] = count,
                        ["minCount"] = minCount,
                        ["maxCount"] = maxCount
                    }
                };
            }

            var okMsg = $"Console count OK: {count} (range [{minCount}, {maxCount}])";
            CodelyLogger.Log($"[CodelyValidation] {okMsg}");
            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["message"] = okMsg,
                ["data"] = new Dictionary<string, object>
                {
                    ["count"] = count,
                    ["minCount"] = minCount,
                    ["maxCount"] = maxCount
                }
            };
        }

        // ---------------------------------------------------------------------
        // Internal helpers
        // ---------------------------------------------------------------------

        /// <summary>
        /// Recursively searches for a field in a JToken tree.
        /// Returns the found value and its path, or null if not found.
        /// </summary>
        private static (bool found, JToken value, string path) FindFieldRecursive(JToken token, string fieldName, string currentPath = "")
        {
            if (token == null || token.Type == JTokenType.Null)
                return (false, null, null);

            // If it's an object, check if it has the field
            if (token.Type == JTokenType.Object)
            {
                var obj = (JObject)token;
                if (obj[fieldName] != null && obj[fieldName].Type != JTokenType.Null)
                {
                    var path = string.IsNullOrEmpty(currentPath) ? fieldName : $"{currentPath}.{fieldName}";
                    return (true, obj[fieldName], path);
                }

                // Recursively search in all properties
                foreach (var prop in obj.Properties())
                {
                    var newPath = string.IsNullOrEmpty(currentPath) ? prop.Name : $"{currentPath}.{prop.Name}";
                    var result = FindFieldRecursive(prop.Value, fieldName, newPath);
                    if (result.found)
                        return result;
                }
            }
            // If it's an array, search in each element
            else if (token.Type == JTokenType.Array)
            {
                var arr = (JArray)token;
                for (int i = 0; i < arr.Count; i++)
                {
                    var newPath = $"{currentPath}[{i}]";
                    var result = FindFieldRecursive(arr[i], fieldName, newPath);
                    if (result.found)
                        return result;
                }
            }

            return (false, null, null);
        }

        private static string GetPlayModeString()
        {
            if (!Application.isPlaying)
            {
                return "stopped";
            }

            return EditorApplication.isPaused ? "paused" : "playing";
        }

        /// <summary>
        /// Reads current console messages via Unity's internal LogEntries API.
        /// We keep this intentionally simple: only type + message text for validation.
        /// </summary>
        private static List<Dictionary<string, object>> ReadConsoleMessages(List<string> allowedTypes, string filterText)
        {
            var results = new List<Dictionary<string, object>>();

            var logEntriesType = Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            var logEntryType = Type.GetType("UnityEditor.LogEntry, UnityEditor.dll");
            if (logEntriesType == null || logEntryType == null)
            {
                CodelyLogger.LogWarning("[CodelyValidation] Unable to access UnityEditor.LogEntries/LogEntry types.");
                return results;
            }

            var getCountMethod = logEntriesType.GetMethod("GetCount", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            var getEntryMethod = logEntriesType.GetMethod("GetEntryInternal", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            var startGettingEntries = logEntriesType.GetMethod("StartGettingEntries", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            var endGettingEntries = logEntriesType.GetMethod("EndGettingEntries", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            if (getCountMethod == null || getEntryMethod == null || startGettingEntries == null || endGettingEntries == null)
            {
                CodelyLogger.LogWarning("[CodelyValidation] Unable to reflect LogEntries methods.");
                return results;
            }

            var entry = Activator.CreateInstance(logEntryType);
            var conditionField = logEntryType.GetField("condition");
            var modeField = logEntryType.GetField("mode");

            startGettingEntries.Invoke(null, null);
            try
            {
                var count = (int)getCountMethod.Invoke(null, null);
                for (int i = 0; i < count; i++)
                {
                    object[] args = { i, entry };
                    getEntryMethod.Invoke(null, args);

                    var condition = conditionField?.GetValue(entry)?.ToString() ?? string.Empty;
                    var modeValue = modeField != null ? (int)modeField.GetValue(entry) : 0;
                    var typeName = LogTypeFromMode(modeValue);

                    if (allowedTypes.Count > 0 && !allowedTypes.Contains(typeName))
                        continue;

                    if (!string.IsNullOrEmpty(filterText) && condition.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;

                    results.Add(new Dictionary<string, object>
                    {
                        ["type"] = typeName,
                        ["message"] = condition
                    });
                }
            }
            finally
            {
                endGettingEntries.Invoke(null, null);
            }

            return results;
        }

        private static string LogTypeFromMode(int mode)
        {
            // Unity uses bit flags; we map common ones to our string types.
            // See: https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/ConsoleWindow.cs
            const int ErrorMask = 1;
            const int AssertMask = 2;
            const int LogMask = 4;
            const int FatalMask = 16;

            if ((mode & ErrorMask) != 0 || (mode & FatalMask) != 0)
                return "error";
            if ((mode & AssertMask) != 0)
                return "assert";
            if ((mode & LogMask) != 0)
                return "log";

            // Fallback
            return "log";
        }

        // =====================================================================
        // Shader / Material Validation Tools
        // =====================================================================

        /// <summary>
        /// Validates that a shader file exists at the expected location and optionally
        /// that its contents contain (or do not contain) specific substrings.
        ///
        /// tool_name: "codely.validate_shader_file"
        ///
        /// Parameters:
        /// {
        ///   "name": "CodelyTestShader1",          // Shader name without .shader (required)
        ///   "path": "Shaders/Custom",            // Optional: same semantics as unity_shader.path (relative to Assets/)
        ///   "shouldExist": true,                 // Optional: default true
        ///   "mustContain": ["Shader", "_Color"], // Optional: substrings that must appear in file
        ///   "mustNotContain": ["TODO"]           // Optional: substrings that must NOT appear
        /// }
        /// </summary>
        [ExecuteCustomTool.CustomTool("codely.validate_shader_file", "Validate shader file existence and contents")]
        public static object ValidateShaderFile(JObject parameters)
        {
            var name = parameters?["name"]?.ToString();
            if (string.IsNullOrEmpty(name))
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = "Parameter 'name' is required",
                    ["data"] = new Dictionary<string, object>()
                };
            }

            // Determine directory relative to Assets/, following the same rules as ManageShader
            var pathParam = parameters?["path"]?.ToString();
            string relativeDir = pathParam ?? "Shaders";
            if (!string.IsNullOrEmpty(relativeDir))
            {
                relativeDir = relativeDir.Replace('\\', '/').Trim('/');
                if (relativeDir.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                {
                    relativeDir = relativeDir.Substring("Assets/".Length).TrimStart('/');
                }
            }
            if (string.IsNullOrEmpty(relativeDir))
            {
                relativeDir = "Shaders";
            }

            var shaderFileName = $"{name}.shader";
            var fullPathDir = System.IO.Path.Combine(Application.dataPath, relativeDir);
            var fullPath = System.IO.Path.Combine(fullPathDir, shaderFileName);
            var relativePath = System.IO.Path.Combine("Assets", relativeDir, shaderFileName)
                .Replace('\\', '/');

            var shouldExist = parameters?["shouldExist"]?.ToObject<bool?>() ?? true;
            var mustContainArray = parameters?["mustContain"] as JArray;
            var mustNotContainArray = parameters?["mustNotContain"] as JArray;

            var errors = new List<string>();
            bool fileExists = System.IO.File.Exists(fullPath);

            if (shouldExist && !fileExists)
            {
                errors.Add($"Shader file expected but not found at '{relativePath}'.");
            }
            else if (!shouldExist && fileExists)
            {
                errors.Add($"Shader file should not exist at '{relativePath}', but it was found.");
            }

            string contents = null;
            if (fileExists && (mustContainArray != null || mustNotContainArray != null))
            {
                try
                {
                    contents = System.IO.File.ReadAllText(fullPath);
                }
                catch (Exception e)
                {
                    errors.Add($"Failed to read shader file '{relativePath}': {e.Message}");
                }
            }

            if (!string.IsNullOrEmpty(contents) && mustContainArray != null)
            {
                foreach (var item in mustContainArray)
                {
                    if (item.Type != JTokenType.String) continue;
                    var expected = item.ToString();
                    if (contents.IndexOf(expected, StringComparison.Ordinal) < 0)
                    {
                        errors.Add($"Shader file '{relativePath}' does not contain required text: \"{expected}\".");
                    }
                }
            }

            if (!string.IsNullOrEmpty(contents) && mustNotContainArray != null)
            {
                foreach (var item in mustNotContainArray)
                {
                    if (item.Type != JTokenType.String) continue;
                    var forbidden = item.ToString();
                    if (contents.IndexOf(forbidden, StringComparison.Ordinal) >= 0)
                    {
                        errors.Add($"Shader file '{relativePath}' contains forbidden text: \"{forbidden}\".");
                    }
                }
            }

            if (errors.Count > 0)
            {
                var msg = "Shader file validation failed: " + string.Join("; ", errors);
                CodelyLogger.LogError($"[CodelyValidation] {msg}");
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = msg,
                    ["data"] = new Dictionary<string, object>
                    {
                        ["name"] = name,
                        ["path"] = relativePath,
                        ["exists"] = fileExists,
                        ["errors"] = errors
                    }
                };
            }

            var okMsg = $"Shader file validation OK for '{relativePath}'.";
            CodelyLogger.Log($"[CodelyValidation] {okMsg}");
            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["message"] = okMsg,
                ["data"] = new Dictionary<string, object>
                {
                    ["name"] = name,
                    ["path"] = relativePath,
                    ["exists"] = fileExists
                }
            };
        }

        /// <summary>
        /// Validates that a material is using the expected shader for the current SRP,
        /// given a shader_for_srp mapping (same structure as unity_shader.ensure_material_shader_for_srp).
        ///
        /// tool_name: "codely.validate_material_shader_for_srp"
        ///
        /// Parameters:
        /// {
        ///   "material_path": "Assets/Materials/CodelyShaderTestMat.mat",
        ///   "shader_for_srp": { "builtin": "Standard", "urp": "Universal Render Pipeline/Lit", "hdrp": "HDRP/Lit" }
        /// }
        /// </summary>
        [ExecuteCustomTool.CustomTool("codely.validate_material_shader_for_srp", "Validate material shader against SRP mapping")]
        public static object ValidateMaterialShaderForSrp(JObject parameters)
        {
            var materialPath = parameters?["material_path"]?.ToString()
                              ?? parameters?["material"]?.ToString();
            if (string.IsNullOrEmpty(materialPath))
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = "Parameter 'material_path' (or legacy 'material') is required",
                    ["data"] = new Dictionary<string, object>()
                };
            }

            var shaderMapping = parameters?["shader_for_srp"] as JObject;
            if (shaderMapping == null)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = "Parameter 'shader_for_srp' is required",
                    ["data"] = new Dictionary<string, object>()
                };
            }

            if (!shaderMapping.ContainsKey("builtin"))
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = "shader_for_srp.builtin is required as fallback shader",
                    ["data"] = new Dictionary<string, object>()
                };
            }

            try
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null)
                {
                    return new Dictionary<string, object>
                    {
                        ["success"] = false,
                        ["message"] = $"Material not found at: {materialPath}",
                        ["data"] = new Dictionary<string, object>()
                    };
                }

                // Detect current SRP (same logic as ManageShader)
                var currentSrp = "builtin";
                var currentRP = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
                if (currentRP != null)
                {
                    var rpName = currentRP.GetType().Name.ToLowerInvariant();
                    var rpFullName = currentRP.GetType().FullName.ToLowerInvariant();
                    if (rpName.Contains("urp") || rpName.Contains("universal") || rpFullName.Contains("universal"))
                    {
                        currentSrp = "urp";
                    }
                    else if (rpName.Contains("hdrp") || rpName.Contains("highdefinition") || rpFullName.Contains("highdefinition"))
                    {
                        currentSrp = "hdrp";
                    }
                }

                // Resolve expected shader name based on SRP
                string expectedShaderName = null;
                if (currentSrp == "urp" && shaderMapping.ContainsKey("urp"))
                {
                    expectedShaderName = shaderMapping["urp"]?.ToString();
                }
                else if (currentSrp == "hdrp" && shaderMapping.ContainsKey("hdrp"))
                {
                    expectedShaderName = shaderMapping["hdrp"]?.ToString();
                }
                else if (shaderMapping.ContainsKey("builtin"))
                {
                    expectedShaderName = shaderMapping["builtin"]?.ToString();
                }

                if (string.IsNullOrEmpty(expectedShaderName))
                {
                    var msgNoMap = $"No shader mapping provided for current SRP: {currentSrp}";
                    CodelyLogger.LogError($"[CodelyValidation] {msgNoMap}");
                    return new Dictionary<string, object>
                    {
                        ["success"] = false,
                        ["message"] = msgNoMap,
                        ["data"] = new Dictionary<string, object>()
                    };
                }

                var actualShaderName = material.shader != null ? material.shader.name : "None";
                if (!string.Equals(actualShaderName, expectedShaderName, StringComparison.Ordinal))
                {
                    var msgMismatch =
                        $"Material '{materialPath}' shader mismatch for SRP '{currentSrp}'. Expected='{expectedShaderName}', Actual='{actualShaderName}'.";
                    CodelyLogger.LogError($"[CodelyValidation] {msgMismatch}");
                    return new Dictionary<string, object>
                    {
                        ["success"] = false,
                        ["message"] = msgMismatch,
                        ["data"] = new Dictionary<string, object>
                        {
                            ["material"] = materialPath,
                            ["currentSrp"] = currentSrp,
                            ["expectedShader"] = expectedShaderName,
                            ["actualShader"] = actualShaderName
                        }
                    };
                }

                var okMsg =
                    $"Material '{materialPath}' shader is correct for SRP '{currentSrp}': '{actualShaderName}'.";
                CodelyLogger.Log($"[CodelyValidation] {okMsg}");
                return new Dictionary<string, object>
                {
                    ["success"] = true,
                    ["message"] = okMsg,
                    ["data"] = new Dictionary<string, object>
                    {
                        ["material"] = materialPath,
                        ["currentSrp"] = currentSrp,
                        ["shader"] = actualShaderName
                    }
                };
            }
            catch (Exception e)
            {
                var msg = $"Failed to validate material shader for SRP: {e.Message}";
                CodelyLogger.LogError($"[CodelyValidation] {msg}");
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = msg,
                    ["data"] = new Dictionary<string, object>()
                };
            }
        }

        /// <summary>
        /// Simple render pipeline validation helper.
        ///
        /// tool_name: "codely.validate_render_pipeline"
        ///
        /// Parameters:
        /// {
        ///   "expected": "builtin" | "urp" | "hdrp" // Optional, if provided will be checked
        /// }
        /// </summary>
        [ExecuteCustomTool.CustomTool("codely.validate_render_pipeline", "Validate current render pipeline SRP value")]
        public static object ValidateRenderPipeline(JObject parameters)
        {
            var expected = parameters?["expected"]?.ToString();

            try
            {
                var srp = "builtin";
                var currentRP = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;

                if (currentRP != null)
                {
                    var rpName = currentRP.GetType().Name.ToLowerInvariant();
                    var rpFullName = currentRP.GetType().FullName.ToLowerInvariant();
                    if (rpName.Contains("urp") || rpName.Contains("universal") || rpFullName.Contains("universal"))
                    {
                        srp = "urp";
                    }
                    else if (rpName.Contains("hdrp") || rpName.Contains("highdefinition") || rpFullName.Contains("highdefinition"))
                    {
                        srp = "hdrp";
                    }
                }

                var errors = new List<string>();
                if (!string.IsNullOrEmpty(expected) &&
                    !string.Equals(expected, srp, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"SRP mismatch. Expected='{expected}', Actual='{srp}'.");
                }

                if (errors.Count > 0)
                {
                    var msgErr = "Render pipeline validation failed: " + string.Join("; ", errors);
                    CodelyLogger.LogError($"[CodelyValidation] {msgErr}");
                    return new Dictionary<string, object>
                    {
                        ["success"] = false,
                        ["message"] = msgErr,
                        ["data"] = new Dictionary<string, object>
                        {
                            ["srp"] = srp,
                            ["errors"] = errors
                        }
                    };
                }

                var okMsg = $"Render pipeline validation OK. srp='{srp}'.";
                CodelyLogger.Log($"[CodelyValidation] {okMsg}");
                return new Dictionary<string, object>
                {
                    ["success"] = true,
                    ["message"] = okMsg,
                    ["data"] = new Dictionary<string, object>
                    {
                        ["srp"] = srp
                    }
                };
            }
            catch (Exception e)
            {
                var msg = $"Failed to validate render pipeline: {e.Message}";
                CodelyLogger.LogError($"[CodelyValidation] {msg}");
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = msg,
                    ["data"] = new Dictionary<string, object>()
                };
            }
        }

        // =====================================================================
        // Scene Validation Tools
        // =====================================================================

        /// <summary>
        /// Validates the active scene matches expected name and/or path.
        ///
        /// tool_name: "codely.validate_active_scene"
        ///
        /// Parameters:
        /// {
        ///   "expectedName": "SceneName",        // Optional: expected scene name (without .unity)
        ///   "expectedPath": "Assets/Scenes/SceneName.unity"  // Optional: expected full asset path
        /// }
        /// </summary>
        [ExecuteCustomTool.CustomTool("codely.validate_active_scene", "Validate active scene name and path")]
        public static object ValidateActiveScene(JObject parameters)
        {
            var expectedName = parameters?["expectedName"]?.ToString();
            var expectedPath = parameters?["expectedPath"]?.ToString();

            if (string.IsNullOrEmpty(expectedName) && string.IsNullOrEmpty(expectedPath))
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = "At least one of 'expectedName' or 'expectedPath' is required",
                    ["data"] = new Dictionary<string, object>()
                };
            }

            var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = "No valid active scene",
                    ["data"] = new Dictionary<string, object>()
                };
            }

            var errors = new List<string>();
            var actualName = activeScene.name;
            var actualPath = activeScene.path;

            if (!string.IsNullOrEmpty(expectedName) && !string.Equals(expectedName, actualName, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"Scene name mismatch: expected='{expectedName}', actual='{actualName}'");
            }

            if (!string.IsNullOrEmpty(expectedPath) && !string.Equals(expectedPath, actualPath, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"Scene path mismatch: expected='{expectedPath}', actual='{actualPath}'");
            }

            if (errors.Count > 0)
            {
                var msg = string.Join("; ", errors);
                CodelyLogger.LogError($"[CodelyValidation] {msg}");
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = msg,
                    ["data"] = new Dictionary<string, object>
                    {
                        ["expectedName"] = expectedName,
                        ["expectedPath"] = expectedPath,
                        ["actualName"] = actualName,
                        ["actualPath"] = actualPath
                    }
                };
            }

            var okMsg = $"Active scene validation OK: name='{actualName}', path='{actualPath}'";
            CodelyLogger.Log($"[CodelyValidation] {okMsg}");
            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["message"] = okMsg,
                ["data"] = new Dictionary<string, object>
                {
                    ["actualName"] = actualName,
                    ["actualPath"] = actualPath
                }
            };
        }

        /// <summary>
        /// Validates the active scene's dirty state.
        ///
        /// tool_name: "codely.validate_scene_dirty"
        ///
        /// Parameters:
        /// {
        ///   "expectedDirty": true/false
        /// }
        /// </summary>
        [ExecuteCustomTool.CustomTool("codely.validate_scene_dirty", "Validate scene dirty state")]
        public static object ValidateSceneDirty(JObject parameters)
        {
            var expectedDirty = parameters?["expectedDirty"]?.ToObject<bool?>() ?? parameters?["isDirty"]?.ToObject<bool?>();

            if (!expectedDirty.HasValue)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = "Parameter 'expectedDirty' (or 'isDirty') is required",
                    ["data"] = new Dictionary<string, object>()
                };
            }

            var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = "No valid active scene",
                    ["data"] = new Dictionary<string, object>()
                };
            }

            var actualDirty = activeScene.isDirty;

            if (expectedDirty.Value != actualDirty)
            {
                var msg = $"Scene dirty state mismatch: expected={expectedDirty.Value}, actual={actualDirty}";
                CodelyLogger.LogError($"[CodelyValidation] {msg}");
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = msg,
                    ["data"] = new Dictionary<string, object>
                    {
                        ["expectedDirty"] = expectedDirty.Value,
                        ["actualDirty"] = actualDirty,
                        ["sceneName"] = activeScene.name
                    }
                };
            }

            var okMsg = $"Scene dirty state OK: isDirty={actualDirty} (scene='{activeScene.name}')";
            CodelyLogger.Log($"[CodelyValidation] {okMsg}");
            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["message"] = okMsg,
                ["data"] = new Dictionary<string, object>
                {
                    ["isDirty"] = actualDirty,
                    ["sceneName"] = activeScene.name
                }
            };
        }

        /// <summary>
        /// Validates the hierarchy root count of the active scene.
        ///
        /// tool_name: "codely.validate_hierarchy_root_count"
        ///
        /// Parameters:
        /// {
        ///   "minCount": 0,
        ///   "maxCount": 100
        /// }
        /// </summary>
        [ExecuteCustomTool.CustomTool("codely.validate_hierarchy_root_count", "Validate scene hierarchy root count")]
        public static object ValidateHierarchyRootCount(JObject parameters)
        {
            var minCount = parameters?["minCount"]?.ToObject<int?>() ?? 0;
            var maxCount = parameters?["maxCount"]?.ToObject<int?>() ?? int.MaxValue;

            var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid() || !activeScene.isLoaded)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = "No valid and loaded active scene",
                    ["data"] = new Dictionary<string, object>()
                };
            }

            var actualCount = activeScene.rootCount;

            if (actualCount < minCount || actualCount > maxCount)
            {
                var msg = $"Hierarchy root count out of range: expected [{minCount}, {maxCount}], actual={actualCount}";
                CodelyLogger.LogError($"[CodelyValidation] {msg}");
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = msg,
                    ["data"] = new Dictionary<string, object>
                    {
                        ["minCount"] = minCount,
                        ["maxCount"] = maxCount,
                        ["actualCount"] = actualCount
                    }
                };
            }

            var okMsg = $"Hierarchy root count OK: {actualCount} (range [{minCount}, {maxCount}])";
            CodelyLogger.Log($"[CodelyValidation] {okMsg}");
            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["message"] = okMsg,
                ["data"] = new Dictionary<string, object>
                {
                    ["actualCount"] = actualCount
                }
            };
        }

        // =====================================================================
        // GameObject Validation Tools
        // =====================================================================

        /// <summary>
        /// Validates that a GameObject exists in the scene.
        ///
        /// tool_name: "codely.validate_gameobject_exists"
        ///
        /// Parameters:
        /// {
        ///   "name": "GameObjectName",
        ///   "shouldExist": true/false  // default: true
        /// }
        /// </summary>
        [ExecuteCustomTool.CustomTool("codely.validate_gameobject_exists", "Validate GameObject existence")]
        public static object ValidateGameObjectExists(JObject parameters)
        {
            var name = parameters?["name"]?.ToString();
            var shouldExist = parameters?["shouldExist"]?.ToObject<bool?>() ?? true;

            if (string.IsNullOrEmpty(name))
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = "Parameter 'name' is required",
                    ["data"] = new Dictionary<string, object>()
                };
            }

            var go = GameObject.Find(name);
            var exists = go != null;

            if (exists != shouldExist)
            {
                var msg = shouldExist
                    ? $"GameObject '{name}' not found but expected to exist"
                    : $"GameObject '{name}' found but expected NOT to exist";
                CodelyLogger.LogError($"[CodelyValidation] {msg}");
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = msg,
                    ["data"] = new Dictionary<string, object>
                    {
                        ["name"] = name,
                        ["shouldExist"] = shouldExist,
                        ["actuallyExists"] = exists
                    }
                };
            }

            var okMsg = shouldExist
                ? $"GameObject '{name}' exists as expected"
                : $"GameObject '{name}' does not exist as expected";
            CodelyLogger.Log($"[CodelyValidation] {okMsg}");
            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["message"] = okMsg,
                ["data"] = new Dictionary<string, object>
                {
                    ["name"] = name,
                    ["exists"] = exists
                }
            };
        }

        /// <summary>
        /// Cleans up leftover test GameObjects in the active scene by name pattern.
        ///
        /// tool_name: "codely.cleanup_test_objects"
        ///
        /// Parameters:
        /// {
        ///   "namePrefixes"?: ["Test"],                 // 按前缀匹配名称，可选
        ///   "exactNames"?: ["ParentObject", ...],      // 精确名称列表，可选
        ///   "contains"?: ["Prefab"],                   // 按子串匹配名称，可选
        ///   "includeInactive"?: true,                  // 是否包含 inactive 对象，默认 true
        ///   "logOnly"?: false,                         // 仅输出将被删除的对象，不实际删除
        ///   "maxDeleted"?: 500                         // 安全上限，超过则报错避免误删
        /// }
        ///
        /// 如果未提供任何匹配条件，默认使用 namePrefixes = ["Test"] 和 contains = ["Prefab"]，
        /// 以覆盖本测试规范中常见的测试对象命名（Test* / *Prefab*）。
        /// </summary>
        [ExecuteCustomTool.CustomTool("codely.cleanup_test_objects", "Cleanup leftover test GameObjects by name pattern")]
        public static object CleanupTestObjects(JObject parameters)
        {
            // 解析参数
            var namePrefixes = ToStringList(parameters?["namePrefixes"] as JArray);
            var exactNames = ToStringList(parameters?["exactNames"] as JArray);
            var contains = ToStringList(parameters?["contains"] as JArray);
            var includeInactive = parameters?["includeInactive"]?.ToObject<bool?>() ?? true;
            var logOnly = parameters?["logOnly"]?.ToObject<bool?>() ?? false;
            var maxDeleted = parameters?["maxDeleted"]?.ToObject<int?>() ?? 500;

            // 如果没有任何过滤条件，使用一套保守的默认规则
            if (namePrefixes.Count == 0 && exactNames.Count == 0 && contains.Count == 0)
            {
                namePrefixes.Add("Test");
                contains.Add("Prefab");
            }

            var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid() || !activeScene.isLoaded)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = "No valid and loaded active scene for cleanup",
                    ["data"] = new Dictionary<string, object>()
                };
            }

            var toDelete = new HashSet<GameObject>();
            var visited = new HashSet<GameObject>();

            foreach (var root in activeScene.GetRootGameObjects())
            {
                CollectMatchingGameObjectsRecursive(
                    root,
                    namePrefixes,
                    exactNames,
                    contains,
                    includeInactive,
                    toDelete,
                    visited
                );
            }

            var totalCandidates = toDelete.Count;
            if (totalCandidates == 0)
            {
                var msgNone = "No matching test GameObjects found to cleanup.";
                CodelyLogger.Log($"[CodelyCleanup] {msgNone}");
                return new Dictionary<string, object>
                {
                    ["success"] = true,
                    ["message"] = msgNone,
                    ["data"] = new Dictionary<string, object>
                    {
                        ["deletedCount"] = 0,
                        ["candidates"] = new List<string>()
                    }
                };
            }

            if (totalCandidates > maxDeleted)
            {
                var msgTooMany =
                    $"Cleanup would delete {totalCandidates} GameObjects which exceeds safety limit maxDeleted={maxDeleted}. Aborting.";
                CodelyLogger.LogError($"[CodelyCleanup] {msgTooMany}");
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = msgTooMany,
                    ["data"] = new Dictionary<string, object>
                    {
                        ["candidateCount"] = totalCandidates,
                        ["maxDeleted"] = maxDeleted
                    }
                };
            }

            var deletedNames = new List<string>();

            if (logOnly)
            {
                foreach (var go in toDelete)
                {
                    if (go != null)
                    {
                        deletedNames.Add(go.name);
                    }
                }

                var msgLogOnly =
                    $"[Dry Run] Found {deletedNames.Count} GameObjects matching cleanup filters. No objects were deleted.";
                CodelyLogger.Log($"[CodelyCleanup] {msgLogOnly}");
                return new Dictionary<string, object>
                {
                    ["success"] = true,
                    ["message"] = msgLogOnly,
                    ["data"] = new Dictionary<string, object>
                    {
                        ["deletedCount"] = 0,
                        ["candidates"] = deletedNames
                    }
                };
            }

            foreach (var go in toDelete)
            {
                if (go == null) continue;
                deletedNames.Add(go.name);
                Undo.DestroyObjectImmediate(go);
            }

            var summary = $"Deleted {deletedNames.Count} GameObjects by cleanup_test_objects.";
            CodelyLogger.Log($"[CodelyCleanup] {summary}");

            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["message"] = summary,
                ["data"] = new Dictionary<string, object>
                {
                    ["deletedCount"] = deletedNames.Count,
                    ["deletedNames"] = deletedNames
                }
            };
        }

        private static List<string> ToStringList(JArray array)
        {
            var result = new List<string>();
            if (array == null) return result;

            foreach (var item in array)
            {
                if (item.Type == JTokenType.String)
                {
                    var value = item.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        result.Add(value);
                    }
                }
            }

            return result;
        }

        private static void CollectMatchingGameObjectsRecursive(
            GameObject go,
            List<string> namePrefixes,
            List<string> exactNames,
            List<string> contains,
            bool includeInactive,
            HashSet<GameObject> matches,
            HashSet<GameObject> visited
        )
        {
            if (go == null) return;
            if (visited.Contains(go)) return;
            visited.Add(go);

            if (includeInactive || go.activeInHierarchy)
            {
                if (MatchesNameFilters(go.name, namePrefixes, exactNames, contains))
                {
                    matches.Add(go);
                }
            }

            var transform = go.transform;
            var childCount = transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child != null)
                {
                    CollectMatchingGameObjectsRecursive(
                        child.gameObject,
                        namePrefixes,
                        exactNames,
                        contains,
                        includeInactive,
                        matches,
                        visited
                    );
                }
            }
        }

        private static bool MatchesNameFilters(
            string name,
            List<string> namePrefixes,
            List<string> exactNames,
            List<string> contains
        )
        {
            if (string.IsNullOrEmpty(name)) return false;

            foreach (var exact in exactNames)
            {
                if (!string.IsNullOrEmpty(exact) && string.Equals(name, exact, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            foreach (var prefix in namePrefixes)
            {
                if (!string.IsNullOrEmpty(prefix) && name.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            foreach (var part in contains)
            {
                if (!string.IsNullOrEmpty(part) && name.IndexOf(part, StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        // =====================================================================
        // Window Validation Tools
        // =====================================================================

        /// <summary>
        /// Validates that an Editor window is open.
        ///
        /// tool_name: "codely.validate_window_open"
        ///
        /// Parameters:
        /// {
        ///   "windowType": "Console" | "Inspector" | "Hierarchy" | "Project" | "Scene" | "Game",
        ///   "shouldBeOpen": true/false  // default: true
        /// }
        /// </summary>
        [ExecuteCustomTool.CustomTool("codely.validate_window_open", "Validate Editor window is open")]
        public static object ValidateWindowOpen(JObject parameters)
        {
            var windowType = parameters?["windowType"]?.ToString() ?? parameters?["windowTitle"]?.ToString();
            var shouldBeOpen = parameters?["shouldBeOpen"]?.ToObject<bool?>() ?? true;

            if (string.IsNullOrEmpty(windowType))
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = "Parameter 'windowType' is required",
                    ["data"] = new Dictionary<string, object>()
                };
            }

            // Map common names to actual EditorWindow types
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
                { "Profiler", "UnityEditor.ProfilerWindow" }
            };

            bool isOpen = false;
            string actualTypeName = windowType;

            if (typeMap.TryGetValue(windowType, out var fullTypeName))
            {
                actualTypeName = fullTypeName;
            }

            // Check if any window of this type is open
            var allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in allWindows)
            {
                var winTypeName = window.GetType().FullName;
                if (winTypeName.Equals(actualTypeName, StringComparison.OrdinalIgnoreCase) ||
                    winTypeName.EndsWith("." + windowType, StringComparison.OrdinalIgnoreCase) ||
                    window.titleContent.text.Equals(windowType, StringComparison.OrdinalIgnoreCase))
                {
                    isOpen = true;
                    break;
                }
            }

            if (isOpen != shouldBeOpen)
            {
                var msg = shouldBeOpen
                    ? $"Window '{windowType}' is not open but expected to be open"
                    : $"Window '{windowType}' is open but expected to be closed";
                CodelyLogger.LogError($"[CodelyValidation] {msg}");
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["message"] = msg,
                    ["data"] = new Dictionary<string, object>
                    {
                        ["windowType"] = windowType,
                        ["shouldBeOpen"] = shouldBeOpen,
                        ["actuallyOpen"] = isOpen
                    }
                };
            }

            var okMsg = shouldBeOpen
                ? $"Window '{windowType}' is open as expected"
                : $"Window '{windowType}' is closed as expected";
            CodelyLogger.Log($"[CodelyValidation] {okMsg}");
            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["message"] = okMsg,
                ["data"] = new Dictionary<string, object>
                {
                    ["windowType"] = windowType,
                    ["isOpen"] = isOpen
                }
            };
        }
    }
}


