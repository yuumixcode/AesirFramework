# CODELY.md — AesirFramework

## 项目概览

**AesirFramework**（原 Unity-Aesir-Packages）是 **Runestone Yuumix** 开发的 Unity/团结引擎框架集合，提供渐进式 MVC 架构框架与功能模块。

- **Unity 版本：** 2022.3.62f3c1
- **渲染管线：** URP（Universal Render Pipeline 14.0.12）
- **许可证：** MIT
- **作者：** [yuumixcode](https://github.com/yuumixcode)
- **语言：** C#（代码注释和 XML 文档使用中文，代码标识符使用英文）
- **代码规模：** 约 210 个 .cs 文件（含 Samples 与 Samples~ 双份示例源），分布在 2 个包中（Architecture 约 168、Modules 约 44）
- **仓库定位：** 纯 Unity 工程仓库 — Codely 本地工具链（`.codely-cli/`、`.codely.packages/`、`.codely/`、`.codelyignore`）已全部 gitignore，仅 `CODELY.md` 概述文档保留入库

### 包列表

| 包名 | 包 ID | 版本 | 命名空间 | 说明 |
|------|------|------|---------|------|
| Aesir Architecture | `cn.runestone.aesir.architecture` | 0.14.0 | `Runestone.AesirArchitecture` | 渐进式 MVC 架构框架 — 能力接口组合、命令/查询模式、轻量事件（MiniEvent）与响应式属性（ObservableValue）、PlayerLoop 生命周期、纯 C# 架构根 + MonoBehaviour 适配层 |
| Aesir Modules | `cn.runestone.aesir.modules` | 0.14.0 | `Runestone.AesirModules` | 功能模块 — 轻量级 UI 框架（Manager-of-Managers 单例、四层 Canvas 层级、面板生命周期、可替换资源加载器）+ 实验性事件模块 |

> **Aesir Inspector 已独立**：迁出为独立公开仓库，定位为专门面向 Odin Inspector 开发者的学习工具包，不再随本仓库分发。

### 依赖关系

- **Aesir Architecture** — 不依赖任何 Aesir 子包，可独立安装
- **Aesir Modules** — 依赖 `cn.runestone.aesir.architecture`（0.14.0）
- **Aesir Inspector** — 独立公开仓库，与本仓库无依赖关系

---

## Aesir Architecture（0.14.0）

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
- **`ObservableList<T>` / `ObservableDictionary<TKey,TValue>`** — 可观察集合；只读接口为不变型（结构体事件参数与协变冲突 CS1961）
- **`MiniEvent` / `MiniEvent<T>`** — 轻量级零分配事件系统（直接多播调用）；返回 `AutoRemoveListenerHandle` 自动清理。异常语义 = 原生 C# 事件（fail-fast）
- **`MonoLifecycleProxy`** — 生命周期代理，将 Unity 原生回调统一为可订阅的 MiniEvent；调用期增删监听为**快照语义**（`_invoking` 标志 + `_pendingChanges` 挂起队列，趟末按发生顺序应用，对齐原生多播委托）
- **`GenericLocator<T>`** — 类型键控的服务定位器（保序注册/查询）
- **`AesirArchitecturePlayerLoop`** — PlayerLoop 注入；`EnsureInjected()` 公开 API + `Register` 期自动检测
- **`ResetStaticsAssistant`** — 仅服务泛型类的静态重置（泛型类 RIOLM 被 Unity 静默跳过）

### 渐进式示例家族（六档）

| 档位 | 示例 | Model 暴露面 | 读写路径 | View 边界 |
|------|------|-------------|---------|-----------|
| MVC-1 快捷 | Counter-Mvc-Quick | 具体类，可写 ObservableValue | View 兼 Controller 直写直读 | `MonoViewController<T>` |
| MVC-2 标准 | Counter-Mvc-Standard | 具体类，只读暴露 + 写方法 | Controller 直调写方法 | View 与 Controller 分离 |
| MVC-3 严格 | Counter-Mvc-Strict | 接口注册，只读暴露 + 写方法 | Command 写 + Query 读 | View 按窄接口持有 Controller |
| MVP-1 快捷 | Counter-Mvp-Quick | 具体类，可写 ObservableValue | Presenter 直写 + 推送 | 纯 MonoBehaviour，零接口 |
| MVP-2 标准 | Counter-Mvp-Standard | 具体类，只读暴露 + 写方法 | Presenter 直调写方法 | View 契约 `IXxxView` |
| MVP-3 严格 | Counter-Mvp-Strict | 接口注册，只读暴露 + 写方法 | Command 写 + Query 读 | View 按窄接口持有 Presenter |

### Samples 双目录结构（0.14.0 起）

- **`Samples/`**（编写主位）— 仓库内直接可见、可运行；每个示例目录含独立 asmdef、Scripts/Scene/Prefab
- **`Samples~/`**（发布镜像）— Git URL 安装后经 Package Manager → Samples 标签页按需导入；内容由 `Samples/` 同步而来，两份保持一致
- 同步方向：**先编写 `Samples/`，再同步到 `Samples~/`**（含 .meta，GUID 与 UPM 导入链路一致）
- **构建剔除** — 全部示例程序集 `includePlatforms: ["Editor"]`，示例脚本不进构建；示例内无 Resources 目录、无构建场景引用，资产亦不入包
- **命名空间规范** — `Runestone.AesirArchitecture.Samples.<示例名>`（MvcQuick / MvcStandard / MvcStrict / MvpQuick / MvpStandard / MvpStrict / PlaneWarMono）；`MiniEvent` 与 `ObservableValue` 两示例因命名空间段与所演示的框架类型同名冲突（CS0118），保留前缀 `Runestone.AesirArchitecture.Samples`

### 设计边界（极简原则）

- **事件总线 / EventChannel** — 不做；跨模块通信使用互相 GetModel + ObservableValue 订阅，或直接引用 MiniEvent
- **Context 多实例** — 不做；CRTP 泛型单例，多存档/多房间在业务层建模
- **Command/Query 池化、async、Undo/Redo** — 不做；保持同步、无缓存
- **View 生命周期脚手架** — 不做；面板生命周期由 Aesir Modules 的 UIModule 负责
- **线程安全** — 不做；仅保证主线程使用
- **监听回调不应抛异常** — 框架约定（fail-fast），异常直接向上传播由 Unity 记日志
- **`Configure()` 中禁止访问 `Instance`** — 会递归创建第二个上下文实例
- **`Register` 与 `Get` 必须使用相同类型参数** — 按键精确匹配
- **运行时替换 Model/Service 仅用于测试调试** — 旧实例被 Dispose，其上的订阅不会迁移（会输出 Warning 日志）
- **第三方 SDK 修改 PlayerLoop 后手动调用 `AesirArchitecturePlayerLoop.EnsureInjected()`** — `Register` 注册回调时会自动检测补插
- **写入纪律档位** — 快捷/标准档表现层可直写 Model；标准档起表现层写入必经 Command；严格档只读 + 写方法；Service 可直写
- **示例不进构建包** — Samples 程序集一律 Editor-only（见 Samples 双目录结构节）

### Documentation 双目录结构（0.14.0 起）

- **`Documentation/`（编写主位）** — Assets 中可见、随 unitypackage 导出；两包各一份，存放 README_EN、教学文档、LICENSE/Third Party Notices 与 AesirArchitecture-Skill（AI 编码指南）
- **`Documentation~/`（UPM 镜像）** — Git URL 安装时包内的隐藏副本；内容无 .meta（UPM 不导入它），由 `Documentation/` 同步而来（同步时排除 .meta 与 .DS_Store）
- **同步方向** — 先编写 `Documentation/`，再同步到 `Documentation~/`
- **构建剔除** — Markdown 经 Unity 导入为 TextAsset，但无任何场景/资产引用，玩家构建自动排除；约定该目录只放文档，不放会被引用的资产
- **双目录动因** — 与 Samples 同理：`~` 目录对 AssetDatabase 不可见、不进 unitypackage，而 Release 下载与包内更新器的用户同样需要随包文档

### 包内更新器（Editor）

- **`AesirUpdateService` / `AesirUpdateWindow`** — 位于 `Editor/UpdateChecker/`（`Runestone.AesirArchitecture.Editor` 程序集），菜单 `Tools/Aesir/Check for Updates`
- 面向"代码导入 Assets/Runestone（非 UPM）"的用户：扫描 `Assets/Runestone/*/package.json` 识别本地安装，与 GitHub Releases 最新版（`releases/latest` API，仅点击时调用）比对版本
- 更新流程：下载 `<包目录名>-v<版本>.unitypackage`（Release 资产命名约定）→ 自动备份 `Assets/Runestone` 到项目根 `.aesir-backup/`（时间戳前缀命名保证排序即时间序，保留最近 3 份）→ 按"上次安装清单 − 新版清单"差集删除残留条目（仅限本包目录内，无历史清单则跳过，不误伤用户新增文件）→ `AssetDatabase.ImportPackage` 静默导入 → 逐包合并登记 `.aesir/installed-manifest.json`（更新中途域重载时已导入包的状态保证正确落盘）
- Release 资产 `files-manifest.json` 由 CI `build_unitypackage.py --manifest` 生成（数组结构，兼容 JsonUtility），随 unitypackage 一起发布；清单含目录条目，供空目录回收
- 设计边界：UPM（Git URL）安装的副本不在管辖内（不在 Assets 下，扫描不到），请用 Package Manager 更新；开发仓库（存在 `.git`）窗口显示警告勿更新——Release 内容会覆盖本地源码；实现参考 QFramework PackageKit（版本记录随包走 + 先删后导），增强点为自动备份与精确差集清理
- EditMode 单测：`Tests/Editor/AesirUpdateServiceTests.cs`（版本比较 / package.json 解析 / 清单差集 / 残留删除与空目录回收 / 备份裁剪 / 清单 JSON / Release 资产定位）

### Odin Inspector 集成

- 通过 `ODIN_INSPECTOR` 定义符号条件编译
- Odin 集成使用独立 asmdef：`Runestone.AesirArchitecture.OdinInspector`（Runtime）和 `Runestone.AesirArchitecture.Editor.OdinInspector`（Editor）
- 预放置实例风险通过 Odin AttributeProcessor 注入 Warning InfoBox
- DDOL 开关字段级 InfoBox — `[Tooltip]` 迁移为 AttributeProcessor 注入的 Info 级信息框（样式与逻辑分离，运行时程序集零 Inspector 样式特性）

### 自定义定义符号

- `AESIR_ARCHITECTURE` — 由 `EnsureAesirArchitectureDefine` 自动设置（注意：`SetScriptingDefineSymbols` 值未变化时不落盘，宏变更后需 grep `ProjectSettings.asset` 验证）
- `ODIN_INSPECTOR`、`ODIN_INSPECTOR_3`、`ODIN_INSPECTOR_3_1`、`ODIN_INSPECTOR_3_2`、`ODIN_INSPECTOR_3_3` — 由 Sirenix 管理
- `AESIR_INSPECTOR` — 已随 AesirInspector 迁出而移除（2026-09-05 已从 ProjectSettings.asset 全平台清除）

---

## Aesir Modules（0.14.0）

### UI 框架

- **`UIModule`** — 通过 `AesirModules.GetOrAddChild<T>()` 创建的 MonoBehaviour 单例，管理面板生命周期
  - 静态 API：`UIModule.Show<T>()`、`UIModule.Hide<T>()`、`UIModule.Get<T>()`、`UIModule.Prewarm<T>()`、`UIModule.RegisterPrefab<T>()`
  - 面板状态：激活 → 停用（缓存）→ 销毁
  - `PrewarmAll()` — 通过协程逐帧预实例化
- **`IUIPanel`** — 生命周期契约：`Initialize → Show(payload) → Hide → DestroyPanel`
  - `Layer`（UILayer）、`DestroyOnHide`（bool）、`IsOpen`（bool）
- **`AesirBasePanel`** — MonoBehaviour 面板基类；`OnDestroy` 静态反清理 `UIModule.RemovePanelRecord`
- **`UIRoot`** — 构建四层 Canvas 层级；`UILayer` 枚举定义层级；层 Canvas / UICamera / EventSystem 为 `[SerializeField]` 序列化引用持久化（`List<LayerCanvasEntry>`）
- **`IUIAssetLoader` / `ResourcesUILoader`** — 可插拔资源加载（默认 Resources；可替换为 Addressables）
- **`BinderAssistant` / `BinderTag`** — UI 元素组件绑定系统（OdinInspector 程序集）
- **`SceneModule`** — 场景管理模块

### 事件模块（实验性）

- EventModule V2：EventModule 直接继承 AesirMonoBehaviour；双注册表分离（AttributeBindings + DynamicBindings）
- `BindingInfo` 基类仅含 BindingKey/Subscriber/Priority + 抽象 `Invoke()`；`StaticBindingInfo` 持有 MethodInfo + 表达式树编译委托；`DynamicBindingInfo<T>` 持有 `Action<T>` 直接委托；Script 订阅返回 `AutoRemoveListenerHandle`
- `SubscriberPriority` 枚举：First/High/Medium/Low/Last
- 事件参数载体继承 `AesirEventArgs`，命名为 XxxEventArgs（仅作数据载体流经 EventModule 调度）
- 设计文档位于 Docs/EventModule/（私有仓库 Aesir-Docs）

### DDOL 机制

- `AesirModules`、`UIRoot`、`UIModule` 均有 `[SerializeField] bool dontDestroyOnLoad = true`，预放置/运行时创建统一由该字段控制
- `UIModule` 的字段仅在预放置为根物体时生效，运行时自动创建时挂载于 [Aesir Modules] 宿主下跟随宿主决策

### 场景编辑器

- `SceneManagerWindow` — 场景管理自定义编辑器窗口
- `BootstrapSceneHelper` — 场景引导工具
- `SceneAssetWrapper` — 可序列化场景资源引用

### Samples 双目录结构（0.14.0 起，与 Architecture 同规则）

- `Samples/`（编写主位）与 `Samples~/`（发布镜像）并存；示例目录 `Events/01_KeyPress`（与 package.json samples 路径一致）
- 示例程序集 `Runestone.AesirModules.Samples.Events.KeyPress` 为 Editor-only（构建剔除）；命名空间 `Runestone.AesirModules.Samples.Events.KeyPress`

---

## 程序集定义

### Aesir Architecture（16 个 asmdef）

| 程序集 | 路径 | 说明 |
|--------|------|------|
| `Runestone.AesirArchitecture` | Runtime/ | 核心运行时 |
| `Runestone.AesirArchitecture.OdinInspector` | Runtime/OdinInspector/ | ODIN_INSPECTOR |
| `Runestone.AesirArchitecture.Editor` | Editor/ | 编辑器（含 QuickCreateSOMenuItem、EnsureAesirArchitectureDefine） |
| `Runestone.AesirArchitecture.Editor.OdinInspector` | Editor/OdinInspector/ | ODIN_INSPECTOR |
| `Runestone.AesirArchitecture.Tests` | Tests/Runtime/ | PlayMode 测试（MonoLifecycleProxy 快照语义等） |
| `Runestone.AesirArchitecture.Tests.Editor` | Tests/Editor/ | EditMode 测试（83 个） |
| `Runestone.AesirArchitecture.Samples.MvcQuick` | Samples/Counter-Mvc-Quick/Scripts/ | Editor-only |
| `Runestone.AesirArchitecture.Samples.MvcStandard` | Samples/Counter-Mvc-Standard/Scripts/ | Editor-only |
| `Runestone.AesirArchitecture.Samples.MvcStrict` | Samples/Counter-Mvc-Strict/Scripts/ | Editor-only |
| `Runestone.AesirArchitecture.Samples.MvpQuick` | Samples/Counter-Mvp-Quick/Scripts/ | Editor-only |
| `Runestone.AesirArchitecture.Samples.MvpStandard` | Samples/Counter-Mvp-Standard/Scripts/ | Editor-only |
| `Runestone.AesirArchitecture.Samples.MvpStrict` | Samples/Counter-Mvp-Strict/Scripts/ | Editor-only |
| `Runestone.AesirArchitecture.Samples.MiniEvent` | Samples/MiniEvent/Scripts/ | Editor-only |
| `Runestone.AesirArchitecture.Samples.ObservableValue` | Samples/ObservableValue/Scripts/ | Editor-only |
| `Runestone.AesirArchitecture.Samples.PlaneWarMono` | Samples/PlaneWar/Scripts/Mono/ | Editor-only |
| `Runestone.AesirArchitecture.Samples.PlaneWarMono.Editor` | Samples/PlaneWar/Editor/ | 场景引用一键修复菜单 |

### Aesir Modules（6 个 asmdef）

| 程序集 | 路径 | 说明 |
|--------|------|------|
| `Runestone.AesirModules` | Runtime/ | 引用 AesirArchitecture |
| `Runestone.AesirModules.OdinInspector` | Runtime/OdinInspector/ | Binder 全家桶（ODIN_INSPECTOR） |
| `Runestone.AesirModules.Editor` | Editor/ | 场景编辑器窗口、UI 菜单项 |
| `Runestone.AesirModules.Editor.OdinInspector` | Editor/OdinInspector/ | ODIN_INSPECTOR |
| `Runestone.AesirModules.InputSystem` | Runtime/UI/InputSystem/ | UIRoot 输入模块替换 |
| `Runestone.AesirModules.Samples.Events.KeyPress` | Samples/Events/01_KeyPress/ | Editor-only |

---

## 场景

| 场景 | 路径 | 用途 |
|------|------|------|
| SampleScene | `Assets/Scenes/SampleScene.unity` | 默认 Unity 示例场景 |
| SampleForCounterMvcQuick | `Assets/Runestone/AesirArchitecture/Samples/Counter-Mvc-Quick/Scene/` | MVC 快捷档计数器示例 |
| SampleForCounterMvcStandard | `Assets/Runestone/AesirArchitecture/Samples/Counter-Mvc-Standard/Scene/` | MVC 标准档计数器示例 |
| SampleForCounterMvcStrict | `Assets/Runestone/AesirArchitecture/Samples/Counter-Mvc-Strict/Scene/` | MVC 严格档计数器示例 |
| SampleForCounterMvpQuick | `Assets/Runestone/AesirArchitecture/Samples/Counter-Mvp-Quick/Scene/` | MVP 快捷档计数器示例 |
| SampleForCounterMvpStandard | `Assets/Runestone/AesirArchitecture/Samples/Counter-Mvp-Standard/Scene/` | MVP 标准档计数器示例 |
| SampleForCounterMvpStrict | `Assets/Runestone/AesirArchitecture/Samples/Counter-Mvp-Strict/Scene/` | MVP 严格档计数器示例 |
| MiniEventSample | `Assets/Runestone/AesirArchitecture/Samples/MiniEvent/Scene/` | MiniEvent 使用示例 |
| ObservableValueInspector | `Assets/Runestone/AesirArchitecture/Samples/ObservableValue/Scene/` | ObservableValue Inspector 演示 |
| SampleForPlaneWarMono | `Assets/Runestone/AesirArchitecture/Samples/PlaneWar/Scene/` | PlaneWar 纵版射击（Mono 版）示例 |

> 示例场景均在包内 `Samples/` 目录（`Samples~/` 为镜像）；Git URL 安装的项目经 Package Manager 导入后位于 `Assets/Samples/<包名>/<版本>/`。

---

## 示例

### Aesir Architecture（包内 `Samples/`，镜像 `Samples~/`）

1. **Counter-Mvc-Quick（快捷档）** — `MonoViewController<T>` 直写直读，最少概念跑通数据驱动 UI 闭环
2. **Counter-Mvc-Standard（标准档）** — Model 只读暴露 + 写方法；View 与 Controller 分离共享 Model
3. **Counter-Mvc-Strict（严格档）** — Model 接口注册 + 只读暴露 + 写方法；Command 写 + Query 读；View 按窄接口持有 Controller
4. **Counter-Mvp-Quick（快捷档）** — Presenter 直改可写 ObservableValue（零接口抽象）并推送被动 View
5. **Counter-Mvp-Standard（标准档）** — Model 只读暴露 + 写方法；Presenter 直调写方法 + Model 直读推送
6. **Counter-Mvp-Strict（严格档）** — Command 写 + Query 读；View 按窄接口持有 Presenter，与 MVC 严格档同构
7. **ObservableValue (Odin Inspector)** — `ObservableValue<T>` 自定义 Drawer 演示；需要 Odin Inspector
8. **MiniEvent** — `MiniEvent` 和 `MiniEvent<T>` 使用示例（无参/单参事件）
9. **PlaneWar（Mono 版）** — 纵版射击飞机大战实战示例：得分 HUD、三型敌机、重开流程；命名空间 `Runestone.AesirArchitecture.Samples.PlaneWarMono`，`Tools → Aesir → PlaneWar → Fix Scene References` 一键修复引用；RAA 版（Scripts/Raa）待编写

### Aesir Modules（包内 `Samples/Events/01_KeyPress`）

1. **Event Module - Key Press** — 事件模块基本发布-订阅示例：按键发布事件、`[AesirListener]` 静态订阅

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

> manifest.json 不含任何本地 `file:` 引用（Codely 本地工具链已于 2026-09-05 清理出库）。

### 第三方插件

- **Sirenix Odin Inspector** — 位于 `Assets/Plugins/Sirenix/`（已 gitignore；独立授权）。Architecture 与 Modules 通过 `#if ODIN_INSPECTOR` 条件编译可选集成，未安装时自动排除。

---

## 目录结构

```
AesirFramework/
├── README.md / README_EN.md / LICENSE / CHANGELOG.md / CONTRIBUTING.md / CODELY.md
├── .github/workflows/auto-publish-branches.yml   # CI：main 推送时按包目录 subtree split 发布版本分支
├── Assets/
│   ├── Runestone/                     # 包源代码
│   │   ├── AesirArchitecture/         # 核心架构框架
│   │   │   ├── Runtime/
│   │   │   │   ├── Core/              # Context 上下文 + MVC/MVP 核心（Engine 纯 C# / Component 适配层）
│   │   │   │   ├── Modules/           # Event(MiniEvent) / CustomLifecycle(MonoLifecycleProxy) / Locator / Observable / Utilities
│   │   │   │   ├── Common/            # AesirArchitecture 单例、Debug、ResetStaticsAssistant
│   │   │   │   └── OdinInspector/     # 独立程序集（ODIN_INSPECTOR）
│   │   │   ├── Editor/                # 定义符号管理、QuickCreateSO、包内更新器、Odin AttributeProcessors
│   │   │   ├── Tests/                 # Editor 42 个 + Runtime PlayMode 测试
│   │   │   ├── Samples/               # 示例（编写主位，9 个，构建剔除）
│   │   │   ├── Samples~/              # 示例发布镜像（Package Manager 按需导入）
│   │   │   ├── Documentation/         # 文档主位（Assets 可见、随 unitypackage 导出、不进构建）
│   │   │   └── Documentation~/        # 文档镜像（Git URL 安装隐藏副本，无 .meta）
│   │   └── AesirModules/              # 功能模块
│   │       ├── Runtime/
│   │       │   ├── Common/            # AesirModules 单例、调试
│   │       │   ├── Scene/             # SceneModule, SceneAssetWrapper
│   │       │   ├── UI/                # UIModule, UIRoot, UICanvasConfigSO, AesirBasePanel（含 InputSystem/）
│   │       │   ├── Events/            # 实验性事件模块
│   │       │   └── OdinInspector/     # Binder 全家桶（ODIN_INSPECTOR）
│   │       ├── Editor/                # 场景编辑器窗口、Odin 集成、UI 菜单项
│   │       ├── Samples/               # 示例（编写主位，Events/01_KeyPress）
│   │       ├── Samples~/              # 示例发布镜像
│   │       ├── Documentation/         # 文档主位（Assets 可见、随 unitypackage 导出）
│   │       └── Documentation~/        # 文档镜像（无 .meta）
│   ├── Scenes/                        # SampleScene
│   ├── Settings/                      # URP 资源（UniversalRP, Renderer2D）
│   └── Plugins/Sirenix/               # Odin Inspector（已 gitignore）
└── Packages/manifest.json             # 无本地 file: 引用
```

---

## 开发规范

### 命名

- **命名空间：** `Runestone.AesirArchitecture`（含 `Runestone.AesirArchitecture.Samples.<示例名>` / `.Editor` / `.OdinInspector`）、`Runestone.AesirModules`（含 `Runestone.AesirModules.Samples.<示例名>`）
- **程序集名：** `Runestone.AesirArchitecture.*`、`Runestone.AesirModules.*`；示例程序集名与命名空间一致
- **类名：** PascalCase（如 `AbstractContext`、`ObservableValue`、`MiniEvent`）
- **接口：** `I` 前缀（如 `ICommand`、`IContext`、`IUIPanel`）
- **抽象类：** `Abstract` 前缀（如 `AbstractModel`、`AbstractCommand`）
- **MonoBehaviour 单例：** 静态 `Instance` 属性、`[DefaultExecutionOrder(-999)]`、`dontDestroyOnLoad` 序列化字段统一控制 DDOL
- **私有字段：** `_camelCase`（非序列化）、`camelCase`（`[SerializeField]`）
- **常量/静态只读：** PascalCase
- **Lifecycle 拼写：** 一个单词 Lifecycle（非 LifeCycle）
- **事件参数载体：** 仅作数据载体流经外部调度器 → `XxxEventArgs`；自身持有监听者并有 Invoke 能力 → `XxxEvent`

### 代码风格

> 统一代码风格指南位于根目录 `CodeStyle/AesirCodeStyle.cs`（不参与编译）。

- **XML 文档注释使用中文**（摘要、参数说明、备注）；代码标识符使用英文
- 数据类标记 `[Serializable]`
- 显式接口实现上下文注入（`IContextHolder.Context`、`ICanSetContext.SetContext`）
- 单例 `Instance` getter 优先 `FindAnyObjectByType` 搜索预放置实例（不用已废弃排序语义的 `FindFirstObjectByType`）
- 非泛型单例类内 `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` 重置静态字段；泛型类经 `ResetStaticsAssistant.Register()` 注册重置回调
- 允许自由使用 `#region` 分段，无最低代码量要求
- MonoBehaviour 运行状态字段一律用显式非序列化字段（自动属性 backing field 会被场景序列化残留）
- Odin 依赖代码隔离在 `OdinInspector/` 子目录 + 独立 asmdef；核心程序集经 `#if ODIN_INSPECTOR` 直接使用 Sirenix API
- Processor：`internal sealed`，与目标类同文件定义（Odin AttributeProcessor 通过 `nameof` 成员匹配，勿改其匹配的成员形式）

---

## 构建与运行

### 编辑器

1. 在 Unity 2022.3.62f3c1（或团结引擎等效版本）中打开项目
2. 打开 `Assets/Runestone/AesirArchitecture/Samples/Counter-Mvp-Quick/Scene/SampleForCounterMvpQuick.unity`
3. 按 **Play** 运行 MVP 快捷档计数器示例（其余示例场景见"场景"节）

### 命令行

```bash
# 编译预热
Unity -batchmode -quit -projectPath . -logFile build.log

# EditMode 测试
Unity -batchmode -projectPath . -testPlatform editmode -runTests \
       -testResults TestResults.xml -logFile test.log
```

尚无自定义构建脚本；示例程序集 Editor-only，玩家构建自动剔除示例。

---

## 版本控制

### 忽略（按 `.gitignore`）

- `Library/`、`Temp/`、`obj/`、`Build/`、`Builds/`、`Logs/`、`UserSettings/`
- `.codely-cli/`、`.codely.packages/`、`.codely/`、`.codelyignore`、`.com-unity-codely.json` — Codely 本地工具链（2026-09-05 起全部不入库）
- `Assets/Plugins/Sirenix/` — Odin Inspector（独立授权，不分发）
- `Docs/` — 开发文档（私有仓库 Aesir-Docs）
- `Assets/Vertical 2D Shooting BE4/` — 第三方付费素材原件（PlaneWar 示例内已有自包含拷贝）
- 生成的 IDE 文件（`*.csproj`、`*.sln`、`.vs/`、`.idea/`）、`*.unitypackage`、测试生成场景等

### 纳入版本控制

- `Assets/Runestone/` — 两个包源代码（含 `Samples/` 与 `Samples~/` 双份示例）
- `Assets/Scenes/`、`Assets/Settings/`
- `Packages/manifest.json`（无本地 file: 引用）、`Packages/packages-lock.json`
- `ProjectSettings/`、`CODELY.md`、`README.md`、`LICENSE`

### 分支策略

- `main` — 开发主线
- 版本分支 `AesirArchitecture-v0.14.0` / `AesirModules-v0.14.0` — CI 在 main 推送时自动 subtree split 生成（包内容为分支根），Git URL 安装经 `#分支名` 固定版本；**只保留最新版本分支**，旧版本分支随发版删除

---

## 待办 / 待确认

- PlaneWar RAA 版示例（`Samples/PlaneWar/Scripts/Raa/`）待编写
- Event Module V2 后续功能见 Feature-Roadmap.md（Docs 私有仓库）
- 无 Addressables 配置（UI 默认 `ResourcesUILoader`；`IUIAssetLoader` 已预留接口）

## Codely Structured Memories

undefined
- [2026-09-05 16:09:05] [project] CI 自动发布 Release 已就位（2026-09-05，用户选定"滚动发布当前版本"策略）：`.github/workflows/auto-release.yml` 在每次推送 main 时发布 tag `v{版本号}` 的 Release（同版本重推会删除重建 tag 与 Release），资产为 AesirArchitecture/AesirModules/AesirFramework 三个 .unitypackage（RAF=两包合并），说明取自根 CHANGELOG 对应版本段落。包由 `.github/scripts/build_unitypackage.py` 纯 Python 生成（无需 Unity），格式经 Unity 导出参考包逐字节校验；语义要点：模拟 AssetDatabase 跳过 `~` 后缀与 `.` 开头目录（Samples~/Documentation~ 不入包，且其 .meta 与 Samples/ 同 GUID），并有意排除 IncludeDependencies 拖入的内置包源文件。**Why:** Tuanjie/Unity 进 CI 需要许可证，脚本方案零依赖且已验证。**How to apply:** 发版前确保根 CHANGELOG 已写好对应 `## [x.y.z]` 段落（缺失则 CI 失败报 version not found）；版本号取两包 package.json（CI 会校验一致）。另：auto-publish-branches.yml 矩阵中的 AesirInspector 残留已移除（此前每次推送该 job 必失败）。

### User
- [2026-08-15 22:20:02] 用户是 yuumixcode（Runestone），三个 Aesir 包（Architecture、Modules、Inspector）的作者，偏好中文交流。
### Feedback
- [2026-08-15 22:20:13] Aesir Inspector 与 Aesir Architecture/Modules 使用不同编码规范。Inspector（2026-07-31 更新）：[Summary] 特性装饰已从全部源码中移除（252 文件，897 处），SummaryAttribute 类仍保留但仅用于 ScriptDocGenerator 的 MemberData 反射；改用 XML 文档注释（/// <summary>）；OdinAutoTooltip（提取自 JakePineOdinTools）自动从 XML 生成 Inspector Tooltip。MIT LICENSE 头部已从所有 .cs 文件移除。原 AesirInspectorCodeStyle.cs 已删除，统一代码风格指南移至 Scripts/CodeStyle/AesirCodeStyle.cs（项目根目录，不参与编译）。保持自文档化代码、禁止对 UnityEngine.Object 派生类使用 ?. /??、事件命名无 On 前缀。Architecture/Modules：中文 XML 文档注释、显式接口实现。编辑代码时需确认所在包以应用正确风格。
- [2026-09-05 16:44:26] 项目英文文档命名规范：根目录英文文档统一使用 `_EN.md` 后缀（如 `README_EN.md`）；`CODE_OF_CONDUCT.en.md` 和 `CONTRIBUTING.en.md` 暂保留 `.en.md` 后缀；各子包的英文 README 统一放在包内 `Documentation/README_EN.md`（2026-09-05 起为 Documentation 双目录主位，`Documentation~/` 降为 UPM 镜像）。**Why:** 统一命名风格，README 从 `.en.md` 改为 `_EN.md` 与子包一致；主位迁至可见 `Documentation/` 以随 unitypackage 导出。**How to apply:** 根目录 README 英文版用 `README_EN.md`，子包英文 README 用 `Documentation/README_EN.md`（改完同步镜像到 `Documentation~/`）。

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
- [2026-09-05 16:20:32] `.codely.packages/` 是 Codely Bridge 包的自动更新缓存（版本号文件夹随升级轮换）。**2026-09-05 晚间最终裁决（再次推翻清理决定）**：Bridge 缓存（.codely.packages/）与 TJGenerators 扩展（.codely-cli/extensions/TJGenerators/，与 Bridge 强绑定）重新入库，`.codely-cli/settings.json`（MCP 配置）一并入库，保证克隆与本机一致；仍忽略：`.codely-cli/` 其余内容（其他扩展 TMPChineseFont、codely-unity-lsp-server；skills；UnityInsight；auto-saves；tool-outputs）、`.codely/`、`.codelyignore`、`.com-unity-codely.json`（含机器端口/路径）。**Why:** 用户要求仓库克隆后与本地环境一模一样。**How to apply:** .gitignore 用白名单负模式维护（`.codely-cli/*` + `!.codely-cli/extensions` + `!.codely-cli/extensions/TJGenerators` + `!.codely-cli/settings.json`）；Bridge 升级时同步 manifest 中的版本路径并提交新版本文件夹。



- [2026-08-24 20:44:35] [feedback] 跨平台中文动态字体方案（用户要求）：不使用 Mac 专有系统字体，不引入全局 TMP 字体资产。运行时 Font.GetOSInstalledFontNames() 探测候选列表 + Font.CreateDynamicFontFromOSFont() 生成。候选优先级：Microsoft YaHei（Windows）→ PingFang SC（macOS）→ Noto Sans CJK SC（Linux/跨平台）→ WenQuanYi Zen Hei（部分 Linux）。全部不可用时回退 Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")（无中文但至少可见）。**Why:** 用户要求兼容 Windows 和 Mac，不使用 Mac 系统专有字体，且不引入全局字体资产修改。**How to apply:** 示例/教学中需要中文 uGUI Text 时用此方案；需要 TMP 中文时才走 TMPChineseFont skill（需用户授权全局回退）。
- [2026-09-05 15:52:04] [feedback] Aesir Inspector 的文档口径：只出现在根 README"推荐链接"章节与 CONTRIBUTING"问题与帮助"的跳转行（指向独立仓库 yuumixcode/AesirInspector），不得再写进包列表、依赖关系、目录树等主内容；根/子包 CHANGELOG 中的历史 [inspector] 条目保留不改（历史记录不重写）。**Why:** 用户 2026-09-05 明确要求"把 Aesir Inspector 放到推荐链接中，不要再写在 README 主要内容中"，当日已完成全仓三包残留清理。**How to apply:** 后续新增/修改任何文档时维持此口径；新文档提到 Inspector 一律以推荐链接形式出现。

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
- [2026-09-03 01:18:26] [project] PlaneWar 飞机大战对比示例（2026-08-24，Mono 版已完成并验证通过）：位于 Assets/Samples/Aesir Architecture/0.13.0/PlaneWar/。Mono 版（Scripts/Mono）已完成：asmdef + Editor asmdef + 6 个 SamplePlaneWarMono* 脚本 + 5 prefab（Player/Bullet/EnemyA-C）+ Scene/SampleForPlaneWarMono.unity + Editor/PlaneWarSceneSetup.cs（菜单 Tools→Aesir→PlaneWar→Fix Scene References 一键修复引用）。素材自包含拷贝自 Assets/Vertical 2D Shooting BE4（Goldmetal，PPU 24 切片）。玩法：玩家在下方（y=-4）朝上射击，敌机自上方（y=6.5）往下飞，子弹命中得分（A=10/B=20/C=30），敌机碰玩家坠毁游戏结束按 Space 重开，HUD 左上角显示得分+系统时间。验证通过：编译 0 错误、HUD 中文显示（跨平台动态字体）、12 秒 Play 录制无报错（PlaneWarMono-final-v5.mp4, 91KB, start_game_view_recording 方式录制）。RAA 版（Scripts/Raa）待编写。**Why:** 用户要建"同一游戏两种写法"的对比教学案例。**How to apply:** 后续任务：①RAA 版编写 ②注册 package.json samples ③sync-samples；命名空间 Runestone.AesirArchitecture.Samples、类名 SamplePlaneWarMono*。



- [2026-08-22 16:23:07] [project] execute_csharp_script 两个实测坑（2026-08-22，团结引擎 2022.3.62）：①会话中新编译的 asmdef 程序集（如新示例包）无法在脚本文本里编译期引用（CS0246），需运行时解析：AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("命名空间.类名", false)).FirstOrDefault(t => t != null) 再 AddComponent(Type) + SerializedObject 设私有字段；②团结引擎 TextureImporter 的切片 API 名与标准 Unity 文档不同：sprites 属性不存在，实际为 spritesheet（SpriteMetaData[]，可写）与 spritePixelsPerUnit（非 spritePixelsToUnit），先用反射列属性名再写导入脚本。**Why:** 两者都导致脚本编译失败，靠报错+反射排查耗时。**How to apply:** 写涉及新程序集类型或 TextureImporter 的编辑器脚本时直接用上述方案。
- [2026-08-22 16:59:25] 用户正在开发视频课程《从 0 构建你的第一套 Unity 架构 —— RAA 极简前端架构实战》，课程规划文档位于 Docs/RAA-Course/（Docs 是独立私有仓库 Aesir-Docs）。核心设定：RAA 迭代拆成 Git 版本教学（V0.1.0 分层+获取路径 → V0.2.0 Command/Query → V0.3.0 MiniEvent/ObservableValue → V1.0.0 实战整合），每版本一个 Tag；实战案例用 Assets/Vertical 2D Shooting BE4（纯精灵资源包）从零实现纵版射击。**Why:** 课程面向初次接触架构的 Unity 开发者，每节 ≤10 分钟是硬约束。**How to apply:** 后续课程相关文档/大纲/讲义统一放 Docs/RAA-Course/，版本规划以 04-版本迭代计划.md 为准。
- [2026-08-24 20:44:35] [project] Unity 2D 触发器碰撞 + 自动属性序列化 三个坑（2026-08-22，团结引擎 2022.3.62 实测）：①两个 Trigger Collider2D 之间必须有至少一个 Kinematic Rigidbody2D 才能触发 OnTriggerEnter2D——Enemy prefab 缺 Rigidbody2D 导致子弹命中无回调，加上 Kinematic Rigidbody2D + NeverSleep 后修复。②C# 自动属性 `public bool IsGameOver { get; private set; }` 的编译器生成 backing field 会被 Unity 序列化到场景文件，编辑器反复 Play 时残留上次值（Awake 中重置会被场景反序列化覆盖）——改为显式非序列化字段 `bool _isGameOver;` + 只读属性 `public bool IsGameOver => _isGameOver;` 修复。③通过 execute_csharp_script 创建的场景对象，其 [SerializeField] 引用（如 HUD 的 Text 组件、Player 的 bulletPrefab）在 Domain Reload 后可能丢失——需编辑器菜单脚本用 SerializedObject 重新绑定。**Why:** 三个坑都导致 Play 模式行为异常且排查耗时极长（自动属性序列化坑最隐蔽）。**How to apply:** MonoBehaviour 中的运行状态字段一律用显式非序列化字段（不带 [SerializeField]）；2D 触发器碰撞双方至少一个挂 Kinematic Rigidbody2D；脚本创建场景对象后务必验证引用完整性。
- [2026-08-24 20:58:48] [project] execute_csharp_script + record_game_view 录制 Play 模式 MP4 的正确方式（2026-08-24 实测）：record_game_view 参数会在脚本执行前触发一次 Roslyn 编译（8-13 秒），期间游戏自由运行——对于有失败条件的实时游戏（如飞机大战），Player 会在编译期间被敌机撞死。解决方案：用 start_game_view_recording（不触发脚本编译）开始录制 → 等待 durationSeconds → finish_game_view_recording 收取 MP4。游戏靠自身 Input 系统 + Player 脚本自然运行，不需要外部驱动脚本。**Why:** record_game_view 的编译延迟对实时游戏是致命的。**How to apply:** 录制实时游戏玩法用 start/finish_game_view_recording 分离方式；只有能在编译期间暂停（如编辑器脚本触发效果）的场景才用 record_game_view。
- [2026-09-02 22:01:16] [project] Observable 集合只读接口为不变型（IReadOnlyObservableList<T> / IReadOnlyObservableDictionary<TKey,TValue> 无 out），与 IReadOnlyObservableValue<out T> 不对称——原因：集合事件参数是结构体（CollectionAddEventArgs 等），结构体类型参数不变，`Action<StructArgs<T>>` 方法参数与 `out T` 协变冲突（CS1961）；ObservableValue 能协变是因为 Action<T> 本身逆变（双重抵消）。**Why:** 2026-09-02 实现 ObservableList/ObservableDictionary 时实测编译报错，代码 remarks 未解释该不对称。**How to apply:** 后续扩展 Observable 家族（HashSet/Queue 等）沿用"不变型接口 + 结构体事件参数"，勿改回 out；若强需协变须把事件参数改为接口/委托形式（不符合极简原则）。
- [2026-09-02 23:26:06] [project] 性能分配实测两个坑（2026-09-02，团结引擎 2022.3.62）：①Unity Mono（Boehm GC）下 GC.GetAllocatedBytesForCurrentThread() 是空实现——控制组（必然分配的 new object()×1000）也读 0 B，瞬态分配无法用运行时计数器实测，零分配验证只能靠 C# 语言语义论证（结构体枚举器 + 模式化 foreach 按规范不装箱，与 BCL List/Dictionary 同机制）；②exec_editor_script 测性能时 CS0246 的更好解法：把测量代码写成 Assets/Editor/ 临时探针类（编译进真实程序集，测量循环内零反射开销），start_compilation_pipeline 后反射调用其静态方法，用完删除——比纯反射调用更适合测量（反射 Invoke 本身分配会污染数据）。**Why:** 实测全 0 差点误当"零分配证据"，控制组校准才暴露测量工具失效。**How to apply:** 做分配实测前先跑控制组校准；需要编译期引用新程序集类型时优先用临时探针文件方案。
- [2026-09-03 22:29:48] MonoLifecycleProxy 调用期增删监听为快照语义（2026-09-03 用户裁决"对齐原生 C# 多播委托语义"）：InvokeEvent 用 _invoking 标志 + _pendingChanges 挂起队列，趟末按发生顺序应用；否决了"保持现状补文档"和"每帧拷贝列表"两个方案。**Why:** 用户明确要对齐原生语义；挂起队列复用 List 保持稳态零分配。**How to apply:** 后续极简化/重构该类时勿把挂起队列回退为即时增删（会重新引入自移除跳帧 bug）；4 个快照语义 PlayMode 测试在 MonoLifecycleProxyTests.cs。
- [2026-09-05 11:09:59] [project] 团结引擎 2022.3.62 的 PlayerSettings.SetScriptingDefineSymbols 在"值未变化"时不会回写 ProjectSettings.asset（运行时 GetScriptingDefineSymbols 有、磁盘 grep 无、文件 mtime 不变）；Set 相同值强制写也不落盘。**Why:** 实测 exec_editor_script 对全部 NamedBuildTarget Set 相同值后 git diff 为空。**How to apply:** 任何新增/确保脚本宏（如 EnsureXxxDefine 类）后必须 grep ProjectSettings.asset 验证落盘；未落盘则按既有分号格式直接编辑该文件补齐（重启后 Unity 以磁盘为准，内存已含同值则一致），勿依赖 SetScriptingDefineSymbols 自动写盘。
- [2026-09-05 15:27:22] [project] AesirFramework 0.14.0 仓库重构完成（2026-09-05，commit 3721b57/8e16afd/559fbe3/f070b84 已推送）：①GitHub 仓库已改名 AesirFramework（本地 remote URL 已同步更新），AesirInspector 迁出为独立公开仓库（定位：Odin Inspector 开发者学习工具包），AESIR_INSPECTOR 宏已从 ProjectSettings 全平台清除。②Samples 双目录规范：包内 Samples/ 为编写主位（直接可见可运行）、Samples~/ 为发布镜像，同步方向 Samples/ → Samples~/（cp -R 含 .meta）；全部 11 个示例 asmdef includePlatforms=["Editor"]（构建剔除）；命名空间 Runestone.AesirArchitecture.Samples.<示例名>（PlaneWar 用 PlaneWarMono），**例外：MiniEvent 与 ObservableValue 示例保留前缀 Runestone.AesirArchitecture.Samples（命名空间段与所演示框架类型同名会 CS0118）**。③分支策略：CI（auto-publish-branches.yml）在 main 推送时 subtree split 生成 AesirArchitecture-v0.14.0 / AesirModules-v0.14.0，旧版本分支随发版删除（6 个旧分支已删）。④manifest.json 已无本地 file: 引用——本机下次打开 Unity 时 Bridge/TJGenerators 包会被卸载，需重新启用 Codely 扩展或手动加回两行。验证基线：batchmode 编译 0 错误、EditMode 67/67 通过、248 个场景/预制体脚本 GUID 零缺失。**How to apply:** 后续发版按此流程（版本号→CI 建分支→删旧分支）；新示例一律 Samples/ 主位 + Editor-only asmdef + 规范命名空间；MiniEvent/ObservableValue 后缀例外勿"修正"。
- [2026-09-05 15:52:04] [project] 两个容易漏的发版同步点（2026-09-05 发现）：①`Assets/Scripts/Editor/AesirPackageInstaller/AesirPackageInstaller.cs` 的 `s_packages` 硬编码版本常量（0.14.0）与 GitRepoUrl，发版需手动同步，否则工具"验证安装"报版本不符（Exporter 无此问题，运行时读 package.json）；该目录是仓库本地编辑器工具，不在两包内。②包内 `Documentation~/README_EN.md` 与中文包 README 靠人工保持同步，本轮发现整体过期（徽章停在 0.13.0、Modules 版整章事件模块缺失），已全部同步至 0.14.0。**Why:** 两处都不在 aesir-version-sync skill 覆盖的清单（package.json/CHANGELOG/根 README）内，改名/发版时易遗漏。**How to apply:** 下次发版把 installer 版本常量纳入同步清单；改中文包 README 时同步对应英文版。
- [2026-09-05 16:20:30] [project] unitypackage 导出方案定稿(2026-09-05 用户裁决"更能满足需求者胜出,弃用另一方案"):实测 Guardingpearsoftware/public-unity-package-exporter(.NET 8 CLI,Lachee fork)后**弃用**——其纯文件级打包不识别本仓库结构:①Samples/ 与 Samples~/ 同 GUID 导致 tar 内 84 对重复 GUID 条目(导入哪个 pathname 不可控);②Documentation~ 等无 .meta 文件被打包且生成全零 GUID 假 meta;③不支持文件夹条目(文件夹 meta 全丢);④glob `**.*` 语义怪异会匹配目录。机制层面它本身满足无-Odin 铁律(纯字节复制、defineConstraints 保留、-r Assets/Runestone 限定后无 Sirenix)。**选定** `.github/scripts/build_unitypackage.py`(配合 auto-release.yml 出 RAA/RAM/RAF 三包),本地实测 253/72/325 条目全通过:零缺失、字节一致、无重复 GUID、无 ~/点路径、无 Sirenix/Plugins、文件夹条目保留、4 处 ODIN_INSPECTOR defineConstraints 原样。**How to apply:** 后续导出/发版一律走 build_unitypackage.py;勿再引入该 CLI 工具或重新评估。
- [2026-09-05 16:39:28] [project] 包内更新器（Aesir Updater）已实现（2026-09-05）：位于 AesirArchitecture/Editor/UpdateChecker/（Runestone.AesirArchitecture.Editor 程序集），菜单 Tools/Aesir/Check for Updates。机制（参考 QF PackageKit"版本记录随包走+先删后导"，增强=自动备份+精确差集）：扫描 Assets/Runestone/*/package.json → GitHub API releases/latest（tag_name+资产）→ 下载 <包目录名>-v<版本>.unitypackage → 备份 Assets/Runestone 到项目根 .aesir-backup/（时间戳前缀命名，保留 3 份）→ 按"上次 .aesir/installed-manifest.json − 新版 files-manifest.json"差集删残留（仅限本包目录，无历史清单跳过）→ ImportPackage 静默导入 → 逐包合并登记清单（域重载中断安全）。CI 侧 auto-release.yml 现发布 3 个 unitypackage + dist/files-manifest.json（build_unitypackage.py --manifest，数组结构兼容 JsonUtility，本地验证 253/72 条目与基线一致）。**Why:** 用户需求"Assets 导入（代码可改）无法走 Git URL 更新"，需 QF 式按钮。**How to apply:** ①开发仓库（有 .git）切勿点更新（Release 会覆盖本地源码，窗口已有警告）②真实导入 E2E 未实测（不可拿 Release 覆盖开发仓库），待下个 Release 后在测试项目验证 ③Release 命名约定 <包目录名>-v<版本>.unitypackage 是更新器资产定位依据，CI 改名会破坏更新 ④EditMode 测试 67→83（新增 AesirUpdateServiceTests 16 个）。
- [2026-09-05 16:44:26] [project] Documentation 双目录方案落地（2026-09-05）：每包新增可见 `Documentation/` 主位（Assets 可见、随 unitypackage 导出、不进玩家构建），保留 `Documentation~/` 为 UPM 镜像（无 .meta，同步时排除 .meta/.DS_Store）；build_unitypackage.py 零改动（只收有 meta 条目、跳过 ~/点路径），实测 RAA 273 条目含 16 个 Documentation 条目、RAM 76 含 4 个、~ 条目 0，files-manifest 同步收录（包内更新器差集清理自动覆盖）。md 在团结 2022.3.62 导入为 TextAsset（非 DefaultAsset），无引用不进构建——约定：该目录只放文档不放会被引用的资产。包 README 徽章/文档链接与根 README/README_EN 已全部指向 Documentation/（CHANGELOG 历史条目不动）。**Why:** unitypackage（Release 下载/包内更新器）用户此前完全没有文档，参照 Samples 双目录补主位。**How to apply:** 改文档先改 Documentation/ 再同步到 Documentation~/；发版条目计数勿拿旧基线（253/72/325）硬校验。

### Reference
- [2026-08-15 22:20:34] AttributeOverviewPro 资产精简方案文档位于 Docs/AttributeOverviewPro-AssetReduction-Plan.md — 包含现状分析、可行性评估、子资产架构设计、详细实现步骤、验证步骤和备选方案。
- [2026-09-05 15:26:38] sync-samples 技能位于 .codely-cli/skills/sync-samples/ — **2026-09-05 起旧工作流已失效**：Assets/Samples 导入副本已从仓库删除，Samples 双目录新规为"包内 Samples/ 为编写主位，内容同步到包内 Samples~/ 发布镜像（含 .meta，GUID 与 UPM 导入链路一致）"；该技能的 Assets/Samples → Samples~ 扫描逻辑不再适用，同步改为直接 cp -R。

