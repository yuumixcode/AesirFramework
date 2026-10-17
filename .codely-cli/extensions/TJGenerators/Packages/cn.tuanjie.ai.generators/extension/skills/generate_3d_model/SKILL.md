---
name: unity-3d-model-generation
description: "Generate 3D models in Unity — generation runs on the MCP backend (generate_3d_model + check_task), landing/import/post-processing runs in Unity via the import_3d_model_from_url custom tool. Use this skill whenever the user wants to create a standalone 3D object, prop, or asset — e.g. \"生成一个3D模型\", \"生成一把椅子\", \"create a 3D rock\", \"make a sword model\". Trigger for furniture, weapons, vehicles, buildings, food, props, environment assets, etc. DO NOT use for terrain. For an animated humanoid from scratch: this skill with import add_motion=true + motion_description (same as UI 添加动作). For an existing FBX: the animated-character skill."
---

> **执行前读取**：[共享执行约定](../../experience/templates/generator-async-pattern.md)。
> 主 agent 按需委托 `general-purpose`；已在子代理中则直接执行本 skill，不再委托。已提交的阶段从原任务续接。

> ⛔ **场景放置规则**
> - **调用方式**：`activate_skill("unity-place-assets-in-scene")` → 按 §4a Prefab 模板用 `exec_editor_script` 跑 `PrefabUtility.InstantiatePrefab`（**不是** `execute_custom_tool`，**不要** `unity_gameobject` 放 Prefab）。
> - **第二段提交后立即调一次**放占位 Prefab（`prefab_output_path`）；`query_local_task` 确认完成后**不再调**（占位子节点自动被真实模型替换）。
> - **主 agent**：报告里的 `prefab_path` 是"已放置"的证据，不是"请你放置"的指示，**不要再调**。
> - **例外**：用户明确要"换位置 / 改 scale / 再放一个"时才再次调用。详见 [共享执行约定](../../experience/templates/generator-async-pattern.md)。

# Generate 3D Model in Unity 🪑（MCP 生成 + Unity 导入）

生成 3D 模型资产。**两段式链路**：

- **第一段·生成（MCP 后端）**：`generate_3d_model` 提交 → 后台等待 → `check_task` 拿模型 CDN URL。详见 [generators/mcp.md](generators/mcp.md)。
- **第二段·导入 + 后处理（Unity CustomTool）**：`import_3d_model_from_url` 下载导入 → 材质重映射 / 法线修正 / auto-fit 归一化 / Prefab 绑定 → 本次等待结束后 `query_local_task` 单次拉取结果。

输出：3D 模型文件（FBX/OBJ/ZIP）+ 自动生成的 Prefab，保存到 `Assets/TJGenerators/History/`。
`add_motion=true` 时，网格落地后由导入工具内置管线自动 UniRig 绑骨 + HunyuanMotion 动作（与 UI「添加动作」一致）。

## 🚦 执行五步（不要跳读外链）

1. （图生 / 多视图时）`file_upload` 上传参考图 → 拿 `file_url` CDN URL
2. 调 MCP `generate_3d_model` → 拿 `task_id` → **在当前子代理内等待，不结束任务**（执行返回的 poll 命令后调用 `check_task`）
3. 下一轮收到生成结果（模型 URL + 可选 rendered_image URL）→ 调 `import_3d_model_from_url` → 拿 `task_id` + `prefab_output_path` → 立即 `activate_skill("unity-place-assets-in-scene")`，按 skill 用 `exec_editor_script` 放占位 Prefab → **在当前子代理内前台等待（每次最多30秒）（`estimated_wait_seconds`，add_motion 时 ~900s）** → **在当前子代理内等待，不结束任务**
4. 本次等待结束 → `query_local_task` **一次** → 读 `model_path` / `prefab_path`（占位子节点已原地替换为真实模型，**不要再 place**）；仍是 `importing` 则再进行一次前台短等待。**不要干等 Unity 侧 `<bg_task_done>` 推送**（不依赖通知恢复已结束的子代理），按共享约定等待后再查询
5. 向主 agent 报告（含 provider 选择理由、`task_id`、`prefab_path`、放置位置）

**档位**：第一段生成 3–15 分钟（tier / provider 影响很大；MCP 轮询为 host 管理通道、通知可靠，未收到时对同一 task_id `check_task` 一次即可）；第二段导入静态实测秒级（预估 60s）、add_motion ~5–15 分钟——一律有间隔等待 + 状态查询。

