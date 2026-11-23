# 音频模块

音频模块提供 2D 音频极简门面:`AudioModule` 单例的公开 API 全为静态成员,零配置调用即用 —— SFX 独占音源轮询、BGM 淡入淡出、三通道音量 / 静音持久化。

## 核心能力

- **SFX** —— 固定数量独占音源轮询(默认 8,可配):等效池化,无每播实例化开销;每次播放的局部音量与音调独立生效;源全忙时按轮询序抢占最旧;`pitchJitter` 音调随机抖动防止机械感(最终音调钳制到 [0.01, 3],任意 pitch 与 jitter 组合不会反播)
- **BGM** —— 专用循环音源:同曲在播且无进行中淡变时幂等返回(跨场景重复触发不打断音乐;淡出进行中重播同曲会取消淡出并续接);切换支持协程淡入淡出(基于 `unscaledDeltaTime`,slow motion 不变调)
- **音量与静音** —— Master / BGM / SFX 三通道乘法链 + 三通道静音(Master 总闸),设置即时生效并经 PlayerPrefs 持久化(重启自动恢复,键前缀可配);音量 setter 每次赋值即落键,连续拖动的滑条请在拖动结束时一次性写入
- **暂停** —— `PauseAll` / `ResumeAll` 一对,适合暂停菜单与切后台

## 快速开始

零配置即可使用 —— 无预放置、无配置资产时使用代码默认值(全音量、持久化开启):

```csharp
using Runestone.AesirModules;

// SFX:一击即走,音调 ±0.1 随机抖动
AudioModule.PlaySfx(clickClip, pitchJitter: 0.1f);

// BGM:立即播放;同曲在播时幂等返回
AudioModule.PlayBgm(bgmClip);

// 切歌:淡出旧曲 1.5 秒 → 淡入新曲 1.5 秒
AudioModule.PlayBgm(sceneB, fadeSeconds: 1.5f);

// 音量:设置即生效、即持久化(setter 每次赋值即落键,连续拖动请在拖动结束一次性写入)
AudioModule.SfxVolume = 0.5f;
AudioModule.MasterMute = true;
```

未预放置时单例自动挂载到 `[Aesir Modules]` 宿主下(跟随宿主 DDOL 决策);预放置实例可在 Inspector 配置 SFX 音源数量与可选配置资产。

## API 速查

| API | 说明 |
|-----|------|
| `PlaySfx(clip, volume, pitch, pitchJitter)` | 播放音效(fire-and-forget;最终音调钳制 [0.01, 3]) |
| `PlayBgm(clip, fadeSeconds)` | 播放 BGM;同曲在播且无淡变进行中时幂等返回(淡出中重播同曲取消淡出续接) |
| `StopBgm(fadeSeconds)` | 淡出停止(`CurrentBgm` 保留) |
| `PauseAll()` / `ResumeAll()` | 暂停 / 恢复全部音源 |
| `MasterVolume` / `BgmVolume` / `SfxVolume` | 三通道音量(0-1,乘法链:通道 × 总) |
| `MasterMute` / `BgmMute` / `SfxMute` | 三通道静音开关 |
| `CurrentBgm` / `IsBgmPlaying` | 当前 BGM 状态查询 |
| `Channels` / `AudioChannel` | 通道维度单一数据源(BGM / SFX);新增通道只需在通道集合登记一项 |
| `ApplyConfig(config)` | 运行时替换配置资产并重新载入 |

## 配置资产(AudioConfigSO)

可选的默认行为定制资产,创建路径 `Assets → Create → Aesir Modules → Audio → AudioConfig`:

| 字段 | 默认值 | 说明 |
|------|--------|------|
| `masterVolume` / `bgmVolume` / `sfxVolume` | 1 | 三通道默认音量(0-1) |
| `persistVolumes` | `true` | 音量 / 静音是否经 PlayerPrefs 持久化 |
| `prefsKey` | `"AesirAudio"` | PlayerPrefs 键前缀(实际键如 `AesirAudio.MasterVolume`) |

> 持久化值优先于配置默认值:玩家调整过的音量(已存键)在初始化时覆盖 SO 默认值;`persistVolumes` 关闭时读写均不发生。

## 设计边界

| 不做 | 替代方案 |
|------|---------|
| 3D 空间音效 | 原生 `AudioSource.PlayClipAtPoint` |
| AudioMixer 集成 | 音量直接写入音源;Snapshot / DSP 需资产管线时自建 |
| 每音效独立 Stop / 播完回调 | `PlaySfx` 为 fire-and-forget;回调需求用 [MiniEvent](../architecture/observable.md) |
| AudioListener 管理 | 调用方保证场景恰好一个 Listener(相机默认自带) |
| NaN `fadeSeconds` 防御 | 参数约定合法数值;NaN 会污染淡变系数使音量链全 NaN,误用不设防 |
| 轮询抢占的瞬态爆音 | 全忙抢占最旧音源热切换 clip 有可闻爆音;在意瞬态噪声请降低触发密度或自建音源做包络 |

## 示例

`Audio/01_BasicUsage` —— OnGUI 面板驱动全部 API:SFX 播放(含音调抖动)、BGM 立即播放与 1.5 秒淡入淡出切换、淡出停止、暂停恢复、三通道音量滑条(演示拖动结束落键)与静音开关、`CurrentBgm` 状态显示。

## 继续阅读

- [特性一览](features.md) —— 五模块速览
- [响应式与事件](../architecture/observable.md) —— 播完回调等事件需求用 MiniEvent
