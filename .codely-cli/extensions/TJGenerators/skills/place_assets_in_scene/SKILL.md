---
name: unity-place-assets-in-scene
description: 将 Unity 资产放入当前场景，支持自然语言描述位置/旋转/缩放。触发条件：用户要求放置或给出位置意图（"放到场景"、"放在桌子旁边"、"scale 0.5"等）；用户询问资产为何未出现在场景；3D 模型任务提交后默认立即放置占位 Prefab（除非用户明确不需要）；search_assets / generate_sprite 完成后主动放置。支持 Prefab / Sprite / AudioClip / Material / Cubemap / AnimationClip / TerrainData。不负责生成资产本身。
---

# Place Assets in Scene 📌

> ⚠️ **这是 skill `unity-place-assets-in-scene`，不是 custom tool。**
> 标准调用链：`activate_skill("unity-place-assets-in-scene")` → 读本文 → 按资产类型小节用 `execute_csharp_script` 跑放置脚本（Prefab → §4a、Sprite → §4b、AudioClip → §4c/§4d、Material → §4e、Skybox → §4f、AnimationClip → §4g、Terrain → §4h、Texture2D → §4i）。
> ⛔ 没有 `execute_custom_tool("place_assets_in_scene", ...)`；调了会报 *tool not found*。
> ⛔ Prefab 不能用 `unity_gameobject` 放（只建空对象，接不上生成完成后的更新链路），必须走 §4a。

将已经存在的 Unity 资产放入当前打开的场景。输入核心是 `asset_path`（来自各生成 skill 返回的 `xxx_path` 字段）、资产类型，以及可选的 `placement_instruction`。本 skill 只负责放置，不负责生成。

## Overview

- 输入：`asset_path` + `asset_type` + 可选 `placement_instruction`
- 输出：按用户意图把资产放入当前场景，必要时自动创建 GameObject / Canvas / AudioSource / Terrain / AnimatorController
- 放置逻辑统一通过 `execute_csharp_script` 完成；除非用户明确要求，否则不要把 `.cs` 文件写到磁盘，避免不必要的 Domain Reload

## 资产类型速查表

| 类型 | 典型来源 | 放置方式 |
|------|---------|---------|
| Prefab（`.prefab`） | `generate_3d_model_by_rodin`、`generate_3d_model_by_tripo_p1`、`generate_animated_character`、`generate_rigged_model`、`search_assets` | `PrefabUtility.InstantiatePrefab` |
| Sprite（`.png`，`TextureImporterType.Sprite`） | `generate_sprite` | 新建空 GameObject + `SpriteRenderer`，或 Canvas 子节点 + `UnityEngine.UI.Image` |
| AudioClip BGM（`.wav`） | `generate_audio_clip` | `AudioSource`，`loop=true`，`spatialBlend=0` |
| AudioClip SFX（`.wav` / `.mp3`） | `generate_sound_effect` | `AudioSource`，`loop=false`，`spatialBlend=1` |
| Material（`.mat`） | `generate_material` | `Renderer.sharedMaterial = material` |
| Cubemap Skybox（`.mat`） | `generate_skybox` | `RenderSettings.skybox = material` |
| AnimationClip（`.anim`） | `generate_sprite_sequence` | `AnimatorController + Animator` |
| Texture2D / Image（`.png` / `.jpg`） | `generate_image` | 赋给 `Material.mainTexture`，或创建 `RawImage` |
| Heightmap（`.png`）→ Terrain | `generate_terrain` | 调用 `apply_terrain_heightmap` |
| TerrainData（`.asset`） | 已有地形资源 | `Terrain.CreateTerrainGameObject` |

## 放置前场景感知

放置资产之前，根据 `placement_instruction` 的具体情况决定是否需要先调查场景。

### 何时需要调查

| 情形 | 是否调查 | 原因 |
|------|---------|------|
| 用户给出明确坐标，如 `"position (2,0,3)"` | 否 | 直接使用坐标 |
| 用户说"放在摄像机前方 N 米" | 否 | C# 内 `Camera.main` 即可 |
| 用户说"放在 xxx 旁边/上面/里面"，且 xxx 是确定的对象名 | 否 | `GameObject.Find("xxx")` 即可 |
| 用户说"放到场景里"，无具体位置，场景可能已有物体 | **是** | 需了解已有物体的位置/密度，避免重叠或堆到原点 |
| 用户说"放在合适的地方"、"摆到空旷处"、"居中对齐" | **是** | 需要场景包围盒信息才能计算合理落点 |
| 批量放置，需根据已有物体排布间距 | **是** | 需知道现有物体的分布范围 |

