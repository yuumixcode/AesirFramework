# Aesir Inspector 目录结构优化计划书

> 创建日期：2026-08-04
> 状态：草案（未实施）

---

## 一、背景与动机

### 1.1 历史设计

Aesir Inspector 原设计为「Odin Inspector 可选依赖」——通过 `OdinBridge` 桥接模式（`IOdinBridge` / `DefaultOdinBridge` / `OdinBridgeLocator` / `OdinInspectorBridge`）在无 Odin 环境下降级运行。代码优先放入 Unity 层（不依赖 Odin），仅在确实需要 Odin 功能时才放入 OdinIntegration 层。

### 1.2 观念转变

- **Odin Inspector 已改为强依赖**（package.json 已声明，参考 Project Memory 2026-07-25）
- 在 AI 时代，其他开发者不需要「复制粘贴」代码——AI 可以完成移植
- 为「无 Odin 用户」保留的大量 Unity 层代码是**伪需求**，反而增加了维护负担和架构复杂度
- OdinBridge 桥接模式是无 Odin 时代的产物，现在应整体移除

### 1.3 目标

- **Odin Integration 层为默认主体**，承载绝大多数功能代码
- **Runtime/Unity 层极简化**：仅保留一个自包含的类型反射解析器（`TypeReflection` 模块），包括自定义注释特性
- **按功能模块组织**，每个模块自包含，减少跨层碎片化
- 消除 OdinBridge 抽象层

---

## 二、当前结构分析

### 2.1 程序集与文件分布

| 程序集 | Odin 依赖 | 当前文件数 | 定位 |
|--------|-----------|-----------|------|
| `Runestone.AesirInspector` (Runtime/Unity) | 无 | ~27 | 过大——混入了大量 Odin 专属逻辑和通用工具 |
| `Runestone.AesirInspector.OdinIntegration` (Runtime/OdinIntegration) | 有 | ~6 | 过小 |
| `Runestone.AesirInspector.Editor` (Editor/Unity) | 无 | ~6 | 合理偏大 |
| `Runestone.AesirInspector.OdinIntegration.Editor` (Editor/OdinIntegration) | 有 | ~90+ | 主体 |

### 2.2 当前目录树（简化）

```
Runtime/
├── Unity/                          ← Odin 无关运行时（当前过大）
│   ├── Attributes/                 ← 空文件夹
│   ├── CodeStyle/
│   ├── Core/                       ← 版本、路径、设置
│   ├── Inspector/                  ← Bilingual 控件（实际仅 Odin 场景使用）
│   ├── Localization/               ← 双语数据
│   ├── Logging/
│   ├── OdinBridge/                 ← 桥接模式（应移除）
│   ├── ScriptDocGenerator/         ← 分析数据模型+核心（部分调用 OdinBridge）
│   ├── TypeReflectionAnalyzer/Attributes/  ← Summary、ReferenceLinkURL
│   └── Utilities/
└── OdinIntegration/                ← Odin 运行时（当前过小）
    ├── Attributes/                 ← 双语特性（5 个）
    └── Utilities/

Editor/
├── Unity/                          ← Odin 无关编辑器
│   ├── Core/                       ← 安装检查、菜单常量
│   ├── MiniTools/                 ← QuickCreateSO
│   └── SummaryTool/                ← XML Summary 工具
└── OdinIntegration/                ← Odin 编辑器（主体）
    ├── AttributeOverviewPro/       ← 特性总览（~200 文件）
    ├── AttributeProcessors/
    ├── Bridge/                     ← OdinInspectorBridge（应移除）
    ├── Drawers/
    ├── ExtensionManager/
    ├── MiniTools/
    ├── ScriptDocGenerator/
    ├── Windows/
    └── AesirInspectorModuleAssetMarkerSO.cs
```

### 2.3 关键问题

1. **OdinBridge 滥用**：`ScriptDocGenerator` 核心分析代码（`TypeData`、`TypeAnalyzerStaticExtensions`）和 `AesirInspectorSettings<T>` 在 Unity 层调用 `OdinBridgeLocator.Bridge` 获取 Odin 的类型名称格式化功能。这导致 Unity 层代码实际依赖 Odin 功能，桥接模式只是遮掩了这一事实。

2. **Inspector 控件错位**：`BilingualDisplayAsStringControl`、`BilingualHeaderControl`、`HorizontalSeparateControl` 放在 Runtime/Unity/Inspector/，但它们仅由 Odin AttributeProcessor 渲染，离开 Odin 毫无意义。

3. **模块碎片化**：「双语」功能分散在 4 个位置：Runtime/Unity/Inspector（控件）、Runtime/OdinIntegration/Attributes（特性）、Editor/OdinIntegration/AttributeProcessors（处理器）、Editor/OdinIntegration/Drawers（Drawer）。

4. **空文件夹**：`Runtime/Unity/Attributes/` 为空，实际特性在 `TypeReflectionAnalyzer/Attributes/`。

5. **Unity 层过载**：Runtime/Unity 包含了 Core 常量、Localization、Logging、Utilities（12 个文件）、CodeStyle 等大量与类型反射无关的代码。这些代码在 Odin 强依赖的前提下没有理由保持 Odin 无关。

---

## 三、设计原则

| 原则 | 说明 |
|------|------|
| **Odin 默认** | 新代码默认放入 OdinIntegration 层。仅当有充分理由时才放入 Unity 层 |
| **Runtime/Unity 极简化** | Runtime/Unity 仅保留一个自包含的类型反射解析器（`TypeReflection` 模块），包括 `[Summary]`、`[ReferenceLinkURL]` 等自定义注释特性。该模块是纯反射+数据模型，不依赖 Odin，有独立价值 |
| **模块自包含** | 每个功能模块（Bilingual、ScriptDocGenerator、AttributeOverviewPro 等）在各自层内拥有独立文件夹，内部完整组织 Attributes/Drawers/Processors 等子结构 |
| **消除伪抽象** | OdinBridge 桥接模式整体移除。TypeReflection 模块内的 OdinBridge 调用替换为自定义实现；Editor 层的 OdinBridge 调用直接使用 Sirenix API |
| **命名一致** | 统一使用 `OdinIntegration`（无空格），与 asmdef 命名一致 |

---

## 四、新目录结构设计

### 4.1 总览

