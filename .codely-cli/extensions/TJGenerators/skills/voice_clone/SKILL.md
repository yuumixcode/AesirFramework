---
name: unity-voice-clone
description: Clone a voice from an audio sample in Unity, producing a custom_voice_id that can be used with generate_tts to synthesize speech in the cloned voice. Use this skill whenever the user wants to clone a voice, create a custom voice profile, or replicate someone's voice for TTS — e.g. "克隆声音", "复制语音", "clone this voice", "make a custom voice", "用这个音频做语音合成". Trigger proactively for any voice cloning or custom voice creation request in Unity. Do NOT use for generating TTS directly (use generate_tts) or generating BGM/SFX (use generate_audio_clip / generate_sound_effect).
---

> ⚠️ **执行约束**
> - **主 agent**：无 `execute_custom_tool` 权限，必须 `task(subagent_name="voice-cloner", ...)` 委托，不要 `activate_skill` 后自己调。
> - **子代理（本文档主要读者）**：有权限，按下方 `execute_custom_tool(...)` 示例执行。

> ℹ️ **`place_assets_in_scene` 不适用**
> - 本 skill 的产出是 `custom_voice_id`（字符串），不是场景资产文件。
> - **不调用** `place_assets_in_scene`。
> - 如需用克隆的语音生成 TTS 音频，在报告中提供 `custom_voice_id`，由 caller 或 `audio-generator` agent 后续调用 `generate_tts(voice_id=custom_voice_id)`。

# Clone Voice in Unity 🎙️

从音频样本克隆语音，生成 `custom_voice_id`，可传给 `generate_tts` 的 `voice_id` 参数以用克隆的语音合成语音。

Output: `custom_voice_id`（字符串），不产生场景资产文件。

## 🚦 执行流程（不要跳读外链）

> ⚠️ **本 skill 无 placeholder、无场景资产**——产出是 voice ID 字符串。

1. 调 `voice_clone`（`audio_path` = 本地音频路径）→ 拿 `task_id`
2. **END RESPONSE TURN** — 不要 poll、不要 `query_voice_clone_status`、不要继续操作
3. 下一轮收到 `<bg_task_done>` → 读 `custom_voice_id` → 报告给 caller

**档位**：短任务 10–60 秒；120 秒内无通知才允许 `query_voice_clone_status` 一次。完整 async 规则见 [generator-async-pattern](../../experience/templates/generator-async-pattern.md)。

## ⚠️ Skill 独有约束

1. **`audio_path` 是本地路径**——C# host 自动上传到 CDN 并提交给后端，与 `generate_image` 的 `image_path` 模式一致。
2. **无 placeholder、无场景资产**——产出是 `custom_voice_id` 字符串，不需要 `place_assets_in_scene`。
3. **音频要求**：时长 10 秒–5 分钟，格式 mp3/wav/m4a，大小 < 20MB。
4. **`custom_voice_id` 用法**——传给 `generate_tts` 的 `voice_id` 参数，即可用克隆的语音合成任意文本的语音。
5. **并发上限 5**——同时运行的 voice_clone 任务最多 5 个。
6. **voice ID 持久性**——`custom_voice_id` 在当前会话内有效，可用于多次 `generate_tts` 调用。

## When to Use / NOT to Use

适用：克隆特定人声用于 TTS、创建自定义语音角色、复制 NPC 语音风格。

不适用：
- 直接生成 TTS 语音 → `generate_tts`（已有默认语音）
- 生成背景音乐 → `generate_audio_clip`
- 生成音效 → `generate_sound_effect`
- 编辑/混音现有音频 → 不支持

## 工具

所有工具通过 `execute_custom_tool` 调用。

### `voice_clone`

```python
execute_custom_tool(
  tool_name="voice_clone",
  parameters={
    "audio_path": "Assets/Audio/voice_sample.mp3"  # Required：本地音频路径
  }
)
```

### 返回字段

- `task_id`
- `audio_path`：回传输入的本地路径
- `notification_mode: "bg_task_done"`

提交失败时 `result["success"] == false`，读 `error_code` / `message`，**不要**poll。

