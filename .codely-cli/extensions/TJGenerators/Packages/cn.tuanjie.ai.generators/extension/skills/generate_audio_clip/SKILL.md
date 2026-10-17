---
name: unity-audio-clip-generation
description: "Generate background music (BGM) and ambient audio clips in Unity using AI via text descriptions. Use this skill when the user wants background music, BGM, soundtrack, ambient sound, or looping audio for a Unity scene — e.g. \"给场景加个背景音乐\", \"生成背景音乐\", \"make some music for my game\", or \"create an audio clip\". DO NOT use for sound effects (SFX) such as gunshots, footsteps, UI clicks, explosions — use the generate_sound_effect skill instead. Two-phase MCP flow: generation runs on the MCP backend (generate_music + check_task), landing runs in Unity via the import_audio_from_url custom tool (is_bgm=true auto-creates/reuses a BGMPlayer)."
---

> **执行前读取**：[共享执行约定](../../experience/templates/generator-async-pattern.md)。
> 主 agent 按需委托 `general-purpose`；已在子代理中则直接执行本 skill，不再委托。已提交的阶段从原任务续接。

# Generate BGM / Ambient Audio in Unity 🎵 (MCP two-phase)

Generate background music / ambient clips as **AudioClip**（WAV/MP3）保存到 `Assets/TJGenerators/History/ImportAudio/`。

**两段式流程**：
- **第一段·生成（MCP 后端）**：`generate_music`（prompt 建议含 "bgm"/"background music"）→ 后台轮询 → `check_task` 拿 **`output.data.audioUrl`**
- **第二段·导入 + 后处理（Unity CustomTool）**：`import_audio_from_url`（**is_bgm=true**）→ 静音 WAV 占位 → 下载原位覆盖 → **场景无引用时自动创建/复用 BGMPlayer（loop + 2D + playOnAwake）**，已有 AudioSource 引用占位则自动重绑真实 clip → History 记录 → 本次等待结束后 `query_local_task` 单次拉取

## 🚦 执行五步

1. 调 MCP `generate_music`：`prompt`（中英皆可，写明风格/情绪/场景/乐器；提示加 "bgm"）+ `duration_seconds`（游戏 BGM 推荐 15-30s；循环 BGM 推荐 30-60s；1-180）+ `output_format`（**只选 `wav` 或 `mp3`**——.aac/.flac 不可导入 Unity）→ 拿 `task_id`，按共享约定执行返回的轮询命令
2. 轮询结束 → `check_task` 一次 → 音频 URL 在 `output.data.audioUrl`（另含实测 `duration`/`fileSize`）
3. `execute_custom_tool("import_audio_from_url", {audio_url, is_bgm: true, prompt, ...})` → 拿 `task_id` + `placeholder_path`（静音 WAV，可先挂 AudioSource 预接线，完成自动重绑）
4. **在当前子代理内前台等待（每次最多30秒）（estimated_wait_seconds≈30s）** → **在当前子代理内等待，不结束任务**
5. 本次等待结束 → `query_local_task` **一次** → 读 `audio_path` + `created_bgm_player`；仍 `importing` 则再进行一次前台短等待。**不要干等 Unity 推送，按共享约定等待后再查询**

**档位**：生成 30-120 秒（按时长计费）；导入实测秒级。

## ⚠️ Skill 独有约束

1. **`output_format` 只选 `wav`/`mp3`**——Unity 不可导入 .aac/.flac/.m4a；check_task 返回其它扩展名时报告格式不兼容，不自动重生成。
2. **按秒计费 + 成本确认**——`duration_seconds` 影响积分；省略时按 90s 估算。预估达阈值会返回确认提示：展示设置与预估积分，取得用户**明确同意**后才 `confirm_cost=true` 重提；时长请求本身不等于批准扣费。
3. **BGMPlayer 行为**——`is_bgm=true` 且场景中没有 AudioSource 引用该 clip 时自动创建 `BGMPlayer`（loop + spatialBlend=0 + playOnAwake）；已有引用则只重绑不新建。不要在场景里手工再建一份。
4. **本工程验证**——`query_local_task` 返回 `audio_path` 即完成；可用 `unity_asset(action="get_info")` 确认，不要用截图验证。

## 旧参数 → MCP 参数映射（原 Unity generate_audio_clip）

| 旧参数 | MCP `generate_music` | 备注 |
|---|---|---|
| `generator_id: sonilo-music` | 无（MCP 固定 provider） | — |
| `prompt` | 同名 | 中英皆可 |
| `duration_seconds` | 同名 | 计费相关 |
| `output_format`（旧默认 wav） | 同名 | 只选 wav / mp3 |
| `play_on_awake` | 传给第二段 `import_audio_from_url` | is_bgm 时默认 true |
| `output_path` | 传给第二段 `import_audio_from_url` | 默认 History/ImportAudio/<url-hash>.<ext> |

## 第二段 · `import_audio_from_url`（execute_custom_tool）

```python
execute_custom_tool(
  tool_name="import_audio_from_url",
  parameters={
    "audio_url": "https://.../result.wav",   # check_task 的 output.data.audioUrl
    "is_bgm": True,                            # BGM/环境音必传；音效勿传
    "prompt": "calm forest bgm",               # 可选，History 显示
    "output_path": "Assets/TJGenerators/History/MyBgm.wav",  # 可选
    "session_id": "..."
  })
```

返回：`task_id`、`placeholder_path`、`output_path`、`estimated_wait_seconds`(30)。完成字段：`audio_path`、`updated_audio_sources`、`created_bgm_player`。**不要无间隔连续查询。**

## 故障处理

| 症状 | 原因 | 处理 |
|---|---|---|
| `check_task` 仍 pending 超预估 | 音乐生成 30-120s 正常 | 对同一 task_id 再 check_task 一次，勿重复提交 |
| 返回确认提示（费用） | 时长 × 单价达阈值 | 展示预估积分，用户同意后 confirm_cost=true 重提 |
| `import_audio_from_url` 报扩展名不支持 | .aac/.flac/.m4a | 报告格式不兼容；新提交应指定 wav/mp3，不自动重生成 |
| query 返回 `recovering` | 域重载中断 | 再进行一次前台短等待等自动恢复（幂等重跑） |
| 通知未到达 | 通知可能未送达当前执行者 | 按共享执行约定处理——有间隔等待 + 状态查询 是标准完成模式 |