```
AesirInspector/
├── Runtime/
│   ├── Unity/                              [Runestone.AesirInspector]
│   │   └── TypeReflection/                 — 类型反射解析器（唯一模块）
│   │       ├── Attributes/                 — 自定义注释特性
│   │       ├── Analysis/                   — 分析数据模型
│   │       └── Core/                       — 核心分析器
│   │
│   └── OdinIntegration/                   [Runestone.AesirInspector.OdinIntegration]
│       ├── Core/                           — 版本、路径、链接、设置
│       ├── Bilingual/                      — 双语特性与控件
│       │   ├── Attributes/
│       │   ├── Controls/
│       │   └── Localization/               — 双语数据层
│       ├── Logging/                        — 日志
│       ├── Utilities/                      — 通用工具
│       └── CodeStyle/                      — 代码风格指南
│
├── Editor/
│   ├── Unity/                              [Runestone.AesirInspector.Editor]
│   │   ├── Core/                           — 安装检查、菜单常量
│   │   ├── SummaryTool/                    — XML Summary 工具
│   │   └── QuickCreateSO/                 — 快速创建 SO
│   │
│   └── OdinIntegration/                   [Runestone.AesirInspector.OdinIntegration.Editor]
│       ├── Core/                           — 模块标记
│       ├── Bilingual/                      — 双语 Drawer 与 Processor
│       │   ├── Drawers/
│       │   └── Processors/
│       ├── AttributeOverviewPro/           — 特性总览
│       │   ├── Abstract/
│       │   ├── Core/
│       │   ├── Data/
│       │   ├── AttributePanels/
│       │   └── UsageExamples/
│       ├── ScriptDocGenerator/             — 文档生成器（编辑器层）
│       │   ├── Editor/
│       │   │   ├── Panels/
│       │   │   ├── Windows/
│       │   │   ├── Controllers/
│       │   │   └── Settings/
│       │   └── Menus/
│       ├── ExtensionManager/               — 扩展包管理器
│       ├── MiniTools/                      — 迷你工具
│       │   ├── MenuItemViewer/
│       │   └── SyntaxHighlighter/
│       └── Windows/                        — 全局窗口
│           ├── GettingStarted/
│           └── Preferences/
│
├── Tests/
│   ├── Editor/                             [Runestone.AesirInspector.Editor.Tests]
│   │   └── ScriptDocGenerator/
│   └── Runtime/                            [Runestone.AesirInspector.Tests]
│
├── Samples~/
├── Documentation~/
└── package.json
```

### 4.2 各模块详细设计

#### Runtime/Unity/ — 类型反射解析器（唯一模块）

```
Runtime/Unity/
├── TypeReflection/
│   ├── Attributes/
│   │   ├── SummaryAttribute.cs                 — [Summary] 特性（ScriptDocGenerator 反射用）
│   │   └── ReferenceLinkURLAttribute.cs         — [ReferenceLinkURL] 特性
│   │
│   ├── Analysis/                                ← 从 ScriptDocGenerator/AnalysisData/ 迁入并重命名
│   │   ├── MemberData.cs                        — 成员分析数据基类
│   │   ├── IDerivedMemberData.cs                — 派生成员数据接口
│   │   ├── ConstructorData.cs                  — 构造函数分析数据
│   │   ├── FieldData.cs                         — 字段分析数据
│   │   ├── MethodData.cs                        — 方法分析数据
│   │   ├── PropertyData.cs                     — 属性分析数据
│   │   ├── ParameterData.cs                    — 参数分析数据
│   │   ├── EventData.cs                        — 事件分析数据
│   │   └── TypeData.cs                         — 类型分析数据
│   │
│   ├── Core/                                    ← 从 ScriptDocGenerator/Core/ 迁入
│   │   ├── AccessModifierType.cs               — 访问修饰符枚举
│   │   ├── TypeCategory.cs                     — 类型分类枚举
│   │   ├── DefaultAnalysisDataFactory.cs       — 默认分析数据工厂
│   │   ├── DefaultAttributeFilter.cs           — 默认特性过滤器
│   │   ├── DerivedMemberDataComparer.cs        — 派生成员比较器
│   │   ├── TypeAnalyzerUtility.cs              — 类型分析工具
│   │   └── TypeAnalyzerStaticExtensions.cs     — 类型分析扩展方法（含 GetReadableTypeName）
│   │
│   ├── ReflectionUtility.cs                    ← 从 Utilities/ 迁入（仅被 TypeReflection 使用）
│   │
│   └── TypeNameFormatter.cs                   ← 新增：替换 OdinBridge 的类型名称格式化
│
└── Runestone.AesirirInspector.asmdef
```

**保留理由**：TypeReflection 是一个自包含的类型反射分析引擎——通过 `System.Reflection` 分析类型的成员（字段、属性、方法、事件、构造函数）并生成结构化数据模型。它不依赖 Odin，有独立的复用价值（例如可用于其他文档生成工具或代码分析场景）。

