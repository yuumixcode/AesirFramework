---
name: unity-image-layers-generation
description: Split one image into multiple independent RGBA layers in Unity using AI (image-layering / Qwen, or seedream_pro for Seedream 5.0 Pro auto layer decomposition). Use this skill whenever the user wants to decompose a picture into layers — e.g. "图片分层", "拆图层", "separate image layers", "extract layers from this image", "把这张图分层", "generate image layers", "Seedream 分层", "图层拆分". Trigger proactively for any layer-decomposition / multi-layer PNG output request from a single input image. Do NOT use for generating a new image from text (use generate_image) or upscaling (use upscale_image).
---

> ⚠️ **执行约束**
> - **主 agent**：无 `execute_custom_tool` 权限，必须 `task(subagent_name="image-layers-generator", ...)` 委托，不要 `activate_skill` 后自己调。
> - **子代理（本文档主要读者）**：有权限，按下方 `execute_custom_tool(...)` 示例执行。

> ⛔ **`place_assets_in_scene` 调用规则**（本 skill **可选 placeholder**）
> - 图层是独立 PNG（Texture2D），通常**不需要**放入场景。
> - 若用户明确要求预览第 0 层：`activate_skill("unity-place-assets-in-scene")` → 按 §4i Texture2D 模板用 `execute_csharp_script`（**不是** `execute_custom_tool`）。
> - 提交后若放置，只用 `placeholder_path` **调一次**；`<bg_task_done>` 后**不再调**（原地覆盖）。
> - **主 agent**：报告里的路径是"已处理"的证据，**不要再调**。
> - 详见 [async-pattern §5.1](../../experience/templates/generator-async-pattern.md#51-place_assets_in_scene-调用规则)。

# Generate Image Layers in Unity

将**一张输入图**拆成多个独立 **RGBA 图层 PNG**。
Output：N 张 PNG（`TextureImporterType.Default`，含 alpha），自动保存到 `Assets/TJGenerators/History/`。

- 第 0 层：覆盖 `placeholder_path`（GUID 不变），文件名形如 `ImageLayers_yyyyMMdd_HHmmss.png`
- 其余层：同目录 `{basename}_1.png`、`{basename}_2.png` …

模型固定：`image-layering`（Qwen image layered）。

另支持 **Seedream 5.0 Pro 图层拆分**（`provider: "seedream_pro"`）：自动拆分为 **1 张底图 + 最多 16 个透明 PNG 图层**，无需指定层数；`prompt` 可选（留空自动拆分全部主体），`num_layers` 被忽略；可选 `size` 档位 `1K / 1.5K / 2K / auto`（默认 auto 跟随输入图）。

## 执行四步（不要跳读外链）

> 本 skill 有 placeholder（第 0 层），但图层通常不放场景。

1. 调 `generate_image_layers`（`image_path` + `prompt` 必填）→ 拿 `task_id` + `placeholder_path`
2. （可选）仅当用户要预览时 `place_assets_in_scene`（`Texture2D`，路径用 `placeholder_path`）
3. **END RESPONSE TURN** — 不要 poll、不要 `query_image_layers_status`、不要继续操作
4. 下一轮收到 `<bg_task_done>` → **优先读 `layer_paths` / `layers_found`**（通知在全部图层落盘后才发出）

**档位**：中任务 60–180 秒；120 秒内无通知才允许 `query_image_layers_status` **一次**。完整 async 规则见 [generator-async-pattern](../../experience/templates/generator-async-pattern.md)。

## Skill 独有约束

1. **`image_path` 必填**——本 skill 只做图驱动分层，没有纯文本模式。
2. **`prompt` 必填（qwen）**——描述画面内容以辅助拆层；用户只给图时先 `Read` 图片再写英文 prompt。**seedream_pro 时可选**（留空自动拆分）。
3. **`num_layers` 范围 1–8（qwen）**——默认 4；超出范围钳制到 1–8；非法值回退默认 4。**seedream_pro 忽略此参数**（自动 1–16 层）。
4. **通知含完整 `layer_paths`**——优先用 payload；若缺失再用 `glob(f"{layers_folder}/ImageLayers_*.png")` 或 `query_image_layers_status`。
5. **并发上限 5**——同时运行的 image_layers 任务最多 5 个。

## When to Use / NOT to Use

适用：把一张图拆成多个可独立编辑的透明图层、角色/道具前景背景分离、UI 元素分层。

不适用：
- 从文本生成新图 → `generate_image`
- 透明背景单张精灵 → `generate_sprite`
- 放大已有图 → `upscale_image`
- 2D 帧动画 → `generate_sprite_sequence`

## 工具

所有工具通过 `execute_custom_tool` 调用。

### `generate_image_layers`

```python
execute_custom_tool(
  tool_name="generate_image_layers",
  parameters={
    "image_path": "Assets/Characters/hero.png",  # Required
    "prompt": "a cute cartoon cat sitting in a sunny meadow",  # Required — English preferred
    "num_layers": 4,  # Optional, 1–8, default 4
    # output_path: 不建议指定，默认 Assets/TJGenerators/History/
  }
)
```

### 返回字段

- `success`: bool
- `task_id`
- `placeholder_path`：第 0 层 1×1 灰 PNG（可立即放置）
- `num_layers`：回传请求层数
- `estimated_wait_seconds` ≈ 90
- `notification_mode: "bg_task_done"`

提交失败时 `result["success"] == false`，读 `error_code` / `message`，**不要**poll。

### `<bg_task_done>` 独有字段

| 字段 | 说明 |
|---|---|
| `layer_0_path` | 第 0 层最终路径（== `placeholder_path`，原地覆盖） |
| `layers_folder` | 所有图层所在目录 |
| `layer_count` | 请求的图层数量 |
| `layers_found` | 实际落盘图层数 |
| `layer_paths` | 按索引排序的完整路径列表（优先使用） |
| `preview_url` | 预览 URL |
| `prompt` | 原始 prompt |
| `input_image_path` | 输入图路径 |

完成后收集全部图层：

```python
# 优先：通知已带完整列表（全部层下载完成后才发 bg_task_done）
layer_paths = payload["layer_paths"]
# fallback：命名为 ImageLayers_yyyyMMdd_HHmmss.png + {basename}_1.png …
glob(f"{layers_folder}/ImageLayers_*.png")
# 或 query_image_layers_status → layer_paths
```

### `query_image_layers_status` / `list_image_layers_tasks`

`query_image_layers_status` 仅作 fallback（120 秒后单次）。`completed` 时额外返回 `layer_0_path` / `layers_folder` / `layer_count` / `layer_paths`。

`list_image_layers_tasks` 返回当前 session 的所有 image_layers 任务。

## 参数速查

| 参数 | 类型 | 默认 | 说明 |
|---|---|---|---|
| `image_path` | string | **required** | 要分层的原图 PNG/JPG（seedream_pro 仅支持 1 张，≥512×512） |
| `provider` | string | `qwen` | `qwen` 或 `seedream_pro`（Seedream 5.0 Pro 自动分层） |
| `prompt` | string | **required**（qwen） | 画面描述，辅助拆层（英文更好）；seedream_pro 可选拆分提示词 |
| `num_layers` | int | `4` | 图层数量 1–8（仅 qwen；seedream_pro 忽略） |
| `size` | string | `auto` | seedream_pro 分辨率档位：`1K` / `1.5K` / `2K` / `auto`（仅 seedream_pro） |
| `output_path` | string | — | 不建议指定 |

### provider 决策

| 场景 | 推荐 |
|---|---|
| 需要精确控制层数（1–8） | `qwen` + `num_layers` |
| 要底图 + 多达 16 个透明图层、含层级/边界信息 | `seedream_pro`（自动分层） |
| 用户提到 Seedream / 火山 / 自动分层 | `seedream_pro` |

### `num_layers` 决策

| 用途 | 推荐 |
|---|---|
| 简单主体 + 背景 | 2–3 |
| 一般场景（默认） | 4 |
| 复杂多物体 | 6–8 |

## 使用示例

### 基础分层

```python
result = execute_custom_tool(
    tool_name="generate_image_layers",
    parameters={
        "image_path": "Assets/Art/character.png",
        "prompt": "a fantasy warrior standing, full body, transparent-friendly layers",
        "num_layers": 4,
    }
)
task_id = result["task_id"]
placeholder_path = result["placeholder_path"]

# ✅ 通常跳过 place；直接 END RESPONSE TURN
# 等 bg_task_done → layer_paths / layers_found
```

### 用户只给图、没给描述

```python
# 1. Read 图片 → 识别内容
# 2. 自行写英文 prompt
# 3. 再调用 generate_image_layers
parameters={
    "image_path": "Assets/Art/scene.png",
    "prompt": "a cozy cabin in a snowy forest with smoke from chimney",
    "num_layers": 5,
}
```

### Seedream 自动分层（底图 + 最多 16 层）

```python
result = execute_custom_tool(
    tool_name="generate_image_layers",
    parameters={
        "image_path": "Assets/Art/scene.png",   # Required — 仅 1 张
        "provider": "seedream_pro",              # Seedream 5.0 Pro 图层拆分
        # prompt 可选：留空自动拆分全部主体；也可指定要拆分的元素
        # "prompt": "separate the character, the sword and the background",
        "size": "2K",                            # 可选：1K / 1.5K / 2K / auto
    }
)
# layer 0 = 底图（不透明），layer 1..N = 透明 PNG 图层
```

## 故障排查

### Skill 独有问题

> 通用故障见 [generator-async-pattern §10](../../experience/templates/generator-async-pattern.md#10-通用故障排查)。

| 问题 | 原因 | 解决 |
|---|---|---|
| `'image_path' is required` | 没传输入图 | 必须提供本地图片路径 |
| `'prompt' is required` | 没写 prompt | 先 Read 图片再写描述 |
| 只有 1 张结果 | `num_layers=1` 或后端只返回一层 | 提高 `num_layers` 重试 |
| 找不到 `_1.png` 等 | 刚完成尚未索引完 | 优先用通知 `layer_paths`；否则 `AssetDatabase.Refresh()` 后 `glob("…/ImageLayers_*.png")` |

### Domain reload 后 task 丢失

通用恢复见 [async-pattern §6](../../experience/templates/generator-async-pattern.md#6-domain-reload-recovery)。本 skill 完成态：

- `layer_0_path` PNG ≥ 50 KB → 第 0 层就绪
- 同目录存在 `{basename}_N.png` → 其余层已落盘

可用 `glob("Assets/TJGenerators/History/ImageLayers_*.png")` 恢复。

---

**Task ID Format**：`image_layers_{counter}_{timestamp}`

**Notes**：
- 输出 PNG 自动导入为 `TextureImporterType.Default`，`alphaIsTransparency: true`
- 自动应用 `TuanjieAI` 标签
- **并发上限 5**
- 需 Unity Editor 在线运行；消耗 AI 服务额度
