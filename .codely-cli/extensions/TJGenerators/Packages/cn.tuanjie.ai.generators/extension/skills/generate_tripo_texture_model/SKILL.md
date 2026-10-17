---
name: unity-tripo-texture-model
description: Re-texture an existing 3D model in Unity using the MCP backend (tripo_texture_model) plus the Unity import tool. Use this skill whenever the user wants to re-texture, re-skin, or change the texture/PBR of an already-generated 3D model — e.g., "给这个模型换个纹理", "重新生成贴图", "change the texture of this model", "re-texture my 3D model", "换个材质风格". Trigger when the user has an existing 3D model (from MCP generate_3d_model) and wants to modify its textures. Requires a prior Tripo task ID or a model URL.
---

> **执行前读取**：[共享执行约定](../../experience/templates/generator-async-pattern.md)。
> 主 agent 按需委托 `general-purpose`；已在子代理中则直接执行本 skill，不再委托。已提交的阶段从原任务续接。

> ⛔ **场景放置规则**
> - **调用方式**：`activate_skill("unity-place-assets-in-scene")` → 按 §4a Prefab 模板用 `exec_editor_script` 跑 `PrefabUtility.InstantiatePrefab`（**不是** `execute_custom_tool`，**不要** `unity_gameobject` 放 Prefab）。
> - **子代理**：第二段提交后**立即调一次**放占位 Prefab；收到 `<bg_task_done>` 后**不再调**（占位子节点自动替换为重贴图模型）。
> - **主 agent**：报告里的 `prefab_path` 是"已放置"的证据，**不要再调**。

# Re-texture 3D Model in Unity 🎨（MCP 生成 + Unity 导入）

对已有 3D 模型重新生成纹理/PBR 贴图。**两段式**：

- **第一段·生成（MCP）**：`tripo_texture_model` 提交重贴图 → 后台等待 → `check_task` 拿重贴图模型 URL
- **第二段·导入（Unity）**：`import_3d_model_from_url`（`provider=tripo-texture`）下载导入 + 后处理 + Prefab 绑定

输出：重新贴图后的 3D 模型 + 自动生成的 Prefab，保存到 `Assets/TJGenerators/History/`。

## 🚦 执行五步

1. 调 MCP `tripo_texture_model` → 拿 `task_id` → **在当前子代理内等待，不结束任务**
2. 下一轮收到生成结果 → 拿重贴图模型 URL（`.fbx`，开 `with_fbx=true` 时）
3. 调 `import_3d_model_from_url`（`provider=tripo-texture`）→ 拿 `task_id` + `prefab_output_path` → 立即 `activate_skill("unity-place-assets-in-scene")`，按 skill 用 `exec_editor_script` 放占位 Prefab → **在当前子代理内等待，不结束任务**
4. 下一轮收到 `<bg_task_done>` → 读 `model_path` / `prefab_path`（占位子节点已替换，**不要再 place**）
5. 报告结果

**档位**：第一段 3–10 分钟；第二段 ~1–2 分钟。超过预估时长才允许 `check_task`（第一段，同一 task_id 一次）/ `query_local_task`（第二段，一次）。

## ⚠️ 独有约束

1. **模型来源二选一**：
   - `original_model_task_id`：**仅限 Tripo 自己的 generate_3d_model 任务 ID**——Rodin/混元来源的模型会被 400 拒绝，一律改用 `url`
   - `url`：直接模型文件 URL。本地/工程内模型先 `file_upload(file_path)` 上传拿永久 CDN `file_url` 再传入；**不要**传 `Assets/...` 路径
2. **纹理提示词全部可选**：`texture_prompt_text` / `texture_prompt_image` / `texture_prompt_style_image` 可全部省略，此时按原模型几何体自动生成纹理
3. **引导图必须是 CDN URL**：本地图片先 `file_upload` 上传，把 `file_url` 传给 `texture_prompt_image` / `texture_prompt_style_image`
4. **model_version 只有两个合法值**：`v2.5-20250123` / `v3.0-20250812`（默认）；**不要**传 `P1-...` / `v3.1-...` 等生成模型版本，会被 400 拒绝
5. **占位 Prefab 是 Cube 子节点**——完成后自动替换，**不要**删掉重建

## 第一段：MCP `tripo_texture_model`

**参数：**

