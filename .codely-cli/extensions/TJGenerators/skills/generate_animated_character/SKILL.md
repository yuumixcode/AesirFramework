---
name: unity-animated-character-generation
description: Rig and/or animate Humanoid 3D characters in Unity (UniRig + HunyuanMotion). From scratch: generate_3d_model_by_tripo_p1 / by_rodin with add_motion=true + motion_description (same as UI 添加动作: 3D → UniRig → HunyuanMotion). Existing FBX: generate_rigged_model, generate_model_motion, or generate_animated_character. Use for "生成带动画角色", "给模型绑骨", "让角色走路", "rig my character", "add walk animation", "animated NPC".
---

> ⚠️ **执行约束**
> - **主 agent**：无 `execute_custom_tool` 权限，必须 `task(subagent_name="animated-character-generator", ...)` 委托，不要 `activate_skill` 后自己调。
> - **子代理（本文档主要读者）**：有权限，按下方决策树选 Tool A / B / C，再读对应 `generators/*.md` 或下方 C 摘要执行。

> ⛔ **`place_assets_in_scene` 调用规则**
> - **调用方式**：`activate_skill("unity-place-assets-in-scene")` → 按 §4a Prefab 模板用 `execute_csharp_script` 跑 `PrefabUtility.InstantiatePrefab`（**不是** `execute_custom_tool`）。
> - **从零（3D + add_motion）/ Tool A / C**：提交后**立即调一次**放占位 Prefab；`<bg_task_done>` 后**不再调**。
> - **Tool B**：无 placeholder；等通知后**调一次**把 `motion_fbx_path` 应用到现有 Humanoid。
> - **主 agent**：报告里的路径是"已处理"的证据，**不要再调**（除非用户明确要求换位置/再放一个）。
> - 详见 [async-pattern §5.1](../../experience/templates/generator-async-pattern.md#51-place_assets_in_scene-调用规则)。

# Rig / Motion / Animated Character in Unity

Humanoid 绑骨和/或动作。本 skill 是**路由文档**。

| 路径 | 工具 | 用途 |
|------|--------|------|
| **从零（UI 同款）** | `generate_3d_model_by_tripo_p1` / `generate_3d_model_by_rodin` + `add_motion` | 3D 出模 → UniRig → HunyuanMotion（同一条 GenerationPipeline） |
| **A** | `generate_rigged_model` | 已有模型，只绑骨（UniRig） |
| **B** | `generate_model_motion` | 已有 Humanoid，只要动作（HunyuanMotion） |
| **C** | `generate_animated_character` | 已有模型，一站式绑骨 + 动作 |

> **仅 Humanoid（双足人形）。** 通用 3D 物件（不要动作）→ `generate_3d_model`（不加 `add_motion`）。本地命令行 UniRig → `fbx-humanoid-auto-rig`。

## 工具选择决策

```
用户想要什么？
  ├── 从零生成带动画角色
  │     → generate_3d_model_by_tripo_p1（默认）或 by_rodin
  │       add_motion=true + motion_description
  │       （禁止 generate_animated_character，禁止 action_id / topology / pose_mode）
  ├── 已有模型，只绑骨             → Tool A  读 generators/unirig.md
  ├── 已有 Humanoid，只要动作      → Tool B  读 generators/hunyuan-motion.md
  └── 已有模型，绑骨+动作一次完成  → Tool C  （下文）
```

| 用户意图关键词 | 工具 |
|---|---|
| "生成一个会走路的角色/机甲" / "animated NPC from scratch" | **从零：3D + add_motion** |
| "给模型绑骨" / "rig my character" | **A** |
| "让 humanoid 动起来" / "add walk animation" / "做个 backflip" | **B** |
| "已有模型，绑骨并生成动画" / "一次完成" | **C** |

## 从零生成（UI「添加动作」同款，一次提交）

与 3D 窗口勾选「添加动作」相同：先出静态网格，落地后管线自动 UniRig + HunyuanMotion。

```python
execute_custom_tool(
  tool_name="generate_3d_model_by_tripo_p1",  # 高精度用 generate_3d_model_by_rodin + ta_pose=true
  parameters={
    "prompt": "a humanoid mecha robot, full body, standing upright, T-pose",
    "prefab_output_path": "Assets/Characters/MechaRobot",
    "add_motion": True,
    "motion_description": "a walking cycle",
  }
)
```

1. 调 3D 工具 → 拿 `task_id` + `prefab_output_path`
2. 立即 `place_assets_in_scene`（Prefab，Cube 占位）
3. **END RESPONSE TURN** — 禁止 poll，禁止再调 `generate_animated_character`
4. `<bg_task_done>` → 读 `model_path` / `prefab_path`（Cube 已替换为绑骨+动作模型）

完整 3D 参数见 `unity-3d-model-generation` 的 `generators/tripo-p1.md` / `rodin.md`。

⛔ **禁止**：`action_id`、`topology`、`should_remesh`、`pose_mode`、`height_meters`、`enable_pbr`、`target_polycount`（Meshy 旧参数）。动作只用 `motion_description` 英文描述。

## 🚦 执行流程

**从零（3D + add_motion，Cube 占位）**：见上方「从零生成」。档位 ~12–20 min；300s 内无通知才允许 `query_3d_model_status_by_*` **一次**。

**Tool A / C**（有 Capsule 占位）：
1. 调工具 → 拿 `task_id` + `prefab_output_path`
2. 立即 `place_assets_in_scene`（Prefab）
3. **END RESPONSE TURN** — 禁止 poll
4. `<bg_task_done>` → 读结果路径（**不要再 place**）

**Tool B**（无占位）：
1. 调 `generate_model_motion` → 拿 `task_id`
2. **跳过** place → **END RESPONSE TURN**
3. `<bg_task_done>` → 读 `motion_fbx_path` → **此时调一次** place（应用到现有角色）

**档位**：A 1–3 min，B 1–2 min，C 2–5 min；300s 内无通知才允许对应 `query_*_status` **一次**。并发上限 **3**。完整规则见 [generator-async-pattern](../../experience/templates/generator-async-pattern.md)。

## ⚠️ 约束

1. **从零禁止调 Tool C**——没有 `source_model_path` 时用 3D + `add_motion`，不要把 prompt / `action_id` 塞进 `generate_animated_character`。
2. **三个工具的提交/查询/列表互不通用**——A/B/C 各用各的 `query_*` / `list_*`；从零用 `query_3d_model_status_by_*`。
3. **Tool A / C 输入是已有模型**——不是 text-to-3D prompt。
4. **Tool B 必须已是 Humanoid rigged FBX**——未绑骨先 A 或直接 C。
5. **Tool C 状态流**：`rigging` (0–50%) → `generating_motion` (50–100%) → `completed`。Stage 2 失败时为 `rigging_complete_motion_failed`，可对 `rigged_model_path` 用 Tool B 重试。
6. **只看 `status == "completed"`**；`generating*` @ 100% ≠ 完成。
7. **占位**：从零是 Cube；A/C 是 Capsule。不要当杂物删。
8. **`force_overwrite` 仅 3D / A / C 有**；B 没有。

## When to Use / NOT to Use

适用：从零带动画 Humanoid（3D + add_motion）、已有模型绑骨、加动作、一站式绑骨+动作。

不适用：
- 通用静态 3D 物件（不要动作）→ `generate_3d_model`（不加 `add_motion`）
- 仅 2D 帧动画 → `generate_sprite_sequence`
- 非 Humanoid（四足等）→ 本 skill 不支持

---

## Tool A — `generate_rigged_model`

完整参数见 [`generators/unirig.md`](generators/unirig.md)。

```python
execute_custom_tool(
  tool_name="generate_rigged_model",
  parameters={
    "source_model_path": "Assets/Models/MyChar.fbx",
    "prefab_output_path": "Assets/Characters/MyChar",
    "force_overwrite": False,
  }
)
```

`<bg_task_done>`：`pipeline_type=rig_only`，`rigged_model_path`，`prefab_path`。

---

## Tool B — `generate_model_motion`

完整参数见 [`generators/hunyuan-motion.md`](generators/hunyuan-motion.md)。

```python
execute_custom_tool(
  tool_name="generate_model_motion",
  parameters={
    "rigged_model_path": "Assets/Models/MyChar_rigged.fbx",
    "motion_description": "a walking cycle",
    "action_duration": 5.0,
    "cfg_strength": 5.0,
    "random_seed": 0,
    "loop": True,
  }
)
```

`<bg_task_done>`：`pipeline_type=motion_only`，`motion_fbx_path`，`controller_path`（无 `prefab_path`）。

---

## Tool C — `generate_animated_character`

仅用于**已有 FBX/OBJ**。从零不要用这个工具。

```python
execute_custom_tool(
  tool_name="generate_animated_character",
  parameters={
    "source_model_path": "Assets/Models/MyChar.fbx",   # Required — FBX/OBJ
    "motion_description": "a walking cycle",           # Required — English preferred
    "prefab_output_path": "Assets/Characters/Hero",    # Optional
    "force_overwrite": False,
    "action_duration": 5.0,                            # 2–3 short, 4–6 loops, 7–10 complex
    "cfg_strength": 5.0,
    "random_seed": 0,
    "loop": True
  }
)
```

提交成功返回：`task_id`，`prefab_output_path`（立即可 place），`expected_rigged_path`，`estimated_wait_seconds`≈300，`notification_mode=bg_task_done`。

`<bg_task_done>` 字段：

| 字段 | 说明 |
|---|---|
| `pipeline_type` | `"rig_and_motion"` |
| `rigged_model_path` | 绑骨 Humanoid FBX |
| `motion_fbx_path` | 动作 FBX |
| `controller_path` | AnimatorController |
| `prefab_path` | 最终 Prefab |
| `source_model_path` / `motion_description` | 输入 |

Fallback：`query_animated_character_status` / `list_animated_character_tasks`（仅超时一次）。

`interrupted` 且 `rigged_stage_completed: true` → 用 Tool B 对 `rigged_model_path` 补动作，避免重新绑骨。

---

## Agent-specific：完成后朝向

角色应直立（Y up）并朝向相机（默认 -Z）。先试 `rotation=[0,0,0]`；若背对试 `[0,180,0]`。用 `execute_csharp_script` 修正。

## 常见错误

| 现象 | 处理 |
|---|---|
| 从零却调了 C（报 `source_model_path is required`） | 改调 `generate_3d_model_by_tripo_p1` + `add_motion=true` + `motion_description` |
| 传入 `action_id` / `topology` / `pose_mode` | Meshy 旧参数，丢弃；动作用 `motion_description` |
| 调错 query 工具 | 从零→`query_3d_model_status_by_*`；A→`query_rigged_model_status`；B→`query_model_motion_status`；C→`query_animated_character_status` |
| Tool B 输入未绑骨 | 先 A，或改用 C |
| 已有模型却拆成 3D 再生成一遍 | 直接用 C |
