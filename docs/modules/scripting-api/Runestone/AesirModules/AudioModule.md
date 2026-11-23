---
title: AudioModule
description: "Runestone.AesirModules.AudioModule 的 API 文档"
---

# `AudioModule`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.Component` → `UnityEngine.Behaviour` → `UnityEngine.MonoBehaviour` → `Sirenix.OdinInspector.SerializedMonoBehaviour` → `Runestone.AesirArchitecture.AesirMonoBehaviour` → `AudioModule`

**实现接口:** `Sirenix.Serialization.ISupportsPrefabSerialization`，`UnityEngine.ISerializationCallbackReceiver`

## 声明

``` csharp
[DisallowMultipleComponent]
[DefaultExecutionOrder]
public class AudioModule : Runestone.AesirArchitecture.AesirMonoBehaviour, 
Sirenix.Serialization.ISupportsPrefabSerialization, 
UnityEngine.ISerializationCallbackReceiver
```

音频管理器（MonoBehaviour 单例）—— 2D 音频极简门面。 负责 SFX 轮询播放、BGM 循环与淡入淡出、三通道音量/静音控制与持久化。
公开 API 全部为静态成员，经 Instance 单例转发。

**备注**

是否加入 DontDestroyOnLoad 场景由序列化字段 dontDestroyOnLoad 控制， 仅在本物体为根物体（场景预放置）时生效；运行时自动创建的实例挂载在 AesirModules 宿主下， 实际是否 DDOL 跟随宿主的 dontDestroyOnLoad 决策。
SFX 采用固定数量独占音源轮询（等效池化：无每播实例化开销，源全忙时按轮询序抢占最旧）； BGM 采用专用循环音源，切换支持协程淡入淡出（基于 Time.unscaledDeltaTime，不受 timeScale 影响）。

