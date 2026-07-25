# AttributeOverviewPro 资产精简方案 — 子资产（Sub-Asset）架构

## 1. 现状分析

### 1.1 当前资产生成流程

当前 `AttributeOverviewPro` 模块在用户首次访问时，会在 `Assets/Editor Default Resources/Aesir Inspector/Attribute Overview Pro/` 下自动生成大量 `.asset` 文件：

| 资产类别 | 数量 | 创建方式 | 存储路径 |
|----------|------|----------|----------|
| PanelSO | ~70 | `AttributeOverviewDatabaseSO.GetAllAttributePanels()` 通过 `CreateInstance` + `AssetDatabase.CreateAsset` | `.../Attribute Overview Pro/Panels/` |
| ExampleSO (Unity 序列化) | ~117 | `AttributeExampleSO<T>.Instance` → `GetSingletonAssetAndDeleteOther<T>()` | `.../Attribute Overview Pro/Attribute Examples/` |
| ExampleSO (Odin 序列化) | ~6 | `OdinAttributeExampleSO<T>.Instance` → `GetSingletonAssetAndDeleteOther<T>()` | 同上 |
| DatabaseSO | 1 | `AttributeOverviewDatabaseSO.Instance` → `GetSingletonAssetAndDeleteOther<T>()` | `.../Attribute Overview Pro/` |
| **合计** | **~194** | | |

### 1.2 关键代码路径

#### PanelSO 创建（`AttributeOverviewDatabaseSO.GetAllAttributePanels()`）

```
TypeCache.GetTypesDerivedFrom<AbstractAttributePanelSO>()
  → 对每个类型检查是否已有 .asset
    → 无则 CreateInstance(type) + AssetDatabase.CreateAsset(asset, path)
```

#### ExampleSO 创建（`AttributeExampleSO<T>.Instance` / `OdinAttributeExampleSO<T>.Instance`）

```
AttributeData 构造函数（如 ButtonAttributeData）
  → 调用 XxxExampleSO.Instance
    → GetSingletonAssetAndDeleteOther<T>(AttributeExamplesPath)
      → AssetDatabase.FindAssets("t:" + typeof(T))
      → 无则 CreateInstance<T>() + AssetDatabase.CreateAsset(...)
```

#### 数据库初始化（`AttributeOverviewDatabaseSO.Initialize()`）

```
Instance getter
  → GetSingletonAssetAndDeleteOther<AttributeOverviewDatabaseSO>(...)
  → 若 AttributePanelMap 为空 → Initialize()
    → GetAllAttributePanels() → 创建/加载所有 PanelSO
    → 遍历 panels → panel.Initialize()
      → SetData(new XxxAttributeData())
        → AttributeData 构造函数 → XxxExampleSO.Instance → 创建 ExampleSO
```

### 1.3 核心约束

1. **PanelSO 是 `SerializedScriptableObject`（Odin 序列化）**：使用 `[OnInspectorInit]`、`[OnInspectorGUI]`、`[InlineEditor]` 等 Odin 特性，必须作为 `UnityEngine.Object` 存在才能被 `OdinMenuEditorWindow` 选中和检视。

2. **ExampleSO 有两种基类**：
   - `AttributeExampleSO<T>` : `ScriptableObject`（Unity 原生序列化）— 117 个
   - `OdinAttributeExampleSO<T>` : `SerializedScriptableObject`（Odin 序列化）— 6 个（Dictionary、Polymorphic、TableMatrix 等需要 Odin 序列化的类型）

3. **`[InlineEditor]` 依赖 ScriptableObject**：`AbstractAttributePanelSO` 中的 `currentSelectedExample` 字段使用 `[InlineEditor]` 显示选中的 ExampleSO，要求目标必须是 `ScriptableObject`。

4. **`OdinMenuTree.AddObjectAtPath(path, panel)`**：接受任意 `UnityEngine.Object`，不要求是独立资产文件。

5. **`AttributeOverviewPanelSO<T>.Instance` 基本未被使用**：经搜索，只有 `OdinSyntaxHighlighterPanelSO.Instance`（MiniTools 模块）使用了此模式，AttributeOverviewPro 的 PanelSO 由数据库直接管理。

6. **`AttributeExampleSO<T>.Instance` / `OdinAttributeExampleSO<T>.Instance` 被广泛使用**：每个 `AttributeData` 的构造函数都通过 `.Instance` 获取对应的 ExampleSO 实例。

