using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    public class AttributeOverviewDatabaseSO : SerializedScriptableObject, IAesirInspectorReset
    {
        static AttributeOverviewDatabaseSO _instance;
        public Dictionary<string, AbstractAttributePanelSO[]> AttributePanelArrayMap;
        public Dictionary<string, AbstractAttributePanelSO> AttributePanelMap =
            new Dictionary<string, AbstractAttributePanelSO>();

        /// <summary>
        /// 获取数据库单例实例。
        /// </summary>
        [Summary("获取数据库单例实例")]
        public static AttributeOverviewDatabaseSO Instance
        {
            get
            {
                if (_instance)
                {
                    return _instance;
                }

                _instance = ScriptableObjectSafeEditorUtility
                    .GetSingletonAssetAndDeleteOther<AttributeOverviewDatabaseSO>(AesirInspectorPaths
                        .AttributeOverviewDatabasePath);

                if (_instance != null && (_instance.AttributePanelMap == null ||
                                          _instance.AttributePanelMap.Count == 0))
                {
                    _instance.Initialize();
                }

                return _instance;
            }
        }

        public void AesirInspectorReset() => Initialize();

        /// <summary>
        /// 初始化数据库：发现所有面板类型，创建缺失的 SO 资源，构建菜单映射。
        /// </summary>
        [Button("初始化数据库", ButtonSizes.Large)]
        [Summary("初始化数据库：发现所有面板类型，创建缺失的 SO 资源，构建菜单映射")]
        public void Initialize()
        {
            var startTime = (float)EditorApplication.timeSinceStartup;
            const float timeout = 30f;

            try
            {
                var allPanels = GetAllAttributePanels();
                if (allPanels == null || allPanels.Length == 0)
                {
                    AttributePanelArrayMap = new Dictionary<string, AbstractAttributePanelSO[]>();
                    AttributePanelMap = new Dictionary<string, AbstractAttributePanelSO>();
                }

                AttributePanelArrayMap = new Dictionary<string, AbstractAttributePanelSO[]>
                {
                    {
                        nameof(AesirAttributeCategory.Essentials),
                        FilterPanels(allPanels, AesirAttributeCategory.Essentials)
                    },
                    {
                        nameof(AesirAttributeCategory.Buttons),
                        FilterPanels(allPanels, AesirAttributeCategory.Buttons)
                    },
                    {
                        nameof(AesirAttributeCategory.Collections),
                        FilterPanels(allPanels, AesirAttributeCategory.Collections)
                    },
                    {
                        nameof(AesirAttributeCategory.Groups),
                        FilterPanels(allPanels, AesirAttributeCategory.Groups)
                    },
                    {
                        nameof(AesirAttributeCategory.Conditionals),
                        FilterPanels(allPanels, AesirAttributeCategory.Conditionals)
                    },
                    {
                        nameof(AesirAttributeCategory.Numbers),
                        FilterPanels(allPanels, AesirAttributeCategory.Numbers)
                    },
                    {
                        nameof(AesirAttributeCategory.TypeSpecifics),
                        FilterPanels(allPanels, AesirAttributeCategory.TypeSpecifics)
                    },
                    {
                        nameof(AesirAttributeCategory.Validation),
                        FilterPanels(allPanels, AesirAttributeCategory.Validation)
                    },
                    {
                        nameof(AesirAttributeCategory.Misc),
                        FilterPanels(allPanels, AesirAttributeCategory.Misc)
                    },
                    {
                        nameof(AesirAttributeCategory.Meta),
                        FilterPanels(allPanels, AesirAttributeCategory.Meta)
                    },
                    {
                        nameof(AesirAttributeCategory.Unity),
                        FilterPanels(allPanels, AesirAttributeCategory.Unity)
                    },
                    {
                        nameof(AesirAttributeCategory.Debug),
                        FilterPanels(allPanels, AesirAttributeCategory.Debug)
                    }
                };

                AttributePanelMap = new Dictionary<string, AbstractAttributePanelSO>();
                int totalPanels = allPanels.Length;
                int processedCount = 0;

                foreach (var (category, panelArray) in AttributePanelArrayMap)
                {
                    foreach (var panel in panelArray)
                    {
                        if (panel == null)
                        {
                            continue;
                        }

                        if (EditorApplication.timeSinceStartup - startTime > timeout)
                        {
                            break;
                        }

                        processedCount++;
                        EditorUtility.DisplayProgressBar("Initializing Aesir Inspector",
                            $"Initializing Panel: {panel.name} ({processedCount}/{totalPanels})",
                            0.5f + (float)processedCount / totalPanels * 0.5f);

                        panel.Initialize();

                        if (panel.BilingualHeaderControl?.headerName == null)
                        {
                            continue;
                        }

                        var menuName = panel.BilingualHeaderControl.headerName.ChineseDisplay;
                        var key = category + "/" + menuName;
                        AttributePanelMap.TryAdd(key, panel);
                    }

                    if (EditorApplication.timeSinceStartup - startTime > timeout)
                    {
                        break;
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        static AbstractAttributePanelSO[] FilterPanels(AbstractAttributePanelSO[] panels,
            AesirAttributeCategory category)
        {
            return panels.Where(x =>
            {
                if (x == null)
                {
                    return false;
                }

                var attr = x.GetType().GetCustomAttribute<AttributeCategoryAttribute>();
                return attr != null && attr.Category.HasFlagFast(category);
            }).ToArray();
        }

        static AbstractAttributePanelSO[] GetAllAttributePanels()
        {
            var panelTypes = TypeCache.GetTypesDerivedFrom<AbstractAttributePanelSO>()
                .Where(t => !t.IsAbstract && !t.IsInterface).ToArray();

            var existingAssets = AssetDatabase.FindAssets("t:" + typeof(AbstractAttributePanelSO))
                .Select(guid =>
                    AssetDatabase.LoadAssetAtPath<AbstractAttributePanelSO>(
                        AssetDatabase.GUIDToAssetPath(guid))).Where(x => x != null).GroupBy(x => x.GetType())
                .ToDictionary(g => g.Key, g => g.First());

            var list = new List<AbstractAttributePanelSO>();
            var needsRefresh = false;

            for (int i = 0; i < panelTypes.Length; i++)
            {
                var type = panelTypes[i];
                EditorUtility.DisplayProgressBar("Initializing Aesir Inspector",
                    $"Discovering Panel: {type.Name} ({i + 1}/{panelTypes.Length})",
                    (float)i / panelTypes.Length * 0.5f);

                if (existingAssets.TryGetValue(type, out var asset))
                {
                    list.Add(asset);
                }
                else
                {
                    asset = (AbstractAttributePanelSO)CreateInstance(type);
                    var fileName = type.Name.EndsWith("SO")
                        ? type.Name.Remove(type.Name.Length - 2)
                        : type.Name;
                    var path = AesirInspectorPaths.AttributePanelsPath + "/" + fileName + ".asset";
                    if (!Directory.Exists(AesirInspectorPaths.AttributePanelsPath))
                    {
                        Directory.CreateDirectory(AesirInspectorPaths.AttributePanelsPath);
                    }

                    AssetDatabase.CreateAsset(asset, path);
                    needsRefresh = true;
                    list.Add(asset);
                }
            }

            if (needsRefresh)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return list.ToArray();
        }
    }
}
