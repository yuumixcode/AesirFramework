using System;
using System.Collections.Generic;
using System.IO;
using Runestone.AesirArchitecture;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 保存管理器（MonoBehaviour 单例）—— 独立游戏级事务式存档门面。
    /// 负责存档槽键值保存 / 加载、三轨序列化注册、存储 / 管理器 / 变换链替换、自动保存与生命周期钩子。
    /// <para>公开 API 全部为静态成员，经 <see cref="Instance" /> 单例转发。</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// 是否加入 DontDestroyOnLoad 场景由序列化字段 <see cref="dontDestroyOnLoad" /> 控制，
    /// 仅在本物体为根物体（场景预放置）时生效；运行时自动创建的实例挂载在 <see cref="AesirModules" /> 宿主下，
    /// 实际是否 DDOL 跟随宿主的 <c>dontDestroyOnLoad</c> 决策。
    /// </para>
    /// <para>
    /// 默认路径一行可用：<c>SaveModule.Save("slot0", data)</c> / <c>SaveModule.Load&lt;T&gt;("slot0")</c>。
    /// 保存经「序列化 → 变换链 → 信封封装 → 临时文件 + 刷盘 + 双 rename 备份轮换 + 原子提交」落盘，
    /// 任意崩溃点至少一份完整可读存档；加载校验失败自动回退备份。扩展点见
    /// <see cref="ISaveSerializer" /> / <see cref="ISaveStorage" /> / <see cref="ISaveManager" /> / <see cref="ISaveTransform" />。
    /// </para>
    /// <para>
    /// 设计边界：全同步契约（KB 级存档无感；大存档主线程捕获后自行后台写入）；不做 Unity 对象引用序列化、
    /// 云存档实现（留扩展点）、多代备份轮换、存档槽 UI。安全边界：CRC32 防意外损坏不防篡改，
    /// AES 防 casual 修改不防逆向（密钥在客户端可被提取）。
    /// </para>
    /// </remarks>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-999)]
    public class SaveModule : AesirMonoBehaviour
    {
        /// <summary>
        /// 是否将本物体加入 DontDestroyOnLoad 场景。仅在本物体为根物体（场景预放置）时生效。
        /// </summary>
        /// <remarks>
        /// 默认 true（跨场景持久）。设为 false 时实例保留在所在场景、随场景卸载销毁，
        /// 必须自行处理多场景叠加（Additive）加载下的生命周期管理。
        /// 运行时自动创建的实例挂载在 [Aesir Modules] 宿主下（非根物体），
        /// DDOL 跟随宿主决策，本字段不参与判断。
        /// </remarks>
        [SerializeField]
        bool dontDestroyOnLoad = true;

        internal const string DontDestroyOnLoadFieldName = nameof(dontDestroyOnLoad);

        /// <summary>
        /// 可选的模块配置资产（预放置实例拖入）。未指定时回退 <see cref="SaveConfigSO.Instance" />
        /// （Resources 兜底 → 内存默认值）。
        /// </summary>
        [Tooltip("可选的模块配置资产 — 存储目录、默认序列化器、备份与自动保存开关；留空时回退 SaveConfigSO.Instance")]
        [SerializeField]
        SaveConfigSO config;

        static SaveModule _instance;

        #region 运行时状态

        // 显式非序列化：Odin 环境下基类为 SerializedMonoBehaviour（默认序列化私有字段），
        // 运行状态落场景残留会导致域重载后 _initialized 与注册表不一致
        [NonSerialized] Dictionary<string, ISaveSerializer> _serializers;
        [NonSerialized] ISaveStorage _storage;
        [NonSerialized] ISaveManager _manager;
        [NonSerialized] List<ISaveTransform> _transforms;
        [NonSerialized] SaveConfigSO _activeConfig;
        [NonSerialized] bool _initialized;
        [NonSerialized] RepeatingTimer _autoSaveTimer;
        [NonSerialized] Action _autoSaveAction;
        [NonSerialized] bool _saveOnPause;
        [NonSerialized] bool _saveOnQuit;

        #endregion

        #region 单例与生命周期

        /// <summary>
        /// 全局单例入口。
        /// 优先在已加载场景中查找预放置的实例（含未激活对象）；未找到时在 <see cref="AesirModules" />（DDOL）下创建子物体。
        /// </summary>
        public static SaveModule Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                // 含未激活对象：Unity 不对未激活物体调用 Awake，Exclude 会把预放置实例判为不存在而重复创建
                _instance = FindAnyObjectByType<SaveModule>(FindObjectsInactive.Include);
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = AesirModules.GetOrAddChild<SaveModule>();
                return _instance;
            }
        }

        /// <summary>
        /// 非创建式单例获取：实例不存在时返回 <c>false</c>，<b>不触发懒创建</b>
        /// （与 <c>AudioModule.TryGetExisting</c> 同款）。
        /// </summary>
        /// <param name="module">已存在的实例（含未激活预放置）；不存在时为 null。</param>
        public static bool TryGetExisting(out SaveModule module)
        {
            if (_instance != null)
            {
                module = _instance;
                return true;
            }

            module = FindAnyObjectByType<SaveModule>(FindObjectsInactive.Include);
            if (module != null)
            {
                _instance = module;
            }

            return module != null;
        }

        /// <summary>兼容 Enter Play Mode（跳过域重载）的静态状态重置。</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _instance = null;
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            // 非根物体时加入 DDOL 会被 Unity 拒绝并告警，仅根物体生效
            if (dontDestroyOnLoad && transform.root == transform)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        void OnDestroy()
        {
            ClearAutoSaveInternal();
            if (_instance != null && _instance == this)
            {
                _instance = null;
            }
        }

        void OnApplicationPause(bool paused)
        {
            if (paused && _saveOnPause)
            {
                _autoSaveAction?.Invoke();
            }
        }

        void OnApplicationQuit()
        {
            if (_saveOnQuit)
            {
                _autoSaveAction?.Invoke();
            }
        }

        /// <summary>获取已完成初始化的单例，所有公开静态 API 的统一入口。</summary>
        static SaveModule Ready()
        {
            var m = Instance;
            m.EnsureInitialized();
            return m;
        }

        /// <summary>
        /// 懒初始化：载入配置、注册默认序列化器、创建默认存储 / 管理器 / 空变换链。
        /// 预放置实例在 Awake 后首次访问时执行；编辑器与测试环境未经 Awake 时由 <see cref="Ready" /> 兜底。
        /// </summary>
        void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            _activeConfig = config != null ? config : SaveConfigSO.Instance;
            _serializers = new Dictionary<string, ISaveSerializer>();
            _serializers[new UnityJsonSerializer().Id] = new UnityJsonSerializer();
            _transforms = new List<ISaveTransform>();
            _storage = CreateStorage(_activeConfig);
            _manager = new TransactionalSaveManager(_serializers, _storage, _transforms,
                _activeConfig.defaultSerializerId);
            _saveOnPause = _activeConfig.saveOnApplicationPause;
            _saveOnQuit = _activeConfig.saveOnApplicationQuit;
        }

        #endregion

        #region 公开 API — 保存与加载

        /// <summary>
        /// 保存数据到存档槽（事务式写入：任意崩溃点至少一份完整可读存档）。
        /// 使用默认序列化器（配置 <c>defaultSerializerId</c>）与当前变换链。
        /// </summary>
        /// <typeparam name="T">数据类型（纯数据对象）。</typeparam>
        /// <param name="key">存档键（仅字母、数字、下划线、连字符）。</param>
        /// <param name="data">待保存数据（非 null）。</param>
        /// <param name="dataVersion">数据版本号（写入存档，加载时用于迁移判定）。</param>
        /// <returns>保存结果；IO 等环境失败以结果表达，编程错误抛异常。</returns>
        public static SaveResult Save<T>(string key, T data, int dataVersion = 0) =>
            Ready()._manager.Save(key, data, dataVersion);

        /// <summary>
        /// 从存档槽加载数据。主存档缺失或损坏时自动回退备份（结果 <c>Source</c> 标明来源）；
        /// 版本不匹配时经迁移器升级，未提供迁移器则返回 <c>VersionMismatch</c> 状态。
        /// </summary>
        /// <typeparam name="T">数据类型，须与保存时一致。</typeparam>
        /// <param name="key">存档键。</param>
        /// <param name="currentVersion">当前数据版本号（与存档记录比对）。</param>
        /// <param name="migrator">版本不匹配时的迁移器（可选）。</param>
        /// <returns>加载结果（状态 / 值 / 来源 / 错误描述）。</returns>
        public static LoadResult<T> Load<T>(string key, int currentVersion = 0,
            ISaveMigrator<T> migrator = null) =>
            Ready()._manager.Load(key, currentVersion, migrator);

        /// <summary>删除存档（主存档与备份）；键不存在时无副作用。</summary>
        /// <param name="key">存档键。</param>
        public static void Delete(string key)
        {
            SaveKey.Validate(key);
            Ready()._storage.Delete(key);
        }

        /// <summary>存档是否存在。</summary>
        /// <param name="key">存档键。</param>
        public static bool Exists(string key)
        {
            SaveKey.Validate(key);
            return Ready()._storage.Exists(key);
        }

        /// <summary>枚举全部已提交存档的键（不含临时文件与备份）。</summary>
        public static IReadOnlyList<string> GetKeys() => Ready()._storage.GetKeys();

        #endregion

        #region 公开 API — 扩展点

        /// <summary>
        /// 注册序列化器（同 Id 覆盖）。注册后即可经配置 / ApplyConfig 将其设为默认，
        /// 旧存档仍按各自记录的序列化器 Id 还原。
        /// </summary>
        /// <param name="serializer">序列化器实现（Id 非空）。</param>
        public static void RegisterSerializer(ISaveSerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            if (string.IsNullOrEmpty(serializer.Id))
            {
                throw new ArgumentException("序列化器 Id 不可为空", nameof(serializer));
            }

            Ready()._serializers[serializer.Id] = serializer;
        }

        /// <summary>
        /// 替换存储实现。当前管理器为默认 <see cref="TransactionalSaveManager" /> 时随之重建以接入新存储；
        /// 自定义管理器自行编排存储，不受本方法影响。
        /// </summary>
        /// <param name="storage">存储实现。</param>
        public static void SetStorage(ISaveStorage storage)
        {
            if (storage == null)
            {
                throw new ArgumentNullException(nameof(storage));
            }

            var m = Ready();
            m._storage = storage;
            m.RebuildDefaultManager();
        }

        /// <summary>
        /// 替换保存管理器（自定义保存管理方案的总入口）。替换后保存 / 加载整体走新实现，
        /// 默认事务式管理器不再参与；删除 / 存在性 / 枚举仍直通存储。
        /// </summary>
        /// <param name="manager">管理器实现。</param>
        public static void SetManager(ISaveManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            Ready()._manager = manager;
        }

        /// <summary>
        /// 设置字节变换链（按序施加：先压缩后加密；加载按存档记录逆序还原）。
        /// 传入空数组清除变换链。链以引用共享给默认管理器，变更立即生效；
        /// 旧存档按各自记录的变换 Id 还原，链上每个 Id 须保持注册。
        /// </summary>
        /// <param name="transforms">变换实现（各 Id 非空）。</param>
        public static void SetTransformChain(params ISaveTransform[] transforms)
        {
            var m = Ready();
            m._transforms.Clear();
            if (transforms == null)
            {
                return;
            }

            for (var i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] == null)
                {
                    throw new ArgumentException($"变换链第 {i} 项为空", nameof(transforms));
                }

                if (string.IsNullOrEmpty(transforms[i].Id))
                {
                    throw new ArgumentException($"变换链第 {i} 项 Id 为空", nameof(transforms));
                }

                m._transforms.Add(transforms[i]);
            }
        }

        #endregion

        #region 公开 API — 自动保存

        /// <summary>
        /// 注册自动保存：按间隔周期触发保存回调（计时不受 <c>Time.timeScale</c> 影响，
        /// 复用 <see cref="AesirTimer.Every" />）；切后台与退出时按配置开关各触发一次。
        /// 重复调用替换既有注册。回调内异常向上传播（fail-fast，游戏代码 bug 不兜底）。
        /// </summary>
        /// <param name="saveAction">保存回调（非 null；典型写法：() =&gt; Save("slot0", Capture())）。</param>
        /// <param name="intervalSeconds">周期间隔（秒，须大于 0）。</param>
        public static void SetAutoSave(Action saveAction, float intervalSeconds)
        {
            if (saveAction == null)
            {
                throw new ArgumentNullException(nameof(saveAction));
            }

            if (intervalSeconds <= 0f)
            {
                throw new ArgumentException("自动保存间隔须大于 0", nameof(intervalSeconds));
            }

            var m = Ready();
            m.ClearAutoSaveInternal();
            m._autoSaveAction = saveAction;
            m._autoSaveTimer = AesirTimer.Every(intervalSeconds, saveAction, unscaled: true);
        }

        /// <summary>清除自动保存（周期计时与切后台 / 退出回调）；无注册时无副作用，不创建模块实例。</summary>
        public static void ClearAutoSave()
        {
            if (TryGetExisting(out var m))
            {
                m.ClearAutoSaveInternal();
            }
        }

        void ClearAutoSaveInternal()
        {
            _autoSaveTimer?.Cancel();
            _autoSaveTimer = null;
            _autoSaveAction = null;
        }

        #endregion

        #region 公开 API — 配置

        /// <summary>
        /// 替换运行时配置并重建存储与默认管理器。传入 null 回退 <see cref="SaveConfigSO.Instance" />。
        /// <para>
        /// 副作用声明：存储根目录与备份开关随配置变更（已保存的存档留在旧目录，不迁移）；
        /// 自定义管理器不受影响（自行编排存储）；自动保存注册保留。
        /// </para>
        /// </summary>
        /// <param name="config">新配置资产（可为 null）。</param>
        public static void ApplyConfig(SaveConfigSO config)
        {
            var m = Ready();
            m.config = config;
            m._activeConfig = config != null ? config : SaveConfigSO.Instance;
            m._storage = CreateStorage(m._activeConfig);
            m.RebuildDefaultManager();
            m._saveOnPause = m._activeConfig.saveOnApplicationPause;
            m._saveOnQuit = m._activeConfig.saveOnApplicationQuit;
        }

        static FileSaveStorage CreateStorage(SaveConfigSO config) =>
            new FileSaveStorage(Path.Combine(Application.persistentDataPath, config.saveDirectory),
                config.backupEnabled);

        /// <summary>当前管理器为默认事务式管理器时重建（接入新存储 / 新默认序列化器）；自定义管理器不动。</summary>
        void RebuildDefaultManager()
        {
            if (_manager is TransactionalSaveManager)
            {
                _manager = new TransactionalSaveManager(_serializers, _storage, _transforms,
                    _activeConfig.defaultSerializerId);
            }
        }

        #endregion
    }
}