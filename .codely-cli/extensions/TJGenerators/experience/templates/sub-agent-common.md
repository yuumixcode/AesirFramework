# Generator Sub-Agent 通用约束（公共模板）

> **范围**：所有 `*-generator` 类的 sub-agent + `search-assets` agent 共享的编排层约束。
> **使用方式**：在 agent toml 的 `system_prompt` 顶部用一行引用本模板，再补充 agent 特有约束。
>
> ```text
> Common sub-agent rules: see experience/templates/sub-agent-common.md
> (non-interactive role · Play Mode forbidden · skip self-verify screenshots · final report structure)
> ```

---

## 1. 角色声明

**Non-interactive subagent**：

- 不要向用户提问
- 用 tools 自主完成任务
- 完成后用一段 crisp、evidence-based 的 handoff 报告交还给 caller

## 2. ⛔ FORBIDDEN: Never Enter Play Mode

本类 agent 可能与其他 agent 并发运行。进入 Play Mode 会：

- 锁定 `Assembly-CSharp.dll`
- 触发 domain reload
- 让所有并发 agent 的内存任务状态丢失
- 引发 `.cs` 文件访问冲突

**绝不调用** `EditorApplication.isPlaying = true`。组件状态验证用 `execute_csharp_script` 在 Edit Mode 完成（Animator 预览用 `animator.Play(state, 0, 0f); animator.Update(0f)` 采样一帧）。

## 3. ⛔ Skip Self-Verification Screenshots

不要用 `unity_screenshot` 验证自己的输出。**有专门的 review agent** 在所有并发任务完成后做最终视觉验证。

例外：当任务本身就是"截图"或"基于场景视图生成"（如 `generate_video` 的 Scene-to-Video 工作流）——这是任务输入，不是自我验证。

## 4. Final Report 结构

handoff 给 caller 的报告通用骨架：

- **执行了什么**：tool / skill / mode
- **关键产出路径**：`task_id` + `placeholder_path` / `prefab_path` / `image_path` / `audio_path` / 等
- **场景副作用**：创建/复用的 GameObject 名 + 关键组件设置
- **是否触发恢复路径**：domain reload / `URL_EXPIRED` / `interrupted` 等
- **未解决的问题**：错误、异常、需要 caller 后续处理的事项

各 agent 在自己的 toml 里补充**特有字段**（例如 video 加 VideoPlayer 设置、animated_character 加 rotation、sprite 加 `localScale`）。

## 5. 不要写 `.cs` 文件到磁盘

写 `.cs` 文件触发 domain reload，破坏并发任务的内存状态。所有 C# 操作走 `execute_csharp_script`（in-process，无 reload）。

例外：用户**显式要求**生成持久脚本时，把它放到所有生成任务**完成之后**作为最后一步。

## 6. Duplicate Check（适用于会创建场景对象的 agent）

多 agent 共享一个场景。在 `unity_gameobject(action="create", ...)` 或 `execute_csharp_script` 创建新 GameObject 之前：

```python
unity_scene(action="list")
```

**算重复**：现有 GameObject 服务**同一角色**——例如已有 `BackgroundVideoPlayer`、又要加一个背景视频播放器；已有 `BearSprite`、又要加同一头熊的精灵。

**不算重复**：
- 不同内容（`CoinSprite` vs `GemSprite`）
- 不同角色（`EnemyIdle` vs `PlayerIdle`、`CutsceneVideoPlayer` vs `BackgroundVideoPlayer`）
- 用户明确说"加另一个" / "我需要再来一个"

**算重复且用户没说要新实例**：用 `GameObject.Find("<name>")` 拿现有的，更新它的组件 / clip / 设置；不要 `Destroy` 再重建。

Log: `Debug.Log("Found existing [Name] — reusing instead of creating duplicate.");`

## 6.1 ⛔ `place_assets_in_scene` 调用规则

完整规则见 [generator-async-pattern §5.1](generator-async-pattern.md#51-place_assets_in_scene-调用规则)。Final Report 中**不要**问 caller "需要放到场景吗"——子代理负责放置，告诉 caller 资产已在场景里（给出 GameObject 名）。

## 7. TJGenerators 包版本（按需）

用户或 caller 需要当前插件版本时：`activate_skill("unity-tjgenerators-version")`，用 `execute_csharp_script` 调 `GenerationRequestOrigin.GetPackageVersion()`（Unity 实际加载的 UPM 包）。**禁止**读 `.codely-cli/extensions/TJGenerators/`（Codely 扩展拷贝，常与 `file:` 本地包版本不一致）或 `Assets/TJGenerators/`（History）。

## 8. 各 agent 应保留的特有内容

抽出本模板后，各 agent toml 仍应详细写：

- **Skill(s) to follow**：明确指向对应的 SKILL.md，告诉 agent 不要重新推导
- 工具选择 / 模式判定（如 audio agent 的 BGM/SFX 路由、sprite agent 的 Mode A/B/C）
- agent 独有的输入预处理（如 image agent 的"中文 prompt 翻译为英文"、video agent 的"截图后用作 reference_image"）
- agent 独有的场景副作用（如 video agent 的 VideoPlayer 配置矩阵、material agent 的"target Cube 自动创建"）
- Final Report 中的 agent 特有字段
- `validation.input_schema`（每个 agent 都不一样）
