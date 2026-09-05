---
name: unity-tripo-texture-model
description: Re-texture an existing 3D model in Unity using Tripo texture-model API. Use this skill whenever the user wants to re-texture, re-skin, or change the texture/PBR of an already-generated 3D model — e.g., "给这个模型换个纹理", "重新生成贴图", "change the texture of this model", "re-texture my 3D model", "换个材质风格". Trigger when the user has an existing 3D model (from generate_3d_model_by_tripo_p1 or generate_3d_model_by_rodin) and wants to modify its textures. Requires either a prior task ID or a direct model URL.
---

> ⚠️ **执行约束**
> - **主 agent**：无 `execute_custom_tool` 权限，必须 `task(subagent_name="3d-model-generator", ...)` 委托，不要 `activate_skill` 后自己调。
> - **子代理（本文档主要读者）**：有权限，按下方 `execute_custom_tool(...)` 示例执行。

> ⛔ **`place_assets_in_scene` 调用规则**
> - **调用方式**：`activate_skill("unity-place-assets-in-scene")` → 按 §4a Prefab 模板用 `execute_csharp_script` 跑 `PrefabUtility.InstantiatePrefab`（**不是** `execute_custom_tool`，**不要** `unity_gameobject` 放 Prefab）。
> - **子代理**：提交后**立即调一次**放占位 Prefab；收到 `<bg_task_done>` 后**不再调**（Cube 子节点自动覆盖为真实模型）。
> - **主 agent**：报告里的 `prefab_path` 是"已放置"的证据，不是"请你放置"的指示，**不要再调**。

# Re-texture 3D Model in Unity 🎨

对已有 3D 模型重新生成纹理/PBR 贴图。输入：已有模型的 task ID 或直接模型 URL。输出：重新贴图后的 3D 模型 + 自动生成的 Prefab，保存到 `Assets/TJGenerators/History/`。

## 🚦 执行四步（不要跳读外链）

1. 调 `generate_texture_model_by_tripo` → 拿 `task_id` + `prefab_output_path`
2. 立即 `place_assets_in_scene`（资产类型 `Prefab`，路径用 `prefab_output_path`）→ 场景出现 Cube 占位
3. **END RESPONSE TURN** — 不要 poll、不要 `query_texture_model_status_by_tripo`、不要继续操作
4. 下一轮收到 `<bg_task_done>` → 读 `model_path` / `prefab_path`（Cube 子节点已原地替换为真实模型，**不要再 place**）

**档位**：长任务 3–10 分钟；300 秒内无通知才允许 `query_texture_model_status_by_tripo` 一次。完整 async 规则见 [generator-async-pattern](../../experience/templates/generator-async-pattern.md)。

## ⚠️ Skill 独有约束

1. **必须提供模型来源**——`original_model_task_id`（来自先前生成任务的 `backend_task_id` 或 `task_id`）或 `url`（直接模型文件 URL），二选一。
2. **纹理提示词可选**——`texture_prompt_text`、`texture_prompt_image`、`texture_prompt_style_image` 可全部省略，此时后端按原模型几何体自动生成纹理。
3. **图片字段处理**——`texture_prompt_image` 和 `texture_prompt_style_image` 可以是：
   - 本地文件路径（`Assets/...` 或绝对路径）——工具自动读取并编码为 base64
   - URL（`http://...` / `https://...`）——直接传递
   - Data URI（`data:image/...`）——直接传递
4. **占位 Prefab 是 Cube 子节点**——生成完成后 Cube 被替换为真实 model 子节点。**不要**把场景里的实例当 Cube 删掉重建。

## 工具

### `generate_texture_model_by_tripo`

启动 Tripo 纹理模型重贴图任务。

**参数：**

| 参数 | 类型 | 必填 | 默认 | 说明 |
|------|------|------|------|------|
| `original_model_task_id` | string | 二选一 | — | 先前生成任务的 `backend_task_id`（从 `generate_3d_model_by_tripo_p1` 或 `generate_3d_model_by_rodin` 返回） |
| `url` | string | 二选一 | — | 直接模型文件 URL（与 `original_model_task_id` 互斥） |
| `texture_prompt_text` | string | 否 | — | 纹理生成文本提示词 |
| `texture_prompt_image` | string | 否 | — | 纹理引导图片（本地路径或 URL） |
| `texture_prompt_style_image` | string | 否 | — | 风格引导图片（本地路径或 URL） |
| `model_version` | string | 否 | `v2.5-20250123` | 模型版本：`v2.5-20250123` / `v3.0-20250812` |
| `texture` | bool | 否 | true | 是否生成纹理 |
| `pbr` | bool | 否 | true | 是否生成 PBR 材质 |
| `bake` | bool | 否 | false | 是否烘焙 |
| `with_fbx` | bool | 否 | true | 完成后转换为 FBX 格式 |
| `texture_seed` | int | 否 | — | 纹理种子（用于可复现结果） |
| `texture_quality` | string | 否 | `standard` | `standard` / `detailed` |
| `texture_alignment` | string | 否 | — | `original_image` / `geometry` |
| `compress` | string | 否 | — | 例如 `geometry` |
| `prefab_output_path` | string | 否 | 自动生成 | 输出 prefab 路径（`.prefab` 自动添加） |
| `force_overwrite` | bool | 否 | false | 覆盖同名 prefab |
| `session_id` | string | 否 | — | 为占位符 prefab 添加 Session 标签 |

