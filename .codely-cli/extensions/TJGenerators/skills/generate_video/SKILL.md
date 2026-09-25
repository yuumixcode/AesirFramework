---
name: unity-video-generation
description: "Generate video assets in Unity using AI from text prompts or reference media. Use this skill whenever the user wants to create a video asset — for example concept videos, motion graphics, animated content, visual effects, transition videos (转场视频), promotional videos (宣传视频), intro/outro sequences, cutscenes, or background videos — even if they just say \"帮我生成一个视频\", \"生成一个动画视频\", \"生成转场视频\", \"生成宣传视频\", \"make me a video\", \"generate a video clip\", \"create a transition\", \"make a promotional video\", \"generate video from scene\". Supports text-to-video, image-to-video and multimodal (reference video/images/audio). Two-phase MCP flow: generation runs on the MCP backend (generate_video + file_upload + check_task), landing runs in Unity via the import_video_from_url custom tool."
---

> **执行前读取**：[共享执行约定](../../experience/templates/generator-async-pattern.md)。
> 主 agent 按需委托 `general-purpose`；已在子代理中则直接执行本 skill，不再委托。已提交的阶段从原任务续接。

# Generate Video Asset in Unity 🎬 (MCP two-phase)

Generate video assets (VideoClip MP4) in Unity, from text prompts or reference media.
Output: MP4 imported as **VideoClip**, saved to `Assets/TJGenerators/History/ImportVideo/`（或指定 output_path）。

**两段式流程**：
- **第一段·生成（MCP 后端）**：`generate_video` 提交（本地参考图/视频/音频先 `file_upload` 拿 CDN URL）→ 后台轮询 → `check_task` 拿**视频 CDN URL**（永久链接）
- **第二段·导入 + 后处理（Unity CustomTool）**：`import_video_from_url` 下载 → 空白 MP4 占位原地覆盖（GUID 稳定）→ VideoClip 导入 + TuanjieAI/Session 标签 + History 记录 → 本次等待结束后 `query_local_task` 单次拉取结果

## 🚦 执行五步

1. 调 MCP `generate_video`（`mode` **必填**；`prompt` 文生视频必填）→ 拿 `task_id`，按共享约定执行返回的轮询命令
2. 轮询结束 → `check_task` 一次 → 视频 URL 在 `output.data.result.video_url`（.mp4）；`return_last_frame=true` 时另有 `output.data.result.last_frame_url`；`output.data.meta` 含时长/分辨率/seed 元数据
3. `execute_custom_tool("import_video_from_url", {video_url, prompt, ...})` → 拿 `task_id` + `placeholder_path`
4. （可选）把占位 VideoClip 先挂到 VideoPlayer；**在当前子代理内前台等待（每次最多30秒）（estimated_wait_seconds≈60s）** → **在当前子代理内等待，不结束任务**
5. 本次等待结束 → `query_local_task` **一次** → 读 `video_path`；仍是 `importing` 则再进行一次前台短等待。**不要干等 Unity 侧 `<bg_task_done>` 推送**，按共享约定等待后再查询

**档位**：第一段生成 1-5 分钟（按服务端返回的间隔等待）；第二段导入实测秒级。

## 场景输入与播放器接线

用户要求基于当前场景生成视频时，先截取场景输入并完成本地参考图上传，再提交 MCP；确需运行一帧时按共享约定处理 Play Mode，截图后恢复原状态。已有参考图或纯文本不需要运行场景。

需要播放时复用指定 VideoPlayer 对象，按用途设置：背景视频 `isLooping=true`、`playOnAwake=true`；一次性/脚本触发片段默认两者为 false；UI 使用 RenderTexture + RawImage；场景表面使用 RenderTexture + 对应材质。遵从用户的触发与宽高比要求，不自行修改摄像机。报告 VideoPlayer 的对象名、循环/自动播放、RenderMode 和 AspectRatio。

## 旧参数 → MCP 参数映射（原 Unity generate_video）

