using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 单脚本模式面板：拖拽单个脚本或手动选择 Type 进行分析
    /// </summary>
    public class SingleScriptPanelSO : ScriptDocGeneratorPanelBase
    {
        const string ConfigName = "SingleScriptPanel";

        [PropertyOrder(25)]
        [SerializeField]
        MonoScript selectedMonoScript;

        [PropertyOrder(25)]
        [OdinSerialize]
        Type targetType;

        public Type TargetType
        {
            get => targetType;
            set => targetType = value;
        }

        protected override BilingualHeaderControl CreateHeaderControl()
        {
            return new BilingualHeaderControl("单脚本模式", "Single Script Mode",
                "拖拽单个脚本文件或手动选择 Type 进行文档生成。",
                "Drag a single script file or manually select a Type for document generation.");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            name = ConfigName;
        }

        public override void Analyze()
        {
            if (targetType == null)
            {
                Debug.LogError("请选择有效的目标类型");
                return;
            }

            typeData = ScriptDocGeneratorController.AnalyzeSingleType(targetType);
            OnAnalysisComplete();
        }

        protected override void PerformGenerateDoc()
        {
            if (!System.IO.Directory.Exists(docFolderPath))
            {
                if (!EditorUtility.DisplayDialog("自动路径补全提示", "当前的文档导出路径不存在，是否自动生成文件夹路径？", "确认", "取消"))
                    return;
                PathSafeEditorUtility.EnsureDirectoryExists(docFolderPath);
            }

            GenerateSingle(typeData);
        }

        void OnSelectedMonoScriptChanged()
        {
            if (selectedMonoScript)
            {
                targetType = selectedMonoScript.GetClass();
                Debug.Log("识别到 Type: " + targetType + "，已更新 TargetType");
            }
        }

        void ResetSelectedMonoScript()
        {
            selectedMonoScript = null;
        }

        void ResetTargetType()
        {
            targetType = null;
        }

        public override void AesirInspectorReset()
        {
            ResetDocFolderPath();
            ResetDocGeneratorSettingSO();
            ResetSelectedMonoScript();
            ResetTargetType();
            _hasFinishedAnalyze = false;
            typeData = null;
        }

        class SingleScriptPanelAttributeProcessor : OdinAttributeProcessor<SingleScriptPanelSO>
        {
            public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
                MemberInfo member,
                List<Attribute> attributes)
            {
                if (member.Name == nameof(selectedMonoScript))
                {
                    attributes.Add(new BilingualTitleAttribute("目标 Type", "Single Target Type"));
                    attributes.Add(new LabelWidthAttribute(270));
                    attributes.Add(new BilingualTextAttribute("拖拽 Script 文件到此处，自动识别类型: ",
                        "Drag Script File Here to Auto Identify Type: "));
                    attributes.Add(new InlineButtonAttribute(nameof(ResetSelectedMonoScript),
                        SdfIconType.ArrowClockwise, ""));
                    attributes.Add(new CustomContextMenuAttribute("Reset To Default",
                        nameof(ResetSelectedMonoScript)));
                    attributes.Add(new OnValueChangedAttribute(nameof(OnSelectedMonoScriptChanged)));
                }

                if (member.Name == nameof(targetType))
                {
                    attributes.Add(new LabelWidthAttribute(130));
                    attributes.Add(new BilingualTextAttribute("手动选择 Type: ", "Manually Select Type: "));
                    attributes.Add(new InlineButtonAttribute(nameof(ResetTargetType),
                        SdfIconType.ArrowClockwise, ""));
                    attributes.Add(new CustomContextMenuAttribute("Reset To Default",
                        nameof(ResetTargetType)));
                }

                if (member.Name == nameof(typeData))
                {
                    attributes.Add(new TitleGroupAttribute("类型分析数据结果", "Type Analysis Result"));
                }
            }
        }
    }
}
