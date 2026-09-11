using System.Collections;
using Runestone.AesirArchitecture;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 音频管理器（MonoBehaviour 单例）—— 2D 音频极简门面。
    /// 负责 SFX 轮询播放、BGM 循环与淡入淡出、三通道音量/静音控制与持久化。
    /// <para>公开 API 全部为静态成员，经 <see cref="Instance" /> 单例转发。</para>
    /// </summary>
    /// <remarks>
    /// 是否加入 DontDestroyOnLoad 场景由序列化字段 <see cref="dontDestroyOnLoad" /> 控制，
    /// 仅在本物体为根物体（场景预放置）时生效；运行时自动创建的实例挂载在 <see cref="AesirModules" /> 宿主下，
    /// 实际是否 DDOL 跟随宿主的 <c>dontDestroyOnLoad</c> 决策。
    /// <para>
    /// SFX 采用固定数量独占音源轮询（等效池化：无每播实例化开销，源全忙时按轮询序抢占最旧）；
    /// BGM 采用专用循环音源，切换支持协程淡入淡出（基于 <c>Time.unscaledDeltaTime</c>，不受 timeScale 影响）。
    /// </para>
    /// <para>
    /// 设计边界（极简取舍）：仅负责 2D 音频——3D 空间音效请使用原生
    /// <c>AudioSource.PlayClipAtPoint</c> 自建音源；不集成 AudioMixer（音量直接写入音源）；
    /// <see cref="PlaySfx" /> 为 fire-and-forget，不提供单个音效的停止与播完回调（回调机制请使用 MiniEvent）。
    /// </para>
    /// </remarks>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-999)]
    public class AudioModule : AesirMonoBehaviour
    {
        internal const string DontDestroyOnLoadFieldName = nameof(dontDestroyOnLoad);

        const string DefaultPrefsKey = "AesirAudio";

        static AudioModule _instance;

        #region 公开 API — SFX

        /// <summary>
        /// 播放一次音效（fire-and-forget）。每次播放轮询取下一个独占音源，
        /// 局部音量与音调独立于其他正在播放的音效。
        /// </summary>
        /// <param name="clip">音频片段。</param>
        /// <param name="volume">本次播放的局部音量（0-1），与 SFX 通道音量、总音量相乘生效。</param>
        /// <param name="pitch">本次播放的基准音调。</param>
        /// <param name="pitchJitter">音调随机抖动幅度（非负）：最终音调在 [pitch - jitter, pitch + jitter] 内随机，用于脚步/射击等防止机械感。</param>
        public static void PlaySfx(AudioClip clip,
            float volume = 1f,
            float pitch = 1f,
            float pitchJitter = 0f)
        {
            if (clip == null)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.AudioModuleTag, "播放音效失败：clip 为空");
                return;
            }

            var m = Ready();

            // 轮询取下一个独占源；源在播时直接前进（全忙即抢占最旧，轮询序即分配序）
            var index = m._sfxCursor;
            m._sfxCursor = (m._sfxCursor + 1) % m._sfxSources.Length;
            var source = m._sfxSources[index];

            m._sfxLocalVolumes[index] = Mathf.Clamp01(volume);
            source.clip = clip;
            source.pitch = pitch + Random.Range(-pitchJitter, pitchJitter);
            source.volume = m._sfxVolume * m._masterVolume * m._sfxLocalVolumes[index];
            source.Play();
        }

        #endregion

        #region 公开 API — 配置

        /// <summary>
        /// 替换运行时配置并重新载入音量（持久化值优先于配置默认值）。
        /// 传入 null 恢复为代码默认配置。预放置实例调用会改写序列化的资产引用。
        /// </summary>
        /// <param name="config">新配置资产（可为 null）。</param>
        public static void ApplyConfig(AudioConfigSO config)
        {
            var m = Ready();
            m.config = config;
            m.LoadVolumesFromConfig();
            m.ApplyVolumes();
            m.ApplyMutes();
        }

        #endregion

        #region 序列化字段

        /// <summary>
        /// 是否将本物体加入 DontDestroyOnLoad 场景。仅在本物体为根物体（场景预放置）时生效。
        /// </summary>
        /// <remarks>
        /// 默认 true（跨场景持久）。设为 false 时实例保留在所在场景、随场景卸载销毁，
        /// 必须自行处理多场景叠加（Additive）加载下的生命周期管理。
        /// 运行时自动创建的实例挂载在 [Aesir Modules] 宿主下（非根物体），
        /// DDOL 跟随宿主决策，本字段不参与判断。
        /// <para>
        /// Inspector 呈现（字段说明 InfoBox 与关闭警告 InfoBox）由
        /// <c>AudioModuleAttributeProcessor</c> 动态注入，运行时代码不持有任何 Inspector 样式特性。
        /// </para>
        /// </remarks>
        [SerializeField]
        bool dontDestroyOnLoad = true;

        /// <summary>
        /// SFX 独占音源数量。源全忙时按轮询序抢占最旧的源；初始化时一次性创建，运行时修改无效。
        /// </summary>
        [Tooltip("SFX 独占音源数量 — 每次播放轮询取下一个源，全忙时抢占最旧；初始化时创建，运行时修改无效")]
        [SerializeField]
        int sfxSourceCount = 8;

        /// <summary>
        /// 可选的模块配置资产。未指定时使用代码默认值（通道音量 1、持久化开启、键前缀 AesirAudio）。
        /// </summary>
        [Tooltip("可选的模块配置资产 — 默认音量、持久化开关与 PlayerPrefs 键前缀；留空时使用代码默认值")]
        [SerializeField]
        AudioConfigSO config;

        #endregion

        #region 运行时状态

        AudioSource _bgmSource;
        AudioSource[] _sfxSources;
        float[] _sfxLocalVolumes;
        int _sfxCursor;
        bool _initialized;

        float _masterVolume = 1f;
        float _bgmVolume = 1f;
        float _sfxVolume = 1f;
        bool _masterMute;
        bool _bgmMute;
        bool _sfxMute;

        /// <summary>BGM 淡入淡出系数（0-1），与通道音量、总音量相乘生效，淡变协程仅修改此值。</summary>
        float _bgmFadeFactor = 1f;

        Coroutine _bgmFadeRoutine;

        bool _persistVolumes = true;
        string _prefsKey = DefaultPrefsKey;

        string MasterVolumeKey => _prefsKey + ".MasterVolume";
        string BgmVolumeKey => _prefsKey + ".BgmVolume";
        string SfxVolumeKey => _prefsKey + ".SfxVolume";
        string MasterMuteKey => _prefsKey + ".MasterMute";
        string BgmMuteKey => _prefsKey + ".BgmMute";
        string SfxMuteKey => _prefsKey + ".SfxMute";

        #endregion

        #region 单例与生命周期

        /// <summary>
        /// 全局单例入口。
        /// 优先在已加载场景中查找预放置的实例；未找到时在 <see cref="AesirModules" />（DDOL）下创建子物体。
        /// </summary>
        public static AudioModule Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                // 尝试在已加载的场景中查找预放置的实例
                // 使用 FindAnyObjectByType 而非 FindFirstObjectByType，后者因依赖 InstanceID 排序在 Unity 6 中已废弃
                _instance = FindAnyObjectByType<AudioModule>();
                if (_instance != null)
                {
                    return _instance;
                }

                // 未找到预放置实例 → 在 AesirModules 下创建（跟随父级 DDOL）
                _instance = AesirModules.GetOrAddChild<AudioModule>();
                return _instance;
            }
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            EnsureInitialized();

            // 非根物体（运行时自动创建于 [Aesir Modules] 宿主下）时 DDOL 跟随宿主，本字段不参与判断
            if (!dontDestroyOnLoad)
            {
                AesirModulesDebug.LogWarning(AesirModulesDebug.AudioModuleTag,
                    "dontDestroyOnLoad 已关闭：实例保留在所在场景、随场景卸载销毁，" + "必须自行处理多场景叠加（Additive）加载下的生命周期");
            }
            else if (transform.root == transform)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        void OnDestroy()
        {
            if (_instance != null && _instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region 公开 API — BGM

        /// <summary>
        /// 播放背景音乐（循环）。正在播放同一片段时幂等返回——跨场景重复触发不打断音乐。
        /// </summary>
        /// <param name="clip">音频片段。</param>
        /// <param name="fadeSeconds">
        /// 淡变时长（秒）。0 表示立即切换；大于 0 时旧曲先在此时长内淡出，随后新曲在同一时长内淡入。
        /// </param>
        public static void PlayBgm(AudioClip clip, float fadeSeconds = 0f)
        {
            if (clip == null)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.AudioModuleTag, "播放背景音乐失败：clip 为空");
                return;
            }

            var m = Ready();

            // 同曲正在播放 → 幂等返回（重复进入场景不打断音乐）
            if (m._bgmSource.clip == clip && m._bgmSource.isPlaying)
            {
                return;
            }

            if (fadeSeconds <= 0f)
            {
                m.StopBgmFade();
                m._bgmFadeFactor = 1f;
                m._bgmSource.clip = clip;
                m.ApplyVolumes();
                m._bgmSource.Play();
                return;
            }

            m.StartBgmFade(m.SwitchBgmRoutine(clip, fadeSeconds));
        }

        /// <summary>
        /// 停止背景音乐。停止后 <see cref="CurrentBgm" /> 保留最后一次播放的片段（不清除）。
        /// </summary>
        /// <param name="fadeSeconds">淡出时长（秒）。0 表示立即停止。</param>
        public static void StopBgm(float fadeSeconds = 0f)
        {
            var m = Ready();

            if (fadeSeconds <= 0f)
            {
                m.StopBgmFade();
                m._bgmFadeFactor = 1f;
                m.ApplyVolumes();
                m._bgmSource.Stop();
                return;
            }

            m.StartBgmFade(m.StopBgmRoutine(fadeSeconds));
        }

        /// <summary>
        /// 当前背景音乐片段。停止播放后仍保留最后一次播放的片段，未播放过时为 null。
        /// </summary>
        public static AudioClip CurrentBgm => Ready()._bgmSource.clip;

        /// <summary>
        /// 背景音乐是否正在播放。
        /// </summary>
        public static bool IsBgmPlaying => Ready()._bgmSource.isPlaying;

        #endregion

        #region 公开 API — 暂停

        /// <summary>
        /// 暂停全部音频（BGM 与所有 SFX 音源）。适合暂停菜单与切后台（配合 <c>OnApplicationPause</c>）。
        /// </summary>
        public static void PauseAll()
        {
            var m = Ready();
            m._bgmSource.Pause();
            for (var i = 0; i < m._sfxSources.Length; i++)
            {
                m._sfxSources[i].Pause();
            }
        }

        /// <summary>
        /// 恢复全部音频，与 <see cref="PauseAll" /> 成对使用；未暂停的音源调用无副作用。
        /// </summary>
        public static void ResumeAll()
        {
            var m = Ready();
            m._bgmSource.UnPause();
            for (var i = 0; i < m._sfxSources.Length; i++)
            {
                m._sfxSources[i].UnPause();
            }
        }

        #endregion

        #region 公开 API — 音量与静音

        /// <summary>
        /// 总音量（0-1），与各通道音量相乘生效。设置即时生效，并按配置持久化到 PlayerPrefs。
        /// </summary>
        public static float MasterVolume
        {
            get => Ready()._masterVolume;
            set
            {
                var m = Ready();
                m._masterVolume = Mathf.Clamp01(value);
                if (m._persistVolumes)
                {
                    PlayerPrefs.SetFloat(m.MasterVolumeKey, m._masterVolume);
                }

                m.ApplyVolumes();
            }
        }

        /// <summary>
        /// 背景音乐通道音量（0-1），与总音量相乘生效。设置即时生效，并按配置持久化。
        /// </summary>
        public static float BgmVolume
        {
            get => Ready()._bgmVolume;
            set
            {
                var m = Ready();
                m._bgmVolume = Mathf.Clamp01(value);
                if (m._persistVolumes)
                {
                    PlayerPrefs.SetFloat(m.BgmVolumeKey, m._bgmVolume);
                }

                m.ApplyVolumes();
            }
        }

        /// <summary>
        /// 音效通道音量（0-1），与总音量相乘生效。设置即时生效，并按配置持久化。
        /// </summary>
        public static float SfxVolume
        {
            get => Ready()._sfxVolume;
            set
            {
                var m = Ready();
                m._sfxVolume = Mathf.Clamp01(value);
                if (m._persistVolumes)
                {
                    PlayerPrefs.SetFloat(m.SfxVolumeKey, m._sfxVolume);
                }

                m.ApplyVolumes();
            }
        }

        /// <summary>
        /// 总静音开关（总闸，与各通道静音相或生效）。设置即时生效，并按配置持久化。
        /// </summary>
        public static bool MasterMute
        {
            get => Ready()._masterMute;
            set
            {
                var m = Ready();
                m._masterMute = value;
                if (m._persistVolumes)
                {
                    PlayerPrefs.SetInt(m.MasterMuteKey, value ? 1 : 0);
                }

                m.ApplyMutes();
            }
        }

        /// <summary>
        /// 背景音乐静音开关，与总静音相或生效。设置即时生效，并按配置持久化。
        /// </summary>
        public static bool BgmMute
        {
            get => Ready()._bgmMute;
            set
            {
                var m = Ready();
                m._bgmMute = value;
                if (m._persistVolumes)
                {
                    PlayerPrefs.SetInt(m.BgmMuteKey, value ? 1 : 0);
                }

                m.ApplyMutes();
            }
        }

        /// <summary>
        /// 音效静音开关，与总静音相或生效。设置即时生效，并按配置持久化。
        /// </summary>
        public static bool SfxMute
        {
            get => Ready()._sfxMute;
            set
            {
                var m = Ready();
                m._sfxMute = value;
                if (m._persistVolumes)
                {
                    PlayerPrefs.SetInt(m.SfxMuteKey, value ? 1 : 0);
                }

                m.ApplyMutes();
            }
        }

        #endregion

        #region 内部实现

        /// <summary>
        /// 获取已完成初始化的单例，所有公开静态 API 的统一入口。
        /// </summary>
        static AudioModule Ready()
        {
            var m = Instance;
            m.EnsureInitialized();
            return m;
        }

        /// <summary>
        /// 懒初始化：载入配置与持久化音量，创建 BGM 音源与 SFX 独占音源组。
        /// 预放置实例在 Awake 调用；编辑器与测试环境未经 Awake 时由 <see cref="Ready" /> 兜底。
        /// </summary>
        void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            LoadVolumesFromConfig();

            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.playOnAwake = false;
            _bgmSource.loop = true;

            var count = Mathf.Max(1, sfxSourceCount);
            _sfxSources = new AudioSource[count];
            _sfxLocalVolumes = new float[count];
            for (var i = 0; i < count; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                _sfxSources[i] = source;
                _sfxLocalVolumes[i] = 1f;
            }

            ApplyVolumes();
            ApplyMutes();
        }

        /// <summary>
        /// 载入配置默认值与持久化音量：持久化键存在时优先于配置默认值；
        /// 未挂配置资产时使用代码默认值（通道音量 1、持久化开启）。
        /// </summary>
        void LoadVolumesFromConfig()
        {
            _persistVolumes = config == null || config.persistVolumes;
            _prefsKey = config != null && !string.IsNullOrEmpty(config.prefsKey)
                ? config.prefsKey
                : DefaultPrefsKey;
            _masterVolume = config != null ? config.masterVolume : 1f;
            _bgmVolume = config != null ? config.bgmVolume : 1f;
            _sfxVolume = config != null ? config.sfxVolume : 1f;
            _masterMute = false;
            _bgmMute = false;
            _sfxMute = false;

            if (!_persistVolumes)
            {
                return;
            }

            if (PlayerPrefs.HasKey(MasterVolumeKey))
            {
                _masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey);
            }

            if (PlayerPrefs.HasKey(BgmVolumeKey))
            {
                _bgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey);
            }

            if (PlayerPrefs.HasKey(SfxVolumeKey))
            {
                _sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey);
            }

            if (PlayerPrefs.HasKey(MasterMuteKey))
            {
                _masterMute = PlayerPrefs.GetInt(MasterMuteKey) == 1;
            }

            if (PlayerPrefs.HasKey(BgmMuteKey))
            {
                _bgmMute = PlayerPrefs.GetInt(BgmMuteKey) == 1;
            }

            if (PlayerPrefs.HasKey(SfxMuteKey))
            {
                _sfxMute = PlayerPrefs.GetInt(SfxMuteKey) == 1;
            }
        }

        /// <summary>
        /// 将音量乘法链单点写入全部音源：BGM = 通道 × 总 × 淡变系数；SFX = 通道 × 总 × 局部音量。
        /// </summary>
        void ApplyVolumes()
        {
            _bgmSource.volume = _bgmVolume * _masterVolume * _bgmFadeFactor;
            for (var i = 0; i < _sfxSources.Length; i++)
            {
                _sfxSources[i].volume = _sfxVolume * _masterVolume * _sfxLocalVolumes[i];
            }
        }

        /// <summary>
        /// 将静音链单点写入全部音源：BGM = 总静音或通道静音；SFX = 总静音或通道静音。
        /// </summary>
        void ApplyMutes()
        {
            _bgmSource.mute = _masterMute || _bgmMute;
            for (var i = 0; i < _sfxSources.Length; i++)
            {
                _sfxSources[i].mute = _masterMute || _sfxMute;
            }
        }

        /// <summary>
        /// 切歌协程：旧曲淡出 → 换片段播放 → 新曲淡入，两段各占 fadeSeconds。
        /// BGM 未在播放时跳过淡出段，直接从 0 淡入（首次播放/停止后重播不空等淡出时长）。
        /// </summary>
        IEnumerator SwitchBgmRoutine(AudioClip clip, float fadeSeconds)
        {
            if (_bgmSource.isPlaying)
            {
                yield return FadeBgmRoutine(0f, fadeSeconds);
            }
            else
            {
                _bgmFadeFactor = 0f;
            }

            _bgmSource.clip = clip;
            _bgmSource.Play();
            yield return FadeBgmRoutine(1f, fadeSeconds);
        }

        /// <summary>
        /// 停止协程：淡出 → 停止播放（片段保留）。
        /// </summary>
        IEnumerator StopBgmRoutine(float fadeSeconds)
        {
            yield return FadeBgmRoutine(0f, fadeSeconds);
            _bgmSource.Stop();
        }

        /// <summary>
        /// 将淡变系数向目标值逐帧插值。基于 unscaledDeltaTime——BGM 不受 timeScale 影响（slow motion 不变调）。
        /// <para>
        /// MoveTowards 到达时精确返回目标值，循环条件用精确比较即可安全退出；
        /// 连续切歌时新协程从当前系数续接，无跳变。
        /// </para>
        /// </summary>
        IEnumerator FadeBgmRoutine(float target, float fadeSeconds)
        {
            while (_bgmFadeFactor != target)
            {
                _bgmFadeFactor =
                    Mathf.MoveTowards(_bgmFadeFactor, target, Time.unscaledDeltaTime / fadeSeconds);
                ApplyVolumes();
                yield return null;
            }
        }

        void StartBgmFade(IEnumerator routine)
        {
            StopBgmFade();
            _bgmFadeRoutine = StartCoroutine(routine);
        }

        void StopBgmFade()
        {
            if (_bgmFadeRoutine != null)
            {
                StopCoroutine(_bgmFadeRoutine);
                _bgmFadeRoutine = null;
            }
        }

        #endregion
    }
}
