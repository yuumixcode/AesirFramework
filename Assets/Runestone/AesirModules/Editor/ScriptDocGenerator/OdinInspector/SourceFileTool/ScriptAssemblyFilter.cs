#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor.Compilation;
using Assembly = System.Reflection.Assembly;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// 脚本程序集过滤器。通过 <see cref="CompilationPipeline" /> 缓存本项目的脚本程序集名集合，
    /// 用于在查找源文件前拦截引擎模块、预编译 DLL 等不可能存在项目源码的类型，
    /// 避免其触发昂贵的项目级内容扫描（历史上这是编辑器卡顿的主要来源）。
    /// 无法取得程序集清单时放行（fail-open），保持无过滤时的旧行为。
    /// </summary>
    internal static class ScriptAssemblyFilter
    {
        static HashSet<string> _scriptAssemblyNames;
        static bool _initializationFailed;

        /// <summary>
        /// 类型是否属于本项目编译产物的脚本程序集（含 Packages 源码程序集）。
        /// 引擎模块与预编译 DLL 类型返回 false——它们不存在项目源文件。
        /// </summary>
        public static bool IsScriptAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                return false;
            }

            try
            {
                EnsureInitialized();
                return _scriptAssemblyNames == null || _scriptAssemblyNames.Count == 0 ||
                       _scriptAssemblyNames.Contains(assembly.GetName().Name);
            }
            catch
            {
                // 动态程序集等 GetName 异常时放行
                return true;
            }
        }

        static void EnsureInitialized()
        {
            if (_scriptAssemblyNames != null || _initializationFailed)
            {
                return;
            }

            try
            {
                _scriptAssemblyNames = new HashSet<string>(StringComparer.Ordinal);
                foreach (var assembly in CompilationPipeline.GetAssemblies())
                {
                    _scriptAssemblyNames.Add(assembly.name);
                }
            }
            catch
            {
                _initializationFailed = true;
            }
        }
    }
}
#endif
