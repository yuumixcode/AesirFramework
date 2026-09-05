---
name: unity-sprite-generation
description: Generate 2D sprite assets (game icons, item images, UI elements, character portraits, skill icons) in Unity using AI via text prompts or reference images. Use this skill whenever the user wants to create a 2D image asset for a Unity game — even if they just say "帮我生成一个道具图标", "给武器画个图", "make me a sprite for my RPG item", or "generate a game icon". Trigger proactively for any 2D art asset creation request in a Unity context, including icons, portraits, textures, or any visual content for games. Uses 火山 SeeDream (huoshan_seedream) as the default model.
---

> ⚠️ **执行约束**
> - **主 agent**：无 `execute_custom_tool` 权限，必须 `task(subagent_name="sprite-and-sprite-sequence-generator", ...)` 委托，不要 `activate_skill` 后自己调。
> - **子代理（本文档主要读者）**：有权限，按下方 `execute_custom_tool(...)` 示例执行。

> ⛔ **`place_assets_in_scene` 调用规则**
> - **调用方式**：`activate_skill("unity-place-assets-in-scene")` → 按 §4b Sprite 模板用 `execute_csharp_script` 建 `SpriteRenderer` 或 UI `Image`（**不是** `execute_custom_tool`）。
> - **子代理**：提交后**立即调一次**放占位 Sprite（SpriteRenderer / UI Image）；收到 `<bg_task_done>` 后**不再调**（PNG 原地覆盖，已实例化的 SpriteRenderer/Image 自动显示真实精灵）。
> - **主 agent**：报告里的 `sprite_path` 是"已放置"的证据，不是"请你放置"的指示，**不要再调**。
> - **例外**：用户明确要"再放一份 / 换位置 / 改 scale"时才再次调用。详见 [async-pattern §5.1](../../experience/templates/generator-async-pattern.md#51-place_assets_in_scene-调用规则)。

# Generate 2D Sprite in Unity 🎨

Generate 2D Sprite assets in Unity using AI, from text prompts or reference images.
Output: PNG 自动导入为 **Sprite (`TextureImporterType.Sprite`)**，保存到 `Assets/TJGenerators/History/`。

支持 30+ 内容类型预设（武器/护甲/道具/UI/角色等）和 30+ 艺术风格预设（像素/动漫/卡通/写实等）共同引导生成方向。

支持三个模型：
- **Frontier Game Design** (`frontier-game-design`) — 游戏向文/图生图，支持 `imageSize` 和 `outputFormat`
- **火山 SeeDream** (`huoshan_seedream`，默认) — 通用 sprite 生成，支持 `type_id` / `style_id` / `size` / `is_segmentation` 等
- **Frontier** (`frontier-effect`) — 风格化特效；支持 `resolution` / `aspect_ratio` / `output_format`

## 🚦 执行四步（不要跳读外链）

1. 调 `generate_sprite` → 拿 `task_id` + `placeholder_path`（1×1 灰 Sprite）
2. 立即 `place_assets_in_scene`（资产类型 `Sprite`，路径用 `placeholder_path`）→ 建立 `SpriteRenderer` 或 UI `Image`
3. **END RESPONSE TURN** — 不要 poll、不要 `query_sprite_status`、不要继续操作
4. 下一轮收到 `<bg_task_done>` → 读 `sprite_path` / `image_path`（PNG 已原地覆盖，已实例化的 SpriteRenderer/Image 自动显示真实精灵，**不要再 place**）

**档位**：短任务 60–180 秒；120 秒内无通知才允许 `query_sprite_status` 一次。最多 **5 个**并发。完整 async 规则见 [generator-async-pattern](../../experience/templates/generator-async-pattern.md)。

## ⚠️ Skill 独有约束

1. **并发上限 5**——同时运行的 sprite 任务最多 5 个。
2. **至少要给一个输入**——`prompt` 或 `image_path` 至少一个。
3. **`size` 最小 ~1920×1920**——小于约 3,686,400 像素会 400 报错（与 generate_image 共享此限制）。
4. **`is_segmentation` 默认 `true`**——sprite 默认抠背景做透明（适合道具/角色/技能图标）；UI 背景/场景类设 `false`。
5. **资产类型必须是 `Sprite` 而非 `Texture2D`**——`place_assets_in_scene` 会按 Sprite 类型把资产挂到 `SpriteRenderer` 或 UI `Image`；用 `Texture2D` 类型会得到 `RawImage`，不能直接做游戏内精灵。

## 与 generate_image 的差异速查

| 维度 | `generate_sprite` | `generate_image` |
|---|---|---|
| Generator | `frontier-game-design` / `huoshan_seedream` / `frontier-effect` | `frontier-game-design` / `huoshan_seedream_image` / `frontier-effect` |
| TextureImporterType | **Sprite**（带 alpha） | **Default**（普通 Texture2D） |
| 默认用途 | 游戏精灵：道具/UI/角色立绘 | 通用图：背景/纹理/概念图 |
| `is_segmentation` 默认 | **true**（抠背景） | 文档建议 false（要完整图） |
| `size` 选项 | 8 项（仅 2K） | 15 项（含 4K） |
| 独有参数 | `type_id` / `style_id`（30+30 预设） | Frontier 专属：`resolution` / `aspect_ratio` / `output_format` |
| Unity 中显示组件 | `SpriteRenderer` / `Image` | `Renderer.material` / `RawImage` |

> 选哪个？**透明背景 + 小道具/图标/角色立绘** → `generate_sprite`；**背景/海报/通用纹理** → `generate_image`。

## When to Use / NOT to Use

适用：游戏图标、道具图、UI 元素、角色头像/立绘、技能图标、卡牌、像素艺术、Q 版头像、武器装备美术。

不适用：
- 通用背景图 / 概念图 / 纹理 → `generate_image`
- 3D 模型 → `generate_3d_model`
- 天空盒 → `generate_skybox`
- 表面材质（PBR） → `generate_material`
- 帧动画序列 → `generate_sprite_sequence`

## 工具

所有工具通过 `execute_custom_tool` 调用。

### `generate_sprite`

```python
execute_custom_tool(
  tool_name="generate_sprite",
  parameters={
    "prompt": "golden sword with glowing runes, fantasy RPG weapon",  # 至少给一个
    "generator_id": "huoshan_seedream",   # 默认；也可 "frontier-game-design" / "frontier-effect"
    "image_path": "Assets/Reference/ref.png",  # 可选：参考图
    # SeeDream 专属：
    "type_id": "weapon_melee",            # 内容类型（30+，见表）
    "style_id": "fantasy",                # 艺术风格（30+，见表）
    "size": "2048x2048",                  # 默认 2K 1:1，最小 ~1920×1920
    # SeeDream / Game Design 共享（后端后处理，不发往 AI API）：
    "is_segmentation": True,              # 抠背景，默认 true（sprite 一般要透明）
    "q_value": 75,                        # 抠图后压缩质量 1–100
    "resize_width": 0,                    # 最大输出宽度，0 = 不缩放
    # Frontier Effect 专属：
    "resolution": "1K",                   # "0.5K"/"1K"/"2K"/"4K"，默认 "1K"
    "aspect_ratio": "auto",               # "auto"/"16:9"/"9:16"/"1:1" 等，默认 "auto"
    "output_format": "png",               # "png"/"jpeg"，默认 "png"
    # Frontier Game Design 专属：
    "imageSize": "square_hd",             # "square_hd"/"square"/"portrait_4_3"/"portrait_16_9"/"landscape_4_3"/"landscape_16_9"，默认 "square_hd"
    "outputFormat": "png",                # "png"/"jpeg"，默认 "png"
    # output_path: 不建议指定，默认 Assets/TJGenerators/History/
  }
)
```

### 返回字段

- `task_id`
- `placeholder_path`：1×1 灰色 Sprite，**立即可用**——交给 `place_assets_in_scene`
- `type_id` / `style_id`：回传（如有）
- `estimated_wait_seconds` ≈ 90
- `notification_mode: "bg_task_done"`

提交失败时 `result["success"] == false`，读 `error_code` / `message`，**不要**poll。

### `<bg_task_done>` 独有字段

通用字段见模板。本 skill 额外字段：

| 字段 | 说明 |
|---|---|
| `image_path` | 最终 Sprite 资产路径（== `placeholder_path`，原地覆盖） |
| `preview_url` | 预览 URL 或本地路径 |
| `generator_id` | 使用的生成器 |
| `prompt` | 原始 prompt |

> 注：`query_sprite_status` 返回里也有 `sprite_path` 字段，是 `image_path` 的别名（仅 `completed` 时存在）。

### `query_sprite_status` / `list_sprite_tasks`

`query_sprite_status` 仅作 fallback（120 秒后单次）。返回字段同 `<bg_task_done>` payload，外加 `placeholder_path`（仅 `generating` 时）和 `sprite_path`（仅 `completed` 时，等同 `image_path`）。

`list_sprite_tasks` 返回当前 session 的所有 sprite 任务。

## 内容类型预设 (`type_id`)

> 设了 `type_id` 后，AI 会自动应用对应类目的合理构图与主题。

| ID | 名称 | 类别 |
|---|---|---|
| `weapon_melee` | 近战武器（剑、斧、锤） | 武器 |
| `weapon_ranged` | 远程武器（弓、枪） | 武器 |
| `weapon_magic` | 魔法武器（法杖、魔导书） | 武器 |
| `armor_head` | 头部装备（头盔、帽子） | 护甲 |
| `armor_body` | 身体装备（胸甲、长袍） | 护甲 |
| `armor_shield` | 盾牌 | 护甲 |
| `armor_accessory` | 饰品（戒指、项链） | 饰品 |
| `consumable_hp` | 生命恢复（药水、食物） | 消耗品 |
| `consumable_mp` | 魔法恢复（法力药水） | 消耗品 |
| `consumable_buff` | 增益道具（药剂、卷轴） | 消耗品 |
| `material_common` | 普通材料（矿石、木材） | 材料 |
| `material_rare` | 稀有材料（精金、龙鳞） | 材料 |
| `material_epic` | 史诗材料（远古遗物） | 材料 |
| `key_item` | 关键物品（任务道具） | 特殊 |
| `quest_item` | 任务物品 | 特殊 |
| `tool_gathering` | 采集工具（镐、钓竿） | 工具 |
| `tool_crafting` | 制造工具（锤子、熔炉） | 工具 |
| `mount` | 坐骑（马、龙） | 角色 |
| `pet` | 宠物（狼、精灵） | 角色 |
| `furniture` | 家具 | 场景 |
| `decoration` | 装饰物（雕像、旗帜） | 场景 |
| `ui_button` | 按钮图标 | UI |
| `ui_icon` | 功能图标 | UI |
| `ui_frame` | 边框装饰 | UI |
| `skill_active` | 主动技能图标 | 技能 |
| `skill_passive` | 被动技能图标 | 技能 |
| `effect_buff` | 增益特效图标 | 效果 |
| `effect_debuff` | 减益特效图标 | 效果 |
| `character_hero` | 英雄头像 | 角色 |
| `character_npc` | NPC 头像 | 角色 |
| `monster` | 怪物图标 | 角色 |

## 艺术风格预设 (`style_id`)

> 与 `type_id` 组合使用效果最佳。

| ID | 名称 | 适用场景 |
|---|---|---|
| `pixel` | 像素艺术 | 复古、经典游戏 |
| `pixel_8bit` | 8 位像素 | 8-bit 复刻 |
| `pixel_16bit` | 16 位像素 | 16-bit 复刻 |
| `cartoon` | 美式卡通 | 休闲、彩色游戏 |
| `anime` | 日式动漫 | JRPG、视觉小说 |
| `chibi` | Q 版可爱 | 移动、可爱风游戏 |
| `realistic` | 写实 | AAA、沉浸式游戏 |
| `semi_realistic` | 半写实 | 动作 RPG |
| `flat` | 扁平设计 | 移动 UI、休闲 |
| `vector` | 矢量艺术 | 干净、可缩放图标 |
| `watercolor` | 水彩画 | 独立、艺术 |
| `oil_painting` | 油画 | 历史、艺术 |
| `sketch` | 素描草图 | 概念图、草稿 |
| `lineart` | 线稿 | 极简、干净 |
| `cell_shading` | 赛璐珞 | 风格化动画 |
| `low_poly` | 低多边形 | 几何、极简 |
| `isometric` | 等轴测 | 策略、城建 |
| `top_down` | 俯视视角 | 俯视游戏 |
| `side_scroller` | 横版卷轴 | 平台、动作 |
| `cyberpunk` | 赛博朋克 | 科幻、霓虹 |
| `steampunk` | 蒸汽朋克 | 维多利亚、工业 |
| `fantasy` | 奇幻 | 奇幻 RPG |
| `sci_fi` | 科幻 | 太空、未来 |
| `horror` | 恐怖 | 黑暗、恐怖 |
| `cute` | 可爱 | 卡哇伊、休闲 |
| `gothic` | 哥特 | 黑暗奇幻 |
| `medieval` | 中世纪 | 历史 RPG |
| `minimalist` | 极简主义 | 干净、现代 |
| `hand_drawn` | 手绘 | 独立、个人 |

## `imageSize` 选项 — 仅 Frontier Game Design

| 值 | 标签 | 适用 |
|---|---|---|
| `"square_hd"` | 1:1 HD | 游戏图标、精细 sprite **(默认)** |
| `"square"` | 1:1 | 标准方形 sprite |
| `"portrait_4_3"` | 3:4 | 角色立绘、卡牌 |
| `"portrait_16_9"` | 9:16 | 移动端 UI、竖图 |
| `"landscape_4_3"` | 4:3 | 横向 sprite、概念图 |
| `"landscape_16_9"` | 16:9 | 宽屏 sprite、splash |

> **注意：**`frontier-game-design` 不支持 `type_id` / `style_id`，请用描述性 prompt 代替。

## 输出尺寸 (`size`) — 仅 SeeDream

> **最小 ~1920×1920，否则 400 报错。** 默认 `"2048x2048"`。Sprite 没有 4K 选项（要 4K 用 `generate_image`）。

| 值 | 说明 |
|---|---|
| `"2048x2048"` | 2K 1:1 — 图标、多数物品 **(默认)** |
| `"2304x1728"` | 2K 4:3 — 横向卡牌 |
| `"1728x2304"` | 2K 3:4 — 竖向卡牌 |
| `"2560x1440"` | 2K 16:9 — 宽屏横向 |
| `"1440x2560"` | 2K 9:16 — 竖向（角色立绘） |
| `"2496x1664"` | 2K 3:2 — 标准照片 |
| `"1664x2496"` | 2K 2:3 — 标准照片竖向 |
| `"3024x1296"` | 2K 21:9 — 超宽 banner |

## `is_segmentation` 决策

| 用途 | 值 | 原因 |
|---|---|---|
| 道具、图标、技能、角色立绘 | `true`（默认） | 需透明背景才能叠加到 UI/场景 |
| UI 背景、面板、纹理 | `false` | 需要完整矩形图 |
| 卡牌、海报、banner | `false` | 需要完整图带背景 |
| 独立道具、装饰物 | `true` | 透明背景便于布置 |
| 平铺图 / 地砖 | `false` | 无缝平铺，不抠图 |

## 使用示例

### RPG 道具图标

```python
result = execute_custom_tool(
    tool_name="generate_sprite",
    parameters={
        "prompt": "ancient magic staff with glowing blue crystal",
        "type_id": "weapon_magic",
        "style_id": "fantasy"
    }
)
if not result.get("success", True):
    raise RuntimeError(f"[{result['error_code']}] {result['message']}")

task_id = result["task_id"]
placeholder_path = result["placeholder_path"]

# ✅ 立即用 place_assets_in_scene 把 placeholder_path 应用为 Sprite
# 然后 END RESPONSE TURN，等 bg_task_done 通知
```

### 像素风武器

```python
parameters={
    "prompt": "iron sword, simple shape, bright colors",
    "type_id": "weapon_melee",
    "style_id": "pixel_16bit"
}
```

### 动漫风角色立绘

```python
parameters={
    "prompt": "young female mage with silver hair and blue eyes",
    "type_id": "character_hero",
    "style_id": "anime",
    "size": "1728x2304",      # 竖向卡牌比例
    "is_segmentation": True   # 透明背景
}
```

### 从参考图生成

```python
parameters={
    "image_path": "Assets/ConceptArt/potion_sketch.png",
    "type_id": "consumable_hp",
    "style_id": "cartoon"
}
```

### 文生图：Frontier Game Design

```python
parameters={
    "generator_id": "frontier-game-design",
    "prompt": "a fantasy game icon of a magical sword with glowing runes",
    "imageSize": "square_hd",
    "outputFormat": "png"
}
```

### 图生图：Frontier Game Design

```python
parameters={
    "generator_id": "frontier-game-design",
    "prompt": "a detailed game icon of a magical staff",
    "image_path": "Assets/ConceptArt/staff_sketch.png",
    "imageSize": "square_hd"
}
```

### 并发批量（最多 5 个）

```python
task_ids = []
items = [
    ("fantasy sword with glowing runes", "weapon_melee"),
    ("golden shield with dragon emblem", "armor_shield"),
    ("red health potion in glass bottle", "consumable_hp"),
]
for prompt, type_id in items:
    result = execute_custom_tool(
        tool_name="generate_sprite",
        parameters={"prompt": prompt, "type_id": type_id, "style_id": "fantasy"}
    )
    task_ids.append(result["task_id"])
    # ✅ 直接继续，不 poll

# END RESPONSE TURN — 每个 task 都会单独发 bg_task_done
```

## 放入场景

资产类型 **`Sprite`**（不是 `Texture2D`），路径用 `placeholder_path` 或最终的 `image_path` / `sprite_path`。`placement_instruction` 决定具体形态：

- **3D 场景** → 空 GameObject + `SpriteRenderer`，需要设 `localScale`（2048px 默认 PPU=100，宽度 ≈ 20.48 单位，太大！）
- **UI Canvas** → Canvas 子节点 + `Image` 组件

缩放参考（SpriteRenderer 模式）：~1 单位用 `0.05f`，~2 单位用 `0.10f`，~4 单位（角色）用 `0.20f`。

规则见 [async-pattern §5 / §5.1](../../experience/templates/generator-async-pattern.md#5-placeholder-工作流适用于会返回-placeholder_path--prefab_output_path-的工具)。

## 参数速查

| 参数 | 类型 | 默认 | 说明 |
|---|---|---|---|
| `generator_id` | string | `"huoshan_seedream"` | 也可 `"frontier-game-design"`、`"frontier-effect"` |
| `prompt` | string | — | 文本描述（与 `image_path` 至少给一个） |
| `image_path` | string | — | 参考图（与 `prompt` 至少给一个） |
| `type_id` | string | — | **SeeDream only** — 30+ 内容类型预设，见表 |
| `style_id` | string | — | **SeeDream only** — 30+ 艺术风格预设，见表 |
| `is_segmentation` | bool | Game Design: `true`; SeeDream: `true` | **Game Design / SeeDream**；后端后处理抠图；背景/平铺图设 `false` |
| `size` | string | `"2048x2048"` | **SeeDream only** — 8 项，**最小 ~1920×1920** |
| `q_value` | int | `75` | **Game Design / SeeDream** — 抠图后压缩质量 1–100 |
| `resize_width` | int | `0` | **Game Design / SeeDream** — 最大输出宽度，0 = 不缩放 |
| `resolution` | string | `"1K"` | **Frontier Effect only** — `"0.5K"`/`"1K"`/`"2K"`/`"4K"` |
| `aspect_ratio` | string | `"auto"` | **Frontier Effect only** — `"auto"`/`"16:9"`/`"9:16"`/`"1:1"` 等 |
| `output_format` | string | `"png"` | **Frontier Effect only** — `"png"`/`"jpeg"` |
| `imageSize` | string | `"square_hd"` | **Game Design only** — `"square_hd"`/`"square"`/`"portrait_4_3"`/`"portrait_16_9"`/`"landscape_4_3"`/`"landscape_16_9"` |
| `outputFormat` | string | `"png"` | **Game Design only** — `"png"`/`"jpeg"` |
| `output_path` | string | — | 自定义路径（不建议指定） |

## Prompt 写作指南

`type_id` + `style_id` 已覆盖类目和风格。`prompt` 字段**只补充具体视觉细节**。

| 目标 | Prompt |
|---|---|
| 道具图标 | `"red health potion in a glass bottle with cork stopper, glowing liquid"` |
| 武器 | `"ornate golden sword with gem-encrusted crossguard, magical runes"` |
| 护甲 | `"dark iron full plate armor, battle-worn, gothic engravings"` |
| 角色 | `"young male warrior, short brown hair, determined expression, half-body"` |
| UI 图标 | `"shield icon representing defense stat, clean and bold"` |
| 技能图标 | `"fire tornado spell effect, swirling flames, circular composition"` |

技巧：
- **颜色**：`golden` / `deep blue` / `blood red`
- **材质**：`leather` / `enchanted crystal` / `rusted iron`
- **构图**：`centered` / `full body` / `half-body portrait`
- 保持简短——过长 prompt 会让模型困惑

## 故障排查

### Skill 独有问题

> 通用故障（配置缺失 / 任务卡住 / 状态异常 / 未登录）见 [generator-async-pattern §10](../../experience/templates/generator-async-pattern.md#10-通用故障排查)。
> 合法 `generator_id`：`"frontier-game-design"` / `"huoshan_seedream"` / `"frontier-effect"`。

| 问题 | 原因 | 解决 |
|---|---|---|
| `Either 'prompt' or 'image_path' must be provided` | 两个都没给 | 至少给一个 |
| 400 size 错误 | size < 1920×1920 | 用上表预设值 |
| `is_segmentation=true` 但背景没被去掉 | 主体不清晰 / 背景太复杂 | 提供更干净的参考图；或先用简单背景 prompt 重生成 |
| 出来的图风格/主体不对 | type_id 与 style_id 没配 | 同时设 `type_id` + `style_id`；prompt 补颜色/材质/姿态 |
| Sprite 在 3D 场景里巨大 | 默认 PPU=100，2048px ≈ 20.48 单位 | `place_assets_in_scene` 时设合适 `localScale`（见上文缩放参考） |

### Domain reload 后 task 丢失

通用恢复流程见 [generator-async-pattern §6](../../experience/templates/generator-async-pattern.md#6-domain-reload-recovery)。本 skill 完成态阈值：

- PNG ≥ 200 KB → 真实 Sprite 已就绪（2K 透明 PNG 通常几 MB，但小图标可能 200 KB）

可用 `glob("Assets/TJGenerators/History/*.png")` + 文件大小恢复。

---

**Task ID Format**：`sprite_{counter}_{timestamp}`

**Notes**：
- 输出 PNG 自动导入为 `TextureImporterType.Sprite`（带 alpha 通道）
- 自动应用 `TuanjieAI` 标签
- `type_id` / `style_id` 不直接发给 API，AI 内部用它们增强 prompt
- **并发上限 5**——批量生成超过 5 个时分批做
- 需 Unity Editor 在线运行；消耗 AI 服务额度
