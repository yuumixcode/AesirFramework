using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 脚本文档生成器逻辑控制类，负责处理文档生成的核心逻辑
    /// </summary>
    public static class ScriptDocGeneratorUtility
    {
        const string IdentifierCn = "## Additional Notes";
        const string NoneAssembly = "None Assembly";
        const string GithubRepository = "https://github.com/yuumixcode/Unity-Aesir-Packages";

        static readonly StringBuilder UserIdentifierDescriptionParagraph = new StringBuilder()
            .AppendLine(IdentifierCn).AppendLine().AppendLine("> 首个 `" + IdentifierCn +
                                                              "` 是增量生成文档标识符，请勿修改标题级别和内容！" +
                                                              "本文档由 [`Aesir Inspector`](" + GithubRepository +
                                                              ") 辅助生成。");

        static readonly IAnalysisDataFactory AnalysisDataFactory = new DefaultAnalysisDataFactory();

        /// <summary>
        /// 检查 Script Doc Generator 是否已初始化。未初始化时提示用户。
        /// </summary>
        /// <returns>true 表示已初始化（或用户刚确认初始化）；false 表示用户取消了初始化。</returns>
        public static bool EnsureInitialized()
        {
            if (AesirInspectorModuleAssetMarkerSO.IsScriptDocGeneratorAssetsInitialized())
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

            AesirInspectorModuleAssetMarkerSO.CreateScriptDocGeneratorMarkerAsset();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static ITypeData AnalyzeSingleType(Type targetType)
        {
            if (targetType != null)
            {
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

            types.RemoveAll(x => x == null);
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

            var targetAssembly = Assembly.Load(assemblyFullName);

            return targetAssembly.GetTypes()
                .Where(t => t.GetCustomAttribute<CompilerGeneratedAttribute>() == null).Select(type =>
                    AnalysisDataFactory.CreateTypeData(type, AnalysisDataFactory)).ToList();
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
                var assembly = Assembly.Load(assemblyFullName);
                var types = assembly.GetTypes()
                    .Where(t => t.GetCustomAttribute<CompilerGeneratedAttribute>() == null).Select(type =>
                        AnalysisDataFactory.CreateTypeData(type, AnalysisDataFactory));
                result.AddRange(types);
            }

            return result;
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

            ReadDocGeneratorSettingSO(typeData, generatorSettings, targetFolderPath, memberData,
                out var markdownText, out var filePathWithExtensions);

            if (File.Exists(filePathWithExtensions))
            {
                if (!EditorUtility.DisplayDialog("提示",
                        "已经存在该文档，继续生成将覆盖部分内容，保留首个 " + IdentifierCn + " 之后的内容，是否继续生成？", "确认", "取消"))
                {
                    return;
                }

                var readAllLines = File.ReadAllLines(filePathWithExtensions);
                if (TryGetFrontMatter(readAllLines, out var frontMatter))
                {
                    markdownText = frontMatter + markdownText;
                }

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

            AssetDatabase.Refresh();
            EditorUtility.OpenWithDefaultApp(filePathWithExtensions);
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

                    ReadDocGeneratorSettingSO(typeData, generatorSettings, targetFolderPath, memberData,
                        out var markdownText, out var filePathWithExtensions);

                    if (File.Exists(filePathWithExtensions))
                    {
                        var readAllLines = File.ReadAllLines(filePathWithExtensions);
                        if (TryGetFrontMatter(readAllLines, out var frontMatter))
                        {
                            markdownText = frontMatter + markdownText;
                        }

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
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.Refresh();
            EditorUtility.OpenWithDefaultApp(targetFolderPath);
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

        static void ReadDocGeneratorSettingSO(ITypeData typeData,
            DocGeneratorSettingsSO generatorSettings,
            string targetFolderPath,
            IMemberData memberData,
            out string markdownText,
            out string filePathWithExtensions)
        {
            markdownText = generatorSettings.GetGeneratedDocumentation(typeData);

            if (generatorSettings.generateIdentifier)
            {
                markdownText = markdownText.EndsWith('\n') || markdownText.EndsWith("\r\n")
                    ? markdownText + UserIdentifierDescriptionParagraph
                    : markdownText + ("\n" + UserIdentifierDescriptionParagraph);
            }

            var fileNameWithoutExtension = memberData.Name.Replace('<', '[').Replace('>', ']');

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

            filePathWithExtensions = Path.Combine(targetFolderPath, fileNameWithoutExtension);

            if (generatorSettings.customizeDocFileExtensionName)
            {
                var ext = generatorSettings.docFileExtensionName;
                filePathWithExtensions += ext.StartsWith(".") ? ext : "." + ext;
            }
            else
            {
                filePathWithExtensions += ".md";
            }
        }

        static bool TryGetFrontMatter(string[] sourceLines, out string frontMatter)
        {
            var frontMatterStringBuilder = new StringBuilder();

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
                    break;
                }
            }

            frontMatter = frontMatterStringBuilder.ToString();
            return true;
        }
    }
}