---

## 2. 用户方案可行性分析

### 2.1 用户提出的方案

> 用一个或几个（按类别）SO 资产，存放所有 ExamplePanel。使用 `CreateInstance` 动态生成，然后持久化保存。不想要新增过多资产。

### 2.2 可行性结论：✅ 可行

用户方案的本质是 **Sub-Asset（子资产）架构**：
- 使用 `ScriptableObject.CreateInstance<T>()` 动态创建实例
- 使用 `AssetDatabase.AddObjectToAsset(obj, parentAssetPath)` 将实例作为子资产嵌入到一个（或几个）父 `.asset` 文件中
- 子资产是完整的 `UnityEngine.Object` 实例，所有 Odin/Unity 检视器功能正常工作
- 持久化由父资产文件自动承载

### 2.3 关键技术验证点

| 验证项 | 结论 | 说明 |
|--------|------|------|
| `SerializedScriptableObject` 作为子资产 | ✅ 支持 | Odin 序列化数据存储在父资产文件中，已有多项目验证 |
| `OdinMenuTree` 引用子资产 | ✅ 支持 | `tree.AddObjectAtPath` 接受任意 `UnityEngine.Object` |
| `[InlineEditor]` 检视子资产 | ✅ 支持 | 子资产是 `ScriptableObject`，Odin 正常检视 |
| 子资产间的引用序列化 | ✅ 支持 | Unity/Odin 通过 GUID + LocalFileID 序列化同文件内引用 |
| Domain Reload 后恢复 | ✅ 支持 | 子资产从父 `.asset` 文件重新加载 |
| `HideFlags` 控制 | ✅ 可控 | 默认 `HideInHierarchy`，用户在 Project 窗口看不到子资产 |

### 2.4 潜在风险

1. **Odin 序列化保存时机**：修改子资产后需显式调用 `EditorUtility.SetDirty(parentSO)` + `AssetDatabase.SaveAssets()`，否则 Odin 序列化数据可能未写入磁盘。
2. **子资产命名冲突**：同类型子资产的 `name` 需唯一，否则在 Project 窗口选中父资产时可能混淆。
3. **迁移旧资产**：已存在的 ~194 个独立 `.asset` 文件需要清理，否则用户项目中残留无用文件。

---

## 3. 推荐方案设计

### 3.1 方案概述：单数据库子资产架构

**将所有 PanelSO 和 ExampleSO 作为 `AttributeOverviewDatabaseSO` 的子资产嵌入到一个 `.asset` 文件中。**

| 指标 | 现状 | 方案后 |
|------|------|--------|
| `.asset` 文件数 | ~194 | **1** |
| 子资产数 | 0 | ~193（70 PanelSO + 123 ExampleSO） |
| 用户可见资产膨胀 | 严重 | **零**（仅 1 个数据库文件） |

### 3.2 为什么选 1 个文件而非 12 个（按类别）

- **复杂度**：1 个文件的管理逻辑最简单
- **性能**：~193 个子资产从 1 个文件加载，性能远优于 194 个独立文件
- **引用完整性**：所有子资产在同一文件内，引用更可靠
- **按分类的需求**：分类逻辑已在 `AttributePanelArrayMap` 中维护，无需物理分文件

### 3.3 架构变更图

```
现状:
  DatabaseSO.asset (1)
  Panels/PanelA.asset, PanelB.asset, ... (70)
  Attribute Examples/ExA.asset, ExB.asset, ... (123)
  = 194 个 .asset 文件

方案后:
  AttributeOverviewDatabaseSO.asset (1)
    ├── [子资产] ButtonAttributePanelSO
    ├── [子资产] ButtonExampleSO
    ├── [子资产] BoxGroupAttributePanelSO
    ├── [子资产] BoxGroupExampleSO
    ├── ... (共 ~193 个子资产)
  = 1 个 .asset 文件
```

### 3.4 需要修改的文件清单

| # | 文件 | 变更类型 | 说明 |
|---|------|----------|------|
| 1 | `Core/AttributeOverviewDatabaseSO.cs` | **重构** | 核心变更：PanelSO 创建改为 `AddObjectToAsset`；新增 ExampleSO 子资产管理 |
| 2 | `Abstract/AttributeExampleSO.cs` | **重构** | `Instance` getter 改为从数据库子资产获取/创建 |
| 3 | `Abstract/OdinAttributeExampleSO.cs` | **重构** | 同上 |
| 4 | `Abstract/AbstractAttributePanelSO.cs` | **微调** | `AttributeOverviewPanelSO<T>.Instance` 改为从数据库子资产获取（或标记弃用） |
| 5 | `Runtime/Unity/Core/AesirInspectorPaths.cs` | **微调** | `AttributePanelsPath` 和 `AttributeExamplesPath` 可能不再需要，或保留用于迁移清理 |

