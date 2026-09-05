---
name: unity-video-generation
description: Generate video assets in Unity using AI from text prompts or reference images. Use this skill whenever the user wants to create a video asset — for example concept videos, motion graphics, animated content, visual effects, transition videos (转场视频), promotional videos (宣传视频), intro/outro sequences, cutscenes, or background videos — even if they just say "帮我生成一个视频", "生成一个动画视频", "生成转场视频", "生成宣传视频", "根据场景内容生成转场视频", "从当前场景生成宣传视频", "make me a video", "generate a video clip", "create a transition", "make a promotional video", "generate video from scene". Supports both text-to-video and image-to-video generation, including scene-to-video workflow for generating transition/promotional videos directly from Unity scenes via Huoshan SeeDream.
---

> ⚠️ **执行约束**
> - **主 agent**：无 `execute_custom_tool` 权限，必须 `task(subagent_name="video-generator", ...)` 委托，不要 `activate_skill` 后自己调。
> - **子代理（本文档主要读者）**：有权限，按下方 `execute_custom_tool(...)` 示例执行。

> ⛔ **`place_assets_in_scene` 调用规则**
> - **调用方式**：`activate_skill("unity-place-assets-in-scene")` → 按本 skill「放入场景」节模板用 `execute_csharp_script` 配置 `VideoPlayer.clip`（**不是** `execute_custom_tool`）。
> - **子代理**：提交后**立即调一次**把占位 VideoClip 配到 VideoPlayer；收到 `<bg_task_done>` 后**不再调**（VideoPlayer.clip 自动指向真实视频）。
> - **主 agent**：报告里的 `video_path` 是"已放置"的证据，不是"请你放置"的指示，**不要再调**。
> - **例外**：用户明确要"换 VideoPlayer / 再加一个实例"时才再次调用。详见 [async-pattern §5.1](../../experience/templates/generator-async-pattern.md#51-place_assets_in_scene-调用规则)。

# Generate Video in Unity 🎬

Generate **video assets** in Unity using AI, from text descriptions or reference images.
Output: video file auto-imported as **VideoClip**, saved to `Assets/Video/`.

> 🎬 Supports both **text-to-video** and **image-to-video**.

## 🚦 执行四步（不要跳读外链）

1. 调 `generate_video` → 拿 `task_id` + `placeholder_path`（占位 mp4）
2. 立即 `place_assets_in_scene`（资产类型 `VideoClip`，路径用 `placeholder_path`）→ VideoPlayer 配置好
3. **END RESPONSE TURN** — 不要 poll、不要 `query_video_status`、不要继续操作
4. 下一轮收到 `<bg_task_done>` → 读 `video_path`（已原地覆盖，VideoPlayer.clip 自动指向真实视频，**不要再 place**）

**档位**：长任务 30–120 秒；300 秒内无通知才允许 `query_video_status` 一次。完整 async 规则见 [generator-async-pattern](../../experience/templates/generator-async-pattern.md)。Domain reload 后按文件大小恢复（<1KB=placeholder，≥100KB=真实）。

## ⚠️ Prompt 必须使用英文

`prompt` 参数**必须**为英文。若用户给中文描述，先翻译再调用。

- ✅ `"a majestic dragon flying over a medieval castle, cinematic lighting, 4K quality"`
- ❌ `"一条巨龙飞越中世纪城堡"`

## When to Use / NOT to Use

适用：cinematic 序列、背景视频、转场效果、宣传素材、scene-to-video 工作流。

不适用：
- 3D 模型 → `generate_3d_model`
- 2D 精灵 → `generate_sprite`
- 精灵序列动画 → `generate_sprite_sequence`
- 音乐/音效 → `generate_audio_clip` / `generate_sound_effect`
- 编辑现有视频文件（本工具仅生成新视频）

---
## 工具

所有工具通过 `execute_custom_tool` 调用。

### `generate_video`

```python
execute_custom_tool(
  tool_name="generate_video",
  parameters={
    "prompt": "a majestic dragon flying over a medieval castle, cinematic lighting",  # Required, English
    "mode": "reference_image",   # "text_to_video" | "reference_image" | "first_frame" | "first_last_frame" | "multimodal"，默认 "reference_image"；提供 image_path 时自动切换，提供 video_path 时自动切换为 multimodal
    "resolution": "720p",        # "480p" | "720p"，默认 "720p"
    "ratio": "16:9",             # "16:9" | "9:16" | "1:1" | "4:3" | "3:4" | "21:9" | "adaptive"，默认 "16:9"
    "duration": 12,              # 4–15 秒，默认 12
    "return_last_frame": True,   # 是否返回最后一帧作预览
    "image_path": "",            # 参考图路径（image-to-video 模式）
    # output_path: 不建议指定，默认 Assets/Video/
  }
)
```

返回字段：
- `task_id`、`placeholder_path`（**立即可用**，赋给 VideoPlayer）
- `estimated_wait_seconds` ≈ 60
- `notification_mode: "bg_task_done"`

提交失败时 `result["success"] == false`，读 `error_code` / `message`，**不要**poll。

### `<bg_task_done>` 独有字段

通用字段见模板。本 skill 额外字段：

| 字段 | 说明 |
|---|---|
| `video_path` | 最终视频路径（== `placeholder_path`，原地覆盖） |
| `preview_url` | 远端预览 URL |
| `last_frame_url` | 最后一帧 URL（`return_last_frame=true` 时） |
| `generator_id` | 生成器标识 |
| `prompt` | 原始 prompt |

> ⚠️ `status == "completed"` 后**用 `video_path`，不用 `placeholder_path`**——后者作为标识符仍然指向同一文件，但 `video_path` 是语义正确的字段名。`AssetDatabase.Refresh()` 后再 `LoadAssetAtPath<VideoClip>` 即可加载。

### `query_video_status` / `list_video_tasks`

`query_video_status` 仅作 fallback（300 秒后单次）。返回字段同 `<bg_task_done>` payload。

`list_video_tasks` 返回当前 session 的所有 video 任务。

## 参数详解

### mode
- `"text_to_video"`：纯文本生成（无参考素材）
- `"reference_image"`：多参考图生视频（1-9 张图，提供 `image_path` 或 `reference_images` 时自动切换）
- `"first_frame"`：首帧生视频（`reference_images` 传 1 张图，自动检测）
- `"first_last_frame"`：首尾帧生视频（`reference_images` 传 2 张图：首帧+尾帧，自动检测）
- `"multimodal"`：多模态参考（提供 `video_path` 时自动切换；可同时传 `reference_images` 风格参考图和 `audio_paths` 音频参考）

### resolution
- `"480p"` (854×480，更快) / `"720p"` (1280×720，更高质量)

### model
- `"doubao-seedance-2-0-mini-260615"`（Mini，默认）
- `"doubao-seedance-2-0-260128"`（标准版）
- `"doubao-seedance-2-0-fast-260128"`（快速版）

### generate_audio
是否为视频生成音频（默认 `true`）。设为 `false` 可加快生成速度。

### ratio
- `"16:9"` 横屏 / `"9:16"` 竖屏 / `"1:1"` 方形 / `"4:3"` / `"3:4"` / `"21:9"` / `"adaptive"`

> ⚠️ scene-to-video 时，`ratio` 必须匹配截图的宽高比：
> - 1280×720 → `16:9`
> - 720×1280 → `9:16`
> - 1024×1024 → `1:1`

### duration
- 4–5 秒：logo 动画、转场
- 6–10 秒：motion graphics、循环
- 11–15 秒：cinematic 序列

### return_last_frame
是否返回最后一帧（用作缩略图或提前查看）。

### image_path
参考图路径（`reference_image` 模式使用单图，或 `first_frame` 模式使用首帧）。

### reference_images
多图路径数组：
- `first_frame` 模式：传 1 张图（首帧）
- `first_last_frame` 模式：传 2 张图（首帧 + 尾帧）
- `reference_image` 模式：传 1-9 张参考图
- `multimodal` 模式：传 0-9 张风格/角色参考图

### video_path
运镜/动作参考视频路径（`multimodal` 模式，自动上传到 TOS）。

### audio_paths
音频参考路径数组（`multimodal` 模式，最多 3 个，自动上传到 TOS）。用于提供节奏/氛围/配音参考。

## 使用示例

### Text-to-Video

```python
result = execute_custom_tool(
    tool_name="generate_video",
    parameters={
        "prompt": "a majestic dragon flying over a medieval castle, cinematic lighting, 4K quality",
        "mode": "text_to_video",
        "resolution": "720p",
        "duration": 12
    }
)
task_id = result["task_id"]
placeholder_path = result["placeholder_path"]

# ✅ 把 placeholder 赋给 VideoPlayer，然后 END RESPONSE TURN
# bg_task_done 通知会自动到达 — 不要 poll
```

### Image-to-Video

```python
result = execute_custom_tool(
    tool_name="generate_video",
    parameters={
        "prompt": "gentle wind blowing through the trees, peaceful atmosphere",
        "image_path": "Assets/Textures/forest_landscape.png",
        "mode": "reference_image",
        "duration": 12
    }
)
```

### 首帧生视频 (First Frame)

```python
result = execute_custom_tool(
    tool_name="generate_video",
    parameters={
        "prompt": "camera slowly zooms into the character's face, dramatic lighting",
        "reference_images": ["Assets/Textures/start_frame.png"],
        "mode": "first_frame",
        "duration": 8
    }
)
```

### 首尾帧生视频 (First + Last Frame)

```python
result = execute_custom_tool(
    tool_name="generate_video",
    parameters={
        "prompt": "smooth transition from day to night, city skyline timelapse",
        "reference_images": ["Assets/Textures/day_frame.png", "Assets/Textures/night_frame.png"],
        "mode": "first_last_frame",
        "duration": 10
    }
)
```

### 多模态参考（视频+图+音频）

```python
result = execute_custom_tool(
    tool_name="generate_video",
    parameters={
        "prompt": "Refer to @Video 1 for camera movement and pacing. Refer to @Image 1 for style. Epic fantasy scene.",
        "video_path": "Assets/Videos/reference_motion.mp4",
        "reference_images": ["Assets/Textures/style_ref.png"],
        "audio_paths": ["Assets/Audio/bgm_reference.mp3"],
        "mode": "multimodal",
        "duration": 8
    }
)
```

### 竖屏 / 方形

```python
# 9:16 移动端竖屏
parameters={"prompt": "...", "ratio": "9:16", "resolution": "720p", "duration": 12}

# 1:1 社交媒体方形
parameters={"prompt": "...", "ratio": "1:1", "duration": 12}
```

## Prompt 写作指南

| 视频类型 | 示例 prompt |
|---|---|
| Cinematic | `"epic dragon flying over medieval castle, dramatic clouds, slow motion, cinematic lighting"` |
| 自然 | `"gentle waves crashing on beach at sunset, peaceful atmosphere, golden hour light"` |
| 产品 | `"360-degree product rotation, clean white background, professional studio lighting"` |
| 角色 | `"character walking slowly towards camera through fog, mysterious atmosphere"` |
| 抽象 | `"colorful particles swirling and morphing, vibrant colors, smooth motion"` |
| Logo | `"logo appearing with particle explosion, smooth fade in, modern style"` |
| 转场 | `"smooth wipe transition from left to right, blur effect, professional style"` |
| 转场（粒子） | `"particle dissolve transition, glowing particles fading in and out, elegant"` |
| 宣传 | `"product showcase with dynamic camera movement, sleek modern style, commercial quality"` |
| 宣传（品牌） | `"brand reveal animation, bold typography, energetic motion graphics, promotional"` |

写 prompt 时按"主体 + 动作 + 视觉风格 + 镜头运动 + 氛围"组织：

- **主体**：dragon / character / product / waves / particles
- **动作**：flying / crashing / rotating / walking / swirling
- **视觉风格**：cinematic / peaceful / professional / mysterious / modern
- **镜头**：slow motion / 360-degree rotation / pan / zoom
- **光照氛围**：golden hour / dramatic / mysterious / vibrant
- **节奏感**：quick flash / smooth flow / slow reveal

## VideoPlayer 配置

赋 placeholder 后立即可用。生成完成会原地覆盖，无需重赋值。

```csharp
string placeholderPath = result["placeholder_path"];
VideoClip clip = AssetDatabase.LoadAssetAtPath<VideoClip>(placeholderPath);
VideoPlayer player = videoObject.GetComponent<VideoPlayer>();
player.clip = clip;
```

推荐设置：

| 用途 | Loop | PlayOnAwake | RenderMode | Audio |
|---|---|---|---|---|
| 背景循环（motion graphics） | true | true | CameraFarPlane | None |
| 一次性 cutscene | false | true | CameraFarPlane | AudioSource |
| 转场视频 | false | false | CameraFarPlane | None |
| 宣传循环 | true | false | CameraFarPlane | AudioSource |
| UI 视频（Canvas 上显示） | true/false | false | RenderTexture（搭配 RawImage） | — |

## Scene-to-Video 工作流 🎬

直接从 Unity 场景生成转场/宣传视频。流程：

```
Play → 截图 → Stop → generate_video(reference_image) → VideoPlayer → 保存场景
```

### 完整示例（转场视频）

```python
# Step 1: Play Mode 让场景渲染
unity_editor(action="play")

# Step 2: 截图
screenshot_result = unity_screenshot(
    action="capture_game_view",
    filename="scene_capture_for_video"
)
screenshot_path = screenshot_result["path"]

# Step 3: 退出 Play
unity_editor(action="stop")

# Step 4: 生成转场视频
video_result = execute_custom_tool(
    tool_name="generate_video",
    parameters={
        "prompt": "Cinematic scene transition with smooth camera push-in, dramatic lighting shifts, particle dissolves and lens flares, elegant motion blur",
        "mode": "reference_image",
        "image_path": screenshot_path,
        "duration": 12,
        "ratio": "16:9",
        "resolution": "720p"
    }
)
if not video_result.get("success", True):
    raise RuntimeError(f"Video generation failed: {video_result.get('message')}")

task_id = video_result["task_id"]
placeholder_path = video_result["placeholder_path"]

# Step 5: 创建 VideoPlayer GameObject
unity_gameobject(action="create", name="TransitionVideoPlayer", position=[0, 0, 0])

# Step 6: 配置 VideoPlayer（execute_csharp_script，不写 .cs 文件）
execute_csharp_script(f"""
var go = GameObject.Find("TransitionVideoPlayer");
var player = go.GetComponent<UnityEngine.Video.VideoPlayer>();
if (player == null) player = go.AddComponent<UnityEngine.Video.VideoPlayer>();

UnityEditor.AssetDatabase.Refresh();
var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Video.VideoClip>("{placeholder_path}");
player.clip = clip;
player.isLooping = false;
player.playOnAwake = true;
player.renderMode = UnityEngine.Video.VideoRenderMode.CameraFarPlane;
player.aspectRatio = UnityEngine.Video.VideoAspectRatio.FitVertically;
player.targetCamera = UnityEngine.Camera.main;
player.audioOutputMode = UnityEngine.Video.VideoAudioOutputMode.None;
UnityEditor.EditorUtility.SetDirty(go);
""")

# Step 7: 保存
unity_scene(action="save")

# END RESPONSE TURN — bg_task_done 自动到达
```

### 转场 vs 宣传：差异速查

| 维度 | 转场 | 宣传 |
|---|---|---|
| 用途 | 场景切换、加载效果 | 游戏宣传、营销 |
| Prompt 重点 | 转场效果（dissolve / wipe / blur） | showcase 动作（reveal / highlight） |
| Loop | false（单次） | true（背景循环） |
| Audio | 通常无 | 可加 BGM / 旁白 |
| 推荐目录 | `Assets/Video/Transitions/` | `Assets/Video/Promotional/` |

### 转场 prompt 速查

| 类型 | Prompt |
|---|---|
| Dissolve | `"elegant particle dissolve transition, glowing particles fading in and out, smooth atmospheric effect"` |
| Wipe | `"smooth diagonal wipe transition with blur effect, professional cinematic style"` |
| Zoom | `"dramatic camera push-in transition with focus blur, depth of field effect"` |
| Fade | `"cinematic crossfade transition with light leak effects, warm atmospheric glow"` |
| Glitch | `"digital glitch transition with chromatic aberration and pixel distortion effects"` |
| Particle | `"magical particle explosion transition, sparkling dust motes swirling and dispersing"` |
| Light | `"dramatic lighting shift transition with lens flare bloom, god rays emanating from center"` |
| Motion Blur | `"dynamic motion blur transition with speed lines effect, high-energy cinematic feel"` |

### 宣传 prompt 速查

| 类型 | Prompt |
|---|---|
| 游戏揭幕 | `"epic game reveal with dramatic camera flyover, showcasing game world with stunning visual effects, cinematic lighting"` |
| 特性高亮 | `"highlight key game features with dynamic camera movements, smooth transitions, energetic showcase style"` |
| Cinematic 预告 | `"cinematic game trailer with emotional storytelling, dramatic camera angles, slow-motion reveals, professional film quality"` |
| 动作蒙太奇 | `"fast-paced action montage with dynamic cuts, energetic camera movements, intense atmosphere"` |
| 氛围 | `"peaceful atmospheric showcase with slow camera movements, beautiful lighting, serene and inviting style"` |
| 角色揭幕 | `"dramatic character reveal with cinematic lighting, slow-motion introduction, heroic and epic feeling"` |

### 触发脚本示例

```csharp
// 场景切换前播放转场
public class SceneTransitionManager : MonoBehaviour
{
    public VideoPlayer transitionPlayer;

    public void PlayTransitionAndLoad(string nextSceneName)
    {
        transitionPlayer.Play();
        StartCoroutine(LoadSceneAfterTransition(nextSceneName));
    }

    IEnumerator LoadSceneAfterTransition(string sceneName)
    {
        yield return new WaitForSeconds((float)transitionPlayer.clip.length);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
```

## 故障排查

### Skill 独有问题

> 通用故障（配置缺失 / 任务卡住 / 状态异常 / 未登录）见 [generator-async-pattern §10](../../experience/templates/generator-async-pattern.md#10-通用故障排查)。

| 问题 | 原因 | 解决 |
|---|---|---|
| 视频与 prompt 不符 | prompt 太模糊 | 加入主体、动作、视觉风格、镜头运动、光照；一次只表达一个核心概念 |
| VideoClip 导入失败 | 资产未刷新或格式不支持 | 右键 Reimport，确认是 MP4/WebM |
| image-to-video 不生效 | 路径无效或 mode 错误 | 确认 `image_path` 指向有效 Unity 资产，`mode="reference_image"` |
| Scene 截图为黑屏 | 进入 Play Mode 太快 | 截图前等 1–2 秒，确认 Main Camera 渲染配置 |
| 宣传视频音频不播放 | AudioSource 未配置 | 设 `audioOutputMode = AudioSource`，添加并启用 AudioSource |
| 转场太短/太长 | duration 不当 | 调整 `duration`（快速转场 2–5 秒，多数 12 秒） |
| 内容策略拦截（`content_moderation`） | 版权角色名等敏感词触发平台审核，策略不可绕过 | 把拦截原因告知用户；如用户需要类似资产，先说明资产库不保证有合适内容，再询问是否改用 `search_assets` |

**内容策略拦截处理（不改写 prompt）：**

1. 直接告知用户："生成被内容策略拒绝，原因：{拦截原因}"（从错误消息里的「拦截原因」后取；若无则只说被拒绝）
2. 说明这是平台限制，无法通过修改 prompt 绕过
3. 提示："也可以尝试从资产库搜索，但不保证有合适的资产，是否搜索？"
4. **仅在用户确认后**再 `activate_skill("unity-asset-search")`；不要擅自改写 prompt 重试生成

### Domain reload 后 task 丢失

通用恢复流程见 [generator-async-pattern §6](../../experience/templates/generator-async-pattern.md#6-domain-reload-recovery)。本 skill 完成态阈值：

- MP4 ≥ 100 KB → 真实视频已生成完成

可用 `glob("Assets/Video/*.mp4")` + 文件大小检查恢复。

## 最佳实践

### 转场视频
1. 截图选择视觉冲击强的瞬间
2. Play Mode 保持短（1–2 秒），只为捕捉一帧
3. Prompt 聚焦转场效果，不重复描述截图内容
4. 通常无音频；用单独 SFX/BGM
5. 存放 `Assets/Video/Transitions/`

### 宣传视频
1. 选择最能代表游戏视觉风格的场景
2. 截图前调整 Main Camera 到最佳角度
3. Prompt 聚焦 showcase 动作（reveal / highlight）
4. 决定循环（背景）还是单次（trailer）
5. 存放 `Assets/Video/Promotional/`

---

**Task Lifecycle / Status / Domain Reload / Polling 禁令**：详见 [generator-async-pattern](../../experience/templates/generator-async-pattern.md)。

**Notes**：
- 输出自动导入为 VideoClip，路径在 `Assets/Video/`
- 自动应用 `TuanjieAI` 标签
- 需 Unity Editor 在线运行
- 消耗 AI 服务额度
- Scene-to-video 是本 skill 独有工作流；用户说"根据场景生成转场/宣传视频"或 "generate transition/promotional video from scene" 时自动启用
