using System.Collections.Generic;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 特性参数渲染器，负责绘制特性参数表格。
    /// </summary>
    public class AttributeParametersRenderer : IAttributeComponentRenderer
    {
        static readonly BilingualData _attributeParametersTitleLabel =
            new BilingualData("特性参数", "Attribute Parameters");

        static readonly BilingualData _attributeParameterReturnTypeLabel =
            new BilingualData("返回值类型", "Return Type");

        static readonly BilingualData _attributeParameterParamNameLabel =
            new BilingualData("参数名", "Parameter Name");

        static readonly BilingualData _attributeParameterParamDescriptionLabel =
            new BilingualData("参数描述", "Parameter Description");

        static readonly BilingualData _guiTableNumberLabel = new BilingualData("序号", "Number");

        readonly Dictionary<string, float> _attributeParameterTextHeightCache =
            new Dictionary<string, float>();

        ParameterValue[] _attributeParameters;
        GUITable _attributeParametersTable;

        public bool IsVisible => _attributeParameters != null && _attributeParameters.Length > 0;

        public void Draw()
        {
            if (!IsVisible)
            {
                return;
            }

            Rect headerToolBarRect;
            var contentRect =
                AbstractAttributePanelSO.BeginDrawContainerWithTitle(_attributeParametersTitleLabel,
                    out headerToolBarRect);
            _attributeParametersTable.DrawTable();
            ResizeAttributeParameterTable();
            AbstractAttributePanelSO.EndDrawContainerWithTitle(contentRect);
        }

        public void OnLanguageChanged()
        {
            _attributeParameterTextHeightCache.Clear();
        }

        public void Reset()
        {
            _attributeParameterTextHeightCache.Clear();
        }

        public void SetData(ParameterValue[] parameters)
        {
            _attributeParameters = parameters;
            if (IsVisible)
            {
                CreateAttributeParametersTable();
            }
        }

        void CreateAttributeParametersTable()
        {
            _attributeParametersTable = GUITable.Create(_attributeParameters, null, new GUITableColumn
            {
                ColumnTitle = _guiTableNumberLabel,
                Width = 60,
                OnGUI = (rect, index) =>
                {
                    EditorGUI.LabelField(rect, (index + 1).ToString(),
                        AttributeOverviewEditorUtility.TableCellTextStyle);
                }
            }, new GUITableColumn
            {
                ColumnTitle = _attributeParameterReturnTypeLabel,
                Width = 140,
                OnGUI = (rect, index) =>
                {
                    EditorGUI.LabelField(rect, _attributeParameters[index].ReturnType,
                        AttributeOverviewEditorUtility.TableCellTextStyle);
                }
            }, new GUITableColumn
            {
                ColumnTitle = _attributeParameterParamNameLabel,
                MinWidth = 140,
                OnGUI = (rect, index) =>
                {
                    EditorGUI.LabelField(rect, _attributeParameters[index].ParameterName,
                        AttributeOverviewEditorUtility.TableCellTextStyle);
                }
            }, new GUITableColumn
            {
                ColumnTitle = _attributeParameterParamDescriptionLabel,
                MinWidth = 200,
                OnGUI = (rect, index) =>
                {
                    EditorGUI.LabelField(rect, _attributeParameters[index].GetDescription(),
                        AttributeOverviewEditorUtility.TableCellTextStyle);
                }
            });
        }

        void ResizeAttributeParameterTable()
        {
            var tableRowHeight = new int[_attributeParametersTable.RowCount];
            for (var row = 0; row < _attributeParametersTable.RowCount; row++)
            {
                for (var col = 0; col < _attributeParametersTable.ColumnCount; col++)
                {
                    var width = _attributeParametersTable[col, row].Rect.width;
                    if (row == 0)
                    {
                        tableRowHeight[0] = (int)Mathf.Max(
                            AbstractAttributePanelSO.GetCachedTextHeight(_attributeParameterReturnTypeLabel,
                                width, _attributeParameterTextHeightCache),
                            AbstractAttributePanelSO.GetCachedTextHeight(_attributeParameterParamNameLabel,
                                width, _attributeParameterTextHeightCache),
                            AbstractAttributePanelSO.GetCachedTextHeight(
                                _attributeParameterParamDescriptionLabel, width,
                                _attributeParameterTextHeightCache));
                    }
                    else
                    {
                        tableRowHeight[row] = (int)Mathf.Max(
                            AbstractAttributePanelSO.GetCachedTextHeight(
                                _attributeParameters[row - 1].ReturnType, width,
                                _attributeParameterTextHeightCache),
                            AbstractAttributePanelSO.GetCachedTextHeight(
                                _attributeParameters[row - 1].ParameterName, width,
                                _attributeParameterTextHeightCache),
                            AbstractAttributePanelSO.GetCachedTextHeight(
                                _attributeParameters[row - 1].GetDescription(), width,
                                _attributeParameterTextHeightCache));
                    }
                }

                _attributeParametersTable[0, row].Height = tableRowHeight[row] + 10f;
            }

            _attributeParametersTable.ReCalculateSizes();
        }
    }
}
