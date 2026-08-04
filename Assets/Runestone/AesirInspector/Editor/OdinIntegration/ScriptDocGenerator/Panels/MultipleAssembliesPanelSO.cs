using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 多程序集模式面板：选择多个程序集，分析其中所有类型
    /// </summary>
    public class MultipleAssembliesPanelSO : ScriptDocGeneratorPanelBase
    {
        const string ConfigName = "MultipleAssembliesPanel";
        const string NoneAssembly = "None Assembly";

        [PropertyOrder(35)]
        [SerializeField]
        List<string> selectedAssemblyFullNames = new List<string>();

        static ValueDropdownList<string> _currentDomainAssemblies;

        public List<string> SelectedAssemblyFullNames
        {
            get => selectedAssemblyFullNames;
            set => selectedAssemblyFullNames = value;
        }

        protected override BilingualHeaderControl CreateHeaderControl()
        {
            return new BilingualHeaderControl("多程序集模式", "Multiple Assemblies Mode",
                "选择多个程序集，批量分析其中所有公共类型并生成文档。",
                "Select multiple assemblies to batch analyze all public types and generate documents.");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            name = ConfigName;
        }

        public override void Analyze()
        {
            if (selectedAssemblyFullNames is not { Count: > 0 })
            {
                Debug.LogError("请选择至少一个目标程序集");
                return;
            }

            var validNames = selectedAssemblyFullNames
                .Where(name => !string.IsNullOrEmpty(name) && name != NoneAssembly)
                .ToList();

            if (validNames.Count == 0)
            {
                Debug.LogError("请选择有效的目标程序集，不能为 " + NoneAssembly);
                return;
            }

            typeDataList = ScriptDocGeneratorUtility.AnalyzeMultipleAssemblies(validNames);
            OnAnalysisComplete();
        }

        protected override void PerformGenerateDoc()
        {
            GenerateMultiple(typeDataList);
        }

        static IEnumerable<ValueDropdownItem<string>> GetAssemblyNameToFullName()
        {
            if (_currentDomainAssemblies is null or { Count: 0 })
            {
                _currentDomainAssemblies = new ValueDropdownList<string> { { NoneAssembly, NoneAssembly } };
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                    _currentDomainAssemblies.Add(assembly.GetName().Name, assembly.FullName);
            }

            return _currentDomainAssemblies;
        }

        void ResetSelectedAssemblyFullNames()
        {
            selectedAssemblyFullNames = new List<string>();
        }

        public override void AesirInspectorReset()
        {
            ResetDocFolderPath();
            ResetDocGeneratorSettingSO();
            ResetSelectedAssemblyFullNames();
            _hasFinishedAnalyze = false;
            typeDataList = null;
        }

        class MultipleAssembliesPanelAttributeProcessor : OdinAttributeProcessor<MultipleAssembliesPanelSO>
        {
            public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
                MemberInfo member,
                List<Attribute> attributes)
            {
                if (member.Name == nameof(selectedAssemblyFullNames))
                {
                    attributes.Add(new BilingualTitleAttribute("目标程序集配置", "Assemblies Config"));
                    attributes.Add(new ValueDropdownAttribute(nameof(GetAssemblyNameToFullName)));
                    attributes.Add(new HideLabelAttribute());
                    attributes.Add(new ListDrawerSettingsAttribute
                    {
                        NumberOfItemsPerPage = 5
                    });
                    attributes.Add(new InlineButtonAttribute(nameof(ResetSelectedAssemblyFullNames),
                        SdfIconType.ArrowClockwise, ""));
                    attributes.Add(new CustomContextMenuAttribute("Reset To Default",
                        nameof(ResetSelectedAssemblyFullNames)));
                }

                if (member.Name == nameof(typeDataList))
                {
                    attributes.Add(new TitleGroupAttribute("类型分析数据结果", "Type Analysis Result"));
                }
            }
        }
    }
}
