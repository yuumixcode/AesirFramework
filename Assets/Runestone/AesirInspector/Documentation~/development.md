# Development Guide

> Aesir Inspector 开发者指南。面向 AI Agent 和贡献者的架构、约定与操作参考。

---

## Architecture

### System Context

```mermaid
graph TB
    Dev[开发者] --> AICode[AI Coding Agent]
    AICode --> AI[Aesir Inspector<br/>Agent Context Layer]
    Dev --> Editor[Tuanjie Editor]
    Editor --> PKG[Aesir Inspector Package]
    PKG --> Odin[Odin Inspector<br/>Optional Dependency]
    PKG --> Unity[Unity/Tuanjie API]
```

### Assembly Dependency

```mermaid
graph LR
    RT[Runestone.AesirInspector] -->|"无依赖"| None[ ]
    ED[Runestone.AesirInspector.Editor] -->|引用| RT
    OW[Runestone.AesirInspector.OdinIntegration] -->|引用| RT
    OWE[...OdinIntegration.Editor] -->|引用| RT
    OWE -->|引用| ED
    OWE -->|引用| OW
    OW -.->|ODIN_INSPECTOR| Odin
    OWE -.->|ODIN_INSPECTOR| Odin
```

### OdinBridge Data Flow

```mermaid
sequenceDiagram
    participant Code as 核心代码
    participant Locator as OdinBridgeLocator
    participant Default as DefaultOdinBridge
    participant OdinBridge as OdinInspectorBridge

    Code->>Locator: OdinBridgeLocator.Bridge
    Locator->>Locator: 搜索 IOdinBridge 实现

    alt Odin 已安装
        Locator->>OdinBridge: 返回 OdinInspectorBridge 实例
        OdinBridge-->>Code: IsOdinInstalled = true
    else Odin 未安装
        Locator->>Default: 返回 DefaultOdinBridge 实例
        Default-->>Code: IsOdinInstalled = false
    end
```

### Bilingual System Data Flow

```mermaid
sequenceDiagram
    participant SO as LanguageSettingsSO
    participant Data as BilingualData
    participant Attr as 双语 Attribute
    participant Drawer as 双语 Drawer
    participant UI as Inspector UI

    SO->>Data: 当前语言 (zh/en)
    Attr->>Data: 读取标签文本
    Attr->>Drawer: 特性参数
    Drawer->>UI: 渲染当前语言标签
    SO->>Drawer: OnLanguageChanged 事件
    Drawer->>UI: 重绘 UI
```

---

## Conventions

### Naming

| 标识符 | 规则 | 示例 |
|---|---|---|
| 类、接口 | `PascalCase`，接口 `I` 前缀 | `PlayerManager`, `IDamageable` |
| 私有非序列化字段 | `_camelCase` | `_health` |
| 序列化字段 `[SerializeField]` | `camelCase` | `moveSpeed` |
| 常量 / 静态只读 | `PascalCase` | `MaxScore` |

### Enum

- 普通：含 `None = 0`，显式赋值。
- Flags：`[Flags]`，值为 `1 << n`，复合用 `|`。

### Unity 禁忌

- **严禁**对 `UnityEngine.Object` 派生类使用 `?.` / `??`
- **严禁**在 `Update` 中调用 `GetComponent`、`Find`、字符串拼接、LINQ

### Events

| 角色 | 命名 | 示例 |
|---|---|---|
| 事件 | 无 `On` 前缀 | `DoorOpened` |
| 订阅方法 | `On` + 事件名 | `OnDoorOpened` |
| 触发方法 | `Raise` + 事件名 | `RaiseDoorOpened` |

### Comments

本项目采用**自文档化代码**和**无注释范式**：

- **禁止 XML 注释**：不使用 `/// <summary>`、`/// <param>` 等 XML 文档注释
- **类必须使用 `[Summary]`**：所有类（class / struct / interface）必须具备 `[Summary("...")]`，解释"为什么"
- **其他成员**：命名即文档，仅复杂逻辑使用 `[Summary]`，解释"为什么"而非"做了什么"

