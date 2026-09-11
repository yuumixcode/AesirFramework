#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// 项目级类型声明索引：类型名 → 声明所在文件路径列表，附带 文件 → 命名空间集合。
    /// 惰性构建，每个域重载周期至多一次全项目扫描（历史实现按"每个找不到源文件的类型"
    /// 全项目扫描一次，N 个外部类型即 N 次全扫，此索引将其收敛为 1 次）。
    /// 类型声明检测复用 <see cref="SourceScanner" /> 的字符串/注释感知净化，
    /// 注释与字符串里的假类型声明不会进入索引。索引只存路径与名称，不驻留文件内容。
    /// </summary>
    internal static class ProjectScriptIndex
    {
        static Dictionary<string, List<string>> _typePaths;
        static Dictionary<string, HashSet<string>> _fileNamespaces;
        static bool _buildFailed;

        /// <summary>
        /// 查询声明了指定类型名的文件路径列表。索引未构建时先构建（全项目一次）。
        /// </summary>
        public static bool TryGetTypePaths(string typeName, out List<string> paths)
        {
            paths = null;
            if (string.IsNullOrEmpty(typeName))
            {
                return false;
            }

            EnsureBuilt();
            return _typePaths != null && _typePaths.TryGetValue(typeName, out paths);
        }

        /// <summary>
        /// 文件是否声明了指定命名空间。用于按期望命名空间过滤候选文件，
        /// 排除其他命名空间中的同名类型。
        /// </summary>
        public static bool FileDeclaresNamespace(string path, string namespaceName)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(namespaceName))
            {
                return true;
            }

            return _fileNamespaces != null && _fileNamespaces.TryGetValue(path, out var namespaces) &&
                   namespaces.Contains(namespaceName);
        }

        static void EnsureBuilt()
        {
            if (_typePaths != null || _buildFailed)
            {
                return;
            }

            var typePaths = new Dictionary<string, List<string>>();
            var fileNamespaces = new Dictionary<string, HashSet<string>>();
            try
            {
                foreach (var guid in AssetDatabase.FindAssets("t:MonoScript"))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(path) ||
                        !path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string[] lines;
                    try
                    {
                        var fullPath = Path.GetFullPath(path);
                        if (!File.Exists(fullPath))
                        {
                            continue;
                        }

                        lines = File.ReadAllLines(fullPath);
                    }
                    catch
                    {
                        continue;
                    }

                    // 仅收集类型/命名空间声明，跳过 /// 文档解析
                    var doc = SourceScanner.Scan(lines, false);
                    fileNamespaces[path] = doc.DeclaredNamespaces;
                    foreach (var typeName in doc.DeclaredTypeNames)
                    {
                        if (!typePaths.TryGetValue(typeName, out var list))
                        {
                            typePaths[typeName] = list = new List<string>();
                        }

                        if (!list.Contains(path))
                        {
                            list.Add(path);
                        }
                    }
                }
            }
            catch
            {
                // 构建中断：保留已完成部分（部分可用优于全无），本域内不再重试
                _buildFailed = true;
            }

            _typePaths = typePaths;
            _fileNamespaces = fileNamespaces;
        }
    }
}
#endif