### 调查方法（按成本由低到高选择）

**方法 1：`unity_scene { action: "get_hierarchy" }`** — 快速看场景有哪些根对象

适合：只需确认场景里有没有某类对象（如地面、摄像机、玩家）时。

**方法 2：`unity_gameobject { action: "list_children", target: "...", depth: 3, includeInactive: true }`** — 获取层级 + Transform

适合：需要知道具体位置/缩放时。`target` 填根对象名；如需全场景概览，对每个根节点逐一调用。  
场景很大时加 `resultMode: "file"` 避免截断。

每个节点返回：`name, position, rotation, scale, active, children`

**方法 3：`unity_gameobject { action: "find", searchTerm: "...", searchMethod: "by_name" }`** — 定位特定对象

适合：用户提到的对象名不确定是否存在，先 find 确认再用其 position。

**方法 4：`execute_csharp_script`** — 自定义查询（最灵活）

适合：需要计算所有物体包围盒、统计对象密度、找最近空位等复杂逻辑。

```csharp
// 获取场景中所有根对象的名字和位置
var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
foreach (var go in roots) {
    Debug.Log($"{go.name}: pos={go.transform.position}, scale={go.transform.localScale}");
}
```

### 调查结果如何影响落点决策

- **场景为空或只有摄像机/灯光** → 放在原点或摄像机前方 3~5 米
- **场景已有物体，用户未指定位置** → 沿 X 轴在已有物体右侧偏移 `spacing` 距离放置；或计算所有物体包围盒，在边缘空白处落点
- **场景有地面（Plane/Terrain）** → `position.y` 设为地面高度，不要放到 y=0 导致穿模
- **用户说"居中对齐场景"** → 计算所有 Renderer 包围盒中心，将新物体置于该中心

---

## 放置描述（placement_instruction）

`placement_instruction` 是可选输入。用户可以自然语言描述位置、旋转、缩放；未描述的维度保持默认值。

| 用户描述 | AI 解析方式 |
|---------|-----------|
| `"放在原点"` | `position = (0, 0, 0)` |
| `"放在地面上"` | `position.y = 0` |
| `"放在摄像机前方 5 米"` | 查询 `Camera.main.transform`，沿 `forward` 偏移 5 |
| `"缩小到 0.5"` / `"scale 0.5"` | `localScale = Vector3.one * 0.5f` |
| `"放在 xxx 物体左边 2 米"` | `GameObject.Find("xxx").transform.position + Vector3.left * 2` |
| `"放在 xxx 物体上面"` | `GameObject.Find("xxx").transform.position + Vector3.up * 高度估算` |
| `"旋转 45 度朝右"` / `"面朝右"` | `rotation = Quaternion.Euler(...)` |
| `"平铺到整个地面"` | Material 应用到所有 Terrain / Plane |
| `"贴到墙上"` | Sprite 常见为竖直放置，示例：`Quaternion.Euler(0, 0, 90)` |
| `"居中对齐场景"` | 计算场景已有物体包围盒中心 |

解析优先级：

1. 明确数值：`position (1,2,3)`、`scale 2`、`rotation (0,90,0)`
2. 相对于已有物体：`xxx 旁边`、`xxx 上面`
3. 相对于摄像机：`摄像机前方`、`面向玩家`
4. 语义方向：`左边`、`右边`、`贴墙`
5. 无描述或无法解析：回退到各类型默认放置逻辑

相对位置代码模式：

```csharp
var target = GameObject.Find("{targetName}");
if (target == null) { Debug.LogWarning("未找到参考物体 {targetName}，使用原点"); }
var basePos = target != null ? target.transform.position : Vector3.zero;
instance.transform.position = basePos + Vector3.left * 2f;
```

```csharp
var cam = Camera.main;
if (cam != null) {
    instance.transform.position = cam.transform.position + cam.transform.forward * 5f;
    instance.transform.rotation = Quaternion.LookRotation(-cam.transform.forward);
}
```

## 标准 C# 样板代码

所有放置代码都通过 `execute_csharp_script` 执行。禁止把临时 `.cs` 文件写入磁盘。

