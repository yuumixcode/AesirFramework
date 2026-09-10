using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// Script Doc Generator 的 UI Toolkit 版窗口（纯 C# 构建 VisualElement 树，无 UXML/USS 资产）。
    /// 核心流程 MVP：类型选择（单类型 / 多类型）→ 分析 → 生成，附带文档输出路径与生成器设置。
    /// </summary>
    public class ScriptDocGeneratorToolkitWindow : EditorWindow
    {
        /// <summary>
        /// MVP 支持的类型来源模式。程序集模式（单程序集/多程序集）留待后续迭代。
        /// </summary>
        enum TypeSourceMode
        {
            SingleType,
            MultipleTypes
        }

        TypeSourceMode _typeSourceMode;
        MonoScript _selectedMonoScript;
        TypesCacheSO _typesCacheSO;
        readonly List<Type> _temporaryTypes = new List<Type>();
        ITypeData _typeData;
        List<ITypeData> _typeDataList;
        bool _hasFinishedAnalyze;
        string _docFolderPath = ScriptDocGeneratorPaths.DefaultDocFolderPath;

        EnumField _typeSourceField;
        ObjectField _monoScriptField;
        ObjectField _typesCacheField;
        VisualElement _singleTypeContainer;
        VisualElement _multiTypeContainer;
        ListView _temporaryTypesList;
        Button _analyzeButton;
        Button _generateButton;
        Label _analyzeResultLabel;
        TextField _docFolderField;
        TextField _docExtensionField;
        Label _statusLabel;

        [MenuItem(ScriptDocGeneratorMenuPaths.ScriptDocGeneratorUIToolkit, false,
            ScriptDocGeneratorMenuPaths.ScriptDocGeneratorUIToolkitOrder)]
        public static void OpenWindow()
        {
            if (!ScriptDocGeneratorUtility.EnsureInitialized())
            {
                return;
            }

            var window = GetWindow<ScriptDocGeneratorToolkitWindow>();
            window.titleContent = new GUIContent("Script Doc Generator (UITK)");
            window.minSize = new Vector2(520, 660);
            window.Show();
        }

        void CreateGUI()
        {
            BuildUI();
            SyncUIFromState();
        }

        #region UI Construction

        void BuildUI()
        {
            var root = rootVisualElement;
            root.style.paddingLeft = 12;
            root.style.paddingRight = 12;
            root.style.paddingTop = 10;
            root.style.paddingBottom = 10;

            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            root.Add(scrollView);
            var content = scrollView;

            content.Add(BuildHeader());

            content.Add(BuildSectionTitle("类型来源"));
            _typeSourceField = new EnumField("类型来源模式", _typeSourceMode);
            _typeSourceField.RegisterValueChangedCallback(_ =>
            {
                _typeSourceMode = (TypeSourceMode)_typeSourceField.value;
                _hasFinishedAnalyze = false;
                SyncUIFromState();
            });
            content.Add(_typeSourceField);

            _singleTypeContainer = BuildSingleTypeSection();
            _multiTypeContainer = BuildMultiTypeSection();
            content.Add(_singleTypeContainer);
            content.Add(_multiTypeContainer);

            _analyzeButton = new Button(PerformAnalyze) { text = "基于当前模式执行类型分析" };
            StylePrimaryButton(_analyzeButton);
            content.Add(_analyzeButton);

            _analyzeResultLabel = new Label("尚未分析")
            {
                style =
                {
                    marginTop = 6,
                    marginBottom = 10,
                    whiteSpace = WhiteSpace.Normal,
                    opacity = 0.85f
                }
            };
            content.Add(_analyzeResultLabel);

            _generateButton = new Button(PerformGenerateDoc) { text = "基于解析结果和文档生成器生成 Markdown 文档" };
            StylePrimaryButton(_generateButton);
            content.Add(_generateButton);

            content.Add(BuildDocFolderSection());
            content.Add(BuildSettingsSection());

            _statusLabel = new Label(string.Empty)
            {
                style =
                {
                    marginTop = 12,
                    whiteSpace = WhiteSpace.Normal,
                    opacity = 0.85f
                }
            };
            content.Add(_statusLabel);
        }

        static VisualElement BuildHeader()
        {
            var header = new VisualElement { style = { marginBottom = 12 } };
            header.Add(new Label("脚本文档生成工具")
            {
                style =
                {
                    fontSize = 20,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 4
                }
            });
            header.Add(new Label(
                "用户提供一个 Type 类型的值，分析 Type 数据，选择合适的文档生成器，一键生成对应的文档。默认提供中文 API 文档生成器，可以自定义适合项目的生成器。")
            {
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                    opacity = 0.75f
                }
            });
            return header;
        }

        static Label BuildSectionTitle(string title)
        {
            return new Label(title)
            {
                style =
                {
                    fontSize = 13,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginTop = 4,
                    marginBottom = 4
                }
            };
        }

        VisualElement BuildSingleTypeSection()
        {
            var container = new VisualElement();

            container.Add(BuildDropArea("拖拽 Script 文件到此处，自动识别类型", OnSingleTypeDragPerformed));

            _monoScriptField = new ObjectField("手动选择 Script：") { allowSceneObjects = false };
            _monoScriptField.objectType = typeof(MonoScript);
            _monoScriptField.RegisterValueChangedCallback(_ => SyncUIFromState());
            container.Add(_monoScriptField);
            return container;
        }

        VisualElement BuildMultiTypeSection()
        {
            var container = new VisualElement();

            _typesCacheField = new ObjectField("TypesCacheSO 资源（优先）：") { allowSceneObjects = false };
            _typesCacheField.objectType = typeof(TypesCacheSO);
            _typesCacheField.RegisterValueChangedCallback(_ => SyncUIFromState());
            container.Add(_typesCacheField);

            container.Add(BuildDropArea("拖拽多个 Script 文件到此处，追加到临时 Type 列表", OnMultiTypesDragPerformed));

            _temporaryTypesList = new ListView(_temporaryTypes)
            {
                headerTitle = "临时 Type 列表",
                showFoldoutHeader = true,
                showBorder = true,
                fixedItemHeight = 24,
                style = { height = 120, marginBottom = 4 }
            };
            _temporaryTypesList.makeItem = () =>
            {
                var row = new VisualElement
                {
                    style = { flexDirection = FlexDirection.Row, alignItems = Align.Center }
                };
                var label = new Label { name = "typeLabel", style = { flexGrow = 1 } };
                var remove = new Button { name = "removeBtn", text = "移除" };
                row.Add(label);
                row.Add(remove);
                return row;
            };
            _temporaryTypesList.bindItem = (element, i) =>
            {
                element.Q<Label>("typeLabel").text = _temporaryTypes[i].FullName ?? _temporaryTypes[i].Name;
                element.Q<Button>("removeBtn").clickable =
                    new Clickable(() => RemoveTemporaryType(i));
            };
            container.Add(_temporaryTypesList);

            var clearButton = new Button(() =>
            {
                _temporaryTypes.Clear();
                _temporaryTypesList.Rebuild();
                SyncUIFromState();
            }) { text = "清空临时 Type 列表" };
            container.Add(clearButton);
            return container;
        }

        VisualElement BuildDocFolderSection()
        {
            var section = new VisualElement { style = { marginTop = 14 } };
            section.Add(BuildSectionTitle("文档输出路径"));

            var row = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center }
            };
            _docFolderField = new TextField { isDelayed = true, style = { flexGrow = 1 } };
            _docFolderField.RegisterValueChangedCallback(e =>
            {
                _docFolderPath = string.IsNullOrWhiteSpace(e.newValue)
                    ? ScriptDocGeneratorPaths.DefaultDocFolderPath
                    : e.newValue.Trim();
                SyncUIFromState();
            });
            row.Add(_docFolderField);

            var browseButton = new Button(BrowseDocFolder) { text = "浏览…" };
            row.Add(browseButton);

            var resetButton = new Button(() =>
            {
                _docFolderPath = ScriptDocGeneratorPaths.DefaultDocFolderPath;
                SyncUIFromState();
            }) { text = "默认" };
            row.Add(resetButton);

            section.Add(row);
            return section;
        }

        VisualElement BuildSettingsSection()
        {
            var settings = DefaultScriptingAPISettingsSO.Instance;

            var foldout = new Foldout { text = "文档生成器设置" };
            foldout.style.marginTop = 8;

            var namespaceToggle = new Toggle("按命名空间生成文件夹") { value = settings.generateNamespaceFolder };
            namespaceToggle.RegisterValueChangedCallback(e =>
            {
                settings.generateNamespaceFolder = e.newValue;
                EditorUtility.SetDirty(settings);
            });
            foldout.Add(namespaceToggle);

            var identifierToggle = new Toggle("生成增量标识符") { value = settings.generateIdentifier };
            identifierToggle.RegisterValueChangedCallback(e =>
            {
                settings.generateIdentifier = e.newValue;
                EditorUtility.SetDirty(settings);
            });
            foldout.Add(identifierToggle);

            var customizeExtensionToggle = new Toggle("自定义文档扩展名")
            {
                value = settings.customizeDocFileExtensionName
            };
            _docExtensionField = new TextField("文档扩展名") { value = settings.docFileExtensionName };
            _docExtensionField.SetEnabled(settings.customizeDocFileExtensionName);
            _docExtensionField.RegisterValueChangedCallback(e =>
            {
                settings.docFileExtensionName = e.newValue;
                EditorUtility.SetDirty(settings);
            });
            customizeExtensionToggle.RegisterValueChangedCallback(e =>
            {
                settings.customizeDocFileExtensionName = e.newValue;
                _docExtensionField.SetEnabled(e.newValue);
                EditorUtility.SetDirty(settings);
            });
            foldout.Add(customizeExtensionToggle);
            foldout.Add(_docExtensionField);

            return foldout;
        }

        VisualElement BuildDropArea(string hintText, Action dragPerformed)
        {
            var area = new VisualElement
            {
                style =
                {
                    height = 44,
                    marginTop = 4,
                    marginBottom = 4,
                    justifyContent = Justify.Center,
                    alignItems = Align.Center,
                    borderBottomWidth = 1,
                    borderTopWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderBottomColor = new Color(0f, 0f, 0f, 0.35f),
                    borderTopColor = new Color(0f, 0f, 0f, 0.35f),
                    borderLeftColor = new Color(0f, 0f, 0f, 0.35f),
                    borderRightColor = new Color(0f, 0f, 0f, 0.35f),
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4
                }
            };
            area.Add(new Label(hintText) { style = { opacity = 0.7f } });

            area.RegisterCallback<DragUpdatedEvent>(_ =>
            {
                if (HasDraggedMonoScripts())
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                }
            });
            area.RegisterCallback<DragPerformEvent>(_ =>
            {
                DragAndDrop.AcceptDrag();
                dragPerformed();
            });
            return area;
        }

        static void StylePrimaryButton(Button button)
        {
            button.style.height = 32;
            button.style.fontSize = 13;
            button.style.marginTop = 4;
        }

        #endregion

        #region State Sync

        void SyncUIFromState()
        {
            if (_typeSourceField != null)
            {
                _typeSourceField.value = _typeSourceMode;
            }

            var isSingle = _typeSourceMode == TypeSourceMode.SingleType;
            if (_singleTypeContainer != null)
            {
                _singleTypeContainer.style.display = isSingle ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_multiTypeContainer != null)
            {
                _multiTypeContainer.style.display = isSingle ? DisplayStyle.None : DisplayStyle.Flex;
            }

            if (_monoScriptField != null)
            {
                _monoScriptField.value = _selectedMonoScript;
            }

            if (_typesCacheField != null)
            {
                _typesCacheField.value = _typesCacheSO;
            }

            _temporaryTypesList?.Rebuild();

            if (_docFolderField != null)
            {
                _docFolderField.SetValueWithoutNotify(_docFolderPath);
            }

            if (_generateButton != null)
            {
                _generateButton.SetEnabled(_hasFinishedAnalyze);
            }

            if (_analyzeResultLabel != null)
            {
                _analyzeResultLabel.text = GetAnalyzeResultText();
            }
        }

        string GetAnalyzeResultText()
        {
            if (!_hasFinishedAnalyze)
            {
                return "尚未分析";
            }

            if (_typeSourceMode == TypeSourceMode.SingleType)
            {
                if (_typeData != null && _typeData.TryAsIMemberData(out var memberData))
                {
                    return $"已分析 1 个类型：{memberData.Name}";
                }

                return "已分析 1 个类型";
            }

            var count = _typeDataList?.Count ?? 0;
            return $"已分析 {count} 个类型";
        }

        #endregion

        #region Actions

        void PerformAnalyze()
        {
            switch (_typeSourceMode)
            {
                case TypeSourceMode.SingleType:
                    var targetType = _selectedMonoScript ? _selectedMonoScript.GetClass() : null;
                    if (targetType == null)
                    {
                        ShowStatus("请选择有效的目标类型");
                        return;
                    }

                    _typeData = ScriptDocGeneratorUtility.AnalyzeSingleType(targetType);
                    break;
                case TypeSourceMode.MultipleTypes:
                    if (_typesCacheSO)
                    {
                        _typeDataList = ScriptDocGeneratorUtility.AnalyzeMultipleTypes(_typesCacheSO);
                    }
                    else if (_temporaryTypes.Count > 0)
                    {
                        _typeDataList = ScriptDocGeneratorUtility.AnalyzeMultipleTypes(_temporaryTypes);
                    }
                    else
                    {
                        ShowStatus("设置有效的 Type 对象列表或者设置 TypeCacheSO 资源");
                        return;
                    }

                    break;
                default:
                    return;
            }

            _hasFinishedAnalyze = true;
            SyncUIFromState();
            ShowNotification(new GUIContent("分析完成，生成按钮已启用"));
        }

        void PerformGenerateDoc()
        {
            if (!_hasFinishedAnalyze)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_docFolderPath))
            {
                ShowStatus("请先设置文档输出路径");
                return;
            }

            if (!Directory.Exists(_docFolderPath))
            {
                if (!EditorUtility.DisplayDialog("自动路径补全提示",
                        "当前的文档导出路径不存在，是否自动生成文件夹路径？", "确认", "取消"))
                {
                    return;
                }

                ScriptDocGeneratorEditorUtility.EnsureDirectoryExists(_docFolderPath);
            }

            var settings = DefaultScriptingAPISettingsSO.Instance;
            switch (_typeSourceMode)
            {
                case TypeSourceMode.SingleType:
                    ScriptDocGeneratorUtility.GenerateSingleTypeDoc(_typeData, settings, _docFolderPath);
                    break;
                case TypeSourceMode.MultipleTypes:
                    ScriptDocGeneratorUtility.GenerateMultipleTypeDocs(_typeDataList, settings, _docFolderPath);
                    break;
                default:
                    return;
            }

            _hasFinishedAnalyze = false;
            SyncUIFromState();
            ShowStatus("文档已生成：" + _docFolderPath);
        }

        void BrowseDocFolder()
        {
            var absolutePath = EditorUtility.OpenFolderPanel("选择文档输出文件夹", _docFolderPath, string.Empty);
            if (string.IsNullOrEmpty(absolutePath))
            {
                return;
            }

            // 始终以绝对路径保存，保证可指向 Assets 外目录（如文档站仓库）
            _docFolderPath = absolutePath.Replace('\\', '/');
            SyncUIFromState();
        }

        void ShowStatus(string message)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = message;
            }

            ShowNotification(new GUIContent(message));
        }

        #endregion

        #region Drag & Drop

        static bool HasDraggedMonoScripts() =>
            DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.OfType<MonoScript>().Any();

        void OnSingleTypeDragPerformed()
        {
            var monoScript = DragAndDrop.objectReferences.OfType<MonoScript>().FirstOrDefault();
            if (monoScript == null)
            {
                return;
            }

            _selectedMonoScript = monoScript;
            _hasFinishedAnalyze = false;
            SyncUIFromState();
        }

        void OnMultiTypesDragPerformed()
        {
            var monoScripts = DragAndDrop.objectReferences.OfType<MonoScript>();
            var hasNewType = false;
            foreach (var monoScript in monoScripts)
            {
                var type = monoScript.GetClass();
                if (type == null || _temporaryTypes.Contains(type))
                {
                    continue;
                }

                _temporaryTypes.Add(type);
                hasNewType = true;
            }

            if (hasNewType)
            {
                _hasFinishedAnalyze = false;
                SyncUIFromState();
            }
        }

        void RemoveTemporaryType(int index)
        {
            if (index < 0 || index >= _temporaryTypes.Count)
            {
                return;
            }

            _temporaryTypes.RemoveAt(index);
            _hasFinishedAnalyze = false;
            _temporaryTypesList.Rebuild();
        }

        #endregion
    }
}
