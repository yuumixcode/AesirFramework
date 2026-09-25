---
name: unity-image-upscaling
description: "Upscale images in Unity using AI super-resolution (Real-ESRGAN). Supports 1x-8x upscaling with model variants for photo, anime/2D game art, and denoising. Use this skill whenever the user wants to enlarge, enhance, or upscale an existing image — e.g. \"放大图片\", \"提高分辨率\", \"upscale this texture\", \"enhance image quality\", \"2x this image\". Two-phase MCP flow: upscaling runs on the MCP backend (file_upload + upscale_image + check_task), landing runs in Unity via import_image_from_url. Trigger proactively for any image resolution enhancement or super-resolution request in Unity. Do NOT use for generating new images (generate_image) or layer decomposition (generate_image_layers)."
---

> **执行前读取**：[共享执行约定](../../experience/templates/generator-async-pattern.md)。
> 主 agent 按需委托 `general-purpose`；已在子代理中则直接执行本 skill，不再委托。已提交的阶段从原任务续接。

# Upscale Image in Unity 🔍 (MCP two-phase)

用 Real-ESRGAN 超分辨率放大工程内图片（1x–8x，照片/动漫 2D/降噪模型变体）。
输出：放大后的 PNG/JPEG，**默认存为 History 新资产**（不覆盖原图；要覆盖时显式传 `output_path=原图路径`，原位覆盖保持 GUID）。

**两段式流程**：
- **第一段·放大（MCP 后端）**：`file_upload`（工程内图片 → CDN URL）→ MCP `upscale_image`（scale/model/face_enhance/output_format）→ 后台轮询 → `check_task` 拿**结果 CDN URL**
- **第二段·导入（Unity CustomTool）**：`import_image_from_url` 单图模式 → 占位原地覆盖 + TextureImporter(Default) + 标签 + History → 本次等待结束后 `query_local_task` 单次拉取

## 🚦 执行五步

1. `file_upload(工程内图片路径)` → CDN URL
2. MCP `upscale_image`：`image_url` + `scale`（1-8，默认 4）+ `model`（可省略自动选）+ `face_enhance`（默认 false）+ `output_format`（png/jpeg）→ 拿 `task_id` → 后台轮询
3. `check_task` 一次 → 结果 URL 在 `output.data.imageUrls`（**数组**；确认 .png/.jpg/.jpeg）
4. `execute_custom_tool("import_image_from_url", {image_url, prompt, output_path?})` → 拿 `task_id` + `placeholder_path` → **在当前子代理内前台等待（每次最多30秒）（~30s）** → **在当前子代理内等待，不结束任务**
5. 本次等待结束 → `query_local_task` **一次** → 读 `image_path`；仍是 `importing` 则再进行一次前台短等待。**不要干等 Unity 侧推送**

## Model 变体选择

| model | 适用 |
|---|---|
| （省略） | 自动按 scale 选：≤2x → x2plus，>2x → x4plus |
| `RealESRGAN_x4plus_anime_6B` | **动漫/2D 游戏美术首选** |
| `RealESRGAN_x4_v3` / `x4_wdn_v3` / `x4_anime_v3` | 降噪/动漫变体 |
| `face_enhance: true` | 人脸增强（照片类） |

## ⚠️ Skill 独有约束

1. **🚫 webp 不可导入**——`output_format` 只选 `png`/`jpeg`
2. **不覆盖原图是默认**；需要原地替换（保 GUID、场景引用不丢）时 `output_path` 传原图路径
3. 本地/工程内图片**必须先 `file_upload`**（MCP 只收 CDN URL）
4. 放大结果建议用 `unity_asset(action="get_info")` 验证新尺寸，不要截图验证

## 工具

### 第一段 · MCP `upscale_image`（直接调用）

```json
{
  "image_url": "https://.../original.png",
  "scale": 4,
  "model": "RealESRGAN_x4plus_anime_6B",
  "face_enhance": false,
  "output_format": "png"
}
```

### 第二段 · `import_image_from_url`（execute_custom_tool，单图模式）

```python
execute_custom_tool(
  tool_name="import_image_from_url",
  parameters={
    "image_url": "https://.../upscaled.png",
    "prompt": "upscale 4x: <原图描述>",
    "output_path": "Assets/TJGenerators/History/Upscaled_4x.png",  # 可选；传原图路径则原位覆盖
    "session_id": "..."
  })
```

返回 `task_id` + `placeholder_path`（GUID 稳定，可提前挂到材质/RawImage）。

### `query_local_task`

本次等待结束后调用**一次**；完成时返回 `image_path` + `preview_url`。

## 故障处理

| 症状 | 处理 |
|---|---|
| 上游报原图尺寸太小/太大 | 按 MCP 错误文本调整（先降 scale 或裁剪） |
| URL 为 .webp | 报告格式不兼容，不自动重生成；新提交应使用 png |
| query 返回 `recovering` | 再进行一次前台短等待（幂等重跑） |
| 导入失败 | 看 `error`；URL 永久，同 URL 重试 `import_image_from_url` |
