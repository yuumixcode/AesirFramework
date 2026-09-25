# MCP 生成契约（第一段：文/图生 3D → 拿 CDN URL）

> **状态：生产路径。** 生成由 MCP 后端（`tripo-3d-generator`）完成，Unity 侧不再有
> `generate_3d_model_by_*` 提交/轮询工具。本文件是 [SKILL.md](../SKILL.md) 第一段的详细文档；
> 第二段（导入 + 后处理）由 Unity CustomTool `import_3d_model_from_url` 完成。

## 工具总览（全部为 MCP 工具）

| 工具 | 用途 | 何时用 |
|------|------|--------|
| `file_upload` | 本地/工程内图片 → 永久 CDN URL | 图生 / 多视图的参考图输入 |
| `generate_3d_model` | 提交文生 / 图生 / 多视图 3D 生成任务 | 所有静态 3D 生成（含从零带动画的网格段） |
| `check_task` | 查询任务状态并拿产物 URL | 执行服务端 poll 命令后查询一次，禁止忙循环 |

### `file_upload`（本地参考图 → CDN）

输入是 `Assets/...` 路径或本地绝对路径的图片时，必须先上传拿 CDN URL：

1. `file_upload(file_path)` → 返回 `already_uploaded` / `upload_url` / `file_url`
2. `already_uploaded == false` → 用 shell `curl -X PUT -T <file> "<upload_url>"` 上传字节并确认 HTTP 200
   （**不要**把 MCP Authorization 发给 CDN；`upload_url` 1 小时过期）
3. 之后把 **`file_url`**（永久有效）传给 `generate_3d_model` 的 `image_url` / `image_urls`，**不要**传 `upload_url`、不要传本地路径

### `generate_3d_model`（提交生成）

| 参数 | 类型 | 说明 |
|------|------|------|
| `mode` | string | `text_to_model` / `image_to_model`（恰好 1 张 `image_url`）/ `multiview_to_model`（2–4 张 `image_urls`，顺序 front(0°)→left(90°)→back(180°)→right(270°)，front 必须第一） |
| `provider` | string | `rodin`（默认）/ `tripo` / `hunyuan`；选择规则见下表 |
| `prompt` | string | 文生必填；可与参考图叠加做指导 |
| `negative_prompt` | string | 仅 tripo |
| `with_fbx` | bool | tripo/rodin 默认 true；**hunyuan 必须显式 `with_fbx=true`**（默认 GLB，导入链路只吃 FBX） |
| `confirm_cost` | bool | 高成本门槛（默认 500 积分）：返回费用提示时先征得用户同意，再以相同参数 + `confirm_cost=true` 重调；低于门槛直接执行 |
| `session_id` / `workspace_name` | string | 会话/工作区归属（host 自动注入） |

**provider 选择**：

| 场景 | provider | 详见 |
|------|----------|------|
| 默认 / 快速 / 轻量道具 / 低面（≤20000 面） | **rodin**（默认，tier 默认 Extreme-Low，最快） | [rodin.md](rodin.md) |
| 低面数小游戏 / 移动端、稳定面数批量、多视图 | **tripo**（P1 低模优化） | [tripo-p1.md](tripo-p1.md) |
| 高精度 hero 资产 / PBR / 高面数（>20000） | rodin 显式 `tier=Gen-2.5-Extreme-High` 或 tripo `model_version=v3.1-20260211`（仅明确要求高模时） | 对应 provider 文档 |
| 用户点名混元 | hunyuan（必须 `with_fbx=true`） | — |

各 provider 独有参数（tier / model_version / face_limit / quad / style / with_segmentation /
texture_mode / hd_texture 等）**必须先读对应 provider 文档**，参数集不通用。

### `check_task`（等待完成 + 拿 URL）——轮询纪律

`generate_3d_model` **立即返回** `task_id` + 任务级轮询元数据（`poll_url` / `poll_interval_seconds` /
`poll_attempts` / `poll_command`）。此后：

1. 按 [共享执行约定](../../../experience/templates/generator-async-pattern.md) 委托一个通用执行者；已经在子代理中则不再委托，实际执行服务端返回的 PowerShell/Bash 轮询命令。
2. 等待结束后对同一任务调用一次 `check_task`。仍在运行则继续有间隔等待；超时不代表失败，也不能据此重新提交生成。
3. 成功后读取产物 URL 和导入指引：导入工具接受 `.fbx` / `.obj` / `.zip`，以及可选 `rendered_image` 贴图 URL。若只有 `.glb`，报告格式不兼容，不自动重生成。

## 与第二段（导入）的衔接

从 `check_task` 结果拿到 URL 后，调 Unity CustomTool `import_3d_model_from_url`：

```
import_3d_model_from_url(
  model_url        = <check_task 返回的模型 URL>,      # .fbx / .obj / .zip
  rendered_image_url = <可选, rendered_image URL>,
  provider         = "rodin" | "tripo" | "tripo-texture" | "hunyuan",   # 选择导入后处理预设
  prompt           = <原始 prompt, 供 History 显示>,
  prefab_output_path = <可选>, force_overwrite = <可选>,
  session_id       = <可选>
)
```

完整参数与占位/通知流程见 SKILL.md 第二段；无 TJGenerators 环境的降级方案见 [import-recipe.md](import-recipe.md)。

## 成本与确认

- 生成任务**成功完成才扣积分**；失败不扣
- 低于门槛（默认 500 积分）直接执行；返回费用提示时：向用户展示设置/预估积分，**获得明确同意后**
  30 分钟内以相同参数 + `confirm_cost=true` 重调；点名模型 ≠ 同意费用
- 被拒的确认不会签发新授权：改回 `confirm_cost=false` 重新拿提示

## 常见错误

| 问题 | 原因 | 解决 |
|---|---|---|
| `check_task` 返回 `.glb` URL | 生成时未开 `with_fbx`（hunyuan 默认 GLB） | 报告已有产物格式不兼容；新的生成需要用户决定，提交前应显式 `with_fbx=true` |
| 高成本确认被拒后反复 true | 确认被拒不产生新授权 | 先 `confirm_cost` 留空重新获取费用提示 |
| 提交后没有通知 | 执行者的后台等待尚未结束 | 超过预估时长（3–15 分钟）后允许对同一 task_id `check_task` 一次 |
| 内容策略拦截 | 平台策略，不可绕过 | 告知用户原因；征得同意后才可用 `search_asset_lib` 搜资产库替代 |
