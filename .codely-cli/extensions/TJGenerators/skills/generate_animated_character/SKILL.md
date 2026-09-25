---
name: unity-animated-character-generation
description: "Rig and/or animate Humanoid 3D characters in Unity (UniRig + HunyuanMotion). Generation runs on the MCP backend (generate_3d_model / unirig_rig / generate_motion), landing and post-processing run via import_3d_model_from_url. From scratch: generate_3d_model then import with add_motion=true + motion_description (same as UI 添加动作). Existing FBX: unirig_rig / generate_motion / chained both. Use for \"生成带动画角色\", \"给模型绑骨\", \"让角色走路\", \"rig my character\", \"add walk animation\", \"animated NPC\"."
---

> **执行前读取**：[共享执行约定](../../experience/templates/generator-async-pattern.md)。
> 主 agent 按需委托 `general-purpose`；已在子代理中则直接执行本 skill，不再委托。已提交的阶段从原任务续接。

> ⛔ **场景放置规则**
> - **调用方式**：`activate_skill("unity-place-assets-in-scene")` → 按 §4a Prefab 模板用 `exec_editor_script` 跑 `PrefabUtility.InstantiatePrefab`（**不是** `execute_custom_tool`）。
> - **从零 / A / C**：导入任务提交后**立即调一次**放占位 Prefab；`query_local_task` 确认完成后**不再调**。
> - **B（动作）**：无占位；query 确认完成后**调一次**把结果应用到现有角色（或确认 `target_prefab_path` 已自动接线）。
> - **主 agent**：报告里的路径是"已处理"的证据，**不要再调**（除非用户明确要求换位置/再放一个）。
> - 详见 [共享执行约定](../../experience/templates/generator-async-pattern.md)。

# Rig / Motion / Animated Character in Unity（MCP 生成 + Unity 导入）

Humanoid 绑骨和/或动作。生成段走 **MCP 工具**，落地段统一走 Unity CustomTool **`import_3d_model_from_url`**。
本 skill 是**路由文档**。

| 路径 | 生成（MCP） | 导入（Unity） | 用途 |
|------|-------------|---------------|------|
| **从零（UI 同款）** | `generate_3d_model` | `import_3d_model_from_url` + `add_motion=true` | 3D 出模 → 管线内置 UniRig + HunyuanMotion |
| **A** | `file_upload` + `unirig_rig` | `import_3d_model_from_url` + `output_type=rigged` | 已有模型，只绑骨 |
| **B** | `generate_motion` | `import_3d_model_from_url` + `output_type=motion` | 已有 Humanoid，只要动作 |
| **C** | A + B 链式 | 两次导入 | 已有模型，绑骨 + 动作一站式 |

> **仅 Humanoid（双足人形）。** 通用 3D 物件（不要动作）→ `generate_3d_model` 后普通导入（不加 `add_motion`）。

## 工具选择决策

```
用户想要什么？
  ├── 从零生成带动画角色
  │     → MCP generate_3d_model（humanoid T-pose prompt）
  │       → import_3d_model_from_url + add_motion=true + motion_description
  │       （禁止拆成两段，禁止 action_id / topology / pose_mode）
  ├── 已有模型，只绑骨             → A  读 generators/unirig.md
  ├── 已有 Humanoid，只要动作      → B  读 generators/hunyuan-motion.md
  └── 已有模型，绑骨+动作一次完成  → C  （A 完成后再 B）
```

| 用户意图关键词 | 路径 |
|---|---|
| "生成一个会走路的角色/机甲" / "animated NPC from scratch" | **从零** |
| "给模型绑骨" / "rig my character" | **A** |
| "让 humanoid 动起来" / "add walk animation" / "做个 backflip" | **B** |
| "已有模型，绑骨并生成动画" / "一次完成" | **C** |

## 从零生成（UI「添加动作」同款，两段提交）

第一段：MCP 出网格；第二段：导入工具内置绑骨+动作。

