using System.Collections.Generic;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 使用提示渲染器，负责绘制特性使用的提示表格。
    /// </summary>
    public class UsageTipsRenderer : IAttributeComponentRenderer
    {
        static readonly BilingualData _usageTipsLabel = new BilingualData("使用提示", "Usage Tips");
        static readonly BilingualData _guiTableNumberLabel = new BilingualData("序号", "Number");

        readonly AbstractAttributePanelSO _panel;
        readonly Dictionary<string, float> _usageTipsTextHeightCache = new Dictionary<string, float>();
        BilingualData[] _usageTips;
        GUITable _usageTipsTable;

        public UsageTipsRenderer(AbstractAttributePanelSO panel) => _panel = panel;

        public bool IsVisible => _usageTips != null && _usageTips.Length > 0;

        public void Draw()
        {
            if (!IsVisible)
            {
                return;
            }

            Rect headerToolBarRect;
            var contentRect =
                AbstractAttributePanelSO.BeginDrawContainerWithTitle(_usageTipsLabel, out headerToolBarRect);
            _usageTipsTable.DrawTable();
            ResizeUsageTipsTable();
            AbstractAttributePanelSO.EndDrawContainerWithTitle(contentRect);
        }

        public void OnLanguageChanged()
        {
            _usageTipsTextHeightCache.Clear();
        }

        public void Reset()
        {
            _usageTipsTextHeightCache.Clear();
        }

        public void SetData(BilingualData[] usageTips)
        {
            _usageTips = usageTips;
            if (IsVisible)
            {
                CreateUsageTipsTable();
            }
        }

        void CreateUsageTipsTable()
        {
            _usageTipsTable = GUITable.Create(_usageTips, null, new GUITableColumn
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
                ColumnTitle = _usageTipsLabel,
                MinWidth = 200,
                OnGUI = (rect, index) =>
                {
                    EditorGUI.LabelField(rect, _usageTips[index],
                        AttributeOverviewEditorUtility.TableCellTextStyle);
                }
            });
        }

        void ResizeUsageTipsTable()
        {
            var tableRowHeight = new int[_usageTipsTable.RowCount];
            for (var row = 0; row < _usageTipsTable.RowCount; row++)
            {
                for (var col = 0; col < _usageTipsTable.ColumnCount; col++)
                {
                    var width = _usageTipsTable[col, row].Rect.width;
                    if (row == 0)
                    {
                        tableRowHeight[0] =
                            (int)AbstractAttributePanelSO.GetCachedTextHeight(_usageTipsLabel, width,
                                _usageTipsTextHeightCache);
                    }
                    else
                    {
                        tableRowHeight[row] =
                            (int)AbstractAttributePanelSO.GetCachedTextHeight(_usageTips[row - 1], width,
                                _usageTipsTextHeightCache);
                    }
                }

                _usageTipsTable[0, row].Height = tableRowHeight[row] + 10f;
            }

            _usageTipsTable.ReCalculateSizes();
        }
    }
}
