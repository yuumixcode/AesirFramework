---
name: aesir-audio
description: Aesir Modules 音频模块（AudioModule）编码指南。加音效与背景音乐时使用——PlaySfx 一击即走、PlayBgm 循环与淡入淡出切歌、Master/Bgm/Sfx 三通道音量与静音持久化、PauseAll/ResumeAll、AudioConfigSO。触发词：AudioModule、PlaySfx、PlayBgm、StopBgm、背景音乐、BGM、音效、音量、静音、淡入淡出、加个点击音效、切场景换背景音乐、做设置里的音量滑条。
---

# Aesir 音频模块（AudioModule）

2D 音频极简门面：**SFX 轮询独占音源** + **BGM 专用循环源与淡变** + **三通道音量/静音持久化**。公开 API 全为静态成员，`AudioModule.xxx` 调用即用（首次调用自动创建，挂在 `[Aesir Modules]` 宿主下，零预放置零配置）。

> 前置：本包依赖 Aesir Architecture（RAA）。本模块只做 2D 音频，3D 空间音效走原生 `AudioSource.PlayClipAtPoint`。
>
> **缺包先过依赖门禁**：工程里没有 `cn.runestone.aesir.modules`（或连带缺 RAA）时，不要改用散落的 `AudioSource.PlayOneShot` + 自建音量静态类顶替——先按 `aesir-router` 的「依赖门禁」探测 → 读记忆 → 询问安装（Git URL，CNB 优先、GitHub 兜底）。

## 默认路径

```csharp
using Runestone.AesirModules;

[SerializeField] AudioClip clickSound;
[SerializeField] AudioClip levelBgm;

void Start()
{
    AudioModule.PlayBgm(levelBgm);                                  // 循环、跨场景持续
}

void OnClick()
{
    AudioModule.PlaySfx(clickSound, volume: 1f, pitch: 1f, pitchJitter: 0.1f); // 一击即走
}

void SwitchStage(AudioClip next)
{
    AudioModule.PlayBgm(next, fadeSeconds: 1.5f);                   // 淡出 1.5s + 淡入 1.5s
}
```

## API 速查

```csharp
// SFX（fire-and-forget）
AudioModule.PlaySfx(AudioClip clip, float volume = 1f, float pitch = 1f, float pitchJitter = 0f);

// BGM
AudioModule.PlayBgm(AudioClip clip, float fadeSeconds = 0f);   // 同曲在播：幂等返回，不打断
AudioModule.StopBgm(float fadeSeconds = 0f);                   // 停止后 CurrentBgm 仍保留
AudioModule.CurrentBgm { get; }
AudioModule.IsBgmPlaying { get; }

// 暂停（BGM 源 + 全部 SFX 源），适合暂停菜单 / OnApplicationPause
AudioModule.PauseAll();
AudioModule.ResumeAll();

// 音量 0–1，三通道乘法链：channel × master
AudioModule.MasterVolume / BgmVolume / SfxVolume;   // get / set
AudioModule.MasterMute / BgmMute / SfxMute;         // get / set，Master 是总闸
```

预放置实例可在 Inspector 调：`sfxSourceCount`（默认 8）、`config`（`AudioConfigSO`）、`dontDestroyOnLoad`。

## 内部机制要点（影响你怎么用）

- **SFX 轮询**：N 个独占 `AudioSource`（默认 8）游标轮转，**不用 `PlayOneShot`、不每次 Instantiate**；源全忙时抢占最旧（轮询序即分配序）。每次播放的 `volume` 与 `pitch` 独立记录——改通道音量时按 `通道 × 总 × 局部` 重算，不会冲掉单次音量。
- **淡变**：`_bgmFadeFactor` 独立于音量链，协程只改系数（基于 `unscaledDeltaTime`，slow motion 不变调）；淡变期间调音量无冲突。首次播放或停止后重播会**跳过淡出段**直接淡入，不空等。`PauseAll` 期间淡变继续推进，恢复后音量自洽。
- **音调钳制**：最终 pitch 钳到 `[0.01, 3]`，`pitch` 与 `pitchJitter` 任意组合都不会反播或静音。

## 配置（AudioConfigSO）

`Assets → Create → Aesir Modules → Audio → AudioConfig`：

| 字段 | 默认 | 说明 |
|---|---|---|
| `masterVolume` / `bgmVolume` / `sfxVolume` | 1 | 默认通道音量 |
| `persistVolumes` | true | 是否经 PlayerPrefs 持久化 |
| `prefsKey` | `"AesirAudio"` | 键前缀（实际键如 `AesirAudio.MasterVolume`） |

**持久化值优先于配置默认值**：玩家存过的键在初始化时覆盖 SO 默认；`persistVolumes=false` 时读写都不发生。运行时替换配置走 `AudioModule.ApplyConfig(config)`。

## 坑位

1. **音量 setter 每次赋值都写一次 PlayerPrefs**——连续拖动的滑条必须在**拖动结束时**一次性写入（uGUI 用 `onPointerUp` 时机），否则逐帧落键。写键不调 `PlayerPrefs.Save()`（Unity 退出时自动保存）。
2. **`StopBgm` 后 `CurrentBgm` 仍保留**（记录的是"最后一首"），判断是否在播要读 `IsBgmPlaying`。
3. **AudioListener 由调用方保证恰好一个**（相机默认自带）；本模块不管 Listener。
4. **全忙抢占最旧会有可闻爆音**（热切换 clip）——高密度触发场景要么降密度，要么自建音源做包络。
5. **`pitchJitter` 约定非负、`fadeSeconds` 约定合法数值**（NaN 会污染整条音量链），不设防御——按约定传参。

## 设计边界（不要尝试）

不做 3D 空间音效、不集成 AudioMixer（音量直接写音源，无 Snapshot/DSP）、不做每音效 Stop 与播完回调（`PlaySfx` 是 fire-and-forget，要回调就用 `MiniEvent`）、不做高密度 SFX 池调优。需要衰减曲线或 DSP 效果请直接自建 `AudioSource`。

## 详情与示例

- 完整机制与配置字段：包内 `Documentation~/audio-module.md`。
- 可运行示例：`Samples/Audio/01_BasicUsage`（OnGUI 面板驱动全部 API：音调抖动、BGM 淡变切换、暂停恢复、三通道滑条与静音、`CurrentBgm` 状态；音频为程序化生成的自包含 wav）。
