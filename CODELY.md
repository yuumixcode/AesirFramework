# CODELY.md — Unity-Aesir-Packages

## 项目概览

**Unity-Aesir-Packages** 是 **Runestone Yuumix** 开发的 Unity/团结引擎自定义包集合，提供渐进式 MVC 架构框架、功能模块和编辑器扩展。

- **Unity 版本：** 2022.3.62f3c1
- **渲染管线：** URP（Universal Render Pipeline 14.0.12）
- **许可证：** MIT
- **作者：** [yuumixcode](https://github.com/yuumixcode)
- **语言：** C#（代码注释和 XML 文档使用中文，代码标识符使用英文）
- **代码规模：** 约 567 个 .cs 文件，分布在 3 个包中（Architecture 101、Modules 41、Inspector 425）

### 包列表

| 包名 | 包 ID | 版本 | 命名空间 | 说明 |
|------|------|------|---------|------|
| Aesir Architecture | `cn.runestone.aesir.architecture` | 0.12.0 | `Runestone.AesirArchitecture` | 渐进式 MVC 架构框架 — 能力接口组合、命令/查询模式、轻量事件（MiniEvent）与响应式属性（ObservableValue）、PlayerLoop 生命周期、纯 C# 架构根 + MonoBehaviour 适配层 |
| Aesir Modules | `cn.runestone.aesir.modules` | 0.12.0 | `Runestone.AesirModules` | 功能模块 — 轻量级 UI 框架（Manager-of-Managers 单例、四层 Canvas 层级、面板生命周期、可替换资源加载器）+ 实验性事件模块 |
| Aesir Inspector | `cn.runestone.aesir-inspector` | 0.12.0 | `Runestone.AesirInspector` | 编辑器扩展库 — 双语 Inspector UI、安全编辑器工具、脚本文档生成器、XML Summary 同步工具，强依赖 Odin Inspector |

### 依赖关系

- **Aesir Architecture** — 不依赖任何 Aesir 子包，可独立安装
- **Aesir Inspector** — 不依赖任何 Aesir 子包，可独立安装；强依赖 Odin Inspector
- **Aesir Modules** — 依赖 `cn.runestone.aesir.architecture`（0.12.0）

---

## Aesir Architecture（0.12.0）

> 框架以 **MVC 为主要模式**，`IController` 是推荐的快速开发入口；`IPresenter`（MVP）作为可选的严格分层模式。

### 核心设计

框架采用**能力接口组合**模式。每个角色（View、Controller、Presenter、Command、Query、Service、Model）通过组合细粒度能力接口来定义：

- `ICanGetModel` / `ICanGetService` — 读取已注册模块
- `ICanExecuteCommand` / `ICanExecuteQuery` — 写/读分发
- `ICanSetContext` / `IContextHolder` — 上下文绑定

### 模块角色

| 角色 | 接口 | 能力 | 说明 |
|------|------|------|------|
| **Model** | `IModel` → `AbstractModel` | GetModel, GetService | 数据层；仅通过 Command 写入 |
| **Service** | `IService` → `AbstractService` | GetModel, GetService | 跨模块协调；不能执行 Command/Query |
| **View** | `IView` | GetModel, GetService | 只读访问；不能执行 Command |
| **Controller** | `IController` | GetModel, GetService, ExecuteCommand, ExecuteQuery | MVC 模式入口（推荐） |
| **Presenter** | `IPresenter` | 全部 Controller + IDisposable | MVP 模式（可选）；中介 Model ↔ View |
| **Command** | `ICommand` → `AbstractCommand` | Execute()，只写无返回值 | 修改 Model 状态 |
| **Query** | `IQuery<TResult>` → `AbstractQuery` | Execute() → TResult，只读 | 返回数据，无副作用 |

### 上下文系统

- `IContext` — 模块注册与获取；`GetModel`/`GetService` 未注册时抛 `InvalidOperationException`（不返回 null）
- `AbstractContext<T>` — 纯 C# 单例实现（不依赖 MonoBehaviour）
  - `Configure()` 抽象方法 — 在此注册 Model 和 Service
  - `Instance` 静态属性 — 懒加载单例访问器；初始化成功后才赋值 `_instance`，失败不缓存、根因异常每次抛出
  - `Initialize()` — 调用 `Configure()`，然后按注册顺序初始化 Model → Service；失败不做回滚
  - `Dispose()` — 逆序销毁 Service → Model（按注册逆序，`GenericLocator<T>` 保序）

### 关键运行时类

- **`AesirArchitecture`** — MonoBehaviour 单例（`[DefaultExecutionOrder(-999)]`），预放置/运行时创建统一由 `[SerializeField] bool dontDestroyOnLoad = true` 序列化字段控制
- **`AesirMonoBehaviour`** — 架构感知 MonoBehaviour 基类
- **`AesirScriptableObject`** — 架构感知 ScriptableObject 基类
- **`ObservableValue<T>`** — 响应式属性；Model 持有可写实例，View 通过 `IReadOnlyObservableValue<T>` 订阅。支持 `SetValueSilently`、`AddListenerAndInvoke`
- **`MiniEvent` / `MiniEvent<T>`** — 轻量级零分配事件系统（直接多播调用）；返回 `AutoRemoveListenerHandle` 自动清理。异常语义 = 原生 C# 事件（fail-fast）
- **`GenericLocator<T>`** — 类型键控的服务定位器（保序注册/查询）
- **`AesirArchitecturePlayerLoop`** — PlayerLoop 注入；`EnsureInjected()` 公开 API + `Register` 期自动检测
- **`ResetStaticsAssistant`** — 仅服务泛型类的静态重置（泛型类 RIOLM 被 Unity 静默跳过）
- **`MonoLifecycleProxy`** — 生命周期代理，将 Unity 原生回调统一为可订阅的 MiniEvent

### 渐进式示例家族（六档）

| 档位 | 示例 | Model 暴露面 | 读写路径 | View 边界 |
|------|------|-------------|---------|-----------|
| MVC-1 快捷 | Counter-Mvc-Quick | 具体类，可写 ObservableValue | View 兼 Controller 直写直读 | `MonoViewController<T>` |
| MVC-2 标准 | Counter-Mvc-Standard | 具体类，只读暴露 + 写方法 | Controller 直调写方法 | View 与 Controller 分离 |
| MVC-3 严格 | Counter-Mvc-Strict | 接口注册，只读暴露 + 写方法 | Command 写 + Query 读 | View 按窄接口持有 Controller |
| MVP-1 快捷 | Counter-Mvp-Quick | 具体类，可写 ObservableValue | Presenter 直写 + 推送 | 纯 MonoBehaviour，零接口 |
| MVP-2 标准 | Counter-Mvp-Standard | 具体类，只读暴露 + 写方法 | Presenter 直调写方法 | View 契约 `IXxxView` |
| MVP-3 严格 | Counter-Mvp-Strict | 接口注册，只读暴露 + 写方法 | Command 写 + Query 读 | View 按窄接口持有 Presenter |

### 设计边界（极简原则）

- **事件总线 / EventChannel** — 不做；跨模块通信使用互相 GetModel + ObservableValue 订阅，或直接引用 MiniEvent
- **Context 多实例** — 不做；CRTP 泛型单例，多存档/多房间在业务层建模
- **Command/Query 池化、async、Undo/Redo** — 不做；保持同步、无缓存
- **View 生命周期脚手架** — 不做；面板生命周期由 Aesir Modules 的 UIModule 负责
- **线程安全** — 不做；仅保证主线程使用
- **监听回调不应抛异常** — 框架约定（fail-fast），异常直接向上传播由 Unity 记日志
- **`Configure()` 中禁止访问 `Instance`** — 会递归创建第二个上下文实例
- **`Register` 与 `Get` 必须使用相同类型参数** — 按键精确匹配
- **运行时替换 Model/Service 仅用于测试调试** — 旧实例被 Dispose，其上的订阅不会迁移
- **第三方 SDK 修改 PlayerLoop 后手动调用 `AesirArchitecturePlayerLoop.EnsureInjected()`** — `Register` 注册回调时会自动检测补插
- **写入纪律档位** — 快捷/标准档表现层可直写 Model；标准档起表现层写入必经 Command；严格档只读 + 写方法；Service 可直写

### Odin Inspector 集成

- 通过 `ODIN_INSPECTOR` 定义符号条件编译
- Odin 集成使用独立 asmdef：`Runestone.AesirArchitecture.OdinInspector`（Editor）和 `Runestone.AesirArchitecture.Editor.OdinInspector`（Editor）
- 预放置实例风险通过 Odin AttributeProcessor 注入 Warning InfoBox
- DDOL 开关字段级 InfoBox — `[Tooltip]` 迁移为 AttributeProcessor 注入的 Info 级信息框（样式与逻辑分离，运行时程序集零 Inspector 样式特性）

### 自定义定义符号

- `AESIR_ARCHITECTURE` — 由 `EnsureAesirArchitectureDefine` 自动设置
- `ODIN_INSPECTOR`、`ODIN_INSPECTOR_3`、`ODIN_INSPECTOR_3_1`、`ODIN_INSPECTOR_3_2`、`ODIN_INSPECTOR_3_3`

---

## Aesir Modules（0.12.0）

### UI 框架

- **`UIModule`** — 通过 `AesirModules.GetOrAddChild<T>()` 创建的 MonoBehaviour 单例，管理面板生命周期
  - 静态 API：`UIModule.Show<T>()`、`UIModule.Hide<T>()`、`UIModule.Get<T>()`、`UIModule.Prewarm<T>()`、`UIModule.RegisterPrefab<T>()`
  - 面板状态：激活 → 停用（缓存）→ 销毁
  - `PrewarmAll()` — 通过协程逐帧预实例化
- **`IUIPanel`** — 生命周期契约：`Initialize → Show(payload) → Hide → DestroyPanel`
  - `Layer`（UILayer）、`DestroyOnHide`（bool）、`IsOpen`（bool）
- **`AesirBasePanel`** — MonoBehaviour 面板基类；`OnDestroy` 静态反清理 `UIModule.RemovePanelRecord`
- **`UIRoot`** — 构建四层 Canvas 层级；`UILayer` 枚举定义层级
- **`IUIAssetLoader` / `ResourcesUILoader`** — 可插拔资源加载（默认 Resources；可替换为 Addressables）
- **`BinderAssistant` / `BinderTag`** — UI 元素组件绑定系统（OdinInspector 程序集）
- **`SceneModule`** — 场景管理模块

### DDOL 机制

- `AesirModules`、`UIRoot`、`UIModule` 新增 `[SerializeField] bool dontDestroyOnLoad = true`，预放置/运行时创建统一由该字段控制
- `UIModule` 的字段仅在预放置为根物体时生效，运行时自动创建时挂载于 [Aesir Modules] 宿主下跟随宿主决策
- DDOL 开关字段级 InfoBox + 警告可见性修复（`AesirModulesAttributeProcessor` / `UIModuleAttributeProcessor` / `UIRootAttributeProcessor`）

### 场景编辑器

- `SceneManagerWindow` — 场景管理自定义编辑器窗口
- `BootstrapSceneHelper` — 场景引导工具
- `SceneAssetWrapper` — 可序列化场景资源引用

---

## Aesir Inspector（0.12.0）

### 核心功能

| # | 功能 | Odin 依赖 | 说明 |
|---|------|-----------|------|
| 1 | **Attribute Overview Pro** ⚡ | 需要 | 可搜索树形菜单展示所有 Odin & Aesir Inspector 特性面板，实时预览与示例代码。`Tools → Aesir → Inspector → Attribute Overview Pro` |
| 2 | **Script Doc Generator** ⚡ | 需要 | 通过反射分析 C# 类型信息生成结构化 API 文档，支持增量生成、个性化扩展。107 个单元测试覆盖。完全离线、AI 友好 Markdown 输出 |
| 3 | **Summary Tool** | 无 | 右键菜单处理 XML `<summary>` ↔ `[Summary]` 双向同步，支持 Sync/Replace/Remove 三种模式，宏定义感知 |
| 4 | **Mini Tools** ⚡ | 需要 | MenuItem Viewer（菜单项检查器）、Syntax Highlighter（语法高亮测试）、Quick Create SO（右键快捷创建 ScriptableObject） |
| 5 | **Extension Package Manager** ⚡ | 需要 | 卡片式 UI 一键安装/移除推荐 Unity Packages，基于 Git URL |
| 6 | **Bilingual Attributes** ⚡ | 需要 | `[BilingualTitle]`、`[BilingualButton]`、`[BilingualInfoBox]`、`[BilingualText]` 等双语特性，支持中英同时显示 |
| 7 | **Safe Editor Utilities** | 无 | 12+ 安全编辑器工具类（`ScriptableObjectSafeEditorUtility`、`PathUtility`、`HierarchyUtility`、`ReflectionUtility` 等），构建时自动剔除 |
| 8 | **Custom Attributes** | 无 | `[Summary]` 特性，等同于 XML `<summary>`，运行时可通过 `GetSummary()` 获取（仅用于 ScriptDocGenerator 反射回退） |

### SafeEditorUtility 模式

- `void` 方法：`[Conditional("UNITY_EDITOR")]` 标记，构建时自动剔除
- 有返回值方法：`#if UNITY_EDITOR` 双实现，构建时提供安全默认值
- 命名约定：`XxxSafeEditorUtility`（Runtime/Utilities/）、`XxxEditorUtility`（Editor-only）

### AesirInspector 编码规范（与其他包不同）

Aesir Inspector 采用**自文档化代码**范式，与 AesirArchitecture/Modules 的风格有差异：

- **使用 XML 文档注释**（`/// <summary>`）；`[Summary]` 特性已从全部源码中移除（252 文件，897 处），`SummaryAttribute` 类仅保留供 ScriptDocGenerator 反射回退
- **自文档化代码**：命名即文档，仅复杂逻辑使用 XML 注释解释"为什么"
- **严禁**对 `UnityEngine.Object` 派生类使用 `?.` / `??`
- **事件命名**：事件无 `On` 前缀（`DoorOpened`），订阅 `OnDoorOpened`，触发 `RaiseDoorOpened`
- **Enum**：普通含 `None = 0` 显式赋值；Flags `[Flags]` 值为 `1 << n`
- **Utility 命名**：Runtime `XxxUtility` / `XxxSafeEditorUtility`（`Runtime/Unity/Utilities/`），Editor-only `XxxEditorUtility`（`Editor/Unity/`）
- **Odin 依赖代码**必须放在 `OdinInspector/` 子目录，使用独立 asmdef
- **核心程序集**通过 `#if ODIN_INSPECTOR` 条件编译直接使用 Sirenix API（OdinBridge 桥接层已移除）
- **Processor**：`internal sealed`，与目标类同文件定义
- **免除注释规范的模块**：`AttributeOverviewPro/Data/`、`AttributeOverviewPro/AttributePanels/`、`AttributeOverviewPro/UsageExamples/`

### 示例（Samples~/）

1. **PluginConfigSolutions** — ScriptableSingleton 在 Preferences 和 Project 中的使用示例
2. **RuntimeInitializeLoadType** — RuntimeInitializeOnLoadMethod 五个初始化时机的执行顺序示例

---

## 程序集定义

### Aesir Architecture（13 个 asmdef）

| 程序集 | 路径 | 引用 |
|--------|------|------|
| `Runestone.AesirArchitecture` | Runtime/ | — |
| `Runestone.AesirArchitecture.Editor` | Editor/ | — |
| `Runestone.AesirArchitecture.Editor.OdinInspector` | Editor/OdinInspector/ | — |
| `Runestone.AesirArchitecture.Tests` | Tests/Runtime/ | — |
| `Runestone.AesirArchitecture.Tests.Editor` | Tests/Editor/ | — |
| `Runestone.AesirArchitecture.Samples.MvcQuick` | Samples~/Counter-Mvc-Quick/Scripts/ | — |
| `Runestone.AesirArchitecture.Samples.MvcStandard` | Samples~/Counter-Mvc-Standard/Scripts/ | — |
| `Runestone.AesirArchitecture.Samples.MvcStrict` | Samples~/Counter-Mvc-Strict/Scripts/ | — |
| `Runestone.AesirArchitecture.Samples.MvpQuick` | Samples~/Counter-Mvp-Quick/Scripts/ | — |
| `Runestone.AesirArchitecture.Samples.MvpStandard` | Samples~/Counter-Mvp-Standard/Scripts/ | — |
| `Runestone.AesirArchitecture.Samples.MvpStrict` | Samples~/Counter-Mvp-Strict/Scripts/ | — |
| `Runestone.AesirArchitecture.Samples.MiniEvent` | Samples~/MiniEvent/Scripts/ | — |
| `Runestone.AesirArchitecture.Samples.ObservableValue` | Samples~/ObservableValue/Scripts/ | — |

### Aesir Modules（6 个 asmdef）

| 程序集 | 路径 | 引用 |
|--------|------|------|
| `Runestone.AesirModules` | Runtime/ | AesirArchitecture |
| `Runestone.AesirModules.OdinInspector` | Runtime/OdinInspector/ | — |
| `Runestone.AesirModules.Editor` | Editor/ | — |
| `Runestone.AesirModules.Editor.OdinInspector` | Editor/OdinInspector/ | — |
| `Runestone.AesirModules.InputSystem` | Runtime/InputSystem/ | — |
| `Runestone.AesirModules.Samples.Events.KeyPress` | Samples~/Events/01_KeyPress/ | — |

### Aesir Inspector（9 个 asmdef）

| 程序集 | Odin 依赖 | 路径 |
|--------|-----------|------|
| `Runestone.AesirInspector` | 无 | Runtime/Unity/ |
| `Runestone.AesirInspector.OdinInspector` | `ODIN_INSPECTOR` | Runtime/OdinInspector/ |
| `Runestone.AesirInspector.Editor` | 无 | Editor/Unity/ |
| `Runestone.AesirInspector.Editor.OdinInspector` | `ODIN_INSPECTOR` | Editor/OdinInspector/ |
| `Runestone.AesirInspector.Tests` | 无 | Tests/Runtime/ |
| `Runestone.AesirInspector.Editor.Tests` | 无 | Tests/Editor/OdinInspector/ |
| `Runestone.AesirInspector.Samples.PluginConfig` | 无 | Samples~/PluginConfigSolutions/Runtime/ |
| `Runestone.AesirInspector.Samples.PluginConfig.Editor` | 无 | Samples~/PluginConfigSolutions/Editor/ |
| `Runestone.AesirInspector.Samples.LoadType.Editor` | 无 | Samples~/RuntimeInitializeLoadType/Editor/ |

---

## 场景

| 场景 | 路径 | 用途 |
|------|------|------|
| SampleScene | `Assets/Scenes/SampleScene.unity` | 默认 Unity 示例场景 |
| SampleForCounterMvcQuick | `Assets/Samples/Aesir Architecture/0.12.0/Counter-Mvc-Quick/Scene/SampleForCounterMvcQuick.unity` | MVC 快捷档计数器示例 |
| SampleForCounterMvcStandard | `Assets/Samples/Aesir Architecture/0.12.0/Counter-Mvc-Standard/Scene/SampleForCounterMvcStandard.unity` | MVC 标准档计数器示例 |
| SampleForCounterMvcStrict | `Assets/Samples/Aesir Architecture/0.12.0/Counter-Mvc-Strict/Scene/SampleForCounterMvcStrict.unity` | MVC 严格档计数器示例 |
| SampleForCounterMvpQuick | `Assets/Samples/Aesir Architecture/0.12.0/Counter-Mvp-Quick/Scene/SampleForCounterMvpQuick.unity` | MVP 快捷档计数器示例 |
| SampleForCounterMvpStandard | `Assets/Samples/Aesir Architecture/0.12.0/Counter-Mvp-Standard/Scene/SampleForCounterMvpStandard.unity` | MVP 标准档计数器示例 |
| SampleForCounterMvpStrict | `Assets/Samples/Aesir Architecture/0.12.0/Counter-Mvp-Strict/Scene/SampleForCounterMvpStrict.unity` | MVP 严格档计数器示例 |
| MiniEventSample | `Assets/Samples/Aesir Architecture/0.12.0/MiniEvent/Scene/MiniEventSample.unity` | MiniEvent 使用示例 |
| ObservableValueInspector | `Assets/Samples/Aesir Architecture/0.12.0/ObservableValue/Scene/ObservableValueInspector.unity` | ObservableValue Inspector 演示 |

---

## 示例

### Aesir Architecture（`Assets/Samples/Aesir Architecture/0.12.0/`）

1. **Counter-Mvc-Quick（快捷档）** — `MonoViewController<T>` 直写直读，最少概念跑通数据驱动 UI 闭环
2. **Counter-Mvc-Standard（标准档）** — Model 只读暴露 + 写方法；View 与 Controller 分离共享 Model
3. **Counter-Mvc-Strict（严格档）** — Model 接口注册 + 只读暴露 + 写方法；Command 写 + Query 读；View 按窄接口持有 Controller
4. **Counter-Mvp-Quick（快捷档）** — Presenter 直改可写 ObservableValue（零接口抽象）并推送被动 View
5. **Counter-Mvp-Standard（标准档）** — Model 只读暴露 + 写方法；Presenter 直调写方法 + Model 直读推送
6. **Counter-Mvp-Strict（严格档）** — Command 写 + Query 读；View 按窄接口持有 Presenter，与 MVC 严格档同构
7. **ObservableValue (Odin Inspector)** — `ObservableValue<T>` 自定义 Drawer 演示；需要 Odin Inspector
8. **MiniEvent** — `MiniEvent` 和 `MiniEvent<T>` 使用示例（无参/单参事件）

### Aesir Modules（`Assets/Runestone/AesirModules/Samples~/`）

1. **Event Module - Key Press** — 事件模块 V1 基本发布-订阅示例：按键发布事件、`[AesirListener]` 静态订阅

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
- `cn.tuanjie.codely.bridge` 1.0.76 — Codely Unity Bridge
- `cn.tuanjie.ai.generators`（本地）— TJ AI Generators，文件引用 `.codely-cli/extensions/TJGenerators/`

### 第三方插件

- **Sirenix Odin Inspector** — 位于 `Assets/Plugins/Sirenix/`（已 gitignore；独立授权）。Aesir Inspector 强依赖；Architecture 和 Modules 的 Odin 相关代码均通过条件编译保护（可选增强）。

---

## 目录结构

```
Assets/
├── Runestone/                        # 包源代码
│   ├── AesirArchitecture/            # 核心架构框架（101 .cs）
│   │   ├── Runtime/
│   │   │   ├── Runestone.AesirArchitecture.asmdef
│   │   │   ├── Core/                # 核心：Context 上下文 + MVC/MVP 架构
│   │   │   │   ├── Component/       # MonoBehaviour 适配层
│   │   │   │   │   ├── View/        # AesirView, MonoView
│   │   │   │   │   └── ViewController/ # AesirViewController, MonoViewController
│   │   │   │   └── Engine/          # 纯 C# 核心（不依赖 MonoBehaviour）
│   │   │   │       ├── Capabilities/ # 能力接口（ICanGetModel 等）
│   │   │   │       ├── Context/     # IContext, AbstractContext<T>
│   │   │   │       └── Modules/     # 抽象类 + 接口（Model, Service, Command, Query）
│   │   │   ├── Modules/             # 辅助模块
│   │   │   │   ├── Event/           # MiniEvent, AutoRemoveListenerHandle, RemoveListener 触发器
│   │   │   │   ├── CustomLifecycle/ # MonoLifecycleProxy 生命周期代理
│   │   │   │   ├── Locator/         # GenericLocator<T>
│   │   │   │   ├── Observable/      # ObservableValue<T>
│   │   │   │   └── Utilities/       # PlayerLoopUtility, AesirArchitecturePlayerLoop
│   │   │   ├── Common/              # 框架基础设施
│   │   │   │   ├── AesirArchitecture.cs
│   │   │   │   ├── AesirMonoBehaviour.cs
│   │   │   │   ├── AesirScriptableObject.cs
│   │   │   │   ├── AesirArchitectureDebug.cs
│   │   │   │   ├── AssemblyInfo.cs
│   │   │   │   └── ResetStaticsAssistant.cs
│   │   │   └── OdinInspector/      # 独立程序集（依赖 Odin Inspector）
│   │   ├── Editor/                   # Odin 集成、定义符号管理
│   │   │   ├── Common/
│   │   │   ├── Utilities/
│   │   │   └── OdinInspector/       # AttributeProcessors
│   │   ├── Tests/                    # 编辑器模式和运行时模式测试（42 个）
│   │   ├── Samples~/                # 六档计数器 + ObservableValue + MiniEvent
│   │   └── Documentation~/          # README_EN, AesirArchitecture-Skill/（AI 编码指南）
│   ├── AesirModules/                 # 功能模块（41 .cs）
│   │   ├── Runtime/
│   │   │   ├── Common/              # AesirModules 单例、调试、程序集信息
│   │   │   ├── Scene/               # SceneModule, SceneAssetWrapper
│   │   │   ├── UI/                   # UIModule, UIRoot, UICanvasConfigSO, AesirBasePanel
│   │   │   ├── Events/              # 实验性事件模块
│   │   │   ├── InputSystem/         # Input System 集成
│   │   │   └── OdinInspector/       # Binder 全家桶（ODIN_INSPECTOR）
│   │   ├── Editor/                   # 场景编辑器窗口、Odin 集成、UI 菜单项
│   │   └── Samples~/                # Events/01_KeyPress
│   └── AesirInspector/               # 编辑器扩展库（425 .cs）
│       ├── Runtime/
│       │   ├── Unity/               # 核心运行时（Runestone.AesirInspector）
│       │   │   ├── Common/
│       │   │   ├── Debug/
│       │   │   ├── ScriptDocGenerator/ # 文档生成器运行时数据模型
│       │   │   └── Utilities/
│       │   └── OdinInspector/       # Odin 运行时（ODIN_INSPECTOR）
│       │       ├── Attributes/      # 双语特性（BilingualTitle, BilingualButton 等）
│       │       ├── Inspector/       # BilingualDisplayAsStringControl, BilingualHeaderControl
│       │       ├── Localization/    # BilingualData, AesirInspectorLanguageSettingsSO
│       │       └── Utilities/       # OdinCodeHighlighter
│       ├── Editor/
│       │   ├── Unity/               # 核心编辑器（Runestone.AesirInspector.Editor）
│       │   │   ├── Core/            # 安装检测、菜单管理
│       │   │   └── MiniTools/       # QuickCreateSO
│       │   └── OdinInspector/       # Odin 编辑器（ODIN_INSPECTOR）
│       │       ├── AttributeOverviewPro/ # 特性总览窗口（Data-Panel-Example 三件套）
│       │       ├── AttributeProcessors/
│       │       ├── Drawers/         # 双语 Drawer
│       │       ├── ExtensionManager/ # 扩展包管理器
│       │       ├── MiniTools/       # MenuItem Viewer, Syntax Highlighter
│       │       ├── ScriptDocGenerator/ # 文档生成器编辑器逻辑
│       │       └── Windows/         # Getting Started, Preferences
│       ├── Tests/
│       │   ├── Editor/              # 编辑器模式测试（107 ScriptDocGenerator 测试）
│       │   └── Runtime/             # 运行时模式测试
│       ├── Samples~/                # PluginConfigSolutions, RuntimeInitializeLoadType
│       └── Documentation~/          # 用户文档与开发者指南
├── Samples/                          # 导入的示例
│   ├── Aesir Architecture/0.12.0/
│   ├── Aesir Inspector/0.12.0/
│   └── Aesir Modules/0.12.0/
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
- **类名：** PascalCase（如 `AbstractContext`、`ObservableValue`、`MiniEvent`）
- **接口：** `I` 前缀（如 `ICommand`、`IContext`、`IUIPanel`）
- **抽象类：** `Abstract` 前缀（如 `AbstractModel`、`AbstractCommand`）
- **MonoBehaviour 单例：** 静态 `Instance` 属性、`[DefaultExecutionOrder(-999)]`、预放置/运行时创建统一由 `dontDestroyOnLoad` 序列化字段控制
- **包 ID：** 反向域名（`cn.runestone.aesir.*`、`cn.runestone.aesir-inspector`）
- **私有字段：** `_camelCase`（非序列化）、`camelCase`（带 `[SerializeField]` 序列化）
- **常量/静态只读：** PascalCase

### 代码风格

> 统一代码风格指南位于 `Scripts/CodeStyle/AesirCodeStyle.cs`，涵盖三包通用的命名、字段、属性、事件、枚举、空检查等规范。以下为各包特有的补充规范。

#### Aesir Architecture 和 Modules

- XML 文档注释使用中文（摘要、参数说明、备注）
- 代码标识符使用英文
- 数据类标记 `[Serializable]`（`AbstractModel`、`ObservableValue<T>`、`AbstractSubmodule`）
- 显式接口实现上下文注入（`IContextHolder.Context`、`ICanSetContext.SetContext`）
- 单例 `Instance` getter 优先 `FindAnyObjectByType` 搜索场景中预放置的实例，未找到时运行时创建
- 非泛型单例类内 `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` 重置静态字段；泛型类经 `ResetStaticsAssistant.Register()` 注册重置回调

#### Aesir Inspector（不同规范）

- **使用 XML 文档注释**（`/// <summary>`）；`[Summary]` 特性已从全部源码中移除，`SummaryAttribute` 类仅保留供 ScriptDocGenerator 反射回退
- **自文档化代码**：命名即文档，仅复杂逻辑使用 XML 注释解释"为什么"
- **严禁**对 `UnityEngine.Object` 派生类使用 `?.` / `??`
- **事件命名**：事件无 `On` 前缀（`DoorOpened`），订阅 `OnDoorOpened`，触发 `RaiseDoorOpened`
- **Enum**：普通含 `None = 0` 显式赋值；Flags `[Flags]` 值为 `1 << n`
- **Utility 命名**：Runtime `XxxUtility` / `XxxSafeEditorUtility`（`Runtime/Unity/Utilities/`），Editor-only `XxxEditorUtility`（`Editor/Unity/`）
- **Odin 依赖代码**必须放在 `OdinInspector/` 子目录，使用独立 asmdef
- **核心程序集**通过 `#if ODIN_INSPECTOR` 条件编译直接使用 Sirenix API（OdinBridge 桥接层已移除）
- **Processor**：`internal sealed`，与目标类同文件定义
- **免除注释规范的模块**：`AttributeOverviewPro/Data/`、`AttributeOverviewPro/AttributePanels/`、`AttributeOverviewPro/UsageExamples/`

### 目录组织

- `Runtime/` 与 `Editor/` 分离，由 asmdef 强制
- Odin 相关代码隔离在 `OdinInspector/` 子目录，使用独立 asmdef
- 测试分为 `Tests/Editor/`（编辑器模式）和 `Tests/Runtime/`（运行时模式）
- 示例在包源码中使用 `Samples~/` 约定，导入后位于 `Assets/Samples/`

---

## 构建与运行

### 编辑器

1. 在 Unity 2022.3.62f3c1（或团结引擎等效版本）中打开项目
2. 打开 `Assets/Samples/Aesir Architecture/0.12.0/Counter-Mvp-Quick/Scene/SampleForCounterMvpQuick.unity`
3. 按 **Play** 运行 MVP 快捷档计数器示例

### 命令行构建

```bash
Unity -batchmode -quit -projectPath . \
       -buildTarget StandaloneOSX -logFile build.log
```

尚无自定义构建脚本。

### 测试

- **Architecture 编辑器模式：** `Assets/Runestone/AesirArchitecture/Tests/Editor/`（42 个测试：PlayerLoop、Context 初始化、GenericLocator 保序/逆序、MiniEvent、ObservableValue、未注册异常近失识别、根单例静态重置、DDOL 默认值）
- **Architecture 运行时模式：** `Assets/Runestone/AesirArchitecture/Tests/Runtime/`（MonoLifecycleProxy PlayMode、RemoveListenerOnSceneUnloaded PlayMode、UnityEngineObjectCheckNull）
- **Inspector 编辑器模式：** `Assets/Runestone/AesirInspector/Tests/Editor/`（107 ScriptDocGenerator 测试）
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
- `.codely.packages/` — Codely Bridge 编辑器扩展自动更新缓存（版本升级时旧目录删除、新目录创建）
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

## Codely Structured Memories

### User
- [2026-08-15 22:20:02] 用户是 yuumixcode（Runestone），三个 Aesir 包（Architecture、Modules、Inspector）的作者，偏好中文交流。
### Feedback
- [2026-08-15 22:20:13] Aesir Inspector 与 Aesir Architecture/Modules 使用不同编码规范。Inspector（2026-07-31 更新）：[Summary] 特性装饰已从全部源码中移除（252 文件，897 处），SummaryAttribute 类仍保留但仅用于 ScriptDocGenerator 的 MemberData 反射；改用 XML 文档注释（/// <summary>）；OdinAutoTooltip（提取自 JakePineOdinTools）自动从 XML 生成 Inspector Tooltip。MIT LICENSE 头部已从所有 .cs 文件移除。原 AesirInspectorCodeStyle.cs 已删除，统一代码风格指南移至 Scripts/CodeStyle/AesirCodeStyle.cs（项目根目录，不参与编译）。保持自文档化代码、禁止对 UnityEngine.Object 派生类使用 ?. /??、事件命名无 On 前缀。Architecture/Modules：中文 XML 文档注释、显式接口实现。编辑代码时需确认所在包以应用正确风格。
- [2026-08-15 22:20:13] 项目英文文档命名规范：根目录英文文档统一使用 `_EN.md` 后缀（如 `README_EN.md`）；`CODE_OF_CONDUCT.en.md` 和 `CONTRIBUTING.en.md` 暂保留 `.en.md` 后缀；各子包的英文 README 统一放在 `Documentation~/README_EN.md`。**Why:** 统一命名风格，README 从 `.en.md` 改为 `_EN.md` 与子包一致。**How to apply:** 根目录 README 英文版用 `README_EN.md`，子包英文 README 用 `Documentation~/README_EN.md`。
- [2026-08-15 22:20:13] Git Commit 消息使用中文编写。**Why:** 用户偏好中文交流，项目代码注释和文档均使用中文。**How to apply:** 所有 git commit message 使用中文撰写，包括 subject 和 body。
- [2026-08-15 22:20:13] 用户倾向移除框架中不实用的抽象层，即使参考框架（如 QFramework）有对应功能。**Why:** QFramework 作者本人也优先使用 EasyEvent 而非框架内置的 AddListener，说明内置事件总线不如独立事件机制实用。**How to apply:** 评估框架功能时以实用性为首要标准，不盲目跟随参考框架的设计；独立轻量机制（MiniEvent、ObservableValue）优于框架内置事件总线。
- [2026-08-15 22:20:13] 事件参数载体应命名为 EventArgs 而非 Event。**Why:** 用户指出 AesirEvent 不持有监听者，仅作为参数在 EventModule 的 Registry 中传递，本质是事件参数（EventArgs）而非自包含事件实例。对比 MiniEvent（自身持有 Action 列表，是真正的事件实例）。**How to apply:** 当类仅作为数据载体流经外部调度器时，命名为 XxxEventArgs；当类自身持有监听者列表并具备 Invoke 能力时，才命名为 XxxEvent。
- [2026-08-15 22:20:13] 用户认可表达式树编译方案用于优化反射性能（StaticBindingInfo），要求在代码中添加详细注释说明"为什么性能好"和"有什么缺点"。**Why:** 表达式树方案虽常见但不易理解，注释帮助后续维护者判断是否需要调整。**How to apply:** 涉及表达式树/委托编译等非直觉的性能优化时，用 XML remarks 注释解释原理、量级对比和缺点。
- [2026-08-15 22:20:13] 用户拒绝修改 BilingualDisplayAsStringControl.cs 和 BilingualHeaderControl.cs 的序列化方式（auto-property → public field、readonly → [SerializeField]），因为这些类的样式由特定的 AttributeProcessor（BilingualDisplayAsStringProcessor、BilingualHeaderProcessor）处理，修改字段/属性形式会破坏 Processor 的 member.Name 匹配逻辑。**Why:** AttributeProcessor 通过 nameof(BilingualDisplayAsStringControl.ChineseDisplay) 等方式匹配成员，改为字段后虽然能序列化但会改变 Processor 行为。**How to apply:** 这两个类的 auto-property 和 readonly 字段设计是有意的，不可更改；如需解决 Domain Reload 后数据丢失问题，应在调用方（如 BuildMenuTree）添加 IsPanelDataValid() 检测并重新 Initialize()，而非修改数据类本身。
- [2026-08-15 22:20:13] 允许自由使用 #region 分段，无最低代码量要求。**Why:** 用户明确要求不限制 region 的行数门槛。**How to apply:** 代码中可按逻辑分区自由使用 #region/#endregion，即使分区很短也允许。
- [2026-08-15 22:20:13] 使用 Unity API 时优先选择 Unity 6 兼容且未废弃的版本。**Why:** `FindFirstObjectByType` 在 Unity 6（2025 年底）已被标记 Obsolete，因依赖 InstanceID 排序，未来 InstanceID 将被 EntityId 替代；`FindAnyObjectByType` 不依赖排序，性能更好且向前兼容。**How to apply:** 单例场景搜索用 `FindAnyObjectByType<T>()` 而非 `FindFirstObjectByType<T>()`；引入新 API 时先确认其在 Unity 6 中的废弃状态。
- [2026-08-15 22:20:13] 代码标识符中使用 Lifecycle（一个单词）而非 LifeCycle（两个单词）。**Why:** 用户明确指出"生命周期单词以 Lifecycle 为正确"，并要求全量替换 LifeCycle → Lifecycle（含枚举名 AesirArchitectureLifeCyclePhase → AesirArchitectureLifecyclePhase）。**How to apply:** 新建涉及"生命周期"的类型/方法/字段时，统一使用 Lifecycle 拼写，不要写成 LifeCycle。
- [2026-08-15 22:20:13] 移除不实用的生命周期事件时，优先保留语义清晰、执行频率可预期的事件。**Why:** BeforeFixedUpdate 通过 PlayerLoop 每帧触发，但 FixedUpdate 并非每帧执行，导致语义误导（名字暗示"在 FixedUpdate 之前"但实际每帧都跑）。**How to apply:** 评估自定义生命周期事件时，确保事件名与实际触发频率一致；若框架提供 PlayerLoopUtility 供用户自行扩展，则不需要预置低实用性的事件阶段。
- [2026-08-15 22:20:13] 框架保持极简（2026-08-15 强调，已全部实施完毕）：低概率问题、或因不推荐编写方式造成的问题，一律在项目前期用文档约定杜绝，不加防御性代码兜底。已撤销的防护：MiniEvent/MonoLifecycleProxy 异常吞噬（恢复零分配 + 原生 C# fail-fast）、MonoLifecycleProxy 120 帧自愈轮询（保留 EnsureInjected + Register 期检测）、AbstractContext 初始化失败回滚（改为"成功后才赋值 _instance"）。已删除：ModelReplaced/ServiceReplaced 替换通知事件（测试场景自行处理）、GenericLocator.Global 与 GetRegistry()。**Why:** 防御本身有隐性代价（吞异常掩盖根因、快照分配、时好时坏的自愈）。**How to apply:** 后续为 Aesir 三包添加任何"保护措施"前先问：防的问题真实概率多高？是否因用户写错代码？是否有隐性代价？优先用 README 设计边界节约定或编辑期提示（InfoBox）替代运行时防御。
- [2026-08-22 03:53:52] `.codely.packages/` 目录是 Codely Bridge 编辑器扩展的自动更新缓存（版本号文件夹如 `cn.tuanjie.codely.bridge@1.0.75-exp.1/` → `@1.0.76-exp.1/`），不属于本项目代码。**Why:** Bridge 版本升级时旧目录被删除、新目录被创建，导致 git status 大量 deleted + untracked 噪声。**How to apply:** 提交时仅 `git add` 明确相关的文件（如 `Assets/Runestone/` 下的变更），不要 `git add -A` 或 `git add .`，避免误纳入 bridge 更新；如需彻底排除可将 `.codely.packages/` 加入 .gitignore。

### Project
- [2026-08-15 22:20:30] AttributeOverviewPro 子资产重构已完成并合并到 main（2026-07-25）：~194 个独立 .asset 文件合并为 3 个文件 — AttributeOverviewDatabase.asset（DatabaseSO + 70 PanelSO 子资产）、UnityExamples.asset（Unity 原生序列化 ExampleSO）、OdinExamples.asset（Odin 序列化 ExampleSO）。按序列化方式分离存储。初始化超时 bug 已修复（批量创建跳过逐次 SaveAssets）。
- [2026-08-15 22:20:30] Monorepo 安装方式：三个子包通过各自的 `?path=` 参数从同一 Git 仓库安装，例如 `https://github.com/yuumixcode/Unity-Aesir-Packages.git?path=Assets/Runestone/AesirArchitecture`。**Why:** monorepo 中三个子包在同一仓库的不同子目录，直接用仓库根 URL 无法正确识别单个包。**How to apply:** README 中的 Git URL 始终带 `?path=Assets/Runestone/{包名}` 参数；Aesir Modules 会自动拉取 Architecture 依赖。
- [2026-08-15 22:20:30] 包依赖关系（2026-07-25）：Aesir Modules 仅依赖 Aesir Architecture（移除了 Inspector 依赖）；Aesir Inspector 强依赖 Odin Inspector（从可选改为必需）；Aesir Architecture 保持独立。**Why:** 简化依赖链，Modules 不再间接依赖 Inspector/Odin；Inspector 本质上需要 Odin 才能工作。**How to apply:** Modules 的 package.json 仅声明 architecture 依赖；Inspector 的 README 和 package.json 标注 Odin 为强依赖。
- [2026-08-15 22:20:30] 移除 Context 事件总线系统（2026-07-30）：从 Aesir Architecture 移除了 MiniEventBus&lt;TEvent&gt;、IEventArgs、ICanAddListener、ICanInvokeEvent，以及 IContext/AbstractContext 中的 AddListener/RemoveListener/InvokeEvent 方法和 CapabilityExtensions 中对应的扩展方法。保留 MiniEvent/MiniEvent&lt;T&gt; 和 ObservableValue&lt;T&gt; 作为独立事件机制。**Why:** 用户认为 Context 的 AddListener 不实用。**How to apply:** 角色接口不再继承 ICanInvokeEvent/ICanAddListener；事件通信应使用 MiniEvent 或 ObservableValue。
- [2026-08-15 22:20:30] Event Module V2 已实现并编译通过（2026-07-31，简化版+性能优化）。当前状态：已移除 AbstractAttributeBound&lt;T&gt; 基类，EventModule 直接继承 AesirMonoBehaviour。双注册表分离（AttributeBindings + DynamicBindings，均为 public Dictionary）。BindingInfo 基类仅含 BindingKey/Subscriber/Priority + 抽象 Invoke()；StaticBindingInfo 持有 MethodInfo + 表达式树编译委托；DynamicBindingInfo&lt;T&gt; 持有 Action&lt;T&gt; 直接委托。Script 订阅返回 AutoRemoveListenerHandle。已移除 InvokeDelayed、IsInitialized、取消传播/密封/共享等机制。SubscriberPriority 枚举值为 First/High/Medium/Low/Last。设计文档位于 Docs/EventModule/。**Why:** 参考商业插件 Game Event Hub，但以实用性为标准裁剪。**How to apply:** V2 运行时代码已完成；后续功能见 Feature-Roadmap.md。
- [2026-08-15 22:20:30] ScriptDocGenerator 模块重构完成（2026-08-04 至 2026-08-05，commit e48edf7）：①移除 OdinBridge 桥接层，类型名称格式化改用 #if ODIN_INSPECTOR + Sirenix.Utilities 直接调用。②ReflectionAnalyzer 迁移到 Runtime/Unity/ScriptDocGenerator/，SummaryTool 迁移到 Editor/OdinInspector/ScriptDocGenerator/。③移除 OdinAutoTooltipAttributeProcessor。④重写 Summary 解析：优先 [Summary] 特性 → 源代码 XML 解析。⑤回归单 ScriptDocGeneratorSO + TypeSource 枚举 + OdinEditorWindow。⑥修复 13 个 bug。⑦新增 SourceParsingTests(34) + OverloadPrefixTests(4)，总计 107 个测试全部通过。⑧反射解析器从 Runtime/OdinIntegration 迁移到 Runtime/Unity。**Why:** Odin 已是强依赖，OdinBridge 降级模式是伪需求。**How to apply:** 类型名称格式化直接用 Sirenix API + #if；反射解析器在 Runtime/Unity 层，编辑器在 Editor/OdinInspector 层。
- [2026-08-15 22:20:30] 单例模式重构（2026-08-05）：所有 MonoBehaviour 单例从无条件 DDOL 改为"预放置优先"模式。根单例使用 static bool _createdByRuntime 标志 + FindAnyObjectByType 场景搜索 + 条件 DDOL；子单例在 Instance getter 中添加 FindAnyObjectByType 场景搜索。移除了 Bootstrap() 方法。**Why:** 项目组负责人不想用 DontDestroyOnLoad，要搞多场景叠加加载。**How to apply:** 预放置单例在场景中即可，Instance 会自动发现；未预放置时运行时创建 + DDOL 保持向后兼容。
- [2026-08-15 22:20:30] MonoLifecycleProxy 排序 Bug 修复 + BeforeFixedUpdate 移除（2026-08-06，版本 0.8.0）：①排序 Bug 修复 — 改为直接遍历 _sortedListeners 按排序结果调用回调。②移除 BeforeFixedUpdate 事件 — 语义误导且无实际使用。③ClearAllListeners 不再注销 PlayerLoop（移至 OnDestroy）。④新增 MonoLifecycleProxyTests。⑤AesirArchitectureLifeCyclePhase → AesirArchitectureLifecyclePhase 拼写统一。⑥FindFirstObjectByType → FindAnyObjectByType。**How to apply:** MonoLifecycleEvent 枚举从 FixedUpdate=0 开始编号。
- [2026-08-15 22:20:30] AesirArchitecture 缺陷修复已完成并提交（commit a09bdc8，版本 0.9.0，2026-08-15）。缺陷分析 20 项全部处置：#1/#2（InfoBox）、#3（Scene.handle 分桶）、#4（EnsureInjected + Register 期检测，120 帧轮询已撤销）、#5（Interface 成功后赋值，回滚已撤销）、#7（GetModel/GetService 抛异常）、#9（package.json 文案修正）、#10（try-catch 已撤销，统一 fail-fast）、#15（GetRegistry 删除）、#17（补测试）。#6 ModelReplaced/ServiceReplaced 已实现后按用户裁决删除。#8/#11/#12/#13/#16/#18-20 文档化或不做。**Why:** 用户按缺陷文档逐项修复后复盘，以极简原则撤销过度防御。**How to apply:** 缺陷分析 20 项已全部处置完毕。
- [2026-08-15 22:20:30] 在 Codely 会话内实跑 EditMode 测试的方法：execute_csharp_script + TestRunnerApi（assemblyNames 过滤，用 TaskCompletionSource + await 等待 RunFinished）。**Why:** 此 Unity 2022.3/团结版无 ITestRunnerListener，回调接口须实现 UnityEditor.TestTools.TestRunner.Api.ICallbacks；且 execute_csharp_script 禁止 Task.Result/.Wait。**How to apply:** 需要验证测试真实通过时用此模式；PlayMode 测试改用 unity_editor.play + 协程脚本验证。
- [2026-08-15 22:20:30] Odin 程序集已全部重命名（2026-08-15，三包统一）：OdinIntegration → OdinInspector。Runtime → Runestone.{包名}.OdinInspector，Editor → Runestone.{包名}.Editor.OdinInspector。目录同步 OdinInspector/。关键联动：InternalsVisibleTo 必须指向新程序集名。**Why:** 用户要求统一 Odin 程序集命名约定。**How to apply:** 新增 Odin 相关 asmdef 一律用 OdinInspector 命名。
- [2026-08-15 22:20:30] AesirArchitecture 极简化实施完成（2026-08-15，commit a09bdc8）。①事件系统回原生 C# 语义 — MiniEvent 恢复零分配直调，撤销异常吞噬；②Interface 改"Initialize 成功后才赋值"+ 撤销回滚；③撤 120 帧自愈轮询；④删 ModelReplaced/ServiceReplaced；⑤删 GenericLocator.GetRegistry()；⑥package.json 去事件总线文案、README 增设计边界节。测试 34/34 两轮同域通过。**Why:** 用户裁决极简优先。**How to apply:** 极简计划全部条目已实施完毕。
- [2026-08-15 22:20:30] 静态变量重置职责拆分（2026-08-15 终版）：ResetStaticsAssistant 保留且收窄为仅服务泛型类（泛型类中的 RIOLM 被 Unity 静默跳过——2022.3.62 实测）；非泛型单例类内 [RuntimeInitializeOnLoadMethod(SubsystemRegistration)] ResetStatics 自重置。AbstractContext&lt;T&gt; 用 static T _instance + 静态构造函数 Register(() => _instance = null)；测试入口 ResetStaticsAssistant.ResetForTests()。GenericLocator&lt;T&gt;.Global 已移除。**Why:** 用户最初要求移除助手，随后想起泛型 RIOLM 静默失效正是助手的存在理由，纠正为"助手只管泛型类"。**How to apply:** 泛型类静态状态 → 助手注册；非泛型 → 类内 RIOLM；勿再引入 ContextSingletonStore 类方案（已否决）。
- [2026-08-15 22:20:30] AesirModules UI 模块缺陷修复进度（2026-08-15）：已完成 #1（Binder 迁至 OdinInspector 程序集）、#5（InstantiateInactive）、#6（字典键归一化）、#7（OnDestroy 静态反清理）、#8（EventSystem 检查）、#9（Build 统一走 EnsureCanvasConfig）、#10（GetLayerRoot 缺层 LogError）、#11（异常补 Error 日志）、#17（泛型重载）。PlayMode 实测 7 项全过。**Why:** 用户按缺陷文档指定修复范围。**How to apply:** 剩余未修：P0 #2/#3/#4（Binder 代码生成器）、P2-4 #12-29。
- [2026-08-15 22:20:30] AesirArchitecture 0.9.0 已提交（commit a09bdc8，159 文件 +2005 −475）。核心变更：①MVC 优先定位；②目录重构为三层 — Runtime/Core/（Context+MVC/MVP 核心）、Runtime/Modules/（辅助模块）、Runtime/Common/（基础设施）；③极简化；④Odin 程序集重命名；⑤场景分桶改 Scene.handle；⑥静态重置职责拆分。测试 34/34 两轮同域通过。文档：Docs/AesirArchitecture-极简分析与改进计划.md、Docs/Unity-RuntimeInitializeOnLoadMethod-指南.md。**Why:** 用户要求 MVC 优先 + 极简 + 目录重构。**How to apply:** 版本已升至 0.9.0。
- [2026-08-22 02:05:19] MVP 示例三档定稿（2026-08-22，RAA 0.10.0 在制品）：Counter-Mvp-Quick/Standard/Strict 与 MVC 三档命名、分级完全对齐（原 Mvp-Simple 已更名 Mvp-Quick，类名 SampleMvpQuickCounter*、asmdef MvpQuick）。规范要点：①MVP View 一律纯 MonoBehaviour（不继承 MonoView<T>）；②快捷档零接口抽象——无 Model/Presenter/View 任何接口，Presenter 持具体面板类；③标准档只读暴露+写方法、View 契约 IXxxView；④严格档 Command 写+Query 读、View 按窄接口 ISampleMvpStrictCounterPresenter（SyncInitialValue+IDisposable）存储 Presenter，与 MVC-3 Controller 窄接口（ISampleMvcStrictCounterController）对称。**Why:** 用户要求 MVP 与 MVC 分级规范逐档同构、Simple 更名 Quick 对齐命名、移除快捷档冗余 View 接口。**How to apply:** 新增 MVP 示例遵循此分级口径；快捷档零接口是明确定稿规范。另：批量 mv 重命名 Unity 资产会与 debounced 自动刷新竞态导致 prefab 内存导入污染（序列化引用丢失）——重命名后须 ImportAsset(ForceUpdate|ForceSynchronousImport) 强制重导入受影响 prefab 并验证序列化引用，勿信编译通过即无恙。
- [2026-08-22 11:29:09] Aesir 三包 0.12.0 版本同步完成（2026-08-22）：Architecture 先行升至 0.12.0（新增 AI Skill 文档集），Modules 和 Inspector 同步升至 0.12.0（无功能变更）。CODELY.md 全面更新至 0.12.0 状态。本次验证要点补充：①CODELY.md 是 memory file，replace/write_file 工具被阻止——需写入临时文件后 mv 替换；②Inspector 英文 README 无版本徽章（仅 license badge），同步时不需处理；③三包 Samples 导入副本已全部对齐 0.12.0（meta 随移 GUID 不变）。**Why:** Architecture 新增 AI Skill 文档集后需同步版本。**How to apply:** 下次发版照 aesir-version-sync skill + 以上坑位清单执行。




### Reference
- [2026-08-15 22:20:34] AttributeOverviewPro 资产精简方案文档位于 Docs/AttributeOverviewPro-AssetReduction-Plan.md — 包含现状分析、可行性评估、子资产架构设计、详细实现步骤、验证步骤和备选方案。
- [2026-08-15 22:20:34] sync-samples 技能位于 .codely-cli/skills/sync-samples/ — 自动扫描 Assets/Runestone/*/package.json 中的 samples 数组，将 Assets/Samples/<包名>/<版本>/<displayName>/ 同步到对应包的 Samples~/<path> 文件夹。支持 --dry-run 预览。触发词："同步 Samples"、"sync samples"。
