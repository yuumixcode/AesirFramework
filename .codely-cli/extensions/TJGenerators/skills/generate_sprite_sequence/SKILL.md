---
name: unity-sprite-sequence-generation
description: Generate 2D sprite sequence animations (frame-by-frame AnimationClip) in Unity using AI from a character reference image. Use this skill whenever the user wants to create a 2D character animation, frame animation, sprite sheet animation, or action frames from an existing character image — even if they say "帮我给角色做个待机动画", "generate walk cycle frames", "make my sprite animate", "给这个角色生成序列帧", or "create idle/run animation for my 2D character". Trigger proactively for any 2D animation asset creation request in Unity where the user has (or will have) a reference character image, including idle, run, walk, attack, or any sprite-based motion.
---

> **执行前读取**：[共享执行约定](../../experience/templates/generator-async-pattern.md)。
> 主 agent 按需委托 `general-purpose`；已在子代理中则直接执行本 skill，不再委托。已提交的阶段从原任务续接。

> ⛔ **场景放置规则**（本 skill **无 placeholder**）
> - **调用方式**：`activate_skill("unity-place-assets-in-scene")` → 按 §4g AnimationClip 模板用 `exec_editor_script` 建 `GameObject + SpriteRenderer + Animator + AnimatorController`（**不是** `execute_custom_tool`）。
> - **子代理**：等 `<bg_task_done>` 拿到 `animation_clip_path` 后**调一次**配 Animator + AnimationClip，**不要再调第二次**。
> - **主 agent**：报告里的 `animation_clip_path` 是"已放置"的证据，不是"请你放置"的指示，**不要再调**。
> - **例外**：用户明确要"再加一个实例"时才再次调用。详见 [共享执行约定](../../experience/templates/generator-async-pattern.md)。

# Generate 2D Sprite Sequence Animation in Unity 🎬

Generate 2D sprite sequence animations in Unity using AI, from a character reference image.
Output：多个 **Sprite PNG 帧** + 一个 **AnimationClip (`.anim`)** 驱动 `SpriteRenderer.m_Sprite`，自动保存到 `Assets/TJGenerators/History/Sequence_yyyyMMdd_HHmmss/`。

支持三种动画类型：`idle` / `frontRun` / `backRun`，可配置帧率与是否循环。

## 🚦 执行四步（不要跳读外链）

> ⚠️ **本 skill 无 placeholder**——提交后没有可立即放置的资产，需要等待任务成功后才能放置。

1. 调 `generate_sprite_sequence`（需要 `image_path`）→ 拿 `task_id`（**没有** `placeholder_path`）
2. **跳过场景放置**（没有占位资产可放）
3. 在当前执行者内按共享约定有间隔等待，调用 `query_local_task` 确认当前阶段的终态
4. 确认当前任务 `completed` 后 → 读 `animation_clip_path` → `activate_skill("unity-place-assets-in-scene")`，**此时用 `exec_editor_script` 放置一次**（资产类型 `AnimationClip`）→ 建 GameObject + SpriteRenderer + Animator + Controller

**档位**：短/中任务 60–180 秒；无通知时按共享约定等待后查询 `query_local_task`。完整 async 规则见 [共享执行约定](../../experience/templates/generator-async-pattern.md)。

## 立绘到动画的串行流程

没有参考图时，在同一个执行者内加载 `unity-sprite-generation`，等待静态图完成后用其真实 `image_path` 提交序列帧，不能把占位图当输入。序列完成后把 AnimationClip 接到目标 Animator，并沿用静态精灵的 `localScale`。只要动画时复用同一角色；用户同时要独立立绘和动画展示时用不同名称分别放置，保持 scale 一致，不重复创建同角色实例。报告图像/动画路径、Controller、目标对象及 scale。

## ⚠️ Skill 独有约束

1. **`image_path` 必须提供**——本 skill 只做图驱动生成，没有纯文本模式；用户没有角色图时建议先用 `generate_sprite` 生成一张。
2. **`image_path` 不前置校验**——文件不存在/路径错时任务**仍会启动**，几秒后才转 `failed`，浪费一次提交。先确认路径再调用。
3. **`animation_type` 仅 3 个有效值**——`"idle"` / `"frontRun"` / `"backRun"`（**大小写敏感**）。其他值会**静默回退到 `idle`**，不报错。
4. **并发上限 5**——同时运行的 sprite_sequence 任务最多 5 个。
5. **资产类型 `AnimationClip`**——放置 skill 的模板会**端到端建立**：创建 GameObject + SpriteRenderer + Animator + AnimatorController + 把 AnimationClip 绑成默认 state。**不需要**用户预先准备这套结构。

## 与 generate_sprite 的差异速查