### `<bg_task_done>` 独有字段

通用字段见模板。本 skill 额外字段：

| 字段 | 说明 |
|---|---|
| `custom_voice_id` | 克隆后的语音 ID — **传给 `generate_tts` 的 `voice_id` 参数** |

> 通知到达后，`custom_voice_id` 可立即用于 `generate_tts`。

### `query_voice_clone_status` / `list_voice_clone_tasks`

`query_voice_clone_status` 仅作 fallback（120 秒后单次）。返回字段同 `<bg_task_done>` payload。

`list_voice_clone_tasks` 返回当前 session 的所有 voice_clone 任务。

## 参数速查

| 参数 | 类型 | 默认 | 说明 |
|---|---|---|---|
| `audio_path` | string | **required** | 本地音频路径（C# host 自动上传） |

## 音频样本要求

| 维度 | 要求 |
|---|---|
| 时长 | 10 秒 – 5 分钟 |
| 格式 | mp3 / wav / m4a |
| 大小 | < 20 MB |
| 内容 | 单人清晰语音，背景噪音低 |
| 最佳时长 | 30 秒 – 2 分钟（太短克隆质量差，太长无额外收益） |

## 使用示例

### 克隆语音

```python
result = execute_custom_tool(
    tool_name="voice_clone",
    parameters={"audio_path": "Assets/Audio/npc_voice.mp3"}
)
if not result.get("success", True):
    raise RuntimeError(f"[{result['error_code']}] {result['message']}")

task_id = result["task_id"]
# ✅ END RESPONSE TURN — 等 bg_task_done
# 通知到达后读 custom_voice_id，报告给 caller
```

## 与 generate_tts 的关系

| 维度 | `voice_clone` (本 skill) | `generate_tts` |
|---|---|---|
| 输入 | 音频样本 (本地路径) | 文本 + voice_id |
| 输出 | `custom_voice_id` (字符串) | AudioClip (WAV/MP3) |
| 用途 | 创建自定义语音 | 用指定语音合成文本 |
| 场景资产 | ❌ 无 | ✅ AudioClip |
| 典型流程 | 先克隆 → 再 TTS | 接收 voice_id → 生成音频 |

> **典型工作流**：`voice_clone` → 拿 `custom_voice_id` → `generate_tts(voice_id=custom_voice_id)` → 拿 AudioClip → `place_assets_in_scene`。TTS 步骤由 caller 或 `audio-generator` agent 执行，不在本 skill 范围内。

## 故障排查

### Skill 独有问题

> 通用故障（配置缺失 / 任务卡住 / 状态异常 / 未登录）见 [generator-async-pattern §10](../../experience/templates/generator-async-pattern.md#10-通用故障排查)。

| 问题 | 原因 | 解决 |
|---|---|---|
| 克隆质量差 | 音频太短 / 背景噪音大 | 使用 30s+ 清晰单人语音；减少背景噪音 |
| `custom_voice_id` 为空 | 克隆失败 | 检查音频格式（mp3/wav/m4a）和大小（<20MB） |
| TTS 用克隆语音效果不好 | 克隆样本质量差 / 文本语言不匹配 | 提供更清晰的克隆样本；确保文本语言与样本语言一致 |

### Domain reload 后 task 丢失

通用恢复流程见 [generator-async-pattern §6](../../experience/templates/generator-async-pattern.md#6-domain-reload-recovery)。本 skill 产出是 voice ID（非文件），无法通过文件大小判断。

- 使用 `query_voice_clone_status(task_id=...)` 查询任务状态
- 若任务已丢失，需重新提交

---

**Task ID Format**：`voice_clone_{counter}_{timestamp}`

**Notes**：
- 产出 `custom_voice_id` 不是文件，不产生场景资产
- `custom_voice_id` 在当前会话内有效，可用于多次 `generate_tts`
- 自动应用 `TuanjieAI` 标签
- **并发上限 5**
- 需 Unity Editor 在线运行；消耗 AI 服务额度
- 基于 fal.ai minimax/voice-clone
