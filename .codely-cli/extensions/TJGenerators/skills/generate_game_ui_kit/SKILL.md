---
name: unity-game-ui-kit-generation
description: Generate game UI asset kits in Unity via a two-step workflow (plus an optional fallback) — Step 1 generates a game UI screenshot from text (Seedream 5.0 Pro), Step 2 decomposes the screenshot into independent transparent PNG UI-element layers (base image + up to 16 layers, ready to use as sprites directly), Step 3 (fallback only) slices a merged layer via CV connected-component detection. Use this skill whenever the user wants to create game UI assets such as HUD layouts, inventory screens, buttons, panels, health bars, skill icons — e.g. "帮我生成游戏UI", "生成背包界面", "make a game HUD", "create UI kit for my game". Trigger proactively for any game UI design or UI element extraction request in Unity. Do NOT use for standalone 2D sprites/icons (use generate_sprite) or general images (use generate_image).
---

> ⚠️ **执行约束**
> - **主 agent**：无 `execute_custom_tool` 权限，必须 `task(subagent_name="game-ui-kit-generator", ...)` 委托，不要 `activate_skill` 后自己调。
> - **子代理（本文档主要读者）**：有权限，按下方 `execute_custom_tool(...)` 示例执行。

> ⛔ **`place_assets_in_scene` 调用规则**（本 skill **无 placeholder**）
> - **调用方式**：`activate_skill("unity-place-assets-in-scene")` → 按 §4b Sprite 模板用 `execute_csharp_script` 建 `SpriteRenderer` 或 Canvas 子节点 + `Image`（**不是** `execute_custom_tool`）。
> - **子代理**：Step 2 产出图层 PNG 后，**可选**调 `place_assets_in_scene` 把需要的 Sprite 放到场景。Step 1 **不调**（中间产物，不需要放置）。
> - **主 agent**：报告里的 `layer_paths` / `sliced_asset_paths` 是产出路径，不是"请你放置"的指示，**不要再调**。
> - **例外**：用户明确要"放到场景"时才调用。详见 [async-pattern §5.1](../../experience/templates/generator-async-pattern.md#51-place_assets_in_scene-调用规则)。

# Generate Game UI Kit in Unity 🎮

通过两步工作流生成游戏 UI 资产套件（外加一个兜底切割步骤）。
Output: Step 1 产出 UI 截图 PNG（2848×1600）；Step 2 产出 **1 张底图 + 最多 16 个透明 PNG 图层**（每个 UI 元素一个独立图层，直接可用作 Sprite），自动保存到 `Assets/TJGenerators/History/`。

## 工作流概览

| 步骤 | 输入 | 输出 | 用途 |
|---|---|---|---|
| Step 1 | 文本描述（无 `screenshot_path`） | UI 截图 PNG（2848×1600） | 预览 UI 布局设计 |
| Step 2 | Step 1 截图本地路径（`screenshot_path`） | 底图 + 最多 16 个透明图层 PNG（`layer_paths`） | **成品**：每层即一个可用的 UI 元素 Sprite |
| Step 3（仅兜底） | 某个仍含多个元素的图层 PNG | 多张独立 Sprite PNG | 图层被合并时才用 |

> ⚠️ **两步串行依赖**——Step 2 必须等 Step 1 完成后才能提交（需要截图路径）。Step 3 只有在某个图层仍包含多个被合并元素时才执行，同步操作。
>
> 💡 **provider**：默认 `seedream_pro`（Step 1 = Seedream 5.0 Pro 生图，Step 2 = Seedream 图层拆分）。`frontier` 为旧路径（Step 2 产出品红底 cutout sheet，必须配合 Step 3 切割）。**两步必须用同一个 provider**。

## 🚦 执行流程（不要跳读外链）

### Step 1：生成 UI 截图

1. 调 `generate_game_ui_kit`（**不传** `screenshot_path`）→ 拿 `task_id` + `placeholder_path`（1×1 灰色 PNG）
2. **跳过** `place_assets_in_scene`（中间产物，不放置）
3. **END RESPONSE TURN** — 不要 poll、不要 `query_game_ui_kit_status`、不要继续操作
4. 下一轮收到 `<bg_task_done>` → 读 `image_path`（截图本地路径）→ **立即提交 Step 2**

### Step 2：图层拆分（主力路径）

5. 调 `generate_game_ui_kit`（`screenshot_path` = Step 1 的 `image_path`，provider 与 Step 1 一致）→ 拿 `task_id` + `placeholder_path`
6. **END RESPONSE TURN** — 不要 poll
7. 下一轮收到 `<bg_task_done>` → 读 `layer_paths` / `layers_found`，按图层类型分流：
   - **`layer_0_path` 是底图（合成背景），不是元素**——不要当作 Sprite 使用
   - **元素美术层**（名称形如 `*_Art`）：**无动态文字**的透明图层，直接可用作 Sprite——动态文字已被刻意移除，数值/名字由引擎运行时用 Text/TMP 组件渲染
   - **动态文字层**（名称形如 `*_Dynamic_Text`）：只有烤死的文字，**仅作位置/字号/风格参考**（用于摆放运行时文本组件），**不要**当 Sprite 入库（数值会过期）
   - **可选**：调 `place_assets_in_scene` 把需要的美术层放到场景（资产类型 `Sprite`）
   - **仅当某个美术层仍包含多个元素时**（16 层上限导致小元素被合并）→ 对该层执行 Step 3

### Step 3（兜底）：CV 切割被合并的图层

8. 调 `slice_image`（`image_path` = 该图层路径，`background_mode` = `"transparent"`）→ 拿 `sliced_asset_paths` + `sliced_count`
9. 报告完成

**档位**：Step 1 约 30–90 秒；Step 2 约 60–180 秒（最多 17 张图下载）；120 秒内无通知才允许 `query_game_ui_kit_status` 一次。Step 3 同步返回。完整 async 规则见 [generator-async-pattern](../../experience/templates/generator-async-pattern.md)。

## ⚠️ Skill 独有约束

1. **两步串行**——Step 2 依赖 Step 1 的 `image_path`，不能并发。
2. **provider 两步一致**——默认 `seedream_pro`；`frontier` 是旧路径（品红 cutout sheet + 必须切割）。混用 provider 会导致 Step 2 输入与产出语义不匹配。
3. **`prompt` 在 Step 2 中被忽略**——Step 2 使用后端固定的图层拆分提示词，`prompt` 参数必须传但内容不影响结果。
4. **第 0 张是底图**——`layer_0_path` / `layer_paths[0]` 是重建的合成背景，不是元素 Sprite；元素在 `layer_paths[1..N]`。
5. **元素层无动态文字、文字独立成层**——美术层（`*_Art`）已去除动态文字（数值/名字/计时），游戏里由引擎运行时渲染文字；动态文字层（`*_Dynamic_Text`）只作位置/风格参考，不当 Sprite 用。
6. **16 层上限**——元素较多时小元素可能被合并进同一图层；此时对该图层调 `slice_image`（`background_mode: "transparent"`）兜底，不要对每个图层都切。
7. **`screenshot_path` 是本地路径**——C# host 自动上传到 CDN 并提交给后端，与 `generate_image` 的 `image_path` 模式一致。
8. **`quality` / `output_format` 仅 frontier 有效**——seedream_pro 路径忽略这两个参数；Step 2 可选 `size`（`"1K"`/`"1.5K"`/`"2K"`/`"auto"`，默认 `"2K"`）。
9. **Step 3 是同步操作**——`slice_image` 立即返回切割结果，不需要 `task_id` 或轮询。
10. **并发上限 5**——同时运行的 game_ui_kit 任务最多 5 个。

## When to Use / NOT to Use

适用：游戏 HUD 设计、背包界面、技能栏、主菜单 UI、设置面板、对话框样式、游戏 UI 元素提取。

不适用：
- 独立 2D 精灵（图标、立绘、道具） → `generate_sprite`
- 通用图片 / 概念图 / 纹理 → `generate_image`
- 任意图片分层（非 UI 场景） → `generate_image_layers`
- 3D 模型 / 材质 / 天空盒 → 各自专属 skill
- 已有 UI 截图、只需拆层 → 仍可用本 skill 的 Step 2（传入 `screenshot_path`）

## 工具

所有工具通过 `execute_custom_tool` 调用。

### `generate_game_ui_kit` — Step 1（生成 UI 截图）

```python
execute_custom_tool(
  tool_name="generate_game_ui_kit",
  parameters={
    "prompt": "fantasy RPG inventory screen with health bars, item slots, skill buttons",  # Required
    # screenshot_path: 不传（Step 1）
    # provider: 可选 "seedream_pro"（默认）/"frontier"
    # quality / output_format: 仅 frontier 有效
    # output_path: 不建议指定，默认 Assets/TJGenerators/History/
  }
)
```

**后端 prompt 增强**：后端会自动在用户 prompt 后追加 UI 设计关键词（`complete game UI screen design, full HUD layout, health bars, mana bars, buttons, panels, inventory grid, skill icons, mini-map, dialogue box, score display, clean professional game interface` 等），无需自己写这些。

### `generate_game_ui_kit` — Step 2（图层拆分）

```python
execute_custom_tool(
  tool_name="generate_game_ui_kit",
  parameters={
    "prompt": "fantasy RPG inventory screen",  # 必传但被忽略，用 Step 1 的原 prompt 即可
    "screenshot_path": "Assets/TJGenerators/History/GameUIKit_xxx.png",  # Required：Step 1 的 image_path
    # provider: 与 Step 1 一致（默认 seedream_pro）
    # size: 可选 "1K"/"1.5K"/"2K"/"auto"，默认 "2K"
  }
)
```

**后端 prompt**：Step 2 使用固定的图层拆分提示词（把每个 UI 元素/元素组拆到独立透明图层），用户 `prompt` 不影响结果。

### 返回字段

**Step 1 / Step 2 通用返回**：
- `task_id`
- `placeholder_path`：1×1 灰色占位 PNG，**立即可用**（但中间产物不需要放置）
- `step`：`1` 或 `2`
- `provider`：`"seedream_pro"` 或 `"frontier"`
- `notification_mode: "bg_task_done"`

提交失败时 `result["success"] == false`，读 `error_code` / `message`，**不要**poll。

### `<bg_task_done>` 独有字段

通用字段见模板。本 skill 额外字段：

**Step 1 完成时**：

| 字段 | 说明 |
|---|---|
| `image_path` | UI 截图本地路径 — **传给 Step 2 的 `screenshot_path`** |
| `preview_url` | 预览 URL |

**Step 2 完成时（seedream_pro 图层拆分）**：

| 字段 | 说明 |
|---|---|
| `layer_0_path` | **底图**（合成背景，非元素）路径 |
| `layers_folder` | 所有图层所在目录 |
| `layer_count` | 预期图层数（上限 17） |
| `layers_found` | 实际落盘图层张数 |
| `layer_paths` | 按序完整路径列表（`[0]` 底图；`*_Art` = 无字美术层直接用；`*_Dynamic_Text` = 动态文字层仅作参考）— **优先使用** |
| `preview_url` | 预览 URL |

**Step 2 完成时（frontier 旧路径）**：

| 字段 | 说明 |
|---|---|
| `image_path` | 品红底 cutout sheet 本地路径（需配合 Step 3 `solid_color` 切割） |

### `query_game_ui_kit_status` / `list_game_ui_kit_tasks`

`query_game_ui_kit_status` 仅作 fallback（120 秒后单次）。返回字段同 `<bg_task_done>` payload（seedream Step 2 任务额外有 `layer_paths`/`layers_found`/`layers_folder`），外加 `placeholder_path`（仅 `generating` 时）。

`list_game_ui_kit_tasks` 返回当前 session 的所有 game_ui_kit 任务（含 seedream Step 1/2 与 frontier 任务）。

## 参数速查

### generate_game_ui_kit

| 参数 | 类型 | 默认 | 说明 |
|---|---|---|---|
| `prompt` | string | **required** | Step 1: UI 描述；Step 2: 被忽略但必传 |
| `screenshot_path` | string | — | Step 2 only：Step 1 的 `image_path`。省略 = Step 1 |
| `provider` | string | `"seedream_pro"` | `"seedream_pro"` / `"frontier"`（旧品红路径），两步一致 |
| `size` | string | `"2K"` | Step 2 seedream only：`"1K"`/`"1.5K"`/`"2K"`/`"auto"` |
| `quality` | string | `"medium"` | 仅 frontier |
| `output_format` | string | `"png"` | 仅 frontier |
| `output_path` | string | — | 不建议指定，默认 `Assets/TJGenerators/History/` |

### slice_image

| 参数 | 类型 | 默认 | 说明 |
|---|---|---|---|
| `image_path` | string | **required** | 要切割的图层/cutout sheet 本地路径 |
| `background_mode` | string | `"auto"` | 图层 PNG 用 `"transparent"`；品红 sheet（frontier）用 `"solid_color"` |
| `color_tolerance` | float | `15` | 0-100，越大越多像素被判为背景 |
| `alpha_threshold` | float | `0.1` | 0-1，透明背景模式下使用 |
| `min_region_pixels` | int | `100` | 小于此像素的区域被忽略 |
| `padding` | int | `2` | 每个切割元素周围的额外像素 |
| `set_as_sprite` | bool | `true` | 自动设为 Sprite 导入模式 |

## 使用示例

### 完整流程

```python
# === Step 1: 生成 UI 截图 ===
result = execute_custom_tool(
    tool_name="generate_game_ui_kit",
    parameters={
        "prompt": "fantasy RPG inventory screen with health bars, item slots, skill buttons",
    }
)
if not result.get("success", True):
    raise RuntimeError(f"[{result['error_code']}] {result['message']}")

task_id = result["task_id"]
# ✅ END RESPONSE TURN — 等 bg_task_done
# 通知到达后读 image_path，提交 Step 2
```

```python
# === Step 2: 图层拆分 ===
# （在 Step 1 的 <bg_task_done> 到达后执行）
result = execute_custom_tool(
    tool_name="generate_game_ui_kit",
    parameters={
        "prompt": "fantasy RPG inventory screen",  # 原始 prompt，Step 2 忽略
        "screenshot_path": screenshot_path,        # Step 1 的 image_path
        # provider 默认 seedream_pro，与 Step 1 一致
    }
)
task_id = result["task_id"]
# ✅ END RESPONSE TURN — 等 bg_task_done
# 通知到达后读 layer_paths：
#   layer_paths[0]   = 底图（不要当元素用）
#   layer_paths[1..N] = 透明元素图层，直接可用
```

```python
# === Step 3（仅兜底）: 切割被合并的图层 ===
# 仅当某个 layer_paths[i] (i>=1) 仍包含多个独立元素时执行
result = execute_custom_tool(
    tool_name="slice_image",
    parameters={
        "image_path": merged_layer_path,        # 该图层路径
        "background_mode": "transparent",      # 图层已是透明背景
        "min_region_pixels": 50,
        "set_as_sprite": True
    }
)
if result.get("success"):
    sliced_paths = result["sliced_asset_paths"]
# ✅ 可选：place_assets_in_scene 放置图层/切割后的 Sprite
# ✅ 报告完成
```

### 跳过 Step 1（用户已有截图）

```python
# 用户提供了已有截图的本地路径
result = execute_custom_tool(
    tool_name="generate_game_ui_kit",
    parameters={
        "prompt": "existing UI",              # 必传但被忽略
        "screenshot_path": "Assets/UI/existing_screenshot.png",
    }
)
# 直接进入 Step 2
```

## CV 切割（Step 3 兜底：slice_image）

对仍包含多个元素的图层（或 frontier 品红 sheet）使用 `slice_image` 自动切割：

- **连通域检测**（8-connected BFS）自动找到每个独立元素
- **透明模式**（图层 PNG）：按 alpha 通道阈值分离前景
- **实色模式**（品红 sheet）：自动估计背景色并从边缘像素中扣除（color decontamination），消除品红残留边
- **羽化边缘**：2px box blur 产生软边缘，避免硬锯齿
- **逐个裁剪**：每个连通域裁剪为独立 PNG，自动设为 Sprite (2D and UI) Single mode

**返回字段**：

| 字段 | 说明 |
|---|---|
| `success` | 是否成功 |
| `sliced_count` | 切割出的 Sprite 数量 |
| `output_directory` | 输出目录（`Assets/TJGenerators/History/{sourceName}_sliced_{timestamp}/`） |
| `sliced_asset_paths` | 切割后的 Sprite 路径数组 |

### slice_image 参数调优

| 问题 | 调整 |
|---|---|
| 品红残留边缘（frontier） | 提高 `color_tolerance` 到 20-25 |
| 元素被误合并 | 提高 `min_region_pixels` 过滤噪声；或降低 `color_tolerance` |
| 元素被切断 | 降低 `color_tolerance`（前景像素被误判为背景） |
| 切割太少 | 降低 `min_region_pixels`；确认 `background_mode` 正确（图层用 `transparent`） |
| 边缘有白边 | `slice_image` 已内置颜色去背景，如仍有残留可手动后处理 |

### 放入场景

图层 PNG 与切割后的 Sprite 可作为 `Sprite` 类型放入场景（按 §4b Sprite 模板）。

> 独立 Sprite 可直接用于 UI Image 组件或 SpriteRenderer。如需打图集，可使用 `generate_sprite_atlas` 工具或 Unity Sprite Atlas。

## Prompt 写作指南

| 用途 | Prompt 示例 |
|---|---|
| RPG 背包 | `"fantasy RPG inventory screen with health bars, item slots, skill buttons"` |
| FPS HUD | `"first-person shooter HUD with ammo counter, minimap, crosshair, health bar"` |
| 主菜单 | `"medieval game main menu with ornate buttons, settings panel, character portrait"` |
| 对话框 | `"visual novel dialogue box with text area, character name plate, choice buttons"` |
| 技能树 | `"skill tree UI with branching nodes, connection lines, unlock buttons"` |

技巧：
- 描述 **UI 类型和包含的元素**（按钮、面板、血条、物品格）
- 提及 **游戏类型/风格**（fantasy RPG / sci-fi / medieval）
- 后端会自动增强 prompt，无需写 "HUD layout" 等关键词
- 英文 prompt 效果更佳

## 故障排查

### Skill 独有问题

> 通用故障（配置缺失 / 任务卡住 / 状态异常 / 未登录）见 [generator-async-pattern §10](../../experience/templates/generator-async-pattern.md#10-通用故障排查)。

| 问题 | 原因 | 解决 |
|---|---|---|
| Step 1 截图不像游戏 UI | prompt 太笼统 | 描述具体 UI 元素（血条、物品格、技能按钮）；后端会增强但基础描述仍重要 |
| Step 2 图层为空/只有底图 | `screenshot_path` 路径错误 | 确认使用 Step 1 `<bg_task_done>` 中的 `image_path` |
| Step 2 提交报错 | 缺少 `screenshot_path` | Step 2 必须传 `screenshot_path`，值为 Step 1 的 `image_path` |
| Step 1 和 Step 2 用了不同 prompt | Step 2 忽略 prompt | 这是正常的——Step 2 使用固定的图层拆分提示词 |
| 某个图层包含多个元素 | 16 层上限合并了小元素 | 对该层调 `slice_image`（`background_mode: "transparent"`），不要全量切割 |
| 把底图当成了元素 | `layer_paths[0]` 语义误解 | 第 0 张是底图（合成背景），元素在 `[1..N]` |
| 想用旧品红 cutout 流程 | provider 选择 | 传 `provider: "frontier"`（两步一致），Step 2 后必须 `slice_image`（`solid_color`） |

### Domain reload 后 task 丢失

通用恢复流程见 [generator-async-pattern §6](../../experience/templates/generator-async-pattern.md#6-domain-reload-recovery)。本 skill 完成态阈值：

- PNG < 5 KB → 仍是 placeholder 或任务丢失
- PNG ≥ 50 KB → 真实图片已就绪

可用 `glob("Assets/TJGenerators/History/*.png")` + 文件大小恢复。注意区分 Step 1 截图和 Step 2 图层产物（按时间和数量判断；图层产物是 `{basename}_1.png … _N.png` 兄弟文件序列）。

---

**Task ID Format**：`game_ui_kit_{counter}_{timestamp}`（Step 2 seedream 任务为 `image_layers_{counter}_{timestamp}`，同属本工具）

**Notes**：
- Step 1 使用 Seedream 5.0 Pro（`huoshan_seedream_pro_image`，2848×1600，不抠图）；Step 2 使用 Seedream 图层拆分（`seedream-image-layering`，默认 2K 档位，最多 1 底图 + 16 图层；元素美术层不带动态文字，动态文字独立成层）
- `provider: "frontier"` 时 Step 1/2 均使用 Frontier Game Design 模型（旧路径，Step 2 品红 cutout sheet）
- Step 2 的图层拆分提示词是后端固定的，用户 prompt 不影响
- Step 3 `slice_image` 使用 CV 连通域检测（8-connected BFS），同步返回
- 自动应用 `TuanjieAI` 标签
- **并发上限 5**
- 需 Unity Editor 在线运行；消耗 AI 服务额度
