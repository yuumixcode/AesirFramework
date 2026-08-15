using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 特性面板 SO 泛型单例基类，继承自 SerializedScriptableObject。
    /// </summary>
    public abstract class AttributeOverviewPanelSO<T> : SerializedScriptableObject, IAesirInspectorReset
        where T : AttributeOverviewPanelSO<T>
    {
        static T _asset;

        /// <summary>
        /// 获取单例实例，若不存在则自动创建。
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_asset)
                {
                    return _asset;
                }

                _asset = ScriptableObjectSafeEditorUtility.GetSingletonAssetAndDeleteOther<T>(
                    AesirInspectorPaths.AttributePanelsPath);
                return _asset;
            }
        }

        /// <summary>
        /// 重置面板状态。
        /// </summary>
        public abstract void AesirInspectorReset();
    }

    /// <summary>
    /// 特性介绍面板抽象基类，负责渲染顶部控件、使用提示、参数表、案例预览与代码预览。
    /// </summary>
    public abstract class AbstractAttributePanelSO : AttributeOverviewPanelSO<AbstractAttributePanelSO>
    {
        const float AfterSpace = 20f;

        [SerializeField]
        [PropertyOrder(-100)]
        [PropertySpace(0, AfterSpace)]
        BilingualHeaderControl bilingualHeaderControl;

        AbstractAttributeData _data;
        UsageTipsRenderer _usageTipsRenderer;
        AttributeParametersRenderer _attributeParametersRenderer;
        ResolvedStringParametersRenderer _resolvedStringParametersRenderer;
        ExamplePreviewRenderer _examplePreviewRenderer;
        CodePreviewRenderer _codePreviewRenderer;

        /// <summary>
        /// 当前选中的示例对象。
        /// </summary>
        public ScriptableObject CurrentSelectedExample
        {
            get => currentSelectedExample;
            set
            {
                if (currentSelectedExample != value)
                {
                    currentSelectedExample = value;
                    UpdateExampleCode();
                }
            }
        }

        /// <summary>
        /// 顶部说明控件引用。
        /// </summary>
        public BilingualHeaderControl BilingualHeaderControl => bilingualHeaderControl;

        void OnDestroy()
        {
            AesirInspectorLanguageSettingsSO.LanguageChanged -= OnLanguageChanged;
        }

        /// <summary>
        /// 初始化面板，子类中调用 SetData 完成数据绑定。
        /// </summary>
        [OnInspectorInit]
        [PropertyOrder(-1000)]
        public abstract void Initialize();

        /// <summary>
        /// 重置面板至初始状态。
        /// </summary>
        public override void AesirInspectorReset()
        {
            _usageTipsRenderer?.Reset();
            _attributeParametersRenderer?.Reset();
            _resolvedStringParametersRenderer?.Reset();
            _examplePreviewRenderer?.Reset();
            _codePreviewRenderer?.Reset();

            if (_data != null)
            {
                currentSelectedExample = _data.GetInitialExample();
                UpdateExampleCode();
            }

            if (_examplePreviewItems is { Length: > 0 })
            {
                foreach (var item in _examplePreviewItems)
                {
                    item.Reset();
                }
            }
        }

        #region Performance Cache

        internal static float GetCachedTextHeight(string text, float width, Dictionary<string, float> cache)
        {
            var key = text + "_" + width;
            if (cache.TryGetValue(key, out var height))
            {
                return height;
            }

            height = AttributeOverviewEditorUtility.TableCellTextStyle.CalcHeight(GUIHelper.TempContent(text),
                width);
            cache[key] = height;
            return height;
        }

        #endregion

        /// <summary>
        /// 子类设置数据
        /// </summary>
        protected void SetData(AbstractAttributeData attributeData) => Internal_SetData(attributeData);

        #region Internal

        void UpdateExampleCode()
        {
            _currentExampleSourceCode =
                AttributeOverviewEditorUtility.GetExampleSourceCodeWithoutNamespace(MarkExampleAttribute);
            _codePreviewRenderer?.SetData(_currentExampleSourceCode);
            _codePreviewRenderer?.Reset();
        }

        void Internal_SetData(AbstractAttributeData attributeData)
        {
            _data = attributeData;
            bilingualHeaderControl = _data.BilingualHeaderControl;
            _usageTipsRenderer ??= new UsageTipsRenderer(this);
            _usageTipsRenderer.SetData(_data.UsageTips);
            _attributeParametersRenderer ??= new AttributeParametersRenderer();
            _attributeParametersRenderer.SetData(_data.AttributeParameters);
            _resolvedStringParametersRenderer ??= new ResolvedStringParametersRenderer();
            _resolvedStringParametersRenderer.SetData(_data.ResolvedStringParameters);
            _examplePreviewRenderer ??= new ExamplePreviewRenderer(this);
            _examplePreviewRenderer.SetData(_data.ExamplePreviewItems);
            _codePreviewRenderer ??= new CodePreviewRenderer(this);
            // 保持原有字段同步（兼容现有代码）
            _usageTips = _data.UsageTips;
            _attributeParameters = _data.AttributeParameters;
            _resolvedStringParameters = _data.ResolvedStringParameters;
            _examplePreviewItems = _data.ExamplePreviewItems;
            if (_examplePreviewItems != null)
            {
                currentSelectedExample = _data.GetInitialExample();
                UpdateExampleCode();
            }

            AesirInspectorLanguageSettingsSO.LanguageChanged -= OnLanguageChanged;
            AesirInspectorLanguageSettingsSO.LanguageChanged += OnLanguageChanged;
        }

        void OnLanguageChanged()
        {
            _usageTipsRenderer?.OnLanguageChanged();
            _attributeParametersRenderer?.OnLanguageChanged();
            _resolvedStringParametersRenderer?.OnLanguageChanged();

            if (_resolvedStringParameters != null)
            {
                foreach (var rValue in _resolvedStringParameters)
                {
                    rValue.CreateResolverInfoTable();
                    rValue.CreateNamedValueTable();
                    if (Event.current != null)
                    {
                        rValue.ResizeAllTables();
                    }
                }
            }
        }

        #endregion

        #region Usage Tips

        BilingualData[] _usageTips;

        bool UsageTipIsEmpty => _usageTips == null || _usageTips.Length == 0;

        [HideIf(nameof(UsageTipIsEmpty))]
        [PropertyOrder(-60)]
        [OnInspectorGUI]
        [PropertySpace(0, AfterSpace)]
        void DrawUsageTips() => _usageTipsRenderer?.Draw();

        #endregion

        #region Attribute Parameters

        ParameterValue[] _attributeParameters;

        bool AttributeParameterIsEmpty => _attributeParameters == null || _attributeParameters.Length == 0;

        [HideIf(nameof(AttributeParameterIsEmpty))]
        [PropertyOrder(-20)]
        [OnInspectorGUI]
        [PropertySpace(0, AfterSpace)]
        void DrawAttributeParameters() => _attributeParametersRenderer?.Draw();

        #endregion

        #region Resolved String Parameters

        static BilingualData _resolvedStringParameterLabel =
            new BilingualData("解析字符串参数", "Resolved String Parameters");

        ResolvedStringParameterValue[] _resolvedStringParameters;
        Rect _resolvedStringParametersContentRect;

        bool ResolvedStringParametersIsEmpty =>
            _resolvedStringParameters == null || _resolvedStringParameters.Length == 0;

        [HideIf(nameof(ResolvedStringParametersIsEmpty))]
        [PropertyOrder(-10)]
        [OnInspectorGUI]
        [PropertySpace(0, AfterSpace)]
        void DrawResolvedStringParameters() => _resolvedStringParametersRenderer?.Draw();

        #endregion

        #region Usage Example

        AttributeExamplePreviewItem[] _examplePreviewItems;

        bool UsageExampleItemsIsEmpty => _examplePreviewItems == null || _examplePreviewItems.Length == 0;

        [SerializeField]
        [HideIf(nameof(UsageExampleItemsIsEmpty))]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [PropertyOrder(0)]
        ScriptableObject currentSelectedExample;

        [HideIf(nameof(UsageExampleItemsIsEmpty))]
        [OnInspectorGUI]
        [PropertyOrder(-1)]
        void DrawUsageExamplePreview() => _examplePreviewRenderer?.BeginDraw();

        [HideIf(nameof(UsageExampleItemsIsEmpty))]
        [PropertySpace(0, AfterSpace)]
        [PropertyOrder(100)]
        [OnInspectorGUI]
        void EndDrawUsageExampleContainer()
        {
            _examplePreviewRenderer?.EndDraw();
        }

        AesirExampleAttribute MarkExampleAttribute => !currentSelectedExample
            ? null
            : AttributeOverviewEditorUtility.GetAttributeInExampleType(currentSelectedExample.GetType());

        #endregion

        #region Code Preview

        bool CurrentExampleIsNull => !currentSelectedExample;
        string _currentExampleSourceCode;

        [HideIf(nameof(CurrentExampleIsNull))]
        [OnInspectorGUI]
        [PropertyOrder(150)]
        [PropertySpace(0, AfterSpace)]
        void DrawCurrentExampleCodePreview() => _codePreviewRenderer?.Draw();

        #endregion

        #region Draw Container Helpers

        internal static Rect BeginDrawContainerWithTitle(string title, out Rect headerToolBarRect)
        {
            var titleStyle = AttributeOverviewEditorUtility.ContainerTitleStyle;
            var titleWidth = titleStyle.CalcSize(GUIHelper.TempContent(title)).x;
            var titleHeight = titleStyle.CalcSize(GUIHelper.TempContent(title)).y;
            headerToolBarRect = SirenixEditorGUI.BeginHorizontalToolbar(titleHeight + 12f);
            var titleRect = headerToolBarRect.AlignCenter(titleWidth);
            EditorGUI.LabelField(titleRect, title, titleStyle);
            GUILayout.FlexibleSpace();
            SirenixEditorGUI.EndHorizontalToolbar();
            GUILayout.Space(-2);
            return EditorGUILayout.BeginVertical(AttributeOverviewEditorUtility.ContainerContentStyle);
        }

        internal static void EndDrawContainerWithTitle(Rect contentRect)
        {
            EditorGUILayout.EndVertical();
            SirenixEditorGUI.DrawBorders(contentRect, 1);
        }

        #endregion
    }
}