**自包含性**：
- `ReflectionUtility` 是 TypeReflection 的唯一外部依赖，仅被 `TypeData`、`MethodData`、`TypeAnalyzerStaticExtensions` 使用（共 3 个文件，9 处调用），因此一并迁入 TypeReflection 模块
- `TypeNameFormatter` 替换原有的 OdinBridge 调用，提供类型名称格式化能力（如 `List<int>` 而非 `List`1[[System.Int32]]`）
- 不依赖 Core 常量、Localization、Logging、其他 Utilities

#### Runtime/OdinIntegration/ — Odin 运行时（扩展后）

```
Runtime/OdinIntegration/
├── Core/                                       ← 从 Runtime/Unity/Core/ 迁入
│   ├── AesirInspectorVersion.cs               — 版本常量
│   ├── AesirInspectorPaths.cs                 — 路径常量
│   ├── AesirInspectorWebLinks.cs              — URL 常量
│   ├── AesirInspectorSettings.cs             — 编辑器设置基类 ★需重构（见 §五）
│   ├── AesirInspectorProjectSettingsSO.cs    — 项目设置 SO
│   └── IAesirInspectorReset.cs               — 重置接口
│
├── Bilingual/
│   ├── Attributes/                             ← 从 Runtime/OdinIntegration/Attributes/ 重组
│   │   ├── BilingualTitleAttribute.cs
│   │   ├── BilingualButtonAttribute.cs
│   │   ├── BilingualInfoBoxAttribute.cs
│   │   ├── BilingualTextAttribute.cs
│   │   └── DetailInfoBoxAttribute.cs
│   ├── Controls/                               ← 从 Runtime/Unity/Inspector/ 迁入
│   │   ├── BilingualDisplayAsStringControl.cs
│   │   ├── BilingualHeaderControl.cs
│   │   └── HorizontalSeparateControl.cs
│   └── Localization/                           ← 从 Runtime/Unity/Localization/ 迁入
│       ├── BilingualData.cs
│       ├── AesirInspectorLanguageSettingsSO.cs
│       ├── ILanguageProvider.cs
│       └── LanguageProviderLocator.cs
│
├── Logging/                                   ← 从 Runtime/Unity/Logging/ 迁入
│   ├── AesirInspectorLogger.cs
│   └── AesirInspectorLoggerSettings.cs
│
├── Utilities/                                  ← 从 Runtime/Unity/Utilities/ 迁入（排除 ReflectionUtility）
│   ├── PathUtility.cs
│   ├── UrlUtility.cs
│   ├── RegexUtility.cs
│   ├── HierarchyUtility.cs
│   ├── PlayerLoopUtility.cs
│   ├── PredefinedAssemblyUtility.cs
│   ├── HierarchySafeEditorUtility.cs
│   ├── MonoScriptSafeEditorUtility.cs
│   ├── PathSafeEditorUtility.cs
│   ├── ProjectSafeEditorUtility.cs
│   ├── ScriptableObjectSafeEditorUtility.cs
│   └── OdinCodeHighlighter.cs                 ← 原已在 OdinIntegration
│
├── CodeStyle/                                  ← 从 Runtime/Unity/CodeStyle/ 迁入
│   └── AesirInspectorCodeStyle.cs
│
└── Runestone.AesirInspector.OdinIntegration.asmdef
```

**变更说明**：
- Inspector 控件从 `Runtime/Unity/Inspector/` 迁入 `Bilingual/Controls/`
- 双语特性从 `Runtime/OdinIntegration/Attributes/` 重组为 `Bilingual/Attributes/`
- Localization 从 `Runtime/Unity/Localization/` 迁入 `Bilingual/Localization/`（与 Bilingual 模块合并）
- Core、Logging、Utilities（排除 ReflectionUtility）、CodeStyle 全部从 Runtime/Unity 迁入
- `AesirInspectorSettings.cs` 的 OdinBridge 调用替换为直接 Sirenix API（见 §五）

#### Editor/Unity/ — Odin 无关编辑器（最小化）

```
Editor/Unity/
├── Core/
│   ├── AesirInspectorInstallationChecker.cs  — 安装方式检测（UPM/Asset）
│   ├── AesirInspectorMenuItems.cs            — 菜单路径常量（全局共享）
│   ├── AesirInspectorProjectSettingsSO.cs    — 项目设置 SO
│   └── IAesirInspectorReset.cs               — 重置接口
│
├── SummaryTool/
│   ├── XmlSummaryTool.cs
│   ├── XmlCodePart.cs
│   └── SummaryToolMenuItems.cs
│
├── QuickCreateSO/
│   └── QuickCreateSOMenuItem.cs
│
└── Runestone.AesirInspector.Editor.asmdef
```

**变更说明**：
- `MiniTools/` 重命名为 `QuickCreateSO/`（更具描述性）
- `AesirInspectorSettings.cs` 迁移至 Runtime/OdinIntegration/Core/（因为使用了 OdinBridge → Sirenix API）
- `AesirInspectorProjectSettingsSO.cs` 和 `IAesirInspectorReset.cs` 也随之迁移至 Runtime/OdinIntegration/Core/
- Editor/Unity 需新增对 `Runestone.AesirInspector.OdinIntegration` 的引用（见 §七）

> **注**：`QuickCreateSOMenuItem.cs` 使用 `AesirInspectorLogger`（迁至 OdinIntegration），`XmlSummaryTool.cs` 使用 `SummaryAttribute`（留在 TypeReflection）。Editor/Unity 需同时引用两个 Runtime 程序集。

#### Editor/OdinIntegration/ — Odin 编辑器（主体）

```
Editor/OdinIntegration/
├── Core/
│   └── AesirInspectorModuleAssetMarkerSO.cs
│
├── Bilingual/
│   ├── Drawers/
│   │   ├── BilingualAttributeDrawer.cs          ← 从 Drawers/ 迁入
│   │   ├── BilingualButtonAttributeDrawer.cs
│   │   ├── BilingualInfoBoxAttributeDrawer.cs
│   │   ├── BilingualTextAttributeDrawer.cs
│   │   ├── BilingualTitleAttributeDrawer.cs
│   │   └── DetailInfoBoxAttributeDrawer.cs
│   └── Processors/
│       ├── BilingualDisplayAsStringProcessor.cs  ← 从 AttributeProcessors/ 迁入
│       ├── BilingualHeaderProcessor.cs
│       ├── HorizontalSeparateProcessor.cs
│       ├── AesirInspectorLanguageSettingsProcessor.cs
│       ├── AesirInspectorResetProcessor.cs
│       └── OdinAutoTooltipAttributeProcessor.cs
│
├── AttributeOverviewPro/
│   ├── Abstract/                               ← 不变
│   ├── Core/                                   ← 不变
│   ├── Data/                                   ← 不变
│   ├── AttributePanels/                        ← 不变
│   └── UsageExamples/                          ← 不变
│
├── ScriptDocGenerator/                         — 仅编辑器层代码
│   ├── Editor/
│   │   ├── Panels/
│   │   ├── Windows/
│   │   ├── Controllers/
│   │   └── Settings/
│   └── Menus/
│
├── ExtensionManager/
│   ├── ExtensionPackageCard.cs
│   ├── ExtensionPackageManagerWindow.cs
│   └── PackageManagerEditorUtility.cs
│
├── MiniTools/
│   ├── AesirInspectorMiniToolsWindow.cs
│   ├── MenuItemViewer/
│   └── SyntaxHighlighter/
│
├── Windows/
│   ├── GettingStarted/
│   │   └── AesirInspectorGettingStartedWindow.cs
│   └── Preferences/
│       └── AesirInspectorPreferencesWindow.cs
│
└── Runestone.AesirInspector.OdinIntegration.Editor.asmdef
```

**变更说明**：
- `Drawers/` 和 `AttributeProcessors/` 合并为 `Bilingual/` 模块
- `Bridge/` 整体移除（OdinBridge 模式消除）
- ScriptDocGenerator 的分析数据模型和核心分析器**保留在 Runtime/Unity/TypeReflection/**，Editor 层仅包含窗口、面板、控制器等编辑器代码
- `Windows/` 细分子文件夹

---

## 五、OdinBridge 模式消除

### 5.1 当前 OdinBridge 使用情况

| 调用方 | 当前位置 | 调用方法 | 用途 | 消除方式 |
|--------|---------|---------|------|---------|
| `TypeData` | Runtime/Unity/ScriptDocGenerator | `GetGenericConstraintsString` | 泛型约束字符串 | 自定义实现（TypeNameFormatter） |
| `TypeAnalyzerStaticExtensions` | Runtime/Unity/ScriptDocGenerator | `GetFriendlyName`, `GetFriendlyFullName` | 类型名称格式化 | 自定义实现（TypeNameFormatter） |
| `AesirInspectorSettings<T>` | Runtime/Unity/Core | `GetFriendlyFullName` | 配置名生成 | 直接使用 `type.GetNiceFullName()`（迁至 OdinIntegration 后） |
| `ScriptDocGeneratorSO` | Editor/OdinIntegration | `GetFriendlyFullName` | 显示名称 | 直接使用 `type.GetNiceFullName()` |
| `CnScriptingAPISettingsSO` | Editor/OdinIntegration | `GetFriendlyFullName` | 显示名称 | 直接使用 `type.GetNiceFullName()` |
| `MenuItemViewerSO` | Editor/OdinIntegration | `GetFriendlyFullName` | 显示名称 | 直接使用 `type.GetNiceFullName()` |
| `OdinSyntaxHighlighterPanelSO` | Editor/OdinIntegration | `GetFriendlyFullName` | 显示名称 | 直接使用 `type.GetNiceFullName()` |
| `PluginConfigRuntimeOnEditorSample` | Samples~ | `GetFriendlyFullName` | 显示名称 | 直接使用 `type.GetNiceFullName()` |

### 5.2 消除方案

OdinBridge 调用方分为两类，消除方式不同：

#### A. Runtime/Unity 层（TypeReflection 模块）— 自定义实现

TypeReflection 模块保留在 Runtime/Unity（Odin 无关），不能直接使用 Sirenix API。需要新增 `TypeNameFormatter.cs` 提供等价功能：

| 原调用 | 替换为 |
|--------|--------|
| `OdinBridgeLocator.Bridge.GetFriendlyName(type)` | `TypeNameFormatter.GetFriendlyName(type)` |
| `OdinBridgeLocator.Bridge.GetFriendlyFullName(type)` | `TypeNameFormatter.GetFriendlyFullName(type)` |
| `OdinBridgeLocator.Bridge.GetGenericConstraintsString(type, true)` | `TypeNameFormatter.GetGenericConstraintsString(type, true)` |

`TypeNameFormatter` 需实现以下功能（均为 `System.Reflection` 字符串格式化）：
- `GetFriendlyName(Type)` — 格式化泛型类型名（如 `List<int>` 而非 `List`1[[System.Int32]]`）
- `GetFriendlyFullName(Type)` — 同上，带命名空间
- `GetGenericConstraintsString(Type, bool)` — 格式化泛型约束（如 `where T : class, new()`）

