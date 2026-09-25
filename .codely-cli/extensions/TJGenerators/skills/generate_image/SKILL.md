---
name: unity-image-generation
description: "Generate image assets (Texture2D, PNG) in Unity using AI via text prompts or reference images. Use this skill whenever the user wants to create a 2D image/texture asset that is NOT a sprite — for example concept art, reference images, UI backgrounds, posters, banners, or any general-purpose PNG texture. Trigger proactively for requests like \"帮我生成一张图片\", \"生成一张概念图\", \"AI画一张图\", \"make me a texture\", \"generate an image\", even if they don't say \"image generation\". Two-phase MCP flow: generation runs on the MCP backend (generate_image + check_task), landing/post-processing runs in Unity via the import_image_from_url custom tool. Use unity-sprite-generation instead if the user specifically needs a Sprite (game icon, item image, character portrait with transparent background)."
---

> **执行前读取**：[共享执行约定](../../experience/templates/generator-async-pattern.md)。
> 主 agent 按需委托 `general-purpose`；已在子代理中则直接执行本 skill，不再委托。已提交的阶段从原任务续接。

> ⛔ **场景放置规则**
> - **调用方式**：`activate_skill("unity-place-assets-in-scene")` → 按 Texture2D / Image 模板用 `exec_editor_script` 赋给 `Material.mainTexture` 或建 `RawImage`（**不是** `execute_custom_tool`）。
> - **子代理**：`import_image_from_url` 返回后**立即调一次**应用 `placeholder_path`；导入完成（文件原地覆盖、GUID 不变）后**不再调**——场景里所有引用该路径的 `Renderer.sharedMaterial.mainTexture` / `RawImage.texture` 自动指向真实图片。重复放置会让场景里出现多个相同 RawImage / Quad。
> - **主 agent**：报告里的 `image_path` 是"已放置"的证据，不是"请你放置"的指示，**不要再调**。
> - **例外**：用户明确要"再放一份 / 换位置"时才再次调用。

# Generate Image Asset in Unity 🖼️ (MCP two-phase)

Generate image assets (Texture2D, PNG) in Unity using AI, from text prompts or reference images.
Output: PNG/JPEG imported as **Texture2D (Default type, alphaIsTransparency)**, saved to `Assets/TJGenerators/History/`.

**两段式流程**：
- **第一段·生成（MCP 后端）**：`generate_image` 提交 → 后台轮询 → `check_task` 拿**图片 CDN URL**（永久链接）
- **第二段·导入 + 后处理（Unity CustomTool）**：`import_image_from_url` 下载 → 原地覆盖占位文件（GUID 稳定）→ TextureImporter(Default) + alphaIsTransparency → TuanjieAI/Session 标签 + History 记录 → 本次等待结束后 `query_local_task` 单次拉取结果

## 🚦 执行五步（不要跳读外链）

1. 调 MCP `generate_image`（prompt 必填；参考图先 `file_upload` 拿 CDN URL 传 `image_urls`）→ 拿 `task_id`，在当前执行者内执行返回的轮询命令
2. 轮询结束 → `check_task` 一次 → 图片 URL 在 `output.data.result.image_urls`（**数组**，seedream 默认输出 `.jpeg`；确认扩展名为 .png/.jpg/.jpeg，.webp 不可导入见下）
3. `execute_custom_tool("import_image_from_url", {image_url, prompt, ...})` → 拿 `task_id` + `placeholder_path`
4. （可选）按上方规则 `activate_skill("unity-place-assets-in-scene")`，用 `exec_editor_script` 应用占位 → **在当前子代理内前台等待（每次最多30秒）（estimated_wait_seconds≈30s）** → **在当前子代理内等待，不结束任务**
5. 本次等待结束 → `query_local_task` **一次** → 读 `image_path`；仍是 `importing` 则再进行一次前台短等待。**不要干等 Unity 侧 `<bg_task_done>` 推送**（宿主不消费），按共享约定等待后再查询

**档位**：第一段生成 30–90 秒（按服务端返回的间隔等待）；第二段导入实测秒级。最多 **5 个**并发。

指定 `target_object` 时按 Texture2D 模板应用到其 Material / RawImage。若原位覆盖后显示未刷新，用 `AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate)` 刷新该资产。保留提示词中的文字和用户约束；需要时把中文视觉描述译成英文。最终报告实际 provider、图片路径、宽高及目标接线。

## Provider 映射（旧 generator_id → MCP provider）

默认不传 `provider`，由 MCP 服务端自动选模；只有用户明确点名模型时才传对应值。下表用于迁移明确的模型选择，不用于替用户指定模型。“高质量”“游戏向”等需求写进 prompt，仍交给服务端选模。最终报告使用服务端实际返回的模型。

| 旧 generator_id | MCP `provider` | 说明 |
|---|---|---|
| `frontier-game-design`（旧默认） | `frontier` | 仅用户明确选择 Frontier 时传入 |
| `huoshan_seedream_image` | `seedream` | 仅用户明确选择 SeeDream 时传入 |
| `frontier-effect` | `frontier`（或 `frontier_flare`） | 风格化特效；Flare=快速高质量，Sunburst=细节/参考一致性 |
| —（新增） | `seedream_pro` / `frontier_sunburst` / `frontier_lite` / `hy_image_v3` | 见 MCP 工具说明 |