```
# 第一段：MCP 生成 humanoid 网格
generate_3d_model(mode="text_to_model", provider="rodin",   # 默认
                  prompt="a humanoid mecha robot, full body, standing upright, T-pose")
# → 在当前子代理内等待，不结束任务 → 执行返回的 poll 命令后调用 check_task → 拿 fbx URL
```

```
# 第二段：导入 + 内置 UniRig + HunyuanMotion（一次调用完成全部后处理）
execute_custom_tool(
  tool_name="import_3d_model_from_url",
  parameters={
    "model_url":         "<第一段 fbx URL>",
    "provider":          "rodin",
    "prompt":            "a humanoid mecha robot, full body, standing upright, T-pose",
    "prefab_output_path": "Assets/Characters/MechaRobot",   # 可选
    "add_motion":        True,
    "motion_description": "a walking cycle",
  }
)
```

1. 第二段提交 → 拿 `task_id` + `prefab_output_path`（Cube 占位）
2. 立即 `activate_skill("unity-place-assets-in-scene")`，按 Prefab 模板用 `exec_editor_script` 放置
3. **在当前子代理内前台等待（每次最多30秒）（estimated_wait_seconds≈900s）** → **在当前子代理内等待，不结束任务** — 禁止拆开绑骨/动作
4. 本次等待结束 → `query_local_task` **一次** → 读 `model_path` / `prefab_path`（占位已替换为绑骨+动作模型）；仍是 `importing` 则再进行一次前台短等待

完整网格生成参数见 `unity-3d-model-generation` 的 [generators/mcp.md](../generate_3d_model/generators/mcp.md)。
档位 ~12–20 min。**不要干等 Unity 侧 `<bg_task_done>` 推送**（不依赖通知恢复已结束的子代理）。

⛔ **禁止**：`action_id`、`topology`、`should_remesh`、`pose_mode`、`height_meters`、`enable_pbr`、`target_polycount`（Meshy 旧参数）。动作只用 `motion_description` 英文描述。

## 🚦 执行流程（A / B / C）

**A**（有 Capsule 占位）：
1. `file_upload(source_model_path)` → CDN URL → `unirig_rig(model_url=...)` → **在当前子代理内等待，不结束任务** 等 check_task 结果
2. `import_3d_model_from_url(output_type=rigged, model_url=<rigged fbx URL>, source_model_path=<源模型>, prefab_output_path=...)` → 拿 `task_id` + 占位 → 立即 place → **前台等待最多 30 秒** → **在当前子代理内等待，不结束任务**
3. 本次等待结束 → `query_local_task` **一次** → 读 `model_path` / `prefab_path`

**B**（无占位）：
1. `generate_motion(input_text=..., random_seed_list="0")` → **在当前子代理内等待，不结束任务** 等 check_task 结果
2. `import_3d_model_from_url(output_type=motion, model_url=<motion fbx URL>, rigged_model_path=..., target_prefab_path=..., loop=True)` → **前台等待最多 30 秒** → **在当前子代理内等待，不结束任务**（跳过 place）
3. 本次等待结束 → `query_local_task` **一次** → 读 `motion_fbx_path` / `controller_path`（`target_prefab_path` 已自动接线；没传时才手动应用）

**C** = A 完成后对 `rigged_model_path` 执行 B（串行两轮）。

**档位**：A 全链 1–3 min（rig 生成 ~1–2 min + 导入 ~1 分钟内）；B 全链 1–2 min；C 3–5 min；从零 12–20 min。
每段统一**前台等待 + `query_local_task` 状态查询**（`importing` 未完则前台等待后查询）。并发上限 **3**。
MCP 段轮询纪律见 [3d-model-generation mcp.md §check_task](../generate_3d_model/generators/mcp.md)。

## ⚠️ 约束

