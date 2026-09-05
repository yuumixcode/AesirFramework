---
name: unity-sound-effect-generation
description: Generate sound effects (SFX) in Unity using AI via text descriptions. Use this skill when the user wants one-shot sound effects such as gunshots, footsteps, explosions, UI clicks, item pickups, environmental sounds, etc. — e.g. "生成枪声音效", "给按钮加个点击音效", "make a footstep sound", "generate explosion SFX". DO NOT use for background music or looping ambient audio — use the `generate_audio_clip` skill instead.
---

> ⚠️ **执行约束**
> - **主 agent**：无 `execute_custom_tool` 权限，必须 `task(subagent_name="audio-generator", ...)` 委托，不要 `activate_skill` 后自己调。
> - **子代理（本文档主要读者）**：有权限，按下方 `execute_custom_tool(...)` 示例执行。

> ⛔ **`place_assets_in_scene` 调用规则**
> - **调用方式**：`activate_skill("unity-place-assets-in-scene")` → 按 §4d AudioClip SFX 模板用 `execute_csharp_script` 配置 `AudioSource`（**不是** `execute_custom_tool`）。
> - **子代理**：提交后**立即调一次**把占位音频配到 AudioSource；收到 `<bg_task_done>` 后**不再调**（AudioSource.clip 自动指向真实音频）。
> - **主 agent**：报告里的 `audio_path` 是"已放置"的证据，不是"请你放置"的指示，**不要再调**。
> - **例外**：用户明确要"换 AudioSource / 再加一个实例"时才再次调用。详见 [async-pattern §5.1](../../experience/templates/generator-async-pattern.md#51-place_assets_in_scene-调用规则)。

# Generate Sound Effect (SFX) in Unity 💥

Generate **one-shot sound effect** assets in Unity using AI, from text descriptions of the sound.
Output: 默认 WAV 文件，自动导入为 **AudioClip**，保存到 `Assets/TJGenerators/History/`。

> ⚠️ **仅一次性 SFX。** BGM、环境音乐、循环音轨用 `generate_audio_clip` skill。

## 🚦 执行四步（不要跳读外链）

1. 调 `generate_sound_effect` → 拿 `task_id` + `placeholder_path`（无声 MP3）
2. 立即 `place_assets_in_scene`（资产类型 `AudioClip SFX`，路径用 `placeholder_path`）→ AudioSource（loop=false, spatialBlend=1）
3. **END RESPONSE TURN** — 不要 poll、不要 `query_sound_effect_status`、不要继续操作
4. 下一轮收到 `<bg_task_done>` → 读 `audio_path`（已原地覆盖，AudioSource.clip 自动指向，**不要再 place**）

**档位**：短任务 10–60 秒；120 秒内无通知才允许 `query_sound_effect_status` 一次。完整 async 规则见 [generator-async-pattern](../../experience/templates/generator-async-pattern.md)。

## ⚠️ Skill 独有约束

1. **`prompt` 支持中文和英文**——Sonilo 音效生成 API 支持中英文描述。
   - ✅ `"sharp wooden door knock, three rapid knocks"`
   - ✅ `"木门敲击声"`
2. **`duration_seconds` 范围 1–180**（**float**，不是 int）——超出会被拒绝。默认 8。
3. **资产类型用 `AudioClip SFX` 不是 `AudioClip BGM`**——`place_assets_in_scene` 会按 SFX 类型配置 AudioSource：`loop=false`、`spatialBlend=1`（**3D，有空间衰减**）。
4. **`play_on_awake` 默认 false**——SFX 一般由脚本触发，不自动播放。环境氛围才设 `true`。

## 与 BGM 的差异速查

| 维度 | BGM (`generate_audio_clip`) | SFX (本 skill) |
|---|---|---|
| 用途 | 背景音乐、loop 音轨 | 一次性音效（枪声、点击、爆炸） |
| 输出格式 | WAV | **WAV**（可选 MP3） |
| Duration | 1–180 秒 (float, 默认 90) | **1–180 秒 (float, 默认 8)** |
| Prompt 语言 | 中文 / 英文 | **中文 / 英文** |
| AudioSource Loop | true | **false** |
| AudioSource SpatialBlend | 0 (2D) | **1 (3D)** |
| `play_on_awake` 默认 | true | **false** |

## When to Use / NOT to Use

适用：枪声、爆炸、脚步、UI 点击、拾取音、开门、咒语、引擎、雨声 burst、跳跃落地等一次性音效；可循环的环境音（雨/风/引擎 hum）也走这里。

不适用：
- BGM / 主题曲 / 长音乐 → `generate_audio_clip`
- 编辑/混音现有音频（本工具只生成新音频）
- 配音 / TTS（不在本 skill 范围）

## 工具

所有工具通过 `execute_custom_tool` 调用。

### `generate_sound_effect`

```python
execute_custom_tool(
  tool_name="generate_sound_effect",
  parameters={
    "prompt": "powerful explosion with debris, low rumble and sharp crack",  # Required, Chinese or English
    "duration_seconds": 8,          # 1–180 秒（float），默认 8
    "output_format": "wav",         # wav|mp3，默认 wav
    "play_on_awake": False,         # AudioSource 是否自动播放，默认 false
    # output_path: 不建议指定，默认 Assets/TJGenerators/History/
  }
)
```

### 返回字段

- `task_id`
- `placeholder_path`：占位 MP3（无声），**立即可用**——交给 `place_assets_in_scene`
- `estimated_wait_seconds` ≈ 30
- `notification_mode: "bg_task_done"`

提交失败时 `result["success"] == false`，读 `error_code` / `message`，**不要**poll。

### `<bg_task_done>` 独有字段

通用字段见模板。本 skill 额外字段：

| 字段 | 说明 |
|---|---|
| `audio_path` | 最终 AudioClip 资产路径（== `placeholder_path`，原地覆盖） |
| `preview_url` | 音频预览 URL 或本地路径 |
| `generator_id` | 使用的生成器 |
| `prompt` | 原始 prompt |

### `query_sound_effect_status` / `list_sound_effect_tasks`

`query_sound_effect_status` 仅作 fallback（120 秒后单次）。返回字段同 `<bg_task_done>` payload，外加 `placeholder_path`（仅 `generating` 时）。

`list_sound_effect_tasks` 返回当前 session 的所有 SFX 任务。

## 参数速查

| 参数 | 类型 | 默认 | 说明 |
|---|---|---|---|
| `prompt` | string | **required** | 中文或英文描述（source / action / material / acoustic context），max 500 字符 |
| `duration_seconds` | float | `8` | 1–180 秒。短：1–3；爆炸/碰撞：3–8；长环境：8–180 |
| `output_format` | string | `"wav"` | `wav` / `mp3` |
| `play_on_awake` | bool | `false` | 创建的 AudioSource 是否在 Play Mode 自动播放（环境音时才设 true） |
| `output_path` | string | — | 自定义路径（不建议指定） |

## 使用示例

### 简单 SFX

```python
result = execute_custom_tool(
    tool_name="generate_sound_effect",
    parameters={
        "prompt": "sharp wooden door knock, three rapid knocks",
        "duration_seconds": 2
    }
)
if not result.get("success", True):
    raise RuntimeError(f"[{result['error_code']}] {result['message']}")

task_id = result["task_id"]
placeholder_path = result["placeholder_path"]

# ✅ 立即用 place_assets_in_scene 把 placeholder_path 应用为 AudioClip SFX
# 然后 END RESPONSE TURN，等 bg_task_done 通知
```

### 环境氛围音

```python
parameters={
    "prompt": "steady rainfall on a window, medium intensity",
    "duration_seconds": 10,
    "play_on_awake": True,    # 环境音自动开始
}
# 注意：AudioSource 的 loop 仍由资产类型 AudioClip SFX 决定（默认 false）
# 若需要循环播放，应改用 AudioClip BGM 类型，或在脚本里手动设 audioSource.loop = true
```

### 高质量爆炸

```python
parameters={
    "prompt": "massive explosion, deep bass rumble, scattered debris falling",
    "duration_seconds": 5,
    "output_format": "wav"    # 无损 WAV
}
```

## 放入场景

资产类型 **`AudioClip SFX`**（不是 `AudioClip BGM`、不是 `AudioClip`），路径用 `placeholder_path` / `audio_path`。

Scene side-effect：创建 AudioSource，`loop=false`、`spatialBlend=1`（3D 空间音）、`playOnAwake=<生成参数 play_on_awake>`；通常需要绑定到具体 GameObject（枪、门、玩家）以获得 3D 空间感。

规则见 [async-pattern §5 / §5.1](../../experience/templates/generator-async-pattern.md#5-placeholder-工作流适用于会返回-placeholder_path--prefab_output_path-的工具)。

## Prompt 写作指南

描述**source / action / material / acoustic context** 四要素。

| 类型 | Prompt 示例 |
|---|---|
| 枪声 | `"single pistol gunshot, sharp crack, indoor reverb"` |
| 爆炸 | `"large explosion, deep bass shockwave, debris rattle"` |
| 脚步 | `"three heavy footsteps on wet gravel, slow pace"` |
| UI 点击 | `"soft UI button click, clean and satisfying, slight tap"` |
| 拾取 | `"bright coin pickup chime, cheerful ding, quick fade"` |
| 开门 | `"old wooden door creaking open slowly, haunting tone"` |
| 咒语 | `"mystical magic spell cast, whoosh with sparkle shimmer"` |
| 引擎 | `"car engine revving, V8, aggressive acceleration"` |
| 雨声 | `"sudden heavy rain burst on leaves, natural outdoor"` |
| 跳跃落地 | `"character jump with grunt and landing thud on grass"` |

技巧：
- **来源**：door / sword / gun / footstep / button
- **动作**：click / slam / crack / whoosh / creak
- **材质**：wood / metal / stone / glass / flesh
- **声学环境**：indoor reverb / outdoor echo / muffled / crisp
- **持续感**：short snap / long rumble / quick fade
- **一次只描述一种声音**——多个声源混在一起会让 AI 困惑

## 故障排查

### Skill 独有问题

> 通用故障（配置缺失 / 任务卡住 / 状态异常 / 未登录）见 [generator-async-pattern §10](../../experience/templates/generator-async-pattern.md#10-通用故障排查)。

| 问题 | 原因 | 解决 |
|---|---|---|
| 音效与 prompt 不符 | prompt 太模糊 / 多声源混淆 | 写更具体的来源 + 材质 + 动作；一次只描述一种声音 |
| 循环音播放有接缝 | Sonilo SFX 不支持 loop 参数 | 如需循环播放，在脚本里手动设 `audioSource.loop = true` |
| 环境 loop 不自动播放 | `play_on_awake` 默认 false | 设 `play_on_awake: True`；同时 AudioSource.loop 需要手动设 true（资产类型 SFX 默认不 loop） |
| 时长不对 | 默认 8 秒可能不匹配音效类型 | 短音效（点击）：1–3 秒；爆炸：3–8 秒；长环境：10–180 秒 |

### Domain reload 后 task 丢失

通用恢复流程见 [generator-async-pattern §6](../../experience/templates/generator-async-pattern.md#6-domain-reload-recovery)。本 skill 完成态阈值（覆盖通用 `<1KB` 占位规则）：

- WAV < 5 KB → 仍是 placeholder（无声占位）
- WAV ≥ 50 KB → 真实音效已就绪（8 秒 WAV 通常 50–500 KB）

可用 `glob("Assets/TJGenerators/History/*.wav")` + 文件大小恢复。

---

**Task ID Format**：`audio_{counter}_{timestamp}`（与 BGM 共用 audio 前缀）

**Notes**：
- 默认输出 WAV（`output_format=wav`）自动导入为 `AudioClip`；也可改为 `mp3`。
- 自动应用 `TuanjieAI` 标签
- 需 Unity Editor 在线运行；消耗 AI 服务额度