### 3.5 不需要修改的部分

- `AttributeData/` 下的 90 个文件 — 数据定义不变，仍通过 `.Instance` 获取 ExampleSO
- `AttributePanels/` 下的 70 个 PanelSO 类 — 类定义不变
- `UsageExamples/` 下的 123 个 ExampleSO 类 — 类定义不变
- `AbstractAttributePanelSO` 的渲染逻辑 — 渲染器、代码预览等不依赖资产存储方式
- `AttributeOverviewWindow` — 窗口逻辑不变，仍通过 `DatabaseSO.AttributePanelArrayMap` 构建菜单树
- `AttributeExamplePreviewItem` — 预览项逻辑不变
- 所有 Renderer 类 — 不依赖资产存储方式

---

## 4. 详细实现步骤

### 步骤 1：重构 `AttributeOverviewDatabaseSO` — PanelSO 子资产管理

**修改 `GetAllAttributePanels()` 方法：**

```csharp
static AbstractAttributePanelSO[] GetAllAttributePanels()
{
    var panelTypes = TypeCache.GetTypesDerivedFrom<AbstractAttributePanelSO>()
        .Where(t => !t.IsAbstract && !t.IsInterface).ToArray();

    // 从数据库资产中加载所有子资产
    var databasePath = AesirInspectorPaths.AttributeOverviewDatabasePath 
        + "/AttributeOverviewDatabase.asset";
    var allSubAssets = AssetDatabase.LoadAllAssetsAtPath(databasePath);
    
    var existingPanels = allSubAssets
        .OfType<AbstractAttributePanelSO>()
        .ToDictionary(a => a.GetType(), a => a);

    var list = new List<AbstractAttributePanelSO>();
    var needsSave = false;

    foreach (var type in panelTypes)
    {
        if (existingPanels.TryGetValue(type, out var asset))
        {
            list.Add(asset);
        }
        else
        {
            asset = (AbstractAttributePanelSO)CreateInstance(type);
            asset.name = type.Name;  // 子资产必须设置 name
            AssetDatabase.AddObjectToAsset(asset, databasePath);
            needsSave = true;
            list.Add(asset);
        }
    }

    if (needsSave)
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    return list.ToArray();
}
```

**关键变更点：**
- `AssetDatabase.CreateAsset` → `AssetDatabase.AddObjectToAsset`（嵌入为子资产而非独立文件）
- 使用 `AssetDatabase.LoadAllAssetsAtPath` 替代 `AssetDatabase.FindAssets` 来查找已有子资产
- 子资产创建后必须设置 `name` 属性

### 步骤 2：新增 `GetOrCreateExampleSubAsset<T>()` 方法

在 `AttributeOverviewDatabaseSO` 中新增：

```csharp
static readonly Dictionary<Type, ScriptableObject> _exampleCache = new();

public static T GetOrCreateExampleSubAsset<T>() where T : ScriptableObject, IAesirInspectorReset
{
    var type = typeof(T);
    
    // 1. 检查内存缓存
    if (_exampleCache.TryGetValue(type, out var cached) && cached != null)
        return (T)cached;

    // 2. 从数据库子资产中查找
    var databasePath = AesirInspectorPaths.AttributeOverviewDatabasePath 
        + "/AttributeOverviewDatabase.asset";
    var allSubAssets = AssetDatabase.LoadAllAssetsAtPath(databasePath);
    var existing = allSubAssets.FirstOrDefault(a => a is T);
    
    if (existing is T found)
    {
        _exampleCache[type] = found;
        return found;
    }

    // 3. 创建新子资产
    var instance = CreateInstance<T>();
    instance.name = type.Name;
    AssetDatabase.AddObjectToAsset(instance, databasePath);
    AssetDatabase.SaveAssets();
    _exampleCache[type] = instance;
    return instance;
}
```

**注意事项：**
- `_exampleCache` 是静态字典，Domain Reload 后清空（正常行为，下次访问会从磁盘重新加载）
- `LoadAllAssetsAtPath` 会返回主资产 + 所有子资产，通过类型过滤
- 创建后立即 `SaveAssets`，确保子资产持久化