```csharp
AssetDatabase.Refresh();
var asset = AssetDatabase.LoadAssetAtPath<T>(path);
if (asset == null) {
    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    asset = AssetDatabase.LoadAssetAtPath<T>(path);
}
if (asset == null) { Debug.LogError("资产未找到: " + path); return; }
```

场景修改后必须标脏：

```csharp
EditorUtility.SetDirty(go);
UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
```

## 各类型放置模板

> `placement_instruction` 通用规则：
> - `position` / `rotation` / `localScale` 按解析结果替换
> - 用户未指定的维度保留默认值
> - 相对位置计算优先在同一次 `execute_csharp_script` 内完成

### 4a. Prefab

重要：`unity_gameobject` 只会创建空对象，永远不要用它来放真实 Prefab，必须用 `execute_csharp_script`。

适用场景：
- `generate_3d_model_by_rodin` / `generate_3d_model_by_tripo_p1` / `generate_animated_character` / `generate_rigged_model` 正常流程：任务启动后立刻用 `prefab_output_path` 放置占位 Prefab；生成完成后 Placeholder 子节点会自动被真实模型替换，无需二次调用
- `search_assets` 下载完成后实例化 Prefab
- 将已完成的 Prefab 放入另一个场景

```csharp
using UnityEngine;
using UnityEditor;

AssetDatabase.Refresh();
var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("{prefab_path}");
if (prefab == null) {
    AssetDatabase.ImportAsset("{prefab_path}", ImportAssetOptions.ForceUpdate);
    prefab = AssetDatabase.LoadAssetAtPath<GameObject>("{prefab_path}");
}
if (prefab == null) { Debug.LogError("Prefab 未找到: {prefab_path}"); return; }

var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
instance.name = "{name}";
instance.transform.position = new Vector3(0f, 0f, 0f);
Undo.RegisterCreatedObjectUndo(instance, "放置 Prefab");
UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
```

带动画角色通过 `generate_animated_character` 生成时，Animator 与 AnimatorController 会在生成完成后自动绑定；这里只需正常实例化。

### 4b. Sprite

`placeholder_path` 和 `sprite_path` 是同一文件。生成任务启动后应立刻放置 `placeholder_path`，生成完成后文件会原地覆盖，已有 `SpriteRenderer` / `Image` 会自动显示真实精灵，无需二次调用。

#### 4b-1. SpriteRenderer（3D / 2D 场景）

关键点：
- 2048px 精灵默认 `PPU=100`，宽度约 20.48 单位，必须设置 `localScale`
- 不要在已有 `MeshRenderer` 的对象上追加 `SpriteRenderer`，应新建空 GameObject

```csharp
using UnityEngine;
using UnityEditor;

AssetDatabase.Refresh();
var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("{sprite_path}");
if (sprite == null) {
    AssetDatabase.ImportAsset("{sprite_path}", ImportAssetOptions.ForceUpdate);
    sprite = AssetDatabase.LoadAssetAtPath<Sprite>("{sprite_path}");
}
if (sprite == null) { Debug.LogError("精灵未找到: {sprite_path}"); return; }

var go = new GameObject("{name}");
var sr = go.AddComponent<SpriteRenderer>();
sr.sprite = sprite;
go.transform.position = new Vector3(0f, 1f, 0f);
go.transform.localScale = Vector3.one * 0.05f;

Undo.RegisterCreatedObjectUndo(go, "放置精灵");
UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
```

缩放参考：

| 目标尺寸 | localScale |
|---------|-----------|
| ~1 单位（图标/装饰） | `0.05f` |
| ~2 单位（小道具） | `0.10f` |
| ~4 单位（角色） | `0.20f` |

#### 4b-2. UI Image（Canvas）

`UnityEngine.UI.Image` 接受的是 `Sprite`。如果资产是 `Texture2D`，请改用 `RawImage`（见 4i）。

