---
name: unity-image-generation
description: Generate image assets (Texture2D, PNG) in Unity using AI via text prompts or reference images. Use this skill whenever the user wants to create a 2D image/texture asset that is NOT a sprite — for example concept art, reference images, UI backgrounds, posters, banners, or any general-purpose PNG texture. Trigger proactively for requests like "帮我生成一张图片", "生成一张概念图", "AI画一张图", "make me a texture", "generate an image", even if they don't say "image generation". Uses Frontier Game Design (frontier-game-design) as the default model. Also supports 火山 SeeDream and Frontier (stylized effects). Use unity-sprite-generation instead if the user specifically needs a Sprite (game icon, item image, character portrait with transparent background).
---

> ⚠️ **执行约束**
> - **主 agent**：无 `execute_custom_tool` 权限，必须 `task(subagent_name="image-generator", ...)` 委托，不要 `activate_skill` 后自己调。
> - **子代理（本文档主要读者）**：有权限，按下方 `execute_custom_tool(...)` 示例执行。

> ⛔ **`place_assets_in_scene` 调用规则**
> - **调用方式**：`activate_skill("unity-place-assets-in-scene")` → 按 §4i Texture2D / Image 模板用 `execute_csharp_script` 赋给 `Material.mainTexture` 或建 `RawImage`（**不是** `execute_custom_tool`）。
> - **子代理**：提交后**立即调一次**应用 `placeholder_path`；收到 `<bg_task_done>` 后**不再调**（场景里所有引用该路径的 `Renderer.sharedMaterial.mainTexture` / `RawImage.texture` 自动指向真实图片）。重复放置会让场景里出现多个相同 RawImage / Quad。
> - **主 agent**：报告里的 `image_path` 是"已放置"的证据，不是"请你放置"的指示，**不要再调**。
> - **例外**：用户明确要"再放一份 / 换位置"时才再次调用。详见 [async-pattern §5.1](../../experience/templates/generator-async-pattern.md#51-place_assets_in_scene-调用规则)。

# Generate Image Asset in Unity 🖼️

Generate image assets (Texture2D, PNG) in Unity using AI, from text prompts or reference images.
Output: PNG imported as **Texture2D (Default type)**, auto-saved to `Assets/TJGenerators/History/`.

支持三个模型：
- **Frontier Game Design** (`frontier-game-design`，默认) — 游戏向文/图生图，带游戏专属 prompt 模板；支持 `imageSize` (square_hd/square/portrait_4_3/portrait_16_9/landscape_4_3/landscape_16_9) 和 `outputFormat` (png/jpeg)
- **火山 SeeDream** (`huoshan_seedream_image`) — 通用文/图生图，支持像素级 size、自动抠背景
- **Frontier** (`frontier-effect`) — 风格化特效；支持 `resolution` (0.5K/1K/2K/4K)、`aspect_ratio`、`output_format` (png/jpeg)

## 🚦 执行四步（不要跳读外链）

1. 调 `generate_image` → 拿 `task_id` + `placeholder_path`（1×1 灰 PNG）
2. 立即 `place_assets_in_scene`（资产类型 `Texture2D`，路径用 `placeholder_path`）→ 赋给 Material/RawImage
3. **END RESPONSE TURN** — 不要 poll、不要 `query_image_status`、不要继续操作
4. 下一轮收到 `<bg_task_done>` → 读 `image_path`（已原地覆盖，所有引用该路径的 `mainTexture` / `RawImage.texture` 自动指向真实图片，**不要再 place**）

**档位**：短任务 30–90 秒；120 秒内无通知才允许 `query_image_status` 一次。最多 **5 个**并发。完整 async 规则见 [generator-async-pattern](../../experience/templates/generator-async-pattern.md)。

## ⚠️ Skill 独有约束

1. **并发上限 5**——同时运行的 image 任务最多 5 个，超过会被拒绝。批量生成超过 5 个时分批做。
2. **`prompt` 永远 required**——即使 image-to-image 模式也必须写 prompt。若用户只给参考图未给描述，先 `Read` 图片识别内容，自行写 prompt 再提交，否则 API 会 500 报错。
3. **`size` 最小阈值 ~3,686,400 像素**——小于该阈值的尺寸后端会自动按宽高比放大；仍报 400 时改用下表预设值。
4. **不要用截图验证图片**——结果是资产文件而非场景对象。用 `unity_asset(action="get_info")` 确认存在与尺寸即可。

## When to Use / NOT to Use

适用：通用图片、纹理、概念图、背景、海报；对参考图做风格化转换（Frontier）。

不适用：
- 透明背景 sprite（图标、物品、角色立绘）→ `generate_sprite`
- 一张图拆成多图层 → `generate_image_layers`
- 天空盒 → `generate_skybox`
- 音频 → `generate_audio_clip` / `generate_sound_effect`
- 3D 模型 → `generate_3d_model`

> 用户说"用火山模型"/"用 SeeDream"时改用 `huoshan_seedream_image`。

## 工具

所有工具通过 `execute_custom_tool` 调用。

### `generate_image`

```python
execute_custom_tool(
  tool_name="generate_image",
  parameters={
    "prompt": "a serene mountain lake at sunset, photorealistic",  # Required（永远）
    "generator_id": "frontier-game-design",  # 默认；或 "huoshan_seedream_image"、"frontier-effect"
    "image_path": "Assets/ref.png",            # 可选：参考图，触发 image-to-image
    # SeeDream 专属：
    "size": "2048x2048",                       # 低于最小阈值会被后端自动放大；见 size 表
    # SeeDream / Game Design 共享（后端后处理，不发往 AI API）：
    "is_segmentation": False,                  # 默认 false（不抠图）；需要透明背景时设 true
    "q_value": 75,                             # 抠图后压缩质量 1–100
    "resize_width": 0,                         # 最大输出宽度，0 = 不缩放
    # Frontier Effect 专属：
    "resolution": "1K",                        # "0.5K" / "1K" / "2K" / "4K"，默认 "1K"
    "aspect_ratio": "auto",                    # "auto"/"16:9"/"9:16"/"1:1"/"4:3"/"3:4"/"3:2"/"2:3"/"5:4"/"4:5"/"21:9"，默认 "auto"
    "output_format": "png",                    # "png" / "jpeg"，默认 "png"
    # Frontier Game Design 专属（SeeDream / Frontier Effect 忽略）：
    "imageSize": "square_hd",                  # "square_hd"/"square"/"portrait_4_3"/"portrait_16_9"/"landscape_4_3"/"landscape_16_9"，默认 "square_hd"
    "outputFormat": "png",                     # "png" / "jpeg"，默认 "png"
    "prompt_template": "",                     # "game_icon" / "concept_art" 或 ""（不加模板）
    # output_path: 不建议指定，默认 Assets/TJGenerators/History/
  }
)
```

返回字段：
- `task_id`、`placeholder_path`（1×1 灰色 PNG，**立即可用**）
- `mode`：`"text-to-image"` 或 `"image-to-image"`
- `estimated_wait_seconds` ≈ 60
- `notification_mode: "bg_task_done"`

提交失败时 `result["success"] == false`，读 `error_code` / `message`，**不要**poll。

> **Image-to-image 工作流**：用户只给参考图未给描述时，先用 `Read` 读图、识别内容，再写 prompt，最后提交——不要不带 prompt 提交。

### `<bg_task_done>` 独有字段

通用字段见模板。本 skill 额外字段：

| 字段 | 说明 |
|---|---|
| `image_path` | 最终 Texture2D 资产路径（== `placeholder_path`，原地覆盖） |
| `preview_url` | 预览 URL 或本地路径 |
| `generator_id` | 使用的生成器 |
| `prompt` | 原始 prompt |

### `query_image_status` / `list_image_tasks`

`query_image_status` 仅作 fallback（120 秒后单次）。返回字段同 `<bg_task_done>` payload，外加 `placeholder_path`（仅 `generating` 时）。

`list_image_tasks` 返回当前 session 的所有 image 任务。

## 模型选择

| 场景 | 推荐模型 |
|---|---|
| 通用文生图（概念、背景、纹理） | `frontier-game-design`（默认） |
| 图生图带风格/特效转换 | `frontier-effect` |
| 需要像素级精确尺寸 | `huoshan_seedream_image` |
| 需要自动去背景 | `frontier-game-design`（`is_segmentation: true`）或 `huoshan_seedream_image`（`is_segmentation: true`） |
| 风格化艺术效果（基于参考图） | `frontier-effect` |
| 需要 JPEG 输出 | `frontier-effect`（`output_format: "jpeg"`）或 `frontier-game-design`（`outputFormat: "jpeg"`） |
| 用户说"用火山模型"/"用 SeeDream" | `huoshan_seedream_image` |
| 游戏向图片生成（图标、概念图） | `frontier-game-design` |
| 需要游戏专属 prompt 模板（game_icon / concept_art） | `frontier-game-design` |

## SeeDream `size` 选项

> **最小像素阈值 ~3,686,400（如 1920×1920）；低于该值后端会自动按比例放大。** 默认 `"2048x2048"`。

| 值 | 说明 |
|---|---|
| `"2048x2048"` | 2K 1:1 — 方形纹理、图标 **(默认)** |
| `"2304x1728"` | 2K 4:3 — 横向 |
| `"1728x2304"` | 2K 3:4 — 竖向 |
| `"2560x1440"` | 2K 16:9 — 宽屏、UI 背景 |
| `"1440x2560"` | 2K 9:16 — 移动端竖屏 |
| `"2496x1664"` | 2K 3:2 — 标准照片 |
| `"1664x2496"` | 2K 2:3 — 标准照片竖向 |
| `"3024x1296"` | 2K 21:9 — 超宽 banner |
| `"4096x4096"` | 4K 1:1 — 主美、高细节纹理 |
| `"4704x3520"` | 4K 4:3 — 大横向 |
| `"3520x4704"` | 4K 3:4 — 大竖向 |
| `"5504x3040"` | 4K 16:9 — banner、splash |
| `"3040x5504"` | 4K 9:16 — 全身立绘竖向 |
| `"4992x3328"` | 4K 3:2 — 大照片 |
| `"3328x4992"` | 4K 2:3 — 大照片竖向 |

## SeeDream `is_segmentation` 决策

| 用途 | 值 | 原因 |
|---|---|---|
| UI 背景、splash、环境美术 | `false` | 需要完整矩形图 |
| 独立主体需要透明背景 | `true` | 抠图覆盖使用 |
| 概念图、纹理、无缝平铺 | `false` | 需要完整图，不要抠图 |
| 海报、banner、卡牌 | `false` | 需要完整图 |

> 通用图片生成（背景、纹理、概念图）默认设 `is_segmentation: false`。

## 使用示例

### 文生图：概念图

```python
result = execute_custom_tool(
    tool_name="generate_image",
    parameters={
        "prompt": "a misty ancient forest with glowing mushrooms, fantasy concept art",
        "is_segmentation": False,
        "size": "2560x1440"
    }
)
task_id = result["task_id"]
placeholder_path = result["placeholder_path"]

# ✅ 立即用 place_assets_in_scene 把 placeholder 放到 Material/RawImage
# 然后 END RESPONSE TURN，等 bg_task_done 通知
```

### 文生图：UI 背景

```python
parameters={
    "prompt": "dark stone dungeon wall texture, seamless, game UI background",
    "is_segmentation": False,
    "size": "2048x2048"
}
```

### 图生图：Frontier 风格转换

```python
parameters={
    "generator_id": "frontier-effect",
    "prompt": "a cute banana mascot, studio lighting, high detail",
    "image_path": "Assets/ConceptArt/banana_sketch.png",
    "resolution": "2K",
    "aspect_ratio": "1:1"
}
```

### 文生图（Frontier）

```python
parameters={
    "generator_id": "frontier-effect",
    "prompt": "cyberpunk city street at night, neon lights, rain reflections",
    "resolution": "2K",
    "aspect_ratio": "16:9"
}
```

### 文生图：Frontier Game Design

```python
parameters={
    "generator_id": "frontier-game-design",
    "prompt": "a fantasy castle on a floating island, dramatic lighting",
    "imageSize": "landscape_16_9",
    "outputFormat": "png"
}
```

### 图生图：Frontier Game Design

```python
parameters={
    "generator_id": "frontier-game-design",
    "prompt": "a detailed game icon of a magical sword",
    "image_path": "Assets/ConceptArt/sword_sketch.png",
    "imageSize": "square_hd",
    "prompt_template": "game_icon"
}
```

### Frontier Game Design + concept_art 模板

```python
parameters={
    "generator_id": "frontier-game-design",
    "prompt": "ancient ruin in a mystical forest",
    "imageSize": "landscape_4_3",
    "prompt_template": "concept_art"
}
```

### 并发批量（最多 5 个）

```python
task_ids = []
images = [
    ("mountain lake at sunset, photorealistic", "2560x1440"),
    ("dark forest with fog, horror atmosphere", "2560x1440"),
    ("futuristic city skyline, sci-fi", "2560x1440"),
]

for prompt, size in images:
    result = execute_custom_tool(
        tool_name="generate_image",
        parameters={"prompt": prompt, "size": size, "is_segmentation": False}
    )
    task_ids.append(result["task_id"])
    # ✅ 直接继续，不 poll

# END RESPONSE TURN — 每个 task 都会单独发 bg_task_done
```

## 验证生成结果

> **不要用 unity_screenshot**——结果是资产文件，截图无意义。

```python
unity_asset(
    action="get_info",
    path="Assets/TJGenerators/History/<image_name>.png"
)
```

返回的尺寸/类型信息足以确认生成成功、资产可用。

## 放入场景

资产类型 **`Texture2D`**，路径用 `image_path`。`placement_instruction` 可指定用途：

| 用途 | placement_instruction |
|---|---|
| 赋给 3D 物体 Material | `"赋给 Xxx 的材质"` |
| UI 背景（Canvas RawImage） | `"作为 RawImage UI"` |
| 天空盒 | 改用 `generate_skybox` 更合适 |

规则见 [async-pattern §5 / §5.1](../../experience/templates/generator-async-pattern.md#5-placeholder-工作流适用于会返回-placeholder_path--prefab_output_path-的工具)。

## Prompt 写作指南

| 用途 | Prompt 示例 |
|---|---|
| 环境/背景 | `"misty pine forest at dawn, god rays, atmospheric fog, photorealistic"` |
| UI 背景 | `"dark stone texture, weathered, seamless, game UI panel background"` |
| 概念图 | `"futuristic space station interior, sci-fi, cinematic lighting"` |
| 特效/纹理 | `"magical energy swirl, blue and gold, transparent background, circular"` |
| 海报/banner | `"epic fantasy battle scene, wide format, dramatic lighting"` |

技巧：
- 背景/纹理加 `"seamless"` 或 `"no background"`
- 氛围词：`dramatic` / `peaceful` / `eerie`
- 光照词：`soft natural light` / `neon glow` / `candlelight`
- Frontier 模型最好搭配参考图
- Frontier Game Design 模型可用 `prompt_template` 自动加游戏向前缀

## `imageSize` 选项 — 仅 Frontier Game Design

| 值 | 标签 | 适用 |
|---|---|---|
| `"square_hd"` | 1:1 HD | 游戏图标、概念图 **(默认)** |
| `"square"` | 1:1 | 标准方图 |
| `"portrait_4_3"` | 3:4 | 角色立绘、卡牌 |
| `"portrait_16_9"` | 9:16 | 移动端 UI、竖图 |
| `"landscape_4_3"` | 4:3 | 横向概念图 |
| `"landscape_16_9"` | 16:9 | 宽屏 banner、splash |

## `prompt_template` 选项 — 仅 Frontier Game Design

可选模板，会在用户 prompt 前自动加固定前缀，引导模型生成对应类型的游戏资产。

| 值 | 名称 | 说明 | 添加的前缀 |
|---|---|---|---|
| `"game_icon"` | 游戏图标 | 透明、易抠图的游戏图标 | `"game asset icon, single centered subject, clean edges, high detail"` |
| `"concept_art"` | 概念设计 | 游戏概念图 | `"game concept art, detailed design, professional illustration"` |
| `""` | — | 不加模板（默认） | — |

## 参数速查

| 参数 | 类型 | 默认 | 说明 |
|---|---|---|---|
| `generator_id` | string | `"frontier-game-design"` | 也可 `"huoshan_seedream_image"`、`"frontier-effect"` |
| `prompt` | string | — | **永远 required** |
| `image_path` | string | — | 参考图，触发 image-to-image |
| `size` | string | `"2048x2048"` | **SeeDream only**；低于最小阈值的尺寸会被后端自动放大 |
| `is_segmentation` | bool | `false` | **Game Design / SeeDream**；后端后处理抠图；背景/纹理类默认不抠图 |
| `q_value` | int | 75 | **Game Design / SeeDream**；segmentation 后压缩质量 1–100 |
| `resize_width` | int | 0 | **Game Design / SeeDream**；最大输出宽度，0 = 不缩放 |
| `resolution` | string | `"1K"` | **Frontier Effect only**；`"0.5K"`/`"1K"`/`"2K"`/`"4K"` |
| `aspect_ratio` | string | `"auto"` | **Frontier Effect only**；详见上文 |
| `output_format` | string | `"png"` | **Frontier Effect only**；`"png"`/`"jpeg"` |
| `imageSize` | string | `"square_hd"` | **Game Design only**；`"square_hd"`/`"square"`/`"portrait_4_3"`/`"portrait_16_9"`/`"landscape_4_3"`/`"landscape_16_9"` |
| `outputFormat` | string | `"png"` | **Game Design only**；`"png"`/`"jpeg"` |
| `prompt_template` | string | `""` | **Game Design only**；`"game_icon"`/`"concept_art"` 或 `""`（不加模板） |
| `output_path` | string | — | 不建议指定，默认 `Assets/TJGenerators/History/` |

## 故障排查

### Skill 独有问题

> 通用故障（配置缺失 / 任务卡住 / 状态异常 / 未登录）见 [generator-async-pattern §10](../../experience/templates/generator-async-pattern.md#10-通用故障排查)。

| 问题 | 原因 | 解决 |
|---|---|---|
| `prompt is required` / 500 错误（image-to-image） | 没写 prompt | image-to-image 也必须写 prompt；先 Read 参考图再生成 prompt |
| `Either 'prompt' or 'image_path' must be provided` | 两个都没传 | prompt 永远 required；image_path 可选 |
| 400 size 错误 | 后端无法按比例放大该尺寸 | 后端通常会自动放大低于阈值的尺寸；仍报错就改用上表预设值 |

### Domain reload 后 task 丢失

通用恢复流程见 [generator-async-pattern §6](../../experience/templates/generator-async-pattern.md#6-domain-reload-recovery)。本 skill 完成态阈值：

- PNG ≥ 50 KB → 真实图片已就绪

可用 `glob("Assets/TJGenerators/History/*.png")` + 文件大小恢复。

---

**Task ID Format**：`image_{counter}_{timestamp}`

**Notes**：
- 输出 PNG 自动导入为 `TextureImporterType.Default`（不是 Sprite）
- 自动应用 `TuanjieAI` 标签
- **并发上限 5**——批量生成超过 5 个时分批做
- 需 Unity Editor 在线运行；消耗 AI 服务额度