> **注**：`TypeAnalyzerStaticExtensions.GetReadableTypeName` 扩展方法内部调用 OdinBridge，修改后改为调用 `TypeNameFormatter`。所有 `*Data.cs` 文件通过 `GetReadableTypeName()` 间接使用，无需逐一修改。

#### B. OdinIntegration 层（Runtime + Editor）— 直接使用 Sirenix API

| 原调用 | 替换为 |
|--------|--------|
| `OdinBridgeLocator.Bridge.GetFriendlyFullName(type)` | `type.GetNiceFullName()` |

**删除的文件（6 个）**：
- `Runtime/Unity/OdinBridge/IOdinBridge.cs`
- `Runtime/Unity/OdinBridge/DefaultOdinBridge.cs`
- `Runtime/Unity/OdinBridge/OdinBridgeLocator.cs`
- `Editor/OdinIntegration/Bridge/OdinInspectorBridge.cs`（含 `OdinBridgeInitializer`）
- 整个 `Runtime/Unity/OdinBridge/` 文件夹
- 整个 `Editor/OdinIntegration/Bridge/` 文件夹

### 5.3 影响处理

#### AesirInspectorSettings&lt;T&gt;（迁至 Runtime/OdinIntegration/Core）

当前调用 `OdinBridgeLocator.Bridge.GetFriendlyFullName(type)` 获取配置名。

**处理方式**：替换为 `type.GetNiceFullName()`（Sirenix.Utilities 扩展方法）。

**理由**：此文件迁移至 OdinIntegration 层后可直接使用 Sirenix API，保持与原有行为一致。

#### ScriptDocGenerator 分析代码（保留在 Runtime/Unity/TypeReflection）

`TypeData.cs` 和 `TypeAnalyzerStaticExtensions.cs` 调用 OdinBridge 获取格式化的类型名称。

**处理方式**：替换为 `TypeNameFormatter` 自定义实现。

**理由**：
- TypeReflection 模块的核心价值是「Odin 无关的类型反射分析」
- 类型名称格式化是纯字符串操作，不依赖 Odin 的高级功能
- `TypeNameFormatter` 实现量小（约 50-80 行），维护成本低
- 保留在 Runtime/Unity 使 ScriptDocGenerator 测试无需 Odin 依赖

---

## 六、迁移清单

### 6.1 从 Runtime/Unity 迁移到 Runtime/OdinIntegration

