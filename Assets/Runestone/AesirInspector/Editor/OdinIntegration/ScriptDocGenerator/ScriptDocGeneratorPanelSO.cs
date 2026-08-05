using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ScriptDocGenerator 可视化操作面板类
    /// </summary>
    public class ScriptDocGeneratorPanelSO : SerializedScriptableObject, IAesirInspectorReset
    {
        public enum TypeSource
        {
            SingleType,
            MultipleTypes,
            SingleAssembly,
            MultipleAssemblies
        }

        const string ScriptDocGeneratorRootPath =
            AesirInspectorPaths.EditorDefaultResourcesPath + "/ScriptDocGenerator";

        const string DefaultTypesCacheSoFolderPath = ScriptDocGeneratorRootPath + "/TypeCache";

        public const string DefaultDocFolderPath =
            AesirInspectorPaths.EditorDefaultResourcesPath + "/Documents";

        const string NoneAssembly = "None Assembly";

        static readonly string ConfigName = typeof(ScriptDocGeneratorPanelSO).GetNiceFullName();

        static ValueDropdownList<string> _currentDomainAssemblies;
        bool _hasFinishedAnalyze;
        bool _isCustomizingSaveConfig;

        [PropertyOrder(-5)]
        [SerializeField]
        BilingualHeaderControl bilingualHeaderControl;

        [PropertyOrder(2)]
        [SerializeField]
        string docFolderPath;

        [PropertyOrder(10)]
        [SerializeField]
        DocGeneratorSettingsSO docGeneratorSettings;

        [PropertyOrder(20)]
        [SerializeField]
        TypeSource typeSource;

        [PropertyOrder(25)]
        [SerializeField]
        MonoScript selectedMonoScript;

        [PropertyOrder(25)]
        [OdinSerialize]
        Type _targetType;

        [PropertyOrder(25)]
        [SerializeField]
        TypesCacheSO typesCache;

        [PropertyOrder(25)]
        [SerializeField]
        MonoScript[] selectedMonoScriptArray;

        [PropertyOrder(25)]
        [OdinSerialize]
        List<Type> _temporaryTypes = new List<Type>();

        [PropertyOrder(25)]
        [SerializeField]
        string typesCacheSOFolderPath = DefaultTypesCacheSoFolderPath;

        [PropertyOrder(35)]
        [SerializeField]
        string targetAssemblyFullName;

        [PropertyOrder(35)]
        [SerializeField]
        List<string> selectedAssemblyFullNames = new List<string>();

        [PropertyOrder(90)]
        [OdinSerialize]
        ITypeData _typeData;

        [PropertyOrder(90)]
        [OdinSerialize]
        List<ITypeData> _typeDataList;

        public static ScriptDocGeneratorPanelSO Instance =>
            ScriptableObjectSafeEditorUtility.GetOrCreateEditorScriptableObject<ScriptDocGeneratorPanelSO>(
                ConfigName, ScriptDocGeneratorRootPath, "ScriptDocGenerator");

        public Type TargetType
        {
            get => _targetType;
            set => _targetType = value;
        }

        public TypeSource TypeSourceProperty
        {
            get => typeSource;
            set => typeSource = value;
        }

        public List<Type> TemporaryTypes
        {
            get => _temporaryTypes;
            set => _temporaryTypes = value;
        }

        bool IsSingleType => typeSource == TypeSource.SingleType;
        bool IsMultipleType => typeSource == TypeSource.MultipleTypes;
        bool IsSingleAssembly => typeSource == TypeSource.SingleAssembly;
        bool IsMultipleAssemblies => typeSource == TypeSource.MultipleAssemblies;

        bool ShowSaveFolderPath => IsMultipleType && _isCustomizingSaveConfig && !typesCache;
        bool CanShowTemporaryTypes => IsMultipleType && !typesCache;

        bool CanShowGenerateButton => _hasFinishedAnalyze && ((IsSingleType && _typeData != null) ||
                                                              (IsMultipleType && _typeDataList is
                                                                  { Count: > 0 }) ||
                                                              (IsSingleAssembly && _typeDataList is
                                                                  { Count: > 0 }) || (IsMultipleAssemblies &&
                                                                  _typeDataList is { Count: > 0 }));

        bool IsNeedTypeAnalysisDataList => IsMultipleType || IsSingleAssembly || IsMultipleAssemblies;

        void OnEnable()
        {
            bilingualHeaderControl = new BilingualHeaderControl("脚本文档生成工具", "Script Doc Generator",
                "用户提供一个 Type 类型的值，分析 Type 数据，选择合适的文档生成器，一键生成对应的文档。默认提供中文 API 文档生成器，可以自定义适合项目的生成器。",
                "The user provides a value of the Type, analyze the Type data, selects the appropriate document generator, " +
                "and generates the corresponding document with one click. By default, a Chinese API document generator is provided, " +
                "and a custom generator suitable for the project can also be defined.");
            AesirInspectorReset();
        }

        public void AesirInspectorReset()
        {
            ResetDocFolderPath();
            ResetDocGeneratorSettingSO();
            ResetTypeSource();
            ResetSelectedMonoScript();
            ResetSingleType();
            ResetTypesCacheSO();
            ResetSelectedMonoScriptArray();
            ResetTemporaryTypes();
            ResetTypesCacheSOFolderPath();
            ResetIsCustomizingSaveConfig();
            ResetSingleAssemblyFullName();
            ResetSelectedAssemblyFullNames();
            ResetHasFinishedAnalyzed();
            ResetTypeAnalysisData();
        }

        public static event Action<ToastPosition, SdfIconType, string, Color, float> ToastRequested;

        [PropertyOrder(50)]
        [BilingualTitle("分析按钮", "Analyze Button")]
        [BilingualButton("基于当前模式执行类型分析", "Analyze Type based on the current mode", ButtonSizes.Large,
            ButtonStyle.Box, SdfIconType.FileEarmarkPlus)]
        public void AnalyzeType()
        {
            PerformAnalyzeType();
        }

        [PropertyOrder(70)]
        [ShowIf("CanShowGenerateButton")]
        [BilingualTitle("生成按钮", "Generate Button")]
        [BilingualButton("基于解析结果和文档生成器生成 Markdown 文档",
            "Generate Markdown Document Based On Analysis Result And Doc Generator", ButtonSizes.Large,
            ButtonStyle.Box, SdfIconType.FileEarmarkPlus)]
        public void GenerateDoc()
        {
            PerformGenerateDoc();
        }

        string GetDocGeneratorTitle()
        {
            var chineseTitle = "文档生成器设置";
            var englishTitle = "Doc Generator Setting";
            if (docGeneratorSettings &&
                docGeneratorSettings.GetType() == typeof(DefaultScriptingAPISettingsSO))
            {
                chineseTitle += " - [当前选择: 中文 API Markdown 文档]";
                englishTitle += " - [Current Selection: Chinese API Markdown Document]";
            }

            return new BilingualData(chineseTitle, englishTitle);
        }

        string GetTypeSourceEnumLabelText()
        {
            var chineseText = "类型来源模式";
            var englishText = "Type Source Mode";
            switch (typeSource)
            {
                case TypeSource.SingleType:
                    chineseText += " - [当前选择: 单类型模式]";
                    englishText += " - [Current Selection: Single Type Mode]";
                    break;
                case TypeSource.MultipleTypes:
                    chineseText += " - [当前选择: 多类型模式]";
                    englishText += " - [Current Selection: Multiple Type Mode]";
                    break;
                case TypeSource.SingleAssembly:
                    chineseText += " - [当前选择: 单程序集模式]";
                    englishText += " - [Current Selection: Single Assembly Mode]";
                    break;
                case TypeSource.MultipleAssemblies:
                    chineseText += " - [当前选择: 多程序集模式]";
                    englishText += " - [Current Selection: Multiple Assemblies Mode]";
                    break;
            }

            return new BilingualData(chineseText, englishText);
        }

        void OnSelectedMonoScriptChanged()
        {
            if (selectedMonoScript)
            {
                _targetType = selectedMonoScript.GetClass();
                Debug.Log("识别到 Type: " + _targetType + "，已更新 TargetType");
            }
        }

        void OnSelectedMonoScriptArrayChanged()
        {
            if (selectedMonoScriptArray.Length > 0)
            {
                var types = selectedMonoScriptArray.Distinct().Select(x => x.GetClass()).ToList();
                _temporaryTypes.AddRange(types);
                var distinctTypes = _temporaryTypes.Distinct().ToList();
                _temporaryTypes = distinctTypes;
            }
        }

        void DrawTemporaryTypesTitleBarGUI()
        {
            var image = SdfIcons.CreateTransparentIconTexture(SdfIconType.SaveFill, Color.white, 24, 24, 0);
            var content = new GUIContent(" 保存为SO资源 ", image,
                "保存为 " + nameof(TypesCacheSO) + " 资源到 " + typesCacheSOFolderPath);
            var filePathWithExtension = typesCacheSOFolderPath + "/" + nameof(TypesCacheSO) + ".asset";
            if (_temporaryTypes.Count > 0 && SirenixEditorGUI.ToolbarButton(content))
            {
                var so = CreateInstance<TypesCacheSO>();
                PathSafeEditorUtility.EnsureDirectoryExists(typesCacheSOFolderPath);
                so.Types = _temporaryTypes;
                ProjectWindowUtil.CreateAsset(so, filePathWithExtension);
                ProjectSafeEditorUtility.PingAndSelectAsset(filePathWithExtension);
                Debug.Log("请更改资源名称，避免下次生成时覆盖内容");
            }

            var image2 = SdfIcons.CreateTransparentIconTexture(SdfIconType.GearFill, Color.white, 24, 24, 0);
            var content2 = new GUIContent(" 自定义资源存储位置 ", image2, "当前路径为 " + typesCacheSOFolderPath);
            if (_isCustomizingSaveConfig)
            {
                return;
            }

            if (SirenixEditorGUI.ToolbarButton(content2))
            {
                _isCustomizingSaveConfig = true;
            }
        }

        void CompleteConfig()
        {
            _isCustomizingSaveConfig = false;
        }

        static ValueDropdownList<string> GetAssemblyNameToFullName()
        {
            if (_currentDomainAssemblies is { Count: > 0 })
            {
                return _currentDomainAssemblies;
            }

            _currentDomainAssemblies = new ValueDropdownList<string> { { NoneAssembly, NoneAssembly } };
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                _currentDomainAssemblies.Add(assembly.GetName().Name, assembly.FullName);
            }

            return _currentDomainAssemblies;
        }

        void PerformAnalyzeType()
        {
            switch (typeSource)
            {
                case TypeSource.SingleType:
                    if (_targetType == null)
                    {
                        Debug.LogError("请选择有效的目标类型");
                        return;
                    }

                    _typeData = ScriptDocGeneratorUtility.AnalyzeSingleType(_targetType);
                    break;
                case TypeSource.MultipleTypes:
                    if (!typesCache && _temporaryTypes.Count <= 0)
                    {
                        Debug.LogError("设置有效的 Type 对象列表或者设置 TypeCacheSO 资源");
                        return;
                    }

                    _typeDataList = typesCache
                        ? ScriptDocGeneratorUtility.AnalyzeMultipleTypes(typesCache)
                        : ScriptDocGeneratorUtility.AnalyzeMultipleTypes(_temporaryTypes);

                    break;
                case TypeSource.SingleAssembly:
                    if (targetAssemblyFullName is null or NoneAssembly)
                    {
                        Debug.LogError("请选择目标程序集，不能为 " + NoneAssembly);
                        return;
                    }

                    _typeDataList = ScriptDocGeneratorUtility.AnalyzeSingleAssembly(targetAssemblyFullName);
                    break;
                case TypeSource.MultipleAssemblies:
                    if (selectedAssemblyFullNames is not { Count: > 0 })
                    {
                        Debug.LogError("请选择至少一个目标程序集");
                        return;
                    }

                    selectedAssemblyFullNames.RemoveAll(string.IsNullOrEmpty);
                    selectedAssemblyFullNames.RemoveAll(name => name == NoneAssembly);
                    if (selectedAssemblyFullNames.Count == 0)
                    {
                        Debug.LogError("请选择有效的目标程序集，不能为 " + NoneAssembly);
                        return;
                    }

                    _typeDataList =
                        ScriptDocGeneratorUtility.AnalyzeMultipleAssemblies(selectedAssemblyFullNames);
                    break;
            }

            _hasFinishedAnalyze = true;
            ToastRequested?.Invoke(ToastPosition.BottomRight, SdfIconType.LightningFill,
                "分析中，等待生成按钮显示。请勿连续点击！", Color.yellow, 4f);
        }

        void PerformGenerateDoc()
        {
            if (!Directory.Exists(docFolderPath))
            {
                if (!EditorUtility.DisplayDialog("自动路径补全提示", "当前的文档导出路径不存在，是否自动生成文件夹路径？", "确认", "取消"))
                {
                    return;
                }

                PathSafeEditorUtility.EnsureDirectoryExists(docFolderPath);
            }

            switch (typeSource)
            {
                case TypeSource.SingleType:
                    ScriptDocGeneratorUtility.GenerateSingleTypeDoc(_typeData, docGeneratorSettings,
                        docFolderPath);
                    break;
                case TypeSource.MultipleTypes:
                case TypeSource.SingleAssembly:
                case TypeSource.MultipleAssemblies:
                    ScriptDocGeneratorUtility.GenerateMultipleTypeDocs(_typeDataList, docGeneratorSettings,
                        docFolderPath);
                    break;
            }

            _hasFinishedAnalyze = false;
        }

        class ScriptDocGeneratorPanelAttributeProcessor : OdinAttributeProcessor<ScriptDocGeneratorPanelSO>
        {
            public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
                MemberInfo member,
                List<Attribute> attributes)
            {
                if (member.Name == nameof(docFolderPath))
                {
                    attributes.Add(new BilingualTitleAttribute("生成脚本文档的目标文件夹路径 [可拖拽]",
                        "Folder Path For Document [Drag And Drop Allowed]"));
                    attributes.Add(new HideLabelAttribute());
                    attributes.Add(new FolderPathAttribute
                    {
                        AbsolutePath = true
                    });
                    attributes.Add(new InlineButtonAttribute(nameof(ResetDocFolderPath),
                        SdfIconType.ArrowClockwise, ""));
                    attributes.Add(new CustomContextMenuAttribute("Reset To Default",
                        nameof(ResetDocFolderPath)));
                }

                if (member.Name == nameof(docGeneratorSettings))
                {
                    attributes.Add(new TitleAttribute("$" + nameof(GetDocGeneratorTitle)));
                    attributes.Add(new HideLabelAttribute());
                    attributes.Add(new InlineButtonAttribute(nameof(ResetDocGeneratorSettingSO),
                        SdfIconType.ArrowClockwise, ""));
                    attributes.Add(new CustomContextMenuAttribute("Reset To Default",
                        nameof(ResetDocGeneratorSettingSO)));
                    attributes.Add(new InlineEditorAttribute(InlineEditorObjectFieldModes.Foldout));
                }

                if (member.Name == nameof(typeSource))
                {
                    attributes.Add(new TitleAttribute("$" + nameof(GetTypeSourceEnumLabelText)));
                    attributes.Add(new HideLabelAttribute());
                    attributes.Add(new InlineButtonAttribute(nameof(ResetTypeSource),
                        SdfIconType.ArrowClockwise, ""));
                    attributes.Add(
                        new CustomContextMenuAttribute("Reset To Default", nameof(ResetTypeSource)));
                    attributes.Add(new EnumPagingAttribute());
                }

                if (member.Name == nameof(selectedMonoScript))
                {
                    attributes.Add(new TitleAttribute("$" + nameof(_singleTypeDataLabel)));
                    attributes.Add(new ShowIfAttribute(nameof(IsSingleType)));
                    attributes.Add(new LabelWidthAttribute(270));
                    attributes.Add(new BilingualTextAttribute("拖拽 Script 文件到此处，自动识别类型: ",
                        "Drag Script File Here to Auto Identify Type: "));
                    attributes.Add(new InlineButtonAttribute(nameof(ResetSelectedMonoScript),
                        SdfIconType.ArrowClockwise, ""));
                    attributes.Add(new CustomContextMenuAttribute("Reset To Default",
                        nameof(ResetSelectedMonoScript)));
                    attributes.Add(new OnValueChangedAttribute(nameof(OnSelectedMonoScriptChanged)));
                }

                if (member.Name == nameof(_targetType))
                {
                    attributes.Add(new ShowIfAttribute(nameof(IsSingleType)));
                    attributes.Add(new LabelWidthAttribute(130));
                    attributes.Add(new BilingualTextAttribute("手动选择 Type: ", "Manually Select Type: "));
                    attributes.Add(new InlineButtonAttribute(nameof(ResetSingleType),
                        SdfIconType.ArrowClockwise, ""));
                    attributes.Add(
                        new CustomContextMenuAttribute("Reset To Default", nameof(ResetSingleType)));
                }

                if (member.Name == nameof(typesCache))
                {
                    attributes.Add(new ShowIfAttribute(nameof(IsMultipleType)));
                    attributes.Add(new BilingualTitleAttribute("目标 Types 列表配置", "Types Config"));
                    attributes.Add(new BilingualInfoBoxAttribute("TypesConfigSO 不为空时，会强制覆盖 Type 列表",
                        "When the TypesConfigSO asset is not empty, TemporaryTypes Config is forced to be overridden"));
                    attributes.Add(new HideLabelAttribute());
                    attributes.Add(new AssetSelectorAttribute
                    {
                        FlattenTreeView = true
                    });
                    attributes.Add(new InlineButtonAttribute(nameof(ResetTypesCacheSO),
                        SdfIconType.ArrowClockwise, ""));
                    attributes.Add(new CustomContextMenuAttribute("Reset To Default",
                        nameof(ResetTypesCacheSO)));
                }

                if (member.Name == nameof(selectedMonoScriptArray))
                {
                    attributes.Add(new ShowIfAttribute(nameof(IsMultipleType)));
                    attributes.Add(new LabelWidthAttribute(270));
                    attributes.Add(new BilingualTextAttribute("拖拽多个 Script 文件到此处，自动识别类型: ",
                        "Drag Multiple Script Files Here to Auto Identify Types: "));
                    attributes.Add(new InlineButtonAttribute(nameof(ResetSelectedMonoScriptArray),
                        SdfIconType.ArrowClockwise, ""));
                    attributes.Add(new CustomContextMenuAttribute("Reset To Default",
                        nameof(ResetSelectedMonoScriptArray)));
                    attributes.Add(new OnValueChangedAttribute(nameof(OnSelectedMonoScriptArrayChanged)));
                }

                if (member.Name == nameof(_temporaryTypes))
                {
                    attributes.Add(new ShowIfAttribute(nameof(CanShowTemporaryTypes)));
                    attributes.Add(new ListDrawerSettingsAttribute
                    {
                        OnTitleBarGUI = nameof(DrawTemporaryTypesTitleBarGUI),
                        NumberOfItemsPerPage = 5
                    });
                    attributes.Add(new HideLabelAttribute());
                    attributes.Add(new InlineButtonAttribute(nameof(ResetTemporaryTypes),
                        SdfIconType.ArrowClockwise, ""));
                    attributes.Add(new CustomContextMenuAttribute("Reset To Default",
                        nameof(ResetTemporaryTypes)));
                }

                if (member.Name == nameof(typesCacheSOFolderPath))
                {
                    attributes.Add(new FolderPathAttribute());
                    attributes.Add(new ShowIfAttribute(nameof(ShowSaveFolderPath)));
                    attributes.Add(new HideLabelAttribute());
                    attributes.Add(new InlineButtonAttribute(nameof(CompleteConfig), SdfIconType.Check,
                        "$" + nameof(_completeConfigButtonLabel)));
                    attributes.Add(new InlineButtonAttribute(nameof(ResetTypesCacheSOFolderPath),
                        SdfIconType.ArrowClockwise, "$" + nameof(_resetSOSaveFolderPathButtonLabel)));
                    attributes.Add(new BilingualTitleAttribute("存放 TypesCacheSO 的文件夹路径",
                        "Folder Path For TypesCacheSO"));
                    attributes.Add(new CustomContextMenuAttribute("Reset To Default",
                        nameof(ResetTypesCacheSOFolderPath)));
                }

                if (member.Name == nameof(targetAssemblyFullName))
                {
                    attributes.Add(new ShowIfAttribute(nameof(IsSingleAssembly)));
                    attributes.Add(new BilingualTitleAttribute("目标程序集配置", "Single Assembly Config"));
                    attributes.Add(new ValueDropdownAttribute(nameof(GetAssemblyNameToFullName)));
                    attributes.Add(new HideLabelAttribute());
                    attributes.Add(new InlineButtonAttribute(nameof(ResetSingleAssemblyFullName),
                        SdfIconType.ArrowClockwise, ""));
                    attributes.Add(new CustomContextMenuAttribute("Reset To Default",
                        nameof(ResetSingleAssemblyFullName)));
                }

                if (member.Name == nameof(selectedAssemblyFullNames))
                {
                    attributes.Add(new ShowIfAttribute(nameof(IsMultipleAssemblies)));
                    attributes.Add(new BilingualTitleAttribute("目标程序集配置", "Multiple Assemblies Config"));
                    attributes.Add(new ValueDropdownAttribute(nameof(GetAssemblyNameToFullName)));
                    attributes.Add(new ListDrawerSettingsAttribute
                    {
                        NumberOfItemsPerPage = 5
                    });
                    attributes.Add(new HideLabelAttribute());
                    attributes.Add(new InlineButtonAttribute(nameof(ResetSelectedAssemblyFullNames),
                        SdfIconType.ArrowClockwise, ""));
                    attributes.Add(new CustomContextMenuAttribute("Reset To Default",
                        nameof(ResetSelectedAssemblyFullNames)));
                }

                if (member.Name == nameof(_typeData))
                {
                    attributes.Add(new TitleGroupAttribute("$" + nameof(_typeAnalysisResultLabel)));
                    attributes.Add(new ShowIfAttribute(nameof(IsSingleType)));
                }

                if (member.Name == nameof(_typeDataList))
                {
                    attributes.Add(new TitleGroupAttribute("$" + nameof(_typeAnalysisResultLabel)));
                    attributes.Add(new ShowIfAttribute(nameof(IsNeedTypeAnalysisDataList)));
                }
            }
        }

        #region Reset Default Values

        void ResetTypeSource()
        {
            typeSource = TypeSource.SingleType;
        }

        void ResetDocFolderPath()
        {
            docFolderPath = DefaultDocFolderPath;
        }

        void ResetDocGeneratorSettingSO()
        {
            docGeneratorSettings = DefaultScriptingAPISettingsSO.Instance;
        }

        void ResetSingleType()
        {
            _targetType = null;
        }

        void ResetSelectedMonoScript()
        {
            selectedMonoScript = null;
        }

        void ResetTypesCacheSO()
        {
            typesCache = null;
        }

        void ResetTemporaryTypes()
        {
            _temporaryTypes = new List<Type>();
        }

        void ResetSelectedMonoScriptArray()
        {
            selectedMonoScriptArray = new MonoScript[] { };
        }

        void ResetTypesCacheSOFolderPath()
        {
            typesCacheSOFolderPath = DefaultTypesCacheSoFolderPath;
        }

        void ResetIsCustomizingSaveConfig()
        {
            _isCustomizingSaveConfig = false;
        }

        void ResetSingleAssemblyFullName()
        {
            targetAssemblyFullName = string.Empty;
        }

        void ResetSelectedAssemblyFullNames()
        {
            selectedAssemblyFullNames = new List<string>();
        }

        void ResetHasFinishedAnalyzed()
        {
            _hasFinishedAnalyze = false;
        }

        void ResetTypeAnalysisData()
        {
            _typeData = null;
            _typeDataList = null;
        }

        #endregion

        #region Bilingualism

        public static readonly BilingualData ModuleName =
            new BilingualData("脚本文档生成工具", "Script Doc Generator");

        BilingualData _singleTypeDataLabel = new BilingualData("目标 Type", "Single Target Type");

        BilingualData _typeAnalysisResultLabel = new BilingualData("类型分析数据结果", "Type Analysis Result");

        BilingualData _completeConfigButtonLabel = new BilingualData("完成设置", "Complete Setting");

        BilingualData _resetSOSaveFolderPathButtonLabel = new BilingualData("重置路径", "Reset Folder Path");

        #endregion
    }
}