| 参数 | 类型 | 必填 | 默认 | 说明 |
|------|------|------|------|------|
| `original_model_task_id` | string | 二选一 | — | 先前 **Tripo** `generate_3d_model` 任务的 task_id |
| `url` | string | 二选一 | — | 直接模型文件 URL（非 Tripo 来源或本地文件用这个） |
| `texture_prompt_text` | string | 否 | — | 纹理生成文本提示词 |
| `texture_prompt_image` | string | 否 | — | 纹理引导图 URL（先 `file_upload`） |
| `texture_prompt_style_image` | string | 否 | — | 风格引导图 URL（先 `file_upload`） |
| `model_version` | string | 否 | `v3.0-20250812` | `v2.5-20250123` / `v3.0-20250812` |
| `texture` | bool | 否 | true | 是否生成纹理 |
| `pbr` | bool | 否 | true | 是否生成 PBR 材质 |
| `bake` | bool | 否 | false | 是否烘焙 |
| `with_fbx` | bool | 否 | true | 完成后转 FBX（导入链路要求 FBX，保持 true） |
| `texture_seed` | int | 否 | — | 纹理种子（可复现） |
| `texture_quality` | string | 否 | — | 纹理质量 |
| `texture_alignment` | string | 否 | — | 贴图对齐设置 |

提交 → 在当前子代理内等待，不结束任务 → 执行返回的 poll 命令 + `check_task` 一次 → 拿重贴图模型 URL。
⛔ 禁止循环 `check_task`；禁止对同一模型重复提交。

## 第二段：`import_3d_model_from_url`

```
execute_custom_tool(
  tool_name="import_3d_model_from_url",
  parameters={
    "model_url":   "<check_task 返回的重贴图模型 URL>",
    "provider":    "tripo-texture",
    "prompt":      "<texture_prompt_text 或简述>",   # 可选，History 显示
    "prefab_output_path": "Assets/...",              # 可选
    "force_overwrite": False,
    "session_id": "<可选>"
  }
)
```

`tool_name` 可选：`import_3d_model_from_url` / `query_local_task`（fallback 仅一次）/ `list_local_tasks`。

## `<bg_task_done>` 通知字段（第二段导入）

| 字段 | 说明 |
|---|---|
| `model_path` | 重贴图模型资产路径 |
| `prefab_path` | Prefab 路径（== `prefab_output_path`） |
| `provider` | `tripo-texture` |
| `session_id` / `start_time` / `end_time` / `duration_seconds` | 通用 |

## 使用示例

```
# 例 1：先前 Tripo 生成任务的重贴图
tripo_texture_model(original_model_task_id="<tripo generate_3d_model task_id>",
                   texture_prompt_text="weathered stone texture, mossy and ancient")
# → 在当前子代理内等待，不结束任务 → check_task 结果 →
execute_custom_tool("import_3d_model_from_url", {
  "model_url": "<retex fbx URL>", "provider": "tripo-texture",
  "prompt": "weathered stone texture, mossy and ancient"})

# 例 2：工程内任意模型（非 Tripo 来源）重贴图
file_upload(file_path="Assets/Models/MyChar.fbx")       # → file_url
tripo_texture_model(url="<file_url>",
                   texture_prompt_text="cyberpunk neon style, metallic chrome")

# 例 3：带风格引导图
file_upload(file_path="Assets/Textures/style_ref.png")   # → style_file_url
tripo_texture_model(url="<model file_url>",
                   texture_prompt_image="<style_file_url>")
```

## 故障排查

> 通用故障见 [共享执行约定](../../experience/templates/generator-async-pattern.md)。

| 问题 | 原因 | 解决 |
|---|---|---|
| `tripo_texture_model` 返回 400 / task id 被拒 | `original_model_task_id` 传了非 Tripo 任务 id | 改用 `file_upload` + `url` |
| `check_task` 返回 `.glb` | `with_fbx` 未开 | 报告格式不兼容；不自动重生成 |
| 导入段 `recovering` | domain reload | 自动幂等重跑，按共享约定等待后查询 |
| 纹理效果不理想 | 提示词不够具体 | 加 `texture_prompt_text` 或 `texture_prompt_image` 引导 |
| `Rigged model not found` / `Source model not found` 类报错 | 混用了 output_type 参数 | 重贴图是**静态导入**，不要传 `output_type`/`source_model_path` |

**Notes**：
- 第一段消耗 MCP 生成积分（成功完成才扣费）；第二段导入不消耗生成积分
- 长任务，需 Unity Editor 一直在线；domain reload 自动恢复