### 步骤 3：重构 `AttributeExampleSO<T>.Instance`

```csharp
public static T Instance
{
    get
    {
        if (_asset) return _asset;
        _asset = AttributeOverviewDatabaseSO.GetOrCreateExampleSubAsset<T>();
        return _asset;
    }
}
```

**变更：** `GetSingletonAssetAndDeleteOther<T>(AttributeExamplesPath)` → `AttributeOverviewDatabaseSO.GetOrCreateExampleSubAsset<T>()`

### 步骤 4：重构 `OdinAttributeExampleSO<T>.Instance`

```csharp
public static T Instance
{
    get
    {
        if (_asset) return _asset;
        _asset = AttributeOverviewDatabaseSO.GetOrCreateExampleSubAsset<T>();
        return _asset;
    }
}
```

**变更：** 同上。

### 步骤 5：处理 `AttributeOverviewPanelSO<T>.Instance`

经搜索，`AttributeOverviewPanelSO<T>.Instance` 仅被 `OdinSyntaxHighlighterPanelSO.Instance`（MiniTools 模块）使用，不在 AttributeOverviewPro 范围内。

**建议：** 保留现有实现不变（MiniTools 的 PanelSO 仍使用独立资产），或后续统一处理。本次方案不涉及此改动。

### 步骤 6：迁移与清理逻辑

在 `AttributeOverviewDatabaseSO.Initialize()` 开头添加迁移逻辑：

```csharp
public void Initialize()
{
    MigrateOldAssets();  // 新增
    // ... 原有逻辑
}

static void MigrateOldAssets()
{
    var oldPanelPath = AesirInspectorPaths.AttributePanelsPath;
    var oldExamplePath = AesirInspectorPaths.AttributeExamplesPath;
    var deleted = false;

    // 删除旧的独立 PanelSO 资产
    if (Directory.Exists(oldPanelPath))
    {
        var oldPanels = AssetDatabase.FindAssets("t:AbstractAttributePanelSO", new[] { oldPanelPath });
        foreach (var guid in oldPanels)
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
            deleted = true;
        }
    }

    // 删除旧的独立 ExampleSO 资产
    if (Directory.Exists(oldExamplePath))
    {
        var oldGuids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { oldExamplePath });
        foreach (var guid in oldGuids)
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
            deleted = true;
        }
    }

    if (deleted)
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
```

### 步骤 7：清理孤立子资产

在 `Initialize()` 中，检测并移除代码中已不存在的 PanelSO/ExampleSO 类型的孤立子资产：

```csharp
static void RemoveOrphanSubAssets(AbstractAttributePanelSO[] validPanels)
{
    var databasePath = AesirInspectorPaths.AttributeOverviewDatabasePath 
        + "/AttributeOverviewDatabase.asset";
    var allSubAssets = AssetDatabase.LoadAllAssetsAtPath(databasePath);
    
    var validPanelTypes = validPanels.Select(p => p.GetType()).ToHashSet();
    
    foreach (var subAsset in allSubAssets)
    {
        // 跳过主资产
        if (subAsset is AttributeOverviewDatabaseSO) continue;
        
        // 如果是 PanelSO 但不在有效列表中，则移除
        if (subAsset is AbstractAttributePanelSO panel && !validPanelTypes.Contains(panel.GetType()))
        {
            AssetDatabase.RemoveObjectFromAsset(subAsset);
        }
        
        // ExampleSO 的孤立清理可选 — 可通过检查 [AesirExample] 标记的类来判断
    }
}
```

### 步骤 8：确保正确保存

在 `Initialize()` 末尾添加：

```csharp
EditorUtility.SetDirty(this);
AssetDatabase.SaveAssets();
AssetDatabase.Refresh();
```

### 步骤 9：更新 `AesirInspectorPaths`（可选）

`AttributePanelsPath` 和 `AttributeExamplesPath` 不再用于创建新资产，但保留用于迁移清理。可添加注释说明：

```csharp
[Summary("已弃用 — PanelSO 现作为数据库子资产存储。仅保留用于旧资产迁移清理")]
public const string AttributePanelsPath = ...;

[Summary("已弃用 — ExampleSO 现作为数据库子资产存储。仅保留用于旧资产迁移清理")]
public const string AttributeExamplesPath = ...;
```

