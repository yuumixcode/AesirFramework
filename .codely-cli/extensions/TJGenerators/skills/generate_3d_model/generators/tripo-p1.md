# Tripo P1 生成器文档

generator_id: `tripo-p1`  
默认 model_version: `P1-20260311`  
适用场景：移动端 / 小游戏 / 低面数要求 / 通用低多边形 3D 资产

---

## 何时选择 Tripo P1

- 默认选择：用户未明确指定模型精度时
- 移动端 / 小游戏 / 试玩广告 / 包体大小严格受限
- 明确要求「低面数、低模、polycount 限制」
- 批量生成需要稳定控制面数与体积

---

## 工具

### `generate_3d_model_by_tripo_p1`

启动 Tripo P1 生成任务。

**参数：**

| 参数 | 类型 | 必填 | 默认 | 说明 |
|------|------|------|------|------|
| `prompt` | string | 无图时必填 | — | 文生3D提示词（≤1024字符） |
| `image_path` | string | 无提示词时必填 | — | 图生3D：Unity 资产路径（`Assets/...`）或绝对路径 |
| `multiview_image_paths` | string[] | 否 | — | 多视图路径数组，顺序必须为 `[front, left, back, right]`，至少3张，front 必须存在 |
| `model_version` | string | 否 | `P1-20260311` | 模型版本 |
| `face_limit` | int | 否 | — | **低面控制**，P1 建议范围 48~20000 |
| `texture` | bool | 否 | true | 是否生成纹理 |
| `pbr` | bool | 否 | true | 是否生成 PBR 材质 |
| `texture_seed` | int | 否 | — | 纹理种子 |
| `texture_quality` | string | 否 | — | `standard` / `detailed` |
| `texture_alignment` | string | 否 | — | `original_image` / `geometry`（图生/多视图常用） |
| `orientation` | string | 否 | — | `default` / `align_image`（图生/多视图常用） |
| `auto_size` | bool | 否 | — | 自动尺寸 |
| `compress` | string | 否 | — | 例如 `geometry` |
| `export_uv` | bool | 否 | true | 关闭可显著提速并减小体积 |
| `with_mesh_segmentation` | bool | 否 | false | 启用模型分割，将模型按语义拆分为多个子网格 |
| `prefab_output_path` | string | 否 | 自动生成 | 输出 prefab 路径（`.prefab` 自动添加） |
| `force_overwrite` | bool | 否 | false | 覆盖同名 prefab |
| `session_id` | string | 否 | — | 为占位符 prefab 添加 Session 标签，用于 agent session 分组 |
| `add_motion` | bool | 否 | false | 与 UI「添加动作」相同：主模型落地后自动 UniRig + HunyuanMotion |
| `motion_description` | string | `add_motion` 时必填 | — | 英文动作描述，例如 `a walking cycle` |

**⚠️ P1 约束（重要）：**

`model_version` 为 `P1-20260311` 时，**不要**传以下参数，官方 API 会报错：
- `quad`
- `smart_low_poly`
- `generate_parts`
- `geometry_quality`

**返回（成功）：**

```json
{
  "success": true,
  "task_id": "tripo_model_1_...",
  "backend_task_id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "status": "submitted",
  "generator_id": "tripo-p1",
  "model_version": "P1-20260311",
  "prefab_output_path": "Assets/TJGenerators/History/Tripo3D.prefab",
  "notification_mode": "bg_task_done"
}
```

**返回（失败）：**

```json
{ "success": false, "error_code": "AUTH_REQUIRED", "message": "Not logged in..." }
```

调用前检查 `result["success"]`。若 `false`，立即上报错误，**不要**继续轮询。

---

### `query_3d_model_status_by_tripo_p1`

查询任务状态（**fallback only，仅一次**——见 [generator-async-pattern §2](../../../experience/templates/generator-async-pattern.md#2--polling-is-strictly-forbidden)）。

**参数：**

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `task_id` | string | 是 | `generate_3d_model_by_tripo_p1` 返回的 `task_id` |

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
  "task_id": "tripo_model_1_...",
  "status": "completed",
  "progress": 100,
  "prefab_path": "Assets/TJGenerators/History/Tripo3D.prefab",
  "model_path": "Assets/TJGenerators/History/Tripo3D_model/Tripo3D_model.fbx",
  "preview_url": "https://example.com/preview.png"
}
```

**返回（进行中）：**

> ⚠️ Fallback 查询规则见 [generator-async-pattern §3](../../../experience/templates/generator-async-pattern.md#3-fallback-超时表)：长任务 300 秒后才允许调用，仅一次。

```json
{
  "success": true,
  "status": "generating",
  "progress": 45
}
```

---

### `list_3d_model_tasks_by_tripo_p1`

列出当前 Unity Editor session 内的所有 Tripo P1 任务。

**参数：** 无

---

## 输入模式

| 模式 | 参数 | 适用场景 |
|------|------|---------|
| 文生3D | `prompt` | 文字描述生成 |
| 图生3D | `image_path` | 从参考图生成 |
| 文+图 | `prompt` + `image_path` | 带文字指导的参考图生成 |
| 多视图 | `multiview_image_paths` | 4个角度（front/left/back/right）生成 |

---

## 通知与 Fallback

异步通用纪律见 [generator-async-pattern](../../../experience/templates/generator-async-pattern.md)：等 `<bg_task_done>` 通知，**不要轮询**；fallback 仅一次。
