using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 示例预览渲染器，负责绘制特性的使用示例及其控制按钮。
    /// </summary>
    public class ExamplePreviewRenderer : IAttributeComponentRenderer
    {
        const int ExampleNumberOneRow = 3;
        static readonly BilingualData _usageExampleLabel = new BilingualData("使用案例预览", "Usage Examples");

        static readonly BilingualData _pingMonoScriptButtonLabel =
            new BilingualData("Ping 脚本文件", "Ping Script File");

        static readonly BilingualData _resetExampleButtonLabel = new BilingualData("重置案例", "Reset Example");

        readonly AbstractAttributePanelSO _panel;
        AttributeExamplePreviewItem[] _examplePreviewItems;
        Rect _usageExampleContentRect;
        Rect _usageHeaderToolbarRect;

        public ExamplePreviewRenderer(AbstractAttributePanelSO panel) => _panel = panel;

        public bool IsVisible => _examplePreviewItems != null && _examplePreviewItems.Length > 0;

        public void Draw()
        {
            // 默认 Draw 不再执行完整逻辑，由 BeginDraw/EndDraw 替代以支持跨成员容器包裹
            BeginDraw();
            EndDraw();
        }

        public void OnLanguageChanged() { }

        public void Reset() { }

        /// <summary>
        /// 开始绘制示例预览容器。
        /// </summary>
        public void BeginDraw()
        {
            if (!IsVisible)
            {
                return;
            }

            _usageExampleContentRect =
                AbstractAttributePanelSO.BeginDrawContainerWithTitle(_usageExampleLabel,
                    out _usageHeaderToolbarRect);

            DrawExamplePreviewItems();
        }

        /// <summary>
        /// 结束绘制示例预览容器。
        /// </summary>
        public void EndDraw()
        {
            if (!IsVisible)
            {
                return;
            }

            AbstractAttributePanelSO.EndDrawContainerWithTitle(_usageExampleContentRect);
            DrawUsageExampleTitleButton();
        }

        public void SetData(AttributeExamplePreviewItem[] items)
        {
            _examplePreviewItems = items;
        }

        void DrawExamplePreviewItems()
        {
            if (_examplePreviewItems is not { Length: > 1 })
            {
                return;
            }

            EditorGUILayout.BeginVertical();
            for (var i = 0; i < _examplePreviewItems.Length; i += ExampleNumberOneRow)
            {
                EditorGUILayout.BeginHorizontal();
                for (var j = 0; j < ExampleNumberOneRow && i + j < _examplePreviewItems.Length; j++)
                {
                    DrawExampleTabButton(_examplePreviewItems[i + j]);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(10f);
        }

        void DrawExampleTabButton(AttributeExamplePreviewItem item)
        {
            var content = GUIHelper.TempContent(" " + item.ItemName,
                GUIHelper.GetAssetThumbnail(null, typeof(MonoBehaviour), false));

            var selectExample = item.ExampleType == AttributeExampleType.OdinSerialized
                ? item.OdinSerializedExample
                : item.UnitySerializedExample;

            var currentSelected = _panel.CurrentSelectedExample;
            var isSelected = selectExample == currentSelected;

            var iconSizeBackup = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
            var rect = GUILayoutUtility.GetRect(content,
                AttributeOverviewEditorUtility.TabButtonCellTextStyle, GUILayoutOptions.Height(26));

            if (Event.current.type == EventType.Repaint)
            {
                Color color;
                if (isSelected)
                {
                    color = EditorGUIUtility.isProSkin
                        ? new Color(0.25f, 0.4f, 0.6f, 1f)
                        : new Color(0.75f, 0.83f, 0.9f, 1f);
                }
                else
                {
                    color = EditorGUIUtility.isProSkin
                        ? new Color(0.3f, 0.3f, 0.3f, 1f)
                        : new Color(0.85f, 0.85f, 0.85f, 1f);
                }

                SirenixEditorGUI.DrawSolidRect(rect, color);
                SirenixEditorGUI.DrawBorders(rect, 1);
            }

            if (GUI.Button(rect, content, AttributeOverviewEditorUtility.TabButtonCellTextStyle))
            {
                _panel.CurrentSelectedExample = selectExample;
            }

            EditorGUIUtility.SetIconSize(iconSizeBackup);
        }

        void DrawUsageExampleTitleButton()
        {
            var headerButtonRect = _usageHeaderToolbarRect.AlignCenterY(_usageHeaderToolbarRect.height)
                .AlignRight(240);
            var leftButtonRect = headerButtonRect.Split(0, 2);
            var pingTexture =
                SdfIcons.CreateTransparentIconTexture(SdfIconType.HandIndexFill, Color.white, 20, 20, 0);
            if (GUI.Button(leftButtonRect,
                    GUIHelper.TempContent(" " + _pingMonoScriptButtonLabel, pingTexture),
                    SirenixGUIStyles.ToolbarButton))
            {
                EditorGUIUtility.PingObject(GetCurrentExampleMonoScript());
            }

            var rightButtonRect = headerButtonRect.Split(1, 2);
            var resetTexture =
                SdfIcons.CreateTransparentIconTexture(SdfIconType.ArrowClockwise, Color.white, 20, 20, 0);
            if (GUI.Button(rightButtonRect,
                    GUIHelper.TempContent(" " + _resetExampleButtonLabel, resetTexture),
                    SirenixGUIStyles.ToolbarButton))
            {
                var currentSelected = _panel.CurrentSelectedExample;
                if (currentSelected is IAesirInspectorReset canReset)
                {
                    canReset.AesirInspectorReset();
                    AttributeOverviewEditorUtility.LogEditorResetSuccess(currentSelected.GetType().Name);
                }
                else if (currentSelected != null)
                {
                    AttributeOverviewEditorUtility.LogEditorResetWarning(currentSelected.GetType().Name);
                }
            }
        }

        Object GetCurrentExampleMonoScript()
        {
            var currentSelected = _panel.CurrentSelectedExample;
            if (!currentSelected)
            {
                return null;
            }

            var markAttribute =
                AttributeOverviewEditorUtility.GetAttributeInExampleType(currentSelected.GetType());
            if (markAttribute == null)
            {
                return null;
            }

            var monoScriptAbsolutePath = markAttribute.FilePath;
            var assetRelativePath =
                "Assets/" + PathUtilities.MakeRelative(Application.dataPath, monoScriptAbsolutePath);
            return AssetDatabase.LoadAssetAtPath<Object>(assetRelativePath);
        }
    }
}