---

## 5. 验证步骤

### 5.1 编译验证
- 修改后执行 `unity_workflow.compile_and_validate`，确保无编译错误

### 5.2 功能验证
1. 打开 `Tools → Aesir → Inspector → Attribute Overview Pro`
2. 确认所有分类和面板正常显示
3. 点击不同面板，确认：
   - 顶部说明控件（BilingualHeaderControl）正常
   - 使用提示表格正常
   - 特性参数表格正常
   - 示例预览区域正常
   - 代码预览正常
4. 切换示例（如果有多个），确认 `[InlineEditor]` 正常
5. 点击"重置案例"按钮，确认 `AesirInspectorReset()` 正常
6. 点击"Ping 脚本文件"按钮，确认源码定位正常

### 5.3 持久化验证
1. 修改某个 ExampleSO 的字段值
2. 触发 Domain Reload（修改任意 .cs 文件）
3. 重新打开窗口，确认修改后的值已保留

### 5.4 资产数量验证
1. 删除 `Assets/Editor Default Resources/Aesir Inspector/Attribute Overview Pro/` 目录
2. 重新打开窗口
3. 确认仅生成 1 个 `.asset` 文件（数据库）
4. 确认无 `Panels/` 和 `Attribute Examples/` 子目录
5. 确认数据库 `.asset` 文件包含所有子资产（选中后在 Inspector 可见）

### 5.5 迁移验证
1. 先用旧版本生成 ~194 个 `.asset` 文件
2. 应用新代码
3. 打开窗口
4. 确认旧文件被清理
5. 确认功能正常

---

## 6. 备选方案（如子资产方案遇到 Odin 序列化问题）

### 备选方案 A：纯内存实例 + HideAndDontSave

- 使用 `CreateInstance` 创建所有实例
- 设置 `hideFlags = HideFlags.HideAndDontSave`
- **零 .asset 文件**
- 缺点：不满足用户"持久化保存"需求；Domain Reload 后所有状态丢失
- 适用场景：如果用户接受每次 Domain Reload 后重置

### 备选方案 B：按分类拆分多个数据库资产

- 创建 12 个 `.asset` 文件（按 `AesirAttributeCategory` 分类）
- 每个分类的 PanelSO 和 ExampleSO 作为该分类资产的子资产
- 优点：单个文件更小，按分类懒加载
- 缺点：管理复杂度增加；12 > 1
- 适用场景：如果单文件子资产数量导致性能问题（极不可能）

### 备选方案 C：ScriptableSingleton + JSON 序列化

- PanelSO/ExampleSO 改为纯 C# 类（非 ScriptableObject）
- 数据序列化为 JSON 存储在 `ScriptableSingleton` 中
- 缺点：无法使用 `[InlineEditor]`、`OdinMenuTree` 等 Odin 功能；重构量巨大
- **不推荐**

---

## 7. 实现顺序与预估工作量

| 步骤 | 文件数 | 复杂度 | 说明 |
|------|--------|--------|------|
| 重构 DatabaseSO | 1 | 中 | 核心：子资产创建/加载/清理逻辑 |
| 重构 ExampleSO 基类 | 2 | 低 | `Instance` getter 一行改动 |
| 迁移清理逻辑 | 1 | 低 | 删除旧 .asset 文件 |
| 路径常量更新 | 1 | 低 | 注释标记弃用 |
| 测试验证 | — | 中 | 功能、持久化、迁移全量验证 |
| **合计** | **5 文件** | | |

---

## 8. 开放问题

1. **`AttributeOverviewPanelSO<T>.Instance` 是否需要统一改为子资产？** — 当前仅 MiniTools 使用，可后续处理。建议本次不涉及。

2. **是否需要在 `Initialize()` 中预创建所有 ExampleSO？** — 当前懒加载模式（`AttributeData` 构造时触发）可以保留。`GetOrCreateExampleSubAsset<T>()` 支持懒加载。

3. **子资产的 `hideFlags` 是否需要额外设置？** — 默认 `HideInHierarchy` 已足够。如果希望用户完全无法在 Project 窗口操作子资产，可考虑 `HideInHierarchy | HideInInspector`。

4. **是否需要提供"重建数据库"菜单项？** — 可在 `AttributeOverviewDatabaseSO` 上添加 `[Button]` 或 MenuItem，方便用户手动触发完全重建（删除所有子资产并重新创建）。