## Provider / 模式选择规则

| 场景 | provider | 子文档 |
|------|----------|--------|
| 默认 / 通用 / 快速 / 轻量道具 | **rodin**（MCP 默认，tier 默认 Extreme-Low 最快） | [rodin.md](generators/rodin.md) |
| 低面数小游戏 / 移动端 / 稳定面数批量 / 多视图（4 视角） | **tripo**（P1 低模优化） | [tripo-p1.md](generators/tripo-p1.md) |
| 高精度 hero / PBR / 高面数 | rodin 显式 `tier=Gen-2.5-Extreme-High`，或 tripo `model_version=v3.1-20260211`（仅用户明确要求高模） | 对应 provider 文档 |
| 用户点名混元 | **hunyuan**（必须 `with_fbx=true`） | [mcp.md](generators/mcp.md) |

> **调用前必须 `Read` 对应子文档**：MCP 的 provider 参数集互不通用（rodin 用 `tier` / `texture_mode`；
> tripo 用 `model_version` / `quad` / `style` / `with_segmentation`），照搬错 provider 的参数会被后端拒绝。
> 完整 MCP 契约（`file_upload` / 轮询纪律 / 成本确认）见 [mcp.md](generators/mcp.md)。

## When to Use / NOT to Use

适用：家具、武器、载具、建筑、食物、道具、环境资产等独立 3D 物件；以及从零带动画 Humanoid
（`add_motion=true`，由 `unity-animated-character-generation` 调用本流程）。

不适用：
- 地形 / 景观 / 峡谷 / 山脉 → `generate_terrain`
- 已有 FBX 只要绑骨/加动作 → `unity-animated-character-generation`（不要重复生成模型）
- 2D 精灵 / 图标 → `generate_sprite`
- 天空盒 → `generate_skybox`

## 工具速查

### 第一段：MCP 生成（详见 [mcp.md](generators/mcp.md)）

```
generate_3d_model(
  mode     = "text_to_model" | "image_to_model" | "multiview_to_model",
  provider = "rodin" | "tripo" | "hunyuan",
  prompt   = "wooden chair, simple shape, low poly",   # text_to_model 必填
  # image_to_model: image_url = <file_upload 返回的 file_url>
  # multiview:     image_urls = "<front>,<left>,<back>,<right>"（front 第一）
  # 带动画角色: 交给第二段 import 工具的 add_motion，不是 MCP 参数
  # with_fbx: hunyuan 必须 true（默认 GLB 导入链路不支持）
)
```

- 返回 `task_id` + 轮询元数据 → **在当前子代理内等待，不结束任务**，等 host 后台等待 + `check_task` 结果
- ⛔ 禁止 agent 循环 `check_task`；禁止对同一模型重复提交生成

### 第二段：Unity 导入（CustomTool）

```
execute_custom_tool(
  tool_name="import_3d_model_from_url",
  parameters={
    "model_url":          "<check_task 返回的 .fbx/.obj/.zip URL>",
    "rendered_image_url": "<可选，tripo 产物的主贴图>",
    "provider":           "rodin",            # rodin / tripo / tripo-texture / hunyuan
    "prompt":             "wooden chair",     # 可选，History 显示
    "prefab_output_path": "Assets/...",        # 可选，默认自动
    # 带动画角色（UI「添加动作」同款）:
    # "add_motion": True, "motion_description": "a walking cycle",
    "session_id": "<可选>"
  }
)
```

`tool_name` 可选：
- `import_3d_model_from_url` — 提交导入（立即返回 `task_id` + `prefab_output_path` 占位）
- `query_local_task` — **主要完成检查**：每次等待结束调用一次；`importing` 未完则继续前台等待后查询同一任务
- `list_local_tasks` — 列出会话内导入任务

`output_type` 扩展（绑骨/动画链路用，详见 animated-character skill）：
- `rigged` — 导入绑骨 FBX（MCP `unirig_rig` 产物），需 `source_model_path`（复用源模型贴图）
- `motion` — 导入动作 FBX（MCP `generate_motion` 产物），需 `rigged_model_path`，可选 `target_prefab_path` + `loop`

## 完成结果字段（query 返回 / 异步通知 payload）

