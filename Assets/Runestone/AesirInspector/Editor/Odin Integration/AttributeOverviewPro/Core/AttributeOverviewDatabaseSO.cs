using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// AttributeOverviewPro 数据库 SO，以子资产形式管理所有 PanelSO 和 ExampleSO。
    /// PanelSO 作为数据库 .asset 的子资产；ExampleSO 按序列化方式分别存入 Unity/Odin 容器 .asset 文件。
    /// </summary>
    public class AttributeOverviewDatabaseSO : SerializedScriptableObject, IAesirInspectorReset
    {
        static AttributeOverviewDatabaseSO _instance;

        /// <summary>
        /// 子资产内存缓存，按类型索引。Domain Reload 后自动清空。
        /// </summary>
        static readonly Dictionary<Type, ScriptableObject> SubAssetCache =
            new Dictionary<Type, ScriptableObject>();

        /// <summary>
        /// 各容器的 LoadAllAssetsAtPath 缓存，按路径索引。
        /// </summary>
        static readonly Dictionary<string, Object[]> SubAssetCaches = new Dictionary<string, Object[]>();

        /// <summary>
        /// 标记初始化期间是否正在批量操作，此时跳过 SaveAssets。
        /// </summary>
        static bool _isBatching;

        public Dictionary<string, AbstractAttributePanelSO[]> AttributePanelArrayMap;

        public Dictionary<string, AbstractAttributePanelSO> AttributePanelMap =
            new Dictionary<string, AbstractAttributePanelSO>();

        /// <summary>
        /// 获取数据库单例实例。
        /// </summary>
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

        /// <summary>
        /// 重置数据库，重新初始化。
        /// </summary>
        public void AesirInspectorReset() => Initialize();

        /// <summary>
        /// 获取或创建类型为 T 的子资产。根据 T 的序列化方式自动路由到对应容器文件：
        /// Odin 序列化（继承 SerializedScriptableObject）→ OdinExamples.asset
        /// Unity 原生序列化 → UnityExamples.asset
        /// </summary>
        /// <typeparam name="T">ScriptableObject 子类型</typeparam>
        /// <returns>已存在或新建的子资产实例</returns>
        public static T GetOrCreateExampleSubAsset<T>() where T : ScriptableObject
        {
            var type = typeof(T);

            if (SubAssetCache.TryGetValue(type, out var cached) && cached != null)
            {
                return (T)cached;
            }

            if (!_instance)
            {
                _ = Instance;
            }

            var containerPath = GetExampleContainerPath<T>();
            EnsureContainerAsset(containerPath);

            var allSubAssets = LoadSubAssets(containerPath);

            foreach (var sub in allSubAssets)
            {
                if (sub is T found)
                {
                    SubAssetCache[type] = found;
                    return found;
                }
            }

            var instance = CreateInstance<T>();
            instance.name = type.Name;
            AssetDatabase.AddObjectToAsset(instance, containerPath);
            InvalidateSubAssetCache(containerPath);

            if (!_isBatching)
            {
                AssetDatabase.SaveAssets();
            }

            SubAssetCache[type] = instance;
            return instance;
        }

        /// <summary>
        /// 根据类型 T 的序列化方式返回对应的容器 .asset 文件路径。
        /// </summary>
        static string GetExampleContainerPath<T>() where T : ScriptableObject
        {
            // Odin 序列化的 ExampleSO（继承 SerializedScriptableObject）→ OdinExamples.asset
            if (typeof(SerializedScriptableObject).IsAssignableFrom(typeof(T)))
            {
                return AesirInspectorPaths.AttributeExamplesOdinPath;
            }

            // Unity 原生序列化的 ExampleSO → UnityExamples.asset
            return AesirInspectorPaths.AttributeExamplesUnityPath;
        }

        /// <summary>
        /// 确保容器 .asset 文件存在。若不存在则创建一个空的 ScriptableObject 作为主资产。
        /// </summary>
        static void EnsureContainerAsset(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) != null)
            {
                return;
            }

            PathSafeEditorUtility.EnsureDirectoryExists(Path.GetDirectoryName(path));
            var container = CreateInstance<ScriptableObject>();
            container.name = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(container, path);
            AssetDatabase.ImportAsset(path);
            InvalidateSubAssetCache(path);
        }

        /// <summary>
        /// 从指定路径加载所有子资产（含主资产），带缓存。
        /// </summary>
        static Object[] LoadSubAssets(string path)
        {
            if (SubAssetCaches.TryGetValue(path, out var cached) && cached != null)
            {
                return cached;
            }

            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            SubAssetCaches[path] = assets ?? Array.Empty<Object>();
            return SubAssetCaches[path];
        }

        /// <summary>
        /// 使指定路径的子资产缓存失效。不传路径则清除所有缓存。
        /// </summary>
        static void InvalidateSubAssetCache(string path = null)
        {
            if (path != null)
            {
                SubAssetCaches.Remove(path);
            }
            else
            {
                SubAssetCaches.Clear();
            }
        }

        /// <summary>
        /// 初始化数据库：迁移旧资产，发现所有面板类型，创建缺失的子资产，构建菜单映射。
        /// </summary>
        [Button("初始化数据库", ButtonSizes.Large)]
        public void Initialize()
        {
            var startTime = (float)EditorApplication.timeSinceStartup;
            const float timeout = 60f;

            try
            {
                _isBatching = true;

                MigrateOldAssets();
                MigrateExampleSubAssetsFromDatabase();

                var allPanels = GetAllAttributePanels();
                RemoveOrphanSubAssets(allPanels);

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
                if (allPanels != null)
                {
                    var totalPanels = allPanels.Length;
                    var processedCount = 0;

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
            }
            finally
            {
                _isBatching = false;
                EditorUtility.ClearProgressBar();
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                InvalidateSubAssetCache();
            }
        }

        /// <summary>
        /// 迁移旧版独立 .asset 文件：删除 Panels/ 和 Attribute Examples/ 目录下的独立资产。
        /// </summary>
        static void MigrateOldAssets()
        {
            var oldPanelPath = AesirInspectorPaths.AttributePanelsPath;
            var oldExamplePath = AesirInspectorPaths.AttributeExamplesPath;
            var deleted = false;

            if (Directory.Exists(oldPanelPath))
            {
                var oldPanelGuids = AssetDatabase.FindAssets("t:AbstractAttributePanelSO",
                    new[] { oldPanelPath });
                foreach (var guid in oldPanelGuids)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
                    deleted = true;
                }
            }

            if (Directory.Exists(oldExamplePath))
            {
                var oldExampleGuids = AssetDatabase.FindAssets("t:ScriptableObject",
                    new[] { oldExamplePath });
                foreach (var guid in oldExampleGuids)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
                    deleted = true;
                }
            }

            if (deleted)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 迁移已存储在数据库 .asset 中的 ExampleSO 子资产：移除旧子资产，使其在下次访问时由正确的容器重新创建。
        /// </summary>
        static void MigrateExampleSubAssetsFromDatabase()
        {
            var dbPath = AssetDatabase.GetAssetPath(_instance);
            var allSubAssets = AssetDatabase.LoadAllAssetsAtPath(dbPath);
            var removed = false;

            foreach (var subAsset in allSubAssets)
            {
                if (subAsset is AttributeOverviewDatabaseSO)
                {
                    continue;
                }

                // 移除所有非 PanelSO 的子资产（即 ExampleSO）
                if (subAsset is not AbstractAttributePanelSO)
                {
                    AssetDatabase.RemoveObjectFromAsset(subAsset);
                    removed = true;
                }
            }

            if (removed)
            {
                EditorUtility.SetDirty(_instance);
                AssetDatabase.SaveAssets();
                InvalidateSubAssetCache();
            }
        }

        /// <summary>
        /// 移除孤立的 PanelSO 子资产（代码中已不存在的类型）。
        /// </summary>
        static void RemoveOrphanSubAssets(AbstractAttributePanelSO[] validPanels)
        {
            var path = AssetDatabase.GetAssetPath(_instance);
            var allSubAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            var validPanelTypes = validPanels.Select(p => p.GetType()).ToHashSet();
            var removed = false;

            foreach (var subAsset in allSubAssets)
            {
                if (subAsset is AttributeOverviewDatabaseSO)
                {
                    continue;
                }

                if (subAsset is AbstractAttributePanelSO panel && !validPanelTypes.Contains(panel.GetType()))
                {
                    AssetDatabase.RemoveObjectFromAsset(subAsset);
                    removed = true;
                }
            }

            if (removed)
            {
                EditorUtility.SetDirty(_instance);
                InvalidateSubAssetCache();
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

            var path = AssetDatabase.GetAssetPath(_instance);
            var allSubAssets = LoadSubAssets(path);

            var existingPanels = allSubAssets.OfType<AbstractAttributePanelSO>().GroupBy(x => x.GetType())
                .ToDictionary(g => g.Key, g => g.First());

            var list = new List<AbstractAttributePanelSO>();

            for (var i = 0; i < panelTypes.Length; i++)
            {
                var type = panelTypes[i];
                EditorUtility.DisplayProgressBar("Initializing Aesir Inspector",
                    $"Discovering Panel: {type.Name} ({i + 1}/{panelTypes.Length})",
                    (float)i / panelTypes.Length * 0.5f);

                if (existingPanels.TryGetValue(type, out var asset))
                {
                    list.Add(asset);
                }
                else
                {
                    asset = (AbstractAttributePanelSO)CreateInstance(type);
                    asset.name = type.Name;
                    AssetDatabase.AddObjectToAsset(asset, path);
                    list.Add(asset);
                }
            }

            EditorUtility.SetDirty(_instance);
            InvalidateSubAssetCache();

            return list.ToArray();
        }
    }
}