设计边界（极简取舍）：仅负责 2D 音频——3D 空间音效请使用原生 AudioSource.PlayClipAtPoint 自建音源；不集成 AudioMixer（音量直接写入音源）； PlaySfx 为 fire-and-forget，不提供单个音效的停止与播完回调（回调机制请使用 MiniEvent）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AudioModule()`](#constructor-audiomodule) | — |

</div>

### AudioModule() {#constructor-audiomodule}

``` csharp
public AudioModule()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Bgm`](#field-bgm) | — |
| [`Sfx`](#field-sfx) | — |
| [`Channels`](#field-channels) | 全部音频通道（只读视图）。通道在此集中登记一次，音量/静音的存取、 PlayerPrefs 持久化与配置默认值载入均遍历本列表。 |

</div>

### Bgm {#field-bgm}

``` csharp
public static readonly AudioChannel Bgm = Runestone.AesirModules.AudioChannel;
```

### Sfx {#field-sfx}

``` csharp
public static readonly AudioChannel Sfx = Runestone.AesirModules.AudioChannel;
```

### Channels {#field-channels}

全部音频通道（只读视图）。通道在此集中登记一次，音量/静音的存取、 PlayerPrefs 持久化与配置默认值载入均遍历本列表。

**备注**

总音量与总静音是跨通道的总闸，不属于"通道"，仍由独立字段承载 （MasterVolume / MasterMute）。 新增通道时在 Index 与本列表中同步登记同一位置。

``` csharp
public static readonly IReadOnlyList<AudioChannel> Channels;
```

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`CurrentBgm`](#property-currentbgm) | 当前背景音乐片段。停止播放后仍保留最后一次播放的片段，未播放过时为 null。 |
| [`Instance`](#property-instance) | 全局单例入口。 优先在已加载场景中查找预放置的实例；未找到时在 AesirModules（DDOL）下创建子物体。 |
| [`BgmMute`](#property-bgmmute) | 背景音乐静音开关，与总静音相或生效。设置即时生效，并按配置持久化。 |
| [`IsBgmPlaying`](#property-isbgmplaying) | 背景音乐是否正在播放。 |
| [`MasterMute`](#property-mastermute) | 总静音开关（总闸，与各通道静音相或生效）。设置即时生效，并按配置持久化。 |
| [`SfxMute`](#property-sfxmute) | 音效静音开关，与总静音相或生效。设置即时生效，并按配置持久化。 |
| [`BgmVolume`](#property-bgmvolume) | 背景音乐通道音量（0-1），与总音量相乘生效。设置即时生效，并按配置持久化。 |
| [`MasterVolume`](#property-mastervolume) | 总音量（0-1），与各通道音量相乘生效。设置即时生效，并按配置持久化到 PlayerPrefs。 |
| [`SfxVolume`](#property-sfxvolume) | 音效通道音量（0-1），与总音量相乘生效。设置即时生效，并按配置持久化。 |

</div>

**继承的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `destroyCancellationToken` | — | `MonoBehaviour` |
| `gameObject` | — | `Component` |
| `hideFlags` | — | `Object` |
| `transform` | — | `Component` |
| `enabled` | — | `Behaviour` |
| `isActiveAndEnabled` | — | `Behaviour` |
| `runInEditMode` | — | `MonoBehaviour` |
| `useGUILayout` | — | `MonoBehaviour` |
| `name` | — | `Object` |
| `tag` | — | `Component` |
| `animation` | — | `Component` |
| `audio` | — | `Component` |
| `camera` | — | `Component` |
| `collider` | — | `Component` |
| `collider2D` | — | `Component` |
| `constantForce` | — | `Component` |
| `hingeJoint` | — | `Component` |
| `light` | — | `Component` |
| `networkView` | — | `Component` |
| `particleSystem` | — | `Component` |
| `renderer` | — | `Component` |
| `rigidbody` | — | `Component` |
| `rigidbody2D` | — | `Component` |

</div>

### CurrentBgm {#property-currentbgm}

当前背景音乐片段。停止播放后仍保留最后一次播放的片段，未播放过时为 null。

**备注**

只读查询，不创建模块实例：模块不存在（从未播放过任何音频）时返回 null。

``` csharp
public static AudioClip CurrentBgm { get; }
```

### Instance {#property-instance}

全局单例入口。 优先在已加载场景中查找预放置的实例；未找到时在 AesirModules（DDOL）下创建子物体。

``` csharp
public static AudioModule Instance { get; }
```

### BgmMute {#property-bgmmute}

背景音乐静音开关，与总静音相或生效。设置即时生效，并按配置持久化。

**备注**

读取会确保模块实例存在（取值语义见 MasterVolume）。

``` csharp
public static bool BgmMute { get; set; }
```

### IsBgmPlaying {#property-isbgmplaying}

背景音乐是否正在播放。

**备注**

只读查询，不创建模块实例：模块不存在或尚未初始化时返回 false （没有音频源，等价于"没在播放"）。

``` csharp
public static bool IsBgmPlaying { get; }
```

### MasterMute {#property-mastermute}

总静音开关（总闸，与各通道静音相或生效）。设置即时生效，并按配置持久化。

**备注**

读取会确保模块实例存在（取值语义见 MasterVolume）。

``` csharp
public static bool MasterMute { get; set; }
```

### SfxMute {#property-sfxmute}

音效静音开关，与总静音相或生效。设置即时生效，并按配置持久化。

**备注**

读取会确保模块实例存在（取值语义见 MasterVolume）。

``` csharp
public static bool SfxMute { get; set; }
```

### BgmVolume {#property-bgmvolume}

背景音乐通道音量（0-1），与总音量相乘生效。设置即时生效，并按配置持久化。

**备注**

读取会确保模块实例存在（取值语义见 MasterVolume）。

``` csharp
public static float BgmVolume { get; set; } = 1f;
```

### MasterVolume {#property-mastervolume}

总音量（0-1），与各通道音量相乘生效。设置即时生效，并按配置持久化到 PlayerPrefs。

**备注**

读取会确保模块实例存在（与 IsBgmPlaying / CurrentBgm 不同）：音量与静音的"有效值" 由配置资产决定（AudioConfigSO 的默认音量 / 持久化开关 / PlayerPrefs 键前缀都挂在模块实例的序列化字段上）， 无实例时无法还原该配置——非创建式取值只可能返回代码默认值，与项目配置矛盾（静默给出错误音量）。 故本组（音量与静音）保持创建式取值，只读播放状态则走非创建式。

``` csharp
public static float MasterVolume { get; set; } = 1f;
```

### SfxVolume {#property-sfxvolume}

音效通道音量（0-1），与总音量相乘生效。设置即时生效，并按配置持久化。

**备注**

读取会确保模块实例存在（取值语义见 MasterVolume）。

``` csharp
public static float SfxVolume { get; set; } = 1f;
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ApplyConfig(AudioConfigSO)`](#method-applyconfig-audioconfigso) | 替换运行时配置并重新载入音量（持久化值优先于配置默认值）。 传入 null 恢复为代码默认配置。预放置实例调用会改写序列化的资产引用。 副作用声明：本方法会先将三通道音量与静音全部重置为配置默认值（无配置时为代码默认值）， 再读持久化键——persistVolumes=false 或持久化键缺失时， 运行中经 API 设置的音量/静音状态会被重置，请在意图明确的配置切换时机调用。 |
| [`PauseAll()`](#method-pauseall) | 暂停全部音频（BGM 与所有 SFX 音源）。适合暂停菜单与切后台（配合 OnApplicationPause）。 |
| [`PlayBgm(AudioClip, float)`](#method-playbgm-audioclip-float) | 播放背景音乐（循环）。同曲稳定播放（无进行中的淡变）时幂等返回——跨场景重复触发不打断音乐。 淡变进行中调用同曲：取消淡变并从当前系数续接淡回全音量（不重新播放）—— 覆盖 StopBgm 淡出途中反悔、切歌淡出途中回到旧曲两个场景。 |
| [`PlaySfx(AudioClip, float, float, float)`](#method-playsfx-audioclip-float-float-float) | 播放一次音效（fire-and-forget）。每次播放轮询取下一个独占音源， 局部音量与音调独立于其他正在播放的音效。 |
| [`ResumeAll()`](#method-resumeall) | 恢复全部音频，与 PauseAll 成对使用；未暂停的音源调用无副作用。 |
| [`StopBgm(float)`](#method-stopbgm-float) | 停止背景音乐。停止后 CurrentBgm 保留最后一次播放的片段（不清除）。 |

</div>

**继承的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetComponent(Type)` | — | `Component` |
| `GetComponent(string)` | — | `Component` |
| `GetComponentInChildren(Type)` | — | `Component` |
| `GetComponentInChildren(Type, bool)` | — | `Component` |
| `GetComponentInParent(Type)` | — | `Component` |
| `GetComponentInParent(Type, bool)` | — | `Component` |
| `GetComponents(Type)` | — | `Component` |
| `GetComponentsInChildren(Type)` | — | `Component` |
| `GetComponentsInChildren(Type, bool)` | — | `Component` |
| `GetComponentsInParent(Type)` | — | `Component` |
| `GetComponentsInParent(Type, bool)` | — | `Component` |
| `StartCoroutine(IEnumerator)` | — | `MonoBehaviour` |
| `StartCoroutine(string)` | — | `MonoBehaviour` |
| `StartCoroutine(string, object)` | — | `MonoBehaviour` |
| `GetComponent()` | — | `Component` |
| `GetComponentInChildren()` | — | `Component` |
| `GetComponentInChildren(bool)` | — | `Component` |
| `GetComponentInParent()` | — | `Component` |
| `GetComponentInParent(bool)` | — | `Component` |
| `GetComponents()` | — | `Component` |
| `GetComponentsInChildren()` | — | `Component` |
| `GetComponentsInChildren(bool)` | — | `Component` |
| `GetComponentsInParent()` | — | `Component` |
| `GetComponentsInParent(bool)` | — | `Component` |
| `GetType()` | — | `object` |
| `CompareTag(string)` | — | `Component` |
| `IsInvoking()` | — | `MonoBehaviour` |
| `IsInvoking(string)` | — | `MonoBehaviour` |
| `TryGetComponent(Type, ref Component)` | — | `Component` |
| `TryGetComponent(ref T)` | — | `Component` |
| `GetComponentIndex()` | — | `Component` |
| `GetInstanceID()` | — | `Object` |
| `Equals(object)` | — | `Object` |
| `GetHashCode()` | — | `Object` |
| `ToString()` | — | `Object` |
| `BroadcastMessage(string)` | — | `Component` |
| `BroadcastMessage(string, SendMessageOptions)` | — | `Component` |
| `BroadcastMessage(string, object)` | — | `Component` |
| `BroadcastMessage(string, object, SendMessageOptions)` | — | `Component` |
| `CancelInvoke()` | — | `MonoBehaviour` |
| `CancelInvoke(string)` | — | `MonoBehaviour` |
| `GetComponents(Type, List<Component>)` | — | `Component` |
| `GetComponents(List<T>)` | — | `Component` |
| `GetComponentsInChildren(List<T>)` | — | `Component` |
| `GetComponentsInChildren(bool, List<T>)` | — | `Component` |
| `GetComponentsInParent(bool, List<T>)` | — | `Component` |
| `Invoke(string, float)` | — | `MonoBehaviour` |
| `InvokeRepeating(string, float, float)` | — | `MonoBehaviour` |
| `SendMessage(string)` | — | `Component` |
| `SendMessage(string, SendMessageOptions)` | — | `Component` |
| `SendMessage(string, object)` | — | `Component` |
| `SendMessage(string, object, SendMessageOptions)` | — | `Component` |
| `SendMessageUpwards(string)` | — | `Component` |
| `SendMessageUpwards(string, SendMessageOptions)` | — | `Component` |
| `SendMessageUpwards(string, object)` | — | `Component` |
| `SendMessageUpwards(string, object, SendMessageOptions)` | — | `Component` |
| `StopAllCoroutines()` | — | `MonoBehaviour` |
| `StopCoroutine(Coroutine)` | — | `MonoBehaviour` |
| `StopCoroutine(IEnumerator)` | — | `MonoBehaviour` |
| `StopCoroutine(string)` | — | `MonoBehaviour` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
| `OnAfterDeserialize()` | — | `SerializedMonoBehaviour` |
| `OnBeforeSerialize()` | — | `SerializedMonoBehaviour` |
| `StartCoroutine_Auto(IEnumerator)` | — | `MonoBehaviour` |