| 字段 | 说明 |
|---|---|
| `model_path` | 导入后的模型资产路径 |
| `prefab_path` | Prefab 路径（== 提交时 `prefab_output_path`） |
| `output_type` | `static` / `rigged` / `motion` |
| `provider` | 静态导入的后处理预设 |
| `add_motion` / `motion_description` | 从零带动画时回显 |
| `target_prefab_path` / `rigged_model_path` / `controller_path` | motion 模式产物 |
| `session_id` / `start_time` / `end_time` / `duration_seconds` | 通用 |

## 放入场景

资产类型 **`Prefab`**，路径用 `prefab_output_path`。第二段提交后立即调一次（里面是占位）；
query 确认完成后**不要**再调——占位子节点自动被真实模型替换。

## 使用示例

### 默认（rodin）—— 简单道具

```
generate_3d_model(mode="text_to_model", provider="rodin",
                  prompt="wooden barrel, medieval style")
# → 在当前子代理内等待，不结束任务 → 生成结果 →
execute_custom_tool("import_3d_model_from_url", {
  "model_url": "<fbx_url>", "provider": "rodin",
  "prompt": "wooden barrel, medieval style"})
# → place 占位 → 前台等待最多 30 秒 → 在当前子代理内等待，不结束任务 → query 一次
```

### 图生 3D（任意 provider）

```
file_upload(file_path="Assets/ConceptArt/chair_concept.png")  # → file_url
generate_3d_model(mode="image_to_model", provider="tripo",
                  image_url="<file_url>", prompt="wooden chair, baroque style")
```

### 从零带动画角色（UI「添加动作」同款）

```
generate_3d_model(mode="text_to_model", provider="rodin",
                  prompt="a humanoid mecha robot, full body, standing upright, T-pose")
# → 在当前子代理内等待，不结束任务 → 生成结果 →
execute_custom_tool("import_3d_model_from_url", {
  "model_url": "<fbx_url>", "provider": "rodin",
  "add_motion": True, "motion_description": "a walking cycle"})
# 网格落地后自动 UniRig + HunyuanMotion（~12–20 分钟）
```

## 故障排查

> 通用故障（配置缺失 / 任务卡住 / 状态异常 / 未登录）见 [共享执行约定](../../experience/templates/generator-async-pattern.md)。

### Skill 独有问题

| 问题 | 原因 | 解决 |
|---|---|---|
| `check_task` 返回 `.glb` URL | 生成时未开 `with_fbx`（hunyuan 默认 GLB） | 报告格式不兼容；不要传 glb 给导入工具，也不要自动重生成 |
| 导入工具报 `Unsupported model_url extension` | URL 不是 `.fbx/.obj/.zip` | 同上；或确认拿到的是模型 URL 而非预览图 URL |
| 第一段提交后想再查一次 | host 后台等待还没结束 | 超过预估时长（3–15 分钟）后对同一 task_id `check_task` 一次 |
| 第二段导入被 domain reload 打断 | 编译触发重载 | **自动恢复**：任务转 `recovering` 并幂等重跑（URL 永久、下载路径确定性）；再进行一次前台短等待，query 一次，不要重复提交 |
| query 返回 `failed` | 下载/校验/导入错误 | 看 `error` 字段；URL 永久有效，修复原因后允许用相同 URL 重新 `import_3d_model_from_url`（`force_overwrite=true`） |
| 一直没收到导入完成通知 | 宿主不消费 Unity 侧推送（已知） | 按共享执行约定处理——标准完成模式就是前台等待 + `query_local_task` 状态查询 |
| 内容策略拦截（`content_moderation`） | 平台策略不可绕过 | 把拦截原因告知用户；征得同意后才可用 `search_asset_lib` 搜资产库替代 |

### 降级路径

无 TJGenerators custom tool 环境（或用户明确要求提示词化导入）时，用
[import-recipe.md](generators/import-recipe.md)：`run_shell_command` 下载 → `unity_editor refresh` →
`exec_editor_script` 跑模板脚本完成导入 + auto-fit（同步链路，无占位无通知）。

---

**Notes**：
- 导入模型由 Unity 原生 FBX / OBJ 导入；Prefab 自动绑定模型为子节点并按包围盒 auto-fit 到 ~1m
- 自动应用 `TuanjieAI` 标签（占位与产物）
- 第一段消耗 MCP 生成积分（成功完成才扣费）；第二段导入不消耗生成积分
- 第二段为长任务（含 add_motion 时 5–15 分钟），需 Unity Editor 一直在线；domain reload 自动幂等恢复