**MCP 上暂无对应、不要传**：`prompt_template`（game_icon/concept_art 前缀请直接写进 prompt）、`q_value`、`aspect_ratio`（用 `image_size` 预设表达比例）、`generator_id` 字段本身。

**参数映射**：`size`（SeeDream 像素）→ `image_size`（预设或 `{"width":W,"height":H}`）；`imageSize` → `image_size` 预设名；`resolution`/`output_format` → 同名 MCP 参数；`is_segmentation` → 同名；`resize_width` → 同名。

## ⚠️ Skill 独有约束

1. **`prompt` 永远 required**——即使图生图模式也必须写。若用户只给参考图未给描述，先读图识别内容自行写 prompt 再提交。
2. **🚫 webp 不可导入 Unity**——`output_format` 只选 `png` 或 `jpeg`。若 check_task 返回 webp URL，重新提交生成要求 png/jpeg。
3. **积分确认**——MCP 高成本门槛返回确认提示时，按 MCP 指引处理（低于阈值自动放行）。
4. **不要用截图验证图片**——结果是资产文件。用 `unity_asset(action="get_info")` 确认存在与尺寸即可。
5. **本地参考图必须先 `file_upload`** 拿 CDN URL（禁止 base64 / 本地路径直传）。

## When to Use / NOT to Use

适用：通用图片、纹理、概念图、背景、海报；参考图风格化转换。

不适用：
- 透明背景 sprite（图标、物品、立绘）→ `generate_sprite`（未迁移，仍走 Unity）
- 一张图拆多图层 → `generate_image_layers`（本 skill 的分层版流程）
- 放大已有图片 → `upscale_image`（本 skill 的放大版流程）
- 序列帧 spritesheet → `generate_frontier_sequence` / `generate_2d_sprite_sequence_*`（未迁移，仍走 Unity）
- 天空盒 / 音频 / 3D → 各自 skill

> 用户说"用火山模型"/"用 SeeDream"时 `provider=seedream`；没有点名模型则省略 `provider`。

## 工具

### 第一段 · MCP `generate_image`（直接调用）

```json
{
  "prompt": "a serene mountain lake at sunset, photorealistic",
  "image_size": "landscape_16_9",
  "is_segmentation": false,
  "image_urls": "https://.../ref.png"
}
```

异步：提交拿 `task_id` + 轮询元数据 → 后台轮询 → `check_task` 一次取图片 URL。

### 第二段 · `import_image_from_url`（execute_custom_tool）

```python
execute_custom_tool(
  tool_name="import_image_from_url",
  parameters={
    "image_url": "https://.../result.png",   # check_task 拿到的永久 CDN URL
    "prompt": "a serene mountain lake at sunset",  # 可选，History 显示
    "output_path": "Assets/TJGenerators/History/MyImage.png",  # 可选，默认 History/ImportImage/<url-hash>.png
    "session_id": "..."
  })
```

返回：`task_id`、`placeholder_path`（1×1 灰 PNG，GUID 稳定）、`estimated_wait_seconds`。

### `query_local_task`

每次等待结束调用**一次**；返回 `status`（importing/recovering/completed/failed）、完成时 `image_path` + `preview_url`。**不要无间隔连续查询。**

## `<bg_task_done>` 通知字段（第二段导入，供被动收到时参考）

`image_path`（项目内资产路径）、`preview_url`（源 CDN URL）、`layer_*` 字段仅多图模式（分层流程）出现。

## 示例

```python
# 1) MCP 生成（直接调用）→ task_id → 后台轮询 → check_task → image_url
# 2) 导入
execute_custom_tool("import_image_from_url", {
  "image_url": image_url, "prompt": "...", "session_id": "..."
})
# → placeholder_path；前台等待最多 30 秒；在当前子代理内等待，不结束任务
# 3) 本次等待结束
execute_custom_tool("query_local_task", {"task_id": import_task_id})
# → image_path = Assets/TJGenerators/History/ImportImage/<hash>.png
```

## 故障处理

| 症状 | 原因 | 处理 |
|---|---|---|
| MCP 提交报 `[InvalidParameter]` | 参数校验失败 | 看错误文本改参数重提 |
| check_task URL 为 .webp | MCP output_format 默认 | 报告格式不兼容；新提交应指定 `output_format: png/jpeg`，不自动重生成 |
| `import_image_from_url` 报扩展名不支持 | 同上 | 同上 |
| query 返回 `recovering` | 域重载中断 | 再进行一次前台短等待等自动恢复（URL 永久、幂等重跑） |
| 导入失败 | 下载/写入错误 | 看 `error` 字段；URL 永久，可用 `import_image_from_url` 同 URL 重试（原位覆盖） |
| 通知未到达 | 通知可能未送达当前执行者 | 按共享执行约定处理——有间隔等待 + 状态查询 是标准完成模式 |
