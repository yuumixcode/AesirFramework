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
    /// 单程序集模式面板：选择一个程序集，分析其中所有类型
    /// </summary>
    public class SingleAssemblyPanelSO : ScriptDocGeneratorPanelBase
    {
        const string ConfigName = "SingleAssemblyPanel";
        const string NoneAssembly = "None Assembly";

        [PropertyOrder(35)]
        [SerializeField]
        string targetAssemblyFullName = string.Empty;

        static ValueDropdownList<string> _currentDomainAssemblies;

        public string TargetAssemblyFullName
        {
            get => targetAssemblyFullName;
            set => targetAssemblyFullName = value;
        }

        protected override BilingualHeaderControl CreateHeaderControl()
        {
            return new BilingualHeaderControl("单程序集模式", "Single Assembly Mode",
                "选择一个程序集，分析其中所有公共类型并生成文档。",
                "Select a single assembly to analyze all public types and generate documents.");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            name = ConfigName;
        }

        public override void Analyze()
        {
            if (string.IsNullOrEmpty(targetAssemblyFullName) || targetAssemblyFullName == NoneAssembly)
            {
                Debug.LogError("请选择目标程序集，不能为 " + NoneAssembly);
                return;
            }

            typeDataList = ScriptDocGeneratorUtility.AnalyzeSingleAssembly(targetAssemblyFullName);
            OnAnalysisComplete();
        }

        protected override void PerformGenerateDoc()
        {
            GenerateMultiple(typeDataList);
        }

        static ValueDropdownList<string> GetAssemblyNameToFullName()
        {
            if (_currentDomainAssemblies is { Count: > 0 })
                return _currentDomainAssemblies;

            _currentDomainAssemblies = new ValueDropdownList<string> { { NoneAssembly, NoneAssembly } };
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
                _currentDomainAssemblies.Add(assembly.GetName().Name, assembly.FullName);

            return _currentDomainAssemblies;
        }

        void ResetSingleAssemblyFullName()
        {
            targetAssemblyFullName = string.Empty;
        }

        public override void AesirInspectorReset()
        {
            ResetDocFolderPath();
            ResetDocGeneratorSettingSO();
            ResetSingleAssemblyFullName();
            _hasFinishedAnalyze = false;
            typeDataList = null;
        }

        class SingleAssemblyPanelAttributeProcessor : OdinAttributeProcessor<SingleAssemblyPanelSO>
        {
            public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
                MemberInfo member,
                List<Attribute> attributes)
            {
                if (member.Name == nameof(targetAssemblyFullName))
                {
                    attributes.Add(new BilingualTitleAttribute("目标程序集配置", "Single Assembly Config"));
                    attributes.Add(new ValueDropdownAttribute(nameof(GetAssemblyNameToFullName)));
                    attributes.Add(new HideLabelAttribute());
                    attributes.Add(new InlineButtonAttribute(nameof(ResetSingleAssemblyFullName),
                        SdfIconType.ArrowClockwise, ""));
                    attributes.Add(new CustomContextMenuAttribute("Reset To Default",
                        nameof(ResetSingleAssemblyFullName)));
                }

                if (member.Name == nameof(typeDataList))
                {
                    attributes.Add(new TitleGroupAttribute("类型分析数据结果", "Type Analysis Result"));
                }
            }
        }
    }
}
