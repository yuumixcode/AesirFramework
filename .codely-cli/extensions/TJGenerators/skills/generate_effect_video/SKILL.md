---
name: unity-effect-video-generation
description: Generate effect videos (image + green-screen VFX video) in Unity using AI from text descriptions. Use this skill whenever the user wants to create a VFX effect video — for example fire explosion effects, magic glow effects, smoke effects, particle effects, energy effects, lightning effects (特效视频, 绿幕特效, 火焰特效, 魔法特效, 烟雾特效, 粒子特效, 能量特效, 闪电特效) — even if they just say "帮我生成一个特效", "生成特效视频", "生成绿幕特效", "make an effect video", "generate a VFX", "create a fire effect". The backend automatically: 1) generates original art with green-screen background (user doesn't need to specify "green screen" in prompt), 2) generates a green-screen effect video. Unity automatically: 3) creates a ChromaKey material — use with VideoPlayer + RenderTexture for real-time transparent playback.
---

> ⚠️ **执行约束**
> - **主 agent**：无 `execute_custom_tool` 权限，必须 `task(subagent_name="effect-video-generator", ...)` 委托，不要 `activate_skill` 后自己调。
> - **子代理（本文档主要读者）**：有权限，按下方 `execute_custom_tool(...)` 示例执行。

> ⛔ **`place_assets_in_scene` 调用规则**
> - **调用方式**：`activate_skill("unity-place-assets-in-scene")` → 按本 skill「VideoPlayer 配置」节模板用 `execute_csharp_script` 配置 VideoPlayer + RenderTexture + ChromaKey Material（**不是** `execute_custom_tool`）。
> - **子代理**：提交后**立即调一次**把占位 VideoClip + ChromaKey 材质配到场景对象；收到 `<bg_task_done>` 后**不再调**（材质和视频自动指向真实内容）。
> - **主 agent**：报告里的 `material_path` 是"已放置"的证据，**不要再调**。

# Generate Effect Video in Unity 🎬✨

Generate **effect videos** in Unity using AI from text descriptions — **fully automated end-to-end**:

1. **Backend** auto-generates original art with **green-screen background** (user prompt does NOT need to include "green screen" — it's added automatically)
2. **Backend** uses that image as the first frame to generate a green-screen effect video
3. **Unity** auto-creates a **ChromaKey material** — use with VideoPlayer + RenderTexture for real-time transparent playback

Output: **ready-to-use** green-screen MP4 + ChromaKey Material, saved to `Assets/Video/`.

> 🎬 This is a **three-step automated pipeline** — image gen → video gen → ChromaKey material. Expect 90–180s.
> ✅ Green-screen prompt is **auto-appended** by the backend. User only needs to describe the VFX effect itself.
> ✅ ChromaKey material is **auto-created** by Unity after video download. Use VideoPlayer + RenderTexture for real-time green-screen keying — no frame extraction, no preprocessing, plays as transparent video at runtime.

## 🚦 执行四步（不要跳读外链）

1. 调 `generate_effect_video` → 拿 `task_id` + `placeholder_path`（占位 mp4）
2. 立即 `place_assets_in_scene`（资产类型 `VideoClip` + `Material`，路径用 `placeholder_path`）→ VideoPlayer + ChromaKey 材质配好
3. **END RESPONSE TURN** — 不要 poll、不要 `query_effect_video_status`、不要继续操作
4. 下一轮收到 `<bg_task_done>` → 读 `video_path`（已原地覆盖，VideoPlayer.clip 自动指向真实视频，**不要再 place**）

**档位**：长任务 90–180 秒（生图→生视频→创建 ChromaKey 材质）；300 秒内无通知才允许 `query_effect_video_status` 一次。完整 async 规则见 [generator-async-pattern](../../experience/templates/generator-async-pattern.md)。Domain reload 后按文件大小恢复（<1KB=placeholder，≥100KB=真实）。

## ⚠️ Prompt 必须使用英文

`prompt` 参数**必须**为英文。**不需要**手动写 "green screen background" — 后端会自动追加。

- ✅ `"fire explosion with sparks and smoke"`（后端自动追加 green screen background）
- ✅ `"magical glow effect with swirling energy particles, blue and purple light"`
- ❌ `"火焰爆炸特效"`（需翻译为英文）
- ❌ `"fire explosion, green screen background"`（不需要手动写 green screen）

- ✅ `"fire explosion with sparks and smoke"`（后端自动追加 green screen background）
- ✅ `"magical glow effect with swirling energy particles, blue and purple light"`
- ❌ `"火焰爆炸特效"`（需翻译为英文）
- ❌ `"fire explosion, green screen background"`（不需要手动写 green screen）

## When to Use / NOT to Use

适用：游戏特效（火焰/冰冻/闪电/魔法/烟雾/粒子/能量/爆炸）、UI 特效动画、技能特效预览、绿幕素材。

不适用：
- 普通视频（非特效）→ `generate_video`
- 3D 模型 → `generate_3d_model`
- 2D 精灵 → `generate_sprite`
- 音乐/音效 → `generate_audio_clip` / `generate_sound_effect`

---
## 工具

所有工具通过 `execute_custom_tool` 调用。

### `generate_effect_video`

```python
execute_custom_tool(
  tool_name="generate_effect_video",
  parameters={
    "prompt": "fire explosion with sparks and smoke, cinematic VFX",  # Required, English — green screen auto-added by backend
    "videoDuration": 5,           # 4–15 秒，默认 5
    "videoRatio": "16:9",        # "16:9" | "9:16" | "1:1" | "4:3"，默认 "16:9"
    "videoResolution": "720p",   # "720p" | "480p"，默认 "720p"
    # output_path: 不建议指定，默认 Assets/Video/
  }
)
```

返回字段：
- `task_id`、`placeholder_path`（**立即可用**，赋给 VideoPlayer）
- `estimated_wait_seconds` ≈ 180
- `notification_mode: "bg_task_done"`

提交失败时 `result["success"] == false`，读 `error_code` / `message`，**不要**poll。

### `<bg_task_done>` 独有字段

通用字段见模板。本 skill 额外字段：

| 字段 | 说明 |
|---|---|
| `video_path` | 绿幕视频路径（MP4，占位原地覆盖） |
| `material_path` | **ChromaKey 材质** — 配合 VideoPlayer + RenderTexture 实现实时透明播放 |
| `preview_url` | 远端预览 URL（生成的原画图片） |
| `generator_id` | 生成器标识（`effect_video_wf`） |
| `prompt` | 原始 prompt |

> ⚠️ 收到 `<bg_task_done>` 后，`video_path` + `material_path` **已经是最终产物**。用 VideoPlayer 播放视频，材质赋给 Quad，shader 实时抠除绿幕 → 透明特效视频播放。

### `query_effect_video_status` / `list_effect_video_tasks`

`query_effect_video_status` 仅作 fallback（300 秒后单次）。返回字段同 `<bg_task_done>` payload。

`list_effect_video_tasks` 返回当前 session 的所有特效视频任务。

## 参数详解

### prompt
特效描述（英文）。按"特效主体 + 动作 + 视觉风格 + 背景"组织：
- **主体**：fire / ice / lightning / magic / smoke / particles / energy
- **动作**：explosion / swirling / flowing / bursting / radiating / crackling
- **视觉风格**：cinematic / realistic / stylized / cartoon
- **背景**：不需要手动写 — 后端自动追加 green screen background

### videoDuration
- 4–5 秒：短特效（爆炸、闪光）
- 6–10 秒：中等特效（魔法施放、能量蓄积）
- 11–15 秒：长特效（持续烟雾、循环粒子）

### videoRatio
- `"16:9"` 横屏 / `"9:16"` 竖屏 / `"1:1"` 方形 / `"4:3"` 传统比例

### videoResolution
- `"480p"` (更快) / `"720p"` (更高质量)

## 使用示例

### 火焰爆炸特效

```python
result = execute_custom_tool(
    tool_name="generate_effect_video",
    parameters={
        "prompt": "fire explosion with sparks and smoke, cinematic VFX",
        "videoDuration": 5,
        "videoRatio": "16:9",
        "videoResolution": "720p"
    }
)
task_id = result["task_id"]
placeholder_path = result["placeholder_path"]

# ✅ END RESPONSE TURN — 后端自动追加 green screen prompt
# ✅ Unity 自动创建 ChromaKey 材质，bg_task_done 返回 video_path + material_path
```

### 魔法光效

```python
result = execute_custom_tool(
    tool_name="generate_effect_video",
    parameters={
        "prompt": "magical glow effect with swirling energy particles, blue and purple light, fantasy VFX",
        "videoDuration": 8,
        "videoRatio": "1:1"
    }
)
```

### 闪电特效

```python
result = execute_custom_tool(
    tool_name="generate_effect_video",
    parameters={
        "prompt": "lightning bolt striking from top to bottom, electric sparks, dramatic flash, dramatic VFX",
        "videoDuration": 4,
        "videoRatio": "9:16"
    }
)
```

## Prompt 写作指南

| 特效类型 | 示例 prompt |
|---|---|
| 火焰 | `"fire explosion with sparks and smoke, cinematic VFX"` |
| 冰冻 | `"ice crystallization effect, frost spreading outward, cold mist, magical VFX"` |
| 闪电 | `"lightning bolt strike with electric arcs and sparks, dramatic VFX"` |
| 魔法 | `"magical energy burst with swirling particles, purple and blue glow, fantasy VFX"` |
| 烟雾 | `"dense smoke flowing and dissipating, atmospheric, realistic VFX"` |
| 粒子 | `"golden particle explosion, sparkling dust dispersing outward, elegant VFX"` |
| 能量 | `"energy shield forming with crackling electricity, blue glow, sci-fi VFX"` |
| 爆炸 | `"massive explosion with debris and fire, shockwave expanding, cinematic VFX"` |
| 治愈 | `"healing light effect with green sparkles and soft glow, fantasy VFX"` |
| 毒雾 | `"toxic green gas cloud spreading, poisonous particles, dark fantasy VFX"` |

写 prompt 时按"特效主体 + 动作 + 视觉风格"组织（**不需要**写 green screen background）：

- **主体**：fire / ice / lightning / magic / smoke / particles / energy / poison
- **动作**：explosion / spreading / swirling / bursting / radiating / flowing / crackling
- **视觉风格**：cinematic / realistic / stylized / cartoon / fantasy / sci-fi

## VideoPlayer 配置（透明播放）

收到 `video_path` 和 `material_path` 后，在场景中配置实时绿幕抠像播放。

详见下方「自动后处理说明」节的代码示例和参数表。

## 自动后处理说明

本 skill 的绿幕抠像是**全自动**的——在视频下载完成后自动创建 ChromaKey 材质：

1. 视频下载完成
2. 自动创建 ChromaKey 材质（使用 `TJGenerators/ChromaKey` shader）
3. 材质参数已预设（tolerance=0.16, feather=0.04, spillRemoval=0.7）
4. 返回 `material_path`

**不抽帧、不转码**——保持原视频不变，播放时通过 shader 实时抠除绿幕背景，实现透明通道视频效果。

## VideoPlayer 配置（透明播放）

收到 `material_path` 后，在场景中配置：

```csharp
string videoPath = result["video_path"];
string materialPath = result["material_path"];

// 加载视频和材质
VideoClip clip = AssetDatabase.LoadAssetAtPath<VideoClip>(videoPath);
Material chromaKeyMat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

// 创建 VideoPlayer
var player = effectObject.GetComponent<VideoPlayer>();
if (player == null) player = effectObject.AddComponent<VideoPlayer>();
player.clip = clip;
player.renderMode = VideoRenderMode.RenderTexture;
player.isLooping = true;
player.playOnAwake = true;

// 创建 RenderTexture
var rt = new RenderTexture((int)clip.width, (int)clip.height, 0);
rt.Create();
player.targetTexture = rt;

// 将 ChromaKey 材质赋给 Renderer，RenderTexture 作为 mainTexture
chromaKeyMat.mainTexture = rt;
var renderer = effectObject.GetComponent<Renderer>();
renderer.sharedMaterial = chromaKeyMat;
```

推荐设置（特效视频）：

| 用途 | Loop | PlayOnAwake | RenderMode | Audio |
|---|---|---|---|---|
| 技能特效（触发播放） | false | false | RenderTexture | None |
| 背景特效（循环） | true | true | RenderTexture | None |
| UI 特效 | true/false | false | RenderTexture | None |

### ChromaKey 材质参数调整

| 参数 | 默认值 | 说明 |
|---|---|---|
| `_ChromaTolerance` | 0.16 | 绿色识别容差，值越大抠除范围越广 |
| `_ChromaFeather` | 0.04 | 边缘羽化，值越大边缘越柔和 |
| `_SpillRemoval` | 0.7 | 绿色溢出抑制，去除前景上的绿色反光 |

## 故障排查

### Skill 独有问题

> 通用故障（配置缺失 / 任务卡住 / 状态异常 / 未登录）见 [generator-async-pattern §10](../../experience/templates/generator-async-pattern.md#10-通用故障排查)。

| 问题 | 原因 | 解决 |
|---|---|---|
| 生成的视频与 prompt 不符 | prompt 太模糊 | 加入特效主体、动作、视觉风格；一次只表达一个核心特效 |
| 视频中没有特效 | prompt 未强调特效元素 | 确保 prompt 包含 VFX 关键词（explosion / glow / particles 等） |
| 绿幕背景不纯 | gpt-image-2 生成的图片背景不均匀 | 调整 ChromaKey 材质的 _ChromaTolerance 参数 |
| 透明效果边缘有绿边 | 抠像容差太小 | 调大 _ChromaFeather 或 _SpillRemoval |
| VideoClip 导入失败 | 资产未刷新或格式不支持 | 右键 Reimport，确认是 MP4 |
| 生成时间过长 | 两步串行（生图+生视频） | 正常现象，等待 90–180 秒 |
| ChromaKey 材质创建失败 | shader 未找到 | 确认 `TJGenerators/ChromaKey` shader 在项目中存在 |

### Domain reload 后 task 丢失

通用恢复流程见 [generator-async-pattern §6](../../experience/templates/generator-async-pattern.md#6-domain-reload-recovery)。本 skill 完成态阈值：

- MP4 ≥ 100 KB → 真实视频已生成完成

可用 `glob("Assets/Video/*.mp4")` + 文件大小检查恢复。

## 最佳实践

1. Prompt 不需要写 "green screen background" — 后端自动追加
2. Prompt 用英文，按"特效主体 + 动作 + 视觉风格"组织
3. 短特效（4-5秒）适合技能释放、打击反馈
4. 长特效（8-15秒）适合环境氛围、持续效果
5. ChromaKey 材质参数可根据实际效果微调（tolerance/feather/spillRemoval）
6. 用 VideoPlayer + RenderTexture + ChromaKey 材质播放，实时抠除绿幕
7. 存放 `Assets/Video/Effects/`

---

**Task Lifecycle / Status / Domain Reload / Polling 禁令**：详见 [generator-async-pattern](../../experience/templates/generator-async-pattern.md)。

**Notes**：
- 输出自动导入为 VideoClip，路径在 `Assets/Video/`
- 自动应用 `TuanjieAI` 标签
- 需 Unity Editor 在线运行
- 消耗 AI 服务额度（生图 + 生视频双重额度）
- 两步串行：先生图（~10-30s），再生视频（~60-120s）
