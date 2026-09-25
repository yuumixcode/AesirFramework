# HunyuanMotion（MCP `generate_motion` + 导入 motion）

generator: MCP 工具 `generate_motion`（后端 HY Motion）  
Use case: generate motion animation for an already-rigged Humanoid FBX

---

## `generate_motion`（MCP，生成段）

Generates motion animation clips from a text description.
If the source model is not yet rigged, rig it first (`unirig_rig`，或从零流程的 `add_motion`)。

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `input_text` | string | yes | — | Motion description（如 `"walk forward"` / `"wave right hand"` / `"jump and land"`；英文效果最好） |
| `action_duration` | float | no | 5.0 | 动作时长（秒） |
| `cfg_strength` | float | no | 5.0 | 文本引导强度（越高越贴近描述，建议 3–7） |
| `random_seed_list` | string | no | `"0,1,2,3"` | 随机种子（**导入链路固定传 `"0"` 单剪辑**：多 seed 产出多剪辑，导入工具按单剪辑自循环控制器处理，多余剪辑被忽略） |

**action_duration Reference:**

| Value | Best for |
|-------|---------|
| 2–3s | Short snappy actions (jump, turn, punch) |
| 4–6s | Standard loops (run cycle, idle stand) |
| 7–10s | Complex sequences (gymnastics, dance) |

**输出**：动作 FBX URL（`check_task` 结果里拿）。

提交 → **在当前子代理内等待，不结束任务** → 执行返回的 poll 命令 + `check_task` 一次 → 拿 motion FBX URL。
⛔ 禁止 agent 循环 `check_task`；禁止对同一动作重复提交。

---

## 导入段（Unity CustomTool `import_3d_model_from_url`，output_type=motion）

```
execute_custom_tool(
  tool_name="import_3d_model_from_url",
  parameters={
    "model_url":          "<motion fbx URL from check_task>",
    "output_type":       "motion",
    "rigged_model_path":  "Assets/Models/MyChar_rigged.fbx",   # Required — 已导入工程的 Humanoid FBX
    "target_prefab_path": "Assets/Characters/MyChar.prefab",   # Optional — 自动接线 controller + avatar
    "loop":              True,                                  # Optional，默认 true（一次性动作传 false）
    "prompt":             "<可选，History 显示>",
    "session_id":         "<可选>"
  }
)
```

**导入后自动完成：**

- 下载动作 FBX → 落在绑骨模型同目录（`{baseName}_motion.fbx` 语义）
- Humanoid 动画导入配置（`loop` 控制循环）
- 创建单剪辑 AnimatorController：`{riggedBaseName}_Controller.controller`；`loop=false` 时不创建自循环跳转
- `target_prefab_path` 传入时：Prefab 的 `Animator` 自动获得 controller + avatar
- `<bg_task_done>`：`motion_fbx_path`、`controller_path`、`rigged_model_path`（无占位 Prefab，跳过 place）

**注意**：`rigged_model_path` 必须是**已导入工程**的 `Assets/...` 路径（通常来自上一步路径 A 的
`model_path`），不是 CDN URL。没传 `target_prefab_path` 时，收到通知后用
`activate_skill("unity-place-assets-in-scene")` 加载放置指引，按 §4a 用 `exec_editor_script` 手动接线一次。

**Fallback**：`query_local_task`（仅超时一次）。domain reload 自动幂等重跑。
