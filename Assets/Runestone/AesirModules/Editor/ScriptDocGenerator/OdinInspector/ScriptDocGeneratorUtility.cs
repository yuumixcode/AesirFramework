using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
using UnityEngine;

[assembly: InternalsVisibleTo("Runestone.AesirModules.Tests.Editor")]

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// 脚本文档生成器逻辑控制类，负责处理文档生成的核心逻辑
    /// </summary>
    public static class ScriptDocGeneratorUtility
    {
        const string IdentifierCn = "## Additional Notes";
        const string NoneAssembly = "None Assembly";
        const string GithubRepository = "https://github.com/yuumixcode/AesirFramework";

        static readonly StringBuilder UserIdentifierDescriptionParagraph = new StringBuilder()
            .AppendLine(IdentifierCn).AppendLine().AppendLine("> 首个 `" + IdentifierCn +
                                                              "` 是增量生成文档标识符，请勿修改标题级别和内容！" +
                                                              "本文档由 [`Script Doc Generator`](" +
                                                              GithubRepository + ") 辅助生成。");

        static readonly IAnalysisDataFactory AnalysisDataFactory = new DefaultAnalysisDataFactory();

        /// <summary>
        /// 检查 Script Doc Generator 是否已初始化。未初始化时提示用户。
        /// </summary>
        /// <returns>true 表示已初始化（或用户刚确认初始化）；false 表示用户取消了初始化。</returns>
        public static bool EnsureInitialized()
        {
            if (ScriptDocGeneratorAssetMarkerSO.IsAssetsInitialized())
            {
                return true;
            }

            var confirmed = EditorUtility.DisplayDialog("Script Doc Generator 提示窗口",
                "未发现 Script Doc Generator 的标识资产，认定 Script Doc Generator 模块的资产尚未生成。\n" +
                "是否立即生成 Script Doc Generator 模块的所有资产？", "立刻生成", "取消");

            if (!confirmed)
            {
                return false;
            }

            InitializeAssets();
            return true;
        }

        public static void InitializeAssets()
        {
            _ = ScriptDocGeneratorPanelSO.Instance;
            _ = DefaultScriptingAPISettingsSO.Instance;

            ScriptDocGeneratorAssetMarkerSO.CreateMarkerAsset();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static ITypeData AnalyzeSingleType(Type targetType)
        {
            if (targetType != null)
            {
                if (TypeAnalyzerUtility.IsGeneratedInternalType(targetType))
                {
                    Debug.LogError("目标类型是编译器或 Unity 生成的内部类型，不支持为其生成文档：" + targetType.FullName);
                    return null;
                }

                // 每次分析前失效源码文档缓存，保证读到磁盘上最新的 XML 注释
                SourceSummaryInitializer.ClearCache();
                return AnalysisDataFactory.CreateTypeData(targetType, AnalysisDataFactory);
            }

            Debug.LogError("请选择有效的目标类型");
            return null;
        }

        public static List<ITypeData> AnalyzeMultipleTypes(List<Type> types)
        {
            if (types is not { Count: > 0 })
            {
                Debug.LogError("设置有效的 Type 对象列表");
                return null;
            }

            types.RemoveAll(x => x == null || TypeAnalyzerUtility.IsGeneratedInternalType(x));
            SourceSummaryInitializer.ClearCache();
            return types.Select(type => AnalysisDataFactory.CreateTypeData(type, AnalysisDataFactory))
                .ToList();
        }

        public static List<ITypeData> AnalyzeMultipleTypes(TypesCacheSO typesCache)
        {
            if (typesCache && typesCache.Types.Count > 0)
            {
                return AnalyzeMultipleTypes(typesCache.Types);
            }

            Debug.LogError("TypesCacheSO 为空或不包含有效的 Type 对象");
            return null;
        }

        public static List<ITypeData> AnalyzeSingleAssembly(string assemblyFullName)
        {
            if (string.IsNullOrEmpty(assemblyFullName) || assemblyFullName == NoneAssembly)
            {
                Debug.LogError("请选择目标程序集，不能为 " + NoneAssembly);
                return null;
            }

            return AnalyzeAssembly(Assembly.Load(assemblyFullName));
        }

        /// <summary>
        /// 分析多个程序集中的所有类型
        /// </summary>
        public static List<ITypeData> AnalyzeMultipleAssemblies(List<string> assemblyFullNames)
        {
            if (assemblyFullNames is not { Count: > 0 })
            {
                Debug.LogError("请选择至少一个目标程序集");
                return null;
            }

            assemblyFullNames.RemoveAll(string.IsNullOrEmpty);
            assemblyFullNames.RemoveAll(name => name == NoneAssembly);

            if (assemblyFullNames.Count == 0)
            {
                Debug.LogError("请选择有效的目标程序集，不能为 " + NoneAssembly);
                return null;
            }

            var result = new List<ITypeData>();
            foreach (var assemblyFullName in assemblyFullNames)
            {
                result.AddRange(AnalyzeAssembly(Assembly.Load(assemblyFullName)));
            }

            return result;
        }

        /// <summary>
        /// 分析单个程序集内的所有可文档化类型（过滤编译器生成类型与引擎生成内部类型）。
        /// 面板与静态 API 共用的分析核心。
        /// </summary>
        internal static List<ITypeData> AnalyzeAssembly(Assembly assembly)
        {
            SourceSummaryInitializer.ClearCache();
            return assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<CompilerGeneratedAttribute>() == null &&
                            !TypeAnalyzerUtility.IsGeneratedInternalType(t)).Select(type =>
                    AnalysisDataFactory.CreateTypeData(type, AnalysisDataFactory)).ToList();
        }

        public static void GenerateSingleTypeDoc(ITypeData typeData,
            DocGeneratorSettingsSO generatorSettings,
            string targetFolderPath)
        {
            if (typeData == null || !generatorSettings || string.IsNullOrEmpty(targetFolderPath))
            {
                Debug.LogError("参数无效，无法生成文档");
                return;
            }

            typeData.TryAsIMemberData(out var memberData);
            if (memberData.IsObsolete &&
                !EditorUtility.DisplayDialog("警告提示", "此类已经被标记为过时，继续生成文档吗？", "确认", "取消"))
            {
                return;
            }

            // 覆盖确认需要目标路径：先做轻量路径计算，再交写入核心（含增量合并）
            var filePath = ComputeTypeDocFilePath(typeData, generatorSettings, targetFolderPath);
            if (File.Exists(filePath) &&
                !EditorUtility.DisplayDialog("提示",
                    "已经存在该文档，继续生成将覆盖部分内容，保留首个 " + IdentifierCn + " 之后的内容，是否继续生成？", "确认", "取消"))
            {
                return;
            }

            var writtenPath = WriteTypeDocSilently(typeData, generatorSettings, targetFolderPath);
            if (writtenPath == null)
            {
                return;
            }

            AssetDatabase.Refresh();
            EditorUtility.OpenWithDefaultApp(writtenPath);
        }

        public static void GenerateMultipleTypeDocs(List<ITypeData> typeDataCollection,
            DocGeneratorSettingsSO generatorSettings,
            string targetFolderPath)
        {
            if (typeDataCollection is not { Count: > 0 } || !generatorSettings ||
                string.IsNullOrEmpty(targetFolderPath))
            {
                Debug.LogError("参数无效，无法生成文档");
                return;
            }

            try
            {
                for (var i = 0; i < typeDataCollection.Count; i++)
                {
                    var typeData = typeDataCollection[i];
                    typeData.TryAsIMemberData(out var memberData);
                    var dataTypeName = memberData.Name;

                    EditorUtility.DisplayProgressBar("脚本文档生成", $"正在生成 {dataTypeName} 文档",
                        (float)i / typeDataCollection.Count);

                    WriteTypeDocSilently(typeData, generatorSettings, targetFolderPath);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.Refresh();
            EditorUtility.OpenWithDefaultApp(targetFolderPath);
        }

        /// <summary>
        /// 静默写入单个类型的文档：组装 Markdown、增量合并（保留 Front Matter 与
        /// <c>## Additional Notes</c> 之后的手写内容）、建目录、写盘。
        /// 无任何确认弹窗、不刷新资源库、不打开生成结果——面板交互流程与静态 API（AI / 自动化调用）共用此核心。
        /// </summary>
        /// <returns>写入的文档文件绝对路径；参数无效时记录错误并返回 null。</returns>
        internal static string WriteTypeDocSilently(ITypeData typeData,
            DocGeneratorSettingsSO generatorSettings,
            string targetFolderPath)
        {
            if (typeData == null || !generatorSettings || string.IsNullOrEmpty(targetFolderPath))
            {
                Debug.LogError("参数无效，无法生成文档");
                return null;
            }

            var markdownText = BuildDocMarkdown(typeData, generatorSettings);
            var filePathWithExtensions = ComputeTypeDocFilePath(typeData, generatorSettings, targetFolderPath);

            if (File.Exists(filePathWithExtensions))
            {
                var readAllLines = File.ReadAllLines(filePathWithExtensions);
                markdownText = MergeFrontMatterWhenMissing(readAllLines, markdownText);

                var additionalDescription = GetAdditionalDescriptionFromExistingFile(readAllLines);
                if (!string.IsNullOrEmpty(additionalDescription))
                {
                    var userIdentifierParagraphString = UserIdentifierDescriptionParagraph.ToString();
                    if (markdownText.Contains(userIdentifierParagraphString))
                    {
                        markdownText = markdownText.Replace(userIdentifierParagraphString,
                            additionalDescription);
                    }
                    else
                    {
                        markdownText += additionalDescription;
                    }
                }
            }

            var directoryPath = Path.GetDirectoryName(filePathWithExtensions);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var utf8WithoutBom = new UTF8Encoding(false);
            File.WriteAllText(filePathWithExtensions, markdownText, utf8WithoutBom);

            return filePathWithExtensions;
        }

        static string GetAdditionalDescriptionFromExistingFile(string[] readAllLines)
        {
            if (readAllLines.Length == 0)
            {
                return string.Empty;
            }

            var identifierIndex = Array.FindIndex(readAllLines, line => line.StartsWith(IdentifierCn));
            if (identifierIndex <= 0)
            {
                return string.Empty;
            }

            var additionalDescriptionStringBuilder = new StringBuilder();
            for (var i = identifierIndex; i < readAllLines.Length; i++)
            {
                additionalDescriptionStringBuilder.AppendLine(readAllLines[i]);
            }

            return additionalDescriptionStringBuilder.ToString();
        }

        /// <summary>
        /// 组装单个类型的完整 Markdown 文本：生成器产出 + 可选的增量标识符段。
        /// </summary>
        static string BuildDocMarkdown(ITypeData typeData, DocGeneratorSettingsSO generatorSettings)
        {
            var markdownText = generatorSettings.GetGeneratedDocumentation(typeData);

            if (generatorSettings.generateIdentifier)
            {
                markdownText = markdownText.EndsWith('\n') || markdownText.EndsWith("\r\n")
                    ? markdownText + UserIdentifierDescriptionParagraph
                    : markdownText + ("\n" + UserIdentifierDescriptionParagraph);
            }

            return markdownText;
        }

        /// <summary>
        /// 计算单个类型的目标文档路径（含 generateNamespaceFolder 的命名空间子目录与扩展名）。
        /// 面板的"已存在覆盖确认"需要在写入前拿到路径，故与写入核心共用本计算。
        /// </summary>
        internal static string ComputeTypeDocFilePath(ITypeData typeData,
            DocGeneratorSettingsSO generatorSettings,
            string targetFolderPath)
        {
            typeData.TryAsIMemberData(out var memberData);

            if (generatorSettings.generateNamespaceFolder)
            {
                var namespaceString = typeData.NamespaceName;
                if (!string.IsNullOrEmpty(namespaceString))
                {
                    var namespaceFolders = namespaceString.Split('.');
                    targetFolderPath = namespaceFolders.Aggregate(targetFolderPath, Path.Combine);
                }
                else
                {
                    targetFolderPath = Path.Combine(targetFolderPath, "WithoutNamespace");
                }

                Directory.CreateDirectory(targetFolderPath);
            }

            var fileNameWithoutExtension =
                TypeAnalyzerUtility.ConvertToDocumentationFileName(memberData.Name);
            var filePathWithExtensions = Path.Combine(targetFolderPath, fileNameWithoutExtension);

            if (generatorSettings.customizeDocFileExtensionName)
            {
                var ext = generatorSettings.docFileExtensionName;
                filePathWithExtensions += ext.StartsWith(".") ? ext : "." + ext;
            }
            else
            {
                filePathWithExtensions += ".md";
            }

            return filePathWithExtensions;
        }

        /// <summary>
        /// 增量生成时的 Front Matter 合并：旧文件存在 Front Matter 且新内容未自带时拼回旧头部，
        /// 新内容自带 Front Matter（如 Zensical 生成器自产 YAML 头）时以新生成的为准，避免产生双重头部。
        /// </summary>
        internal static string MergeFrontMatterWhenMissing(string[] existingLines, string markdownText) =>
            !HasFrontMatter(markdownText) && TryGetFrontMatter(existingLines, out var frontMatter)
                ? frontMatter + markdownText
                : markdownText;

        /// <summary>文本是否自带 Front Matter 头（以 --- 或 +++ 起始）。</summary>
        static bool HasFrontMatter(string text) =>
            text.StartsWith("---", StringComparison.Ordinal) ||
            text.StartsWith("+++", StringComparison.Ordinal);

        static bool TryGetFrontMatter(string[] sourceLines, out string frontMatter)
        {
            var frontMatterStringBuilder = new StringBuilder();

            // Front Matter 必须以 --- 或 +++ 开头且在文件内闭合；无闭合分隔符视为无 Front Matter，
            // 避免把整个旧文件误当头部拼回
            if (sourceLines.Length == 0 || (sourceLines[0] != "---" && sourceLines[0] != "+++"))
            {
                frontMatter = string.Empty;
                return false;
            }

            frontMatterStringBuilder.AppendLine(sourceLines[0]);

            for (var i = 1; i < sourceLines.Length; i++)
            {
                frontMatterStringBuilder.AppendLine(sourceLines[i]);

                if ((sourceLines[0] == "---" && sourceLines[i] == "---") ||
                    (sourceLines[0] == "+++" && sourceLines[i] == "+++"))
                {
                    frontMatterStringBuilder.AppendLine();
                    frontMatter = frontMatterStringBuilder.ToString();
                    return true;
                }
            }

            frontMatter = string.Empty;
            return false;
        }
    }
}