</div>

### ApplyConfig(AudioConfigSO) {#method-applyconfig-audioconfigso}

替换运行时配置并重新载入音量（持久化值优先于配置默认值）。 传入 null 恢复为代码默认配置。预放置实例调用会改写序列化的资产引用。
副作用声明：本方法会先将三通道音量与静音全部重置为配置默认值（无配置时为代码默认值）， 再读持久化键——persistVolumes=false 或持久化键缺失时， 运行中经 API 设置的音量/静音状态会被重置，请在意图明确的配置切换时机调用。

``` csharp
public static void ApplyConfig(AudioConfigSO config)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `config` | `AudioConfigSO` | 新配置资产（可为 null）。 |

</div>

### PauseAll() {#method-pauseall}

暂停全部音频（BGM 与所有 SFX 音源）。适合暂停菜单与切后台（配合 OnApplicationPause）。

``` csharp
public static void PauseAll()
```

### PlayBgm(AudioClip, float) {#method-playbgm-audioclip-float}

播放背景音乐（循环）。同曲稳定播放（无进行中的淡变）时幂等返回——跨场景重复触发不打断音乐。
淡变进行中调用同曲：取消淡变并从当前系数续接淡回全音量（不重新播放）—— 覆盖 StopBgm 淡出途中反悔、切歌淡出途中回到旧曲两个场景。

``` csharp
public static void PlayBgm(AudioClip clip, float fadeSeconds = 0f)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `clip` | `AudioClip` | 音频片段。 |
| `fadeSeconds` | `float` | 淡变时长（秒）。0 表示立即切换；大于 0 时旧曲先在此时长内淡出，随后新曲在同一时长内淡入。 |

