using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 多脚本模式面板：通过 TypesCacheSO 或临时 Type 列表批量生成文档
    /// </summary>
    public class MultipleScriptsPanelSO : ScriptDocGeneratorPanelBase
    {
        const string ConfigName = "MultipleScriptsPanel";
        const string DefaultTypesCacheSoFolderPath =
            AesirInspectorPaths.EditorDefaultResourcesPath + "/ScriptDocGenerator/TypeCache";

        [PropertyOrder(25)]
        [SerializeField]
        TypesCacheSO typesCache;

        [PropertyOrder(25)]
        [SerializeField]
        MonoScript[] selectedMonoScriptArray = Array.Empty<MonoScript>();

        [PropertyOrder(25)]
        [OdinSerialize]
        List<Type> temporaryTypes = new List<Type>();

        [PropertyOrder(25)]
        [SerializeField]
        string typesCacheSOFolderPath = DefaultTypesCacheSoFolderPath;

        bool _isCustomizingSaveConfig;

        public List<Type> TemporaryTypes
        {
            get => temporaryTypes;
            set => temporaryTypes = value;
        }

        bool ShowSaveFolderPath => _isCustomizingSaveConfig && !typesCache;
        bool CanShowTemporaryTypes => !typesCache;

        protected override BilingualHeaderControl CreateHeaderControl()
        {
            return new BilingualHeaderControl("多脚本模式", "Multiple Scripts Mode",
                "通过 TypesCacheSO 资源或临时 Type 列表批量生成文档。",
                "Generate documents in batch via TypesCacheSO asset or temporary Type list.");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            name = ConfigName;
        }

        public override void Analyze()
        {
            if (!typesCache && temporaryTypes.Count <= 0)
            {
                Debug.LogError("设置有效的 Type 对象列表或者设置 TypeCacheSO 资源");
                return;
            }

            typeDataList = typesCache
                ? ScriptDocGeneratorUtility.AnalyzeMultipleTypes(typesCache)
                : ScriptDocGeneratorUtility.AnalyzeMultipleTypes(temporaryTypes);

            OnAnalysisComplete();
        }

        protected override void PerformGenerateDoc()
        {
            GenerateMultiple(typeDataList);
        }

        void OnSelectedMonoScriptArrayChanged()
        {
            if (selectedMonoScriptArray.Length > 0)
            {
                var types = selectedMonoScriptArray.Distinct().Select(x => x.GetClass()).ToList();
                temporaryTypes.AddRange(types);
                temporaryTypes = temporaryTypes.Distinct().ToList();
            }
        }

        void DrawTemporaryTypesTitleBarGUI()
        {
            var image = SdfIcons.CreateTransparentIconTexture(SdfIconType.SaveFill, Color.white, 24, 24, 0);
            var content = new GUIContent(" 保存为SO资源 ", image,
                "保存为 " + nameof(TypesCacheSO) + " 资源到 " + typesCacheSOFolderPath);
            var filePathWithExtension = typesCacheSOFolderPath + "/" + nameof(TypesCacheSO) + ".asset";
            if (temporaryTypes.Count > 0 && SirenixEditorGUI.ToolbarButton(content))
            {
                var so = CreateInstance<TypesCacheSO>();
                PathSafeEditorUtility.EnsureDirectoryExists(typesCacheSOFolderPath);
                so.Types = temporaryTypes;
                ProjectWindowUtil.CreateAsset(so, filePathWithExtension);
                ProjectSafeEditorUtility.PingAndSelectAsset(filePathWithExtension);
                Debug.Log("请更改资源名称，避免下次生成时覆盖内容");
            }

            var image2 = SdfIcons.CreateTransparentIconTexture(SdfIconType.GearFill, Color.white, 24, 24, 0);
            var content2 = new GUIContent(" 自定义资源存储位置 ", image2, "当前路径为 " + typesCacheSOFolderPath);
            if (_isCustomizingSaveConfig)
                return;

            if (SirenixEditorGUI.ToolbarButton(content2))
                _isCustomizingSaveConfig = true;
        }

        void CompleteConfig()
        {
            _isCustomizingSaveConfig = false;
        }

        void ResetTypesCacheSO()
        {
            typesCache = null;
        }

        void ResetSelectedMonoScriptArray()
        {
            selectedMonoScriptArray = Array.Empty<MonoScript>();
        }

        void ResetTemporaryTypes()
        {
            temporaryTypes = new List<Type>();
        }

        void ResetTypesCacheSOFolderPath()
        {
            typesCacheSOFolderPath = DefaultTypesCacheSoFolderPath;
        }

        void ResetIsCustomizingSaveConfig()
        {
            _isCustomizingSaveConfig = false;
        }

        public override void AesirInspectorReset()
        {
            ResetDocFolderPath();
            ResetDocGeneratorSettingSO();
            ResetTypesCacheSO();
            ResetSelectedMonoScriptArray();
            ResetTemporaryTypes();
            ResetTypesCacheSOFolderPath();
            ResetIsCustomizingSaveConfig();
            _hasFinishedAnalyze = false;
            typeDataList = null;
        }

        class MultipleScriptsPanelAttributeProcessor : OdinAttributeProcessor<MultipleScriptsPanelSO>
        {
            public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
                MemberInfo member,
                List<Attribute> attributes)
            {
                if (member.Name == nameof(typesCache))
                {
                    attributes.Add(new BilingualTitleAttribute("目标 Types 列表配置", "Types Config"));
                    attributes.Add(new BilingualInfoBoxAttribute("TypesConfigSO 不为空时，会强制覆盖 Type 列表",
                        "When the TypesConfigSO asset is not empty, TemporaryTypes Config is forced to be overridden"));
                    attributes.Add(new HideLabelAttribute());
                    attributes.Add(new AssetSelectorAttribute { FlattenTreeView = true });
                    attributes.Add(new InlineButtonAttribute(nameof(ResetTypesCacheSO),
                        SdfIconType.ArrowClockwise, ""));
                    attributes.Add(new CustomContextMenuAttribute("Reset To Default",
                        nameof(ResetTypesCacheSO)));
                }

                if (member.Name == nameof(selectedMonoScriptArray))
                {
                    attributes.Add(new LabelWidthAttribute(270));
                    attributes.Add(new BilingualTextAttribute("拖拽多个 Script 文件到此处，自动识别类型: ",
                        "Drag Multiple Script Files Here to Auto Identify Types: "));
                    attributes.Add(new InlineButtonAttribute(nameof(ResetSelectedMonoScriptArray),
                        SdfIconType.ArrowClockwise, ""));
                    attributes.Add(new CustomContextMenuAttribute("Reset To Default",
                        nameof(ResetSelectedMonoScriptArray)));
                    attributes.Add(new OnValueChangedAttribute(nameof(OnSelectedMonoScriptArrayChanged)));
                }

                if (member.Name == nameof(temporaryTypes))
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
                        "完成设置"));
                    attributes.Add(new InlineButtonAttribute(nameof(ResetTypesCacheSOFolderPath),
                        SdfIconType.ArrowClockwise, "重置路径"));
                    attributes.Add(new BilingualTitleAttribute("存放 TypesCacheSO 的文件夹路径",
                        "Folder Path For TypesCacheSO"));
                    attributes.Add(new CustomContextMenuAttribute("Reset To Default",
                        nameof(ResetTypesCacheSOFolderPath)));
                }

                if (member.Name == nameof(typeDataList))
                {
                    attributes.Add(new TitleGroupAttribute("类型分析数据结果", "Type Analysis Result"));
                }
            }
        }
    }
}
