using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Attribute Overview 功能的编辑器 GUI 工具类，提供懒加载样式与代码处理方法。
    /// </summary>
    public static class AttributeOverviewEditorUtility
    {
        const int ContainerContentPadding = 10;

        static GUIStyle _containerTitleStyle;
        static GUIStyle _containerContentStyle;
        static GUIStyle _tableCellTextStyle;
        static GUIStyle _resolvedStringParameterValueTitleStyle;
        static GUIStyle _tabButtonCellTextStyle;
        static GUIStyle _codeTextEditorStyle;

        /// <summary>
        /// 容器标题样式。
        /// </summary>
        public static GUIStyle ContainerTitleStyle
        {
            get
            {
                _containerTitleStyle ??= new GUIStyle(SirenixGUIStyles.TitleCentered) { fontSize = 16 };
                return _containerTitleStyle;
            }
        }

        /// <summary>
        /// 容器内容区域样式。
        /// </summary>
        public static GUIStyle ContainerContentStyle
        {
            get
            {
                _containerContentStyle ??= new GUIStyle(SirenixGUIStyles.ToolbarBackground)
                {
                    stretchHeight = false,
                    padding = new RectOffset(ContainerContentPadding, ContainerContentPadding,
                        ContainerContentPadding, ContainerContentPadding)
                };
                return _containerContentStyle;
            }
        }

        /// <summary>
        /// 表格单元格文本样式。
        /// </summary>
        public static GUIStyle TableCellTextStyle
        {
            get
            {
                _tableCellTextStyle ??= new GUIStyle(SirenixGUIStyles.MultiLineCenteredLabel)
                {
                    padding = new RectOffset(5, 5, 5, 5),
                    clipping = TextClipping.Overflow,
                    richText = true
                };
                return _tableCellTextStyle;
            }
        }

        /// <summary>
        /// 被解析字符串参数标题样式。
        /// </summary>
        public static GUIStyle ResolvedStringParameterValueTitleStyle
        {
            get
            {
                _resolvedStringParameterValueTitleStyle ??= new GUIStyle(SirenixGUIStyles.TitleCentered)
                    { fontSize = 16 };
                return _resolvedStringParameterValueTitleStyle;
            }
        }

        /// <summary>
        /// 案例标签页按钮样式。
        /// </summary>
        public static GUIStyle TabButtonCellTextStyle
        {
            get
            {
                var textColor = EditorGUIUtility.isProSkin ? Color.white : new Color(0.12f, 0.12f, 0.12f);
                _tabButtonCellTextStyle ??= new GUIStyle
                {
                    normal = new GUIStyleState { textColor = textColor },
                    active = new GUIStyleState { textColor = textColor },
                    hover = new GUIStyleState { textColor = textColor },
                    focused = new GUIStyleState { textColor = textColor },
                    padding = new RectOffset(10, 10, 2, 2),
                    alignment = TextAnchor.MiddleCenter,
                    clipping = TextClipping.Overflow
                };
                return _tabButtonCellTextStyle;
            }
        }

        /// <summary>
        /// 代码预览文本编辑器样式。
        /// </summary>
        public static GUIStyle CodeTextEditorStyle
        {
            get
            {
                _codeTextEditorStyle ??= new GUIStyle(SirenixGUIStyles.MultiLineLabel)
                {
                    normal = new GUIStyleState { textColor = OdinCodeHighlighter.TextColor },
                    active = new GUIStyleState { textColor = OdinCodeHighlighter.TextColor },
                    focused = new GUIStyleState { textColor = OdinCodeHighlighter.TextColor },
                    wordWrap = false,
                    fontSize = 12
                };
                return _codeTextEditorStyle;
            }
        }

        #region Internal

        [InitializeOnEnterPlayMode]
        static void ResetStyles()
        {
            _containerTitleStyle = null;
            _containerContentStyle = null;
            _tableCellTextStyle = null;
            _resolvedStringParameterValueTitleStyle = null;
            _tabButtonCellTextStyle = null;
            _codeTextEditorStyle = null;
        }

        #endregion

        #region --- Public Methods ---

        /// <summary>
        /// 从示例类型获取 AesirExampleAttribute。
        /// </summary>
        public static AesirExampleAttribute GetAttributeInExampleType(Type exampleType)
        {
            if (exampleType != null)
            {
                return TypeCache.GetTypesWithAttribute<AesirExampleAttribute>()
                    .FirstOrDefault(type => type == exampleType)?.GetCustomAttribute<AesirExampleAttribute>();
            }

            Debug.LogError("[AttributeOverview] exampleType 不能为空");
            return null;
        }

        /// <summary>
        /// 读取示例文件源码并移除 namespace 包裹层与 AesirExample 特性标注行。
        /// </summary>
        public static string GetExampleSourceCodeWithoutNamespace(AesirExampleAttribute attribute)
        {
            if (attribute == null)
            {
                const string Msg = "attribute 不能为空，可能是案例没有添加 [AesirExample] 特性";
                Debug.LogError("[AttributeOverview] " + Msg);
                return Msg;
            }

            var readLines = File.ReadLines(attribute.FilePath);
            var result = new List<string>();
            var isInNamespace = false;
            var exampleAttrShort = nameof(AesirExampleAttribute)[
                ..(nameof(AesirExampleAttribute).Length - "Attribute".Length)];

            foreach (var line in readLines)
            {
                if ((line.StartsWith("using") && !isInNamespace) || line.StartsWith("#"))
                {
                    result.Add(line);
                    continue;
                }

                if (line.StartsWith("namespace"))
                {
                    isInNamespace = true;
                    continue;
                }

                if (line.TrimStart().StartsWith("[" + exampleAttrShort + "]"))
                {
                    continue;
                }

                if (isInNamespace)
                {
                    if (line.StartsWith("{"))
                    {
                        continue;
                    }

                    if (line.StartsWith("}"))
                    {
                        isInNamespace = false;
                        continue;
                    }

                    result.Add(line.Length > 4 ? line[4..] : line);
                }
                else
                {
                    result.Add(line);
                }
            }

            return string.Join("\n", result);
        }

        /// <summary>
        /// 从完整源码提取简化代码（仅保留字段特性与字段声明行）。
        /// </summary>
        public static string GetExampleShortenCode(string sourceCode)
        {
            var lines = sourceCode.Split('\n').ToList();
            var shortenLines = new List<string>();
            var isInClass = false;

            foreach (var line in lines)
            {
                if (line.StartsWith("<"))
                {
                    continue;
                }

                if (!isInClass)
                {
                    if (line.StartsWith("{"))
                    {
                        isInClass = true;
                    }

                    continue;
                }

                if (line.StartsWith("{") || line.StartsWith("}"))
                {
                    continue;
                }

                if (line.StartsWith("using") || line.StartsWith("namespace"))
                {
                    continue;
                }

                if (line.StartsWith("public") || line.StartsWith("private") || line.StartsWith("protected") ||
                    line.StartsWith("internal"))
                {
                    continue;
                }

                if (line.StartsWith("#"))
                {
                    shortenLines.Add(line);
                    continue;
                }

                if (line.StartsWith(" "))
                {
                    shortenLines.Add(line.Length > 4 ? line[4..] : line);
                    continue;
                }

                shortenLines.Add(line);
            }

            while (shortenLines.Count > 0 && shortenLines[0] == "")
            {
                shortenLines.RemoveAt(0);
            }

            return string.Join("\n", shortenLines);
        }

        /// <summary>
        /// 输出重置成功日志（双语）。
        /// </summary>
        public static void LogEditorResetSuccess(string typeName)
        {
            if (AesirInspectorLanguageSettingsSO.CurrentIsChinese)
            {
                Debug.Log(typeName + " 重置成功！");
            }
            else
            {
                Debug.Log(typeName + " reset success!");
            }
        }

        /// <summary>
        /// 输出未实现重置接口的警告日志（双语）。
        /// </summary>
        public static void LogEditorResetWarning(string typeName)
        {
            if (AesirInspectorLanguageSettingsSO.CurrentIsChinese)
            {
                Debug.LogWarning("当前案例脚本类为：" + typeName + "，没有实现 IAesirInspectorReset 接口！");
            }
            else
            {
                Debug.LogWarning("Current example script class: " + typeName +
                                 " does not implement IAesirInspectorReset interface!");
            }
        }

        #endregion
    }
}
