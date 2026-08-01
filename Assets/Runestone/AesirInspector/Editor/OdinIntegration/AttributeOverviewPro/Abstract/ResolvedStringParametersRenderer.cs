using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 解析字符串参数渲染器，负责绘制 ResolvedStringParameterValue 列表。
    /// </summary>
    public class ResolvedStringParametersRenderer : IAttributeComponentRenderer
    {
        static readonly BilingualData _resolvedStringParameterLabel =
            new BilingualData("被解析的字符串参数", "Resolved String Parameters");

        ResolvedStringParameterValue[] _resolvedStringParameters;

        public bool IsVisible => _resolvedStringParameters != null && _resolvedStringParameters.Length > 0;

        public void Draw()
        {
            if (!IsVisible)
            {
                return;
            }

            var contentRect =
                AbstractAttributePanelSO.BeginDrawContainerWithTitle(_resolvedStringParameterLabel, out _);

            SirenixEditorGUI.BeginVerticalList();
            foreach (var resolvedString in _resolvedStringParameters)
            {
                SirenixEditorGUI.BeginListItem(false);
                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(8);
                EditorGUILayout.BeginVertical();
                GUILayout.Space(5);
                GUILayout.Label(resolvedString.ParameterName,
                    AttributeOverviewEditorUtility.ResolvedStringParameterValueTitleStyle);
                GUILayout.Space(5);
                resolvedString.ResolverInfoTable.DrawTable();
                GUILayout.Space(5);
                GUILayout.Label(ResolvedStringParameterValue.NamedValuesLabel,
                    AttributeOverviewEditorUtility.ResolvedStringParameterValueTitleStyle);
                resolvedString.NamedValueTable.DrawTable();
                resolvedString.ResizeAllTables();
                GUILayout.Space(8);
                EditorGUILayout.EndVertical();
                GUILayout.Space(8);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
                SirenixEditorGUI.EndListItem();
            }

            SirenixEditorGUI.EndVerticalList();

            AbstractAttributePanelSO.EndDrawContainerWithTitle(contentRect);
        }

        public void OnLanguageChanged() { }

        public void Reset() { }

        public void SetData(ResolvedStringParameterValue[] parameters)
        {
            _resolvedStringParameters = parameters;
        }
    }
}
