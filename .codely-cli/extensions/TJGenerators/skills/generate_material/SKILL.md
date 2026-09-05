---
name: unity-material-generation
description: Generate PBR surface material assets (seamless texture PNG + Unity .mat file) in Unity using AI via text prompts, reference images, or built-in texture pattern templates. Use this skill whenever the user wants to create a surface material for a 3D object in a Unity game — even if they just say "帮我生成一个金属材质", "给地面加个石头纹理", "make a wood texture for my floor", or "generate a brick wall material". Trigger proactively for any surface material or PBR texture creation request in a Unity context, including environment textures, object surfaces, architectural materials, or any tileable texture content for 3D objects. Do NOT use for 2D sprites, icons, or UI images — those belong to the unity-sprite-generation skill.
---

> ⚠️ **执行约束**
> - **主 agent**：无 `execute_custom_tool` 权限，必须 `task(subagent_name="material-generator", ...)` 委托，不要 `activate_skill` 后自己调。
> - **子代理（本文档主要读者）**：有权限，按下方 `execute_custom_tool(...)` 示例执行。

> ⛔ **`place_assets_in_scene` 调用规则**
> - **调用方式**：`activate_skill("unity-place-assets-in-scene")` → 按 §4e Material 模板用 `execute_csharp_script` 赋给 `Renderer.sharedMaterial`（**不是** `execute_custom_tool`）。
> - **子代理**：提交后**立即调一次**把占位 `.mat` 赋给目标 Renderer；收到 `<bg_task_done>` 后**不再调**（已赋上的 `Renderer.sharedMaterial` 自动显示真实贴图）。
> - **主 agent**：报告里的 `material_path` 是"已放置"的证据，不是"请你放置"的指示，**不要再调**。
> - **例外**：用户明确要"换 Renderer / 再赋一个对象"时才再次调用。详见 [async-pattern §5.1](../../experience/templates/generator-async-pattern.md#51-place_assets_in_scene-调用规则)。

# Generate Surface Material in Unity 🪨

Generate PBR surface material assets in Unity using Huoshan SeeDream AI, from text prompts, reference images, or built-in texture pattern templates.
Output: seamless PNG texture (imported as **Default Texture**) + a ready-to-use **Unity `.mat` Material asset**, auto-saved to `Assets/TJGenerators/History/`.

支持 material type preset（金属/木头/石头等）、texture pattern 模板（条纹/蜂窝/裂纹等）、surface state 风格（崭新/做旧/脏污/潮湿/风化）共同引导生成方向。

## 🚦 执行四步（不要跳读外链）

1. 调 `generate_material` → 拿 `task_id` + `placeholder_path`（PNG）+ `placeholder_material_path`（`.mat`）
2. 立即 `place_assets_in_scene`（资产类型 `Material`，路径用 `placeholder_material_path`）→ 赋给目标 Renderer
3. **END RESPONSE TURN** — 不要 poll、不要 `query_material_status`、不要继续操作
4. 下一轮收到 `<bg_task_done>` → 读 `material_path` / `texture_path`（PNG 已原地覆盖，`.mat.mainTexture` 自动更新，已绑定的 Renderer 不需要重赋值，**不要再 place**）

**档位**：短任务 60–180 秒；120 秒内无通知才允许 `query_material_status` 一次。最多 **5 个**并发。完整 async 规则见 [generator-async-pattern](../../experience/templates/generator-async-pattern.md)。

## ⚠️ Skill 独有约束

1. **并发上限 5**——同时运行的 material 任务最多 5 个，超过会被拒绝。
2. **`pattern_id` 依赖本地模板**——pattern 模板是本地图片文件，**必须先**在 Unity Editor 通过菜单 **AI生成 → 开发 → 材质模板生成器** 预生成。模板不存在时 `pattern_id` 会被忽略，回退使用 `image_path`。
3. **至少要给一个输入**——`prompt`、`preset_id`、`pattern_id`、`image_path` 至少提供一个，否则会被拒绝；只给 `preset_id` 就够了。

## When to Use / NOT to Use

适用：3D 物体表面材质、环境纹理、建筑材质、可平铺贴图。

不适用：
- 2D sprite / 图标 / UI 图片 → `generate_sprite` / `generate_image`
- 通用纹理（无需 PBR `.mat`） → `generate_image`
- 天空盒 → `generate_skybox`

## 工具

所有工具通过 `execute_custom_tool` 调用。

### `generate_material`

```python
execute_custom_tool(
  tool_name="generate_material",
  parameters={
    "prompt": "rough iron plate with surface rust",   # 可选：自由文本
    "generator_id": "huoshan_seedream_material",      # 唯一可用 generator（默认）
    "image_path": "Assets/References/ref.png",        # 可选：参考图
    "preset_id": "metal",                             # 可选：材质类型预设（见表）
    "pattern_id": "horizontal_lines",                 # 可选：纹理图案模板（见表）
    "style_id": "aged",                               # 可选：表面状态风格（见表）
    "size": "2048x2048",                              # 可选：1K / 2K / 4K
    # output_path: 不建议指定，默认 Assets/TJGenerators/History/
  }
)
```

**Prompt 拼接公式**（重要）：发给 AI 的 prompt 由系统按以下顺序拼接

```
preset.prompt + ", " + style.prompt + ", " + your prompt
```

设了 `preset_id` 和 `style_id` 后，`prompt` 字段通常**只需补充细节**（特定颜色、特定纹理细节），不必从头写。

### 返回字段

- `task_id`
- `placeholder_path`：占位 PNG（1×1 灰色），**立即可用**
- `placeholder_material_path`：占位 `.mat`（Standard shader），**立即可用**——直接应用到物体
- `expected_texture_path` / `expected_material_path`：最终路径（与 placeholder 同路径）
- `prompt`：实际拼接发给 AI 的 prompt
- `estimated_wait_seconds` ≈ 90
- `notification_mode: "bg_task_done"`

提交失败时 `result["success"] == false`，读 `error_code` / `message`，**不要**poll。

### `<bg_task_done>` 独有字段

通用字段见模板。本 skill 额外字段：

| 字段 | 说明 |
|---|---|
| `texture_path` | 最终 PNG 纹理资产路径（== `placeholder_path`，原地覆盖） |
| `material_path` | 最终 `.mat` Material 资产路径（== `placeholder_material_path`） |
| `preview_url` | 预览 URL 或本地路径 |
| `generator_id` | 使用的生成器 |
| `prompt` | 实际拼接的 prompt |

### `query_material_status` / `list_material_tasks`

`query_material_status` 仅作 fallback（120 秒后单次）。返回字段同 `<bg_task_done>` payload，外加 `placeholder_path` / `placeholder_material_path`（仅 `generating` 时）。

`list_material_tasks` 返回当前 session 的所有 material 任务。

## 参数速查

| 参数 | 类型 | 默认 | 说明 |
|---|---|---|---|
| `generator_id` | string | `"huoshan_seedream_material"` | 唯一可用 |
| `prompt` | string | — | 自由文本（与 preset/style 拼接） |
| `preset_id` | string | — | 材质类型，见下表 |
| `pattern_id` | string | — | 纹理图案模板，**需先预生成** |
| `style_id` | string | — | 表面状态，见下表 |
| `image_path` | string | — | 参考图；`pattern_id` 模板存在时被覆盖 |
| `size` | string | `"2048x2048"` | `"1024x1024"` / `"2048x2048"` / `"4096x4096"` |
| `output_path` | string | — | 自定义路径（自动加 `.png`/`.mat` 后缀） |

## 材质类型预设 (`preset_id`)

> Preset 同时设置生成的 `.mat` 上的 `Metallic` / `Smoothness` 数值，无需手动调。

| ID | 名称 | 类别 | Metallic | Smoothness |
|----|------|------|----------|------------|
| `metal` | 金属 | 基础 | 0.9 | 0.8 |
| `wood` | 木头 | 基础 | 0 | 0.3 |
| `stone` | 石头 | 基础 | 0 | 0.1 |
| `fabric` | 布料 | 基础 | 0 | 0.3 |
| `leather` | 皮革 | 基础 | 0 | 0.3 |
| `concrete` | 混凝土 | 建筑 | 0 | 0.1 |
| `brick` | 砖块 | 建筑 | 0 | 0.1 |
| `tile` | 瓷砖 | 建筑 | 0 | 0.1 |
| `glass` | 玻璃 | 透明 | 0 | 0.95 |
| `ceramic` | 陶瓷 | 特殊 | 0 | 0.5 |
| `grass` | 草地 | 自然 | 0 | 0.5 |
| `sand` | 沙地 | 自然 | 0 | 0.5 |
| `snow` | 雪地 | 自然 | 0 | 0.5 |

## 纹理图案模板 (`pattern_id`)

> ⚠️ 必须先在 Unity Editor 用 **AI生成 → 开发 → 材质模板生成器** 预生成模板图。模板不存在时 `pattern_id` 被忽略。

| ID | 名称 | 描述 | 类别 |
|----|------|------|------|
| `uniform` | 均匀平滑 | 无方向性平滑表面 | 基础 |
| `horizontal_lines` | 水平条纹 | 水平方向条纹 | 条纹 |
| `vertical_lines` | 垂直条纹 | 垂直方向条纹 | 条纹 |
| `cross_hatch` | 交叉网格 | 交叉网格纹理 | 网格 |
| `diagonal` | 对角线 | 对角方向纹理 | 条纹 |
| `wave` | 波浪 | 波浪形纹理 | 曲线 |
| `noise` | 噪点粗糙 | 随机噪点纹理 | 随机 |
| `honeycomb` | 蜂窝 | 六边形蜂窝纹理 | 几何 |
| `brick_layout` | 砖块排列 | 砖块排列纹理 | 几何 |
| `scales` | 鳞片 | 鳞片状纹理 | 有机 |
| `cracks` | 裂纹 | 裂纹纹理 | 破损 |
| `woven` | 编织 | 编织纹理 | 织物 |

## 表面状态风格 (`style_id`)

| ID | 名称 | 描述 |
|----|------|------|
| `new` | 崭新 | 干净、无磨损 |
| `aged` | 做旧 | 有划痕、磨损 |
| `dirty` | 脏污 | 有污渍、灰尘 |
| `wet` | 潮湿 | 有水渍、湿润 |
| `weathered` | 风化 | 自然风化效果 |

## 输出尺寸 (`size`)

| 值 | 说明 |
|---|---|
| `"1024x1024"` | 1K — 小道具、移动端 |
| `"2048x2048"` | 2K — 标准质量 **(默认)** |
| `"4096x4096"` | 4K — 主美资产、近景表面 |

## 使用示例

### 按材质类型生成

```python
result = execute_custom_tool(
    tool_name="generate_material",
    parameters={
        "preset_id": "metal",
        "style_id": "aged",
        "prompt": "iron plate with surface rust"
    }
)
if not result.get("success", True):
    raise RuntimeError(f"[{result['error_code']}] {result['message']}")

task_id = result["task_id"]
placeholder_material_path = result["placeholder_material_path"]

# ✅ 立即用 place_assets_in_scene 把 .mat 应用到目标物体
# 然后 END RESPONSE TURN，等 bg_task_done 通知
```

### 按纹理图案生成

```python
parameters={
    "preset_id": "wood",
    "pattern_id": "horizontal_lines",   # 使用模板图控制结构
    "style_id": "new"
}
```

### 从参考图生成

```python
parameters={
    "image_path": "Assets/References/brick_photo.jpg",
    "preset_id": "brick",
    "style_id": "weathered"
}
```

### 并发批量（最多 5 个）

```python
task_ids = []
materials = [
    ("metal", "aged", "rusty iron"),
    ("wood",  "new",  "clean oak planks"),
    ("stone", "weathered", "mossy cobblestone"),
]
for preset, style, prompt in materials:
    result = execute_custom_tool(
        tool_name="generate_material",
        parameters={"preset_id": preset, "style_id": style, "prompt": prompt}
    )
    task_ids.append(result["task_id"])
    # ✅ 直接继续，不 poll

# END RESPONSE TURN — 每个 task 都会单独发 bg_task_done
```

## 放入场景

资产类型 **`Material`**，路径用 `placeholder_material_path` / `material_path`。`placement_instruction` 例：`"赋给 Floor 的材质"`、`"应用到 Wall_001"`。

规则见 [async-pattern §5 / §5.1](../../experience/templates/generator-async-pattern.md#5-placeholder-工作流适用于会返回-placeholder_path--prefab_output_path-的工具)。

## Prompt 写作指南

`preset_id` + `style_id` 通常已经覆盖大部分语义。`prompt` 字段**只补充细节**。

| 目标 | Prompt |
|---|---|
| 指定颜色 | `"dark grey slate with blue-grey tones"` |
| 表面细节 | `"fine grain wood with visible knots"` |
| 图案提示 | `"large irregular cobblestone blocks"` |
| 强制 seamless | `"seamless tileable, no visible seams"` |
| 综合 | `"rusty iron, orange and brown tones, heavy corrosion"` |

技巧：
- 优先让 `preset_id` + `style_id` 工作；`prompt` 只补 preset/style 表达不出的细节
- 平铺表面要明确加 `"seamless tileable"`
- `pattern_id` + `preset_id` 组合可同时控制结构与材质类型
- **不要描述物体**（如 "a sword"），只描述**表面本身**

## 故障排查

> 通用故障（配置缺失 / 任务卡住 / 状态异常 / 未登录）见 [generator-async-pattern §10](../../experience/templates/generator-async-pattern.md#10-通用故障排查)。

### Skill 独有问题

| 问题 | 原因 | 解决 |
|---|---|---|
| `At least one of 'prompt', 'preset_id', 'pattern_id', or 'image_path' must be provided` | 4 个输入都没给 | 至少给一个；只给 `preset_id` 也行 |
| `pattern_id` 没起效 | 模板图未预生成 | 在 Unity Editor 跑 **AI生成 → 开发 → 材质模板生成器** 一次；之后会回退到 `image_path` 或纯文本 |
| 材质长得不像目标类型 | preset/style/prompt 不匹配 | 设正确 `preset_id`（控类型）+ `style_id`（控状态），`prompt` 补颜色/纹理细节 |

### Domain reload 后 task 丢失

通用恢复流程见 [generator-async-pattern §6](../../experience/templates/generator-async-pattern.md#6-domain-reload-recovery)。本 skill 完成态阈值：

- PNG ≥ 100 KB → 真实纹理已就绪
- `.mat` 文件本身较小（约几 KB）——通过对应 PNG 大小判定即可

可用 `glob("Assets/TJGenerators/History/*.png")` + 文件大小恢复。

---

**Task ID Format**：`material_{counter}_{timestamp}`

**Notes**：
- PNG 自动导入为 `TextureImporterType.Default`；`.mat` 的 shader 默认为 Standard
- 自动应用 `TuanjieAI` 标签
- **并发上限 5**——批量生成超过 5 个时分批做
- 需 Unity Editor 在线运行；消耗 AI 服务额度
