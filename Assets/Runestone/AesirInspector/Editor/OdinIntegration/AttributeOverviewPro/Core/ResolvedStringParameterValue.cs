using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 解析器类型枚举。
    /// </summary>
    public enum ResolverType
    {
        ValueResolver = 0,
        ActionResolver = 1
    }

    /// <summary>
    /// 被解析的字符串参数数据类，包含解析器信息表与命名值表的绘制逻辑。
    /// </summary>
    public class ResolvedStringParameterValue
    {
        static readonly BilingualData ResolverTypeLabel = new BilingualData("解析器类型", "Resolver Type");

        static readonly BilingualData ResolverTargetTypeLabel =
            new BilingualData("解析器目标类型", "Resolver Target Type");

        static readonly BilingualData FallbackValueLabel = new BilingualData("回退值", "Fallback Value");

        internal static readonly BilingualData NamedValuesLabel =
            new BilingualData("特殊命名参数值 - Named Values", "Named Values");

        static readonly ParameterValue[] DefaultExistedNamedValues =
        {
            new ParameterValue(typeof(InspectorProperty).FullName, "$property",
                new BilingualData(
                    "InspectorProperty 代表检查器中的一个 Property，即应用此特性的成员。类似于 Unity 的 SerializedProperty。",
                    "The InspectorProperty representing the member that has attribute applied to it. Similar to Unity's SerializedProperty.")),
            new ParameterValue("TParent", "$root",
                new BilingualData("TParent 代表此成员所在的类。可以通过这个类访问类中的其他成员。",
                    "The TParent representing the parent type of the member that has attribute applied to it."))
        };

        public ResolvedStringParameterValue(string parameterName,
            ResolverType resolverType,
            string resolverTargetType,
            string fallbackValue,
            List<ParameterValue> additionalNamedValues)
        {
            ParameterName = parameterName;
            ResolverType = resolverType;
            ResolverTargetType = resolverTargetType;
            FallbackValue = fallbackValue;
            NamedValues = new List<ParameterValue>(DefaultExistedNamedValues);
            NamedValues.AddRange(additionalNamedValues);
            CreateResolverInfoTable();
            CreateNamedValueTable();
            ResizeAllTables();
        }

        /// <summary>
        /// 参数名称。
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// 解析器类型。
        /// </summary>
        public ResolverType ResolverType { get; }

        /// <summary>
        /// 解析器目标类型名。
        /// </summary>
        public string ResolverTargetType { get; }

        /// <summary>
        /// 回退值。
        /// </summary>
        public string FallbackValue { get; }

        /// <summary>
        /// 命名参数值列表。
        /// </summary>
        public List<ParameterValue> NamedValues { get; }

        /// <summary>
        /// 解析器信息 GUI 表格。
        /// </summary>
        public GUITable ResolverInfoTable { get; private set; }

        /// <summary>
        /// 命名参数值 GUI 表格。
        /// </summary>
        public GUITable NamedValueTable { get; private set; }

        /// <summary>
        /// 重建解析器信息表格。
        /// </summary>
        public void CreateResolverInfoTable()
        {
            ResolverInfoTable = GUITable.Create(1, null, new GUITableColumn
            {
                ColumnTitle = ResolverTypeLabel,
                MinWidth = 100f,
                OnGUI = (rect, _) => DrawTableCell(rect, GetResolverTypeString())
            }, new GUITableColumn
            {
                ColumnTitle = ResolverTargetTypeLabel,
                MinWidth = 140f,
                OnGUI = (rect, _) => DrawTableCell(rect, ResolverTargetType)
            }, new GUITableColumn
            {
                ColumnTitle = FallbackValueLabel,
                MinWidth = 100f,
                OnGUI = (rect, _) => DrawTableCell(rect, FallbackValue)
            });
        }

        /// <summary>
        /// 重建命名参数值表格。
        /// </summary>
        public void CreateNamedValueTable()
        {
            NamedValueTable = GUITable.Create(NamedValues, null, new GUITableColumn
            {
                ColumnTitle = new BilingualData("参数类型", "Parameter Type"),
                MinWidth = 140f,
                OnGUI = (rect, index) => DrawTableCell(rect, NamedValues[index].ReturnType)
            }, new GUITableColumn
            {
                ColumnTitle = new BilingualData("参数名", "Parameter Name"),
                MinWidth = 140f,
                OnGUI = (rect, index) => DrawTableCell(rect, NamedValues[index].ParameterName)
            }, new GUITableColumn
            {
                ColumnTitle = new BilingualData("参数描述", "Parameter Description"),
                MinWidth = 200f,
                OnGUI = (rect, index) => DrawTableCell(rect, NamedValues[index].GetDescription())
            });
        }

        /// <summary>
        /// 根据当前宽度重新计算所有表格行高。
        /// </summary>
        public void ResizeAllTables()
        {
            var resolverTypeHeight = CalculateHeight(GetResolverTypeString(), ResolverInfoTable, 0, 1);
            var resolvesToHeight = CalculateHeight(ResolverTargetType, ResolverInfoTable, 1, 1);
            var fallbackValueHeight = CalculateHeight(FallbackValue, ResolverInfoTable, 2, 1);
            var maxHeight = Mathf.Max(resolverTypeHeight, resolvesToHeight, fallbackValueHeight);
            ResolverInfoTable[0, 1].Height = maxHeight + 10f;

            for (var row = 1; row < NamedValueTable.RowCount; row++)
            {
                var namedValue = NamedValues[row - 1];
                var nameHeight = CalculateHeight(namedValue.ParameterName, NamedValueTable, 0, row);
                var typeHeight = CalculateHeight(namedValue.ReturnType, NamedValueTable, 1, row);
                var descriptionHeight = CalculateHeight(namedValue.GetDescription(), NamedValueTable, 2, row);
                maxHeight = Mathf.Max(nameHeight, typeHeight, descriptionHeight);
                for (var col = 0; col < NamedValueTable.ColumnCount; col++)
                {
                    NamedValueTable[col, row].Height = maxHeight + 10f;
                }
            }

            ResolverInfoTable.ReCalculateSizes();
            NamedValueTable.ReCalculateSizes();
        }

        static float CalculateHeight(string content, GUITable table, int col, int row) =>
            AttributeOverviewEditorUtility.TableCellTextStyle.CalcHeight(GUIHelper.TempContent(content),
                table[col, row].Rect.width);

        static void DrawTableCell(Rect rect, string text) =>
            EditorGUI.LabelField(rect, text, AttributeOverviewEditorUtility.TableCellTextStyle);

        string GetResolverTypeString() =>
            ResolverType switch
            {
                ResolverType.ValueResolver => "Value Resolver",
                ResolverType.ActionResolver => "Action Resolver",
                _ => ResolverType.ToString()
            };
    }
}
