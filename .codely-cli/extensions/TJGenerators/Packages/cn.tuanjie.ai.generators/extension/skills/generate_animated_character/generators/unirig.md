# UniRig Rigging（MCP `unirig_rig` + 导入 rigged）

generator: MCP 工具 `unirig_rig`（后端 UniRig AI 绑骨）  
Use case: rig any FBX/OBJ model into a Humanoid skeleton

---

## `unirig_rig`（MCP，生成段）

Rigs a model into a Humanoid skeleton. No motion animation is generated.
For an animated character from scratch, use the from-scratch flow
(`generate_3d_model` → `import_3d_model_from_url` + `add_motion=true`) instead of this tool.

**Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `model_url` | string | yes | CDN URL of the model (`.glb` / `.fbx` / `.obj`)。**必须先 `file_upload` 上传本地 FBX 拿 `file_url`**，不要传 `Assets/...` 路径或绝对路径 |

> ⚠️ **只能传本工具自己生态的 URL**：`file_upload` 产出的 CDN URL；其他来源（Tripo task id / Rodin job id 等）会被拒绝——一律先上传再绑。

**输出**：rigged Humanoid FBX URL（`check_task` 结果里拿）。

提交 → **在当前子代理内等待，不结束任务** → 执行返回的 poll 命令 + `check_task` 一次 → 拿 rigged FBX URL。
⛔ 禁止 agent 循环 `check_task`；禁止对同一模型重复提交。

---

## 导入段（Unity CustomTool `import_3d_model_from_url`，output_type=rigged）

```
execute_custom_tool(
  tool_name="import_3d_model_from_url",
  parameters={
    "model_url":         "<rigged fbx URL from check_task>",
    "output_type":      "rigged",
    "source_model_path": "Assets/Models/MyChar.fbx",   # Required — 源模型，用于复用贴图
    "prefab_output_path": "Assets/Characters/MyChar",  # Optional — 占位 Prefab
    "prompt":           "<可选，History 显示>",
    "session_id":        "<可选>"
  }
)
```

**导入后自动完成：**

- 下载 rigged FBX → 落在源模型同目录（`{baseName}_rigged.fbx` 语义）
- Humanoid 导入配置 + 骨映射修复 + 从 `source_model_path` 恢复贴图
- Capsule 占位 Prefab → 绑骨模型替换 + `Animator` / Avatar 绑定
- `<bg_task_done>`：`model_path`（绑骨 FBX）、`prefab_path`；Enter Play Mode 可见 T-Pose

**Submit response (success):**

```json
{
  "success": true,
  "task_id": "import_model_...",
  "status": "importing",
  "output_type": "rigged",
  "prefab_output_path": "Assets/TJGenerators/History/rigged_MyChar.prefab",
  "estimated_wait_seconds": 120,
  "notification_mode": "bg_task_done"
}
```

**Fallback**：`query_local_task`（仅超时一次）。domain reload 自动幂等重跑
（URL 永久，状态 `recovering` 时按共享约定等待后查询）。