| 文件 | 源路径 | 目标路径 | 原因 |
|------|--------|---------|------|
| `AesirInspectorVersion.cs` | `Runtime/Unity/Core/` | `Runtime/OdinIntegration/Core/` | Odin 为默认层 |
| `AesirInspectorPaths.cs` | `Runtime/Unity/Core/` | `Runtime/OdinIntegration/Core/` | 同上 |
| `AesirInspectorWebLinks.cs` | `Runtime/Unity/Core/` | `Runtime/OdinIntegration/Core/` | 同上 |
| `AesirInspectorSettings.cs` | `Runtime/Unity/Core/` | `Runtime/OdinIntegration/Core/` | 需使用 Sirenix API |
| `AesirInspectorProjectSettingsSO.cs` | `Runtime/Unity/Core/` | `Runtime/OdinIntegration/Core/` | Odin 为默认层 |
| `IAesirInspectorReset.cs` | `Runtime/Unity/Core/` | `Runtime/OdinIntegration/Core/` | 同上 |
| `BilingualDisplayAsStringControl.cs` | `Runtime/Unity/Inspector/` | `Runtime/OdinIntegration/Bilingual/Controls/` | 仅由 Odin Processor 渲染 |
| `BilingualHeaderControl.cs` | `Runtime/Unity/Inspector/` | `Runtime/OdinIntegration/Bilingual/Controls/` | 同上 |
| `HorizontalSeparateControl.cs` | `Runtime/Unity/Inspector/` | `Runtime/OdinIntegration/Bilingual/Controls/` | 同上 |
| `BilingualData.cs` | `Runtime/Unity/Localization/` | `Runtime/OdinIntegration/Bilingual/Localization/` | 归入 Bilingual 模块 |
| `AesirInspectorLanguageSettingsSO.cs` | `Runtime/Unity/Localization/` | `Runtime/OdinIntegration/Bilingual/Localization/` | 同上 |
| `ILanguageProvider.cs` | `Runtime/Unity/Localization/` | `Runtime/OdinIntegration/Bilingual/Localization/` | 同上 |
| `LanguageProviderLocator.cs` | `Runtime/Unity/Localization/` | `Runtime/OdinIntegration/Bilingual/Localization/` | 同上 |
| `AesirInspectorLogger.cs` | `Runtime/Unity/Logging/` | `Runtime/OdinIntegration/Logging/` | Odin 为默认层 |
| `AesirInspectorLoggerSettings.cs` | `Runtime/Unity/Logging/` | `Runtime/OdinIntegration/Logging/` | 同上 |
| `PathUtility.cs` | `Runtime/Unity/Utilities/` | `Runtime/OdinIntegration/Utilities/` | Odin 为默认层 |
| `UrlUtility.cs` | `Runtime/Unity/Utilities/` | `Runtime/OdinIntegration/Utilities/` | 同上 |
| `RegexUtility.cs` | `Runtime/Unity/Utilities/` | `Runtime/OdinIntegration/Utilities/` | 同上 |
| `HierarchyUtility.cs` | `Runtime/Unity/Utilities/` | `Runtime/OdinIntegration/Utilities/` | 同上 |
| `PlayerLoopUtility.cs` | `Runtime/Unity/Utilities/` | `Runtime/OdinIntegration/Utilities/` | 同上 |
| `PredefinedAssemblyUtility.cs` | `Runtime/Unity/Utilities/` | `Runtime/OdinIntegration/Utilities/` | 同上 |
| `HierarchySafeEditorUtility.cs` | `Runtime/Unity/Utilities/` | `Runtime/OdinIntegration/Utilities/` | 同上 |
| `MonoScriptSafeEditorUtility.cs` | `Runtime/Unity/Utilities/` | `Runtime/OdinIntegration/Utilities/` | 同上 |
| `PathSafeEditorUtility.cs` | `Runtime/Unity/Utilities/` | `Runtime/OdinIntegration/Utilities/` | 同上 |
| `ProjectSafeEditorUtility.cs` | `Runtime/Unity/Utilities/` | `Runtime/OdinIntegration/Utilities/` | 同上 |
| `ScriptableObjectSafeEditorUtility.cs` | `Runtime/Unity/Utilities/` | `Runtime/OdinIntegration/Utilities/` | 同上 |
| `AesirInspectorCodeStyle.cs` | `Runtime/Unity/CodeStyle/` | `Runtime/OdinIntegration/CodeStyle/` | Odin 为默认层 |

### 6.2 从 Runtime/Unity 迁移到 Runtime/Unity/TypeReflection（内部重组）

| 文件 | 源路径 | 目标路径 | 原因 |
|------|--------|---------|------|
| `SummaryAttribute.cs` | `Runtime/Unity/TypeReflectionAnalyzer/Attributes/` | `Runtime/Unity/TypeReflection/Attributes/` | 扁平化，统一模块 |
| `ReferenceLinkURLAttribute.cs` | `Runtime/Unity/TypeReflectionAnalyzer/Attributes/` | `Runtime/Unity/TypeReflection/Attributes/` | 同上 |
| `TypeData.cs` | `Runtime/Unity/ScriptDocGenerator/AnalysisData/` | `Runtime/Unity/TypeReflection/Analysis/` | 归入 TypeReflection 模块 |
| `MemberData.cs` | `Runtime/Unity/ScriptDocGenerator/AnalysisData/` | `Runtime/Unity/TypeReflection/Analysis/` | 同上 |
| `IDerivedMemberData.cs` | `Runtime/Unity/ScriptDocGenerator/AnalysisData/` | `Runtime/Unity/TypeReflection/Analysis/` | 同上 |
| `ConstructorData.cs` | `Runtime/Unity/ScriptDocGenerator/AnalysisData/` | `Runtime/Unity/TypeReflection/Analysis/` | 同上 |
| `FieldData.cs` | `Runtime/Unity/ScriptDocGenerator/AnalysisData/` | `Runtime/Unity/TypeReflection/Analysis/` | 同上 |
| `MethodData.cs` | `Runtime/Unity/ScriptDocGenerator/AnalysisData/` | `Runtime/Unity/TypeReflection/Analysis/` | 同上 |
| `PropertyData.cs` | `Runtime/Unity/ScriptDocGenerator/AnalysisData/` | `Runtime/Unity/TypeReflection/Analysis/` | 同上 |
| `ParameterData.cs` | `Runtime/Unity/ScriptDocGenerator/AnalysisData/` | `Runtime/Unity/TypeReflection/Analysis/` | 同上 |
| `EventData.cs` | `Runtime/Unity/ScriptDocGenerator/AnalysisData/` | `Runtime/Unity/TypeReflection/Analysis/` | 同上 |
| `AccessModifierType.cs` | `Runtime/Unity/ScriptDocGenerator/Core/` | `Runtime/Unity/TypeReflection/Core/` | 归入 TypeReflection 模块 |
| `TypeCategory.cs` | `Runtime/Unity/ScriptDocGenerator/Core/` | `Runtime/Unity/TypeReflection/Core/` | 同上 |
| `DefaultAnalysisDataFactory.cs` | `Runtime/Unity/ScriptDocGenerator/Core/` | `Runtime/Unity/TypeReflection/Core/` | 同上 |
| `DefaultAttributeFilter.cs` | `Runtime/Unity/ScriptDocGenerator/Core/` | `Runtime/Unity/TypeReflection/Core/` | 同上 |
| `DerivedMemberDataComparer.cs` | `Runtime/Unity/ScriptDocGenerator/Core/` | `Runtime/Unity/TypeReflection/Core/` | 同上 |
| `TypeAnalyzerUtility.cs` | `Runtime/Unity/ScriptDocGenerator/Core/` | `Runtime/Unity/TypeReflection/Core/` | 同上 |
| `TypeAnalyzerStaticExtensions.cs` | `Runtime/Unity/ScriptDocGenerator/Core/` | `Runtime/Unity/TypeReflection/Core/` | 同上 |
| `ReflectionUtility.cs` | `Runtime/Unity/Utilities/` | `Runtime/Unity/TypeReflection/` | 仅被 TypeReflection 使用（3 文件 9 处） |

### 6.3 新增文件

| 文件 | 目标路径 | 用途 |
|------|---------|------|
| `TypeNameFormatter.cs` | `Runtime/Unity/TypeReflection/` | 替换 OdinBridge 的类型名称格式化（自定义实现） |

### 6.4 删除（OdinBridge 消除）

