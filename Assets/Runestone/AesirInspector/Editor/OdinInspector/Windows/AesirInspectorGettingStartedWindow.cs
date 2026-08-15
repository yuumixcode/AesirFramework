using System;
using Runestone.AesirInspector.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Aesir Inspector Getting Started 窗口
    /// </summary>
    [InitializeOnLoad]
    public class AesirInspectorGettingStartedWindow : OdinEditorWindow
    {
        static readonly BilingualData SloganData = new BilingualData(
            "基于 Odin Inspector 的双语 Inspector 扩展，优化编辑器开发体验",
            "Bilingual Inspector extension based on Odin Inspector, optimizing editor development workflow.");

        static readonly BilingualData FeaturesTitleData = new BilingualData("主要功能", "Key Features");

        [PropertyOrder(-90)]
        public HorizontalSeparateControl separate1;

        [PropertyOrder(-30)]
        public HorizontalSeparateControl separate2;

        [PropertyOrder(-10)]
        [TableList(HideToolbar = true, AlwaysExpanded = true, IsReadOnly = true)]
        public SummaryDetailGroup[] features;

        GUIStyle _sectionTitleStyle;

        GUIStyle _sloganStyle;

        static AesirInspectorGettingStartedWindow()
        {
            // 首次导入时不再自动打开 Getting Started 窗口
            // 用户可通过菜单 Tools > Aesir > Inspector > Getting Started 手动打开
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            WindowPadding = new Vector4(10f, 10f, 10f, 10f);
            AesirInspectorLanguageSettingsSO.LanguageChanged -= Repaint;
            AesirInspectorLanguageSettingsSO.LanguageChanged += Repaint;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            AesirInspectorLanguageSettingsSO.LanguageChanged -= Repaint;
        }

        [PropertyOrder(-60)]
        [OnInspectorGUI]
        void DrawInitButton()
        {
            if (AesirInspectorProjectSettingsSO.Instance.IsInitialized)
            {
                EditorGUILayout.HelpBox(
                    AesirInspectorLanguageSettingsSO.CurrentIsEnglish
                        ? "Aesir Inspector has been initialized."
                        : "Aesir Inspector 已完成初始化。", MessageType.Info);
                return;
            }

            if (GUILayout.Button(
                    AesirInspectorLanguageSettingsSO.CurrentIsEnglish
                        ? "Initialize Aesir Inspector"
                        : "初始化 Aesir Inspector (生成 100+ 案例资产)", GUILayout.Height(40)))
            {
                InitAesirInspector();
            }
        }

        public static void InitAesirInspector()
        {
            try
            {
                // 初始化 Database (内部已包含 Panels 和 Examples 的生成及进度条)
                var database = AttributeOverviewDatabaseSO.Instance;
                if (database == null)
                {
                    Debug.LogError("Failed to get AttributeOverviewDatabaseSO instance.");
                    return;
                }

                database.Initialize();

                AesirInspectorProjectSettingsSO.Instance.IsInitialized = true;
                Debug.Log("Aesir Inspector initialized successfully!");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        [OnInspectorInit]
        void EnsureContent()
        {
            separate1 = new HorizontalSeparateControl(2, 1, 15f, 10f);
            separate2 = new HorizontalSeparateControl(2, 1, 15f, 10f);
            features = new[]
            {
                new SummaryDetailGroup
                {
                    summary = "双语 Inspector",
                    details = "提供 BilingualTitle、BilingualText、BilingualButton 等双语特性，Inspector 中中英文无缝切换。"
                },
                new SummaryDetailGroup
                {
                    summary = "Summary 注释特性",
                    details = "SummaryAttribute 等效于 XML 注释中的 Summary 部分，支持快捷同步、替换、删除操作。"
                },
                new SummaryDetailGroup
                {
                    summary = "Attribute Overview Pro",
                    details = "以可搜索的树形菜单展示所有已注册的 Odin Inspector 特性面板。"
                },
                new SummaryDetailGroup
                {
                    summary = "Script Doc Generator",
                    details = "基于 SummaryAttribute 一键生成 Scripting API 文档，支持多种输出格式。"
                },
                new SummaryDetailGroup
                {
                    summary = "Mini Tools",
                    details = "提供语法高亮处理器、菜单项检查器等便捷小工具。"
                },
                new SummaryDetailGroup
                {
                    summary = "Extension Package Manager",
                    details = "提供快捷安装扩展包模块，基于 Git URL 安装其他优质开源包。"
                }
            };
        }

        [PropertyOrder(-1000)]
        [OnInspectorGUI]
        void DrawVersionButton()
        {
            var rect = GUILayoutUtility.GetRect(1f, EditorGUIUtility.singleLineHeight + 6f,
                GUILayout.ExpandWidth(true));
            var label = AesirInspectorLanguageSettingsSO.CurrentIsEnglish
                ? "Version - v" + AesirInspectorVersion.Version
                : "当前版本 - v" + AesirInspectorVersion.Version;
            var style = new GUIStyle(SirenixGUIStyles.Button)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            var content = new GUIContent(label);
            var size = style.CalcSize(content);
            const float Padding = 4f;
            var width = Mathf.Min(size.x + Padding * 2f, rect.width);
            var height = Mathf.Max(EditorGUIUtility.singleLineHeight, rect.height - Padding);
            var buttonRect = new Rect(rect.xMax - width, rect.y + (rect.height - height) * 0.5f, width,
                height);
            if (GUI.Button(buttonRect, content, style))
            {
                Application.OpenURL(AesirInspectorWebLinks.GithubRepository);
            }
        }

        [PropertyOrder(-100)]
        [OnInspectorGUI]
        void DrawSlogan()
        {
            EnsureStyles();
            GUILayout.Label(SloganData, _sloganStyle);
        }

        [PropertyOrder(-80)]
        [HorizontalGroup("Docs")]
        [Button("Odin Inspector 官方文档", ButtonSizes.Medium)]
        public void OpenOdinInspectorDocs() =>
            Application.OpenURL(AesirInspectorWebLinks.OdinInspectorDocsUrl);

        [PropertySpace(10, 10)]
        [PropertyOrder(-50)]
        [HorizontalGroup("OpenSource")]
        [Button("LICENSE", ButtonSizes.Medium)]
        public void OpenLicense() => Application.OpenURL(AesirInspectorWebLinks.LicenseUrl);

        [PropertySpace(10, 10)]
        [PropertyOrder(-40)]
        [HorizontalGroup("OpenSource")]
        [Button("CHANGELOG", ButtonSizes.Medium)]
        public void OpenChangelog() => Application.OpenURL(AesirInspectorWebLinks.ChangelogUrl);

        [PropertyOrder(-20)]
        [OnInspectorGUI]
        void FeaturesSection()
        {
            GUILayout.Label(FeaturesTitleData, _sectionTitleStyle);
            GUILayout.Space(10f);
        }

        /// <summary>
        /// 打开 Getting Started 窗口
        /// </summary>
        public static void OpenWindow()
        {
            var window = GetWindow<AesirInspectorGettingStartedWindow>();
            window.titleContent = new GUIContent(AesirInspectorMenuItems.GettingStartedWindowName);
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 800);
            window.Show();
        }

        void EnsureStyles()
        {
            if (_sloganStyle != null)
            {
                return;
            }

            _sloganStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                fontSize = 20
            };
            _sectionTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                fontSize = 18
            };
        }

        /// <summary>
        /// 功能摘要-详情组，用于在 Getting Started 窗口中展示功能列表
        /// </summary>
        [Serializable]
        public class SummaryDetailGroup
        {
            [DisplayAsString(TextAlignment.Center, FontSize = 14)]
            public string summary;

            [DisplayAsString(TextAlignment.Left, FontSize = 14)]
            public string details;
        }
    }
}
