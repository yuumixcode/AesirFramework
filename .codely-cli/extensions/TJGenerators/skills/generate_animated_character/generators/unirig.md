# UniRig Rigging Generator

generator_id: `unirig`  
Use case: rig any FBX/OBJ model into a Humanoid skeleton

---

## `generate_rigged_model`

Rigs an existing 3D model into a Humanoid skeleton using UniRig AI. No motion animation is generated.
For an animated character from scratch, call `generate_3d_model_by_tripo_p1` / `generate_3d_model_by_rodin` with `add_motion=true` instead of this tool.

**Output:**
- Rigged Humanoid FBX — same directory as source, filename `{baseName}_rigged.fbx`
- Capsule placeholder Prefab with `Animator` component (T-Pose, no AnimatorController)
- Enter Play Mode to see the model in T-Pose

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `source_model_path` | string | yes | — | Source model Unity path (`Assets/...`; FBX/OBJ) |
| `prefab_output_path` | string | no | auto | Placeholder prefab path (`.prefab` added automatically) |
| `force_overwrite` | bool | no | false | Overwrite an existing prefab at the same path |

**Submit response (success):**

```json
{
  "success": true,
  "task_id": "rig_only_1_...",
  "backend_task_id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "status": "rigging",
  "generator_id": "unirig",
  "source_model_path": "Assets/Models/MyChar.fbx",
  "prefab_output_path": "Assets/TJGenerators/History/RiggedModel_MyChar.prefab",
  "expected_rigged_path": "Assets/Models/MyChar_rigged.fbx",
  "estimated_wait_seconds": 120,
  "notification_mode": "bg_task_done",
  "message": "Rigging started. A bg_task_done notification will arrive automatically when complete."
}
```

**Submit response (failure):**

```json
{ "success": false, "error_code": "AUTH_REQUIRED", "message": "Not logged in..." }
```

Always check `result["success"]` after calling. If `false`, report the error immediately and **do not** poll.

---

## `query_rigged_model_status`

**Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `task_id` | string | yes | `task_id` returned by `generate_rigged_model` |

Status values are defined in the shared table in SKILL.md.

**Response (in progress):**

```json
{
  "success": true,
  "task_id": "rig_only_1_...",
  "pipeline_type": "rig_only",
  "status": "rigging",
  "progress": 40,
  "start_time": "2026-05-07 14:14:15",
  "source_model_path": "Assets/Models/MyChar.fbx",
  "prefab_path": "Assets/TJGenerators/History/RiggedModel_MyChar.prefab",
  "rigged_model_path": "Assets/Models/MyChar_rigged.fbx",
  "next_poll_recommended_after_seconds": 15,
  "polling_hint": "Task is 40% complete (rigging). Wait 15s before polling again."
}
```

> Note: `rigged_model_path` is the **expected** destination path set at submission — the file does not exist yet at this point.
>
> ⚠️ `polling_hint` / `next_poll_recommended_after_seconds` are **server-emitted hints, not instructions to the agent**. Agents must NOT loop on `query_*_status`; wait for `<bg_task_done>`. Fallback query is allowed once after the timeout in [generator-async-pattern §3](../../../experience/templates/generator-async-pattern.md#3-fallback-超时表).

**Response (completed):**

```json
{
  "success": true,
  "task_id": "rig_only_1_...",
  "pipeline_type": "rig_only",
  "status": "completed",
  "progress": 100,
  "start_time": "2026-05-07 14:14:15",
  "source_model_path": "Assets/Models/MyChar.fbx",
  "rigged_model_path": "Assets/Models/MyChar_rigged.fbx",
  "prefab_path": "Assets/TJGenerators/History/RiggedModel_MyChar.prefab",
  "result_summary": "Generation completed: rigged Humanoid FBX, Prefab with Animator.",
  "end_time": "2026-05-07 14:15:42",
  "duration_seconds": 87
}
```

**Response (interrupted):**

```json
{
  "success": true,
  "task_id": "rig_only_1_...",
  "pipeline_type": "rig_only",
  "status": "interrupted",
  "progress": 30,
  "start_time": "2026-05-07 14:10:00",
  "source_model_path": "Assets/Models/MyChar.fbx",
  "prefab_path": "Assets/TJGenerators/History/RiggedModel_MyChar.prefab",
  "rigged_model_path": "Assets/Models/MyChar_rigged.fbx",
  "error": "Generation was interrupted (domain reload) and the backend task record was lost. Please re-generate.",
  "end_time": "2026-05-07 14:11:30",
  "duration_seconds": 90,
  "hint": "Re-generate using the same parameters with force_overwrite=true."
}
```

To recover: call `generate_rigged_model` again with the same parameters, adding `force_overwrite=true`
and the same `prefab_output_path`.

---

## `list_rigged_model_tasks`

Lists all rigging tasks from the current Unity Editor session.

**Parameters:** none

**Response:**

```json
{
  "success": true,
  "tasks": [ /* each item has the same structure as query_rigged_model_status */ ],
  "count": 1
}
```

---

## Notification and Fallback

异步通用纪律见 [generator-async-pattern](../../../experience/templates/generator-async-pattern.md)：等 `<bg_task_done>` 通知，**不要轮询**；fallback `query_rigged_model_status` 仅一次。