#### 免除规范的模块

以下模块为展示/示例用途，不适用通用注释规范，使用 `//` 单行/多行注释进行特殊性补充即可：

- `Runtime/Unity/CodeStyle/` — 代码风格示例文件
- `Editor/Odin Integration/AttributeOverviewPro/Data/` — 属性数据类
- `Editor/Odin Integration/AttributeOverviewPro/AttributePanels/` — Panel SO 定义
- `Editor/Odin Integration/AttributeOverviewPro/UsageExamples/` — 示例 SO

### Methods

- `Internal_` 前缀：私有/保护/内部方法与公开方法重名时使用

### Odin Inspector 规范

- Odin 依赖代码**必须**放在 `Odin Integration/` 子目录
- Processor：`internal sealed`，与目标类**同文件**定义，无需 XML / `[Summary]`
- Processor 需 `nameof` 引用私有成员时，定义为**嵌套类**（仍 `internal`）
- Drawer：继承 `OdinAttributeDrawer`，独立存于 `Drawers/` 目录

### Utility Naming

| 类别 | 命名规则 | 目录 | 示例 |
|---|---|---|---|
| Runtime | `XxxUtility` | `Runtime/Unity/Utilities/` | `PathUtility` |
| Editor 安全封装 | `XxxSafeEditorUtility` | `Runtime/Unity/Utilities/` | `HierarchySafeEditorUtility` |
| Editor-Only | `XxxEditorUtility` | `Editor/Unity/` | `PackageManagerEditorUtility` |

### SafeEditorUtility Pattern

```csharp
public static class XxxSafeEditorUtility
{
    // void 方法：构建剔除
    [Conditional("UNITY_EDITOR")]
    public static void PingObject(Object target) { /* Editor 实现 */ }

    // 有返回值方法：双实现
    public static bool TryGetAssetPath(Object target, out string path)
    {
#if UNITY_EDITOR
        path = AssetDatabase.GetAssetPath(target);
        return !string.IsNullOrEmpty(path);
#else
        path = default;
        return false;
#endif
    }
}
```

### Event Subscription

```csharp
// 先 - 再 +，防止重复
AesirInspectorLanguageSettingsSO.OnLanguageChanged -= Internal_OnLanguageChanged;
AesirInspectorLanguageSettingsSO.OnLanguageChanged += Internal_OnLanguageChanged;

// OnDestroy 中取消
AesirInspectorLanguageSettingsSO.OnLanguageChanged -= Internal_OnLanguageChanged;
```

---

## Modules

### Runtime/Unity/ — Core Runtime

| Component | Directory | Key Types |
|-----------|-----------|-----------|
| Attributes | `Attributes/` | `SummaryAttribute` |
| Core | `Core/` | `AesirInspectorVersion`, `AesirInspectorPaths`, `AesirInspectorWebLinks`, `IAesirInspectorReset` |
| Inspector | `Inspector/` | `BilingualDisplayAsStringControl`, `BilingualHeaderControl`, `HorizontalSeparateControl` |
| Localization | `Localization/` | `BilingualData`, `AesirInspectorLanguageSettingsSO` |
| Logging | `Logging/` | `AesirInspectorLogger`, `AesirInspectorLoggerSettings` |
| OdinBridge | `OdinBridge/` | `IOdinBridge`, `DefaultOdinBridge`, `OdinBridgeLocator` |
| ScriptDocGenerator | `ScriptDocGenerator/` | `ITypeData`, `MemberData`, `FieldData`, `PropertyData`, `MethodData`, `ConstructorData`, `EventData`, `ParameterData`, `TypeData` |
| Utilities | `Utilities/` | `ScriptableObjectSafeEditorUtility`, `MonoScriptSafeEditorUtility`, `PathUtility`, `PathSafeEditorUtility`, `HierarchyUtility`, `HierarchySafeEditorUtility`, `ProjectSafeEditorUtility`, `UrlUtility`, `ReflectionUtility`, `PredefinedAssemblyUtility`, `PlayerLoopUtility`, `RegexUtility` |