| 文件 | 路径 | 原因 |
|------|------|------|
| `IOdinBridge.cs` | `Runtime/Unity/OdinBridge/` | 桥接接口，不再需要 |
| `DefaultOdinBridge.cs` | `Runtime/Unity/OdinBridge/` | 降级实现，不再需要 |
| `OdinBridgeLocator.cs` | `Runtime/Unity/OdinBridge/` | 定位器，不再需要 |
| `OdinInspectorBridge.cs` | `Editor/OdinIntegration/Bridge/` | Odin 实现，直接内联到调用方 |

### 6.5 重命名/重组

| 原 | 新 | 原因 |
|----|-----|------|
| `Runtime/Unity/TypeReflectionAnalyzer/Attributes/` | `Runtime/Unity/TypeReflection/Attributes/` | 统一模块名称 |
| `Runtime/Unity/ScriptDocGenerator/AnalysisData/` | `Runtime/Unity/TypeReflection/Analysis/` | 归入 TypeReflection 模块 |
| `Runtime/Unity/ScriptDocGenerator/Core/` | `Runtime/Unity/TypeReflection/Core/` | 同上 |
| `Runtime/Unity/Attributes/`（空文件夹） | 删除 | 清理 |
| `Runtime/Unity/Inspector/`（迁空后） | 删除 | 清理 |
| `Runtime/Unity/ScriptDocGenerator/`（迁空后） | 删除 | 清理 |
| `Runtime/Unity/OdinBridge/`（删除后） | 删除 | 清理 |
| `Runtime/Unity/Core/`（迁空后） | 删除 | 清理 |
| `Runtime/Unity/Localization/`（迁空后） | 删除 | 清理 |
| `Runtime/Unity/Logging/`（迁空后） | 删除 | 清理 |
| `Runtime/Unity/Utilities/`（迁空后） | 删除 | 清理 |
| `Runtime/Unity/CodeStyle/`（迁空后） | 删除 | 清理 |
| `Editor/Unity/MiniTools/` | `Editor/Unity/QuickCreateSO/` | 更具描述性（仅含 QuickCreateSO） |
| `Editor/OdinIntegration/Drawers/` | `Editor/OdinIntegration/Bilingual/Drawers/` | 归入 Bilingual 模块 |
| `Editor/OdinIntegration/AttributeProcessors/` | `Editor/OdinIntegration/Bilingual/Processors/` | 归入 Bilingual 模块 |

### 6.6 保留不动

| 文件/文件夹 | 位置 | 保留理由 |
|------------|------|---------|
| TypeReflection（全部） | Runtime/Unity/TypeReflection/ | 自包含类型反射解析器，Odin 无关 |
| AesirInspectorInstallationChecker | Editor/Unity/Core | 安装检测，Odin 无关 |
| AesirInspectorMenuItems | Editor/Unity/Core | 菜单常量，跨层共享 |
| SummaryTool | Editor/Unity/SummaryTool | XML 工具，Odin 无关 |
| QuickCreateSO | Editor/Unity/QuickCreateSO | 独立工具，Odin 无关 |
| AttributeOverviewPro（全部） | Editor/OdinIntegration/AttributeOverviewPro | 已是 Odin 主体，不变 |

---

## 七、程序集变更

### 7.1 asmdef 引用调整

#### `Runestone.AesirInspector`（Runtime/Unity）

```
references: (无变化 — 无外部引用)
```

此程序集仅包含 TypeReflection 模块，不依赖任何其他程序集。移除 OdinBridge 后，不再有任何 Odin 相关代码。

#### `Runestone.AesirInspector.OdinIntegration`（Runtime/OdinIntegration）

```
references: ["Runestone.AesirInspector"]  (无变化)
defineConstraints: ["ODIN_INSPECTOR"]  (无变化)
```

新增从 Runtime/Unity 迁入的 Core、Bilingual（含 Controls + Localization）、Logging、Utilities（排除 ReflectionUtility）、CodeStyle。

#### `Runestone.AesirInspector.Editor`（Editor/Unity）

```diff
references: [
  "Runestone.AesirInspector",
+ "Runestone.AesirInspector.OdinIntegration"
]
+ defineConstraints: ["ODIN_INSPECTOR"]
```

**原因**：`QuickCreateSOMenuItem` 使用 `AesirInspectorLogger`（已迁至 OdinIntegration），`XmlSummaryTool` 使用 `SummaryAttribute`（留在 TypeReflection/Runtime/Unity）。因此 Editor/Unity 需同时引用两个 Runtime 程序集。由于 Odin 是强依赖，添加 `ODIN_INSPECTOR` 约束是合理的。

#### `Runestone.AesirInspector.OdinIntegration.Editor`（Editor/OdinIntegration）

```
references: [
  "Runestone.AesirInspector",
  "Runestone.AesirInspector.Editor",
  "Runestone.AesirInspector.OdinIntegration"
]
defineConstraints: ["ODIN_INSPECTOR"]  (无变化)
```

无变化。ScriptDocGenerator 的分析数据模型在 `Runestone.AesirInspector`（TypeReflection 模块）中，通过已有引用访问。

### 7.2 测试程序集调整

#### `Runestone.AesirInspector.Editor.Tests`（Tests/Editor）

```diff
references: [
  "Runestone.AesirInspector.Editor",
  "Runestone.AesirInspector",
+ "Runestone.AesirInspector.OdinIntegration"
]
defineConstraints: [
  "UNITY_INCLUDE_TESTS",
+ "ODIN_INSPECTOR"
]
precompiledReferences: [
  "nunit.framework.dll",
+ "Sirenix.OdinInspector.dll",
+ "Sirenix.Utilities.dll"
]
```

**原因**：测试代码引用 `TypeData` 等 TypeReflection 类（在 `Runestone.AesirInspector` 中，无需 Odin）和 `OdinBridgeLocator`（已删除）。由于 Editor/Unity 现在引用 OdinIntegration，测试也需要同步。部分测试可能涉及 Odin 相关的 Editor 代码。

#### `Runestone.AesirInspector.Tests`（Tests/Runtime）

无变化（仅含 UnityEngineObject 运算符重载测试，不涉及 OdinBridge 或 ScriptDocGenerator）。

---

## 八、文件量对比

| 位置 | 当前 | 优化后 | 变化 |
|------|------|--------|------|
| Runtime/Unity | ~27 | ~21 | -6（移除 OdinBridge 3 + Inspector 3 + Core 6 + Localization 4 + Logging 2 + Utilities 11 + CodeStyle 1 = -30，但 TypeReflection 内部重组 18 + ReflectionUtility 1 + TypeNameFormatter 1 = +20，净 -6，去除空文件夹 -1 = -7，实际保留 20+1=21） |
| Runtime/OdinIntegration | ~6 | ~35 | +29（Core 6 + Bilingual Controls 3 + Bilingual Localization 4 + Logging 2 + Utilities 11 + CodeStyle 1 + 原有 6 = 33+2=35） |
| Editor/Unity | ~6 | ~5 | -1（AesirInspectorSettings 迁出） |
| Editor/OdinIntegration | ~90+ | ~89+ | -1（Bridge 移除） |
| **总计** | ~129+ | ~150+ | +21（含新增 TypeNameFormatter） |

