using System;
using System.Collections.Generic;
using System.Reflection;
using Codely.Newtonsoft.Json.Linq;
using UnityEngine;
using UnityTcp.Editor.Helpers;

namespace UnityTcp.Editor.Tools
{
    /// <summary>
    /// Executes custom tools registered in the Unity project.
    /// Custom tools must be static methods with a specific signature:
    /// public static object ToolName(JObject parameters)
    /// 
    /// Tools can be registered via the [CustomTool] attribute or
    /// by convention in the CustomToolsRegistry static class.
    /// </summary>
    public static class ExecuteCustomTool
    {
        // Registry of custom tools: tool_name -> method info
        private static readonly Dictionary<string, MethodInfo> _registeredTools = new Dictionary<string, MethodInfo>();
        private static bool _initialized = false;
        private static readonly object _initLock = new object();

        /// <summary>
        /// Attribute to mark a method as a custom tool.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public class CustomToolAttribute : Attribute
        {
            public string Name { get; }
            public string Description { get; }

            public CustomToolAttribute(string name, string description = null)
            {
                Name = name;
                Description = description;
            }
        }

        /// <summary>
        /// Main handler for executing custom tools.
        /// </summary>
        public static object HandleCommand(JObject @params)
        {
            if (@params == null)
            {
                return Response.Error("Parameters cannot be null.");
            }

            // Ensure tools are discovered
            EnsureInitialized();

            string toolName = @params["tool_name"]?.ToString();
            if (string.IsNullOrEmpty(toolName))
            {
                return Response.Error("'tool_name' parameter is required.");
            }

            JToken rawToolParams = @params["parameters"];
            JObject toolParams;
            if (rawToolParams == null || rawToolParams.Type == JTokenType.Null)
            {
                toolParams = new JObject();
            }
            else if (rawToolParams.Type == JTokenType.Object)
            {
                toolParams = (JObject)rawToolParams;
            }
            else
            {
                // Be forgiving with model/runtime mistakes. Custom tools receive
                // an empty object instead of failing the whole tool call.
                toolParams = new JObject();
            }

            try
            {
                // Check if tool exists
                if (!_registeredTools.TryGetValue(toolName, out MethodInfo method))
                {
                    // Try case-insensitive lookup
                    method = FindToolCaseInsensitive(toolName);
                    if (method == null)
                    {
                        return Response.Error($"Custom tool '{toolName}' not found. Available tools: {string.Join(", ", _registeredTools.Keys)}");
                    }
                }

                // Execute the tool
                CodelyLogger.Log($"[ExecuteCustomTool] Executing custom tool: {toolName}");
                var result = method.Invoke(null, new object[] { toolParams });

                // Wrap result in standard response format if not already
                if (result is Dictionary<string, object> dictResult && dictResult.ContainsKey("success"))
                {
                    // Already in standard format
                    return result;
                }

                // Wrap in success response
                return new
                {
                    success = true,
                    message = $"Custom tool '{toolName}' executed successfully.",
                    data = result
                };
            }
            catch (TargetInvocationException tie)
            {
                // Unwrap the inner exception
                var innerEx = tie.InnerException ?? tie;
                CodelyLogger.LogError($"[ExecuteCustomTool] Tool '{toolName}' failed: {innerEx}");
                return Response.Error($"Custom tool '{toolName}' execution failed: {innerEx.Message}");
            }
            catch (Exception e)
            {
                CodelyLogger.LogError($"[ExecuteCustomTool] Error executing tool '{toolName}': {e}");
                return Response.Error($"Error executing custom tool '{toolName}': {e.Message}");
            }
        }

        /// <summary>
        /// Registers a custom tool manually.
        /// </summary>
        public static void RegisterTool(string name, MethodInfo method)
        {
            lock (_initLock)
            {
                if (_registeredTools.ContainsKey(name))
                {
                    CodelyLogger.LogWarning($"[ExecuteCustomTool] Tool '{name}' is already registered. Overwriting.");
                }
                _registeredTools[name] = method;
                CodelyLogger.Log($"[ExecuteCustomTool] Registered custom tool: {name}");
            }
        }

        /// <summary>
        /// Lists all registered custom tools.
        /// </summary>
        public static IEnumerable<string> GetRegisteredTools()
        {
            EnsureInitialized();
            return _registeredTools.Keys;
        }

        /// <summary>
        /// Discovers and registers all custom tools in the project.
        /// </summary>
        private static void EnsureInitialized()
        {
            lock (_initLock)
            {
                if (_initialized)
                    return;

                try
                {
                    // Scan all assemblies for methods with [CustomTool] attribute
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        // Skip system assemblies for performance, but keep UnityTcp assemblies
                        var assemblyName = assembly.GetName().Name;
                        if (assemblyName.StartsWith("UnityTcp"))
                        {
                            // Always scan our own assemblies
                        }
                        else if (assemblyName.StartsWith("System") ||
                            assemblyName.StartsWith("mscorlib") ||
                            assemblyName.StartsWith("Unity") ||
                            assemblyName.StartsWith("Newtonsoft") ||
                            assemblyName.StartsWith("netstandard") ||
                            assemblyName.StartsWith("Microsoft"))
                            continue;

                        try
                        {
                            foreach (var type in assembly.GetTypes())
                            {
                                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                                {
                                    var attr = method.GetCustomAttribute<CustomToolAttribute>();
                                    if (attr != null)
                                    {
                                        // Validate signature
                                        var parameters = method.GetParameters();
                                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(JObject))
                                        {
                                            _registeredTools[attr.Name] = method;
                                            CodelyLogger.Log($"[ExecuteCustomTool] Discovered custom tool: {attr.Name} ({type.FullName}.{method.Name})");
                                        }
                                        else
                                        {
                                            CodelyLogger.LogWarning($"[ExecuteCustomTool] Invalid signature for tool '{attr.Name}'. Expected: public static object ToolName(JObject parameters)");
                                        }
                                    }
                                }
                            }
                        }
                        catch (ReflectionTypeLoadException)
                        {
                            // Ignore assembly load errors
                        }
                    }

                    CodelyLogger.Log($"[ExecuteCustomTool] Initialization complete. {_registeredTools.Count} custom tools registered.");
                }
                catch (Exception e)
                {
                    CodelyLogger.LogError($"[ExecuteCustomTool] Failed to initialize: {e}");
                }
                finally
                {
                    _initialized = true;
                }
            }
        }

        /// <summary>
        /// Finds a tool by name (case-insensitive).
        /// </summary>
        private static MethodInfo FindToolCaseInsensitive(string toolName)
        {
            foreach (var kvp in _registeredTools)
            {
                if (string.Equals(kvp.Key, toolName, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }
            return null;
        }
    }
}

