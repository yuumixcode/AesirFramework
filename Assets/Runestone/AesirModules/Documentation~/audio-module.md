# 音频模块（Audio Module）

## 概述

音频模块提供 2D 音频极简门面：SFX 轮询播放、BGM 循环与淡入淡出、三通道音量/静音控制与持久化。公开 API 全部为静态成员，经全局单例转发，调用即用。

- **SFX**：固定数量独占音源轮询（默认 8，可配）——等效池化，无每播实例化开销；每次播放的局部音量与音调独立生效；源全忙时按轮询序抢占最旧
- **BGM**：专用循环音源；同曲在播时幂等返回（跨场景重复触发不打断音乐）；切换支持协程淡入淡出（基于 `Time.unscaledDeltaTime`，slow motion 不变调）
- **音量**：Master / BGM / SFX 三通道乘法链，设置即时生效并经 PlayerPrefs 持久化（重启自动恢复）
- **静音**：三通道独立开关，Master 为总闸
- **暂停**：`PauseAll` / `ResumeAll` 一对，适合暂停菜单与切后台（配合 `OnApplicationPause`）

> **注意**：模块仅负责 2D 音频。3D 空间音效请使用原生 `AudioSource.PlayClipAtPoint`；需要衰减曲线或 DSP 效果请自建音源。不集成 AudioMixer（音量直接写入音源）。

## 核心类型

### AudioModule

MonoBehaviour 单例，直接继承 `AesirMonoBehaviour`。预放置于场景时优先被发现；未预放置时自动挂载到 `[Aesir Modules]` 宿主下（跟随宿主 DDOL 决策）。公开 API 全为静态成员。

```csharp
// SFX（fire-and-forget）
public static void PlaySfx(AudioClip clip, float volume = 1f, float pitch = 1f, float pitchJitter = 0f);

// BGM
public static void PlayBgm(AudioClip clip, float fadeSeconds = 0f);  // 同曲在播幂等返回
public static void StopBgm(float fadeSeconds = 0f);                  // 停止后 CurrentBgm 保留
public static AudioClip CurrentBgm { get; }
public static bool IsBgmPlaying { get; }

// 暂停（BGM 源 + 全部 SFX 源）
public static void PauseAll();
public static void ResumeAll();

// 音量（0-1，三通道乘法链：channel × master）
public static float MasterVolume { get; set; }
public static float BgmVolume { get; set; }
public static float SfxVolume { get; set; }

// 静音（总闸与通道相或）
public static bool MasterMute { get; set; }
public static bool BgmMute { get; set; }
public static bool SfxMute { get; set; }

// 配置
public static void ApplyConfig(AudioConfigSO config);  // 运行时替换配置并重新载入

// 单例
public static AudioModule Instance { get; }

// 预放置实例可调（序列化字段）
// [SerializeField] int sfxSourceCount = 8;  — SFX 独占音源数量
// [SerializeField] AudioConfigSO config;  — 可选配置资产
// [SerializeField] bool dontDestroyOnLoad = true;  — 预放置为根物体时生效
```

### AudioConfigSO

模块配置资产。创建路径：`Assets → Create → Aesir Modules → Audio → AudioConfig`。

| 字段 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `masterVolume` | float | 1 | 默认总音量（0-1） |
| `bgmVolume` | float | 1 | 默认 BGM 通道音量（0-1） |
| `sfxVolume` | float | 1 | 默认音效通道音量（0-1） |
| `persistVolumes` | bool | true | 音量/静音是否经 PlayerPrefs 持久化 |
| `prefsKey` | string | `"AesirAudio"` | PlayerPrefs 键前缀（实际键如 `AesirAudio.MasterVolume`） |

> 持久化值优先于配置默认值：玩家调整过的音量（已存键）在初始化时覆盖 SO 默认值；`persistVolumes` 关闭时读写均不发生。

## 快速开始

零配置即可使用——无预放置、无配置资产时使用代码默认值（全音量 1、持久化开启）：

```csharp
using Runestone.AesirModules;
using UnityEngine;

public class AudioUsageExample : MonoBehaviour
{
    [SerializeField] AudioClip clickSound;
    [SerializeField] AudioClip bgm;

    void Start()
    {
        // BGM：循环播放，跨场景持续（自动挂载到 [Aesir Modules] 下）
        AudioModule.PlayBgm(bgm);
    }

    void Update()
    {
        // SFX：一击即走，音调 ±0.1 随机抖动防止机械感
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AudioModule.PlaySfx(clickSound, pitch: 1f, pitchJitter: 0.1f);
        }
    }
}
```

切换 BGM（淡入淡出各 1.5 秒，共 3 秒）：

```csharp
AudioModule.PlayBgm(sceneB, fadeSeconds: 1.5f);
```

设置菜单中的音量滑条（设置即生效、即持久化）：

```csharp
AudioModule.MasterVolume = masterSlider.value;
AudioModule.BgmVolume = bgmSlider.value;
AudioModule.SfxVolume = sfxSlider.value;
```

## 内部机制

- **音源布局**：BGM 专用 loop 源 1 个 + SFX 独占源 N 个，全部为 AudioModule 同物体的 `AudioSource` 组件，Awake 懒创建（编辑器与测试环境由首次 API 调用兜底创建）。`playOnAwake` 恒为 false。
- **SFX 轮询**：每次播放取下一个独占源（游标循环），源在播时直接前进——全忙即抢占最旧（轮询序即分配序）。每源记录局部音量，通道音量更新时按 `通道 × 总 × 局部` 重算，不冲掉 `volume` 参数。
- **淡入淡出**：`_bgmFadeFactor`（0-1）独立于音量链，协程只修改该系数——淡变期间调整音量无写冲突；连续切歌时新协程从当前系数续接，无跳变。淡出与淡入各占 `fadeSeconds`；BGM 未在播放时（首次播放 / 停止后重播）跳过淡出段直接从 0 淡入，不空等淡出时长。淡变基于 unscaled 时间，`PauseAll` 暂停期间淡变继续推进，恢复后音量自洽。
- **持久化**：键为 `<prefsKey>.MasterVolume` 等 6 个；写键不调用 `PlayerPrefs.Save()`（Unity 在退出时自动保存）。

## 设计边界（极简取舍）

| 不做 | 理由 |
|------|------|
| 3D 空间音效 | 原生 `AudioSource.PlayClipAtPoint` 一行已覆盖；需要衰减请自建音源 |
| AudioMixer 集成 | 快慢门/Snapshot/DSP 需资产管线，破坏零配置；音量直接写入音源 |
| 每音效独立 Stop / 播完回调 | `PlaySfx` 为 fire-and-forget；回调需求请使用 MiniEvent |
| 每秒上百次的高密度 SFX 池调优 | 8 源轮询已满足绝大多数场景；更高密度建议直接评估原生方案 |
| AudioListener 管理 | 调用方保证场景有恰好一个 Listener（相机默认自带） |
| 负 `pitchJitter` 防御 | 参数约定非负，误用不设防（极简原则） |

## 示例

包内 `Samples/Audio/01_BasicUsage/`（Package Manager → Samples 导入副本位于 `Samples~/Audio/01_BasicUsage`）：

**Audio Module - Basic Usage** — OnGUI 面板驱动全部 API：SFX 播放（含音调抖动）、BGM 立即播放与 1.5 秒淡入淡出切换、淡出停止、暂停恢复、三通道音量滑条与静音开关、`CurrentBgm` 状态显示。音频资源为程序化生成的自包含 wav。