> **注**：文件总量增加是因为 TypeReflection 内部重组（ScriptDocGenerator 文件夹重命名但保留在 Runtime/Unity），以及新增 TypeNameFormatter。实际逻辑代码没有增加。

---

## 九、风险与注意事项

### 9.1 命名空间变更

Runtime/Unity 中 TypeReflection 模块的文件从 `Runtime/Unity/ScriptDocGenerator/` 和 `Runtime/Unity/TypeReflectionAnalyzer/` 重组为 `Runtime/Unity/TypeReflection/`。如果命名空间随之变更（从 `Runestone.AesirInspector` 子命名空间调整），需更新所有引用。

**缓解**：保持命名空间为 `Runestone.AesirInspector`（不加子命名空间），仅移动文件位置。这样所有引用方代码无需修改 `using` 语句。

Runtime/OdinIntegration 中迁入的文件（Core、Logging、Utilities 等）命名空间需从 `Runestone.AesirInspector` 变更为 `Runestone.AesirInspector.OdinIntegration`。

**缓解**：使用 IDE 全局替换命名空间。需检查 Editor 代码中的 `using` 语句。

### 9.2 TypeNameFormatter 实现质量

`TypeNameFormatter` 需要实现与 Sirenix `GetNiceName` / `GetNiceFullName` / `GetGenericConstraintsString` 等价的功能。如果实现不完整，ScriptDocGenerator 生成的文档中类型名称格式可能退化。

**缓解**：参考 Sirenix 源码或通过测试用例验证。现有 ScriptDocGenerator 测试（153+ 个）可作为回归验证。

### 9.3 Bilingual 控件迁移的序列化兼容性

`BilingualDisplayAsStringControl` 和 `BilingualHeaderControl` 从 Runtime/Unity 迁移到 Runtime/OdinIntegration。如果有序列化资产（.asset 文件）引用了这些类型，迁移后命名空间变更可能导致序列化断开。

**缓解**：这些控件在 AttributeOverviewPro 的 PanelSO 中作为内联字段使用。检查 `AttributeOverviewDatabase.asset`、`UnityExamples.asset`、`OdinExamples.asset` 是否有这些类型的序列化引用。如有，需更新 .asset 文件中的 `GUID` 和 `fileID` 映射，或重新创建子资产。

### 9.4 AesirInspectorSettings 迁移

`AesirInspectorSettings<T>` 从 Runtime/Unity/Core 迁移至 Runtime/OdinIntegration/Core。同时 `OdinBridgeLocator.Bridge.GetFriendlyFullName(type)` 替换为 `type.GetNiceFullName()`。

**影响**：命名空间变更可能影响子类（如 `AesirInspectorLanguageSettingsSO`）的 `using` 语句。配置名格式保持一致（Sirenix 的 `GetNiceFullName` 与原 OdinBridge 行为相同）。

### 9.5 Editor/Unity 新增 Odin 依赖

Editor/Unity 新增对 `Runestone.AesirInspector.OdinIntegration` 的引用和 `ODIN_INSPECTOR` 约束。这意味着 Editor/Unity 代码仅在 Odin Inspector 存在时编译。

**缓解**：Odin 已是强依赖，这是预期行为。Editor/Unity 中的代码（SummaryTool、QuickCreateSO）本身不使用 Odin API，仅通过引用间接依赖。

### 9.6 AesirInspectorCodeStyle.cs 中的注释

CodeStyle 文件中提到 `OdinBridgeLocator.Bridge` 的使用建议需要更新。迁移至 Runtime/OdinIntegration 后，应改为建议直接使用 `Sirenix.Utilities` API。

### 9.7 文档更新

以下文档需要同步更新：
- `CODELY.md`（目录结构、OdinBridge 描述、程序集表）
- `Documentation~/development.md`（OdinBridge 时序图、依赖说明）
- `Documentation~/README_EN.md`（OdinBridge 功能描述）
- `README.md`（OdinBridge 功能描述）
- `CHANGELOG.md`（记录本次结构变更）

---

## 十、实施建议

### 10.1 推荐分阶段实施

| 阶段 | 内容 | 风险 |
|------|------|------|
| **Phase 1** | 新增 `TypeNameFormatter.cs`，替换 TypeReflection 中的 OdinBridge 调用 | 中 — 需验证格式化输出一致性 |
| **Phase 2** | 删除 OdinBridge（6 个文件），替换 Editor 层调用为直接 Sirenix API | 低 — 编译错误易于定位 |
| **Phase 3** | 迁移 Inspector 控件到 Runtime/OdinIntegration/Bilingual/ | 中 — 需检查序列化兼容性 |
| **Phase 4** | 迁移 Core、Localization、Logging、Utilities、CodeStyle 到 Runtime/OdinIntegration | 中 — 涉及命名空间变更 |
| **Phase 5** | 重组 TypeReflection 模块（ScriptDocGenerator → TypeReflection 内部重组） | 低 — 纯文件移动 |
| **Phase 6** | 重组 Bilingual 模块（Drawers + Processors 合并）、重命名、清理空文件夹 | 低 |
| **Phase 7** | 更新 asmdef 引用、测试程序集配置 | 中 — 需验证编译 |
| **Phase 8** | 更新文档（CODELY.md、README、CHANGELOG 等） | 低 |

### 10.2 验证清单

每个阶段完成后执行：

- [ ] `unity_editor.start_compilation_pipeline` 编译通过
- [ ] `unity_console.get` 无错误
- [ ] Tests/Editor 全部通过（特别是 ScriptDocGenerator 的 153+ 测试）
- [ ] Tests/Runtime 全部通过
- [ ] AttributeOverviewPro 窗口可正常打开
- [ ] ScriptDocGenerator 窗口可正常生成文档（验证类型名称格式正确）
- [ ] Bilingual 特性在 Inspector 中正常显示
- [ ] Getting Started 窗口可正常打开

---

## 附录 A：完整目标目录树

