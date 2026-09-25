---
name: unity-sound-effect-generation
description: "Generate sound effects (SFX) in Unity using AI via text descriptions. Use this skill when the user wants one-shot sound effects such as gunshots, footsteps, explosions, UI clicks, item pickups, environmental sounds, etc. — e.g. \"生成枪声音效\", \"给按钮加个点击音效\", \"make a footstep sound\", \"generate explosion SFX\". DO NOT use for background music or looping ambient audio — use the generate_audio_clip skill instead. Two-phase MCP flow: generation runs on the MCP backend (generate_sound_effect + check_task), landing runs in Unity via the import_audio_from_url custom tool."
---

> **执行前读取**：[共享执行约定](../../experience/templates/generator-async-pattern.md)。
> 主 agent 按需委托 `general-purpose`；已在子代理中则直接执行本 skill，不再委托。已提交的阶段从原任务续接。

# Generate Sound Effects (SFX) in Unity 🔊 (MCP two-phase)

Generate one-shot sound effects as **AudioClip**（WAV/MP3）保存到 `Assets/TJGenerators/History/ImportAudio/`。

**两段式流程**：
- **第一段·生成（MCP 后端）**：`generate_sound_effect`（prompt 建议含 "sound effect"/"SFX"）→ 后台轮询 → `check_task` 拿 **`output.data.audioUrl`**
- **第二段·导入（Unity CustomTool）**：`import_audio_from_url`（is_bgm **不传**或 false）→ 下载导入 → History 记录 → 本次等待结束后 `query_local_task` 单次拉取

## 🚦 执行五步

1. 调 MCP `generate_sound_effect`：`prompt`（中英皆可；写清楚声源、动作、环境、质感，如 "short wooden door creak, single squeak, dry interior"）+ `duration_seconds`（1-180，默认 8；一次性音效通常 1-10s）+ `output_format`（**只选 `wav` 或 `mp3`**）→ 拿 `task_id`，按共享约定执行返回的轮询命令
2. 轮询结束 → `check_task` 一次 → 音频 URL 在 `output.data.audioUrl`
3. `execute_custom_tool("import_audio_from_url", {audio_url, prompt, ...})` → 拿 `task_id` + `placeholder_path`（静音 WAV，可先挂 AudioSource 预接线，完成自动重绑）
4. **在当前子代理内前台等待（每次最多30秒）（estimated_wait_seconds≈30s）** → **在当前子代理内等待，不结束任务**
5. 本次等待结束 → `query_local_task` **一次** → 读 `audio_path`；仍 `importing` 则再进行一次前台短等待。**不要干等 Unity 推送，按共享约定等待后再查询**

**档位**：生成 10-60 秒；导入实测秒级。

## ⚠️ Skill 独有约束

1. **`output_format` 只选 `wav`/`mp3`**——Unity 不可导入 .aac/.flac/.m4a；返回其它扩展名时报告格式不兼容，不自动重生成。
2. **is_bgm 勿传 true**——音效不是 BGM；传了会触发 BGMPlayer 自动创建。
3. **按秒计费 + 成本确认**——预估达阈值会返回确认提示，取得用户明确同意后 `confirm_cost=true` 重提。
4. **AudioSource 设置**——一次性音效 `loop=false`，默认 `playOnAwake=false`；空间声源用 3D，UI/旁白用 2D。不明确是否背景循环时按 SFX 处理，显式背景音乐走 BGM skill。
5. **放置**——音效 clip 需要挂到事件触发的 AudioSource（ gunfire 挂角色、UI click 挂按钮）；先接 `placeholder_path`，完成自动重绑。

## 旧参数 → MCP 参数映射（原 Unity generate_sound_effect）

| 旧参数 | MCP `generate_sound_effect` | 备注 |
|---|---|---|
| `generator_id: sonilo-sfx` | 无（MCP 固定 provider） | — |
| `prompt` | 同名 | 中英皆可 |
| `duration_seconds` | 同名 | 默认 8 |
| `output_format` | 同名 | 只选 wav / mp3 |
| `play_on_awake` | 传给第二段 `import_audio_from_url` | 音效默认 false |
| `output_path` | 传给第二段 `import_audio_from_url` | 默认 History/ImportAudio/<url-hash>.<ext> |

## 第二段 · `import_audio_from_url`（execute_custom_tool）

```python
execute_custom_tool(
  tool_name="import_audio_from_url",
  parameters={
    "audio_url": "https://.../result.wav",   # check_task 的 output.data.audioUrl
    "prompt": "short wooden door creak",     # 可选，History 显示
    "output_path": "Assets/TJGenerators/History/MySfx.wav",  # 可选
    "session_id": "..."
  })
```

返回：`task_id`、`placeholder_path`、`estimated_wait_seconds`(30)。完成字段：`audio_path`、`updated_audio_sources`。**不要无间隔连续查询。**

## 故障处理

| 症状 | 原因 | 处理 |
|---|---|---|
| `check_task` 仍 pending 超预估 | SFX 生成 10-60s 正常 | 对同一 task_id 再 check_task 一次，勿重复提交 |
| `import_audio_from_url` 报扩展名不支持 | .aac/.flac/.m4a | 报告格式不兼容；新提交应指定 wav/mp3，不自动重生成 |
| query 返回 `recovering` | 域重载中断 | 再进行一次前台短等待等自动恢复（幂等重跑） |
| 通知未到达 | 通知可能未送达当前执行者 | 按共享执行约定处理——有间隔等待 + 状态查询 是标准完成模式 |