```csharp
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

AssetDatabase.Refresh();
var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("{sprite_path}");
if (sprite == null) {
    AssetDatabase.ImportAsset("{sprite_path}", ImportAssetOptions.ForceUpdate);
    sprite = AssetDatabase.LoadAssetAtPath<Sprite>("{sprite_path}");
}
if (sprite == null) { Debug.LogError("精灵未找到: {sprite_path}"); return; }

var canvas = Object.FindObjectOfType<Canvas>();
if (canvas == null) {
    var canvasGO = new GameObject("Canvas");
    canvas = canvasGO.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    canvasGO.AddComponent<CanvasScaler>();
    canvasGO.AddComponent<GraphicRaycaster>();
    Undo.RegisterCreatedObjectUndo(canvasGO, "创建 Canvas");
}

var imageGO = new GameObject("{name}");
imageGO.transform.SetParent(canvas.transform, false);
var image = imageGO.AddComponent<Image>();
image.sprite = sprite;
image.SetNativeSize();

Undo.RegisterCreatedObjectUndo(imageGO, "放置精灵 UI");
UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
```

### 4c. AudioClip BGM

```csharp
using UnityEngine;
using UnityEditor;

AssetDatabase.Refresh();
var clip = AssetDatabase.LoadAssetAtPath<AudioClip>("{audio_path}");
if (clip == null) {
    AssetDatabase.ImportAsset("{audio_path}", ImportAssetOptions.ForceUpdate);
    clip = AssetDatabase.LoadAssetAtPath<AudioClip>("{audio_path}");
}
if (clip == null) { Debug.LogError("AudioClip 未找到: {audio_path}"); return; }

var go = GameObject.Find("BGMPlayer");
if (go == null) {
    go = new GameObject("BGMPlayer");
    Undo.RegisterCreatedObjectUndo(go, "创建 BGM AudioSource");
}
var source = go.GetComponent<AudioSource>();
if (source == null) source = go.AddComponent<AudioSource>();
source.clip = clip;
source.loop = true;
source.spatialBlend = 0f;
source.playOnAwake = true;

EditorUtility.SetDirty(go);
UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
```

### 4d. AudioClip SFX

```csharp
using UnityEngine;
using UnityEditor;

AssetDatabase.Refresh();
var clip = AssetDatabase.LoadAssetAtPath<AudioClip>("{audio_path}");
if (clip == null) {
    AssetDatabase.ImportAsset("{audio_path}", ImportAssetOptions.ForceUpdate);
    clip = AssetDatabase.LoadAssetAtPath<AudioClip>("{audio_path}");
}
if (clip == null) { Debug.LogError("AudioClip 未找到: {audio_path}"); return; }

var go = GameObject.Find("{targetName}");
if (go == null) {
    go = new GameObject("SFXEmitter_{name}");
    Undo.RegisterCreatedObjectUndo(go, "创建 SFX AudioSource");
}
var source = go.GetComponent<AudioSource>();
if (source == null) source = go.AddComponent<AudioSource>();
source.clip = clip;
source.loop = false;
source.spatialBlend = 1f;
source.playOnAwake = false;

EditorUtility.SetDirty(go);
UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
```

### 4e. Material

```csharp
using UnityEngine;
using UnityEditor;

AssetDatabase.Refresh();
var mat = AssetDatabase.LoadAssetAtPath<Material>("{material_path}");
if (mat == null) {
    AssetDatabase.ImportAsset("{material_path}", ImportAssetOptions.ForceUpdate);
    mat = AssetDatabase.LoadAssetAtPath<Material>("{material_path}");
}
if (mat == null) { Debug.LogError("材质未找到: {material_path}"); return; }

var go = GameObject.Find("{targetName}");
if (go == null) { Debug.LogError("目标物体未找到: {targetName}"); return; }
var renderer = go.GetComponent<Renderer>();
if (renderer == null) { Debug.LogError(go.name + " 没有 Renderer 组件"); return; }

Undo.RecordObject(renderer, "应用生成材质");
renderer.sharedMaterial = mat;
EditorUtility.SetDirty(renderer);
UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
```

### 4f. Cubemap / 天空盒

传入的是 `placeholder_material_path` 或最终 `.mat` 路径，不要传 `texture_path` PNG。

```csharp
using UnityEngine;
using UnityEditor;

AssetDatabase.Refresh();
var skyboxMat = AssetDatabase.LoadAssetAtPath<Material>("{skybox_material_path}");
if (skyboxMat == null) {
    AssetDatabase.ImportAsset("{skybox_material_path}", ImportAssetOptions.ForceUpdate);
    skyboxMat = AssetDatabase.LoadAssetAtPath<Material>("{skybox_material_path}");
}
if (skyboxMat == null) { Debug.LogError("天空盒材质未找到: {skybox_material_path}"); return; }

RenderSettings.skybox = skyboxMat;
DynamicGI.UpdateEnvironment();
UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
```