```
AesirInspector/
├── Runtime/
│   ├── Unity/                                      [Runestone.AesirInspector]
│   │   └── TypeReflection/
│   │       ├── Attributes/
│   │       │   ├── SummaryAttribute.cs
│   │       │   └── ReferenceLinkURLAttribute.cs
│   │       ├── Analysis/
│   │       │   ├── MemberData.cs
│   │       │   ├── IDerivedMemberData.cs
│   │       │   ├── ConstructorData.cs
│   │       │   ├── FieldData.cs
│   │       │   ├── MethodData.cs
│   │       │   ├── PropertyData.cs
│   │       │   ├── ParameterData.cs
│   │       │   ├── EventData.cs
│   │       │   └── TypeData.cs
│   │       ├── Core/
│   │       │   ├── AccessModifierType.cs
│   │       │   ├── TypeCategory.cs
│   │       │   ├── DefaultAnalysisDataFactory.cs
│   │       │   ├── DefaultAttributeFilter.cs
│   │       │   ├── DerivedMemberDataComparer.cs
│   │       │   ├── TypeAnalyzerUtility.cs
│   │       │   └── TypeAnalyzerStaticExtensions.cs
│   │       ├── ReflectionUtility.cs
│   │       └── TypeNameFormatter.cs                     ← NEW
│   │   └── Runestone.AesirInspector.asmdef
│   │
│   └── OdinIntegration/                           [Runestone.AesirInspector.OdinIntegration]
│       ├── Core/
│       │   ├── AesirInspectorVersion.cs
│       │   ├── AesirInspectorPaths.cs
│       │   ├── AesirInspectorWebLinks.cs
│       │   ├── AesirInspectorSettings.cs
│       │   ├── AesirInspectorProjectSettingsSO.cs
│       │   └── IAesirInspectorReset.cs
│       ├── Bilingual/
│       │   ├── Attributes/
│       │   │   ├── BilingualTitleAttribute.cs
│       │   │   ├── BilingualButtonAttribute.cs
│       │   │   ├── BilingualInfoBoxAttribute.cs
│       │   │   ├── BilingualTextAttribute.cs
│       │   │   └── DetailInfoBoxAttribute.cs
│       │   ├── Controls/
│       │   │   ├── BilingualDisplayAsStringControl.cs
│       │   │   ├── BilingualHeaderControl.cs
│       │   │   └── HorizontalSeparateControl.cs
│       │   └── Localization/
│       │       ├── BilingualData.cs
│       │       ├── AesirInspectorLanguageSettingsSO.cs
│       │       ├── ILanguageProvider.cs
│       │       └── LanguageProviderLocator.cs
│       ├── Logging/
│       │   ├── AesirInspectorLogger.cs
│       │   └── AesirInspectorLoggerSettings.cs
│       ├── Utilities/
│       │   ├── PathUtility.cs
│       │   ├── UrlUtility.cs
│       │   ├── RegexUtility.cs
│       │   ├── HierarchyUtility.cs
│       │   ├── PlayerLoopUtility.cs
│       │   ├── PredefinedAssemblyUtility.cs
│       │   ├── HierarchySafeEditorUtility.cs
│       │   ├── MonoScriptSafeEditorUtility.cs
│       │   ├── PathSafeEditorUtility.cs
│       │   ├── ProjectSafeEditorUtility.cs
│       │   ├── ScriptableObjectSafeEditorUtility.cs
│       │   └── OdinCodeHighlighter.cs
│       ├── CodeStyle/
│       │   └── AesirInspectorCodeStyle.cs
│       └── Runestone.AesirInspector.OdinIntegration.asmdef
│
├── Editor/
│   ├── Unity/                                     [Runestone.AesirInspector.Editor]
│   │   ├── Core/
│   │   │   ├── AesirInspectorInstallationChecker.cs
│   │   │   └── AesirInspectorMenuItems.cs
│   │   ├── SummaryTool/
│   │   │   ├── XmlSummaryTool.cs
│   │   │   ├── XmlCodePart.cs
│   │   │   └── SummaryToolMenuItems.cs
│   │   ├── QuickCreateSO/
│   │   │   └── QuickCreateSOMenuItem.cs
│   │   └── Runestone.AesirInspector.Editor.asmdef
│   │
│   └── OdinIntegration/                           [Runestone.AesirInspector.OdinIntegration.Editor]
│       ├── Core/
│       │   └── AesirInspectorModuleAssetMarkerSO.cs
│       ├── Bilingual/
│       │   ├── Drawers/
│       │   │   ├── BilingualAttributeDrawer.cs
│       │   │   ├── BilingualButtonAttributeDrawer.cs
│       │   │   ├── BilingualInfoBoxAttributeDrawer.cs
│       │   │   ├── BilingualTextAttributeDrawer.cs
│       │   │   ├── BilingualTitleAttributeDrawer.cs
│       │   │   └── DetailInfoBoxAttributeDrawer.cs
│       │   └── Processors/
│       │       ├── BilingualDisplayAsStringProcessor.cs
│       │       ├── BilingualHeaderProcessor.cs
│       │       ├── HorizontalSeparateProcessor.cs
│       │       ├── AesirInspectorLanguageSettingsProcessor.cs
│       │       ├── AesirInspectorResetProcessor.cs
│       │       └── OdinAutoTooltipAttributeProcessor.cs
│       ├── AttributeOverviewPro/
│       │   ├── Abstract/
│       │   ├── Core/
│       │   ├── Data/
│       │   ├── AttributePanels/
│       │   └── UsageExamples/
│       ├── ScriptDocGenerator/
│       │   ├── Editor/
│       │   │   ├── Panels/
│       │   │   ├── Windows/
│       │   │   ├── Controllers/
│       │   │   └── Settings/
│       │   └── Menus/
│       ├── ExtensionManager/
│       │   ├── ExtensionPackageCard.cs
│       │   ├── ExtensionPackageManagerWindow.cs
│       │   └── PackageManagerEditorUtility.cs
│       ├── MiniTools/
│       │   ├── AesirInspectorMiniToolsWindow.cs
│       │   ├── MenuItemViewer/
│       │   └── SyntaxHighlighter/
│       ├── Windows/
│       │   ├── GettingStarted/
│       │   └── Preferences/
│       └── Runestone.AesirInspector.OdinIntegration.Editor.asmdef
│
├── Tests/
│   ├── Editor/                                    [Runestone.AesirInspector.Editor.Tests]
│   │   ├── ScriptDocGenerator/
│   │   └── SummaryTool/
│   └── Runtime/                                   [Runestone.AesirInspector.Tests]
│
├── Samples~/
│   ├── PluginConfigSolutions/
│   └── RuntimeInitializeLoadType/
│
├── Documentation~/
│   ├── aesir-inspector.md
│   ├── development.md
│   ├── README_EN.md
│   ├── CHANGELOG_EN.md
│   ├── LICENSE.md
│   └── Third Party Notices.md
│
└── package.json
```
