# Unity 素材加载 + Fallback 模式

提供通用的素材加载策略：优先从磁盘加载已生成的素材，缺失时自动 fallback 到程序化生成。

## 适用场景

- 所有使用 `asset_requirements.json` 定义素材的项目
- 素材由上游流程（如 AI 生成）预先创建
- 需要在素材缺失时仍能运行（graceful degradation）

## 核心模式

```
素材加载优先级：
1. AssetDatabase.LoadAssetAtPath（Editor 脚本中）
2. 程序化生成 fallback（运行时脚本中）
```

## 代码模板

### Editor 脚本中的加载方法（用于 GameSceneBuilder 等）

```csharp
// ---- 素材加载核心方法（放在 Editor 脚本中）----

/// 优先加载 Assets/Materials/{name}Mat.mat，缺失则创建纯色材质
static Material LoadOrCreateMaterial(string name, Color fallbackColor)
{
    string path = $"Assets/Materials/{name}Mat.mat";
    Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
    if (mat != null)
    {
        Debug.Log($"[AssetLoader] Loaded material: {path}");
        return mat;
    }
    Debug.Log($"[AssetLoader] Material not found at {path}, creating fallback...");
    return CreateFlatMaterial(name, fallbackColor);
}

/// 优先加载 Assets/Models/{name}.asset，缺失则调用 fallback 函数
static Mesh LoadMeshOrFallback(string name, System.Func<Mesh> fallbackCreator)
{
    string path = $"Assets/Models/{name}.asset";
    Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
    if (mesh != null)
    {
        Debug.Log($"[AssetLoader] Loaded mesh: {path}");
        return mesh;
    }
    Debug.Log($"[AssetLoader] Mesh not found at {path}, using procedural fallback...");
    return fallbackCreator();
}

/// Fallback：程序化创建纯色材质
static Material CreateFlatMaterial(string name, Color color)
{
    var mat = new Material(Shader.Find("Standard"));
    mat.name = name;
    mat.color = color;
    mat.SetFloat("_Glossiness", 0f);  // 注意：Standard shader 用 _Glossiness 不是 _Smoothness
    mat.SetFloat("_Metallic", 0f);
    string dir = "Assets/Materials";
    if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder("Assets", "Materials");
    AssetDatabase.CreateAsset(mat, $"{dir}/{name}Mat.mat");
    return AssetDatabase.LoadAssetAtPath<Material>($"{dir}/{name}Mat.mat");
}
```

### 运行时脚本中的 Fallback 模式（用于 MonoBehaviour）

```csharp
// 在需要 Mesh 的 MonoBehaviour 中，将 Mesh 声明为 public 字段：

[Header("Meshes (由 SceneBuilder 赋值，如为 null 则 fallback 到程序化生成)")]
public Mesh myMesh;

void Start()
{
    FallbackMeshes();
    // ... 其他初始化 ...
}

void FallbackMeshes()
{
    // 仅对 null 的字段做 fallback，已赋值的不覆盖
    if (myMesh == null) myMesh = ProceduralMeshes.CreateSomeMesh();
}
```

### Editor 脚本中赋值 Mesh 给运行时组件

```csharp
// 在 GameSceneBuilder 等 Editor 脚本中：
var myComponent = obj.AddComponent<MyComponent>();

// 材质：通过 LoadOrCreateMaterial 加载
myComponent.myMaterial = LoadOrCreateMaterial("MyMat", Color.white);

// Mesh：通过 AssetDatabase 加载（null 则运行时 fallback）
myComponent.myMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Models/MyMesh.asset");
```

## 集成说明

### 步骤 1：定义素材路径约定

```
Assets/
├── Materials/          ← .mat 文件，命名：{Name}Mat.mat
│   ├── RoadMat.mat
│   └── PlayerMat.mat
├── Models/             ← .asset 文件（Mesh），命名：{Name}.asset
│   ├── PlayerMesh.asset
│   └── RoadTile.asset
```

### 步骤 2：在 Editor 脚本中使用 LoadOrCreateMaterial / LoadMeshOrFallback

- 材质：`LoadOrCreateMaterial("Road", new Color(0.35f, 0.35f, 0.4f))`
- Mesh：`LoadMeshOrFallback("PlayerMesh", ProceduralMeshes.CreatePlayerMesh)`

### 步骤 3：在运行时脚本中声明 public Mesh/Material 字段

- 由 Editor 脚本赋值（序列化到场景）
- `FallbackMeshes()` 在 Start 中对 null 字段做程序化 fallback

### 步骤 4：asset_requirements.json 中定义 output_path

每个素材必须有 `output_path` 字段，与代码中的加载路径一致：
```json
{
  "name": "PlayerMesh",
  "output_path": "Assets/Models/PlayerMesh.asset",
  "fallback_generator": "ProceduralMeshes.CreatePlayerMesh()"
}
```

## 已知陷阱

| 陷阱 | 后果 | 解决 |
|------|------|------|
| 运行时脚本中用 `AssetDatabase` | 编译报错（仅 Editor 可用） | 运行时用 public 字段 + fallback，不用 AssetDatabase |
| Standard shader 用 `_Smoothness` | 设置无效 | **正确属性名是 `_Glossiness`** |
| LoadAssetAtPath 路径大小写错误 | 返回 null | 路径必须与磁盘文件完全一致 |
| Mesh .asset 文件中包含多个子对象 | LoadAssetAtPath 可能加载错误对象 | 确保每个 .asset 只包含一个 Mesh |
| fallback 创建的材质覆盖了已有文件 | 上游生成的材质被覆盖 | LoadOrCreateMaterial 先检查文件是否存在 |