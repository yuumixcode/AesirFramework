---
name: unity-image-upscaling
description: Upscale images in Unity using AI super-resolution (Real-ESRGAN). Supports 1x-8x upscaling with multiple model variants. Use this skill whenever the user wants to enlarge, enhance, or upscale an existing image — e.g. "放大图片", "提高分辨率", "upscale this texture", "enhance image quality", "2x this image". Trigger proactively for any image resolution enhancement or super-resolution request in Unity. Do NOT use for generating new images (use generate_image) or generating sprites (use generate_sprite).
---

> ⚠️ **执行约束**
> - **主 agent**：无 `execute_custom_tool` 权限，必须 `task(subagent_name="image-upscaler", ...)` 委托，不要 `activate_skill` 后自己调。
> - **子代理（本文档主要读者）**：有权限，按下方 `execute_custom_tool(...)` 示例执行。

> ⛔ **`place_assets_in_scene` 调用规则**（本 skill **有 placeholder**）
> - **调用方式**：`activate_skill("unity-place-assets-in-scene")` → 按 §4i Texture2D 模板用 `execute_csharp_script` 赋给 `Material.mainTexture` 或建 `RawImage`（**不是** `execute_custom_tool`）。
> - **子代理**：提交后**立即调一次**把 `placeholder_path` 放到场景（如有 `target_object`）；收到 `<bg_task_done>` 后**不再调**（已原地覆盖）。
> - **主 agent**：报告里的 `image_path` 是"已放置"的证据，不是"请你放置"的指示，**不要再调**。
> - **例外**：用户明确要"换位置 / 再加一个实例"时才再次调用。详见 [async-pattern §5.1](../../experience/templates/generator-async-pattern.md#51-place_assets_in_scene-调用规则)。

# Upscale Image in Unity 🔍

使用 AI 超分辨率（Real-ESRGAN）放大图片。
Output: 放大后的 PNG/JPEG，自动保存到 `Assets/TJGenerators/History/`。

支持 1–8 倍放大，多种 ESRGAN 模型变体。

## 🚦 执行流程（不要跳读外链）

> ⚠️ **本 skill 有 placeholder**——提交后立即返回 1×1 灰色占位 PNG，可放置到场景。

1. 调 `upscale_image`（`image_path` = 本地图片路径）→ C# host 自动上传到 TOS → 拿 `task_id` + `placeholder_path`
2. **END RESPONSE TURN** — 不要 poll、不要 `query_upscale_image_status`、不要继续操作
3. 下一轮收到 `<bg_task_done>` → 读 `image_path`（已原地覆盖）→ 如有 `target_object` 调一次 `place_assets_in_scene`

**档位**：短任务 10–60 秒；120 秒内无通知才允许 `query_upscale_image_status` 一次。完整 async 规则见 [generator-async-pattern](../../experience/templates/generator-async-pattern.md)。

## ⚠️ Skill 独有约束

1. **`image_path` 是本地路径**——C# host 自动上传到 CDN 并提交给后端，与 `generate_image` 的 `image_path` 模式一致。
2. **有 placeholder**——返回 `placeholder_path`（1×1 灰色 PNG），可立即放置到场景。完成时原地覆盖。
3. **`scale` 范围 1–8**——超出范围会被钳制到默认值 4。
4. **模型自动选择**——不传 `model` 时，后端按 `scale` 自动选择：`scale<=2` → `RealESRGAN_x2plus`；`scale>2` → `RealESRGAN_x4plus`。
5. **anime/2D 推荐 anime 模型**——二次元/2D 游戏美术使用 `RealESRGAN_x4plus_anime_6B` 效果更好。
6. **并发上限 5**——同时运行的 upscale 任务最多 5 个。

## When to Use / NOT to Use

适用：放大低分辨率图片、提高纹理质量、增强截图清晰度、放大 AI 生成图片。

不适用：
- 生成新图片 → `generate_image`
- 生成 2D 精灵 → `generate_sprite`
- 生成 PBR 材质 → `generate_material`
- 视频放大 → 不支持

## 工具

所有工具通过 `execute_custom_tool` 调用。

### `upscale_image`

```python
execute_custom_tool(
  tool_name="upscale_image",
  parameters={
    "image_path": "Assets/Textures/low_res.png",  # Required：本地图片路径
    "scale": 4,              # 可选：1–8，默认 4
    "model": "",              # 可选：模型变体（见下表），省略则自动选择
    "face_enhance": False,    # 可选：人脸增强，默认 false
    "output_format": "png",   # 可选："png" / "jpeg"，默认 "png"
    # output_path: 不建议指定，默认 Assets/TJGenerators/History/
  }
)
```

### 返回字段

- `task_id`
- `image_path`：回传输入的本地路径
- `placeholder_path`：1×1 灰色占位 PNG，**立即可用**
- `scale`：回传放大倍数
- `model`：回传使用的模型
- `notification_mode: "bg_task_done"`

提交失败时 `result["success"] == false`，读 `error_code` / `message`，**不要**poll。

### `<bg_task_done>` 独有字段

通用字段见模板。本 skill 额外字段：

| 字段 | 说明 |
|---|---|
| `image_path` | 放大后图片本地路径（已下载到 `Assets/TJGenerators/History/`） |
| `preview_url` | 预览 URL |
| `scale` | 实际放大倍数 |
| `model` | 使用的 ESRGAN 模型 |

### `query_upscale_image_status` / `list_upscale_image_tasks`

`query_upscale_image_status` 仅作 fallback（120 秒后单次）。返回字段同 `<bg_task_done>` payload。

`list_upscale_image_tasks` 返回当前 session 的所有 upscale 任务。

## 参数速查

| 参数 | 类型 | 默认 | 说明 |
|---|---|---|---|
| `image_path` | string | **required** | 本地图片路径（C# host 自动上传） |
| `scale` | int | `4` | 1–8 |
| `model` | string | 自动 | ESRGAN 模型变体（见下表） |
| `face_enhance` | bool | `false` | 人脸增强 |
| `output_format` | string | `"png"` | `"png"` / `"jpeg"` |
| `output_path` | string | — | 不建议指定 |

## 模型选择 (`model`)

| 模型 | 适用场景 | 说明 |
|---|---|---|
| `RealESRGAN_x4plus` | 通用 4x 放大（照片、3D 渲染） | `scale>2` 时的默认选择 |
| `RealESRGAN_x2plus` | 通用 2x 放大 | `scale<=2` 时的默认选择 |
| `RealESRGAN_x4plus_anime_6B` | **anime / 2D 游戏美术** | 推荐 2D 游戏、动漫风格图片 |
| `RealESRGAN_x4_v3` | 通用 4x（v3 版本） | 新版通用模型 |
| `RealESRGAN_x4_wdn_v3` | 去噪 4x（v3 版本） | 适合有噪点的图片 |
| `RealESRGAN_x4_anime_v3` | anime 4x（v3 版本） | 新版 anime 模型 |

### 模型选择决策

| 场景 | 推荐 model | 推荐 scale |
|---|---|---|
| 通用照片/3D 渲染放大 | （省略，自动选 x4plus） | 4 |
| 2x 轻度放大 | （省略，自动选 x2plus） | 2 |
| Anime / 2D 游戏美术 | `RealESRGAN_x4plus_anime_6B` | 4 |
| 有噪点的旧图 | `RealESRGAN_x4_wdn_v3` | 4 |
| 8x 极限放大 | （省略，自动选 x4plus） | 8 |
| 人脸照片 | （省略）+ `face_enhance: true` | 4 |

## 使用示例

### 本地图片放大 4 倍

```python
result = execute_custom_tool(
    tool_name="upscale_image",
    parameters={
        "image_path": "Assets/Textures/low_res.png",
        "scale": 4
    }
)
if not result.get("success", True):
    raise RuntimeError(f"[{result['error_code']}] {result['message']}")

task_id = result["task_id"]
# ✅ END RESPONSE TURN — 等 bg_task_done
```

### Anime 图片放大

```python
result = execute_custom_tool(
    tool_name="upscale_image",
    parameters={
        "image_path": "Assets/Sprites/character.png",
        "scale": 4,
        "model": "RealESRGAN_x4plus_anime_6B"
    }
)
```

### 2x 轻度放大

```python
result = execute_custom_tool(
    tool_name="upscale_image",
    parameters={
        "image_path": "Assets/Textures/icon.png",
        "scale": 2
    }
)
```

### 人脸照片增强

```python
result = execute_custom_tool(
    tool_name="upscale_image",
    parameters={
        "image_path": "Assets/Textures/portrait.png",
        "scale": 4,
        "face_enhance": True
    }
)
```

## 放入场景

放大后的图片可作为 `Texture2D` 放入场景。

资产类型 **`Texture2D`**，路径用 `image_path`。规则见 [async-pattern §5 / §5.1](../../experience/templates/generator-async-pattern.md#5-placeholder-工作流适用于会返回-placeholder_path--prefab_output_path-的工具)。

> 常见用例是替换原有低分辨率贴图——用 `place_assets_in_scene` 把新 `image_path` 赋给目标 `Material.mainTexture`。

## 故障排查

### Skill 独有问题

> 通用故障（配置缺失 / 任务卡住 / 状态异常 / 未登录）见 [generator-async-pattern §10](../../experience/templates/generator-async-pattern.md#10-通用故障排查)。

| 问题 | 原因 | 解决 |
|---|---|---|
| 放大后图片模糊 | scale 太高或模型不匹配 | anime 图片用 `RealESRGAN_x4plus_anime_6B`；照片用默认模型 |
| 放大后文件太大 | 8x 放大 4K 图会非常大 | 用 `output_format: "jpeg"` 或降低 scale |
| 人脸失真 | 未启用 face_enhance | 设 `face_enhance: true` |
| anime 线条被平滑 | 用了非 anime 模型 | 改用 `RealESRGAN_x4plus_anime_6B` |

### Domain reload 后 task 丢失

通用恢复流程见 [generator-async-pattern §6](../../experience/templates/generator-async-pattern.md#6-domain-reload-recovery)。本 skill 完成态阈值：

- PNG < 5 KB → 仍是 placeholder 或任务丢失
- PNG ≥ 100 KB → 真实图片已就绪（放大后通常较大）

可用 `glob("Assets/TJGenerators/History/*.png")` + 文件大小恢复。

---

**Task ID Format**：`upscale_{counter}_{timestamp}`

**Notes**：
- 放大后图片自动导入为 `TextureImporterType.Default`
- 自动应用 `TuanjieAI` 标签
- **并发上限 5**
- 需 Unity Editor 在线运行；消耗 AI 服务额度
- 本工具基于 fal.ai Real-ESRGAN
