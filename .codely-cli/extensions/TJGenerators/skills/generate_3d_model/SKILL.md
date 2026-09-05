---
name: unity-3d-model-generation
description: Generate 3D models in Unity using AI (defaults to Tripo P1; use Rodin Gen-2.5 Extreme-High for high-poly / PBR / FBX output). Use this skill whenever the user wants to create a standalone 3D object, prop, or asset — e.g., "生成一个3D模型", "生成一把椅子", "create a 3D rock", "make a sword model". Trigger for furniture, weapons, vehicles, buildings, food, props, environment assets, etc. DO NOT use for terrain. For an animated humanoid from scratch: this skill WITH add_motion=true + motion_description (same as UI 添加动作: 3D → UniRig → HunyuanMotion). For an existing FBX: generate_animated_character.
---

> ⚠️ **执行约束**
> - **主 agent**：无 `execute_custom_tool` 权限，必须 `task(subagent_name="3d-model-generator", ...)` 委托，不要 `activate_skill` 后自己调。
> - **子代理（本文档主要读者）**：有权限，按下方 `execute_custom_tool(...)` 示例执行。

> ⛔ **`place_assets_in_scene` 调用规则**
> - **调用方式**：`activate_skill("unity-place-assets-in-scene")` → 按 §4a Prefab 模板用 `execute_csharp_script` 跑 `PrefabUtility.InstantiatePrefab`（**不是** `execute_custom_tool`，**不要** `unity_gameobject` 放 Prefab）。
> - **子代理**：提交后**立即调一次**放占位 Prefab；收到 `<bg_task_done>` 后**不再调**（Cube 子节点自动覆盖为真实模型）。
> - **主 agent**：报告里的 `prefab_path` 是"已放置"的证据，不是"请你放置"的指示，**不要再调**。
> - **例外**：用户明确要"换位置 / 改 scale / 再放一个"时才再次调用。详见 [async-pattern §5.1](../../experience/templates/generator-async-pattern.md#51-place_assets_in_scene-调用规则)。

# Generate 3D Model in Unity 🪑

生成 3D 模型资产。本 skill 是 **路由文档**：决定使用哪个 generator，再读对应 `generators/*.md` 子文档拿完整参数。

输出：3D 模型文件（Tripo 依 API 返回；Rodin 为 **FBX**）+ 自动生成的 Prefab，保存到 `Assets/TJGenerators/History/`。勾选 `add_motion` 时，主模型落地后自动 UniRig 绑骨 + HunyuanMotion 动作（与 UI「添加动作」同一条 `GenerationPipeline`）。

## 🚦 执行四步（不要跳读外链）

1. 调 `generate_3d_model_by_tripo_p1` / `..._by_rodin` → 拿 `task_id` + `prefab_output_path`
2. 立即 `place_assets_in_scene`（资产类型 `Prefab`，路径用 `prefab_output_path`）→ 场景出现 Cube 占位
3. **END RESPONSE TURN** — 不要 poll、不要 `query_3d_model_status_by_*`、不要继续操作
4. 下一轮收到 `<bg_task_done>` → 读 `model_path` / `prefab_path`（Cube 子节点已原地替换为真实模型，**不要再 place**）

**档位**：长任务 3–15 分钟；300 秒内无通知才允许 `query_3d_model_status_by_*` 一次。完整 async 规则见 [generator-async-pattern](../../experience/templates/generator-async-pattern.md)。

## ⚠️ Skill 独有约束

1. **必须先选 generator 再调工具**——本 skill 没有"统一入口工具"，只有按 generator 区分的 `generate_3d_model_by_tripo_p1` 和 `generate_3d_model_by_rodin`。
2. **必须先读子文档再调工具**——两个 generator 的参数集差异较大（Tripo P1 不支持 `tier`/`quality`/`material`/`mesh_mode`/`ta_pose`，Rodin 的面数控制参数是 `quality_override` 而非 Tripo 的 `face_limit`）。直接照搬错 generator 的参数会被官方 API 拒绝。
3. **`prompt` 与 `image_path` 至少一个**——两个 generator 都可文生 / 图生 / 文+图混合。Tripo P1 还支持 4 视图模式（`multiview_image_paths`，顺序 `[front, left, back, right]`，front 必须）。
4. **占位 Prefab 是 Cube 子节点**——生成完成后 Cube 被替换为真实 model 子节点。**不要**把场景里的实例当 Cube 删掉重建。
5. **带动画角色从零 = UI「添加动作」**——`add_motion=true` + `motion_description`（如 `"a walking cycle"`）。**不要**再调 `generate_animated_character`，也**不要**传 Meshy 的 `action_id` / `topology` / `pose_mode`。Rodin 建议同时 `ta_pose=true`。

## 模型选择规则

| 场景 | 选择 | 工具前缀 | 子文档 |
|------|------|----------|--------|
| 默认 / 通用 / 低面 / 小游戏 / 移动端 / 试玩广告 | **Tripo P1** | `generate_3d_model_by_tripo_p1` | [`generators/tripo-p1.md`](generators/tripo-p1.md) |
| 高精度 / hero 资产 / PBR / FBX 输出 | **Rodin Gen-2.5** | `generate_3d_model_by_rodin` | [`generators/rodin.md`](generators/rodin.md) |

### 选择决策树

- 用户没明确说精度要求 → **Tripo P1**（默认）
- 用户说"低面 / 低模 / 移动端 / 包体限制" → **Tripo P1**（`face_limit` 48~20000；面数 >20000 时用 Rodin 的 `quality_override`）
- 用户说"高精度 / hero / 主角资产 / PBR" → **Rodin Gen-2.5**（输出 FBX，默认 Extreme-High tier）
- 用户明确指定 face_count > 20000 → **Rodin Gen-2.5**（`quality_override` 上限 200 万，Tripo P1 上限约 20000）
- 批量生成需要稳定面数与体积 → **Tripo P1**

> **调用前必须 `Read` 对应子文档**，获取完整参数表与该 generator 的独有约束。

## When to Use / NOT to Use

适用：家具、武器、载具、建筑、食物、道具、环境资产等独立 3D 物件；以及从零带动画 Humanoid（`add_motion=true`，由 `animated-character-generator` 调用本工具）。

不适用：
- 地形 / 景观 / 峡谷 / 山脉 → `generate_terrain`
- 已有 FBX/OBJ 只要绑骨/加动作 → `animated-character-generator`（不要再生成一遍模型）
- 2D 精灵 / 图标 → `generate_sprite`
- 天空盒 → `generate_skybox`

## 工具速查

> 完整参数表见各 generator 子文档。

### Tripo P1（详见 [tripo-p1.md](generators/tripo-p1.md)）

```python
execute_custom_tool(
  tool_name="generate_3d_model_by_tripo_p1",
  parameters={
    "prompt": "wooden chair, simple shape, low poly",  # 与 image_path 至少一个
    "face_limit": 5000,                                # P1 独有低面控制（48~20000）
    "texture": True, "pbr": True,
    # 带动画角色（UI「添加动作」同款）:
    # "add_motion": True, "motion_description": "a walking cycle",
    # 完整参数读 generators/tripo-p1.md
  }
)
```

`tool_name` 可选：
- `generate_3d_model_by_tripo_p1` — 提交生成
- `query_3d_model_status_by_tripo_p1` — fallback 查询（仅一次）
- `list_3d_model_tasks_by_tripo_p1` — 列出 session 内所有任务

### Rodin Gen-2.5（详见 [rodin.md](generators/rodin.md)）

```python
execute_custom_tool(
  tool_name="generate_3d_model_by_rodin",
  parameters={
    "prompt": "ornate medieval sword, high detail",   # 与 image_path 至少一个
    "tier": "Gen-2.5-Extreme-High",                    # 默认极高（Extreme-High）
    "quality": "high",
    "material": "PBR",
    # 完整参数读 generators/rodin.md
  }
)
```

`tool_name` 可选：
- `generate_3d_model_by_rodin` — 提交生成
- `query_3d_model_status_by_rodin` — fallback 查询（仅一次）
- `list_3d_model_tasks_by_rodin` — 列出 session 内所有任务

## `<bg_task_done>` 通知字段（两个 generator 共享）

通用字段见模板。本 skill 额外字段（两个 generator 的通知 payload 形状一致）：

| 字段 | 说明 |
|---|---|
| `model_path` | 最终 3D 模型路径（Tripo 依 API；Rodin 为 `.fbx`） |
| `prefab_path` | 最终 Prefab 路径（== 提交时的 `prefab_output_path`） |
| `preview_url` | 渲染预览缩略图 URL（可能为空） |
| `generator_type` | `"tripo-p1"` 或 `"rodin"` |
| `prompt` | 原始 prompt |
| `image_path` | 原始参考图（如有） |

## 放入场景

资产类型 **`Prefab`**，路径用 `prefab_output_path`。提交后立即调一次（里面是 Cube 占位）；通知到达后**不要**再调——Cube 子节点自动被真实模型替换。详见 [async-pattern §5 / §5.1](../../experience/templates/generator-async-pattern.md#5-placeholder-工作流适用于会返回-placeholder_path--prefab_output_path-的工具)。

## 使用示例

### 默认（Tripo P1）—— 简单道具

```python
result = execute_custom_tool(
    tool_name="generate_3d_model_by_tripo_p1",
    parameters={
        "prompt": "wooden barrel, medieval style",
        "face_limit": 3000
    }
)
if not result.get("success", True):
    raise RuntimeError(f"[{result['error_code']}] {result['message']}")

task_id = result["task_id"]
prefab_output_path = result["prefab_output_path"]

# ✅ 立即用 place_assets_in_scene 把 prefab_output_path 应用为 Prefab
# 然后 END RESPONSE TURN，等 bg_task_done 通知（3–15 分钟）
```

### Rodin Gen-2.5 Extreme-High —— 高精度 hero 武器

```python
parameters={
    "prompt": "ornate golden sword with gem-encrusted hilt, magical runes engraved on blade",
    "tier": "Gen-2.5-Extreme-High",
    "quality": "high",
    "material": "PBR"
}
```

### 图生 3D（任意 generator）

```python
parameters={
    "image_path": "Assets/ConceptArt/chair_concept.png",
    # Tripo 还可以叠加 prompt 做指导
    "prompt": "wooden chair, baroque style"
}
```

### Tripo P1 多视图（4 视图）

```python
parameters={
    "multiview_image_paths": [
        "Assets/Refs/character_front.png",
        "Assets/Refs/character_left.png",
        "Assets/Refs/character_back.png",
        "Assets/Refs/character_right.png"
    ]
}
# front 必须存在；缺其他视图最少 3 张
```

## 故障排查

> 通用故障（配置缺失 / 任务卡住 / 状态异常 / 未登录）见 [generator-async-pattern §10](../../experience/templates/generator-async-pattern.md#10-通用故障排查)。

### Skill 独有问题

| 问题 | 原因 | 解决 |
|---|---|---|
| 调用 Tripo 工具传了 Rodin 参数（如 `tier` / `quality`） | 参数集不通用 | Tripo 用 `face_limit` / `pbr`；查 `generators/tripo-p1.md` |
| 调用 Tripo 工具传了 `quad` / `smart_low_poly` / `generate_parts` / `geometry_quality` | P1-20260311 不支持这些 | 直接删掉这些参数，或改用 Rodin |
| 调用 Rodin 工具传了 `face_limit` / `multiview_image_paths` | Rodin 参数名是 `quality_override`（等价 face_limit，500–2000000）；多视图只 Tripo 支持 | Rodin 用 `tier` / `quality` / `quality_override` |
| 状态变 `interrupted`（仅 Rodin） | domain reload 丢失后端记录 | 用 `generate_3d_model_by_rodin` + `force_overwrite=true` + 相同 `prefab_output_path` 重新提交 |
| 内容策略拦截（`content_moderation`） | Tripo P1 对武器/政治/版权词敏感，平台策略不可绕过 | 把拦截原因告知用户；如用户需要类似资产，先说明资产库不保证有合适内容，再询问是否改用 `search_assets` |

**内容策略拦截处理（不改写 prompt）：**

1. 直接告知用户："生成被内容策略拒绝，原因：{拦截原因}"（从错误消息里的「拦截原因」后取；若无则只说被拒绝）
2. 说明这是平台限制，无法通过修改 prompt 绕过
3. 提示："也可以尝试从资产库搜索，但不保证有合适的资产，是否搜索？"
4. **仅在用户确认后**再 `activate_skill("unity-asset-search")`；不要擅自改写 prompt 重试生成

### Domain reload 后 task 丢失

通用恢复流程见 [generator-async-pattern §6](../../experience/templates/generator-async-pattern.md#6-domain-reload-recovery)。本 skill 完成态判定：

- Prefab 内 Cube 子节点 → 仍是占位
- Prefab 内已绑定真实 model 子节点 → 已生成完成
- `Assets/TJGenerators/History/<name>_model/` 目录存在且包含 `.fbx` / `.obj` / `.zip` → 已生成完成

可用 `glob("Assets/TJGenerators/History/*_model/*.{fbx,obj,zip}")` 找已完成任务。

---

**Task ID Format**：
- Tripo P1：`tripo_model_{counter}_{timestamp}`
- Rodin：`static_model_{counter}_{timestamp}`

**Notes**：
- 输出模型由 Unity 原生 FBX / OBJ 导入（插件已移除 GLTFast / GLB 专用链路）；Prefab 自动绑定模型为子节点
- 自动应用 `TuanjieAI` 标签
- 长任务（3–15 分钟），需 Unity Editor 一直在线
- domain reload 任务自动恢复（除非 Rodin 显示 `interrupted`）
- 消耗 AI 服务额度
