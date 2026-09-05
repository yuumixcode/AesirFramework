---
name: unity-audio-clip-generation
description: Generate background music (BGM) and ambient audio clips in Unity using AI via text descriptions. Use this skill when the user wants background music, BGM, soundtrack, ambient sound, or looping audio for a Unity scene — e.g. "给场景加个背景音乐", "生成背景音乐", "make some music for my game", or "create an audio clip". DO NOT use for sound effects (SFX) such as gunshots, footsteps, UI clicks, explosions — use the `generate_sound_effect` skill instead.
---

> ⚠️ **执行约束**
> - **主 agent**：无 `execute_custom_tool` 权限，必须 `task(subagent_name="audio-generator", ...)` 委托，不要 `activate_skill` 后自己调。
> - **子代理（本文档主要读者）**：有权限，按下方 `execute_custom_tool(...)` 示例执行。

> ⛔ **`place_assets_in_scene` 调用规则**
> - **调用方式**：`activate_skill("unity-place-assets-in-scene")` → 按 §4c AudioClip BGM 模板用 `execute_csharp_script` 配置 `AudioSource`（**不是** `execute_custom_tool`）。
> - **子代理**：提交后**立即调一次**把占位 WAV 配到 AudioSource；收到 `<bg_task_done>` 后**不再调**（AudioSource.clip 自动指向真实音频）。
> - **主 agent**：报告里的 `audio_path` 是"已放置"的证据，不是"请你放置"的指示，**不要再调**。
> - **例外**：用户明确要"换 AudioSource / 再加一个实例"时才再次调用。详见 [async-pattern §5.1](../../experience/templates/generator-async-pattern.md#51-place_assets_in_scene-调用规则)。

# Generate Audio Clip (BGM / Ambient Music) in Unity 🎵

Generate **background music and ambient audio** assets in Unity using Sonilo AI, from text descriptions of music style, mood, or scene.
Output: WAV file auto-imported as **AudioClip**, saved到 `Assets/TJGenerators/History/`。

> ⚠️ **仅 BGM 和环境音乐。** 一次性音效（枪声、脚步、UI 点击、爆炸等）用 `generate_sound_effect` skill。

## 🚦 执行四步（不要跳读外链）

1. 调 `generate_audio_clip` → 拿 `task_id` + `placeholder_path`（无声 WAV）
2. 立即 `place_assets_in_scene`（资产类型 `AudioClip BGM`，路径用 `placeholder_path`）→ 创建 `BGMPlayer` + AudioSource（loop=true, spatialBlend=0）
3. **END RESPONSE TURN** — 不要 poll、不要 `query_audio_clip_status`、不要继续操作
4. 下一轮收到 `<bg_task_done>` → 读 `audio_path`（WAV 已原地覆盖，AudioSource.clip 自动指向真实音乐，**不要再 place**）

**档位**：短任务 60–180 秒；120 秒内无通知才允许 `query_audio_clip_status` 一次。完整 async 规则见 [generator-async-pattern](../../experience/templates/generator-async-pattern.md)。

## ⚠️ Skill 独有约束

1. **`prompt` required**——必须描述音乐风格/情绪/乐器/场景。支持中文和英文。
2. **`duration_seconds` 范围 1–180**（float，秒）——超出会被拒绝；默认 90。
3. **资产类型用 `AudioClip BGM` 不是 `AudioClip SFX`**——`place_assets_in_scene` 会按 BGM 类型配置 AudioSource：`loop=true`、`spatialBlend=0`（2D，无空间衰减）。
4. **`play_on_awake` 直接控 AudioSource**——这个参数不影响生成本身，而是控制 `place_assets_in_scene` 创建 AudioSource 时是否自动播放（默认 `true`）。需要脚本触发时设 `false`。

## When to Use / NOT to Use

适用：BGM、环境氛围音乐、loop 背景音轨、场景音乐主题、菜单音乐、boss 战音乐。

不适用：
- 一次性音效（枪声/脚步/UI 点击/爆炸/拾取） → `generate_sound_effect`
- 编辑/混音现有音频文件（本工具只生成新音频）
- 配音 / TTS（不在本 skill 范围）

## 工具

所有工具通过 `execute_custom_tool` 调用。

### `generate_audio_clip`

```python
execute_custom_tool(
  tool_name="generate_audio_clip",
  parameters={
    "prompt": "epic orchestral battle music, intense drums, rising tension",  # Required
    "generator_id": "sonilo-music",      # 唯一可用 generator（默认）
    "duration_seconds": 90,               # 1–180 秒，默认 90
    "output_format": "wav",              # wav|mp3，默认 wav
    "play_on_awake": True,               # 创建的 AudioSource 是否自动播放，默认 true
    # output_path: 不建议指定，默认 Assets/TJGenerators/History/
  }
)
```

> ⚠️ **不要随便指定 `output_path`**——使用项目中尚未存在的目录（如 `Assets/Audio/BGM`）会导致资产无法被 Unity 正确导入。默认路径是标准位置。

### 返回字段

- `task_id`
- `placeholder_path`：占位 WAV（无声），**立即可用**——交给 `place_assets_in_scene`
- `estimated_wait_seconds` ≈ 90
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

### `query_audio_clip_status` / `list_audio_clip_tasks`

`query_audio_clip_status` 仅作 fallback（120 秒后单次）。返回字段同 `<bg_task_done>` payload，外加 `placeholder_path`（仅 `generating` 时）。

`list_audio_clip_tasks` 返回当前 session 的所有 audio_clip 任务。

## 参数速查

| 参数 | 类型 | 默认 | 说明 |
|---|---|---|---|
| `generator_id` | string | `"sonilo-music"` | 唯一可用 |
| `prompt` | string | **required** | 风格/情绪/乐器/场景描述（支持中文和英文） |
| `duration_seconds` | float | `90` | 输出长度秒数（**1–180**） |
| `output_format` | string | `"wav"` | `wav` / `mp3` |
| `play_on_awake` | bool | `true` | 创建的 AudioSource 是否在 Play Mode 自动播放 |
| `output_path` | string | — | 自定义路径（**不建议指定**） |

## 使用示例

### 生成 BGM

```python
result = execute_custom_tool(
    tool_name="generate_audio_clip",
    parameters={
        "prompt": "calm ambient fantasy RPG town music, gentle flute and strings, peaceful atmosphere",
        "duration_seconds": 60
    }
)
if not result.get("success", True):
    raise RuntimeError(f"[{result['error_code']}] {result['message']}")

task_id = result["task_id"]
placeholder_path = result["placeholder_path"]

# ✅ 立即用 place_assets_in_scene 把 placeholder_path 应用为 AudioClip BGM
# 然后 END RESPONSE TURN，等 bg_task_done 通知
```

### 脚本触发的 BGM（不自动播放）

```python
parameters={
    "prompt": "tense boss fight music, heavy brass, fast percussion",
    "duration_seconds": 90,
    "play_on_awake": False        # 脚本里手动 audioSource.Play() 触发
}
```

## 放入场景

资产类型 **`AudioClip BGM`**（不是 `AudioClip SFX`、不是 `AudioClip`），路径用 `placeholder_path` / `audio_path`。

Scene side-effect：创建 `BGMPlayer` GameObject + AudioSource，`loop=true`、`spatialBlend=0`、`playOnAwake=<生成参数 play_on_awake>`；不需要绑定到具体 GameObject（BGM 是 2D 全局音）。

规则（占位/原地覆盖/只调一次）见 [async-pattern §5 / §5.1](../../experience/templates/generator-async-pattern.md#5-placeholder-工作流适用于会返回-placeholder_path--prefab_output_path-的工具)。

## Prompt 写作指南

像给作曲家写 brief 一样，描述**风格 / 情绪 / 乐器 / 场景**。

| 场景 | Prompt 示例 |
|---|---|
| RPG 城镇 | `"peaceful medieval town music, acoustic guitar, flute, warm and welcoming"` |
| Boss 战 | `"intense orchestral battle, heavy brass, fast percussion, rising tension"` |
| 探索 | `"ambient open world exploration, soft synth pads, gentle piano, sense of wonder"` |
| 恐怖 | `"unsettling horror ambiance, dissonant strings, low rumbles, eerie silence"` |
| 胜利 | `"triumphant fanfare, full orchestra, brass stabs, heroic and celebratory"` |
| 科幻 | `"futuristic electronic ambient, pulsing synths, glitchy textures, space atmosphere"` |
| 休闲手游 | `"upbeat cheerful casual game music, ukulele, xylophone, light and fun"` |

技巧：
- **流派**：orchestral / electronic / jazz / folk / metal
- **情绪**：peaceful / tense / mysterious / heroic / melancholic
- **乐器**：piano / guitar / strings / synth / drums
- **能量**：calm / slow build / driving / intense
- **用途**：background loop / intro jingle / boss fight / menu theme

## 故障排查

### Skill 独有问题

> 通用故障（配置缺失 / 任务卡住 / 状态异常 / 未登录）见 [generator-async-pattern §10](../../experience/templates/generator-async-pattern.md#10-通用故障排查)。

| 问题 | 原因 | 解决 |
|---|---|---|
| 音乐与 prompt 不符 | prompt 太模糊 / 描述矛盾 | 写更具体的乐器 + 情绪；避免矛盾描述 |
| 想快速试错 prompt | duration 默认 90 太长 | 临时设 `duration_seconds: 30` 加快迭代 |
| AudioClip 在 Unity 没显示 | output_path 目录不存在 | 不要自定义 `output_path`，用默认路径 |

### Domain reload 后 task 丢失

通用恢复流程见 [generator-async-pattern §6](../../experience/templates/generator-async-pattern.md#6-domain-reload-recovery)。本 skill 完成态阈值（覆盖通用 `<1KB` 占位规则）：

- WAV < 10 KB → 仍是 placeholder（无声占位）
- WAV ≥ 1 MB → 真实音乐已就绪（60 秒 WAV 通常几 MB）

可用 `glob("Assets/TJGenerators/History/*.wav")` + 文件大小恢复。

---

**Task ID Format**：`audio_{counter}_{timestamp}`

**Notes**：
- 输出 WAV 自动导入为 `AudioClip`
- 自动应用 `TuanjieAI` 标签
- 需 Unity Editor 在线运行；消耗 AI 服务额度