### Editor/Unity/ — Core Editor

| Component | Directory | Key Types |
|-----------|-----------|-----------|
| Core | `Core/` | `AesirInspectorInstallationChecker`, `AesirInspectorMenuItems` |
| MiniTools | `MiniTools/` | `QuickCreateSOMenuItem` |
| SummaryTool | `SummaryTool/` | `XmlSummaryTool`, `XmlCodePart`, `SummaryToolMenuItems` |

### Runtime/Odin Integration/ — Odin Runtime

| Component | Directory | Key Types |
|-----------|-----------|-----------|
| Attributes | `Attributes/` | `BilingualTitleAttribute`, `BilingualButtonAttribute` 等 4 个双语特性 |
| OdinCodeHighlighter | `OdinCodeHighlighter.cs` | 语法高亮器运行时数据 |

### Editor/Odin Integration/ — Odin Editor

| Component | Directory | Key Types |
|-----------|-----------|-----------|
| AttributeOverviewPro | `AttributeOverviewPro/` | Data-Panel-Example 三件套架构 |
| AttributeProcessors | `AttributeProcessors/` | OdinAttributeProcessor 实现 |
| Bridge | `Bridge/` | `OdinInspectorBridge` |
| Drawers | `Drawers/` | 4 个双语 Drawer |
| ExtensionManager | `ExtensionManager/` | 一键安装/移除推荐包 |
| MiniTools | `MiniTools/` | MenuItem Viewer, Syntax Highlighter |
| ScriptDocGenerator | `ScriptDocGenerator/` | 文档生成器编辑器逻辑 |
| Windows | `Windows/` | Getting Started, Preferences |

---

## Architecture Decisions

### ADR-001: OdinBridge Separation

核心运行时通过 `IOdinBridge` 接口查询 Odin 可用性，不直接引用 Odin 类型。无 Odin 时 `DefaultOdinBridge` 提供默认实现，有 Odin 时 `OdinInspectorBridge` 提供增强实现。

<details>
<summary>Consequences</summary>

**优点**：
- 核心程序集零 Odin 依赖，可在无 Odin 环境下独立运行和分发
- Odin 集成通过编译约束独立启用/禁用，无需条件编译指令散落各处
- 新的编辑器增强只需实现 `IOdinBridge`，不影响核心稳定性

**缺点**：
- 新增 Odin 特性时需同时修改 Bridge 接口和两个实现类，维护成本增加
- 通过接口间接调用存在微小的性能开销（Inspector 场景下可忽略）
- Bridge 接口变更会同时影响两个实现，需注意向后兼容

</details>

### ADR-002: Bilingual Attribute + Drawer Separation

Attribute 只承载数据，Drawer 负责渲染逻辑，Processor 负责动态注入。新增双语特性只需创建 Attribute + Drawer（+ 可选 Processor）。

<details>
<summary>Consequences</summary>

**优点**：
- 关注点分离：数据、渲染、注入各司其职，单一职责明确
- 扩展性强：新增双语特性只需 2-3 个文件，无需修改已有代码
- Drawer 可独立测试渲染逻辑，不依赖 Odin Attribute 的数据定义

**缺点**：
- Attribute 和 Drawer 之间通过泛型参数 `TAttribute` 耦合，重命名 Attribute 需同步修改 Drawer
- Processor 与被处理类同文件定义，文件可能变大（但保证了查找便利性）
- 双语文本需在 Attribute 中同时维护中英两套，数据冗余

</details>

### ADR-003: Core/Integration Assembly Separation

核心程序集（`Runtime/Unity/`、`Editor/Unity/`）零 Odin 依赖；Odin Integration 程序集通过 `ODIN_INSPECTOR` 编译约束自动启用/禁用。

<details>
<summary>Consequences</summary>

