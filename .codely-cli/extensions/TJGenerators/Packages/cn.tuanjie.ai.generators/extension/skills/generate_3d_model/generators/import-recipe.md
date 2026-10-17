# Recipe 导入（实验路径 · 后处理提示词化）

> **状态：rodin 文生3D 全链路验证通过（2026-09-15 实测：11574 顶点、auto-fit 世界尺寸精确 1m、
> 材质重映射 + PBR 贴图提取正常、无错误着色器）。** 生产默认路径仍是 `import_3d_model_from_url`（SKILL.md 第二段）。
> 本 recipe 是第二段的**替代方案**：不依赖 TJGenerators custom tool，由 agent 用通用工具
> （`run_shell_command` / `unity_editor refresh` / `exec_editor_script`）按模板完成
> 下载 → 导入 → 材质 → auto-fit 全链路。用于：① 验证「后处理全提示词化」效果；
> ② 未安装 TJGenerators / 无 custom tool 环境的降级导入。
>
> ⚠️ **执行纪律：模板脚本逐字复制，只允许改标记的参数行，禁止改任何逻辑行。**
> auto-fit 的 root-local 测量法是修过竞态 bug 的版本，改写会重新引入 ~1cm 缩小问题。

## 适用范围

- ✅ 静态 `.fbx`（rodin / tripo / hunyuan `with_fbx=true`）
- ❌ 带绑骨/动画的模型（`add_motion` → 用包内工具或 animated-character skill）
- ❌ `.obj` / `.zip`（生产 import 工具支持，本 recipe v1 未覆盖）
- ❌ `.glb`（生成时没开 `with_fbx` → 报告不兼容，不自动重生成）

## 流程

第一段（MCP 生成 + 轮询 + `check_task` 拿 URL）与生产路径**完全相同**，见 [mcp.md](mcp.md)。
本 recipe 只替换第二段：

```
1. 拿到 .fbx URL（+ rendered_image URL 如有）
2. curl 下载到 Assets/Models/<Name>/          ← run_shell_command
3. unity_editor refresh                        ← 导入磁盘新文件
4. exec_editor_script 跑下方模板脚本            ← 只改三个参数
5. 读结构化返回验证 → 实例化 Prefab 到场景
```

同步链路，全程 agent 驱动，**无占位 Cube、无 bg_task_done、无 query 兜底**；
任何一步失败直接重跑对应步骤（CDN URL 永久有效）。

## 第 2 步下载命令（PowerShell）

```powershell
# 目录名用 ASCII + 下划线，与 fbx 文件名一致
New-Item -ItemType Directory -Force "Assets/Models/<Name>" | Out-Null
curl.exe -L -o "Assets/Models/<Name>/<name>.fbx" "<fbx_url>"

# rendered_image（如有；tripo 产物通常有，rodin 贴图一般内嵌于 FBX 可跳过）
curl.exe -L -o "Assets/Models/<Name>/<name>_render.webp" "<rendered_image_url>"
```

## 第 4 步模板脚本（exec_editor_script，逐字复制）

只改开头 **三个参数**，其余逐字保留：