| 维度 | `generate_sprite_sequence` | `generate_sprite` |
|---|---|---|
| 输入 | **必须有参考图**（image_path required） | prompt 或 image_path 至少一个 |
| 输出 | N 帧 PNG + 1 个 `.anim` | 1 张 PNG（带 alpha） |
| Placeholder | **❌ 无** | ✅ 1×1 灰色 Sprite |
| 资产类型 | `AnimationClip` | `Sprite` |
| 场景结构 | GameObject + SpriteRenderer + **Animator + AnimatorController** | GameObject + SpriteRenderer 或 UI Image |
| 用途 | 帧动画（idle/run） | 静态精灵（图标/立绘） |
| 内容预设 | `animation_type` 仅 3 种 | `type_id` 30+ / `style_id` 30+ |
| 并发上限 | 5 | 5 |

> **典型工作流**：先用 `generate_sprite` 出角色立绘 → 再用 `generate_sprite_sequence` 给这张图做动画。同一通用执行者 在用户同时要立绘+动画时会自动编排这个两阶段流程。

## When to Use / NOT to Use

适用：2D 角色动画、帧动画、idle/run/walk/attack 动作、敌人 NPC 动效、像素角色循环动画。

不适用：
- 静态 2D 图标 / 道具 / 立绘 → `generate_sprite`
- 无参考图、用户也无法提供 → 先用 `generate_sprite` 出图
- 3D 模型动画 → `unity-animated-character-generation`（MCP `unirig_rig` / `generate_motion` + `import_3d_model_from_url`，或从零 `generate_3d_model` + 导入 `add_motion`）
- 自定义动作（非 idle/frontRun/backRun） → 不支持，只能选这 3 种之一
- 天空盒 / 音乐 / 通用图 → 各自专属 skill

## 工具

所有工具通过 `execute_custom_tool` 调用。

### `generate_sprite_sequence`

```python
execute_custom_tool(
  tool_name="generate_sprite_sequence",
  parameters={
    "image_path": "Assets/Characters/hero.png",  # ⚠️ Required
    "generator_id": "sprite_sequence_v1",         # 唯一可用 generator（默认）
    "animation_type": "idle",                     # "idle" | "frontRun" | "backRun"，默认 "idle"
    "fps": 12,                                    # AnimationClip 帧率 1–60，默认 12
    "loop": True,                                 # AnimationClip 是否循环，默认 true
  }
)
```

### 返回字段

- `success`: bool
- `task_id`
- `animation_type` / `fps` / `loop`：回传
- `estimated_wait_seconds` ≈ 90
- `notification_mode: "bg_task_done"`
- **没有 `placeholder_path`**——这是本 skill 与其他 generator 的核心区别

提交失败时 `result["success"] == false`，读 `error_code` / `message`，**不要**poll。

> 初始 `progress` 总是 `0`；用 `query_local_task` 看实时进度（按共享约定先等待再查）。

### `<bg_task_done>` 独有字段

通用字段见模板。本 skill 额外字段：

| 字段 | 说明 |
|---|---|
| `folder_path` | 输出目录（包含所有帧 PNG 和 `.anim`） |
| `frame_count` | 生成的帧数 |
| `animation_clip_path` | `.anim` 文件路径（**主要消费目标**，用于放置 skill 的 AnimationClip 脚本） |
| `preview_url` | 第一帧预览 URL 或本地路径 |
| `generator_id` | 使用的生成器 |

> 注意：本 skill 是图驱动，通知里**没有 `prompt` 字段**。

### `query_local_task` / `list_local_tasks`

`query_local_task` 仅作 fallback（120 秒后单次）。返回字段同 `<bg_task_done>` payload，外加 `progress`（实时 0–100）。

`list_local_tasks` 返回当前 session 的所有 sprite_sequence 任务。

## 动画类型

| `animation_type` | 中文 | 描述 |
|---|---|---|
| `idle` | 待机 | 角色站立，含细微动作（呼吸、轻微摆动）**(默认)** |
| `frontRun` | 向前跑 | 朝镜头方向跑（向前） |
| `backRun` | 向后跑 | 远离镜头方向跑（向后） |

> **仅这三个值有效。** 其他值会静默回退到 `idle`。

## 输出资产

输出目录：`Assets/TJGenerators/History/Sequence_yyyyMMdd_HHmmss/`

| 资产 | 描述 |
|---|---|
| `frame_0001.png` … `frame_XXXX.png` | 帧 PNG，自动导入为 `TextureImporterType.Sprite` |
| `Sequence_xxx.anim` | AnimationClip，关键帧驱动 `SpriteRenderer.m_Sprite` |

通知里的 `animation_clip_path` 指向 `.anim` 文件。`folder_path` 是包含所有产物的父目录。

## 参数速查

