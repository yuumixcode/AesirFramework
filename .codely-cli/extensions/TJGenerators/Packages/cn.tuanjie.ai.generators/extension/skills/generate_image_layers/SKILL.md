---
name: unity-image-layers-generation
description: "Split one image into multiple independent RGBA layers in Unity using AI (image-layering / Qwen, or seedream_pro for Seedream 5.0 Pro auto layer decomposition). Use this skill whenever the user wants to decompose a picture into layers — e.g. \"图片分层\", \"拆图层\", \"separate image layers\", \"extract layers from this image\", \"把这张图分层\", \"generate image layers\", \"Seedream 分层\", \"图层拆分\". Two-phase MCP flow: layering runs on the MCP backend (generate_image_layers + check_task), landing runs in Unity via import_image_from_url (multi-image mode, one task imports base + all layers). Trigger proactively for any layer-decomposition / multi-layer PNG output request from a single input image. Do NOT use for generating a new image from text (generate_image) or upscaling (upscale_image)."
---

> **执行前读取**：[共享执行约定](../../experience/templates/generator-async-pattern.md)。
> 主 agent 按需委托 `general-purpose`；已在子代理中则直接执行本 skill，不再委托。已提交的阶段从原任务续接。

# Split One Image Into Layers 🧅 (MCP two-phase)

把一张图分解为独立 RGBA 图层（前景/背景/物件分层，便于在 Unity 中独立调整）。

**两段式流程**：
- **第一段·分层（MCP 后端）**：本地图片先 `file_upload` 拿 CDN URL → `generate_image_layers` 提交 → 后台轮询 → `check_task` 拿**底图 + 各图层 CDN URL 列表**（含 z_index/name/bounding_box 元数据）
- **第二段·导入（Unity CustomTool）**：`import_image_from_url` **多图模式**（`image_urls` 逗号分隔，底图在前）→ 单任务批量导入到目标文件夹 → TextureImporter(Default) + 标签 → History 逐张补录（一图一格）→ 本次等待结束后 `query_local_task` 单次拉取

## 🚦 执行五步

1. `file_upload(本地图片)` → CDN URL（分层必须有输入图）
2. MCP `generate_image_layers`：`image_url`（CDN）+ `prompt`（qwen 必填：一句话描述图片内容）+ `num_layers`（1-8，默认 4）+ `provider`（默认 `qwen`；`seedream_pro` 为自动拆分 1 底图+最多 16 层）→ 拿 `task_id` → 后台轮询
3. `check_task` 一次 → 图层 URL 在 `output.data.imageUrls`（**数组**，底图在前；确认扩展名 .png/.jpg/.jpeg）
4. `execute_custom_tool("import_image_from_url", {image_urls: "底图URL,图层1URL,...", names: "layer_0,layer_1,...", prompt, output_path: "Assets/.../MyLayers"})` → 拿 `task_id` + `layers_folder` → **在当前子代理内前台等待（每次最多30秒）（~30s）** → **在当前子代理内等待，不结束任务**
5. 本次等待结束 → `query_local_task` **一次** → 读 `layer_0_path`（底图）/ `layers_folder` / `layer_count` / `layer_paths`；仍是 `importing` 则再进行一次前台短等待。**不要干等 Unity 侧推送**

## ⚠️ Skill 独有约束

1. **底图在前**：`image_urls` 第一个是 base image（layer_0），其余是图层；`names` 与之一一对应（默认 layer_0/layer_1/...）
2. **一次任务导全部图层**——不要每层一个任务/一个等待者
3. **🚫 webp 不可导入**——MCP `output_format` 只选 `png`（seedream_pro 的 webp 选项不要用）
4. **图片≥512×512**（seedream_pro 要求）；本地文件必须先 `file_upload`
5. **qwen 分层必须给 prompt**——缺 prompt 时先读图补一句内容描述
6. **seedream_pro 高成本确认**——MCP 返回费用提示时按指引处理；qwen 不触发高成本门槛

## Provider 选择

| provider | 特点 |
|---|---|
| `qwen`（默认） | 1-8 层，需 prompt；普通图片分层够用 |
| `seedream_pro` | 自动拆分（1 底图 + 最多 16 透明层 + z_index/name/bounding_box 元数据），高精度场景；需显式要求或用户同意升级时使用 |

## 工具

### 第一段 · MCP `generate_image_layers`（直接调用）

```json
{
  "image_url": "https://.../input.png",
  "prompt": "a character standing in a forest clearing, game screenshot",
  "num_layers": 4,
  "provider": "qwen"
}
```

### 第二段 · `import_image_from_url`（execute_custom_tool，多图模式）

```python
execute_custom_tool(
  tool_name="import_image_from_url",
  parameters={
    "image_urls": "https://.../base.png,https://.../layer_1.png,https://.../layer_2.png",
    "names": "layer_0,layer_1,layer_2",
    "prompt": "forest character layered",
    "output_path": "Assets/TJGenerators/History/MyLayers",   # 文件夹
    "session_id": "..."
  })
```

多图模式**无占位**（占位仅单图模式）；返回 `task_id` + `layers_folder` + `estimated_wait_seconds`。

### `query_local_task`

本次等待结束后调用**一次**；完成时返回 `layer_0_path`（底图）、`layers_folder`、`layer_count`、`layer_paths`（数组）。

## 故障处理

| 症状 | 处理 |
|---|---|
| qwen 报缺 prompt | 补一句图片内容描述重提 |
| URL 为 .webp | 报告格式不兼容，不自动重生成；新提交应使用 png |
| query 返回 `recovering` | 再进行一次前台短等待（幂等重跑，URL 永久） |
| 图层顺序/z_index 需要调整 | 用 check_task 返回的元数据在 Unity 侧按 z_index 排布，导入顺序不决定渲染顺序 |