```csharp
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// ===== 只改这三个参数 =====
string fbxAssetPath = "Assets/Models/<Name>/<name>.fbx"; // 已下载并 refresh 过的 FBX
float targetSizeMeters = 1f;                             // auto-fit 目标最长边（米）
string renderedTexturePath = null;                       // rendered_image 的 Unity 路径，无则保持 null

// ===== 以下逐字保留，禁止修改 =====
string parentDir = Path.GetDirectoryName(fbxAssetPath).Replace("\\", "/");
string baseName = Path.GetFileNameWithoutExtension(fbxAssetPath);

// 1) 提取内嵌贴图到独立 .fbm 子目录（每模型独立，避免固定贴图名互相覆盖）
var importer = AssetImporter.GetAtPath(fbxAssetPath) as ModelImporter;
if (importer == null)
    return new { error = "ModelImporter not found (fbx only): " + fbxAssetPath };

string extractDir = parentDir + "/" + baseName + ".fbm";
string absExtract = Application.dataPath + "/" + extractDir.Substring("Assets/".Length);
Directory.CreateDirectory(absExtract);
importer.ExtractTextures(extractDir);
foreach (var f in Directory.GetFiles(absExtract))
    AssetDatabase.ImportAsset(extractDir + "/" + Path.GetFileName(f), ImportAssetOptions.ForceUpdate);

// 法线贴图类型修正（Normal* / *_normal → NormalMap，避免导入设置弹窗）
foreach (var f in Directory.GetFiles(absExtract))
{
    string ext = Path.GetExtension(f).ToLowerInvariant();
    if (ext != ".png" && ext != ".jpg" && ext != ".jpeg") continue;
    string fileName = Path.GetFileName(f);
    string nameNoExt = Path.GetFileNameWithoutExtension(fileName);
    bool isNormal = fileName.StartsWith("Normal", System.StringComparison.OrdinalIgnoreCase)
                 || nameNoExt.EndsWith("_normal", System.StringComparison.OrdinalIgnoreCase);
    if (!isNormal) continue;
    string unityPath = extractDir + "/" + fileName;
    var ti = AssetImporter.GetAtPath(unityPath) as TextureImporter;
    if (ti != null && ti.textureType != TextureImporterType.NormalMap)
    {
        ti.textureType = TextureImporterType.NormalMap;
        ti.SaveAndReimport();
    }
}

// 2) 材质按贴图名本地重映射
importer.isReadable = true;
importer.SearchAndRemapMaterials(ModelImporterMaterialName.BasedOnTextureName, ModelImporterMaterialSearch.Local);
importer.SaveAndReimport();

var modelGo = AssetDatabase.LoadAssetAtPath<GameObject>(fbxAssetPath);
if (modelGo == null)
    return new { error = "FBX load failed: " + fbxAssetPath };

// 3) rendered_image 应用为主贴图（mainTexture / _BaseMap / _MainTex）
int appliedRendered = 0;
if (!string.IsNullOrEmpty(renderedTexturePath))
{
    var renderedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(renderedTexturePath);
    if (renderedTex == null)
        return new { error = "rendered_image load failed: " + renderedTexturePath };
    foreach (var rend in modelGo.GetComponentsInChildren<Renderer>(true))
        foreach (var mat in rend.sharedMaterials)
        {
            if (mat == null) continue;
            mat.mainTexture = renderedTex;
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", renderedTex);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", renderedTex);
            EditorUtility.SetDirty(mat);
            appliedRendered++;
        }
    AssetDatabase.SaveAssets();
}

// 4) 网格校验（顶点数必须 > 0）
int totalVerts = 0;
foreach (var mf in modelGo.GetComponentsInChildren<MeshFilter>(true))
    if (mf.sharedMesh != null) totalVerts += mf.sharedMesh.vertexCount;
foreach (var smr in modelGo.GetComponentsInChildren<SkinnedMeshRenderer>(true))
    if (smr.sharedMesh != null) totalVerts += smr.sharedMesh.vertexCount;
if (totalVerts == 0)
    return new { error = "mesh validate failed: 0 vertices, regenerate" };

// 5) 默认白色材质兜底（null / 错误着色器）
Shader ResolveLitShader()
{
    var rp = GraphicsSettings.defaultRenderPipeline;
    if (rp == null) return Shader.Find("Standard") ?? Shader.Find("Unlit/Texture");
    var rpName = rp.GetType().Name;
    if (rpName.Contains("Universal")) return Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
    if (rpName.Contains("HDRender")) return Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
    return Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("HDRP/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Unlit/Texture");
}
string defaultMatPath = "Assets/TJGenerators/DefaultWhite.mat";
Material defaultMat = AssetDatabase.LoadAssetAtPath<Material>(defaultMatPath);
if (defaultMat == null)
{
    Directory.CreateDirectory(Application.dataPath + "/TJGenerators");
    defaultMat = new Material(ResolveLitShader());
    AssetDatabase.CreateAsset(defaultMat, defaultMatPath);
}
if (defaultMat.HasProperty("_BaseColor")) defaultMat.SetColor("_BaseColor", Color.white);
if (defaultMat.HasProperty("_Color")) defaultMat.SetColor("_Color", Color.white);
EditorUtility.SetDirty(defaultMat);

// 6) auto-fit 实例化：在实例根节点本地空间测内容包围盒最长边 → 归一到 targetSize。
//    root-local 测量通过 InverseTransformPoint 剔除根节点自身缩放（含厘米单位 FBX 的
//    ×100 文件单位换算），不受该换算传播时序影响——与包内修复竞态后的实现一致。
float ContentLongestEdgeInRootLocal(GameObject root)
{
    var renderers = root.GetComponentsInChildren<Renderer>(true);
    if (renderers == null || renderers.Length == 0) return 0f;
    bool has = false; Vector3 min = Vector3.zero, max = Vector3.zero;
    void Encap(Vector3 p)
    {
        if (!has) { min = p; max = p; has = true; return; }
        min = Vector3.Min(min, p); max = Vector3.Max(max, p);
    }
    var rootT = root.transform;
    foreach (var r in renderers)
    {
        if (r == null) continue;
        Bounds b = r.bounds;
        Encap(rootT.InverseTransformPoint(b.min));
        Encap(rootT.InverseTransformPoint(b.max));
        Encap(rootT.InverseTransformPoint(new Vector3(b.min.x, b.min.y, b.max.z)));
        Encap(rootT.InverseTransformPoint(new Vector3(b.min.x, b.max.y, b.min.z)));
        Encap(rootT.InverseTransformPoint(new Vector3(b.max.x, b.min.y, b.min.z)));
        Encap(rootT.InverseTransformPoint(new Vector3(b.min.x, b.max.y, b.max.z)));
        Encap(rootT.InverseTransformPoint(new Vector3(b.max.x, b.min.y, b.max.z)));
        Encap(rootT.InverseTransformPoint(new Vector3(b.max.x, b.max.y, b.min.z)));
    }
    if (!has) return 0f;
    Vector3 size = max - min;
    float longest = Mathf.Max(Mathf.Max(size.x, size.y), size.z);
    return longest > Mathf.Epsilon ? longest : 0f;
}

var wrapper = new GameObject(baseName);
try
{
    var instance = PrefabUtility.InstantiatePrefab(modelGo, wrapper.transform) as GameObject;
    if (instance == null)
        return new { error = "InstantiatePrefab failed: " + fbxAssetPath };
    instance.name = "GeneratedModel";
    instance.transform.localPosition = Vector3.zero;
    instance.transform.localRotation = Quaternion.identity;
    instance.transform.localScale = Vector3.one;

    float contentLongest = ContentLongestEdgeInRootLocal(instance);
    float appliedScale = 0f;
    if (targetSizeMeters > 0f && contentLongest > 0f)
    {
        appliedScale = targetSizeMeters / contentLongest;
        instance.transform.localScale = Vector3.one * appliedScale;
    }

    int fixedMaterials = 0;
    foreach (var rend in instance.GetComponentsInChildren<Renderer>(true))
    {
        if (rend.sharedMaterials == null) continue;
        var mats = rend.sharedMaterials; bool changed = false;
        for (int i = 0; i < mats.Length; i++)
        {
            var m = mats[i];
            if (m == null || m.shader == null || m.shader.name == "Hidden/InternalErrorShader")
            { mats[i] = defaultMat; changed = true; fixedMaterials++; }
        }
        if (changed) rend.sharedMaterials = mats;
    }

    string prefabPath = parentDir + "/" + baseName + ".prefab";
    PrefabUtility.SaveAsPrefabAsset(wrapper, prefabPath);
    return new
    {
        prefab_path = prefabPath,
        model_path = fbxAssetPath,
        total_vertices = totalVerts,
        content_longest_edge = contentLongest,
        auto_fit_scale = appliedScale,
        rendered_textures_applied = appliedRendered,
        default_material_fixes = fixedMaterials,
        extract_dir = extractDir
    };
}
finally
{
    UnityEngine.Object.DestroyImmediate(wrapper);
}
```