**返回（成功）：**

```json
{
  "success": true,
  "task_id": "texture_model_1_...",
  "backend_task_id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "status": "submitted",
  "generator_id": "tripo-texture-model",
  "model_version": "v2.5-20250123",
  "prefab_output_path": "Assets/TJGenerators/History/TextureModel.prefab",
  "notification_mode": "bg_task_done"
}
```

**返回（失败）：**

```json
{ "success": false, "error_code": "AUTH_REQUIRED", "message": "Not logged in..." }
```

调用前检查 `result["success"]`。若 `false`，立即上报错误，**不要**继续轮询。

---

### `query_texture_model_status_by_tripo`

查询任务状态（**fallback only，仅一次**）。

**参数：**

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `task_id` | string | 是 | `generate_texture_model_by_tripo` 返回的 `task_id` |

**状态值：**

| Status | 含义 |
|--------|------|
| `initializing` | 任务已创建，等待后端任务 ID |
| `generating` | 后端生成中（进度 0–100%） |
| `recovering` | domain reload 后自动恢复中——**等 `<bg_task_done>` 通知**,不要重复 query |
| `completed` | 完成，模型已下载并绑定到 prefab |
| `failed` | 生成失败（查看 `error` 字段） |

**返回（完成）：**

```json
{
  "success": true,
  "task_id": "texture_model_1_...",
  "status": "completed",
  "progress": 100,
  "prefab_path": "Assets/TJGenerators/History/TextureModel.prefab",
  "model_path": "Assets/TJGenerators/History/TextureModel_model/TextureModel.fbx",
  "preview_url": "https://example.com/preview.png"
}
```

---

### `list_texture_model_tasks_by_tripo`

列出当前 Unity Editor session 内的所有纹理模型任务。

**参数：** 无

---

## 使用示例

### 基本用法——用先前任务 ID 重贴图

```python
result = execute_custom_tool(
    tool_name="generate_texture_model_by_tripo",
    parameters={
        "original_model_task_id": "abc123-def456-...",  # 来自先前 generate_3d_model_by_tripo_p1
        "texture_prompt_text": "weathered stone texture, mossy and ancient"
    }
)
if not result.get("success", True):
    raise RuntimeError(f"[{result['error_code']}] {result['message']}")

task_id = result["task_id"]
prefab_output_path = result["prefab_output_path"]

# ✅ 立即用 place_assets_in_scene 把 prefab_output_path 应用为 Prefab
# 然后 END RESPONSE TURN，等 bg_task_done 通知（3–10 分钟）
```

### 用模型 URL 重贴图

```python
parameters={
    "url": "https://example.com/models/character.glb",
    "texture_prompt_text": "cyberpunk neon style, metallic chrome with glowing accents",
    "texture_quality": "detailed",
    "pbr": True
}
```

### 带纹理引导图

```python
parameters={
    "original_model_task_id": "abc123-def456-...",
    "texture_prompt_image": "Assets/Textures/desired_texture.png",
    "texture_prompt_style_image": "Assets/Textures/style_reference.png"
}
```

## `<bg_task_done>` 通知字段

通用字段见模板。本 skill 额外字段：

| 字段 | 说明 |
|---|---|
| `model_path` | 最终 3D 模型路径（.fbx 或其他格式） |
| `prefab_path` | 最终 Prefab 路径（== 提交时的 `prefab_output_path`） |
| `preview_url` | 渲染预览缩略图 URL（可能为空） |
| `generator_type` | `"tripo-texture-model"` |
| `prompt` | 纹理提示词或描述 |

## 放入场景

资产类型 **`Prefab`**，路径用 `prefab_output_path`。提交后立即调一次（里面是 Cube 占位）；通知到达后**不要**再调——Cube 子节点自动被真实模型替换。

## 故障排查

> 通用故障（配置缺失 / 任务卡住 / 状态异常 / 未登录）见 [generator-async-pattern §10](../../experience/templates/generator-async-pattern.md#10-通用故障排查)。

| 问题 | 原因 | 解决 |
|---|---|---|
| `original_model_task_id` 和 `url` 都未提供 | 缺少模型来源 | 至少提供一个 |
| 状态变 `recovering` | domain reload 后自动恢复 | 等 `<bg_task_done>` 通知，不要重复 query |
| 纹理效果不理想 | 提示词不够具体 | 添加 `texture_prompt_text` 或 `texture_prompt_image` 引导 |

**Task ID Format**：`texture_model_{counter}_{timestamp}`

**Notes**：
- 输出模型由 Unity 原生 FBX / OBJ 导入；Prefab 自动绑定模型为子节点
- 长任务（3–10 分钟），需 Unity Editor 一直在线
- domain reload 任务自动恢复
- 消耗 AI 服务额度
