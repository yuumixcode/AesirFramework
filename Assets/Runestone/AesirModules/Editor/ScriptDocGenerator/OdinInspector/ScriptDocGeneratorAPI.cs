using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// 脚本文档生成器静态 API —— 面板（Tools → Aesir → Modules → Script Doc Generator）的无 UI 等价入口，
    /// 面向自动化脚本与 AI 助手直接调用：不需要打开面板、不需要任何点击。
    /// </summary>
    /// <remarks>
    /// <para>与面板共用同一套分析与写入核心（<see cref="ScriptDocGeneratorUtility" />），
    /// 但全程无确认弹窗、不自动打开生成结果。覆盖语义与面板"多程序集模式"一致：
    /// 已存在的文档按增量规则合并（保留 Front Matter 与 <c>## Additional Notes</c> 之后的手写内容），未存在则新建。</para>
    /// <para>调用示例（AI / 自动化）：
    /// <br />- 为程序集生成 Zensical 文档：<c>ScriptDocGeneratorAPI.GenerateDocsForAssembly("Runestone.AesirModules", ScriptDocGeneratorAPI.ZensicalSettings)</c>
    /// <br />- 为文件夹内全部脚本生成默认文档：<c>ScriptDocGeneratorAPI.GenerateDocsForFolder("Assets/Runestone/AesirModules/Runtime/Scene")</c>
    /// <br />- 为单个类型生成文档：<c>ScriptDocGeneratorAPI.GenerateDocsForType(typeof(SomeClass))</c></para>
    /// <para>settings 传 null 使用 <see cref="DefaultSettings" />；outputFolder 传 null 使用
    /// <see cref="DefaultOutputFolder" />（项目根 <c>ScriptDocGenerator/</c>，Assets 外不产生 .meta）。
    /// 自定义生成器：继承 <see cref="DocGeneratorSettingsSO" /> 创建设置资产后传入，
    /// 或经 <see cref="FindAllSettings" /> 枚举项目内已有设置资产。</para>
    /// </remarks>
    public static class ScriptDocGeneratorAPI
    {
        /// <summary>内置中文 API 文档生成设置（与面板同款预设资产）。</summary>
        public static DocGeneratorSettingsSO DefaultSettings => DefaultScriptingAPISettingsSO.Instance;

        /// <summary>内置 Zensical 静态站点文档生成设置（与面板同款预设资产）。</summary>
        public static DocGeneratorSettingsSO ZensicalSettings => ZensicalScriptingAPISettingsSO.Instance;

        /// <summary>默认输出根目录：项目根 <c>ScriptDocGenerator/</c>（Assets 外，不为生成文档产生 .meta）。</summary>
        public static string DefaultOutputFolder => ScriptDocGeneratorPaths.DefaultDocFolderPath;

        /// <summary>
        /// 枚举项目内全部文档生成器设置资产：含两个内置预设（Default / Zensical）与
        /// 用户自建的 <see cref="DocGeneratorSettingsSO" /> 派生资产，按资产名排序。
        /// </summary>
        public static List<DocGeneratorSettingsSO> FindAllSettings() =>
            AssetDatabase.FindAssets("t:" + nameof(DocGeneratorSettingsSO))
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                .Select(AssetDatabase.LoadAssetAtPath<DocGeneratorSettingsSO>)
                .Where(settings => settings)
                .OrderBy(settings => settings.name, StringComparer.Ordinal)
                .ToList();

        /// <summary>
        /// 为单个类型生成脚本文档。
        /// </summary>
        /// <param name="type">目标类型；null 或编译器/引擎生成内部类型时记录错误并返回空结果。</param>
        /// <param name="settings">文档生成器设置；null 使用 <see cref="DefaultSettings" />。</param>
        /// <param name="outputFolder">输出根目录（绝对路径或项目内相对路径均可）；null 使用 <see cref="DefaultOutputFolder" />。</param>
        public static ScriptDocGenerationResult GenerateDocsForType(Type type,
            DocGeneratorSettingsSO settings = null,
            string outputFolder = null)
        {
            var generatorSettings = ResolveSettings(settings);
            var folder = ResolveOutputFolder(outputFolder);
            var result = NewResult(generatorSettings, folder);

            var typeData = ScriptDocGeneratorUtility.AnalyzeSingleType(type);
            if (typeData == null)
            {
                // AnalyzeSingleType 已记录具体错误原因
                return result;
            }

            AppendGeneratedFile(result,
                ScriptDocGeneratorUtility.WriteTypeDocSilently(typeData, generatorSettings, folder));
            FinalizeResult(result);
            return result;
        }

        /// <summary>
        /// 为一组类型生成脚本文档（等价面板"多类型模式"的静默版）。
        /// </summary>
        /// <param name="types">目标类型集合；空集合记录错误并返回空结果。</param>
        /// <param name="settings">文档生成器设置；null 使用 <see cref="DefaultSettings" />。</param>
        /// <param name="outputFolder">输出根目录；null 使用 <see cref="DefaultOutputFolder" />。</param>
        public static ScriptDocGenerationResult GenerateDocsForTypes(IEnumerable<Type> types,
            DocGeneratorSettingsSO settings = null,
            string outputFolder = null)
        {
            var generatorSettings = ResolveSettings(settings);
            var folder = ResolveOutputFolder(outputFolder);
            var result = NewResult(generatorSettings, folder);

            var typeList = types?.Where(type => type != null).Distinct().ToList();
            if (typeList is not { Count: > 0 })
            {
                Debug.LogError("[ScriptDocGeneratorAPI] 类型列表为空，无法生成文档");
                return result;
            }

            var typeDataList = ScriptDocGeneratorUtility.AnalyzeMultipleTypes(typeList);
            if (typeDataList is not { Count: > 0 })
            {
                // AnalyzeMultipleTypes 已记录具体错误原因
                return result;
            }

            GenerateCore(result, typeDataList, generatorSettings, folder);
            return result;
        }

        /// <summary>
        /// 为一个程序集内的全部类型生成脚本文档（等价面板"单程序集模式"的静默版）。
        /// </summary>
        /// <param name="assemblyName">
        /// 程序集名：支持短名（如 <c>Runestone.AesirModules</c>，面板下拉同款）或
        /// FullName（如 <c>Runestone.AesirModules, Version=0.0.0.0, ...</c>）。
        /// </param>
        /// <param name="settings">文档生成器设置；null 使用 <see cref="DefaultSettings" />。</param>
        /// <param name="outputFolder">输出根目录；null 使用 <see cref="DefaultOutputFolder" />。</param>
        public static ScriptDocGenerationResult GenerateDocsForAssembly(string assemblyName,
            DocGeneratorSettingsSO settings = null,
            string outputFolder = null)
        {
            var generatorSettings = ResolveSettings(settings);
            var folder = ResolveOutputFolder(outputFolder);
            var result = NewResult(generatorSettings, folder);

            var assembly = ResolveScriptAssembly(assemblyName);
            if (assembly == null)
            {
                Debug.LogError("[ScriptDocGeneratorAPI] 找不到目标程序集 '" + assemblyName +
                               "'（支持程序集短名或 FullName）");
                return result;
            }

            var typeDataList = ScriptDocGeneratorUtility.AnalyzeAssembly(assembly);
            if (typeDataList is not { Count: > 0 })
            {
                Debug.LogWarning("[ScriptDocGeneratorAPI] 程序集 " + assemblyName + " 内没有可文档化类型");
                return result;
            }

            GenerateCore(result, typeDataList, generatorSettings, folder);
            return result;
        }

        /// <summary>
        /// 为一个文件夹（含子文件夹）内全部脚本声明的类型生成脚本文档。
        /// 源文件经 <see cref="SourceScanner" /> 解析出类型与命名空间声明后映射到当前编译域内的类型，
        /// 支持普通 C# 类（不限 MonoBehaviour / ScriptableObject）；解析不到的类型名记入
        /// <see cref="ScriptDocGenerationResult.UnresolvedTypeNames" />（典型成因：被条件编译剔除）。
        /// </summary>
        /// <param name="folderPath">
        /// 项目内文件夹路径：Assets / Packages 下的相对路径（如 <c>Assets/Runestone/AesirModules/Runtime/Scene</c>）
        /// 或项目内绝对路径。项目外路径 AssetDatabase 索引不到，会被拒绝。
        /// </param>
        /// <param name="settings">文档生成器设置；null 使用 <see cref="DefaultSettings" />。</param>
        /// <param name="outputFolder">输出根目录；null 使用 <see cref="DefaultOutputFolder" />。</param>
        public static ScriptDocGenerationResult GenerateDocsForFolder(string folderPath,
            DocGeneratorSettingsSO settings = null,
            string outputFolder = null)
        {
            var generatorSettings = ResolveSettings(settings);
            var folder = ResolveOutputFolder(outputFolder);
            var result = NewResult(generatorSettings, folder);

            if (!TryNormalizeAssetFolderPath(folderPath, out var normalizedFolder))
            {
                Debug.LogError("[ScriptDocGeneratorAPI] 无效的脚本文件夹路径 '" + folderPath +
                               "'：需要项目内 Assets/ 或 Packages/ 下的真实文件夹（支持绝对路径）");
                return result;
            }

            var types = ResolveTypesInFolder(normalizedFolder, result);
            if (types is not { Count: > 0 })
            {
                var hint = result.UnresolvedTypeNames.Count > 0
                    ? "（存在声明了但当前编译域内不存在的类型，可能被条件编译剔除）"
                    : string.Empty;
                Debug.LogWarning("[ScriptDocGeneratorAPI] 文件夹 " + normalizedFolder +
                                 " 内没有解析到可生成文档的类型" + hint);
                return result;
            }

            var typeDataList = ScriptDocGeneratorUtility.AnalyzeMultipleTypes(types);
            if (typeDataList is not { Count: > 0 })
            {
                return result;
            }

            GenerateCore(result, typeDataList, generatorSettings, folder);
            return result;
        }

        /// <summary>
        /// 批量写入核心：逐类型静默写入，单个类型失败不中断批量（失败原因已记录）。
        /// </summary>
        static void GenerateCore(ScriptDocGenerationResult result,
            List<ITypeData> typeDataList,
            DocGeneratorSettingsSO generatorSettings,
            string folder)
        {
            foreach (var typeData in typeDataList)
            {
                AppendGeneratedFile(result,
                    ScriptDocGeneratorUtility.WriteTypeDocSilently(typeData, generatorSettings, folder));
            }

            FinalizeResult(result);
        }

        static void AppendGeneratedFile(ScriptDocGenerationResult result, string writtenPath)
        {
            if (!string.IsNullOrEmpty(writtenPath))
            {
                result.GeneratedFiles.Add(writtenPath);
            }
        }

        static void FinalizeResult(ScriptDocGenerationResult result)
        {
            if (result.GeneratedFiles.Count > 0)
            {
                AssetDatabase.Refresh();
            }
        }

        static DocGeneratorSettingsSO ResolveSettings(DocGeneratorSettingsSO settings) =>
            settings ? settings : DefaultScriptingAPISettingsSO.Instance;

        static string ResolveOutputFolder(string outputFolder)
        {
            var folder = string.IsNullOrEmpty(outputFolder)
                ? ScriptDocGeneratorPaths.DefaultDocFolderPath
                : outputFolder;
            ScriptDocGeneratorEditorUtility.EnsureDirectoryExists(folder);
            return folder;
        }

        static ScriptDocGenerationResult NewResult(DocGeneratorSettingsSO generatorSettings, string folder) =>
            new ScriptDocGenerationResult
            {
                SettingsName = generatorSettings
                    ? generatorSettings.name + " (" + generatorSettings.GetType().Name + ")"
                    : "<null>",
                OutputFolder = folder
            };

        /// <summary>
        /// 按短名（优先）或 FullName 在当前编译域内解析程序集；找不到返回 null。
        /// </summary>
        internal static Assembly ResolveScriptAssembly(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
            {
                return null;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // 短名精确匹配（面板下拉展示的就是短名）
            var byShortName = assemblies.FirstOrDefault(assembly =>
                string.Equals(assembly.GetName().Name, assemblyName, StringComparison.Ordinal));
            if (byShortName != null)
            {
                return byShortName;
            }

            // FullName 兜底（面板序列化保存的是 FullName）
            return assemblies.FirstOrDefault(assembly =>
                string.Equals(assembly.FullName, assemblyName, StringComparison.Ordinal));
        }

        /// <summary>
        /// 归一化脚本文件夹路径为 AssetDatabase 可见的相对路径（Assets/ 或 Packages/ 开头）。
        /// 绝对路径仅接受项目内路径；目录不存在视为无效。
        /// </summary>
        internal static bool TryNormalizeAssetFolderPath(string inputPath, out string normalizedPath)
        {
            normalizedPath = null;
            if (string.IsNullOrWhiteSpace(inputPath))
            {
                return false;
            }

            var path = inputPath.Trim().Replace('\\', '/').TrimEnd('/');
            if (path.Length == 0)
            {
                return false;
            }

            if (IsAssetIndexPath(path))
            {
                normalizedPath = path;
                return Directory.Exists(path);
            }

            if (!Path.IsPathRooted(path))
            {
                return false;
            }

            // AssetDatabase 只索引 Assets 与 Packages，项目外绝对路径直接拒绝
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (projectRoot == null)
            {
                return false;
            }

            var normalizedRoot = projectRoot.Replace('\\', '/').TrimEnd('/');
            if (!path.StartsWith(normalizedRoot + "/", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var relative = path.Substring(normalizedRoot.Length + 1);
            if (!IsAssetIndexPath(relative))
            {
                return false;
            }

            normalizedPath = relative;
            return Directory.Exists(path);
        }

        static bool IsAssetIndexPath(string path) =>
            path.Equals("Assets", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("Packages", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 解析文件夹（含子文件夹）内全部脚本声明的类型：
        /// 源码扫描（<see cref="SourceScanner" />，仅收集声明，不解析文档注释）得到类型名与命名空间，
        /// 再经当前编译域内脚本程序集的简单名索引映射到 Type——普通 C# 类不依赖 MonoScript.GetClass()。
        /// 解析不到的类型名记入 result 的 UnresolvedTypeNames。
        /// </summary>
        internal static List<Type> ResolveTypesInFolder(string assetFolderPath,
            ScriptDocGenerationResult result)
        {
            var resolvedTypes = new List<Type>();
            var typeIndex = BuildSimpleNameTypeIndex();
            var reportedUnresolved = new HashSet<string>(StringComparer.Ordinal);

            foreach (var guid in AssetDatabase.FindAssets("t:MonoScript", new[] { assetFolderPath }))
            {
                var scriptPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(scriptPath) ||
                    !scriptPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string[] lines;
                try
                {
                    lines = File.ReadAllLines(scriptPath);
                }
                catch
                {
                    // 单个文件读取失败（被占用/权限）不阻塞整个文件夹的解析
                    continue;
                }

                var parsedDoc = SourceScanner.Scan(lines, false);
                if (parsedDoc.DeclaredTypeNames.Count == 0)
                {
                    continue;
                }

                foreach (var typeName in parsedDoc.DeclaredTypeNames)
                {
                    if (!typeIndex.TryGetValue(typeName, out var candidates))
                    {
                        if (reportedUnresolved.Add(typeName))
                        {
                            result.UnresolvedTypeNames.Add(typeName);
                        }

                        continue;
                    }

                    // 按文件声明的命名空间过滤，排除其他命名空间中的同名类型；
                    // 嵌套类型的 Namespace 与外层一致，天然命中
                    resolvedTypes.AddRange(candidates.Where(candidate =>
                        NamespaceMatchesFile(candidate, parsedDoc.DeclaredNamespaces)));
                }
            }

            // partial 类型会在多个文件重复声明，去重后返回
            return resolvedTypes.Distinct().ToList();
        }

        /// <summary>
        /// 当前编译域内全部脚本程序集（含 Packages 源码程序集）的简单名 → 类型索引。
        /// 泛型声明名对齐源码写法：Foo`1 以 Foo 入键。
        /// </summary>
        static Dictionary<string, List<Type>> BuildSimpleNameTypeIndex()
        {
            var index = new Dictionary<string, List<Type>>(StringComparer.Ordinal);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!ScriptAssemblyFilter.IsScriptAssembly(assembly))
                {
                    continue;
                }

                Type[] assemblyTypes;
                try
                {
                    assemblyTypes = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    // 个别程序集类型加载不全时跳过，不阻塞整体索引构建
                    continue;
                }

                foreach (var type in assemblyTypes)
                {
                    var simpleName = type.Name;
                    var arityIndex = simpleName.IndexOf('`');
                    if (arityIndex >= 0)
                    {
                        simpleName = simpleName.Substring(0, arityIndex);
                    }

                    if (!index.TryGetValue(simpleName, out var list))
                    {
                        index[simpleName] = list = new List<Type>();
                    }

                    list.Add(type);
                }
            }

            return index;
        }

        static bool NamespaceMatchesFile(Type candidate, HashSet<string> fileDeclaredNamespaces) =>
            string.IsNullOrEmpty(candidate.Namespace)
                ? fileDeclaredNamespaces.Count == 0
                : fileDeclaredNamespaces.Contains(candidate.Namespace);
    }
}