## 第 5 步验证清单

返回对象必须满足，否则视为失败并按差异表处理：

| 字段 | 判定 |
|---|---|
| `error` | 必须不存在 |
| `total_vertices` | > 0 |
| `auto_fit_scale` | > 0（= 0 说明无可渲染网格或未做归一） |
| `prefab_path` | 存在，直接用于实例化 |

场景实例化（可直接跑）：

```csharp
using UnityEditor;
using UnityEngine;
var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("<prefab_path>");
var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
go.transform.position = new Vector3(0f, 0.5f, 0f);
return new { scene_instance = go.name, world_size = go.GetComponentInChildren<Renderer>().bounds.size };
```

`world_size` 最长边应 ≈ `targetSizeMeters`（1m）。

## Provider 特例

| Provider | 要点 |
|---|---|
| rodin | 贴图内嵌 FBX；cm 单位 ×100 换算由 root-local 测量法天然处理，**勿手改 importer scale** |
| tripo | 下载 `rendered_image` (webp) 并传入 `renderedTexturePath`；多模型勿混放同一目录（`.fbm` 独立子目录已规避） |
| hunyuan | 生成时必须 `with_fbx=true`（默认 GLB 本 recipe 不支持） |

## 与 `import_3d_model_from_url` 的差异

| 能力 | import 工具（生产） | 本 recipe |
|---|---|---|
| Cube 占位 + `<bg_task_done>` 异步通知 | ✅ | ❌（同步链路，无需通知） |
| 域重载恢复 / `query_local_task` 兜底 | ✅ | ❌（失败直接重跑，URL 永久） |
| History 记录 + TuanjieAI/Session 标签 | ✅ | ❌（落盘为普通资产） |
| auto-fit（root-local 竞态安全版） | ✅ | ✅ 模板内置 |
| 材质重映射 + 法线修正 + rendered_image + 白材质兜底 | ✅ | ✅ 模板内置 |
| `.obj` / `.zip` / `add_motion` | ✅ | ❌（仅静态 `.fbx`） |
| 依赖 TJGenerators custom tool | 需要 | **不需要** |

## 故障排查

| 现象 | 处理 |
|---|---|
| `ModelImporter not found` | URL 是 `.glb` / `.obj` / 路径写错；glb → 报告格式不兼容，不自动重生成 |
| `mesh validate failed: 0 vertices` | 检查 FBX 下载及导入错误；保留已有 URL，不自动重生成 |
| `auto_fit_scale` = 0 且网格正常 | 检查是否改动了测量逻辑（应逐字复制） |
| 模型洋红色 | 项目是 URP/HDRP 时白材质兜底已按管线选 shader；检查 FBX 自带材质 shader 是否被正确导入 |
| webp 显示异常 | Tuanjie 2022.3 对 webp 支持有限时，可改下载 rendered_image 为 png（若 URL 可替换扩展名），或跳过 rendered_image 用内嵌贴图 |
