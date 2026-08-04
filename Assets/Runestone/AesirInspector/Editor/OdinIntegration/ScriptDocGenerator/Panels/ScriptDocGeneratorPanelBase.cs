using System;
using System.Collections.Generic;
using System.IO;
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
    /// ScriptDocGenerator 面板基类，包含共享配置和分析/生成逻辑入口。
    /// </summary>
    public abstract class ScriptDocGeneratorPanelBase : SerializedScriptableObject, IAesirInspectorReset
    {
        const string ScriptDocGeneratorRootPath =
            AesirInspectorPaths.EditorDefaultResourcesPath + "/ScriptDocGenerator";

        public const string DefaultDocFolderPath =
            AesirInspectorPaths.EditorDefaultResourcesPath + "/Documents";

        [PropertyOrder(-5)]
        [SerializeField]
        protected BilingualHeaderControl bilingualHeaderControl;

        [PropertyOrder(2)]
        [SerializeField]
        protected string docFolderPath = DefaultDocFolderPath;

        [PropertyOrder(10)]
        [SerializeField]
        protected DocGeneratorSettingsSO docGeneratorSettings;

        [PropertyOrder(90)]
        [OdinSerialize]
        protected ITypeData typeData;

        [PropertyOrder(90)]
        [OdinSerialize]
        protected List<ITypeData> typeDataList;

        protected bool _hasFinishedAnalyze;

        protected bool HasFinishedAnalyze => _hasFinishedAnalyze;

        bool CanShowGenerateButton => _hasFinishedAnalyze && (
            (typeData != null) ||
            (typeDataList is { Count: > 0 }));

        public DocGeneratorSettingsSO DocGeneratorSettings
        {
            get => docGeneratorSettings;
            set => docGeneratorSettings = value;
        }

        public string DocFolderPath
        {
            get => docFolderPath;
            set => docFolderPath = value;
        }

        protected virtual void OnEnable()
        {
            bilingualHeaderControl = CreateHeaderControl();
            if (docGeneratorSettings == null)
                docGeneratorSettings = DefaultScriptingAPISettingsSO.Instance;
            if (string.IsNullOrEmpty(docFolderPath))
                docFolderPath = DefaultDocFolderPath;
        }

        protected abstract BilingualHeaderControl CreateHeaderControl();

        public abstract void Analyze();

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

        [PropertyOrder(50)]
        [BilingualTitle("分析按钮", "Analyze Button")]
        [BilingualButton("执行类型分析", "Analyze Type", ButtonSizes.Large,
            ButtonStyle.Box, SdfIconType.FileEarmarkPlus)]
        public void AnalyzeButton()
        {
            Analyze();
        }

        public static event Action<ToastPosition, SdfIconType, string, Color, float> ToastRequested;

        protected void OnAnalysisComplete()
        {
            _hasFinishedAnalyze = true;
            ToastRequested?.Invoke(ToastPosition.BottomRight, SdfIconType.LightningFill,
                "分析中，等待生成按钮显示。请勿连续点击！", Color.yellow, 4f);
        }

        protected abstract void PerformGenerateDoc();

        protected void GenerateSingle(ITypeData data)
        {
            ScriptDocGeneratorUtility.GenerateSingleTypeDoc(data, docGeneratorSettings, docFolderPath);
            _hasFinishedAnalyze = false;
        }

        protected void GenerateMultiple(List<ITypeData> dataList)
        {
            if (!Directory.Exists(docFolderPath))
            {
                if (!EditorUtility.DisplayDialog("自动路径补全提示", "当前的文档导出路径不存在，是否自动生成文件夹路径？", "确认", "取消"))
                    return;
                PathSafeEditorUtility.EnsureDirectoryExists(docFolderPath);
            }

            ScriptDocGeneratorUtility.GenerateMultipleTypeDocs(dataList, docGeneratorSettings, docFolderPath);
            _hasFinishedAnalyze = false;
        }

        string GetDocGeneratorTitle()
        {
            var chineseTitle = "文档生成器设置";
            var englishTitle = "Doc Generator Setting";
            if (docGeneratorSettings && docGeneratorSettings.GetType() == typeof(DefaultScriptingAPISettingsSO))
            {
                chineseTitle += " - [当前选择: 中文 API Markdown 文档]";
                englishTitle += " - [Current Selection: Chinese API Markdown Document]";
            }

            return new BilingualData(chineseTitle, englishTitle);
        }

        protected void ResetDocFolderPath()
        {
            docFolderPath = DefaultDocFolderPath;
        }

        protected void ResetDocGeneratorSettingSO()
        {
            docGeneratorSettings = DefaultScriptingAPISettingsSO.Instance;
        }

        public abstract void AesirInspectorReset();

        class PanelBaseAttributeProcessor : OdinAttributeProcessor<ScriptDocGeneratorPanelBase>
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
                    attributes.Add(new FolderPathAttribute { AbsolutePath = true });
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
            }
        }
    }
}