**优点**：
- 核心程序集可在任何 Unity/Tuanjie 项目中使用，无需 Odin 授权
- `ODIN_INSPECTOR` 编译约束确保 Odin 代码不会在无 Odin 环境下编译报错
- 程序集边界清晰，依赖方向单向（Integration → Core），避免循环引用

**缺点**：
- 核心功能若需 Odin 增强，必须通过 Bridge 模式间接调用，增加间接层
- 4 个程序集增加了项目结构和 asmdef 文件的复杂度
- 新增跨程序集的功能时，需考虑代码应放在 Core 还是 Integration

</details>

### ADR-004: SafeEditorUtility Pattern

Runtime 工具类使用 `XxxSafeEditorUtility` 模式：`void` 方法加 `[Conditional("UNITY_EDITOR")]`，有返回值方法用 `#if UNITY_EDITOR` 双实现。构建时自动剔除，零运行时开销。

<details>
<summary>Consequences</summary>

**优点**：
- Runtime 工具类可安全引用 Editor API，构建时自动剔除，零运行时开销
- `[Conditional]` 标记的 `void` 方法调用在非 Editor 构建中完全移除，包括参数评估
- 双实现模式（`#if UNITY_EDITOR`）确保有返回值的方法在构建时提供安全的默认值

**缺点**：
- 同一功能需维护两份实现（Editor 实现 + 构建时的默认值），代码重复
- `[Conditional]` 方法中的副作用在构建时会被静默移除，可能隐藏逻辑错误
- 命名约定需严格遵守（`XxxSafeEditorUtility`），违反约定可能导致构建失败

</details>

---

## Task Guides

### Add Bilingual Attribute

1. **Attribute**: `Runtime/Odin Integration/Attributes/Bilingual{Name}Attribute.cs` — 命名 `Bilingual{OdinOriginalName}Attribute`，必须 `[DontApplyToListElements]`，公共类必须 `[Summary]`，禁止 XML 注释
2. **Drawer**: `Editor/Odin Integration/Drawers/Bilingual{Name}AttributeDrawer.cs` — 继承 `OdinAttributeDrawer<TAttribute>`，读取 `AesirInspectorLanguageSettingsSO.CurrentLanguage`，无需 XML / `[Summary]`
3. **Processor** (可选): 与被处理类同文件，`internal sealed`，无需 XML / `[Summary]`
4. **AttributeOverviewPro** (可选): 创建 Data-Panel-Example 三件套

### Add Inspector Control

1. **文件**: `Runtime/Unity/Inspector/{Name}Control.cs`
2. **命名**: `{Purpose}Control`
3. **双语**: 使用 `BilingualData` + `AesirInspectorLanguageSettingsSO.CurrentLanguage`
4. **Editor 功能**: 通过 `SafeEditorUtility` 模式桥接

### Add Odin Drawer

1. **文件**: `Editor/Odin Integration/Drawers/{Name}Drawer.cs`
2. **继承**: `OdinAttributeDrawer<TAttribute>`
3. **双语**: 通过 `AesirInspectorLanguageSettingsSO` 获取当前语言，订阅 `OnLanguageChanged` 事件
4. **无需** XML / `[Summary]`

### Add OdinAttributeProcessor

1. **位置**: 与被处理类**同文件**，`internal sealed`
2. **无需** XML / `[Summary]`
3. **需 `nameof` 引用私有成员**: 定义为**嵌套类**（仍 `internal`）
4. **隐藏条件属性命名**: 集合为空 `{FieldName}IsEmpty`，对象为空 `{Target}IsNull`

### Add Utility

1. **Runtime 工具**: `XxxUtility` → `Runtime/Unity/Utilities/`
2. **Editor 安全封装**: `XxxSafeEditorUtility` → `Runtime/Unity/Utilities/`
3. **Editor-Only**: `XxxEditorUtility` → `Editor/Unity/`
4. **必须** `public static class`，私有方法加 `Internal_` 前缀
5. 日志使用 `AesirInspectorLogger`，Odin 操作通过 `OdinInspectorSafeEditorUtility` 桥接