### 4g. AnimationClip

如场景中不存在目标对象，则自动创建 `GameObject + SpriteRenderer + Animator`。建议控制器保存在 `folder_path` 同目录。

```csharp
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

AssetDatabase.Refresh();
var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("{animation_clip_path}");
if (clip == null) {
    AssetDatabase.ImportAsset("{animation_clip_path}", ImportAssetOptions.ForceUpdate);
    clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("{animation_clip_path}");
}
if (clip == null) { Debug.LogError("AnimationClip 未找到: {animation_clip_path}"); return; }

var go = GameObject.Find("{characterName}");
if (go == null) {
    go = new GameObject("{characterName}");
    go.AddComponent<SpriteRenderer>();
    go.transform.position = new Vector3(0f, 0f, 0f);
    Undo.RegisterCreatedObjectUndo(go, "创建角色 GameObject");
}

string controllerPath = "{folder_path}/{characterName}Controller.controller";
var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
var state = controller.layers[0].stateMachine.AddState(clip.name);
state.motion = clip;
controller.layers[0].stateMachine.defaultState = state;

var animator = go.GetComponent<Animator>();
if (animator == null) animator = go.AddComponent<Animator>();
animator.runtimeAnimatorController = controller;

EditorUtility.SetDirty(go);
AssetDatabase.SaveAssets();
UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
```

### 4h. TerrainData

情形 A：如果输入是 `generate_terrain` 返回的 `heightmap_path`，真正的放置步骤是 `apply_terrain_heightmap`：

```text
generate_terrain
→ query_terrain_status（拿到 heightmap_path）
→ apply_terrain_heightmap
→ query_terrain_apply_status（等待 completed）
```

情形 B：如果输入已经是 `TerrainData` 资产，则直接创建 Terrain：

```csharp
using UnityEngine;
using UnityEditor;

AssetDatabase.Refresh();
var terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>("{terrain_data_path}");
if (terrainData == null) {
    AssetDatabase.ImportAsset("{terrain_data_path}", ImportAssetOptions.ForceUpdate);
    terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>("{terrain_data_path}");
}
if (terrainData == null) { Debug.LogError("TerrainData 未找到: {terrain_data_path}"); return; }

var terrainGO = Terrain.CreateTerrainGameObject(terrainData);
terrainGO.name = "Terrain";
var size = terrainData.size;
terrainGO.transform.position = new Vector3(-size.x / 2f, 0f, -size.z / 2f);

Undo.RegisterCreatedObjectUndo(terrainGO, "放置地形");
UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
```

放置后检查平行光：

```csharp
using UnityEngine;
using UnityEditor;

bool hasDirectional = false;
foreach (var light in Object.FindObjectsOfType<Light>()) {
    if (light.type == LightType.Directional) {
        hasDirectional = true;
        break;
    }
}

if (!hasDirectional) {
    var lightGO = new GameObject("Directional Light");
    var light = lightGO.AddComponent<Light>();
    light.type = LightType.Directional;
    light.color = new Color(1f, 0.96f, 0.84f);
    lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    Undo.RegisterCreatedObjectUndo(lightGO, "创建 Directional Light");
    UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
}
```

### 4i. Texture2D / Image

注意：`Texture2D` 不是 `Sprite`。赋给 UI 时应使用 `RawImage`，不要和 `Image` 混用。

#### 用途 A：赋给 3D 物体材质

```csharp
using UnityEngine;
using UnityEditor;

AssetDatabase.Refresh();
var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("{image_path}");
if (texture == null) {
    AssetDatabase.ImportAsset("{image_path}", ImportAssetOptions.ForceUpdate);
    texture = AssetDatabase.LoadAssetAtPath<Texture2D>("{image_path}");
}
if (texture == null) { Debug.LogError("Texture2D 未找到: {image_path}"); return; }

var go = GameObject.Find("{targetName}");
if (go == null) { Debug.LogError("目标物体未找到: {targetName}"); return; }
var renderer = go.GetComponent<Renderer>();
if (renderer == null || renderer.sharedMaterial == null) { Debug.LogError("目标没有可用材质"); return; }

Undo.RecordObject(renderer.sharedMaterial, "应用生成图片");
renderer.sharedMaterial.mainTexture = texture;
EditorUtility.SetDirty(renderer.sharedMaterial);
UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
```

