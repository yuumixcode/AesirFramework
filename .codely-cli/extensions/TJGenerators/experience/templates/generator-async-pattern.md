# Unity 生成任务：委托、等待与完成

生成 skill 的共享执行约定。MCP 的实时 schema、返回的轮询命令和导入指引是后端契约；各 skill 补充 Unity 资产类型、后处理和场景接线。本文只维护一次跨资源类型的执行规则。

## 委托给通用子代理

- 主 agent 加载匹配的 skill。同步查询、版本检查、已有资产的简单放置直接执行；耗时生成使用内置 `general-purpose`，不创建专属代理配置或规划代理。
- MCP 提交一次取得 `task_id` 后，按返回指引委托一个后台子代理等待、导入和验证。也可把尚未提交的完整流程委托给它。Unity 原生生成链同样可整体委托。
- 委托 prompt 必须带上：skill 名称及文件路径、用户需求和已确认参数、参考输入、输出路径、目标对象/变换、是否放置，以及已提交的任务 ID、阶段、轮询元数据、已有产物和接线记录。子代理看不到主会话；不要只传“继续”。
- **同一任务只有一个执行者。** 已在子代理内时，直接加载 skill 并执行后续步骤，不再创建子代理。已提交的阶段只等待和取结果，不再次生成。主 agent 委托后不重复轮询、导入或放置。
- 多个独立资产可并行；同一资产的串行步骤留在同一个子代理中（如立绘→序列帧、截图→UI 图层、绑骨→动作、克隆→用户要求的 TTS）。受宿主并发槽位和工具限制约束，共用场景对象/输出路径的改动串行执行。
- 工具权限以当前宿主实际提供的工具为准，skill 不授予权限。缺少 MCP、`execute_custom_tool` 或脚本工具时，返回缺失项；不改用户全局配置、不创建 TOML 来绕过。

委托示意（参数以宿主 `task` schema 为准）：

```text
task(description="完成 Unity 资产任务", subagent_name="general-purpose", run_in_background=true,
     prompt="加载 <skill 名称，SKILL.md 路径> 及其共享执行约定。
     用户需求/已确认参数：<完整需求>。当前阶段：<未提交/生成中/待导入/导入中>。
     已有任务：<生成 ID、导入 ID、服务端轮询元数据，未提交则明确写无>。
     目标：<输出路径、目标对象、变换、放置要求、已完成的接线>。
     负责剩余流程直到状态完成且实际资产验证通过；恢复时续同一任务，不重复提交。")
```

## MCP 等待

1. 提交成功后记录任务 ID，实际执行响应中的 `poll_command_powershell`（PowerShell）或 `poll_command`（Bash/Git Bash）。返回命令不会自动启动等待。
2. 通用子代理已经是后台等待者，在其中用 `run_shell_command(run_in_background=false)` 执行轮询命令，不另起无人负责的后台进程或 Cron 定时器。保留服务端的 `poll_interval_seconds` 和任务范围；单次命令超过宿主时限时分段等待同一任务（例如不超过 240 秒）。
3. shell 等待结束（包括超时、`poll_error`）后，对同一个 ID 调一次 `check_task`，读取真实状态、产物 URL 和导入指引。脚本退出不代表生成成功；仍在运行时按服务端指引继续有间隔等待，不能忙循环 `check_task` 或重新提交。
4. 主 agent 已提供最终成功结果时直接从导入阶段继续。同步 MCP 工具没有异步任务时直接处理结果。

## 同名工具与路由

`generate_material`、`generate_game_ui_kit` 同时存在 MCP 版本和 Unity custom tool 版本。两个对应 skill 当前使用 `execute_custom_tool(tool_name=..., parameters=...)`，保留材质和图层导入等 Unity 后处理。不要因同名改成直接调用 MCP，也不要两边各提交一次。

两侧参数不可混用：Unity 材质用 `preset_id/style_id/pattern_id`，MCP 用 `material_preset/material_style/texture_pattern`；Unity UI Kit 用 `screenshot_path`，MCP 用 `screenshot_url`。若任务明确选择 MCP 路径，按其实时 schema 和返回的导入指引执行，不再调用 Unity 的同名生成工具充当导入器。

MCP `check_task` 只接收 MCP 提交得到的 ID；Unity 原生生成和本地导入统一使用 `query_local_task`，不能把本地 task ID 发给 MCP。

天空盒生成只走 MCP `generate_skybox`，本地使用 `import_image_from_url(import_type="skybox")` 导入，不再与原生生成入口混用。

## Unity 生成和导入等待

调用 skill 指定的 Unity custom tool，记录它返回的 task ID；MCP task ID 与 Unity task ID 不可互换。

