using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Runestone.AesirModules
{
    /// <summary>
    /// 可序列化的场景引用，支持在编辑器中拖拽 SceneAsset 赋值，等价于
    /// Eflatun.SceneReference（SceneReference 类型）+ Odin Inspector 的组合体。
    /// <para>
    /// 设计要点：
    /// <list type="number">
    /// <item>
    /// 编辑器侧以 <see cref="SceneAsset" /> 对象引用为数据源，每次访问自动同步路径、GUID、
    /// Addressables 地址三个缓存字段；场景移动/重命名由 Unity 的对象引用机制自动重定向，
    /// 对象引用意外丢失时用序列化的 GUID 自愈路径。
    /// </item>
    /// <item>运行时侧不依赖任何编辑器 API 与 Addressables 程序集，直接使用序列化的纯字符串数据。</item>
    /// <item>
    /// 遵循最小惊讶原则：Addressables 相关 API 在未安装 Addressables 包时依旧可见、可编译，
    /// 卸载包不会导致任何编译错误；运行期访问 <see cref="Address" /> 会抛出
    /// <see cref="AddressablesSupportDisabledException" />。
    /// </item>
    /// <item>
    /// 校验语义对位 Eflatun.SceneReference：先查 <see cref="State" /> /
    /// <see cref="UnsafeReason" />，或直接用 <see cref="TryGetScenePath" /> 等 TryGet 家族；
    /// 未分配场景的访问器按 fail-fast 约定抛 <see cref="EmptySceneAssetWrapperException" />。
    /// </item>
    /// </list>
    /// </para>
    /// </summary>
    [Serializable]
    public class SceneAssetWrapper : IEquatable<SceneAssetWrapper>
    {
        /// <summary>
        /// 场景相对路径缓存值（含后缀名）。编辑器下拖拽赋值或访问时自动同步，运行时直接使用此缓存值。
        /// </summary>
        [SerializeField]
        [HideInInspector]
        string scenePath = string.Empty;

        /// <summary>
        /// 场景资产 GUID 缓存值（编辑期锚点）。GUID 在场景移动/重命名时保持不变，
        /// 用于 SceneAsset 对象引用丢失时自愈路径；运行时不参与任何查找。
        /// </summary>
        [SerializeField]
        [HideInInspector]
        string sceneGuid = string.Empty;

        /// <summary>
        /// Addressables 地址缓存值。编辑器赋值/访问时经地址桥实时核验并回写；
        /// 运行时直接使用此缓存值加载（<c>Addressables.LoadSceneAsync(wrapper.Address)</c>）。
        /// </summary>
        [SerializeField]
        [HideInInspector]
        string sceneAddress = string.Empty;

        /// <summary>创建一个空引用（未分配任何场景）的包装器。永不抛异常。</summary>
        public SceneAssetWrapper()
        {
        }

        #region 校验属性

        /// <summary>
        /// 引用的可用状态。安全（Regular/Addressable）只保证"有一条可行的加载途径"，
        /// 并不要求场景当前已加载——是否已加载请查 <see cref="LoadedScene" />。
        /// </summary>
        public SceneAssetWrapperState State
        {
            get
            {
#if UNITY_EDITOR
                EditorSyncFromAsset();
#endif
                if (string.IsNullOrEmpty(scenePath) && string.IsNullOrEmpty(sceneAddress))
                {
                    return SceneAssetWrapperState.Unsafe;
                }

                // BuildSettings 途径优先于 Addressables（不依赖 Addressables 包，兼容性最好）
                if (!string.IsNullOrEmpty(scenePath) &&
                    SceneUtility.GetBuildIndexByScenePath(scenePath) != -1)
                {
                    return SceneAssetWrapperState.Regular;
                }

                if (!string.IsNullOrEmpty(sceneAddress))
                {
                    return SceneAssetWrapperState.Addressable;
                }

                return SceneAssetWrapperState.Unsafe;
            }
        }

        /// <summary>
        /// 引用不安全的具体原因。Empty 优先级最高；Addressable 场景即使不在 BuildSettings 也视为安全。
        /// </summary>
        public SceneAssetWrapperUnsafeReason UnsafeReason
        {
            get
            {
#if UNITY_EDITOR
                EditorSyncFromAsset();
#endif
                if (string.IsNullOrEmpty(scenePath) && string.IsNullOrEmpty(sceneAddress))
                {
                    return SceneAssetWrapperUnsafeReason.Empty;
                }

                if (!string.IsNullOrEmpty(sceneAddress))
                {
                    return SceneAssetWrapperUnsafeReason.None;
                }

                if (!string.IsNullOrEmpty(scenePath) &&
                    SceneUtility.GetBuildIndexByScenePath(scenePath) != -1)
                {
                    return SceneAssetWrapperUnsafeReason.None;
                }

                return SceneAssetWrapperUnsafeReason.NotInBuild;
            }
        }

        /// <summary>
        /// 项目是否安装了 Addressables 包。由 asmdef 的 versionDefines 宏
        /// <c>AESIR_MODULES_ADDRESSABLES</c> 驱动，包缺失时相关代码不会编译，但 API 始终可见。
        /// </summary>
        public static bool AddressablesSupportEnabled
        {
            get
            {
#if AESIR_MODULES_ADDRESSABLES
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// 此场景是否为 Addressable 场景。
        /// 编辑器下（桥已注册）实时核验并回写缓存；运行时按序列化数据判定。
        /// </summary>
        public bool IsAddressable
        {
            get
            {
#if UNITY_EDITOR
                if (SceneAssetWrapperAddressablesBridge.IsAvailable)
                {
                    EditorSyncFromAsset();
                    return !string.IsNullOrEmpty(sceneAddress);
                }
#endif
                return !string.IsNullOrEmpty(sceneAddress);
            }
        }

        #endregion

        #region 数据访问属性

        /// <summary>
        /// 场景资产 GUID。
        /// </summary>
        /// <exception cref="EmptySceneAssetWrapperException">引用未分配任何场景。</exception>
        public string Guid
        {
            get
            {
#if UNITY_EDITOR
                EditorSyncFromAsset();
#endif
                if (string.IsNullOrEmpty(scenePath) && string.IsNullOrEmpty(sceneAddress))
                {
                    throw new EmptySceneAssetWrapperException();
                }

                return sceneGuid;
            }
        }

        /// <summary>
        /// 场景相对路径，包含后缀名。
        /// </summary>
        /// <exception cref="EmptySceneAssetWrapperException">引用未分配任何场景。</exception>
        public string ScenePath
        {
            get
            {
#if UNITY_EDITOR
                EditorSyncFromAsset();
#endif
                if (string.IsNullOrEmpty(scenePath) && string.IsNullOrEmpty(sceneAddress))
                {
                    throw new EmptySceneAssetWrapperException();
                }

                return scenePath;
            }
        }

        /// <summary>
        /// 场景在 BuildSettings 中的序号。未加入（或被禁用）时返回 -1，不抛异常。
        /// </summary>
        /// <exception cref="EmptySceneAssetWrapperException">引用未分配任何场景。</exception>
        public int BuildIndex
        {
            get
            {
                var path = ScenePath;
                return SceneUtility.GetBuildIndexByScenePath(path);
            }
        }

        /// <summary>
        /// 场景名称（不包含扩展名）。
        /// </summary>
        /// <exception cref="EmptySceneAssetWrapperException">引用未分配任何场景。</exception>
        public string SceneName
        {
            get
            {
                var path = ScenePath;
                return Path.GetFileNameWithoutExtension(path);
            }
        }

        /// <summary>
        /// 场景的 <see cref="Scene" /> 结构。场景未加载时返回的结构无效，
        /// 用 <see cref="Scene.IsValid" /> 判断（与 Eflatun.SceneReference 同语义）。
        /// </summary>
        /// <exception cref="EmptySceneAssetWrapperException">引用未分配任何场景。</exception>
        public Scene LoadedScene
        {
            get
            {
                var path = ScenePath;
                var scene = SceneManager.GetSceneByPath(path);
                if (!scene.IsValid())
                {
                    // Addressable 场景加载后可能仅以名称注册，做一次名称回退
                    scene = SceneManager.GetSceneByName(Path.GetFileNameWithoutExtension(path));
                }

                return scene;
            }
        }

        /// <summary>
        /// 场景在 Addressables 中的地址，供 <c>Addressables.LoadSceneAsync</c> 使用。
        /// </summary>
        /// <exception cref="AddressablesSupportDisabledException">项目未安装 Addressables 包。</exception>
        /// <exception cref="EmptySceneAssetWrapperException">引用未分配任何场景。</exception>
        /// <exception cref="SceneNotAddressableException">此场景不是 Addressable 场景。</exception>
        public string Address
        {
            get
            {
                if (!AddressablesSupportEnabled)
                {
                    throw new AddressablesSupportDisabledException();
                }

                if (string.IsNullOrEmpty(scenePath) && string.IsNullOrEmpty(sceneAddress))
                {
                    throw new EmptySceneAssetWrapperException();
                }

                if (string.IsNullOrEmpty(sceneAddress))
                {
                    throw new SceneNotAddressableException();
                }

                return sceneAddress;
            }
        }

        /// <summary>
        /// 场景不在 BuildSettings 中时返回 true（含"已加入但被禁用"）；空引用返回 false。
        /// </summary>
        public bool NotInBuildSettings
        {
            get
            {
#if UNITY_EDITOR
                EditorSyncFromAsset();
#endif
                if (string.IsNullOrEmpty(scenePath))
                {
                    return false;
                }

                return SceneUtility.GetBuildIndexByScenePath(scenePath) == -1;
            }
        }

        /// <summary>输出场景名称；空引用输出空字符串（不抛异常）。</summary>
        public override string ToString() => TryGetSceneName(out var sceneName) ? sceneName : string.Empty;

        #endregion

        #region TryGet 家族

        /// <summary>
        /// 尝试获取场景路径。空引用返回 false，不抛异常。
        /// </summary>
        public bool TryGetScenePath(out string path)
        {
#if UNITY_EDITOR
            EditorSyncFromAsset();
#endif
            path = scenePath;
            return !string.IsNullOrEmpty(path);
        }

        /// <summary>
        /// 尝试获取 BuildSettings 序号。返回 true 时序号也可能为 -1（场景未加入 BuildSettings）。
        /// </summary>
        public bool TryGetBuildIndex(out int buildIndex)
        {
            if (!TryGetScenePath(out var path))
            {
                buildIndex = -1;
                return false;
            }

            buildIndex = SceneUtility.GetBuildIndexByScenePath(path);
            return true;
        }

        /// <summary>
        /// 尝试获取场景名称（不含扩展名）。
        /// </summary>
        public bool TryGetSceneName(out string sceneName)
        {
            if (!TryGetScenePath(out var path))
            {
                sceneName = null;
                return false;
            }

            sceneName = Path.GetFileNameWithoutExtension(path);
            return true;
        }

        /// <summary>
        /// 尝试获取已加载场景的 <see cref="Scene" /> 结构。与 <see cref="LoadedScene" /> 不同，
        /// 仅当场景确实已加载且有效时返回 true。
        /// </summary>
        public bool TryGetLoadedScene(out Scene loadedScene)
        {
            if (!TryGetScenePath(out var path))
            {
                loadedScene = default;
                return false;
            }

            var scene = SceneManager.GetSceneByPath(path);
            if (!scene.IsValid())
            {
                // Addressable 场景加载后可能仅以名称注册，做一次名称回退
                scene = SceneManager.GetSceneByName(Path.GetFileNameWithoutExtension(path));
            }

            loadedScene = scene;
            return scene.IsValid();
        }

        /// <summary>
        /// 尝试获取 Addressables 地址。与 <see cref="Address" /> 一致，
        /// 项目未安装 Addressables 包时抛 <see cref="AddressablesSupportDisabledException" />。
        /// </summary>
        /// <exception cref="AddressablesSupportDisabledException">项目未安装 Addressables 包。</exception>
        public bool TryGetAddress(out string address)
        {
            if (!AddressablesSupportEnabled)
            {
                throw new AddressablesSupportDisabledException();
            }

            if (string.IsNullOrEmpty(sceneAddress))
            {
                address = null;
                return false;
            }

            address = sceneAddress;
            return true;
        }

        #endregion

        #region 构造工厂

        /// <summary>
        /// 按场景路径构造。编辑器下校验路径上确实存在场景资产并解析 GUID/Addressables 地址；
        /// 运行时（构建后）不做存在性校验，加载是否可行由 <see cref="State" /> 表达。
        /// </summary>
        /// <exception cref="SceneAssetWrapperCreationException">路径为空，或（仅编辑器）路径上不存在场景资产。</exception>
        public static SceneAssetWrapper FromScenePath(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                throw new SceneAssetWrapperCreationException(
                    $"场景路径为空：'{scenePath}'。请提供有效场景的资产路径。");
            }

#if UNITY_EDITOR
            var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (asset == null)
            {
                throw new SceneAssetWrapperCreationException(
                    $"路径上不存在场景资产：'{scenePath}'。请确认路径指向一个 .unity 场景文件。");
            }

            var wrapper = new SceneAssetWrapper
            {
                SceneAsset = asset
            };
            return wrapper;
#else
            var runtimeWrapper = new SceneAssetWrapper
            {
                scenePath = scenePath
            };
            return runtimeWrapper;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// 按 <see cref="SceneAsset" /> 构造（仅编辑器）。赋值时自动同步路径/GUID/Addressables 地址。
        /// </summary>
        /// <exception cref="SceneAssetWrapperCreationException">场景资产为空。</exception>
        public static SceneAssetWrapper FromAsset(SceneAsset sceneAsset)
        {
            if (sceneAsset == null)
            {
                throw new SceneAssetWrapperCreationException("场景资产为空。请提供有效的 SceneAsset。");
            }

            var wrapper = new SceneAssetWrapper
            {
                SceneAsset = sceneAsset
            };
            return wrapper;
        }
#endif

        /// <summary>
        /// 判断此引用与另一引用是否指向同一场景：优先比较 GUID，其次比较路径（均忽略大小写）。
        /// </summary>
        public bool Equals(SceneAssetWrapper other) =>
            other != null &&
            string.Equals(IdentityKey, other.IdentityKey, StringComparison.OrdinalIgnoreCase);

        /// <inheritdoc cref="Equals(SceneAssetWrapper)" />
        public override bool Equals(object obj) => Equals(obj as SceneAssetWrapper);

        /// <inheritdoc cref="Equals(SceneAssetWrapper)" />
        public override int GetHashCode() =>
            StringComparer.OrdinalIgnoreCase.GetHashCode(IdentityKey);

        /// <summary>空引用之间相等。</summary>
        public static bool operator ==(SceneAssetWrapper left, SceneAssetWrapper right) =>
            left is null ? right is null : left.Equals(right);

        /// <summary>空引用之间相等。</summary>
        public static bool operator !=(SceneAssetWrapper left, SceneAssetWrapper right) =>
            !(left == right);

        /// <summary>身份键：优先 GUID，其次路径；空引用时为空字符串。</summary>
        string IdentityKey => string.IsNullOrEmpty(sceneGuid) ? scenePath : sceneGuid;

        #endregion

#if UNITY_EDITOR
        #region 编辑器：数据源与同步

        internal const string SceneAssetPropertyName = nameof(SceneAsset);

        internal const string AddCurrentSceneToBuildSettingsMethodName =
            nameof(AddCurrentSceneToBuildSettings);

        internal const string EnableCurrentSceneInBuildSettingsMethodName =
            nameof(EnableCurrentSceneInBuildSettings);

        internal const string AddSceneToAddressablesMethodName = nameof(AddSceneToAddressables);

        internal const string ResetSceneMethodName = nameof(ResetScene);
        internal const string GetSceneAssetColorMethodName = nameof(GetSceneAssetColor);

        [SerializeField]
        [HideInInspector]
        SceneAsset sceneAsset;

        /// <summary>
        /// 编辑器中拖拽的 SceneAsset（数据源）。赋值时自动同步路径/GUID/Addressables 地址。
        /// </summary>
        public SceneAsset SceneAsset
        {
            get => sceneAsset;
            set
            {
                sceneAsset = value;
                if (sceneAsset == null)
                {
                    scenePath = string.Empty;
                    sceneGuid = string.Empty;
                    sceneAddress = string.Empty;
                }
                else
                {
                    EditorSyncFromAsset();
                    // 桥不可用（未装 Addressables）时清空旧地址——换了场景，旧地址必然失效
                    if (!SceneAssetWrapperAddressablesBridge.IsAvailable)
                    {
                        sceneAddress = string.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// 场景在 BuildSettings 中存在条目但被禁用（编辑器实时检测）。
        /// </summary>
        public bool DisabledInBuildSettings
        {
            get
            {
                EditorSyncFromAsset();
                if (string.IsNullOrEmpty(scenePath))
                {
                    return false;
                }

                for (var i = 0; i < EditorBuildSettings.scenes.Length; i++)
                {
                    if (EditorBuildSettings.scenes[i].path == scenePath &&
                        !EditorBuildSettings.scenes[i].enabled)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// 场景完全不在 BuildSettings 中（既没有启用条目，也没有禁用条目）。
        /// </summary>
        public bool MissingFromBuild
        {
            get
            {
                EditorSyncFromAsset();
                if (string.IsNullOrEmpty(scenePath))
                {
                    return false;
                }

                for (var i = 0; i < EditorBuildSettings.scenes.Length; i++)
                {
                    if (EditorBuildSettings.scenes[i].path == scenePath)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>是否显示"添加到 BuildSettings"修复按钮。</summary>
        public bool CanAddToBuild => MissingFromBuild && !IsAddressable;

        /// <summary>是否显示"在 BuildSettings 中启用"修复按钮。</summary>
        public bool CanEnableInBuild => DisabledInBuildSettings && !IsAddressable;

        /// <summary>是否显示"加入 Addressables"修复按钮（需要安装 Addressables 包且场景当前不可寻址）。</summary>
        public bool CanMakeAddressable =>
            sceneAsset != null &&
            SceneAssetWrapperAddressablesBridge.IsAvailable &&
            SceneAssetWrapperAddressablesBridge.GetAddressHandler(scenePath) == null;

        /// <summary>
        /// 编辑器数据同步：SceneAsset → 路径/GUID/Addressables 地址。
        /// <para>
        /// 场景移动/重命名由 Unity 的对象引用机制自动重定向，此处负责回写缓存；
        /// 对象引用意外丢失时（场景移动后引用断链、数据经脚本或文本操作产生）用 GUID 自愈路径；
        /// Addressables 状态经桥实时核验，在 Addressables 窗口的增删无需重新赋值即可被发现。
        /// </para>
        /// </summary>
        void EditorSyncFromAsset()
        {
            if (sceneAsset != null)
            {
                var assetPath = AssetDatabase.GetAssetPath(sceneAsset);
                scenePath = assetPath;
                sceneGuid = AssetDatabase.AssetPathToGUID(assetPath);
                if (SceneAssetWrapperAddressablesBridge.IsAvailable)
                {
                    sceneAddress =
                        SceneAssetWrapperAddressablesBridge.GetAddressHandler(assetPath) ?? string.Empty;
                }
            }
            else if (!string.IsNullOrEmpty(sceneGuid))
            {
                var recovered = AssetDatabase.GUIDToAssetPath(sceneGuid);
                if (!string.IsNullOrEmpty(recovered) &&
                    recovered.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                {
                    scenePath = recovered;
                }
            }
        }

        #endregion

        #region 编辑器：一键修复工具箱（Odin AttributeProcessor 经 nameof 契约绑定，勿改成员形式）

        /// <summary>
        /// 把当前场景加入 BuildSettings。已有条目时绝不重复添加，只确保启用。
        /// </summary>
        void AddCurrentSceneToBuildSettings()
        {
            EditorSyncFromAsset();
            if (string.IsNullOrEmpty(scenePath))
            {
                AesirModulesDebug.LogWarning(AesirModulesDebug.SceneModuleTag, "无法把空场景引用添加到 BuildSettings。");
                return;
            }

            var scenes = EditorBuildSettings.scenes;
            EditorBuildSettingsScene existing = null;
            for (var i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].path == scenePath)
                {
                    existing = scenes[i];
                    break;
                }
            }

            if (existing != null)
            {
                if (!existing.enabled)
                {
                    existing.enabled = true;
                    EditorBuildSettings.scenes = scenes;
                    AesirModulesDebug.Log(AesirModulesDebug.SceneModuleTag,
                        $"已在 BuildSettings 中启用场景：{scenePath}");
                }

                return;
            }

            var sceneList = scenes.ToList();
            sceneList.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = sceneList.ToArray();
            AesirModulesDebug.Log(AesirModulesDebug.SceneModuleTag, $"已把场景添加到 BuildSettings：{scenePath}");
        }

        /// <summary>
        /// 启用 BuildSettings 中被禁用的当前场景条目。
        /// </summary>
        void EnableCurrentSceneInBuildSettings()
        {
            EditorSyncFromAsset();
            if (string.IsNullOrEmpty(scenePath))
            {
                AesirModulesDebug.LogWarning(AesirModulesDebug.SceneModuleTag, "无法启用空场景引用。");
                return;
            }

            var scenes = EditorBuildSettings.scenes;
            EditorBuildSettingsScene target = null;
            for (var i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].path == scenePath)
                {
                    target = scenes[i];
                    break;
                }
            }

            if (target == null)
            {
                AesirModulesDebug.LogWarning(AesirModulesDebug.SceneModuleTag,
                    $"场景不在 BuildSettings 中，无法启用：{scenePath}");
                return;
            }

            if (target.enabled)
            {
                return;
            }

            target.enabled = true;
            EditorBuildSettings.scenes = scenes;
            AesirModulesDebug.Log(AesirModulesDebug.SceneModuleTag, $"已在 BuildSettings 中启用场景：{scenePath}");
        }

        /// <summary>
        /// 把当前场景加入 Addressables 默认组（需要安装 Addressables 包）。
        /// </summary>
        void AddSceneToAddressables()
        {
            if (!SceneAssetWrapperAddressablesBridge.IsAvailable)
            {
                return;
            }

            EditorSyncFromAsset();
            if (string.IsNullOrEmpty(scenePath))
            {
                AesirModulesDebug.LogWarning(AesirModulesDebug.SceneModuleTag, "无法把空场景引用加入 Addressables。");
                return;
            }

            var address = SceneAssetWrapperAddressablesBridge.MakeAddressableHandler?.Invoke(scenePath);
            if (address == null)
            {
                AesirModulesDebug.LogWarning(AesirModulesDebug.SceneModuleTag,
                    "加入 Addressables 失败：Addressables 尚未初始化。请先打开 Window/Asset Management/Addressables/Groups 创建设置。");
                return;
            }

            sceneAddress = address;
            AesirModulesDebug.Log(AesirModulesDebug.SceneModuleTag,
                $"已把场景加入 Addressables 默认组：{scenePath}（Address: {address}）");
        }

        void ResetScene()
        {
            SceneAsset = null;
        }

        /// <summary>
        /// 着色优先级：Addressable（青）＞ 引用悬空（红）＝ 未加入 Build（红）＞ 被禁用（黄）＞ 正常（白）。
        /// </summary>
        Color GetSceneAssetColor()
        {
            if (IsAddressable)
            {
                return new Color(0.13f, 0.72f, 0.93f);
            }

            if (sceneAsset == null && !string.IsNullOrEmpty(scenePath))
            {
                // SceneAsset 引用丢失但路径仍在（场景被删除或 GUID 断链）
                return Color.red;
            }

            if (MissingFromBuild)
            {
                return Color.red;
            }

            if (DisabledInBuildSettings)
            {
                return Color.yellow;
            }

            return Color.white;
        }

        #endregion
#endif
    }
}