#### 用途 B：作为 UI RawImage

```csharp
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

AssetDatabase.Refresh();
var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("{image_path}");
if (texture == null) {
    AssetDatabase.ImportAsset("{image_path}", ImportAssetOptions.ForceUpdate);
    texture = AssetDatabase.LoadAssetAtPath<Texture2D>("{image_path}");
}
if (texture == null) { Debug.LogError("Texture2D 未找到: {image_path}"); return; }

var canvas = Object.FindObjectOfType<Canvas>();
if (canvas == null) {
    var canvasGO = new GameObject("Canvas");
    canvas = canvasGO.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    canvasGO.AddComponent<CanvasScaler>();
    canvasGO.AddComponent<GraphicRaycaster>();
    Undo.RegisterCreatedObjectUndo(canvasGO, "创建 Canvas");
}

var go = new GameObject("{name}");
go.transform.SetParent(canvas.transform, false);
var rawImage = go.AddComponent<RawImage>();
rawImage.texture = texture;
go.GetComponent<RectTransform>().sizeDelta = new Vector2(texture.width * 0.5f, texture.height * 0.5f);

Undo.RegisterCreatedObjectUndo(go, "放置 RawImage");
UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
```

## 批量放置

可一次处理多个同类或异类资产。默认横向排布，便于检查。

- 间距策略：优先使用 `spacing = 2f`
- 第 `i` 个对象位置：`new Vector3(i * spacing, 0f, 0f)`
- 想居中时：`x = (i - (count - 1) / 2f) * spacing`

示例：

```csharp
int count = paths.Length;
float spacing = 2f;
for (int i = 0; i < count; i++) {
    float x = (i - (count - 1) / 2f) * spacing;
    instance.transform.position = new Vector3(x, 0f, 0f);
}
```

当用户请求“把三把武器并排放好”或 `search_assets` 批量下载完成后，可直接套用此策略。

## Domain Reload

- 会触发 Domain Reload 的典型操作：把新的 `.cs` 文件写入工程、修改脚本并触发编译、进入某些需要脚本重载的编辑器流程
- 不会触发：`execute_csharp_script`、`AssetDatabase.Refresh()`、实例化 Prefab、创建普通场景对象
- 如果生成任务期间发生 Domain Reload，多数生成 skill 都会自动恢复；这里的放置操作应继续优先使用 `execute_csharp_script`，不要把一次性脚本落盘

## 截图验证

仅当用户反馈”看不到””朝向不对””漂浮””太大/太小”等问题时，才触发截图验证。

截图使用 `unity_screenshot` 工具，有效的 `action` 值为：

| action | 说明 |
|--------|------|
| `capture_scene_camera` | 捕获 Scene 视图（**场景放置验证首选**） |
| `capture_main_camera` | 捕获 Main Camera 渲染结果（运行时视角） |
| `capture` | 捕获当前活跃视图 |
| `capture_specific_camera` | 捕获指定 Camera 组件 |

> **注意**：不存在 `capture_scene_view` 动作，请勿使用。

验证放置效果时，固定使用 `capture_scene_camera`：

```json
{ “action”: “capture_scene_camera” }
```

| 常见问题 | 建议检查 | 常见修复 |
|---------|---------|---------|
| Prefab 倒置/侧躺 | rotation | `Quaternion.Euler(90,0,0)`、`(0,180,0)` |
| Prefab 漂浮/下沉 | Y 轴 | 调整 `position.y` 到地面 |
| Sprite 太大 | scale | 用 `0.05f` / `0.1f` / `0.2f` |
| Skybox 没刷新 | 光照环境 | `DynamicGI.UpdateEnvironment()` |
| Terrain 过暗 | 场景光源 | 自动补 Directional Light |
| UI 图片没显示 | 组件类型 | Sprite 用 `Image`，Texture2D 用 `RawImage` |

## 使用提示

- `generate_sprite`：拿到 `placeholder_path` 后立即调用本 skill，通常最省事
- `search_assets`：下载完成拿到 `prefab_path` 后立即调用本 skill
- `generate_3d_model_by_rodin` / `generate_3d_model_by_tripo_p1` / `generate_animated_character` / `generate_rigged_model`：用 `prefab_output_path` 提前放置占位物体，生成完成自动替换
- `generate_terrain`：常规流程继续走 `apply_terrain_heightmap`，不要绕过