| 旧参数 | MCP `generate_video` | 迁移注意 |
|---|---|---|
| `generator_id: huoshan_seedance` | `provider: seedance2` | 另有 `provider: minimax_h3`（中文 prompt / 2K / 60s 需求时用） |
| `model` | `model`（同名：`doubao-seedance-2-0-mini-260615` 默认 / `doubao-seedance-2-0-260128` / `doubao-seedance-2-0-fast-260128`；`doubao-seedance-2-5-260628` 高成本仅在用户点名时用） | 新增 `MiniMax-H3` / `MiniMax-H3-Max` |
| `mode`（旧：可省略自动检测） | `mode`（**Required，无自动检测**） | 见下方判定表 |
| `image_path`（本地单图） | `file_upload` → `image_urls` | 本地文件必须先传 CDN |
| `video_path`（本地参考视频，旧链自动传 TOS） | `file_upload` → `video_urls`，且 prompt 中用 `@Video N` 引用 | MCP 用 file_upload 三步上传 |
| `reference_images`（本地图数组） | `file_upload` → `image_urls`（逗号分隔多个） | — |
| `audio_paths`（本地音频数组） | `file_upload` → `audio_urls` | Seedance 2.0 音频必须搭配图/视频 |
| `resolution`（480p/720p） | 同名（seedance 480p/720p；MiniMax 768P/2K，注意大小写） | 枚举混装 provider 专属值，按 provider 传对大小写 |
| `ratio` | 同名（16:9 等 7 值 + adaptive） | — |
| `duration`（**旧默认 12**） | `duration`（**默认 5**） | 不传时长会拿 5 秒视频！4-15（2.0）/ 4-30（2.5）/ 4-60（H3） |
| `return_last_frame`（**旧默认 true**） | 同名（**默认 false**） | 需要末帧续接工作流时显式传 `true` |
| `generate_audio`（默认 true） | 同名（seedance2 专属；MiniMax 恒有原生音频） | — |
| `output_path` | 传给第二段 `import_video_from_url` 的 `output_path` | — |

**`mode` 判定表（必填）**：

| 输入 | mode |
|---|---|
| 仅文本 | `text_to_video`（禁止任何媒体） |
| 1 张首帧图 | `first_frame`（仅 image_urls 1 张） |
| 2 张首末帧图 | `first_last_frame`（仅 image_urls 2 张） |
| 多图风格/角色/材质参考 | `reference_image`（2.0: 1-9 张 / 2.5: 1-30 张；禁止视频音频） |
| 有参考视频/音频（+可选图） | `multimodal`（Seedance 必须含视频或音频；prompt 用 `@Video N` / `@Image N` / `@Audio N` 引用） |

## ⚠️ Skill 独有约束

1. **Seedance prompt 必须英文**——中文需求先翻译成英文再提交，或改用 `minimax_h3`（中英皆可）。
2. **`mode` 必填**——MCP 不做旧链的自动检测；按上表显式传。
3. **积分确认**——高成本档（Seedance 2.5 / 长时长 / H3 Max）预估达阈值会返回确认提示：展示设置与预估积分，取得用户同意后才 `confirm_cost=true` 重提；模型/时长请求本身不等于批准扣费。
4. **扩展名**——`result.video_url` 为 .mp4，可直接导入；若出现其它扩展名，报告格式不兼容，不自动重生成。
5. **本工程验证**——结果是资产文件：`query_local_task` 确认 completed 后验证 VideoClip 可加载、元数据有效，不要用截图验证。

## 第二段 · `import_video_from_url`（execute_custom_tool）

```python
execute_custom_tool(
  tool_name="import_video_from_url",
  parameters={
    "video_url": "https://.../result.mp4",   # check_task 拿到的永久 CDN URL
    "prompt": "...",                          # 可选，History 显示
    "output_path": "Assets/TJGenerators/History/MyVideo.mp4",  # 可选，默认 History/ImportVideo/<url-hash>.mp4
    "session_id": "..."
  })
```

返回：`task_id`、`placeholder_path`（空白 MP4，GUID 稳定）、`estimated_wait_seconds`(60)。目标文件已存在时不写占位（防覆盖），下载后原位覆盖。

`query_local_task`：每次等待结束调用**一次**；完成返回 `video_path`。**不要无间隔连续查询。**

## 故障处理

| 症状 | 原因 | 处理 |
|---|---|---|
| MCP 提交报 InvalidParameter | `mode` 缺失/与媒体数不符 | 按判定表改参数重提 |
| `check_task` 仍 pending 超预估 | 视频生成本就 1-5 分钟 | 对同一 task_id 再 check_task 一次，勿重复提交 |
| CLI 报 "[Error: Could not parse tool response]" | CLI 吞掉服务端 JSON-RPC 错误 | 见服务端原始响应定位（内容安全驳回不建任务不扣费） |
| `import_video_from_url` 报扩展名不支持 | URL 非 .mp4 | 报告格式不兼容，不自动重生成 |
| query 返回 `recovering` | 域重载中断 | 再进行一次前台短等待等自动恢复（URL 永久、幂等重跑） |
| 导入失败 | 下载/写入错误 | 看 `error` 字段；URL 永久，可用同 URL 重试（原位覆盖） |
| 通知未到达 | 通知可能未送达当前执行者 | 按共享执行约定处理——有间隔等待 + 状态查询 是标准完成模式 |