本地只暴露 `query_local_task`、`list_local_tasks` 和 `list_session_assets`。query 传 `task_id`，Terrain 应用传 apply_task_id 的值（兼容同名参数）；恢复 ID 有歧义时加 `task_type`。list 支持 `task_type`、`status`、`offset`、`limit`，返回完整状态和产物字段。类型包括 image/image_layers（含 UI Kit 两阶段）、sprite、material、skybox（旧任务）、sprite_sequence、auto_sprite_sequence、terrain、terrain_apply、model_import、image_import、audio_import、video_import；路由任务不重复列出其底层任务。

在当前执行者中前台等待最多 30 秒，再调用该任务的 `query_local_task` 一次；进行中则再次等待后查询。`queued`、`running`、`generating`、`processing`、`downloading`、`importing`、`rigging`、`generating_motion`、`recovering` 都不是成功。不要结束子代理来等待通知。

`<bg_task_done>` 可以提前唤醒主 agent，但必须匹配任务 ID 和阶段。Unity 阶段通知不等于整个工作流结束；主 agent 等待执行者完成剩余接线和验证，不因中途通知重复执行后续阶段，也不把执行者正在查询的同一阶段通知再次转发来打断它。通知已给出完整终态时可直接验证资产，不必重复查询。

## 完成与续接

- 只有所有要求的阶段达到成功终态，并且真实资产能被 Unity 按预期类型加载，才报告成功。占位文件、计时结束、文件大小、面板停止转圈都不能单独证明完成。验证在 Edit Mode 进行：图片尺寸/类型，AudioClip 时长/采样，VideoClip 元数据，Prefab 的实际网格，动画/Avatar/Controller 等按任务选取。
- `complete_task` 是宿主工具，直接调用；它不是 `execute_custom_tool` 的 tool_name。普通最终文本也可能结束子代理，不用“已提交，稍后完成”作为成功交付。
- 内置通用代理有回合/时间上限，以当前宿主配置为准。临近上限仍运行时，以 `complete_task(status="partial", ...)` 交回完整续接信息：未完成阶段、原任务 ID、轮询元数据、已生成的 URL/本地路径、已放置对象和下一步。主 agent 继续同一任务；不能把子代理退出当成资产完成，也不能重做已完成阶段。
- 后端要求费用确认、缺少权限/输入或出现终态失败时，返回实际原因与现有产物；需要用户决定的事项交给主 agent，不能替用户确认费用或自动重生成。
- 最终报告包含各阶段 task ID、真实产物路径（克隆声音是 voice ID）、验证结果、创建/复用对象与关键设置、恢复或未完成事项。主 agent 据此汇报，不重复放置。

## 场景和文件边界

- 保留用户的生成参数、输出目录和放置要求；各 skill 的既有默认行为仍适用，用户明确“只生成资产/不放场景”时遵从。不要为了规划任务预先创建场景、保存计划文件、改摄像机或删对象。
- 通过 `activate_skill("unity-place-assets-in-scene")` 读取对应资产类型的接线方法；它不是 custom tool。先查同名/同角色目标并复用，不销毁重建。Prefab 用 `PrefabUtility.InstantiatePrefab`；Sprite 用空 GameObject + SpriteRenderer 或 UI Image。
- 同一资产对同一目标接线一次。已有占位引用在完成后更新，不再创建第二份对象。仅在用户要求新实例或改位置时增加/调整。
- 使用宿主的 `exec_editor_script` 执行进程内 C#；入口不可用时报告缺失，不猜测其他工具名。各文档的 C# 示例均使用这一入口。避免写 `.cs` 触发 reload；用户要求持久脚本时安排在活跃生成任务结束后。
- 验证脚本用顶层 `return` 返回证据，例如 `return Verify();`。只调用 `Verify();` 可能执行成功但返回 null；这不能当作验证通过，也无需重跑生成或导入。
- 不为验证进入 Play Mode。场景作为视频输入时，先在提交任何异步任务前完成截图；确需 Play Mode 才能取得输入时，仅在无活跃任务时短暂进入并恢复原状态。不假设另有 review agent 会验证结果。
- 共享场景设置（天空盒、摄像机等）由一个执行者修改；批量资产的共同风格和布局由主 agent 从用户需求中传入。

## 恢复和故障

`recovering` 表示宿主恢复中，继续等待查询原任务。`interrupted` / Task not found 时先查`list_local_tasks`、已有后端 ID 及实际输出；文件存在不代表任务成功，文件小也不代表必须重新生成。已知 URL 可在确认没有活跃导入后重试导入，不重做收费生成；覆盖已有路径仍须遵循用户要求。

| 问题 | 处理 |
|---|---|
| 缺 generator 配置 | 检查实际加载的 TJGenerators 包和配置；需要时用已有清除配置缓存菜单 |
| `AUTH_REQUIRED` | 向主 agent 报告需要 Unity Connect 登录 |
| 后端仍运行 / shell 超时 | 等待原任务，查询实际状态，不以预估时长判失败 |
| 输出格式不可导入 | 报告格式问题和已有 URL，不自动重生成或覆盖用户文件 |
| 面板仍 Processing 但任务已完成 | 以任务终态和实际资产验证为准，不重复提交 |
