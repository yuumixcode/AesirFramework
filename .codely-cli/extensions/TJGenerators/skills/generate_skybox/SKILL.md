---
name: unity-skybox-generation
description: Generate skybox (Cubemap) assets in Unity using AI via text descriptions or reference images. Use this skill whenever the user wants to create a skybox, environment background, sky texture, HDRI-like environment, or scene atmosphere using AI — even if they just say "给场景加个天空盒", "生成背景天空", "make a sky", or "create an environment". Trigger proactively when context suggests setting up a scene environment.
---

> ⚠️ **执行约束**
> - **主 agent**：无 `execute_custom_tool` 权限，必须 `task(subagent_name="skybox-generator", ...)` 委托，不要 `activate_skill` 后自己调。
> - **子代理（本文档主要读者）**：有权限，按下方 `execute_custom_tool(...)` 示例执行。

> ⛔ **`place_assets_in_scene` 调用规则**
> - **调用方式**：`activate_skill("unity-place-assets-in-scene")` → 按 §4f Cubemap / 天空盒 模板用 `execute_csharp_script` 写入 `RenderSettings.skybox`（**不是** `execute_custom_tool`）。
> - **子代理**：提交后**立即调一次**把占位 Skybox `.mat` 设为 `RenderSettings.skybox`；收到 `<bg_task_done>` 后**不再调**（Cubemap/`.mat` 原地覆盖，场景天空盒自动刷新）。
> - **主 agent**：报告里的 `skybox_material_path` 是"已放置"的证据，不是"请你放置"的指示，**不要再调**。
> - **例外**：用户明确要"换场景再赋一次"时才再次调用。详见 [async-pattern §5.1](../../experience/templates/generator-async-pattern.md#51-place_assets_in_scene-调用规则)。

# Generate Skybox in Unity 🌅

Generate Cubemap skybox assets in Unity using Rodin Skybox AI, from text prompts or reference images.
Output: PNG imported as **TextureCube (Cubemap)** + 一个绑好 Cubemap 的 Skybox `.mat` Material。资产保存在 `Assets/TJGenerators/History/`。

## 🚦 执行四步（不要跳读外链）

1. 调 `generate_skybox` → 拿 `task_id` + `placeholder_path`（PNG）+ `placeholder_material_path`（`.mat`）
2. 立即 `place_assets_in_scene`（资产类型 **`Cubemap Skybox`**，路径用 `placeholder_material_path`）→ 写入 `RenderSettings.skybox`
3. **END RESPONSE TURN** — 不要 poll、不要 `query_skybox_status`、不要继续操作
4. 下一轮收到 `<bg_task_done>` → PNG 与 `.mat` 已原地覆盖，已写入的 `RenderSettings.skybox` 引用自动更新（**不要再 place**）

**档位**：短任务 60–180 秒；120 秒内无通知才允许 `query_skybox_status` 一次。完整 async 规则见 [generator-async-pattern](../../experience/templates/generator-async-pattern.md)。

## ⚠️ Skill 独有约束

1. **至少要给一个输入**——`prompt` 或 `image_path` 至少一个。
2. **应用对象是 RenderSettings.skybox，不是 Renderer**——天空盒不挂物体上。把 `placeholder_material_path` 交给 `place_assets_in_scene`，资产类型用 **`Cubemap Skybox`**（不是 `Material`），它会执行 `RenderSettings.skybox = material` + `DynamicGI.UpdateEnvironment()`。
3. **传 `.mat` 路径，不传 `.png`**——`place_assets_in_scene` 期望的是 `placeholder_material_path` 或最终的 `material_path`，不是 `texture_path`。
4. **TextureImporter shape 必须是 Cube**——若 Cubemap 出现黑色或缺失，先在 Inspector 确认 shape = `Cube`，必要时 Reimport。

## When to Use / NOT to Use

适用：场景天空、环境背景、HDRI 风格氛围、室内/室外大气背景。

不适用：
- 3D 模型 → `generate_3d_model`
- 2D 精灵 / 通用纹理 → `generate_sprite` / `generate_image`
- 动画角色 / 绑骨 / 动作 → `animated-character-generator`
- 表面材质（PBR） → `generate_material`

## 工具

所有工具通过 `execute_custom_tool` 调用。

### `generate_skybox`

```python
execute_custom_tool(
  tool_name="generate_skybox",
  parameters={
    "generator_id": "rodin-skybox",                # 唯一可用 generator（默认）
    "prompt": "sunset over ocean, dramatic orange sky",  # 至少给一个
    "image_path": "Assets/Reference/concept.png",  # 可选：参考图
    "resolution": "2048",                          # "512" | "1024" | "2048" | "4096"，默认 "2048"
    "high_res": False,                             # 可选：高清模式
    # output_path: 不建议指定，默认 Assets/TJGenerators/History/
  }
)
```

### 返回字段

- `task_id`
- `placeholder_path`：占位 Cubemap PNG（1×1 灰），**立即可用**
- `placeholder_material_path`：占位 Skybox `.mat`，**立即可用**——交给 `place_assets_in_scene`
- `estimated_wait_seconds` ≈ 90
- `notification_mode: "bg_task_done"`

提交失败时 `result["success"] == false`，读 `error_code` / `message`，**不要**poll。

### `<bg_task_done>` 独有字段

通用字段见模板。本 skill 额外字段：

| 字段 | 说明 |
|---|---|
| `texture_path` | 最终 Cubemap PNG 资产路径（== `placeholder_path`） |
| `material_path` | 最终 Skybox `.mat` 资产路径（== `placeholder_material_path`） |
| `preview_url` | 预览 URL 或本地路径 |
| `generator_id` | 使用的生成器 |
| `prompt` | 原始 prompt |
| `image_path` | 原始参考图（如有） |

### `query_skybox_status` / `list_skybox_tasks`

`query_skybox_status` 仅作 fallback（120 秒后单次）。返回字段同 `<bg_task_done>` payload，外加 `placeholder_path` / `placeholder_material_path`（仅 `generating` 时）。

`list_skybox_tasks` 返回当前 session 的所有 skybox 任务。

## 参数速查

| 参数 | 类型 | 默认 | 说明 |
|---|---|---|---|
| `generator_id` | string | `"rodin-skybox"` | 唯一可用 |
| `prompt` | string | — | 文本描述（与 `image_path` 至少给一个） |
| `image_path` | string | — | 参考图（与 `prompt` 至少给一个） |
| `resolution` | string | `"2048"` | `"512"` / `"1024"` / `"2048"` / `"4096"` |
| `high_res` | bool | `false` | 高清模式 |
| `output_path` | string | — | 自定义路径（自动加 `.png` 后缀） |

> 分辨率越高质量越好但更慢、文件更大。`"2048"` 适合多数游戏场景；`"4096"` 留给 cinematic / hero 环境。

## 使用示例

### 文本生成

```python
result = execute_custom_tool(
    tool_name="generate_skybox",
    parameters={"prompt": "night sky with stars, milky way visible, deep blue"}
)
if not result.get("success", True):
    raise RuntimeError(f"[{result['error_code']}] {result['message']}")

task_id = result["task_id"]
placeholder_material_path = result["placeholder_material_path"]

# ✅ 立即用 place_assets_in_scene 把 placeholder_material_path 应用为 Cubemap Skybox
# 然后 END RESPONSE TURN，等 bg_task_done 通知
```

### 参考图生成

```python
parameters={
    "image_path": "Assets/ConceptArt/environment_concept.png",
    "resolution": "4096"
}
```

### 自定义路径 + 高清

```python
parameters={
    "prompt": "fantasy sunset with purple clouds and twin moons",
    "output_path": "Assets/Environments/FantasySky",
    "resolution": "2048",
    "high_res": True
}
```

## 放入场景

资产类型 **`Cubemap Skybox`**（不是 `Material`、不是 `Cubemap`），路径用 `placeholder_material_path` / `material_path`。

Scene side-effect：`RenderSettings.skybox = material; DynamicGI.UpdateEnvironment();`，不挂在任何 GameObject 上——天空盒是场景级别的环境设置。

规则见 [async-pattern §5 / §5.1](../../experience/templates/generator-async-pattern.md#5-placeholder-工作流适用于会返回-placeholder_path--prefab_output_path-的工具)。

## Prompt 写作指南

好的天空盒 prompt 描述**整体氛围**，不描述具体前景物体。

| 目标 | Prompt |
|---|---|
| 白天户外 | `"clear blue sky, white fluffy clouds, bright sunny day"` |
| 日落 | `"dramatic orange and pink sunset, golden hour lighting"` |
| 夜晚 | `"starry night sky, milky way, deep blue, moonlit"` |
| 暴风雨 | `"dark stormy sky, heavy clouds, lightning, dramatic lighting"` |
| 奇幻 | `"fantasy twin moons, purple nebula sky, magical atmosphere"` |
| 科幻 | `"alien planet sky, red atmosphere, distant ringed planet"` |
| 室内（影棚） | `"neutral gray studio background, soft even lighting, HDRI"` |

技巧：
- 描述光照质量：`soft` / `dramatic` / `harsh` / `overcast`
- 描述时间：`dawn` / `dusk` / `midnight` / `noon`
- 描述情绪：`peaceful` / `ominous` / `epic` / `serene`
- **不要描述前景物体**——只描述天空与氛围

## 故障排查

### Skill 独有问题

> 通用故障（配置缺失 / 任务卡住 / 状态异常 / 未登录）见 [generator-async-pattern §10](../../experience/templates/generator-async-pattern.md#10-通用故障排查)。

| 问题 | 原因 | 解决 |
|---|---|---|
| Cubemap 显示为黑色或缺失 | TextureImporter shape 不对 | 在 Inspector 确认 shape = `Cube`；右键资产 → Reimport |
| Skybox 应用了但场景光照没刷新 | 漏调 `DynamicGI.UpdateEnvironment()` | 用 `place_assets_in_scene` 的 `Cubemap Skybox` 类型自动包含此调用 |
| 输出质量差 | prompt 太模糊 / 分辨率不足 | 写更详细 prompt；试 `resolution: "4096"` + `high_res: true`；提供 `image_path` 参考图 |

### Domain reload 后 task 丢失

通用恢复流程见 [generator-async-pattern §6](../../experience/templates/generator-async-pattern.md#6-domain-reload-recovery)。本 skill 完成态阈值：

- PNG ≥ 500 KB → 真实 Cubemap 已就绪（512px 起步即较大）

可用 `glob("Assets/TJGenerators/History/*.png")` + 文件大小恢复。

---

**Task ID Format**：`skybox_{counter}_{timestamp}`

**Notes**：
- PNG 自动导入为 `TextureCube`（不是 `Default`）；`.mat` 用 Skybox shader
- 自动应用 `TuanjieAI` 标签
- 需 Unity Editor 在线运行；消耗 AI 服务额度
