---
name: unity-effect-video-generation
description: "Generate effect videos (green-screen VFX) in Unity using AI from text descriptions. Use this skill whenever the user wants to create a VFX effect video — for example fire explosion effects, magic glow effects, smoke effects, particle effects, energy effects, lightning effects (特效视频, 绿幕特效, 火焰特效, 魔法特效, 烟雾特效, 粒子特效, 能量特效, 闪电特效) — even if they just say \"帮我生成一个特效\", \"生成特效视频\", \"生成绿幕特效\", \"make an effect video\", \"generate a VFX\", \"create a fire effect\". Two-phase MCP flow: the backend auto-chains image generation (with auto green-screen prompt) + green-screen video generation (generate_effect_video + check_task); Unity landing runs via import_video_from_url with chroma_key=true, which creates the ChromaKey material — use with VideoPlayer + RenderTexture for real-time transparent playback."
---

> **执行前读取**：[共享执行约定](../../experience/templates/generator-async-pattern.md)。
> 主 agent 按需委托 `general-purpose`；已在子代理中则直接执行本 skill，不再委托。已提交的阶段从原任务续接。

# Effect Video in Unity 🎆 (MCP two-phase)

后端两步自动串联：1) 生成绿幕底图（prompt **无需**写"绿幕"，服务端自动加）；2) 生成绿幕特效视频。
Unity 侧：3) `import_video_from_url`（chroma_key=true）下载导入并**自动创建 ChromaKey 材质**、布置特效播放器（VideoPlayer + RenderTexture 实时透明播放）。

**输出**：绿幕 MP4（VideoClip）+ ChromaKey 材质（`material_path`），保存在 `Assets/TJGenerators/History/ImportVideo/` 下。

## 🚦 执行五步

1. 调 MCP `generate_effect_video`（`prompt` 必填：VFX 描述，如 "fire explosion"；可选 `video_duration` 4-15 默认 5、`video_ratio` 默认 16:9、`video_resolution` 480p/720p 默认 720p、`quality`、`output_format`）→ 拿 `task_id`，按共享约定执行返回的轮询命令（全链路 1-3 分钟）
2. 轮询结束 → `check_task` 一次 → 视频 URL（绿幕 MP4，字段同视频流：`output.data.result.video_url`）
3. `execute_custom_tool("import_video_from_url", {video_url, chroma_key: true, prompt, ...})` → 拿 `task_id` + `placeholder_path`
4. **在当前子代理内前台等待（每次最多30秒）（estimated_wait_seconds≈60s）** → **在当前子代理内等待，不结束任务**
5. 本次等待结束 → `query_local_task` **一次** → 读 `video_path` + `material_path`；仍 `importing` 则再进行一次前台短等待。**不要干等 Unity 推送，按共享约定等待后再查询**

VFX 提示词用英文描述“主体 + 动作 + 风格”，不额外加绿幕背景。导入后验证 `material_path` 的 ChromaKey 材质和已创建播放器的 RenderTexture/Quad 接线，复用导入工具创建的对象；按用户用途设置循环和自动播放。报告视频、材质路径及接线结果。

## ⚠️ Skill 独有约束

1. **prompt 不写"绿幕/green screen"**——服务端自动注入绿幕背景；只描述特效本身。
2. **`chroma_key: true` 必传**——否则导入的是带绿底的普通视频，不会生成 ChromaKey 材质和特效播放器。
3. **积分确认**——生图+生视频两段成本高于单视频；预估达阈值会返回确认提示，取得用户同意后 `confirm_cost=true` 重提。
4. **实时透明播放**——导入完成后场景中已有布置好的特效播放器（VideoPlayer + RenderTexture + ChromaKey 材质）；Play Mode 下即可看到透明叠加效果。

## 第二段 · `import_video_from_url`（execute_custom_tool，chroma_key 模式）

```python
execute_custom_tool(
  tool_name="import_video_from_url",
  parameters={
    "video_url": "https://.../green_screen_effect.mp4",  # check_task 拿到的永久 CDN URL
    "chroma_key": True,                                   # 关键：走 ChromaKey 材质 + 特效播放器链路
    "prompt": "fire explosion",                           # 可选，History 显示
    "output_path": "Assets/TJGenerators/History/MyEffect.mp4",  # 可选
    "session_id": "..."
  })
```

返回：`task_id`、`placeholder_path`、`chroma_key: true`。完成字段：`video_path`（绿幕 MP4）+ `material_path`（ChromaKey 材质）。

## 故障处理

| 症状 | 原因 | 处理 |
|---|---|---|
| `check_task` 仍 pending 超预估 | 生图+生视频两段串行，1-3 分钟正常 | 对同一 task_id 再 check_task 一次，勿重复提交 |
| 完成但 `material_path` 为空 | ChromaKey 材质创建失败（非绿幕内容等） | 看 `error`/日志；确认视频确为绿幕内容后重试 |
| `import_video_from_url` 报扩展名不支持 | URL 非 .mp4 | 报告格式不兼容，不自动重生成 |
| query 返回 `recovering` | 域重载中断 | 再进行一次前台短等待等自动恢复（幂等重跑，chroma_key 状态随任务持久化） |
| 通知未到达 | 通知可能未送达当前执行者 | 按共享执行约定处理——有间隔等待 + 状态查询 是标准完成模式 |
