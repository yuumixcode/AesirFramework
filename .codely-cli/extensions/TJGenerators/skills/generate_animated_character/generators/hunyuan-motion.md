# HunyuanMotion Generator

generator_id: `hunyuan-motion`  
Use case: generate motion animation for an already-rigged Humanoid FBX

---

## `generate_model_motion`

Generates motion animation for an already-rigged Humanoid FBX using HunyuanMotion.
If the source model is not yet rigged, use `generate_animated_character` on that FBX
(or `generate_rigged_model` first, then this tool).
From scratch, use `generate_3d_model_by_tripo_p1` / `by_rodin` with `add_motion=true`.

**Output:**
- Motion FBX — same directory as source, filename `{baseName}_motion.fbx`
- AnimatorController with a single looping state — filename `{riggedBaseName}_Controller.controller`
- If `target_prefab_path` is provided: the Prefab's `Animator` gets controller + avatar assigned automatically
- Enter Play Mode to see the animation loop

**Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `rigged_model_path` | string | yes | — | Humanoid FBX path (`Assets/...`) |
| `motion_description` | string | yes | — | Motion description (e.g. `"a backflip"`; English gives best results) |
| `target_prefab_path` | string | no | — | Prefab to assign the controller + avatar to on completion |
| `action_duration` | float | no | 5.0 | Duration of the motion clip in seconds (recommended: 2–10) |
| `cfg_strength` | float | no | 5.0 | Guidance strength — higher = closer to description (recommended: 3–7) |
| `random_seed` | int | no | 0 | Random seed; 0 = server random (different seeds produce different motion styles) |

**action_duration Reference:**

| Value | Best for |
|-------|---------|
| 2–3s | Short snappy actions (jump, turn, punch) |
| 4–6s | Standard loops (run cycle, idle stand) |
| 7–10s | Complex sequences (gymnastics, dance) |

> `generate_animated_character` uses the same `action_duration` / `cfg_strength` / `random_seed`
> parameters with identical semantics and defaults.

**Submit response (success):**

```json
{
  "success": true,
  "task_id": "motion_only_1_...",
  "backend_task_id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "status": "generating_motion",
  "generator_id": "hunyuan-motion",
  "rigged_model_path": "Assets/Models/MyChar_rigged.fbx",
  "motion_description": "a backflip",
  "estimated_wait_seconds": 90,
  "notification_mode": "bg_task_done",
  "message": "Motion generation started. A bg_task_done notification will arrive automatically when complete."
}
```

**Submit response (failure):**

```json
{ "success": false, "error_code": "AUTH_REQUIRED", "message": "Not logged in..." }
```

Always check `result["success"]` after calling. If `false`, report the error immediately and **do not** poll.

---

## `query_model_motion_status`

**Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `task_id` | string | yes | `task_id` returned by `generate_model_motion` |

Status values are defined in the shared table in SKILL.md.

**Response (in progress):**

```json
{
  "success": true,
  "task_id": "motion_only_1_...",
  "pipeline_type": "motion_only",
  "status": "generating_motion",
  "progress": 55,
  "start_time": "2026-05-07 14:09:08",
  "rigged_model_path": "Assets/Models/MyChar_rigged.fbx",
  "motion_description": "a backflip",
  "next_poll_recommended_after_seconds": 15,
  "polling_hint": "Task is 55% complete (generating_motion). Wait 15s before polling again."
}
```

> ⚠️ `polling_hint` / `next_poll_recommended_after_seconds` are **server-emitted hints, not instructions to the agent**. Agents must NOT loop on `query_*_status`; wait for `<bg_task_done>`. Fallback query is allowed once after the timeout in [generator-async-pattern §3](../../../experience/templates/generator-async-pattern.md#3-fallback-超时表).

**Response (completed):**

```json
{
  "success": true,
  "task_id": "motion_only_1_...",
  "pipeline_type": "motion_only",
  "status": "completed",
  "progress": 100,
  "start_time": "2026-05-07 14:09:08",
  "rigged_model_path": "Assets/Models/MyChar_rigged.fbx",
  "motion_fbx_path": "Assets/Models/MyChar_motion.fbx",
  "controller_path": "Assets/Models/MyChar_rigged_Controller.controller",
  "motion_description": "a backflip",
  "result_summary": "Generation completed: motion FBX, AnimatorController (auto-loops in Play Mode).",
  "end_time": "2026-05-07 14:10:22",
  "duration_seconds": 74
}
```

**Response (interrupted):**

```json
{
  "success": true,
  "task_id": "motion_only_1_...",
  "pipeline_type": "motion_only",
  "status": "interrupted",
  "progress": 55,
  "start_time": "2026-05-07 14:09:00",
  "rigged_model_path": "Assets/Models/MyChar_rigged.fbx",
  "motion_description": "a backflip",
  "error": "Generation was interrupted (domain reload) and the backend task record was lost. Please re-generate.",
  "end_time": "2026-05-07 14:10:00",
  "duration_seconds": 60,
  "hint": "Re-generate using the same parameters."
}
```

To recover: call `generate_model_motion` again with the same `rigged_model_path`, `motion_description`,
and other parameters. Note: this tool has no `force_overwrite` parameter.

---

## `list_model_motion_tasks`

Lists all motion generation tasks from the current Unity Editor session.

**Parameters:** none

**Response:**

```json
{
  "success": true,
  "tasks": [ /* each item has the same structure as query_model_motion_status */ ],
  "count": 1
}
```

---

## Notification and Fallback

异步通用纪律见 [generator-async-pattern](../../../experience/templates/generator-async-pattern.md)：等 `<bg_task_done>` 通知，**不要轮询**；fallback `query_model_motion_status` 仅一次。