1. **从零禁止拆成 A/B/C**——网格生成 + `add_motion` 导入两段搞定，不要先把角色当"已有模型"再绑骨。
2. **A/B/C 的输入是已有模型**——不是 text-to-3D prompt；A/C 输入 FBX/OBJ，B 输入必须是**已绑骨 Humanoid FBX**（未绑骨先 A）。
3. **B 单剪辑**：`random_seed_list` 固定传 `"0"`（多 seed 产出多剪辑，导入工具按单剪辑自循环控制器处理，多余剪辑被忽略）。
4. **只看 `status == "completed"`**；`importing` / `recovering` ≠ 完成。
5. **占位**：从零/A/C 是 Cube/Capsule。不要当杂物删。
6. **domain reload 自动恢复**：MCP 段 URL 永久、导入段幂等重跑；状态 `recovering` 按共享约定等待后查询，不要重复提交。
7. **`force_overwrite` 仅导入工具有**（覆盖同路径 prefab）；MCP 生成段没有该参数，重复提交生成是被禁止的。

## When to Use / NOT to Use

适用：从零带动画 Humanoid、已有模型绑骨、加动作、一站式绑骨+动作。

不适用：
- 通用静态 3D 物件（不要动作）→ `generate_3d_model` 普通导入（不加 `add_motion`）
- 仅 2D 帧动画 → `generate_sprite_sequence`
- 非 Humanoid（四足等）→ 本 skill 不支持

---

## 路径 A — 绑骨（MCP `unirig_rig` + 导入 rigged）

完整参数与示例见 [`generators/unirig.md`](generators/unirig.md)。

```
file_upload(file_path="Assets/Models/MyChar.fbx")          # → file_url（已有 CDN URL 可跳过）
unirig_rig(model_url="<file_url>")                          # → 在当前子代理内等待，不结束任务 → check_task 结果
execute_custom_tool("import_3d_model_from_url", {
  "model_url":        "<rigged fbx URL>",
  "output_type":      "rigged",
  "source_model_path": "Assets/Models/MyChar.fbx",         # 复用源模型贴图
  "prefab_output_path": "Assets/Characters/MyChar",        # 可选
})
```

完成结果（query 返回 / 异步 payload）：`output_type=rigged`，`model_path`（绑骨 FBX），`prefab_path`（含 Animator/Avatar）。

---

## 路径 B — 动作（MCP `generate_motion` + 导入 motion）

完整参数与示例见 [`generators/hunyuan-motion.md`](generators/hunyuan-motion.md)。

```
generate_motion(input_text="a walking cycle", random_seed_list="0")
                                            # → 在当前子代理内等待，不结束任务 → check_task 结果
execute_custom_tool("import_3d_model_from_url", {
  "model_url":         "<motion fbx URL>",
  "output_type":      "motion",
  "rigged_model_path": "Assets/Models/MyChar_rigged.fbx",
  "target_prefab_path": "Assets/Characters/MyChar.prefab",  # 可选；传入则自动接线
  "loop":             True,
})
```

完成结果（query 返回 / 异步 payload）：`output_type=motion`，`motion_fbx_path`，`controller_path`（单剪辑自循环，Play Mode 自动循环）。

---

## 完成后朝向

角色应直立（Y up）并朝向相机（默认 -Z）。先试 `rotation=[0,0,0]`；若背对试 `[0,180,0]`。用 `exec_editor_script` 修正。

## 常见错误

| 现象 | 处理 |
|---|---|
| 从零却走了 A/C（重复生成角色） | 从零 = `generate_3d_model` + 导入 `add_motion=true`，两段完成 |
| 传入 `action_id` / `topology` / `pose_mode` | Meshy 旧参数，丢弃；动作用 `motion_description` / `input_text` |
| `unirig_rig` 收到本地路径报错 | 先 `file_upload` 拿 CDN `file_url` 再传 `model_url` |
| B 输入未绑骨 | 先 A（或从零流程），再 B |
| 导入工具报 `Rigged model not found` | `rigged_model_path` 必须是**已导入工程**的 Humanoid FBX 路径（`Assets/...`），不是 CDN URL |
| MCP 段失败后重复提交生成 | 被禁止；对同一 task_id 查 `check_task`；确证失败后报告原因，由主 agent 按用户要求决定是否重试 |
