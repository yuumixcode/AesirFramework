using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runestone.AesirModules.Tests.Editor.Audio
{
    /// <summary>
    /// AudioModule 的 EditMode 测试：
    /// 单例生命周期、SFX 独占源轮询、BGM 播放语义、音量/静音乘法链、
    /// PlayerPrefs 持久化与 AudioConfigSO 配置载入。
    /// <para>
    /// 测试自建模块实例（挂 [Aesir Modules] 宿主下、反射驱动 Awake），
    /// TearDown 全量销毁创建的 GameObject / 资产并清理 PlayerPrefs 键，
    /// 避免污染编辑器全局状态。
    /// </para>
    /// </summary>
    public class AudioModuleTests
    {
        const string TestPrefsKey = "AesirAudioTest";
        const string DefaultPrefsKey = "AesirAudio";

        static readonly string[] PrefsKeySuffixes =
        {
            ".MasterVolume", ".BgmVolume", ".SfxVolume", ".MasterMute", ".BgmMute", ".SfxMute"
        };

        #region 环境管理

        readonly List<GameObject> _createdObjects = new List<GameObject>();
        readonly List<Object> _createdAssets = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            DeletePrefsKeys(DefaultPrefsKey);
            DeletePrefsKeys(TestPrefsKey);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var created in _createdObjects)
            {
                if (created != null)
                {
                    Object.DestroyImmediate(created);
                }
            }

            _createdObjects.Clear();

            foreach (var asset in _createdAssets)
            {
                if (asset != null)
                {
                    Object.DestroyImmediate(asset);
                }
            }

            _createdAssets.Clear();

            DeletePrefsKeys(DefaultPrefsKey);
            DeletePrefsKeys(TestPrefsKey);
        }

        /// <summary>创建 GameObject 并登记，TearDown 统一销毁。</summary>
        GameObject NewGameObject(string name)
        {
            var gameObject = new GameObject(name);
            _createdObjects.Add(gameObject);
            return gameObject;
        }

        /// <summary>
        /// 创建挂载在 [Aesir Modules] 宿主下的模块实例（非根物体 → Awake 的 DDOL 分支不触发），
        /// 经反射驱动 Awake 完成初始化。
        /// </summary>
        AudioModule CreateModule(int sfxCount = 8, AudioConfigSO config = null)
        {
            var host = AesirModules.Instance;
            if (!_createdObjects.Contains(host.gameObject))
            {
                _createdObjects.Add(host.gameObject);
            }

            var go = NewGameObject("TestAudioModule");
            go.transform.SetParent(host.transform, false);
            var module = go.AddComponent<AudioModule>();

            var so = new SerializedObject(module);
            if (sfxCount != 8)
            {
                so.FindProperty("sfxSourceCount").intValue = sfxCount;
            }

            if (config != null)
            {
                so.FindProperty("config").objectReferenceValue = config;
            }

            so.ApplyModifiedProperties();

            InvokePrivate(module, "Awake");
            return module;
        }

        /// <summary>创建运行时测试用 AudioClip（短单声道，无需磁盘资产）。</summary>
        static AudioClip NewClip(string name) => AudioClip.Create(name, 44100, 1, 44100, false);

        /// <summary>创建配置资产并登记，字段经 InternalsVisibleTo 直接赋值。</summary>
        AudioConfigSO NewConfig(float master = 1f,
            float bgm = 1f,
            float sfx = 1f,
            bool persist = true,
            string key = TestPrefsKey)
        {
            var config = ScriptableObject.CreateInstance<AudioConfigSO>();
            _createdAssets.Add(config);
            config.masterVolume = master;
            config.bgmVolume = bgm;
            config.sfxVolume = sfx;
            config.persistVolumes = persist;
            config.prefsKey = key;
            return config;
        }

        static AudioSource GetBgmSource(AudioModule module) =>
            module.GetComponents<AudioSource>().First(s => s.loop);

        static AudioSource[] GetSfxSources(AudioModule module) =>
            module.GetComponents<AudioSource>().Where(s => !s.loop).ToArray();

        static void InvokePrivate(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "未找到私有方法：" + methodName);
            method.Invoke(target, null);
        }

        static void DeletePrefsKeys(string prefix)
        {
            foreach (var suffix in PrefsKeySuffixes)
            {
                PlayerPrefs.DeleteKey(prefix + suffix);
            }
        }

        #endregion

        #region 单例与生命周期

        [Test]
        public void Instance_CreatesUnderAesirModulesHost_WhenNoPreplaced()
        {
            var created = AudioModule.Instance;

            Assert.IsNotNull(created);
            Assert.IsNotNull(created.transform.parent);
            Assert.IsInstanceOf<AesirModules>(created.transform.parent.GetComponent<AesirModules>());

            if (!_createdObjects.Contains(created.transform.parent.gameObject))
            {
                _createdObjects.Add(created.transform.parent.gameObject);
            }
        }

        [Test]
        public void Instance_PrefersPreplacedInScene()
        {
            var preplaced = NewGameObject("PreplacedAudioModule").AddComponent<AudioModule>();

            var found = AudioModule.Instance;

            Assert.AreSame(preplaced, found);
        }

        [Test]
        public void Awake_DuplicateInstance_DoesNotStealSingleton()
        {
            var first = CreateModule();

            var go = NewGameObject("DuplicateAudioModule");
            go.transform.SetParent(AesirModules.Instance.transform, false);
            var duplicate = go.AddComponent<AudioModule>();
            // EditMode 反射驱动 Awake 时，Awake 内的 Destroy 会输出 Unity 内置错误提示（转 DestroyImmediate 语义）
            LogAssert.Expect(LogType.Error, new Regex("Destroy may not be called from edit mode"));
            InvokePrivate(duplicate, "Awake");

            Assert.AreSame(first, AudioModule.Instance);
        }

        [Test]
        public void OnDestroy_ClearsSingleton()
        {
            var module = CreateModule();
            Assert.AreSame(module, AudioModule.Instance);

            Object.DestroyImmediate(module.gameObject);

            var next = AudioModule.Instance;
            Assert.AreNotSame(module, next);
            if (!_createdObjects.Contains(next.transform.parent.gameObject))
            {
                _createdObjects.Add(next.transform.parent.gameObject);
            }
        }

        #endregion

        #region SFX — 独占源轮询

        [Test]
        public void PlaySfx_RotatesThroughSources()
        {
            var module = CreateModule();
            var sfxSources = GetSfxSources(module);
            var clipA = NewClip("A");
            var clipB = NewClip("B");
            var clipC = NewClip("C");

            AudioModule.PlaySfx(clipA);
            AudioModule.PlaySfx(clipB);
            AudioModule.PlaySfx(clipC);

            Assert.AreSame(clipA, sfxSources[0].clip);
            Assert.AreSame(clipB, sfxSources[1].clip);
            Assert.AreSame(clipC, sfxSources[2].clip);
            Assert.IsNull(sfxSources[3].clip);
        }

        [Test]
        public void PlaySfx_WrapsAroundWhenAllSourcesUsed()
        {
            var module = CreateModule();
            var sfxSources = GetSfxSources(module);

            for (var i = 0; i <= sfxSources.Length; i++)
            {
                AudioModule.PlaySfx(NewClip("Clip" + i));
            }

            // 超过源数量后回到起点，抢占最旧的源
            Assert.AreEqual("Clip" + sfxSources.Length, sfxSources[0].clip.name);
            Assert.AreEqual("Clip8", sfxSources[8 % sfxSources.Length].clip.name);
        }

        [Test]
        public void PlaySfx_WritesLocalVolumeAndPitch()
        {
            var module = CreateModule();
            var sfxSources = GetSfxSources(module);
            var clip = NewClip("Clip");

            AudioModule.PlaySfx(clip, 0.5f, 2f);

            Assert.AreEqual(0.5f, sfxSources[0].volume, 1e-4f);
            Assert.AreEqual(2f, sfxSources[0].pitch, 1e-4f);
        }

        [Test]
        public void PlaySfx_PitchJitter_StaysWithinRange()
        {
            var module = CreateModule();
            var sfxSources = GetSfxSources(module);
            var clip = NewClip("Clip");

            for (var i = 0; i < 100; i++)
            {
                AudioModule.PlaySfx(clip, 1f, 1f, 0.2f);
            }

            foreach (var source in sfxSources)
            {
                Assert.GreaterOrEqual(source.pitch, 0.8f - 1e-4f);
                Assert.LessOrEqual(source.pitch, 1.2f + 1e-4f);
            }
        }

        [Test]
        public void PlaySfx_ClampsLocalVolume()
        {
            var module = CreateModule();
            var sfxSources = GetSfxSources(module);

            AudioModule.PlaySfx(NewClip("Clip"), 1.5f);
            Assert.AreEqual(1f, sfxSources[0].volume, 1e-4f);

            AudioModule.PlaySfx(NewClip("Clip"), -0.5f);
            Assert.AreEqual(0f, sfxSources[1].volume, 1e-4f);
        }

        [Test]
        public void PlaySfx_NullClip_LogsErrorAndKeepsSourcesUntouched()
        {
            var module = CreateModule();
            var sfxSources = GetSfxSources(module);
            LogAssert.Expect(LogType.Error, new Regex("clip 为空"));

            AudioModule.PlaySfx(null);

            foreach (var source in sfxSources)
            {
                Assert.IsNull(source.clip);
            }
        }

        #endregion

        #region BGM

        [Test]
        public void PlayBgm_SetsClipAndLoop()
        {
            var module = CreateModule();
            var bgmSource = GetBgmSource(module);
            var clip = NewClip("Bgm");

            AudioModule.PlayBgm(clip);

            Assert.AreSame(clip, bgmSource.clip);
            Assert.IsTrue(bgmSource.loop);
            Assert.AreEqual(1f, bgmSource.volume, 1e-4f);
        }

        [Test]
        public void PlayBgm_SwitchesClipImmediately_WithoutFade()
        {
            var module = CreateModule();
            var bgmSource = GetBgmSource(module);
            var clipA = NewClip("BgmA");
            var clipB = NewClip("BgmB");

            AudioModule.PlayBgm(clipA);
            AudioModule.PlayBgm(clipB);

            Assert.AreSame(clipB, bgmSource.clip);
        }

        [Test]
        public void StopBgm_KeepsClipForRestart()
        {
            var module = CreateModule();
            var bgmSource = GetBgmSource(module);
            var clip = NewClip("Bgm");

            AudioModule.PlayBgm(clip);
            AudioModule.StopBgm();

            Assert.AreSame(clip, bgmSource.clip);
            Assert.AreSame(clip, AudioModule.CurrentBgm);
        }

        [Test]
        public void CurrentBgm_Null_WhenNeverPlayed()
        {
            var module = CreateModule();

            Assert.IsNull(AudioModule.CurrentBgm);
            Assert.IsFalse(AudioModule.IsBgmPlaying);
        }

        [Test]
        public void PlayBgm_NullClip_LogsError()
        {
            var module = CreateModule();
            var bgmSource = GetBgmSource(module);
            LogAssert.Expect(LogType.Error, new Regex("clip 为空"));

            AudioModule.PlayBgm(null);

            Assert.IsNull(bgmSource.clip);
        }

        #endregion

        #region 音量乘法链

        [Test]
        public void VolumeChain_AppliesToAllSources()
        {
            var module = CreateModule();
            var bgmSource = GetBgmSource(module);
            var sfxSources = GetSfxSources(module);

            AudioModule.MasterVolume = 0.5f;
            AudioModule.BgmVolume = 0.8f;
            AudioModule.SfxVolume = 0.6f;

            Assert.AreEqual(0.4f, bgmSource.volume, 1e-4f);
            foreach (var source in sfxSources)
            {
                Assert.AreEqual(0.3f, source.volume, 1e-4f);
            }
        }

        [Test]
        public void VolumeChain_LocalVolumeSurvivesChannelChange()
        {
            var module = CreateModule();
            var sfxSources = GetSfxSources(module);

            AudioModule.PlaySfx(NewClip("Clip"), 0.5f);
            AudioModule.SfxVolume = 0.5f;

            // 局部音量 0.5 不被通道音量更新冲掉：0.5（通道）× 1（总）× 0.5（局部）
            Assert.AreEqual(0.25f, sfxSources[0].volume, 1e-4f);
        }

        [Test]
        public void VolumeSetter_ClampsToUnitRange()
        {
            var module = CreateModule();

            AudioModule.MasterVolume = 2f;
            Assert.AreEqual(1f, AudioModule.MasterVolume, 1e-4f);

            AudioModule.SfxVolume = -1f;
            Assert.AreEqual(0f, AudioModule.SfxVolume, 1e-4f);
        }

        [Test]
        public void VolumeGetter_ReflectsPersistedValueOnInit()
        {
            PlayerPrefs.SetFloat(TestPrefsKey + ".MasterVolume", 0.33f);
            PlayerPrefs.Save();

            var module = CreateModule(config: NewConfig());

            Assert.AreEqual(0.33f, AudioModule.MasterVolume, 1e-4f);
        }

        #endregion

        #region 静音链

        [Test]
        public void MasterMute_MutesAllSources()
        {
            var module = CreateModule();
            var bgmSource = GetBgmSource(module);
            var sfxSources = GetSfxSources(module);

            AudioModule.MasterMute = true;

            Assert.IsTrue(bgmSource.mute);
            foreach (var source in sfxSources)
            {
                Assert.IsTrue(source.mute);
            }
        }

        [Test]
        public void ChannelMute_OnlyMutesOwnChannel()
        {
            var module = CreateModule();
            var bgmSource = GetBgmSource(module);
            var sfxSources = GetSfxSources(module);

            AudioModule.BgmMute = true;

            Assert.IsTrue(bgmSource.mute);
            foreach (var source in sfxSources)
            {
                Assert.IsFalse(source.mute);
            }

            AudioModule.SfxMute = true;
            AudioModule.BgmMute = false;

            Assert.IsFalse(bgmSource.mute);
            foreach (var source in sfxSources)
            {
                Assert.IsTrue(source.mute);
            }
        }

        [Test]
        public void MuteSetter_PersistsAsInt()
        {
            var module = CreateModule(config: NewConfig());

            AudioModule.MasterMute = true;

            Assert.AreEqual(1, PlayerPrefs.GetInt(TestPrefsKey + ".MasterMute"));
        }

        #endregion

        #region 持久化

        [Test]
        public void VolumeSetter_PersistsToPlayerPrefs()
        {
            var module = CreateModule(config: NewConfig());

            AudioModule.MasterVolume = 0.7f;
            AudioModule.BgmVolume = 0.6f;
            AudioModule.SfxVolume = 0.5f;
            AudioModule.BgmMute = true;

            Assert.AreEqual(0.7f, PlayerPrefs.GetFloat(TestPrefsKey + ".MasterVolume"), 1e-4f);
            Assert.AreEqual(0.6f, PlayerPrefs.GetFloat(TestPrefsKey + ".BgmVolume"), 1e-4f);
            Assert.AreEqual(0.5f, PlayerPrefs.GetFloat(TestPrefsKey + ".SfxVolume"), 1e-4f);
            Assert.AreEqual(1, PlayerPrefs.GetInt(TestPrefsKey + ".BgmMute"));
        }

        [Test]
        public void PersistDisabled_DoesNotReadOrWrite()
        {
            var config = NewConfig(persist: false);
            var module = CreateModule(config: config);

            // 持久化关闭：已存在的键被忽略，读配置默认值
            PlayerPrefs.SetFloat(TestPrefsKey + ".MasterVolume", 0.9f);
            AudioModule.ApplyConfig(config);
            Assert.AreEqual(1f, AudioModule.MasterVolume, 1e-4f);

            // 持久化关闭：写设置不落键
            AudioModule.MasterVolume = 0.2f;
            Assert.AreEqual(0.9f, PlayerPrefs.GetFloat(TestPrefsKey + ".MasterVolume"), 1e-4f);
        }

        #endregion

        #region 配置

        [Test]
        public void ApplyConfig_AppliesDefaultVolumes()
        {
            var module = CreateModule();

            AudioModule.ApplyConfig(NewConfig(0.8f, 0.6f, 0.4f));

            Assert.AreEqual(0.8f, AudioModule.MasterVolume, 1e-4f);
            Assert.AreEqual(0.6f, AudioModule.BgmVolume, 1e-4f);
            Assert.AreEqual(0.4f, AudioModule.SfxVolume, 1e-4f);
            var bgmSource = GetBgmSource(module);
            Assert.AreEqual(0.48f, bgmSource.volume, 1e-4f);
        }

        [Test]
        public void ApplyConfig_Null_FallsBackToCodeDefaults()
        {
            var module = CreateModule(config: NewConfig(0.8f, 0.6f, 0.4f));

            AudioModule.ApplyConfig(null);

            Assert.AreEqual(1f, AudioModule.MasterVolume, 1e-4f);
            Assert.AreEqual(1f, AudioModule.BgmVolume, 1e-4f);
            Assert.AreEqual(1f, AudioModule.SfxVolume, 1e-4f);
        }

        [Test]
        public void ApplyConfig_EmptyPrefsKey_FallsBackToDefault()
        {
            var module = CreateModule();

            AudioModule.ApplyConfig(NewConfig(key: ""));

            AudioModule.MasterVolume = 0.7f;

            Assert.AreEqual(0.7f, PlayerPrefs.GetFloat(DefaultPrefsKey + ".MasterVolume"), 1e-4f);
        }

        #endregion

        #region 暂停与音源数量

        [Test]
        public void PauseAll_ResumeAll_DoesNotThrow()
        {
            var module = CreateModule();
            AudioModule.PlayBgm(NewClip("Bgm"));
            AudioModule.PlaySfx(NewClip("Sfx"));

            Assert.DoesNotThrow(() =>
            {
                AudioModule.PauseAll();
                AudioModule.PauseAll();
                AudioModule.ResumeAll();
                AudioModule.ResumeAll();
            });
        }

        [Test]
        public void SfxSourceCount_CreatesThatManySources()
        {
            var module = CreateModule(3);

            // 1 个 BGM 源 + 3 个 SFX 源
            Assert.AreEqual(4, module.GetComponents<AudioSource>().Length);
        }

        [Test]
        public void SfxSourceCount_ClampedToOne()
        {
            var module = CreateModule(0);

            // 1 个 BGM 源 + 1 个 SFX 源
            Assert.AreEqual(2, module.GetComponents<AudioSource>().Length);
        }

        #endregion
    }
}
