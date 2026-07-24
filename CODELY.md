# CODELY.md — Unity-Aesir-Packages

## 项目概览

**Unity-Aesir-Packages** 是 **Runestone Yuumix** 开发的 Unity/团结引擎自定义包集合，提供渐进式 MVP 架构框架、功能模块和编辑器扩展。

- **Unity 版本：** 2022.3.62f3c1
- **渲染管线：** URP（Universal Render Pipeline 14.0.12）
- **许可证：** MIT
- **作者：** [yuumixcode](https://github.com/yuumixcode)
- **语言：** C#（代码注释和 XML 文档使用中文，代码标识符使用英文）
- **代码规模：** 约 500 个 .cs 文件，分布在 3 个包中（Architecture 50、Modules 28、Inspector 422）

### 包列表

| 包名 | 包 ID | 版本 | 命名空间 | 说明 |
|------|------|------|---------|------|
| Aesir Architecture | `cn.runestone.aesir.architecture` | 0.4.2 | `Runestone.AesirArchitecture` | 渐进式 MVP/MVC 架构框架 — 能力接口组合、命令模式、查询模式、MiniEventBus、PlayerLoop 生命周期、纯 C# 架构根 + MonoBehaviour 适配层 |


| Aesir Modules | `cn.runestone.aesir.modules` | 0.4.2 | `Runestone.AesirModules` | 功能模块 — 轻量级 UI 框架（Manager-of-Managers 单例、四层 Canvas 层级、面板生命周期、可替换资源加载器） |

| Aesir Inspector | `cn.runestone.aesir-inspector` | 0.4.2 | `Runestone.AesirInspector` | 编辑器扩展库 — 双语 Inspector UI、安全编辑器工具、脚本文档生成器、XML Summary 同步工具、Odin Inspector 可选集成 |


---

## Aesir Architecture（0.4.2）



### 核心设计

框架采用**能力接口组合**模式。每个角色（View、Controller、Presenter、Command、Query、Service、Model）通过组合细粒度能力接口来定义：

- `ICanGetModel` / `ICanGetService` — 读取已注册模块
- `ICanInvokeEvent` / `ICanAddListener` — 事件总线交互
- `ICanExecuteCommand` / `ICanExecuteQuery` — 写/读分发
- `ICanSetContext` / `IContextHolder` — 上下文绑定

### 模块角色

| 角色 | 接口 | 能力 | 说明 |
|------|------|------|------|
| **Model** | `IModel` → `AbstractModel` | GetModel, GetService | 数据层；仅通过 Command 写入 |
| **Service** | `IService` → `AbstractService` | GetModel, GetService, Invoke, AddListener | 跨模块协调；不能执行 Command/Query |
| **View** | `IView` | GetModel, GetService, AddListener, Invoke | 只读访问；不能执行 Command |
| **Controller** | `IController` | GetModel, GetService, ExecuteCommand, ExecuteQuery | MVC 模式入口 |
| **Presenter** | `IPresenter` | 全部 Controller + AddListener + Invoke + IDisposable | MVP 模式；中介 Model ↔ View |
| **Command** | `ICommand` → `AbstractCommand` | Execute()，只写无返回值 | 修改 Model 状态 |
| **Query** | `IQuery<TResult>` → `AbstractQuery` | Execute() → TResult，只读 | 返回数据，无副作用 |

### 上下文系统

- `IContext` — 模块注册、获取与事件操作
- `AbstractContext<T>` — 纯 C# 单例实现（不依赖 MonoBehaviour）
  - `Configure()` 抽象方法 — 在此注册 Model 和 Service
  - `Interface` 静态属性 — 懒加载单例访问器
  - `Initialize()` — 调用 `Configure()`，然后按注册顺序初始化 Model → Service
  - `Dispose()` — 逆序销毁 Service → Model

### 关键运行时类

- **`AesirArchitecture`** — MonoBehaviour 单例（`[DefaultExecutionOrder(-999)]`），通过 `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)` 引导启动，`DontDestroyOnLoad`
- **`AesirMonoBehaviour`** — 架构感知 MonoBehaviour 基类
- **`AesirScriptableObject`** — 架构感知 ScriptableObject 基类
- **`ObservableValue<T>`** — 响应式属性；Model 持有可写实例，View 通过 `IReadOnlyObservableValue<T>` 订阅。支持 `SetValueSilently`、`AddListenerAndInvoke`
- **`MiniEvent` / `MiniEvent<T>`** — 轻量级零分配事件系统；返回 `AutoRemoveListenerHandle` 自动清理
- **`MiniEventBus<TEvent>`** — 静态类型化事件总线；事件需实现 `IEventArgs`
- **`GenericLocator<T>`** — 类型键控的服务定位器

### Odin Inspector 集成

- 通过 `ODIN_INSPECTOR` 定义符号条件编译
- Odin 集成使用独立 asmdef：`Runestone.AesirArchitecture.OdinIntegration`（Runtime）和 `Runestone.AesirArchitecture.Editor.OdinIntegration`（Editor）
- `AesirModules.Editor.Odin Integration` — UI/Scene 配置的 Attribute Processor
- 示例包含需要 Odin Inspector 的 ObservableValue Drawer 演示

### 自定义定义符号

- `AESIR_ARCHITECTURE` — 由 `EnsureAesirArchitectureDefine` 自动设置
- `ODIN_INSPECTOR`、`ODIN_INSPECTOR_3`、`ODIN_INSPECTOR_3_1`、`ODIN_INSPECTOR_3_2`、`ODIN_INSPECTOR_3_3`

---

## Aesir Modules（0.4.2）


### UI 框架

- **`UIModule`** — 通过 `AesirModules.GetOrAddChild<T>()` 创建的 MonoBehaviour 单例，管理面板生命周期
  - 静态 API：`UIModule.Show<T>()`、`UIModule.Hide<T>()`、`UIModule.Get<T>()`、`UIModule.Prewarm<T>()`、`UIModule.RegisterPrefab<T>()`
  - 面板状态：激活 → 停用（缓存）→ 销毁
  - `PrewarmAll()` — 通过协程逐帧预实例化
- **`IUIPanel`** — 生命周期契约：`Initialize → Show(payload) → Hide → DestroyPanel`
  - `Layer`（UILayer）、`DestroyOnHide`（bool）、`IsOpen`（bool）
- **`AesirBasePanel`** — MonoBehaviour 面板基类
- **`UIRoot`** — 构建四层 Canvas 层级；`UILayer` 枚举定义层级
- **`IUIAssetLoader` / `ResourcesUILoader`** — 可插拔资源加载（默认 Resources；可替换为 Addressables）
- **`BinderAssistant` / `BinderTag`** — UI 元素组件绑定系统
- **`SceneModule`** — 场景管理模块

### 场景编辑器

- `SceneManagerWindow` — 场景管理自定义编辑器窗口
- `BootstrapSceneHelper` — 场景引导工具
- `SceneAssetWrapper` — 可序列化场景资源引用

---

## Aesir Inspector（0.4.2）


### 核心功能

| # | 功能 | Odin 依赖 | 说明 |
|---|------|-----------|------|
| 1 | **Attribute Overview Pro** ⚡ | 需要 | 可搜索树形菜单展示所有 Odin & Aesir Inspector 特性面板，实时预览与示例代码。`Tools → Aesir → Inspector → Attribute Overview Pro` |
| 2 | **Script Doc Generator** ⚡ | 需要 | 通过反射分析 C# 类型信息生成结构化 API 文档，支持增量生成、个性化扩展。153 个单元测试覆盖。完全离线、AI 友好 Markdown 输出 |
| 3 | **Summary Tool** | 无 | 右键菜单处理 XML `<summary>` ↔ `[Summary]` 双向同步，支持 Sync/Replace/Remove 三种模式，宏定义感知 |
| 4 | **Mini Tools** ⚡ | 需要 | MenuItem Viewer（菜单项检查器）、Syntax Highlighter（语法高亮测试）、Quick Create SO（右键快捷创建 ScriptableObject） |
| 5 | **Extension Package Manager** ⚡ | 需要 | 卡片式 UI 一键安装/移除推荐 Unity Packages，基于 Git URL |
| 6 | **Bilingual Attributes** ⚡ | 需要 | `[BilingualTitle]`、`[BilingualButton]`、`[BilingualInfoBox]`、`[BilingualText]` 等双语特性，支持中英同时显示 |
| 7 | **OdinBridge** | 无 | `IOdinBridge` 接口隔离 Odin 依赖，无 Odin 时 `DefaultOdinBridge` 回退，有 Odin 时 `OdinInspectorBridge` 增强 |
| 8 | **Safe Editor Utilities** | 无 | 12+ 安全编辑器工具类（`ScriptableObjectSafeEditorUtility`、`PathUtility`、`HierarchyUtility`、`ReflectionUtility` 等），构建时自动剔除 |
| 9 | **Custom Attributes** | 无 | `[Summary]` 特性，等同于 XML `<summary>`，运行时可通过 `GetSummary()` 获取 |
| 10 | **Code Style** | 无 | 内置代码风格指南 `Runtime/Unity/CodeStyle/AESIR_INSPECTOR_CODE_STYLE.cs` |

### OdinBridge 架构

核心程序集（`Runtime/Unity/`、`Editor/Unity/`）零 Odin 依赖。Odin Integration 程序集通过 `ODIN_INSPECTOR` 编译约束自动启用/禁用：

```
IOdinBridge (接口) ──→ OdinBridgeLocator (运行时定位)
                        ├── OdinInspectorBridge (Odin 可用时)
                        └── DefaultOdinBridge (Odin 不可用时)
```

### SafeEditorUtility 模式

- `void` 方法：`[Conditional("UNITY_EDITOR")]` 标记，构建时自动剔除
- 有返回值方法：`#if UNITY_EDITOR` 双实现，构建时提供安全默认值
- 命名约定：`XxxSafeEditorUtility`（Runtime/Utilities/）、`XxxEditorUtility`（Editor-only）

### AesirInspector 编码规范（与其他包不同）

Aesir Inspector 采用**自文档化代码**和**无注释范式**，与 AesirArchitecture/Modules 的 XML 注释风格不同：

- **禁止 XML 注释**：不使用 `/// <summary>`、`/// <param>` 等
- **类必须使用 `[Summary]`**：所有 class/struct/interface 必须具备 `[Summary("...")]`
- **命名即文档**：其他成员通过清晰命名传达意图，仅复杂逻辑使用 `[Summary]`
- **严禁**对 `UnityEngine.Object` 派生类使用 `?.` / `??`
- **事件命名**：事件无 `On` 前缀（`DoorOpened`），订阅 `OnDoorOpened`，触发 `RaiseDoorOpened`
- **Utility 命名**：Runtime `XxxUtility`、Editor 安全封装 `XxxSafeEditorUtility`、Editor-only `XxxEditorUtility`

### 示例（Samples~/）

1. **PluginConfigSolutions** — ScriptableSingleton 在 Preferences 和 Project 中的使用示例
2. **RuntimeInitializeLoadType** — RuntimeInitializeOnLoadMethod 五个初始化时机的执行顺序示例

---

## 程序集定义

### Aesir Architecture（6 个 asmdef）

| 程序集 | 路径 | 引用 |
|--------|------|------|
| `Runestone.AesirArchitecture` | Runtime/ | — |
| `Runestone.AesirArchitecture.OdinIntegration` | Runtime/OdinIntergration/ | — |
| `Runestone.AesirArchitecture.Editor` | Editor/ | — |
| `Runestone.AesirArchitecture.Editor.OdinIntegration` | Editor/OdinIntegration/ | — |
| `Runestone.AesirArchitecture.Tests` | Tests/Runtime/ | — |
| `Runestone.AesirArchitecture.Tests.Editor` | Tests/Editor/ | — |

### Aesir Modules（3 个 asmdef）

| 程序集 | 路径 | 引用 |
|--------|------|------|
| `Runestone.AesirModules` | Runtime/ | AesirArchitecture |
| `Runestone.AesirModules.Editor` | Editor/ | — |
| `Runestone.AesirModules.Editor.OdinIntegration` | Editor/Odin Integration/ | — |

### Aesir Inspector（9 个 asmdef）

| 程序集 | Odin 依赖 | 路径 |
|--------|-----------|------|
| `Runestone.AesirInspector` | 无 | Runtime/Unity/ |
| `Runestone.AesirInspector.OdinIntegration` | `ODIN_INSPECTOR` | Runtime/Odin Integration/ |
| `Runestone.AesirInspector.Editor` | 无 | Editor/Unity/ |
| `Runestone.AesirInspector.OdinIntegration.Editor` | `ODIN_INSPECTOR` | Editor/Odin Integration/ |
| `Runestone.AesirInspector.Tests` | 无 | Tests/Runtime/ |
| `Runestone.AesirInspector.Editor.Tests` | 无 | Tests/Editor/ |
| `Runestone.AesirInspector.Samples.PluginConfig` | 无 | Samples~/PluginConfigSolutions/Runtime/ |
| `Runestone.AesirInspector.Samples.PluginConfig.Editor` | 无 | Samples~/PluginConfigSolutions/Editor/ |
| `Runestone.AesirInspector.Samples.LoadType.Editor` | 无 | Samples~/RuntimeInitializeLoadType/Editor/ |

Architecture 和 Modules 的示例有各自的 asmdef（如 `Runestone.AesirArchitecture.Samples.UI.Counter.Mvp`）。

---

## 场景

| 场景 | 路径 | 用途 |
|------|------|------|
| SampleScene | `Assets/Scenes/SampleScene.unity` | 默认 Unity 示例场景 |
| SampleForCounterMvp | `Assets/Samples/Aesir Architecture/0.4.2/UI Counter-MVP/Scene/SampleForCounterMvp.unity` | MVP 计数器示例 — 包含增加/减少/重置按钮的 Canvas |


### 当前场景层级（SampleForCounterMvp）

```
Camera
Canvas
  └─ SampleMvpCounterMainPanel
       ├─ IncreaseButton → Text
       ├─ CountText
       ├─ DecreaseButton → Text
       └─ ResetButton → Text
EventSystem
```

---

## 示例

### Aesir Architecture（`Assets/Samples/Aesir Architecture/0.4.2/`）


1. **Counter-MVC** — MVC 模式：Context → Controller → Command → Model → View 事件通知
2. **UI Counter-MVP** — MVP 模式：Presenter 中介 Model 和 View；使用 `UIModule` 管理面板
3. **ObservableValue (Odin Inspector)** — `ObservableValue<T>` 自定义 Drawer 演示；需要 Odin Inspector
4. **MiniEvent** — `MiniEvent` 和 `MiniEvent<T>` 使用示例（无参/单参事件）

### Aesir Inspector（`Assets/Runestone/AesirInspector/Samples~/`）

1. **PluginConfigSolutions** — ScriptableSingleton 在 Preferences 和 Project 中的使用示例
2. **RuntimeInitializeLoadType** — RuntimeInitializeOnLoadMethod 五个初始化时机的执行顺序与最佳实践示例

---

## 依赖

### Unity Registry 包

- `com.unity.render-pipelines.universal` 14.0.12 — URP
- `com.unity.feature.2d` 2.0.1 — 2D 工具包
- `com.unity.textmeshpro` 3.0.9 — 文本渲染
- `com.unity.timeline` 1.7.7 — Timeline
- `com.unity.ugui` 1.0.0 — uGUI
- `com.unity.test-framework` 1.1.33 — Unity 测试框架
- `com.unity.ide.rider` 3.0.40 — Rider IDE 支持
- `com.unity.ide.visualstudio` 2.0.27 — VS IDE 支持
- `cn.tuanjie.codely.bridge` 1.0.72 — Codely Unity Bridge
- `cn.tuanjie.ai.generators`（本地）— TJ AI Generators，文件引用 `.codely-cli/extensions/TJGenerators/`

### 第三方插件

- **Sirenix Odin Inspector** — 位于 `Assets/Plugins/Sirenix/`（已 gitignore；独立授权）。三个包的可选依赖。所有 Odin 相关代码均通过条件编译保护。

---

## 目录结构

```
Assets/
├── Runestone/                        # 包源代码
│   ├── AesirArchitecture/            # 核心架构框架（50 .cs）
│   │   ├── Runtime/
│   │   │   ├── Component/            # MonoBehaviour 适配器
│   │   │   │   ├── Common/           # AesirArchitecture, AesirMonoBehaviour, AesirScriptableObject
│   │   │   │   ├── Event/            # 自动移除监听触发器
│   │   │   │   ├── View/             # AesirView, MonoView
│   │   │   │   └── ViewController/   # AesirViewController, MonoViewController
│   │   │   └── Engine/
│   │   │       ├── Capabilities/     # 能力接口（ICanGetModel 等）
│   │   │       ├── Common/          # PlayerLoop, Debug, ResetStatics, AssemblyInfo
│   │   │       ├── Context/          # IContext, AbstractContext<T>
│   │   │       ├── Event/           # MiniEvent, MiniEventBus, AutoRemoveListenerHandle
│   │   │       ├── Locator/          # GenericLocator<T>
│   │   │       ├── Modules/          # 抽象类（Model, Service, Command, Query）+ 接口
│   │   │       ├── Observable/       # ObservableValue<T>, IObservableValue<T>
│   │   │       └── Utilities/       # PlayerLoopUtility
│   │   ├── Editor/                   # Odin 集成、定义符号管理
│   │   └── Tests/                    # 编辑器模式和运行时模式测试
│   ├── AesirModules/                 # 功能模块（28 .cs）
│   │   ├── Runtime/
│   │   │   ├── Core/                 # AesirModules 单例、调试、程序集信息
│   │   │   ├── Scene/                # SceneModule, SceneAssetWrapper
│   │   │   └── UI/                   # UIModule, UIRoot, UICanvasConfigSO, Binder, AesirBasePanel
│   │   └── Editor/                   # 场景编辑器窗口、Odin 集成、UI 菜单项
│   └── AesirInspector/               # 编辑器扩展库（422 .cs）
│       ├── Runtime/
│       │   ├── Unity/               # 核心运行时（Runestone.AesirInspector）
│       │   │   ├── Attributes/      # [Summary] 等自定义特性
│       │   │   ├── Core/            # 版本、路径、链接常量
│       │   │   ├── Inspector/       # Inspector 显示控件
│       │   │   ├── Localization/    # BilingualData, AesirInspectorLanguageSettingsSO
│       │   │   ├── Logging/         # AesirInspectorLogger, AesirInspectorLoggerSettings
│       │   │   ├── OdinBridge/      # IOdinBridge, DefaultOdinBridge, OdinBridgeLocator
│       │   │   ├── ScriptDocGenerator/ # 文档生成器运行时数据模型
│       │   │   ├── Utilities/       # 12+ 安全编辑器工具类
│       │   │   └── CodeStyle/       # 代码风格指南
│       │   └── Odin Integration/     # Odin 运行时（ODIN_INSPECTOR）
│       │       ├── Attributes/      # 双语特性（BilingualTitle, BilingualButton 等）
│       │       └── OdinCodeHighlighter.cs
│       ├── Editor/
│       │   ├── Unity/               # 核心编辑器（Runestone.AesirInspector.Editor）
│       │   │   ├── Core/            # 安装检测、菜单管理
│       │   │   ├── MiniTools/       # QuickCreateSO
│       │   │   └── SummaryTool/     # XmlSummaryTool
│       │   └── Odin Integration/     # Odin 编辑器（ODIN_INSPECTOR）
│       │       ├── AttributeOverviewPro/ # 特性总览窗口（Data-Panel-Example 三件套）
│       │       ├── AttributeProcessors/   # OdinAttributeProcessor
│       │       ├── Bridge/          # OdinInspectorBridge
│       │       ├── Drawers/         # 双语 Drawer
│       │       ├── ExtensionManager/ # 扩展包管理器
│       │       ├── MiniTools/       # MenuItem Viewer, Syntax Highlighter
│       │       ├── ScriptDocGenerator/ # 文档生成器编辑器逻辑
│       │       └── Windows/         # Getting Started, Preferences
│       ├── Tests/
│       │   ├── Editor/              # 编辑器模式测试（153+ ScriptDocGenerator 测试）
│       │   └── Runtime/             # 运行时模式测试
│       ├── Samples~/                # 使用示例
│       │   ├── PluginConfigSolutions/
│       │   └── RuntimeInitializeLoadType/
│       └── Documentation~/          # 用户文档与开发者指南
│           ├── aesir-inspector.md
│           ├── development.md
│           └── en/                  # 英文文档（README, CHANGELOG）
├── Samples/                          # 导入的 Architecture 示例
│   └── Aesir Architecture/0.4.2/

├── Scenes/                           # 示例场景
├── Settings/                         # URP 资源（Renderer2D, UniversalRP）、场景模板
└── Plugins/
    └── Sirenix/                      # Odin Inspector（已 gitignore）
```

---

## 开发规范

### 命名

- **命名空间：** `Runestone.AesirArchitecture` / `Runestone.AesirModules`（Architecture 和 Modules）、`Runestone.AesirInspector`（Inspector）
- **程序集名：** `Runestone.AesirArchitecture.*`、`Runestone.AesirModules.*`、`Runestone.AesirInspector.*`
- **类名：** PascalCase（如 `AbstractContext`、`ObservableValue`、`MiniEventBus`）
- **接口：** `I` 前缀（如 `ICommand`、`IContext`、`IUIPanel`）
- **抽象类：** `Abstract` 前缀（如 `AbstractModel`、`AbstractCommand`）
- **MonoBehaviour 单例：** 静态 `Instance` 属性、`[DefaultExecutionOrder(-999)]`、`DontDestroyOnLoad`
- **包 ID：** 反向域名（`cn.runestone.aesir.*`、`cn.runestone.aesir-inspector`）
- **私有字段：** `_camelCase`（非序列化）、`camelCase`（带 `[SerializeField]` 序列化）
- **常量/静态只读：** PascalCase

### 代码风格

#### Aesir Architecture 和 Modules

- XML 文档注释使用中文（摘要、参数说明、备注）
- 代码标识符使用英文
- 数据类标记 `[Serializable]`（`AbstractModel`、`ObservableValue<T>`、`AbstractSubmodule`）
- 显式接口实现上下文注入（`IContextHolder.Context`、`ICanSetContext.SetContext`）
- 静态 `Bootstrap()` 方法使用 `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]` 自动初始化
- `ResetStaticsAssistant.Register()` 保障 Domain Reload 安全

#### Aesir Inspector（不同规范）

- **禁止 XML 注释**，使用 `[Summary("...")]` 特性替代
- **自文档化代码**：命名即文档，仅复杂逻辑使用 `[Summary]` 解释"为什么"
- **严禁**对 `UnityEngine.Object` 派生类使用 `?.` / `??`
- **事件命名**：事件无 `On` 前缀（`DoorOpened`），订阅 `OnDoorOpened`，触发 `RaiseDoorOpened`
- **Enum**：普通含 `None = 0` 显式赋值；Flags `[Flags]` 值为 `1 << n`
- **Utility 命名**：Runtime `XxxUtility` / `XxxSafeEditorUtility`（`Runtime/Unity/Utilities/`），Editor-only `XxxEditorUtility`（`Editor/Unity/`）
- **Odin 依赖代码**必须放在 `Odin Integration/` 子目录
- **核心程序集不允许**直接引用 Odin API — 通过 `IOdinBridge` 桥接
- **Processor**：`internal sealed`，与目标类同文件定义
- **免除注释规范的模块**：`CodeStyle/`、`AttributeOverviewPro/Data/`、`AttributeOverviewPro/AttributePanels/`、`AttributeOverviewPro/UsageExamples/`

### 目录组织

- `Runtime/` 与 `Editor/` 分离，由 asmdef 强制
- Odin 相关代码隔离在 `OdinIntegration/` 或 `Odin Integration/` 子目录，使用独立 asmdef
- 测试分为 `Tests/Editor/`（编辑器模式）和 `Tests/Runtime/`（运行时模式）
- 示例在包源码中使用 `Samples~/` 约定，导入后位于 `Assets/Samples/`

---

## 构建与运行

### 编辑器

1. 在 Unity 2022.3.62f3c1（或团结引擎等效版本）中打开项目
2. 打开 `Assets/Samples/Aesir Architecture/0.4.2/UI Counter-MVP/Scene/SampleForCounterMvp.unity`

3. 按 **Play** 运行 MVP 计数器示例

### 命令行构建

```bash
Unity -batchmode -quit -projectPath . \
       -buildTarget StandaloneOSX -logFile build.log
```

尚无自定义构建脚本。

### 测试

- **Architecture 编辑器模式：** `Assets/Runestone/AesirArchitecture/Tests/Editor/`（如 `AesirArchitecturePlayerLoopTests.cs`）
- **Architecture 运行时模式：** `Assets/Runestone/AesirArchitecture/Tests/Runtime/`（如 `UnityEngineObjectCheckNullTests.cs`）
- **Inspector 编辑器模式：** `Assets/Runestone/AesirInspector/Tests/Editor/`（153+ ScriptDocGenerator 测试）
- **Inspector 运行时模式：** `Assets/Runestone/AesirInspector/Tests/Runtime/`
- **CLI 运行测试：**
  ```bash
  Unity -batchmode -quit -projectPath . \
         -testPlatform editmode -runTests \
         -testResults TestResults.xml -logFile test.log
  ```

---

## 版本控制

### 忽略（按 `.gitignore`）

- `Library/`、`Temp/`、`obj/`、`Build/`、`Builds/`、`Logs/`、`UserSettings/`
- `Assets/Plugins/Sirenix/` — Odin Inspector（独立授权，不分发）
- 生成的 IDE 文件（`*.csproj`、`*.sln`、`.vs/`、`.idea/`）
- `*.unitypackage`、`*.apk`、`*.aab`、`*.app`
- Addressables 构建产物（如使用）
- 测试运行器自动生成的场景（`InitTestScene*.unity*`）

### 纳入版本控制

- `Assets/Runestone/` — 所有包源代码（Architecture、Modules、Inspector）
- `Assets/Samples/` — 导入的示例
- `Assets/Scenes/`、`Assets/Settings/`
- `Packages/manifest.json` — 包依赖
- `ProjectSettings/` — Unity 项目配置
- `CODELY.md`、`README.md`、`LICENSE`

---

## 待办 / 待确认

- 无自定义构建脚本（`BuildScript.cs`）— CLI 构建使用 Unity 默认
- 未找到 CI/CD 管道配置
- 无 Addressables 配置（UI 默认使用 `ResourcesUILoader`；`IUIAssetLoader` 接口已预留 Addressables 支持）
- URP 设置位于 `Assets/Settings/UniversalRP.asset` 和 `Renderer2D.asset`
- Aesir Inspector `Third Party Notices.md` 包含占位内容（Semver/MyComponent）— 需更新
- Aesir Inspector 版本为 `0.4.2`


## Codely Structured Memories

### User
- [2026-07-24 15:50:00] 用户是 yuumixcode（Runestone），三个 Aesir 包（Architecture、Modules、Inspector）的作者，偏好中文交流。


### Feedback
- [2026-07-24 15:43:36] Aesir Inspector 与 Aesir Architecture/Modules 使用不同编码规范。Inspector：禁止 XML 注释（用 [Summary] 特性替代）、自文档化代码、禁止对 UnityEngine.Object 派生类使用 ?. /??、事件命名无 On 前缀。Architecture/Modules：中文 XML 文档注释、显式接口实现。编辑代码时需确认所在包以应用正确风格。
- [2026-07-24 22:03:03] 项目英文文档命名规范：根目录文档使用 `.en.md` 后缀（如 `README.en.md`、`CODE_OF_CONDUCT.en.md`、`CONTRIBUTING.en.md`）；各子包的英文 README 统一放在 `Documentation~/README_EN.md`。**Why:** 子包英文 README 归入 Documentation~ 文件夹（UPM 隐藏目录），使用 `README_EN.md` 命名。**How to apply:** 根目录英文文档用 `.en.md`，子包英文 README 用 `Documentation~/README_EN.md`。
- [2026-07-25 00:55:49] Git Commit 消息使用中文编写。**Why:** 用户偏好中文交流，项目代码注释和文档均使用中文。**How to apply:** 所有 git commit message 使用中文撰写，包括 subject 和 body。

### Project
- [2026-07-24 21:08:41] AttributeOverviewPro 子资产重构计划：将 ~194 个独立 .asset 文件（70 PanelSO + 123 ExampleSO + 1 DatabaseSO）合并为 1 个数据库 .asset 文件，所有 PanelSO/ExampleSO 作为 DatabaseSO 的子资产（AddObjectToAsset）。**Why:** 用户导入 Aesir Inspector 后自动生成资产数量膨胀。**How to apply:** 实现时仅需修改 5 个文件（DatabaseSO、AttributeExampleSO、OdinAttributeExampleSO、AbstractAttributePanelSO、AesirInspectorPaths），Data/Panel/Example 类定义不变。

### Reference
- [2026-07-24 21:08:41] AttributeOverviewPro 资产精简方案文档位于 Docs/AttributeOverviewPro-AssetReduction-Plan.md — 包含现状分析、可行性评估、子资产架构设计、详细实现步骤、验证步骤和备选方案。