</div>

### PlaySfx(AudioClip, float, float, float) {#method-playsfx-audioclip-float-float-float}

播放一次音效（fire-and-forget）。每次播放轮询取下一个独占音源， 局部音量与音调独立于其他正在播放的音效。

``` csharp
public static void PlaySfx(AudioClip clip, float volume = 1f, float pitch = 1f, float pitchJitter = 0f)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `clip` | `AudioClip` | 音频片段。 |
| `volume` | `float` | 本次播放的局部音量（0-1），与 SFX 通道音量、总音量相乘生效。 |
| `pitch` | `float` | 本次播放的基准音调。 |
| `pitchJitter` | `float` | 音调随机抖动幅度（非负）：最终音调在 [pitch - jitter, pitch + jitter] 内随机并钳制到 [0.01, 3]（不会反播），用于脚步/射击等防止机械感。 |

</div>

### ResumeAll() {#method-resumeall}

恢复全部音频，与 PauseAll 成对使用；未暂停的音源调用无副作用。

``` csharp
public static void ResumeAll()
```

### StopBgm(float) {#method-stopbgm-float}

停止背景音乐。停止后 CurrentBgm 保留最后一次播放的片段（不清除）。

``` csharp
public static void StopBgm(float fadeSeconds = 0f)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `fadeSeconds` | `float` | 淡出时长（秒）。0 表示立即停止。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