| 参数 | 类型 | 默认 | 说明 |
|---|---|---|---|
| `image_path` | string | **required** | 角色参考图 PNG/JPG，主体清晰，背景透明或简单 |
| `animation_type` | string | `"idle"` | **仅** `"idle"` / `"frontRun"` / `"backRun"` |
| `fps` | int | `12` | 帧率 1–60 |
| `loop` | bool | `true` | 循环或单次 |
| `generator_id` | string | `"sprite_sequence_v1"` | 唯一可用 |

### `fps` 决策

| 用途 | 推荐 FPS |
|---|---|
| 经典像素 / 复古 | 8–12 |
| 移动端流畅角色 | 12–18 |
| 高质量动画 | 24 |
| 极快动作 / 粒子 | 30+ |

### `loop` 决策

| 动画 | Loop |
|---|---|
| Idle / walk / run / hover | `true`（默认） |
| Attack / jump（一次性） | `false` |
| Death / celebrate（播一次） | `false` |

## 使用示例

### 待机动画

```python
result = execute_custom_tool(
    tool_name="generate_sprite_sequence",
    parameters={
        "image_path": "Assets/Characters/warrior.png",
        "animation_type": "idle",
        "fps": 12,
        "loop": True
    }
)
task_id = result["task_id"]

# 在当前执行者内有间隔等待/查询该阶段的终态，再继续后续步骤（见共享执行约定）
# 等任务 completed，再 activate_skill("unity-place-assets-in-scene")，按指引用 exec_editor_script 放置
```

### 并发生成 idle/frontRun/backRun 完整套装

```python
image = "Assets/Characters/goblin.png"
task_ids = {}

for anim_type in ["idle", "frontRun", "backRun"]:
    result = execute_custom_tool(
        tool_name="generate_sprite_sequence",
        parameters={
            "image_path": image,
            "animation_type": anim_type,
            "fps": 12,
            "loop": True
        }
    )
    task_ids[anim_type] = result["task_id"]
    # ✅ 直接继续，不 poll

# 在当前执行者内有间隔等待/查询该阶段的终态，再继续后续步骤（见共享执行约定）
return f"Started 3 animations. Task IDs: {task_ids}"
```

## 放入场景（**通知到达后再做**）

本 skill **无 placeholder**，必须等任务成功拿到真实 `animation_clip_path` 后才能放置。

资产类型 **`AnimationClip`**，路径用通知里的 `animation_clip_path`。Scene side-effect 一站式建立完整结构：

1. 创建 GameObject + SpriteRenderer（若场景没有匹配名）
2. 创建 AnimatorController（保存到 `folder_path` 同目录）
3. 把 AnimationClip 设为 default state
4. 给 GameObject 加 Animator 并绑定 controller

用户**不需要**预先创建任何场景结构。规则见 [共享执行约定](../../experience/templates/generator-async-pattern.md)。

## 故障排查

### Skill 独有问题

> 通用故障（配置缺失 / 任务卡住 / 状态异常 / 未登录）见 [共享执行约定](../../experience/templates/generator-async-pattern.md)。

| 问题 | 原因 | 解决 |
|---|---|---|
| `'image_path' is required for sprite sequence generation` | 没传参考图 | 本 skill 必须有图；用户没有时先用 `generate_sprite` 出图 |
| 任务启动后立刻 failed | `image_path` 路径错误（不前置校验） | 提交前用 `read_file` / `glob` 确认路径存在 |
| 动画类型回退到 idle | `animation_type` 拼错或大小写错 | 仅支持 `"idle"` / `"frontRun"` / `"backRun"`（驼峰）；不要写 `"front_run"` 或 `"FrontRun"` |
| AnimationClip 存在但角色不动 | 缺 SpriteRenderer / Animator / Controller | 按放置 skill 的 AnimationClip 模板用 `exec_editor_script` 建立完整结构；只能在 Play Mode（或 Animation 预览面板）看到动画 |
| 帧率不对 | `fps` 默认 12 | 在已有 Animator 状态的 Speed 字段调整速度，避免重复生成 |

### Domain reload 后恢复

通用恢复流程见 [共享执行约定](../../experience/templates/generator-async-pattern.md)。查询原任务并核对返回目录内的帧数、Sprite 类型、AnimationClip 的 Sprite 曲线，以及 Animator/Controller（若要求放置）。目录或 .anim 存在不能单独证明成功；未完成先恢复原任务，不重新生成。

---

**Task ID Format**：`sprite_sequence_{counter}_{timestamp}`

**Notes**：
- 帧 PNG 自动导入为 `TextureImporterType.Sprite`
- AnimationClip 关键帧驱动 `SpriteRenderer.m_Sprite` 属性
- 自动应用 `TuanjieAI` 标签
- **并发上限 5**——批量生成超过 5 个时分批做
- 需 Unity Editor 在线运行；消耗 AI 服务额度
- 想做"立绘 + 动画"完整套装时，同一通用执行者会自动串联 `generate_sprite` → `generate_sprite_sequence`
