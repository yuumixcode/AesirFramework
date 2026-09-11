# CODELY.md — AesirFramework

## 项目概览

**AesirFramework**（原 Unity-Aesir-Packages）是 **Runestone Yuumix** 开发的 Unity/团结引擎框架集合，提供渐进式 MVC 架构框架与功能模块。

- **Unity 版本：** 2022.3.62f3c1
- **渲染管线：** URP（Universal Render Pipeline 14.0.12）
- **许可证：** MIT
- **作者：** [yuumixcode](https://github.com/yuumixcode)
- **语言：** C#（代码注释和 XML 文档使用中文，代码标识符使用英文）
- **代码规模：** 约 312 个 .cs 文件（含 Samples 与 Samples~ 双份示例源），分布在 2 个包中（Architecture 约 188、Modules 约 124）
- **仓库定位：** 纯 Unity 工程仓库 — Codely 本地工具链（`.codely-cli/`、`.codely.packages/`、`.codely/`、`.codelyignore`）已全部 gitignore，仅 `CODELY.md` 概述文档保留入库

### 包列表

| 包名 | 包 ID | 版本 | 命名空间 | 说明 |
|------|------|------|---------|------|
| Aesir Architecture | `cn.runestone.aesir.architecture` | 0.19.0 | `Runestone.AesirArchitecture` | 渐进式 MVC 架构框架 — 能力接口组合、命令/查询模式、轻量事件（MiniEvent）与响应式属性（ObservableValue）、PlayerLoop 生命周期、纯 C# 架构根 + MonoBehaviour 适配层 |
| Aesir Modules | `cn.runestone.aesir.modules` | 0.19.0 | `Runestone.AesirModules` | 功能模块 — 轻量级 UI 框架（Manager-of-Managers 单例、四层 Canvas 层级、面板生命周期、可替换资源加载器）+ 实验性事件模块 + 脚本文档生成模块（需 Odin） |

> **Aesir Inspector 已独立**：迁出为独立公开仓库，定位为专门面向 Odin Inspector 开发者的学习工具包，不再随本仓库分发。

### 依赖关系

- **Aesir Architecture** — 不依赖任何 Aesir 子包，可独立安装
- **Aesir Modules** — 依赖 `cn.runestone.aesir.architecture`（0.19.0）
- **Aesir Inspector** — 独立公开仓库，与本仓库无依赖关系

---

## Aesir Architecture（0.19.0）

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
- **构建剔除（2026-09-06 修订）** — 示例程序集为**运行时程序集**（不设 includePlatforms）+ 每个示例脚本**整文件 `#if UNITY_EDITOR` 包裹**：编辑器内正常编译、场景可挂载、Play Mode 可运行，玩家构建时整体编译剔除（示例类型 0 入包）。纯编辑器工具（PlaneWarMono.Editor、RuntimeInitializeLoadType）仍为 Editor-only asmdef。动因：Editor-only asmdef 的 MonoBehaviour 会被 Unity 判定为"编辑器脚本"而**禁止挂载到场景物体**（Missing Script），示例场景无法运行。示例内无 Resources 目录、无构建场景引用，资产亦不入包
- **命名空间规范** — `Runestone.AesirArchitecture.Samples.<示例名>`（MvcQuick / MvcStandard / MvcStrict / MvpQuick / MvpStandard / MvpStrict / PlaneWarMono / ObservableCollections）；`MiniEvent` 与 `ObservableValue` 两示例因命名空间段与所演示的框架类型同名冲突（CS0118），保留前缀 `Runestone.AesirArchitecture.Samples`

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
- **示例不进构建包** — 示例程序集为运行时程序集 + 整文件 `#if UNITY_EDITOR`，构建整体剔除（见 Samples 双目录结构节；勿改回 Editor-only asmdef——其脚本禁止挂载场景物体）

### Documentation 双目录结构（0.14.0 起）

- **`Documentation/`（编写主位）** — Assets 中可见、随 unitypackage 导出；两包各一份，存放 README_EN、教学文档与 AesirArchitecture-Skill（AI 编码指南）；LICENSE.md 与 Third Party Notices.md 位于包根（UPM 约定位置，随 unitypackage 导出）
- **`Documentation~/`（UPM 镜像）** — Git URL 安装时包内的隐藏副本；内容无 .meta（UPM 不导入它），由 `Documentation/` 同步而来（同步时排除 .meta 与 .DS_Store）
- **同步方向** — 先编写 `Documentation/`，再同步到 `Documentation~/`
- **构建剔除** — .md 为 Unity 官方 TextAsset 格式（2022.3 手册明确列出 .md；本项目 Unity 2022.3.62f3c1 实测经 TextScriptImporter 导入），但无任何场景/资产引用，玩家构建自动排除；约定该目录只放文档，不放会被引用的资产
- **双目录动因** — 与 Samples 同理：`~` 目录对 AssetDatabase 不可见、不进 unitypackage，而 Release 下载与包内更新器的用户同样需要随包文档

### 包内更新器（Editor）

- **`AesirUpdateService` / `AesirUpdateWindow`** — 位于 `Editor/UpdateChecker/`（`Runestone.AesirArchitecture.Editor` 程序集），菜单 `Tools/Aesir/Check for Updates`
- 面向"代码导入 Assets/Runestone（非 UPM）"的用户：扫描 `Assets/Runestone/*/package.json` 识别本地安装；版本检测三级兜底（首个成功即返回）：① jsDelivr 多域名（cdn/testingcf/gcore/fastly，5s 超时）拉取仓库内 `.github/update-info.json`（大陆友好、无限流；分支引用 CDN 缓存最长约 12 小时）② GitHub Releases API（未认证 60 次/时/IP，按出口 IP 计数）③ GitHub releases/latest 的 302 Location 探测（完全绕开 API 限流）。unitypackage 下载始终走 GitHub Release 直链（jsDelivr 不代理 Release 资产）
- 更新流程：下载 `<包目录名>-v<版本>.unitypackage`（Release 资产命名约定）→ 自动备份 `Assets/Runestone` 到项目根 `.aesir-backup/`（时间戳前缀命名保证排序即时间序，保留最近 3 份）→ 按"上次安装清单 − 新版清单"差集删除残留条目（仅限本包目录内，无历史清单则跳过，不误伤用户新增文件）→ `AssetDatabase.ImportPackage` 静默导入 → 逐包合并登记 `.aesir/installed-manifest.json`（更新中途域重载时已导入包的状态保证正确落盘）
- CI（auto-release.yml）发版后把 `update-info.json`（version/tag + 各包文件清单，`build_unitypackage.py --update-info` 生成，数组结构兼容 JsonUtility）以 `[skip ci]` 提交回 main 供 jsDelivr 拉取；Release 资产仅 3 个 unitypackage。302 探测降级路径无清单 → 更新时自动跳过残留清理
- 设计边界：UPM（Git URL）安装的副本不在管辖内（不在 Assets 下，扫描不到），请用 Package Manager 更新；开发仓库（存在 `.git`）窗口显示警告勿更新——Release 内容会覆盖本地源码；实现参考 QFramework PackageKit（版本记录随包走 + 先删后导），增强点为自动备份与精确差集清理
- EditMode 单测：`Tests/Editor/AesirUpdateServiceTests.cs`（版本比较 / package.json 解析 / 清单差集 / 残留删除与空目录回收 / 备份裁剪 / 清单 JSON / Release 资产定位）

### Odin Inspector 集成

- 通过 `ODIN_INSPECTOR` 定义符号条件编译
- Odin 编辑器处理器使用独立 asmdef：`Runestone.AesirArchitecture.Editor.OdinInspector`（Editor/OdinInspector/，ODIN_INSPECTOR 条件编译）；运行时无独立 Odin 程序集，核心运行时程序集经 `#if ODIN_INSPECTOR` 直接使用 Sirenix API（Sirenix DLL 以自动引用方式接入，未安装 Odin 时相关代码整体编译剔除）
- 预放置实例风险通过 Odin AttributeProcessor 注入 Warning InfoBox
- DDOL 开关字段级 InfoBox — `[Tooltip]` 迁移为 AttributeProcessor 注入的 Info 级信息框（样式与逻辑分离，运行时程序集零 Inspector 样式特性）

### 自定义定义符号

- `AESIR_ARCHITECTURE` — 由 `EnsureAesirArchitectureDefine` 自动设置（注意：`SetScriptingDefineSymbols` 值未变化时不落盘，宏变更后需 grep `ProjectSettings.asset` 验证）
- `ODIN_INSPECTOR`、`ODIN_INSPECTOR_3`、`ODIN_INSPECTOR_3_1`、`ODIN_INSPECTOR_3_2`、`ODIN_INSPECTOR_3_3` — 由 Sirenix 管理
- `AESIR_INSPECTOR` — 已随 AesirInspector 迁出而移除（2026-09-05 已从 ProjectSettings.asset 全平台清除）

---

## Aesir Modules（0.19.0）

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
- `SubscriberPriority` 枚举：First/High/Medium/Last（4 档；High=Attribute 订阅默认、Medium=Script 订阅默认）
- 事件参数载体继承 `AesirEventArgs`，命名为 XxxEventArgs（仅作数据载体流经 EventModule 调度；支持 WithFilter/WithFilters 链式声明订阅者过滤器）
- 分发增强（2026-09-11）：订阅者过滤器（`ISubscriberFilter` 策略接口 + 内建 WithTag/WithPriority/SameSceneAsEmitter/OnlySelf/InsideCollider2D，fail-closed）；死引用清理（分发期自动移除已销毁订阅者 + 编辑器告警）；性能监控（`executionMsLimit` 毫秒阈值，默认 0 关闭）；`AesirListenerAttribute` 补 AllowMultiple=true
- 热路径绑定键缓存（2026-09-11）：`AesirEventUtility.GetEventBindingKey` 按事件类型缓存 `AssemblyQualifiedName`（原生拼接 ~1µs/次且每次新分配字符串，缓存后 ~20ns 字典查询零分配）——分发热路径稳态零分配；性能特征回归测试锁定（键缓存引用同一性 + 编译委托 vs 反射 20 万次对比计时，实测加速 ~78 倍），22 用例 EventModuleTests
- SO 资产化（2026-09-11）：`AesirEventArgsSO`（CreateAssetMenu → Aesir/Event Module/，以资产为发布者 Raise）+ `UnityEventOnAesirEvent` 桥接组件 + `SubclassSelector`/`ExcludeSubclassSelector`（[SerializeReference] 子类下拉，UI Toolkit Drawer，运行时程序集放特性/编辑器程序集放 Drawer）+ SO 自定义 Inspector（运行模式限定 Raise 按钮）
- 系统事件（元事件）与 DefaultChannel 暂缓（依赖编辑器工具链）；单实例合并不实施（Awake 已销毁重复实例，订阅均经 Instance 单一通道）
- 设计文档位于 Docs/AesirModules/EventModule/（私有仓库 Aesir-Docs：Feature-Roadmap.md / Sample-Plan.md）
- EditMode 测试：Tests/Editor/EventModuleTests.cs（19 用例，含过滤器/死引用清理/性能监控/SO/多特性订阅）

### DDOL 机制

- `AesirModules`、`UIRoot`、`UIModule` 均有 `[SerializeField] bool dontDestroyOnLoad = true`，预放置/运行时创建统一由该字段控制
- `UIModule` 的字段仅在预放置为根物体时生效，运行时自动创建时挂载于 [Aesir Modules] 宿主下跟随宿主决策

### 场景编辑器

- `SceneManagerWindow` — 场景管理自定义编辑器窗口
- `BootstrapSceneHelper` — 场景引导工具
- `SceneAssetWrapper` — 可序列化场景资源引用

### 脚本文档生成模块（需 Odin）

- 位于 `Runtime/ScriptDocGenerator/OdinInspector/` 与 `Editor/ScriptDocGenerator/OdinInspector/`，整体经 asmref 汇入 `Runestone.AesirModules.OdinInspector` / `Runestone.AesirModules.Editor.OdinInspector`（强依赖 Odin Inspector，未安装自动排除）；`Editor.OdinInspector` asmdef 因本模块新增对运行时 Odin 程序集的引用
- 命名空间 `Runestone.AesirModules.ScriptDocGenerator`(.Editor)；全部 public API 跨程序集访问（与原独立 asmdef 一致，无 InternalsVisibleTo）
- 功能：反射生成结构化 API 文档（增量保留 `## Additional Notes` 后手写内容）+ Summary 工具（XML `<summary>` ↔ `[Summary]` 双向同步，自动补 `using Runestone.AesirModules.ScriptDocGenerator;`）
- 入口 `Tools → Aesir → Script Doc Generator`（含 UI Toolkit 版）；Assets 右键 `Assets/Script Doc Generator`
- 测试位于包根 `Tests/Editor/ScriptDocGenerator/`（命名空间 `Runestone.AesirModules.Tests.Editor.ScriptDocGenerator`，153 用例）；模块文档 `Documentation/script-doc-generator.md`（镜像 `Documentation~/`）

### Samples 双目录结构（0.14.0 起，与 Architecture 同规则）

- `Samples/`（编写主位）与 `Samples~/`（发布镜像）并存；事件模块示例目录 `Events/01_KeyPress`、`Events/02_Filters`、`Events/03_SOAsset`（与 package.json samples 路径一致；02/03 含场景与 SO 资产）
- 示例程序集 `Runestone.AesirModules.Samples.Events.KeyPress` 为运行时程序集 + 整文件 `#if UNITY_EDITOR`（构建剔除）；命名空间 `Runestone.AesirModules.Samples.Events.KeyPress`

---

## 程序集定义

### Aesir Architecture（15 个 asmdef）

| 程序集 | 路径 | 说明 |
|--------|------|------|
| `Runestone.AesirArchitecture` | Runtime/ | 核心运行时 |
| `Runestone.AesirArchitecture.Editor` | Editor/ | 编辑器（含 QuickCreateSOMenuItem、EnsureAesirArchitectureDefine） |
| `Runestone.AesirArchitecture.Editor.OdinInspector` | Editor/OdinInspector/ | ODIN_INSPECTOR |
| `Runestone.AesirArchitecture.Tests` | Tests/Runtime/ | PlayMode 测试（MonoLifecycleProxy 快照语义等） |
| `Runestone.AesirArchitecture.Tests.Editor` | Tests/Editor/ | EditMode 测试（110 个） |
| `Runestone.AesirArchitecture.Samples.MvcQuick` | Samples/Counter-Mvc-Quick/Scripts/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirArchitecture.Samples.MvcStandard` | Samples/Counter-Mvc-Standard/Scripts/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirArchitecture.Samples.MvcStrict` | Samples/Counter-Mvc-Strict/Scripts/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirArchitecture.Samples.MvpQuick` | Samples/Counter-Mvp-Quick/Scripts/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirArchitecture.Samples.MvpStandard` | Samples/Counter-Mvp-Standard/Scripts/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirArchitecture.Samples.MvpStrict` | Samples/Counter-Mvp-Strict/Scripts/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirArchitecture.Samples.MiniEvent` | Samples/MiniEvent/Scripts/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirArchitecture.Samples.ObservableValue` | Samples/ObservableValue/Scripts/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirArchitecture.Samples.ObservableCollections` | Samples/ObservableCollections/Scripts/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirArchitecture.Samples.PlaneWarMono` | Samples/PlaneWar/Scripts/Mono/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirArchitecture.Samples.PlaneWarMono.Editor` | Samples/PlaneWar/Editor/ | 场景引用一键修复菜单 |

### Aesir Modules（8 个 asmdef + 4 个 asmref）

> 标准 Unity 自定义包根结构：包根两级目录 `Runtime/` 与 `Editor/`，功能模块以子目录形式存在于对应层级（如 `Runtime/UI/`、`Editor/UI/`），模块间零依赖；删除模块 = 删 `Runtime/<模块>/` 与 `Editor/<模块>/`。核心程序集锚点在层根，层内模块主代码自动汇入；细分程序集锚点在 `Common/` 下，模块专属代码（`OdinInspector/`、`Addressables/` 子目录）经 Assembly Definition Reference（asmref）汇入。

| 程序集 | 锚点路径 | 说明 |
|--------|------|------|
| `Runestone.AesirModules` | Runtime/（层根） | 核心运行时锚点（引用 AesirArchitecture）；Common/UI/Scene/Events 主代码自动汇入 |
| `Runestone.AesirModules.OdinInspector` | Runtime/Common/OdinInspector/ | Binder 全家桶（ODIN_INSPECTOR）；Runtime/UI/OdinInspector/ 与 Runtime/ScriptDocGenerator/OdinInspector/ 经 asmref 汇入 |
| `Runestone.AesirModules.Editor` | Editor/（层根） | 核心编辑器锚点；Common/UI/Scene 编辑器代码自动汇入 |
| `Runestone.AesirModules.Editor.OdinInspector` | Editor/Common/OdinInspector/ | Odin 处理器（ODIN_INSPECTOR）；Editor/UI、Editor/Scene、Editor/ScriptDocGenerator 的 OdinInspector/ 经 asmref 汇入 |
| `Runestone.AesirModules.Editor.Addressables` | Editor/Common/Addressables/ | Addressables 胶水（AESIR_MODULES_ADDRESSABLES）；Editor/Scene/Addressables/ 经 asmref 汇入 |
| `Runestone.AesirModules.InputSystem` | Runtime/UI/InputSystem/ | UIRoot 输入模块替换（ENABLE_INPUT_SYSTEM，独立可选） |
| `Runestone.AesirModules.Scene.Tests` | Editor/Scene/Tests/ | Scene 模块 EditMode 测试（UNITY_INCLUDE_TESTS） |
| `Runestone.AesirModules.Samples.Events.KeyPress` | Samples/Events/01_KeyPress/ | 运行时 + #if UNITY_EDITOR |

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
| ObservableCollectionsSample | `Assets/Runestone/AesirArchitecture/Samples/ObservableCollections/Scene/` | 可观察集合（List/Dictionary/HashSet）使用示例 |
| SampleForPlaneWarMono | `Assets/Runestone/AesirArchitecture/Samples/PlaneWar/Scene/` | PlaneWar 纵版射击（Mono 版）示例 |
| SampleForEventFilters | `Assets/Runestone/AesirModules/Samples/Events/02_Filters/Scene/` | 事件模块订阅者过滤器对照示例 |
| SampleForEventSOAsset | `Assets/Runestone/AesirModules/Samples/Events/03_SOAsset/Scene/` | 事件模块 SO 资产化 + UnityEvent 桥接示例 |

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
8. **ObservableCollections（可观察集合）** — `ObservableList<T>` / `ObservableDictionary<TKey,TValue>` / `ObservableHashSet<T>` 使用示例：订阅 Added / Removed / Replaced / Updated / Cleared 事件，ContextMenu 触发增删改查与集合运算；命名空间 `Runestone.AesirArchitecture.Samples.ObservableCollections`
9. **MiniEvent** — `MiniEvent` 和 `MiniEvent<T>` 使用示例（无参/单参事件）
10. **PlaneWar（Mono 版）** — 纵版射击飞机大战实战示例：得分 HUD、三型敌机、重开流程；命名空间 `Runestone.AesirArchitecture.Samples.PlaneWarMono`，`Tools → Aesir → PlaneWar → Fix Scene References` 一键修复引用；RAA 版（Scripts/Raa）待编写

### Aesir Modules（包内 `Samples/Events/`）

1. **Event Module - Key Press** — 事件模块基本发布-订阅示例：按键发布事件、`[AesirListener]` 静态订阅
2. **Event Module - Filters（过滤器）** — `02_Filters`：Space 发布 `WithTag("Player")`+`InsideCollider2D` 双重过滤警报、R 发布 `OnlySelf` 家族命令；场景 `SampleForEventFilters.unity` 内置圈内/圈外/无标签、家族/无关多组对照组 + uGUI HUD 接收日志（跨平台中文字体）；命名空间 `Runestone.AesirModules.Samples.Events.Filters`
3. **Event Module - SO Asset（SO 资产化）** — `03_SOAsset`：`ScoreEventAsset.asset`（AesirEventArgsSO + managedReference 指向 ScoreChangedEvent，NewScore=100）+ 场景 `SampleForEventSOAsset.unity` 演示 `UnityEventOnAesirEvent` 桥接组件零代码串联 UnityEvent 回调（On Raised 持久绑定 ScoreBulb.Flash）+ ScorePublisher 代码订阅同事件展示双消费方式；命名空间 `Runestone.AesirModules.Samples.Events.SOAsset`

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
│   │   │   │   └── Common/            # AesirArchitecture 单例、Debug、ResetStaticsAssistant
│   │   │   ├── Editor/                # 定义符号管理、QuickCreateSO、包内更新器、Odin AttributeProcessors
│   │   │   ├── Tests/                 # Editor 110 个 + Runtime PlayMode 测试
│   │   │   ├── Samples/               # 示例（编写主位，10 个，构建剔除）
│   │   │   ├── Samples~/              # 示例发布镜像（Package Manager 按需导入）
│   │   │   ├── Documentation/         # 文档主位（Assets 可见、随 unitypackage 导出、不进构建）
│   │   │   └── Documentation~/        # 文档镜像（Git URL 安装隐藏副本，无 .meta）
│   │   └── AesirModules/              # 功能模块（标准包根结构，模块间零依赖）
│   │       ├── Runtime/               # 核心运行时锚点（Runestone.AesirModules.asmdef）
│   │       │   ├── Common/            # 共享基础 + Odin 运行时锚点（OdinInspector/）
│   │       │   ├── UI/                # UIModule、UIRoot 等（InputSystem/ 独立程序集、OdinInspector/ Binder 经 asmref）
│   │       │   ├── Scene/             # SceneModule、SceneAssetWrapper（含 Exceptions/）
│   │       │   ├── Events/            # 实验性事件模块（含 Component/）
│   │       │   └── ScriptDocGenerator/ # 脚本文档生成模块（OdinInspector/ 经 asmref 汇入 Odin 程序集）
│   │       ├── Editor/                # 核心编辑器锚点（Runestone.AesirModules.Editor.asmdef）
│   │       │   ├── Common/            # Odin 编辑器锚点（OdinInspector/）+ Addressables 胶水锚点（Addressables/）
│   │       │   ├── UI/                # UI 菜单项（OdinInspector/ Processor 经 asmref）
│   │       │   ├── Scene/             # 场景编辑器窗口（Tests/ 测试程序集、OdinInspector/、Addressables/ 经 asmref/asmdef）
│   │       │   └── ScriptDocGenerator/ # 脚本文档生成模块编辑器（OdinInspector/ 经 asmref 汇入 Odin 编辑器程序集）
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
- Odin 依赖代码隔离在 `OdinInspector/` 子目录（RAA 仅 Editor 侧独立 asmdef；RAM 经 asmref 汇入独立 Odin 程序集）；核心程序集经 `#if ODIN_INSPECTOR` 直接使用 Sirenix API
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

尚无自定义构建脚本；示例脚本以整文件 `#if UNITY_EDITOR` 剔除，玩家构建自动排除示例。

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
- 版本分支 `AesirArchitecture-v0.19.0` / `AesirModules-v0.19.0` — CI 在 main 推送时自动 subtree split 生成（包内容为分支根），Git URL 安装经 `#分支名` 固定版本；**只保留最新版本分支**，旧版本分支随发版删除

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
- [2026-09-05 16:56:26] `.codely.packages/` 是 Codely Bridge 包的自动更新缓存（版本号文件夹随升级轮换）。**2026-09-05 晚间最终裁决（再次推翻清理决定）**：Bridge 缓存（.codely.packages/，1.0.81-exp.1）与 TJGenerators 扩展（.codely-cli/extensions/TJGenerators/，与 Bridge 强绑定）重新入库，保证克隆与本机一致；`.codely-cli/settings.json` 曾尝试入库但**被 Codely 应用持续自动重写（model 字段数分钟内漂移）最终改回忽略**；仍忽略：`.codely-cli/` 其余内容（其他扩展 TMPChineseFont、codely-unity-lsp-server；skills；UnityInsight；auto-saves；tool-outputs）、`.codely/`、`.codelyignore`、`.com-unity-codely.json`（含机器端口/路径）。**Why:** 用户要求仓库克隆后与本地环境一模一样；但 settings.json 是应用管理的易变状态，且可能包含 apikey 等敏感配置（用户 2026-09-05 明确要求忽略），追踪只会产生无休止提交竞赛。**How to apply:** .gitignore 用白名单负模式维护（`.codely-cli/*` + `!.codely-cli/extensions` + `!.codely-cli/extensions/TJGenerators`）；Bridge 升级时同步 manifest 中的版本路径并提交新版本文件夹；另外本仓库 `core.fsmonitor=true`，批量文件操作（扩展自更新/脚本重写）后 status 会出现大量幻影 M，`rm .git/index && git reset` 重建索引即清。





- [2026-08-24 20:44:35] [feedback] 跨平台中文动态字体方案（用户要求）：不使用 Mac 专有系统字体，不引入全局 TMP 字体资产。运行时 Font.GetOSInstalledFontNames() 探测候选列表 + Font.CreateDynamicFontFromOSFont() 生成。候选优先级：Microsoft YaHei（Windows）→ PingFang SC（macOS）→ Noto Sans CJK SC（Linux/跨平台）→ WenQuanYi Zen Hei（部分 Linux）。全部不可用时回退 Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")（无中文但至少可见）。**Why:** 用户要求兼容 Windows 和 Mac，不使用 Mac 系统专有字体，且不引入全局字体资产修改。**How to apply:** 示例/教学中需要中文 uGUI Text 时用此方案；需要 TMP 中文时才走 TMPChineseFont skill（需用户授权全局回退）。
- [2026-09-05 15:52:04] [feedback] Aesir Inspector 的文档口径：只出现在根 README"推荐链接"章节与 CONTRIBUTING"问题与帮助"的跳转行（指向独立仓库 yuumixcode/AesirInspector），不得再写进包列表、依赖关系、目录树等主内容；根/子包 CHANGELOG 中的历史 [inspector] 条目保留不改（历史记录不重写）。**Why:** 用户 2026-09-05 明确要求"把 Aesir Inspector 放到推荐链接中，不要再写在 README 主要内容中"，当日已完成全仓三包残留清理。**How to apply:** 后续新增/修改任何文档时维持此口径；新文档提到 Inspector 一律以推荐链接形式出现。
- [2026-09-05 17:25:58] [feedback] 示例菜单与 ScriptableSingleton 落盘路径约定（2026-09-05）：①示例的菜单项放在 `Tools/Aesir/Architecture/Samples/` 之下（如 Tools/Aesir/Architecture/Samples/RuntimeInitializeLoadType），不要直接挂在 Tools/Aesir 下；②ScriptableSingleton + [FilePath(Location.ProjectFolder)] 的设置资产路径统一套一层 `ScriptableSingleton/` 前缀（如 ScriptableSingleton/AesirArchitecture/Samples），避免项目根目录出现多个品牌文件夹。**Why:** 用户要求减少项目根目录的文件夹混乱；菜单按 Architecture/Samples 分组归位。**How to apply:** 新增示例窗口菜单、ScriptableSingleton FilePath 常量时遵循此层级；.gitignore 用 /ScriptableSingleton/ 忽略该运行时生成目录。
- [2026-09-10 00:39:26] [feedback] RAM 新增"整体强依赖 Odin"模块的结构定稿（2026-09-10 ScriptDocGenerator 整合时用户裁决）：不设独立 asmdef，全部代码放 `Runtime/<模块>/OdinInspector/` + `Editor/<模块>/OdinInspector/` 经 asmref 汇入 `Runestone.AesirModules(.Editor).OdinInspector`；Editor.OdinInspector asmdef 需手动加对运行时 Odin 程序集的引用（此前 Editor Odin 代码不引用运行时 Odin 类型）；包级测试进 `Tests/Editor/<模块>/`，测试命名空间用 `Runestone.AesirModules.Tests.Editor.<模块>`（Tests asmdef 需加 Editor.OdinInspector 引用）。**Why:** 用户明确选择 asmref 方案而非独立 asmdef，与 Binder 同模式保持单一 Odin 程序集。**How to apply:** 后续强依赖 Odin 的新模块按此结构；菜单归入 Tools/Aesir/；模块文档放 Documentation/<module>.md 并同步 Documentation~/。注意：老代码若靠"父命名空间链"解析跨命名空间类型（如 X.Y.Editor.Tests 解析 X.Y.Editor 类型），命名空间改名后会 CS0246，需补 using。
- [2026-09-10 00:40:22] [feedback] run_shell_command 内 `nohup ... &` 启动的长驻进程会在命令返回后被清理（子进程组随 shell 退出）；zensical serve 等本地预览服务器必须用 run_in_background=true 启动。**Why:** 2026-09-10 两次 nohup 起 zensical serve 都秒死，pgrep 无进程。**How to apply:** 任何长驻进程（预览服务器、watch 任务）一律 run_in_background；一次性命令才用前台执行。
- [2026-09-10 18:29:13] RAA/RAM 代码审查规范裁决（2026-09-10 全包锐评时用户明示）：代码规范冲突时 **Rider 规范优先于外部审查 skill**（如 code-review-unity 遵循的 Unity 6 官方 C# 风格指南）——`_camelCase` 私有实例字段、非 is 前缀布尔（Initialized/_sortDirty 等）均不构成违规，勿按 skill 条目误报；审查风格要求"锐评"：直接、有立场、四维带评分锚点、文档宣称与实现不符要点名。**Why:** 用户明确要求"代码规范以 Rider 的代码规范优先"，项目自有规范（CODELY.md）本就与 Rider 一致，skill 的 Unity 6 条目（裸 camelCase 字段、is 前缀布尔）会与之冲突。**How to apply:** 后续任何代码审查（无论用不用 skill）先按 Rider + 项目 CODELY.md 规范裁决再套检查项；code-review-unity skill 已装项目级 .codely-cli/skills/code-review-unity/（新会话自动出现在技能列表）。

### Project
- [2026-08-15 22:20:30] AttributeOverviewPro 子资产重构已完成并合并到 main（2026-07-25）：~194 个独立 .asset 文件合并为 3 个文件 — AttributeOverviewDatabase.asset（DatabaseSO + 70 PanelSO 子资产）、UnityExamples.asset（Unity 原生序列化 ExampleSO）、OdinExamples.asset（Odin 序列化 ExampleSO）。按序列化方式分离存储。初始化超时 bug 已修复（批量创建跳过逐次 SaveAssets）。
- [2026-08-15 22:20:30] Monorepo 安装方式：三个子包通过各自的 `?path=` 参数从同一 Git 仓库安装，例如 `https://github.com/yuumixcode/Unity-Aesir-Packages.git?path=Assets/Runestone/AesirArchitecture`。**Why:** monorepo 中三个子包在同一仓库的不同子目录，直接用仓库根 URL 无法正确识别单个包。**How to apply:** README 中的 Git URL 始终带 `?path=Assets/Runestone/{包名}` 参数；Aesir Modules 会自动拉取 Architecture 依赖。
- [2026-08-15 22:20:30] 包依赖关系（2026-07-25）：Aesir Modules 仅依赖 Aesir Architecture（移除了 Inspector 依赖）；Aesir Inspector 强依赖 Odin Inspector（从可选改为必需）；Aesir Architecture 保持独立。**Why:** 简化依赖链，Modules 不再间接依赖 Inspector/Odin；Inspector 本质上需要 Odin 才能工作。**How to apply:** Modules 的 package.json 仅声明 architecture 依赖；Inspector 的 README 和 package.json 标注 Odin 为强依赖。
- [2026-08-15 22:20:30] 移除 Context 事件总线系统（2026-07-30）：从 Aesir Architecture 移除了 MiniEventBus&lt;TEvent&gt;、IEventArgs、ICanAddListener、ICanInvokeEvent，以及 IContext/AbstractContext 中的 AddListener/RemoveListener/InvokeEvent 方法和 CapabilityExtensions 中对应的扩展方法。保留 MiniEvent/MiniEvent&lt;T&gt; 和 ObservableValue&lt;T&gt; 作为独立事件机制。**Why:** 用户认为 Context 的 AddListener 不实用。**How to apply:** 角色接口不再继承 ICanInvokeEvent/ICanAddListener；事件通信应使用 MiniEvent 或 ObservableValue。
- [2026-09-11 10:12:37] Event Module V2 已实现并编译通过（2026-07-31，简化版+性能优化）。当前状态：已移除 AbstractAttributeBound&lt;T&gt; 基类，EventModule 直接继承 AesirMonoBehaviour。双注册表分离（AttributeBindings + DynamicBindings，均为 public Dictionary）。BindingInfo 基类仅含 BindingKey/Subscriber/Priority + 抽象 Invoke()；StaticBindingInfo 持有 MethodInfo + 表达式树编译委托；DynamicBindingInfo&lt;T&gt; 持有 Action&lt;T&gt; 直接委托。Script 订阅返回 AutoRemoveListenerHandle。已移除 InvokeDelayed、IsInitialized、取消传播/密封/共享等机制。SubscriberPriority 枚举实际为 4 档 First/High/Medium/Last（见 2026-09-08 记忆条目）。设计文档位于 Docs/AesirModules/EventModule/。**Why:** 参考商业插件 Game Event Hub，但以实用性为标准裁剪。**How to apply:** V2 运行时代码已完成；后续功能见 Feature-Roadmap.md。

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
- [2026-09-05 16:55:23] [project] Observable 集合只读接口为不变型（IReadOnlyObservableList<T> / IReadOnlyObservableDictionary<TKey,TValue> / IReadOnlyObservableHashSet<T> 无 out），与 IReadOnlyObservableValue<out T> 不对称——不变型成因分两类：List/Dictionary 是结构体事件参数（CollectionAddEventArgs 等，结构体类型参数不变，`Action<StructArgs<T>>` 方法参数与 `out T` 协变冲突 CS1961）；HashSet 无结构体参数，只因只读接口自行声明的 Contains(T) 处于输入位也必须不变型。ObservableValue 能协变是因为 Action<T> 本身逆变（双重抵消）。ObservableHashSet 已实现（2026-09-05，14 测试全过）：事件负载单值直传 Action<T>（对齐 Dictionary 用 KeyValuePair 直传），多字段负载才造结构体 EventArgs。**Why:** 2026-09-02 实现 List/Dictionary 实测编译报错；2026-09-05 实现 HashSet 验证第二类成因并确立单值直传先例。**How to apply:** 后续扩展 Observable 家族（Queue 等）沿用不变型接口，勿改回 out；事件参数按负载定——单值直传、多字段才造结构体 EventArgs，勿为单值负载引入 EventArgs 类型。

- [2026-09-02 23:26:06] [project] 性能分配实测两个坑（2026-09-02，团结引擎 2022.3.62）：①Unity Mono（Boehm GC）下 GC.GetAllocatedBytesForCurrentThread() 是空实现——控制组（必然分配的 new object()×1000）也读 0 B，瞬态分配无法用运行时计数器实测，零分配验证只能靠 C# 语言语义论证（结构体枚举器 + 模式化 foreach 按规范不装箱，与 BCL List/Dictionary 同机制）；②exec_editor_script 测性能时 CS0246 的更好解法：把测量代码写成 Assets/Editor/ 临时探针类（编译进真实程序集，测量循环内零反射开销），start_compilation_pipeline 后反射调用其静态方法，用完删除——比纯反射调用更适合测量（反射 Invoke 本身分配会污染数据）。**Why:** 实测全 0 差点误当"零分配证据"，控制组校准才暴露测量工具失效。**How to apply:** 做分配实测前先跑控制组校准；需要编译期引用新程序集类型时优先用临时探针文件方案。
- [2026-09-03 22:29:48] MonoLifecycleProxy 调用期增删监听为快照语义（2026-09-03 用户裁决"对齐原生 C# 多播委托语义"）：InvokeEvent 用 _invoking 标志 + _pendingChanges 挂起队列，趟末按发生顺序应用；否决了"保持现状补文档"和"每帧拷贝列表"两个方案。**Why:** 用户明确要对齐原生语义；挂起队列复用 List 保持稳态零分配。**How to apply:** 后续极简化/重构该类时勿把挂起队列回退为即时增删（会重新引入自移除跳帧 bug）；4 个快照语义 PlayMode 测试在 MonoLifecycleProxyTests.cs。
- [2026-09-05 11:09:59] [project] 团结引擎 2022.3.62 的 PlayerSettings.SetScriptingDefineSymbols 在"值未变化"时不会回写 ProjectSettings.asset（运行时 GetScriptingDefineSymbols 有、磁盘 grep 无、文件 mtime 不变）；Set 相同值强制写也不落盘。**Why:** 实测 exec_editor_script 对全部 NamedBuildTarget Set 相同值后 git diff 为空。**How to apply:** 任何新增/确保脚本宏（如 EnsureXxxDefine 类）后必须 grep ProjectSettings.asset 验证落盘；未落盘则按既有分号格式直接编辑该文件补齐（重启后 Unity 以磁盘为准，内存已含同值则一致），勿依赖 SetScriptingDefineSymbols 自动写盘。
- [2026-09-06 11:50:08] [project] AesirFramework 0.14.0 仓库重构完成（2026-09-05，commit 3721b57/8e16afd/559fbe3/f070b84 已推送）：①GitHub 仓库已改名 AesirFramework（本地 remote URL 已同步更新），AesirInspector 迁出为独立公开仓库（定位：Odin Inspector 开发者学习工具包），AESIR_INSPECTOR 宏已从 ProjectSettings 全平台清除。②Samples 双目录规范：包内 Samples/ 为编写主位（直接可见可运行）、Samples~/ 为发布镜像，同步方向 Samples/ → Samples~/（cp -R 含 .meta）；全部 11 个示例 asmdef 当时设 includePlatforms=["Editor"]（构建剔除——**2026-09-06 已废弃**，该方式导致示例场景 Missing Script 无法运行，见 2026-09-06 新条目）；命名空间 Runestone.AesirArchitecture.Samples.<示例名>（PlaneWar 用 PlaneWarMono），**例外：MiniEvent 与 ObservableValue 示例保留前缀 Runestone.AesirArchitecture.Samples（命名空间段与所演示框架类型同名会 CS0118）**。③分支策略：CI（auto-publish-branches.yml）在 main 推送时 subtree split 生成 AesirArchitecture-v0.14.0 / AesirModules-v0.14.0，旧版本分支随发版删除（6 个旧分支已删）。④manifest.json 已无本地 file: 引用——本机下次打开 Unity 时 Bridge/TJGenerators 包会被卸载，需重新启用 Codely 扩展或手动加回两行。验证基线：batchmode 编译 0 错误、EditMode 67/67 通过、248 个场景/预制体脚本 GUID 零缺失。**How to apply:** 后续发版按此流程（版本号→CI 建分支→删旧分支）；新示例一律 Samples/ 主位 + 运行时程序集 + 整文件 #if UNITY_EDITOR + 规范命名空间（2026-09-06 起替代 Editor-only asmdef）；MiniEvent/ObservableValue 后缀例外勿"修正"。

- [2026-09-05 17:42:15] [project] 容易漏的发版同步点：包内 `Documentation~/README_EN.md` 与中文包 README 靠人工保持同步，本轮发现整体过期（徽章停在 0.13.0、Modules 版整章事件模块缺失），已全部同步至 0.14.0。**Why:** 不在 aesir-version-sync skill 覆盖的清单（package.json/CHANGELOG/根 README）内，改名/发版时易遗漏。**How to apply:** 改中文包 README 时同步对应英文版。（原第①点 AesirPackageInstaller 硬编码版本常量已失效——2026-09-05 该安装器已整体删除，版本/安装检查统一为包内更新器 AesirUpdateService/Window，动态读 package.json 无需同步版本常量）

- [2026-09-05 16:20:30] [project] unitypackage 导出方案定稿(2026-09-05 用户裁决"更能满足需求者胜出,弃用另一方案"):实测 Guardingpearsoftware/public-unity-package-exporter(.NET 8 CLI,Lachee fork)后**弃用**——其纯文件级打包不识别本仓库结构:①Samples/ 与 Samples~/ 同 GUID 导致 tar 内 84 对重复 GUID 条目(导入哪个 pathname 不可控);②Documentation~ 等无 .meta 文件被打包且生成全零 GUID 假 meta;③不支持文件夹条目(文件夹 meta 全丢);④glob `**.*` 语义怪异会匹配目录。机制层面它本身满足无-Odin 铁律(纯字节复制、defineConstraints 保留、-r Assets/Runestone 限定后无 Sirenix)。**选定** `.github/scripts/build_unitypackage.py`(配合 auto-release.yml 出 RAA/RAM/RAF 三包),本地实测 253/72/325 条目全通过:零缺失、字节一致、无重复 GUID、无 ~/点路径、无 Sirenix/Plugins、文件夹条目保留、4 处 ODIN_INSPECTOR defineConstraints 原样。**How to apply:** 后续导出/发版一律走 build_unitypackage.py;勿再引入该 CLI 工具或重新评估。
- [2026-09-05 16:39:28] [project] 包内更新器（Aesir Updater）已实现（2026-09-05）：位于 AesirArchitecture/Editor/UpdateChecker/（Runestone.AesirArchitecture.Editor 程序集），菜单 Tools/Aesir/Check for Updates。机制（参考 QF PackageKit"版本记录随包走+先删后导"，增强=自动备份+精确差集）：扫描 Assets/Runestone/*/package.json → GitHub API releases/latest（tag_name+资产）→ 下载 <包目录名>-v<版本>.unitypackage → 备份 Assets/Runestone 到项目根 .aesir-backup/（时间戳前缀命名，保留 3 份）→ 按"上次 .aesir/installed-manifest.json − 新版 files-manifest.json"差集删残留（仅限本包目录，无历史清单跳过）→ ImportPackage 静默导入 → 逐包合并登记清单（域重载中断安全）。CI 侧 auto-release.yml 现发布 3 个 unitypackage + dist/files-manifest.json（build_unitypackage.py --manifest，数组结构兼容 JsonUtility，本地验证 253/72 条目与基线一致）。**Why:** 用户需求"Assets 导入（代码可改）无法走 Git URL 更新"，需 QF 式按钮。**How to apply:** ①开发仓库（有 .git）切勿点更新（Release 会覆盖本地源码，窗口已有警告）②真实导入 E2E 未实测（不可拿 Release 覆盖开发仓库），待下个 Release 后在测试项目验证 ③Release 命名约定 <包目录名>-v<版本>.unitypackage 是更新器资产定位依据，CI 改名会破坏更新 ④EditMode 测试 67→83（新增 AesirUpdateServiceTests 16 个）。
- [2026-09-05 16:56:26] [project] Documentation 双目录方案落地（2026-09-05）：每包新增可见 `Documentation/` 主位（Assets 可见、随 unitypackage 导出、不进玩家构建），保留 `Documentation~/` 为 UPM 镜像（无 .meta，同步时排除 .meta/.DS_Store）；build_unitypackage.py 零改动（只收有 meta 条目、跳过 ~/点路径），实测 RAA 273 条目含 16 个 Documentation 条目、RAM 76 含 4 个、~ 条目 0，files-manifest 同步收录（包内更新器差集清理自动覆盖）。md 为 Unity 官方 TextAsset 格式（2022.3 手册明确列出 .md；本项目 Unity 2022.3.62f3c1 实测经 TextScriptImporter 导入为 TextAsset，非 DefaultAsset），无引用不进构建——约定：该目录只放文档不放会被引用的资产。包 README 徽章/文档链接与根 README/README_EN 已全部指向 Documentation/（CHANGELOG 历史条目不动）。**Why:** unitypackage（Release 下载/包内更新器）用户此前完全没有文档，参照 Samples 双目录补主位。**How to apply:** 改文档先改 Documentation/ 再同步到 Documentation~/；发版条目计数勿拿旧基线（253/72/325）硬校验。
- [2026-09-05 17:06:26] [project] 包内更新器大陆优化——jsDelivr 方案定稿（2026-09-05 用户裁决"可以使用这个"，弃 Gitee）：版本检测三级兜底 ①jsDelivr 四域名（cdn/testingcf/gcore/fastly，5s 超时，fastly 301 自动跟随）拉仓库内 `.github/update-info.json`（build_unitypackage.py --update-info 生成，version/tag+包文件清单，CI 发版后以 [skip ci] 提交回 main——防 auto-release/auto-publish 循环触发）②GitHub API ③releases/latest 302 Location 探测（redirectLimit=0 读 Location 头，完全绕开 API 限流，实测 location 格式 .../releases/tag/vX.Y.Z）。unitypackage 下载始终走 GitHub Release 直链（jsDelivr 不代理 Release 资产）。**Why:** 限流按出口 IP 计（家庭用户各享 60 次/时碰不到；共享风险在校园网/VPN 出口）；大陆真实痛点是连通性而非限流；Gitee 实测门槛高（需建仓库+实名+token，dromara/hutool 实际路径是 chinabugotech/hutool 且无 Release 对象，releases/latest 对无 Release 仓库 404）。**How to apply:** ①jsDelivr 分支引用缓存最长约 12h，新发版延迟半天被检测到（窗口 HelpBox 已注明）②update-info.json 首次由下个 Release 的 CI 生成，此前 jsDelivr 404 属预期（自动降级 GitHub API，已实测）③C# 9 不支持 const 内插字符串（CS8773），含 RepoPath 的 URL 常量用 static readonly ④OrderByDescending(d => d.Length) 勿配 StringComparer（CS0411）⑤EditMode 测试 83→100（更新器 16 个 + 另一会话 ObservableHashSet 14 个）。
- [2026-09-05 18:08:20] CODELY.md 正文"程序集定义"表中 `Runestone.AesirArchitecture.OdinInspector`（Runtime/OdinInspector/）行及目录结构对应条目已失效——该目录与 asmdef 均不存在（AesirInspector 迁出后遗留）。**Why:** 2026-09-05 文档核查发现；记忆工具不能直改 CODELY.md 正文。**How to apply:** 用户手动维护 CODELY.md 时删除该行；两包中英 README 的结构树已在 2026-09-05 同步修正（同时删去了不存在的 MonoLifecycleProxyAttributeProcessor.cs、DescriptionSO.cs 条目）。
- [2026-09-05 22:52:22] [project] Docs/（Aesir-Docs 私有仓库）已按包重组并归档（2026-09-05，commit 0e2a4dd）：活跃文档按包归类——AesirArchitecture/（RAA-Course、Unity-RuntimeInitializeOnLoadMethod-指南、DI容器调研[仍未 git 提交]）、AesirModules/EventModule/；跨包 Auto-Publish-Branches.md 留根；Analysis/ 与 TaskReports/ 目录撤销，10 篇归档入 Archive/2026-09-v0.15.0/（含 Inspector 迁出后失效的 JakePineOdinTools×4 + Summary-Feature-Replacement-Plan）；新增 Docs/README.md 索引；Auto-Publish-Branches.md 已对齐当前 CI（2 包、版本分支 AesirArchitecture-v0.15.0 式命名、AesirFramework 仓库 URL）。**Why:** Inspector 迁出后仓库只有两包，文档按包格局重组。**How to apply:** 旧记忆/CODELY.md 正文引用的 Docs/EventModule/、Docs/RAA-Course/、Docs/Analysis/ 路径均已失效，新路径为 Docs/AesirModules/EventModule/、Docs/AesirArchitecture/RAA-Course/；CODELY.md 正文"事件模块"节的 Docs/EventModule/ 引用需用户手动更新；Docs 仓库提交风格为 docs: 中文单行主题。
- [2026-09-05 23:30:19] [project] RAM Scene 模块重构完成（2026-09-05，Eflatun.SceneReference 功能吸收，未发版）：SceneAssetWrapper ≈ SceneReference+Odin——新增 GUID 锚点自愈（sceneGuid 序列化字段+EditorSyncFromAsset）、State/UnsafeReason 状态机、TryGet 家族、专用异常族（SceneAssetWrapperException 基类+Empty/Creation/NotAddressable/SupportDisabled）、FromScenePath/FromAsset(编辑器) 工厂、Address 序列化缓存与 AddressablesSupportEnabled；Addressables 条件架构=核心 asmdef versionDefines 设 AESIR_MODULES_ADDRESSABLES + 独立胶水程序集 Runestone.AesirModules.Editor.Addressables（defineConstraints 排除，经 SceneAssetWrapperAddressablesBridge 静态委托桥注册 GetAddress/MakeAddressable）；SceneModule 修复全部 P0 语义 bug；新增 Tests/Editor 程序集 27 用例（自适应装/不装 Addressables）。**Why:** 用户要求吸收 Eflatun 有用功能、支持 Addressables 且遵循最小惊讶原则、缺失依赖时"不报错而是不编译"。**How to apply:** 下次发版 CHANGELOG 必须写破坏性变更清单——①删 AddScene/UnloadAddedScene(+WithWrapper 变体)，统一 LoadSceneAdditive 纯叠加追踪；②*WithSceneAssetWrapper 6 方法改为同名重载；③ReloadScene 同步→异步(带回调)；④ScenePath/Guid/SceneName/BuildIndex/LoadedScene 空引用由返回空值改为抛 EmptySceneAssetWrapperException；⑤LoadSceneAdditive 不再卸载上个场景、Additive 不再抢激活场景；⑥加载失败新增 onFailed 回调。
- [2026-09-05 23:30:33] [project] 条件编译方案三态实测结论（2026-09-05，团结引擎 2022.3.62）：①asmdef defineConstraints 不满足时会先排除程序集再解析 references——胶水程序集用 **name 引用** Unity.Addressables.Editor + defineConstraints [AESIR_MODULES_ADDRESSABLES]（versionDefines 由 com.unity.addressables 任意版本触发），装/卸包双向实测 0 编译错误（Eflatun 用的是 GUID 引用+整文件 #if，两者皆可行）；②versionDefines 声明在核心 asmdef 与胶水 asmdef 各放一份（belt-and-braces，Unity 对宏是否全局可见无需依赖单一声明位置）；③**坑：安装 Addressables 后 Odin Inspector 会自动生成 Assets/Plugins/Sirenix/Odin Inspector/Modules/Unity.Addressables/ 模块（硬引用无守卫），卸包后 Odin 不会主动撤走 → 42 个 CS0234 编译错误**——需手动删除该模块文件夹（.data/.info.txt 注册表文件保留，Odin 装包时会自动重新导入）；④Unity 包 resolve 会把 manifest.json 中 file: 本地引用重排到列表头部，checkout 恢复即可。**Why:** 卸包验证时踩中 Odin 模块残留与 manifest 重排两个非预期变更。**How to apply:** 装卸 Addressables 做验证时：装→跑测试→卸→删 Odin 模块文件夹→refresh→checkout manifest.json；任何"可选包集成"用 defineConstraints+独立程序集方案可放心用 name 引用。
- [2026-09-06 01:11:40] [project] RAM 功能模块结构重组完成（2026-09-06，commit 49b9e76 + 标准包根二次调整，未发版）：**标准 Unity 自定义包根结构**——包根两级目录 `Runtime/` 与 `Editor/`，功能模块以子目录存在于对应层级（`Runtime/UI/`、`Editor/UI/`），模块间零依赖；删除模块 = 删 `Runtime/<模块>/` 与 `Editor/<模块>/`。核心程序集锚点在层根（`Runtime/Runestone.AesirModules.asmdef`、`Editor/Runestone.AesirModules.Editor.asmdef`），层内模块主代码自动汇入无需 asmref；细分程序集锚点在 `Common/` 下（Odin 运行时 `Runtime/Common/OdinInspector/`、Odin 编辑器 `Editor/Common/OdinInspector/`、Addressables 胶水 `Editor/Common/Addressables/`），模块专属代码放各自 `OdinInspector/`、`Addressables/` 子目录经 4 个 asmref 汇入；Scene 测试程序集改名 Runestone.AesirModules.Scene.Tests 位于 Editor/Scene/Tests/；模块隔离为约定保证（重组时审计零跨模块引用，单核心程序集模式下无编译期强制）；删除 Events 模块需连带删 Samples/Events/（示例依赖事件模块）。**Why:** 用户裁决采用标准 Unity Custom Package 根目录结构（首次方案把模块文件夹放顶层被用户纠正）。**How to apply:** 新增 RAM 功能模块 = 建 `Runtime/<模块>/` + `Editor/<模块>/`（主代码自动汇入核心程序集）；Odin 专属代码放该层 `OdinInspector/` 子目录 + 一个 asmref；Addressables 胶水锚点在 Editor/Common/Addressables/；关键实测依据：asmref 指向被 defineConstraints 排除的程序集时整体静默排除、约束满足时正确汇入（双向探针验证，团结 2022.3.62）；SampleScene 存在一条重组前即缺失的脚本 GUID（b7cd017143765464fbeb78c71e462d18，历史遗留，做 GUID 扫描时勿误判为新问题）。
- [2026-09-06 11:02:22] aesir-version-sync 技能已于 2026-09-06 重写对齐现状（技能文件在 .codely-cli/skills/，属 gitignore 不入库）。两包发版现状事实：①Inspector 已迁出，仅 Architecture/Modules 两包同号（CI 校验一致）；②根英文 README 是 README_EN.md（非 README.en.md）；③Assets/Samples/ 导入副本已不存在；④包英文 README 主位在 Documentation/README_EN.md（改完 cp 到 Documentation~ 镜像）；⑤根 CHANGELOG 顶部"当前版本"表格与固定分支名也要同步；⑥Release 标题已改为纯 tag 名（auto-release.yml --title ${TAG}，v0.16.2 起生效）。**Why:** 技能旧步骤基于三包时代，0.16.1/0.16.2 发版实测逐项失效。**How to apply:** 发版时以 `git grep <旧版本号>` 全仓清单为准逐项替换（排除 CHANGELOG 历史段与 .github/update-info.json——后者由 CI 回写）；推送前确保根 CHANGELOG 有对应 `## [x.y.z]` 段落；推送被拒通常是 CI 的 [skip ci] 回写提交，fetch+rebase 后重推；推完等 CI 生成新版本分支后删除所有旧版本分支（遗留的 v0.14.0/v0.15.0 分支已在 0.16.1 发版时补删）。**坑：** 对 CODELY.md 做 `sed s/旧版本/新版本/g` 会连记忆条目里的版本号一起改写，条目含版本号时需事后核对修正。

- [2026-09-06 11:01:40] replace/write_file 工具的读取状态跨用户轮次失效：上一轮编辑/提交过的文件，在新一轮指令里直接 replace 会报 "Use read_file to read the file before editing"（实测 0.16.2 发版批量替换 11 个文件全部被拒）。**Why:** 工具按会话轮次校验文件新鲜度，git commit 或新用户消息会重置状态。**How to apply:** 跨轮次的批量编辑（如版本号全仓替换）先对每个目标文件做一次小窗口 read_file（满足校验即可）再并行 replace_all，避免整批重跑。
- [2026-09-06 11:50:20] [project] 示例程序集新规范（2026-09-06，用户选定方案 A）：两包 10 个示例代码程序集改为**运行时程序集（includePlatforms: []）+ 51 个示例脚本整文件 #if UNITY_EDITOR 包裹**（Eflatun 同款模式，包裹行注释统一为"#if UNITY_EDITOR // 示例仅编辑器内参与编译…"）。**Why:** 实测推翻旧假设——Editor-only asmdef（includePlatforms:["Editor"]）的 MonoBehaviour 被 Unity 判定为"编辑器脚本"（Can't add script behaviour X because it is an editor script），**禁止挂载到场景物体**，Edit/Play Mode 中示例场景全部组件 Missing Script；而磁盘 GUID 引用正确、DLL 正常编译加载（AssetDatabase 能反射到类型），极具迷惑性。0.14.0 的 Editor-only 改造使所有示例场景无法运行（PlaneWar 0.13.0 时代可玩正是因当时还是运行时程序集）。方案 A 兼得 Play Mode 运行与构建 0 示例类型；弃 B（运行时裸奔→构建带死代码 DLL）与 C（只留 Samples~→开发仓库内示例不可用）。**How to apply:** 新示例一律运行时 asmdef + 整文件 #if UNITY_EDITOR；PlaneWarMono.Editor 与 RuntimeInitializeLoadType 纯编辑器程序集保持 Editor-only（注意：RuntimeInitializeLoadType 的 Editor asmdef 文件名不带 .Editor 后缀，按文件名排除的批量脚本会漏改它）；Samples~/ 镜像只同步本次 61 个改动文件，Samples/ 顶层目录级 .meta 镜像缺失属 0.14.0 历史遗留未处理；Samples~/PlaneWar 场景未同步用户未提交的 m_Bits 改动。验证基线：refresh 0 错误、EditMode 125 过/0 失败/2 跳过、PlaneWar Play 敌机 3 秒 0→4 + MP4（screenshots/GameView_2026-09-06_11-43-57-851.mp4）。
- [2026-09-06 12:21:57] [project] Bridge unity_editor.refresh 的场景污染坑（2026-09-06，团结 2022.3.62 实测）：refresh 在编译域重载前会执行 CompilationHelper.EnsureScenesSavedBeforeReload → EditorSceneManager.SaveScene，把当前打开场景（含未清理的测试对象）写盘。**How to apply:** 编辑器 E2E 测试若向已打开场景注入临时对象，必须保证每次 refresh 前对象已删除；refresh 后用 git diff 核实场景文件。另注意两个坑：①无编译发生时 refresh 不触发保存，内存干净但磁盘仍是旧快照，需 exec_editor_script 强制 EditorSceneManager.SaveScene；②GenerateCode 类内部调用 AssetDatabase.Refresh() 并未立即触发编译（编译推迟到下一次 unity_editor.refresh），DidReloadScripts 挂载流程实际在显式 refresh 后执行。
- [2026-09-06 13:52:12] [project] Binder 功能组织定稿（2026-09-06 用户裁决）：Binder 全部收录于 Odin 程序集（Runtime/UI/OdinInspector/Binder/，含 BinderBaseTypeAttribute）——原因：其类型选择器（组件/基类 ValueDropdown）强依赖 Odin Inspector，已写入三份包 README。联动变化：①BinderBaseTypeAttribute 从核心程序集迁回 Odin 程序集，核心三个面板基类（AesirBasePanel/View<T>/ViewController<T>）不再标注（核心无法反向引用 Odin asmdef）；②面板家族改由 BinderAssistant.GetBaseTypes 经 typeof 内置预选（泛型以 <T> 占位），[BinderBaseType] 仅用于用户自定义基类（需引用 Runestone.AesirModules.OdinInspector，autoReferenced 对 Assembly-CSharp 生效）；③Binder 单元测试从 Editor/UI/OdinInspector/Binder/Tests/ 迁至包根 Tests/Editor/，asmdef 改名 Runestone.AesirModules.Tests（引用 core+Odin，Editor-only + UNITY_INCLUDE_TESTS），BinderCodeGenerator 的 InternalsVisibleTo 已联动改名。**How to apply:** CODELY.md「程序集定义」表需用户手动补 Runestone.AesirModules.Tests 行；后续包级测试（非 Scene 模块）放 Tests/Editor/。
- [2026-09-06 14:50:48] [project] 单程序集编译失败会静默冻结整个编译管线（2026-09-06 HUDPanel 泛型占位案例实测，团结 2022.3.62）：任一程序集编译失败（如 Assembly-CSharp 里自动生成文件含非法基类占位 <T>）→ 域不重载，**所有程序集停留在旧版本**——磁盘源码已更新但反射探测不到新成员（AddComponent 后报 "Could not find field"）、TestRunnerApi 跑的是旧测试（用例数不变是关键线索）、仅 Console 显示那个无关文件的错误。另：exec_editor_script 的 Roslyn 脚本程序集不在 InternalsVisibleTo 名单内，探针访问 internal 类型/成员必须全反射（先 Type.GetType 到目标程序集再 GetMethod/Field，BindingFlags 显式 Public|NonPublic）。**How to apply:** refresh 后若新代码"没生效"，先查 Console 是否有其他程序集（尤其用户生成文件/Assembly-CSharp）的编译错误，修复阻塞源文件本身即可恢复；验证新代码是否真正加载用反射探测新成员存在性，勿信测试通过即加载成功。
- [2026-09-06 16:14:34] [project] 团结引擎 ScriptableSingleton API 差异（2026-09-06 BinderEditorSettings 实测）：团结引擎 2022.3.62 的 UnityEditor.ScriptableSingleton&lt;T&gt; 实例属性是**小写 instance**（标准 Unity 为大写 Instance），代码直接写 .Instance 会报 CS0117。**How to apply:** 需要双引擎兼容时用反射封装（GetProperty("Instance") ?? GetProperty("instance")，Public|Static + NonPublic 兜底），缓存后使用；参考 BinderEditorSettings.Settings。另：Odin 类级 DetailedInfoBox 只能放一个（两个会抛 InvalidOperationException: The state 'ShowDetailedMessage' already exists on '$ROOT'）——多个说明合并进一个 box 分节展示。
- [2026-09-06 18:27:52] [project] SerializedMonoBehaviour 反射修改会被 Odin 序列化数据在域重载后覆盖（2026-09-06 HUD ContextTypeName 两次丢失实测）：对 SerializedMonoBehaviour 派生组件用反射 FieldInfo.SetValue + SetDirty 修改 public 字段，场景保存写入 Unity YAML 键，但域重载时 Odin 用自身序列化数据恢复字段（覆盖 YAML 键值）→ 修改"丢失"。**How to apply:** 编辑器脚本修改 SerializedMonoBehaviour 组件字段必须走 SerializedObject（FindProperty + ApplyModifiedProperties）+ SetDirty + SaveScene；验证修改存活需跨一次 refresh（域重载）后重新检查。另：Modules 测试 asmdef 用 overrideReferences:true 时 Sirenix 预编译 DLL 也被屏蔽——测试引用 BinderAssistant（SerializedMonoBehaviour 链）需在 precompiledReferences 显式列出 Sirenix.OdinInspector.Attributes / Sirenix.Serialization / Sirenix.Serialization.Config / Sirenix.Utilities（nunit 之外）。
- [2026-09-06 20:37:02] AesirFramework 公开文档站（yuumixcode/AesirFramework-Docs，gh-cli 创建）：Zensical 静态站 + GitHub Pages（deploy.yml，build_type=workflow，CI= pip install zensical + build --strict），线上 https://yuumixcode.github.io/AesirFramework-Docs/。本地源位于 /Users/yuumix/Projects/Unity/AesirFramework/AesirFramework-Docs（**已按用户要求移入主项目根、与 Assets 同级**，主仓 .gitignore 已加 /AesirFramework-Docs/ 排除）。2026-09-06 已完成正式内容填充：基于两包 README 与 Documentation/ 核查（概览/快速开始/特性/兼容性×2 包 + FAQ/support，版本口径 0.17.0），后续内容同步仍以包内 README 为事实基线；提交风格 docs: 中文单行主题。**Why:** 用户要公开官方文档站，与私有 Aesir-Docs（开发文档）并存。**How to apply:** 文档站改动进该仓库（勿混入 Aesir-Docs）；本地构建用 ~/.local/bin/zensical（pipx 安装，PEP 668 禁止 pip 系统级安装，bin 目录需显式加 PATH）；git 身份用 zeriying@gmail.com（全局默认 yuumixcode@foxmail.com，新仓库需按品牌惯例覆盖）。
- [2026-09-11 10:13:22] [project] SubscriberPriority 实际只有 4 档（First/High/Medium/Last，代码 Assets/Runestone/AesirModules/Runtime/Events/SubscriberPriority.cs），High=Attribute 订阅默认、Medium=Script 订阅默认。**Why:** 2026-09-08 写文档站时实测源码确认；旧记忆与包内 Documentation/event-module.md 的"5 档 Essential→High→Medium→Low→Cleanup"均为过时描述（枚举 XML 注释头也误写 5 档），AesirFramework-Docs 已按 4 档修正。**How to apply:** 涉及事件模块优先级的文档/代码一律以 4 档为准；CODELY.md 正文与包内 event-module.md 的"5 档"表述已于 2026-09-11 全量修正（含中英 README 与源码 XML 注释），勿再当待办。

- [2026-09-10 00:40:22] [project] AesirFramework-Docs 文档站子仓库被主仓 .gitignore 排除（/AesirFramework-Docs/），glob / list_directory / search_file_content 对该目录完全不可见（报 "N files were ignored"）。**Why:** 2026-09-10 主页优化时 glob 探索被全挡，改用 shell find 才拿到结构。**How to apply:** 操作文档站文件直接用 run_shell_command（find/ls）或 read_file 绝对路径；勿用 glob 探索该目录。
- [2026-09-10 00:40:22] [project] tabbit-cli 当前在本机不可用：BROWSER_RUNTIME_UNAVAILABLE（exit 69），`open -a "Tabbit Browser"` 重启浏览器后三次重试仍失败——Runtime/Agent 集成需在 Tabbit Browser 应用内启用，CLI 侧无法修复。**Why:** 2026-09-10 主页视觉验证因此改走构建产物结构化校验。**How to apply:** 需要浏览器截图前先确认 Runtime Service 已启用；不可用时用 zensical build --strict + site/ 产物结构校验替代，并给用户本地预览 URL。
- [2026-09-10 11:33:43] [project] Scripting API 文档的家已迁至文档站（2026-09-10）：`AesirFramework-Docs/docs/architecture/api/`（83 个类型页 + index.md 命名空间参考，Unity ScriptReference 风格分组：类/结构体/接口/枚举/委托），站点样式 CSS 在 `docs/stylesheets/api.css`（锚点约定：.api-summary-table/.api-params-table/.api-returns-table；元信息块为主题原生无标题 `!!! note ""` 警示框——api-meta 自定义卡片样式已被用户否决删除；区块名"声明"非"语法"；成员详情统一先注释后声明）。**Why:** 生成器 `docFolderPath`（ScriptDocGenerator.asset）仍指向已删除的 `Assets/Editor Default Resources/Script Doc Generator/Documents`，下次重新生成会在旧位置重建文件；且新增/删除类型后必须同步重做 ①索引页 ②zensical.toml 的 Scripting API 导航块（均由解析 front matter+note 块种类+`## 声明` 的脚本生成，勿手改）。**How to apply:** API 变更后重新生成 → 复制到 docs/architecture/api/ → 重跑索引/导航生成脚本 → build --strict；或推动生成器支持输出到 Assets 外路径。泛型文件名含 `{T}`/空格/逗号，Zensical 产物 URL 自动百分号编码，GitHub Pages 可正常服务。

- [2026-09-10 01:33:49] [project] Zensical(0.0.60 实测)的 attr_list 块级标记 `{: .cls }` 对**表格无效**——class 不应用且标记原文渲染残留（标题级 `{#anchor}` 行内标记正常）。**Why:** 2026-09-10 Scripting API 页 425 处 `{: .api-summary-table }` 等全部残留，构建产物 0 个 class 生效。**How to apply:** 表格挂 class 一律用 `<div class="x" markdown="1">` 包裹（md_in_html），CSS 选择器写 `.x table`；站点 84 个 API 页已全部改为 div 包裹，api.css 选择器已同步。**生成器已同步修复（2026-09-10）**:`ZensicalScriptingAPISettingsSO.cs` 三处表格输出（概览/参数/返回值）已改 div 包裹，输出形状与站点文件逐字符对齐（花括号平衡校验过；EditMode 测试与重新生成端到端验证待 Unity 会话补跑——Unity Bridge 当时未连接）。
- [2026-09-10 10:42:27] [project] Zensical 模板覆写必须显式声明目录：`[project.theme] custom_dir = "docs/overrides"`(0.0.60 实测默认 None,docs/overrides/ 不会自动生效——技能文档"自动覆盖"说法不完整，放文件不声明则静默不生效）。**Why:** 2026-09-10 写 overrides/main.html 后构建无错但覆写未渲染，查 zensical config.py 才发现 custom_dir 默认 None。**How to apply:** 任何 overrides 先配 custom_dir；调试模板变量别用 HTML 注释输出（构建会剥离注释），用 `<div data-x="{{ var }}" hidden>` 之类可见节点；page.url 形如 `architecture/api/ObservableValue{T}/`,MiniJinja 支持 `"x" in page.url` 子串判断。**后续(2026-09-10)**:API 页专属布局(侧栏钩子+加宽)已被用户否决——全站统一版式：侧栏 12.1→10/9.5rem、nav/TOC 字体 0.7→0.60rem、长标识符 overflow-wrap:anywhere，规则集中在 extra.css 全站段，md-grid 保持 61rem(所有页面两侧空白一致);overrides/main.html 与 custom_dir 已移除，模板覆写机制留作将来需要时用。
- [2026-09-10 17:50:58] [project] 新增含场景的包内示例在"编辑器未开本项目"时的全手写流程（2026-09-10 ObservableCollections 示例实测走通）：①uuidgen 生成 32 位小写 hex GUID，先 grep 全仓 *.meta 防碰撞；②场景仿 MiniEventSample.unity 模板改写（m_Script 指向新 .cs.meta 的 GUID；URP 相机附加数据组件用内置 GUID a79441f348de89743a2939f4d699eac1 原样保留）；③.meta 按既有模板逐字复制（folder/MonoImporter/AssemblyDefinitionImporter；场景 .meta 是 DefaultImporter 而非 NativeFormatImporter——以 MiniEventSample.unity.meta 为准）；④cp -R 同步 Samples~（顶层不带 folder .meta，与 9 个旧示例惯例一致）；⑤验证：`Unity -batchmode -quit -projectPath . -logFile` 后 grep "error CS"/Exception/Aborting，确认新 asmdef DLL 生成 + 场景以新 GUID 导入（RTT 项目在另一实例打开不冲突）。**Why:** Unity Bridge 未连接时无法让编辑器代生成 meta，手写方案已端到端验证（0 错误、DLL 14.8KB 落盘）。**How to apply:** 下次新增示例（无论编辑器是否连接）可复用该流程；场景 YAML 由模板改写最稳，勿凭空构造。
- [2026-09-10 18:29:13] RAA 全包锐评完成（2026-09-10，code-review-unity 全包模式 + 一手精读 Runtime 约 40 文件 + 严格档示例全套 + 2 个只读子代理交叉验证；报告未落盘、仅在会话中）。核心未修复发现清单：① Model OnInitialize 期 GetService 必炸且报错文案误导（指示重排注册顺序，实际两阶段初始化"先全部 Model 后全部 Service"决定重排无解，CapabilityExtensions.cs L63-67）；② Editor 主 asmdef defineConstraints=UNITY_INCLUDE_TESTS + package.json 硬依赖 test-framework——消费者删 TF 后全部编辑器工具（含 EnsureAesirArchitectureDefine 宏确保器、包内更新器）静默消失；③ Tests/Runtime asmdef overrideReferences 硬引用 3 个 Sirenix DLL，与"Odin 可选"宣称矛盾；④ AbstractContext.Dispose 后 Instance 返回僵尸上下文（GetModel 报"未注册"指向性错误，修复可 Dispose 内置 _instance=null）；⑤ 三处 XML 文档失实：AesirArchitecturePlayerLoop.cs L46/99"周期性检测"（120 帧轮询 0.9.0 已删仍写）、AesirArchitecture.cs L10 组件宣称"初始化架构基础设施"（实为空壳 DDOL 宿主，Context 是纯 C# 懒加载）、MiniEvent.cs L10/85"零分配监听管理"（仅 Invoke 成立，AddListener 有闭包+多播 O(n) 拷贝分配）；⑥ 测试盲区：Command/Query 两条链零测试（ExecuteCommand/ExecuteQuery 扩展从未被任何用例执行）、View/ViewController 四基类零测试、RemoveListenerOnDestroy/OnDisable 零测试（120 用例集中在集合/Context/Locator/PlayerLoop/MiniEvent）。功能性缺口按频率排序：async/await（框架零 async 故事）> 时间调度原语（延迟/下一帧/定时，纯 C# 类无协程无合法延时手段）> DI/构造注入 > FSM（设计边界未声明，规划盲区）> OnEnable 重订原语（OnDisableTrigger 移除永久不可逆，池化 UI 痛点）> 调试工具（GetListenerCount API 已有但无任何 UI 消费，无 Context 注册表窗口）；其他零碎：私有 void Reset() 撞 Unity 魔法方法（MonoLifecycleProxy L69/SceneUnloadedTrigger L78）、ICustomLifecycle.cs 文件级 doc 挂首类型、AbstractQuery 缺 [Serializable]、RegisterCustomLifecycle 同名重载语义分裂（带参版不绑销毁清理）、Duplicate Awake Destroy(gameObject) 连带销毁用户整 GO。**Why:** 用户可能要求后续修复，重做全包审查需 50+ 工具调用成本高。**How to apply:** 用户提"修上次锐评/审查发现的问题"时按此清单直接定位修复，无需重新审查；逐条修复后更新此记忆对应条目。
- [2026-09-10 20:31:00] [project] RAA 锐评全部建议项已当日修复完毕（在存档提交 9c5bea5 之后的工作树上，含新测试文件待提交）：Critical 全修——GetService 报错改两阶段初始化说明文案、Editor asmdef 移除 UNITY_INCLUDE_TESTS + package.json 移除 test-framework 硬依赖、Tests/Runtime asmdef 加 ODIN_INSPECTOR defineConstraints 守卫（Sirenix 引用保留，无 Odin 环境自动排除整个测试程序集）、AbstractContext.Dispose 解除 _instance 单例缓存、五文件（MiniEvent/ObservableValue/三集合）"零分配"宣称修正为仅 Invoke 路径；Style 全修——PlayerLoop"周期性检测"失实宣称 ×2、AesirArchitecture 空壳宿主注释改写、IView 措辞降级为档位约定、私有 Reset→ClearState（避免撞 Unity 魔法方法）、ICustomLifecycle 文档错挂、AbstractQuery 补 [Serializable]；Suggestions——AesirArchitecturePlayerLoop.Register 返回 AutoRemoveListenerHandle、RegisterCustomLifecycle(mono/go,evt,cb) 改绑销毁自动移除【行为变更，原先不自动移除】、AesirArchitecture/MonoLifecycleProxy 重复实例 Destroy(this) 替代 Destroy(gameObject)、RemoveListenerOnSceneUnloadedTrigger 补重复守卫、ObservableList.AddRange null→ArgumentNullException、能力矩阵补 ExecuteQuery 列（中英 README + Documentation~ 镜像）、决策表 ACA→RAA；新增 Tests/Editor/CapabilityExtensionsTests.cs 10 用例锁定 CQRS 链（此前 Command/Query 零覆盖）与 Dispose 重建行为。验证：EditMode 438 total/436 passed/0 failed/2 skipped；踩坑——Unity batchmode 存在编译错误时退出码仍为 0（2022.3.62 实测），必须 grep 日志勿信退出码；RemoveListenerWhenGameObjectOnDestroyed 返回 void 不可链式（CS0029）、属性访问不可单独作语句表达式（CS0201，用 _ = 丢弃赋值解决）。**Why:** 上一条锐评清单要求修复后更新对应条目。**How to apply:** 上一条清单已全部处置完毕，勿再当待办引用；剩余项仅新功能方向（async/时间调度/DI/FSM/OnEnable 重订，0.18 候选）；RAM 的 Tests asmdef 存在同样的 Sirenix 硬引用问题，修复时参照 RAA 的 ODIN_INSPECTOR defineConstraints 方案。
- [2026-09-10 21:05:00] [project] 文档站 API 页结构规律（2026-09-10 手工同步 0.18.0 变更实测）：ScriptDocGenerator 生成的 API 页**只引用 XML <summary>（类与成员摘要），<remarks> 不上页**——因此改源码 remarks 不必同步文档站，改 summary 则必须；页内每个成员出现两次（顶部摘要表 + 成员详情段），手改需两处同步；成员有返回值时详情段含 **返回值** + api-returns-table div 块（签名变化时需补齐）；api/index.md 命名空间索引表也复制类 summary，类级 summary 变更时第三处同步。**Why:** Unity Bridge 不可用时文档站只能手工同步，摸清页面生成规律避免漏改。**How to apply:** API summary 变更后手工同步 = 摘要表行 + 成员详情段 +（类级含 api/index.md 行）；完成后 zensical build --strict 验证；彻底对齐仍需下个 Unity 会话跑生成器重新生成。
- [2026-09-10 22:45:54] [project] RAA 0.18.0 第二轮全包锐评完成（2026-09-10 晚，code-review-unity 全包模式 + Runtime 56 文件精读 + 3 只读子代理交叉验证；上轮 20+ 修复项已逐条验证全部落地）。本轮新发现待修清单：① Critical-文档失实：AesirArchitectureAttributeProcessor.cs XML 宣称三个信息框（含"仅 DDOL 关闭时显示的类级 Warning"），实现只有两个 Info 框，条件 Warning 不存在（可用 InfoBoxAttribute 第三参 VisibleIf 补）；② RemoveListenerOnSceneUnloadedTriggerAttributeProcessor 的 Warning 信息框无条件常显（文案却是条件句式，默认配置下永久噪音，应 VisibleIf 绑宿主开关）；③ AbstractSubmodule.Dispose 不重置 Initialized（已释放模块仍自称已初始化）；④ RemoveListenerOnSceneUnloadedTrigger.Awake 不设 _instance=this（另两个单例都设，预放置双实例会双双订阅 sceneUnloaded，空桶无害但范式不同形）；⑤ README 中英"MiniEvent 零分配事件"项目结构行缺"（Invoke 路径）"限定（0.18.0 修类文档时漏网）+ 项目结构漏列 Samples/RuntimeInitializeLoadType；⑥ 示例：Mvp-Quick Model 公开可变字段 count（封装倒退）、严格档通知回调里重复 ExecuteQuery（有注释自曝）；⑦ 架构判词：能力墙是半 advisory——GetModel/GetService 可经 self.Context.GetModel 直接绕过（IContext 公开），ExecuteCommand/Query 是硬墙（Context 无对应方法），读写强度倒挂；⑧ Tests/Runtime asmdef 的 3 个 Sirenix precompiledReferences 不能直接删（MonoLifecycleProxy 继承 AesirMonoBehaviour→Odin 环境下 SerializedMonoBrowsable 链），后果=无 Odin 环境 PlayMode 测试整体静默消失，候选解法 overrideReferences:false（Sirenix DLL auto-referenced 时测试可全环境跑，需消费机验证）；⑨ 测试盲区实证：RemoveListenerOnDestroy/OnDisableTrigger + RemoveListenerExtensions 零测试（严格档示例在用）、View/ViewController 四基类零测试、MiniEvent.RemoveListener/GetListeners 零测试、GenericLocatorTests 无 SetUp 无静态隔离（顺序耦合）；⑩ IModel 自 0.9.0 起就无 ICanGetService（git 考古确认），CODELY.md 能力表"Model: GetModel, GetService"是陈旧记录（公开 README 反而准确）。四维评分：架构 9 / API 8.5 / 工程可信 8 / 实用覆盖 5，总评 7.5——纪律教材 9 分，生产力框架 6 分。**Why:** 用户可能要求后续修复，避免重做 50+ 工具调用的全包审查。**How to apply:** 用户提"修锐评/审查发现的问题"时按此清单定位；上轮清单（2026-09-10 21:05 条目）已全部处置完毕勿再当待办。
- [2026-09-11 00:35:19] [project] RAA 第二轮锐评 10 条清单已全部修复完毕（2026-09-11 凌晨，叠在用户未提交的 Rider 格式化 pass 之上、未提交）：① AesirArchitectureAttributeProcessor 补条件 Warning（`@!$value.DontDestroyOnLoad` 表达式）；② SceneUnloadedTriggerProcessor 警告改条件显示（`$value.GetComponent(typeof(AesirArchitecture))+转型` 读宿主属性，宿主缺失不显示）；③ AbstractSubmodule.Dispose 重置 Initialized=false；④ RemoveListenerOnSceneUnloadedTrigger.Awake 补 `_instance = this`（三单例范式同形）；⑤ README 中英"零分配事件"补（Invoke 路径）限定 + 树补列 ObservableCollections/RuntimeInitializeLoadType（ObservableCollections 系修复时新发现，docs 子代理漏报）；⑥ 快捷档 Mvc-Quick+Mvp-Quick 两 Model 同改私有 [SerializeField] 字段+只读 `Count` 属性（Mvc-Quick 同款系修复时发现，为保 MVC/MVP 对照一致同改；序列化名 count 不变数据兼容）；⑦ 严格档 Mvc+Mvp 的 Controller/Presenter 缓存 Query 实例复用（带实例重载零分配，替代每次 new 的无参重载）；⑧ 新增 `IContext`/`AbstractContext.UnregisterModel/UnregisterService`（幂等无操作、摘除即 Dispose、再注册追加注册顺序末尾）+ 新测试文件 AbstractContextUnregisterTests（5 用例）；⑨ `AesirArchitecture` 新增只读属性 `DontDestroyOnLoad`（Odin @ 表达式经 C# 转型链访问 private 字段会被 C# 可见性挡住，public 属性是条件显示的必要前提）；⑩ CHANGELOG 新增 [Unreleased] 段（含新 API，按语义化版本发版应 bump 0.19.0，版本号待用户定）。验证基线：Bridge refresh 0 错误 0 警告；临时探针 EditorWindow 实测两轮 OnGUI 绘制（OnGuiCount=8 证非空洞通过）Odin 两个表达式零解析错误、控制台干净；TestRunnerApi EditMode 115 passed/0 failed/0 skipped 全绿（Initialized 重置未破坏任何既有用例）；Samples~ 7 文件 + Documentation~ 镜像已 cp 同步并验证。**Why:** 避免下轮把已修复清单再当待办。**How to apply:** 上一条 2026-09-10 22:45 锐评待修清单已全部处置完毕勿再引用；后续 Odin 条件 InfoBox 先例直接参照这两个 Processor 的 @ 表达式；工作树上还叠着用户自己的 Rider 格式化未提交改动（33 文件，纯注释换行/region 清理），提交时注意区分。
- [2026-09-11 12:18:42] [2026-09-11] RAM Events 模块分发增强 + SO 资产化已完成（工作树未提交，含用户自身未提交改动）：①订阅者过滤器 — ISubscriberFilter（ShouldReceive(args, subscriber, priority) 三参签名）+ AesirEventArgs.WithFilter/WithFilters 链式（[NonSerialized] 列表懒分配，未过滤时 null 零开销；EventModule 经 internal FilterList 拿具体 List 避免接口枚举装箱）+ 内建 WithTag/WithPriority/SameSceneAsEmitter/OnlySelf/InsideCollider2D，全部 fail-closed（解析不了 GameObject 就拦截，含已销毁假 null——TryGetGameObject 显式 Unity null 检查）；②死引用清理 — 分发循环内收集死绑定到复用列表、循环外从双注册表移除 + AesirModulesDebug.LogWarning（[Conditional("UNITY_EDITOR")]，玩家构建静默清理）；③性能监控 — [SerializeField] float executionMsLimit（默认 0 关闭）+ static Stopwatch 复用；④SO 资产化 — AesirEventArgsSO（发布者=资产本身）+ UnityEventOnAesirEvent（OnEnable 经非泛型 AddListener(this, eventArgs, cb) 订阅、OnDisable Dispose 句柄；运行时改类型需重新启用组件）+ SubclassSelector/ExcludeSubclassSelector（PropertyAttribute 必须在运行时程序集，Drawer 在 Editor/Events/ 汇入 Editor 程序集；UITK PropertyField(SerializedProperty) 构造即自绑定，无需 root.Bind）。裁决记录：系统事件/DefaultChannel 暂缓（依赖编辑器工具链）；单实例合并不实施（Awake 已销毁重复实例 + Instance 单一通道，重复实例不可能积累绑定）；AesirListener 补 AllowMultiple=true（文档早已宣称、Bind 本按多特性写）。**第二轮（同日）**：⑤热路径绑定键缓存 — AesirEventUtility.GetEventBindingKey 按 Type 缓存 AssemblyQualifiedName（原生拼接 ~1µs/次且每次新分配字符串=分发热路径唯一剩余分配点，缓存后 ~20ns 零分配），实测编译委托 ~4ns/次 vs MethodInfo.Invoke ~300ns（加速 ~78 倍）、1000 订阅者单次发布 ~571µs、Bind 冷路径 ~50µs/订阅者；性能特征回归测试 2 项（键缓存引用同一性 + 20 万次对比计时）+ 类型推断注册测试，EventModuleTests 共 22 用例全绿；⑥包内示例 — Samples/Events/02_Filters（场景 SampleForEventFilters.unity：Space=WithTag("Player")+InsideCollider2D 双重过滤、R=OnlySelf 家族命令，圈内/圈外/无标签+家族/无关对照组 + SampleHud uGUI 中文日志跨平台动态字体；运行时验证接收矩阵全对 + MP4 语义验证过）与 03_SOAsset（ScoreEventAsset.asset NewScore=100 + 场景 SampleForEventSOAsset.unity：UnityEventOnAesirEvent 持久绑定 ScoreBulb.Flash 零代码桥接 + ScorePublisher 代码订阅双消费演示；发布方法均提取为 public 便于教学与自动化驱动）；均登记 package.json samples、同步 Samples~ 镜像、中英 README/CHANGELOG/event-module.md（新增"性能模型（冷/热路径）"节+实测数据表）/CODELY.md/Feature-Roadmap 均已同步。**Why:** 用户要求按 Roadmap 实用性排序补齐事件模块核心能力+补示例+反射性能分析；功能选择按极简原则裁决。**How to apply:** 03_SOAsset 场景运行时 E2E（非泛型 AddListener + 持久 UnityEvent 桥接闭环）因并行会话持续占用编辑器（编译挂起+长任务）尚未跑完，已设一次性 cron 兜底重试；实测坑：**编辑器失焦时编译挂起 started 状态（osascript activate 解除）、exec_runtime_script 的 record_game_view 需 Play Mode 已激活且两次出现竞态、并行会话占用时 unity_job list 可见其任务勿打断**；团结 TestRunnerApi 签名是 RegisterCallbacks/UnregisterCallbacks + Execute(new ExecutionSettings(new[]{filter}))、TestStatus 是枚举非字符串；EditMode 跨场景用 NewPreviewScene。

- [2026-09-11 10:18:36] [2026-09-11 10:20:00] [project] RAA 注释口径约定（2026-09-11 全包注释核查定稿，0.19.0 之后、未提交）：源码注释只描述当前版本行为，废弃设计的历史叙述（"此前/原名/参考自/更名史"）一律写入包内 Documentation/设计变更记录.md（Documentation~ 有镜像；新增 .md 需手写 .meta——TextScriptImporter 模板 + uuidgen 小写 hex 防碰撞）。设计变更记录收录十组废弃机制（事件总线体系/ModelReplaced/120帧轮询/回滚/异常吞噬/BeforeFixedUpdate/GenericLocator.Global/Interface→Instance/DDOL标志组合/fake-null 隐式重置）+ 四项设计来源（QFramework PackageKit/Odin EnsureOdinInspectorDefine/Aesir Inspector 复刻与 AESIR_INSPECTOR 让位/Cysharp.ObservableCollections 边界参照）+ 命名演进速查表。已去重的重复注释先例：4 个能力标记接口 remarks、4 个 Execute 扩展方法 remarks、AbstractContext.Dispose 双重注释、ObservableValue 类级重复段。**Why:** 用户要求注释精简、指出当前版本最关键设计点、废弃点写外部文档；"设计史"原名被用户否决，定名"设计变更记录"。**How to apply:** 后续给 RAA 写注释勿引入历史对比叙述，演进记录去设计变更记录.md 增补；验证基线 refresh 0 错 0 警、EditMode 115/115；CS0108 已修（AesirArchitecture.DontDestroyOnLoad 属性加 new 修饰符，0.19.0 遗留）。遗留待办：文档站 API 页需一次重生成对齐（本轮 4 个 Editor 类 summary 变更：QuickCreateSOMenuItem/AesirUpdateService/AesirUpdateWindow/ScriptingSymbolUtility；叠加 0.19.0 新 API 成员 UnregisterModel/UnregisterService/DontDestroyOnLoad 属性）。
- [2026-09-11 10:18:53] [2026-09-11 10:18:00] [project] RAM ScriptDocGenerator 全模块锐评完成（2026-09-11，44 源文件约 9.7k 行全精读 + 文档对照 + 测试分布核查；报告仅会话交付未落盘）。总评 6.5/10：SourceScanner/数据层/索引缓存是 8 分内核，Default 生成器/双窗口/Summary 工具是 5 分外围。待修清单：**P0 正确性** ① DefaultScriptingAPISettingsSO.CreateEventsContent/CreateMethodsContent 入口阈值 `Length <= 1`（单事件类、单方法接口直接丢章节，应 `<= 0`）；② CreateFieldsContent 常量表过滤 `!IsApiMember() && !IsConstant` 的 `&&` 应为 `||`（public 非 const 字段混入常量表且双重展示）；③ CreatePropertiesContent flag 预计算漏 IsApiMember 守卫（仅私有继承属性→空"继承的属性"章节）；④ XmlCodePart.SummaryAttributeText 引号不转义（XML summary 含 `"` 生成非法 C#）；⑤ XmlSummaryTool 全文件 LF 化 + 空行折叠 + `////` 被 StartsWith("///") 误判为文档行；⑥ ScriptDocGeneratorUtility.TryGetFrontMatter 无闭合 `---` 时整个旧文件被当 Front Matter 拼回；⑦ TypeData 方法过滤 Contains("add_") 应 StartsWith；⑧ ReflectionUtility.GetAssembliesOfNameContainString catch 后 throw new Exception(msg) 毁堆栈、GetAttributes<T> 裸 catch 吞异常（违 fail-fast）。**P1 结构**：Default 生成器 578 行复制粘贴应统一到 Zensical 的 MemberGroup/AppendMemberSection 引擎（两生成器各 ~80 行配置）；IMethodData 应加 IParameterData[] Parameters（Zensical 现从格式化字符串反解析参数，~80 行 ParseParameters 应删）；param/returns/remarks/value/typeparam 收集管线全无生成器消费者（Remarks/Value 连数据类都没暴露）——要么渲染落地要么删半死管线；双窗口裁决建议 Odin 窗口为唯一主入口（UITK 版硬编码 Default 生成器、仅 2/4 模式、状态不落盘，工作流逻辑双写）。**P2 交互**：ScriptDocGeneratorPanelSO.OnEnable→ResetToDefault() 每次域重载清空用户全部配置（工作流杀手，应只清 _typeData/_typeDataList/_hasFinishedAnalyze）；程序集下拉未复用 ScriptAssemblyFilter 过滤（几百项）；分析阶段无进度条（生成阶段反而有）+ Toast"分析中"文案在完成后才弹；TypeDataProcessor 把全量 TypeData 整图画进 Inspector + Odin 序列化进 PanelSO .asset；Summary 工具 Remove 模式无确认直接改写磁盘源码；默认输出目录在 Assets/Editor Default Resources 内产生 .meta（应改项目根 Docs）。**P3 清理**：文档失实——"双向同步"实为单向 XML→[Summary]、"零等待毫秒级"程序集模式不成立、"三种粒度"实为 4 模式、测试表漏 SourceParsing(48)/XmlSummaryTool(18)/Misc 套件、DefaultAnalysisDataFactory 注释仍写"Aesir Inspector"；死代码——TypeData.TypeInfo 属性零消费、SourceSummaryInitializer.ClearCache() 无调用者（文件级 doc 缓存仅域重载失效）、ReflectionUtility.IsStatic(EventInfo) 分支 GetRaiseMethod 必 NRE（无调用路径）、GetReadableTypeName 的 EndsWith("obj") 截断魔法（无注释无测试）。**Why:** 用户可能要求后续修复，重做全模块审查需 69 工具调用。**How to apply:** 用户提"修 ScriptDocGenerator 锐评发现的问题"时按此清单直接定位，无需重审；逐条修复后更新此条目。评分：结构 7.5 / 正确性 6 / 性能 7 / 易用性 5.5 / 实用性 7。
- [2026-09-11 10:33:51] [2026-09-11 10:36:00] [project] RAM 音频模块（Audio Module）已完成（工作树未提交，与用户 Events 增强改动同存）：`Runtime/Audio/`（AudioModule 全静态门面 + AudioConfigSO）+ `Editor/Audio/`（预放置菜单 + Odin Processor/asmref）+ Tests/Editor/Audio（30 用例）+ Samples/Audio/01_BasicUsage（OnGUI 面板 + Python 合成的 4 个自包含 wav + 场景）+ audio-module.md/中英 README/CHANGELOG [Unreleased]/package.json samples，Samples~ 与 Documentation~ 镜像已同步。核心裁决：SFX 用 N 个独占 AudioSource 轮询（默认 8，等效池化、每播音调/音量独立——PlayOneShot 做不到且每播 Instantiate 有 GC）而非单源 PlayOneShot；BGM 专用 loop 源 + _bgmFadeFactor 独立系数淡变（unscaledDeltaTime，与音量链无写冲突）；三通道音量乘法链 + PlayerPrefs 持久化（键前缀 SO 可配）；公开 API 全静态（比 UIModule 双轨更贴音频场景）。设计边界：不做 3D（原生 PlayClipAtPoint 替代）/Mixer/每音效 Stop/播完回调。code-review-unity 锐评 8.6 分（架构 9/API 9/正确性 8/工程 9/实用 8），两个发现已修：① SwitchBgmRoutine 首播带 fade 先空转一个淡出周期（修为 !isPlaying 时 factor=0 直接淡入，StartCoroutine 同步到首 yield → PlayBgm 返回瞬间 isPlaying=True、factor≈0 可作确定性断言）；② PlaySfx 冗余 mute 写入（ApplyMutes 单一真源）。验证基线：refresh 0 错 0 警、EditMode 353/353、runtime 结构化断言 + MP4 语义验证通过。**Why:** 后续会话涉及音频模块扩展（AudioMixer/3D 源/节拍检测）时按此基线裁决，勿重做需求分析。**How to apply:** 实测坑位——① 验证协程渐变勿用时间窗采样（进 Play Mode 的卡顿帧 unscaledDeltaTime 不受 maximumDeltaTime 截断，一帧吃掉大半 fade 进度致误判），改用 PlayBgm 返回瞬间的同步断言；② 核心程序集访问 internal 需在 Runtime/Common/AssemblyInfo.cs 补 InternalsVisibleTo("Runestone.AesirModules.Tests")（BinderCodeGenerator.cs 里的同款声明位于 asmref 汇入的 Odin 程序集，不覆盖核心成员）；③ EditorSceneManager.SaveScene 不自动建父目录；④ EditMode 反射驱动重复实例 Awake 须 LogAssert.Expect(LogType.Error, "Destroy may not be called from edit mode")；⑤ CODELY.md 正文（模块表/程序集表/示例表）待用户手动补 Audio 行。
- [2026-09-11 15:03:38] [project] RAM UI 模块锐评已完成全量修复并验证通过（2026-09-11 15:00，用户指令"其他按照文档逐项优化"，P3-1 层级扩展/P3-2 per-panel Canvas 按用户裁决搁置仅文档声明；锐评报告 Docs/AesirModules/UIModule/01-模块锐评.md）。落地清单：① OnClose 语义修正——XML/README 明确"仅受控销毁路径（HidePanel+DestroyOnHide=true）调用，场景卸载/外部 Destroy 只走 OnDestroy，解绑须放 OnDestroy"；② loader 宣称收敛为同步契约（Addressables 需预加载后同步返回+WebGL Handle.Result 死锁警告写进 README/XML）；③ 键语义诊断三件套——基类类型重复 ShowPanel LogError 拒绝（不再重复实例化）、HidePanel/GetPanel LogWarning 提示实际类型、精确 miss 幂等静默（GetPanel 原警告删除，Hide/Get 口径统一）；④ UIRoot.Awake 删 RegisterUIRoot 推注册改纯拉（EnsureReady 自愈，Edit Mode 菜单 Create UIRoot 不再连带创建 [Aesir Modules] 宿主固化进场景）；⑤ UIModule 状态机重构——三字典（激活/停用/全集）合并为单 _panelDict，激活态由面板自身 IUIPanel.IsOpen 承担，"内部状态异常"不可达防御分支随之消灭，ShowPanel/PrewarmPanel 共用 InstantiateAndAttach 单实现（4 处挂层复制粘贴→1），HidePanel<T> 泛型约束统一 MonoBehaviour,IUIPanel；⑥ 缺层 fail-fast——层 Canvas 缺失时 Show/Prewarm 记录错误并中止（不保留半挂载僵尸实例）；⑦ IUIAssetLoader.Unload 死接口删除；⑧ UIRoot.EnsureDefaultCanvasConfigAsset 共享实现（Inspector 按钮与 Assets/Create 菜单幂等统一，消灭双份实现）+ UIModuleMenuItems 复用；⑨ UIModule 顶部空 #if ODIN_INSPECTOR 残留清除。新增 UIModuleTests 13 用例（Tests/Editor/UI/）+ Documentation/ui-module.md 专属文档（手写 .meta）+ 中英 README UI 章节全量修订（核心类型表+目录结构补 AesirBasePanelViewController、键语义警示、加载契约、设计边界节：层级封闭集/per-panel Canvas 取舍/主相机排 UI 层）+ CHANGELOG [Unreleased]（Changed/Removed 破坏性标注：RegisterUIRoot、Unload、HidePanel 约束、菜单幂等、缺层中止）。**验证基线：refresh 0 错 0 警；UIModuleTests 13/13 全绿；全 Tests 程序集 377/377 零回归；Documentation~ 镜像一致。**Why:** 后续 UI 模块改动以测试+文档为基线；How to apply: 键语义/OnClose/加载契约的解释以 ui-module.md 为权威口径；测试断言日志一律用 CaptureLogs 自捕获模式（见下）。

- [2026-09-11 11:15:56] [2026-09-11 11:05:00] [project] RAM Scene 模块锐评完成（2026-09-11，code-review-unity 全量模式 + Runtime/Scene 6 文件与 Editor/Scene 5 文件全精读 + README/gitignore 宣称对照 + grep 消费者核查；报告已落盘 Docs/AesirModules/SceneModule/01-模块锐评与优化方案.md）。总评 7/10（架构 8/API 7/工程可信 5.5/实用覆盖 7）——SceneAssetWrapper 是包内最佳单类型（9 分，Eflatun 对位+GUID 自愈+异常族+27 用例），SceneModule 是包内生命周期最粗糙单例（4.5 分）。待修清单：**P0** ①预放置 SceneModule 无 dontDestroyOnLoad 字段，被自己的 LoadSceneSingle 杀死（协程随宿主销毁、onCompleted/onFailed 静默丢失；修复=照抄 UIModule 补 DDOL 字段+Awake 重复实例 Destroy(gameObject)→Destroy(this) 对齐 RAA 先例）；②Single 模式 _addedScenePaths.Clear()（L296-298）发生在 LoadSceneAsync（L301）之前，加载失败丢追踪（移到 yield return op 后）；③ScriptableSingleton.instance 小写团结特有 API 共 9 处（BootstrapSceneHelper L32/89/130/142/163/165/171/173 + SceneManagerWindow L19），标准 Unity CS0117 编译失败，破坏 Unity-2022.3+ 徽章宣称（修复=照抄 BinderEditorSettings 反射封装 GetProperty("Instance")??GetProperty("instance")）；④SceneEditorSettings FilePath 用 ProjectEditorSettings/ 违反 2026-09-05 ScriptableSingleton/ 前缀裁决且未被 .gitignore 覆盖（改 ScriptableSingleton/AesirModules/ 自动被忽略）。**P1** ⑤非 Odin 环境 wrapper Inspector 完全不可用（四序列化字段全 HideInInspector、唯一集成是 Odin Processor，README 宣称 Odin 可选——补原生 PropertyDrawer 或 README 明示）；⑥BootstrapSceneHelper 预设名搜索早断 bug：FindAssets 子串（大小写不敏感）命中即 break + 文件名精确比较（大小写敏感）双标，Foo_Bootstrapper 之类会吞掉真实 bootstrap 场景的注册；⑦README"启动场景"段把编辑器 opt-in（SetupBootstrapper 默认 false）的 BuildSettings 重排写成运行时自动行为，运行时 AutoSetupBootstrapScene 实为 GetSceneByName 只查已加载场景且零下游消费者（DetailedInfoBox"搜索 BuildSettings"文案同样失实）；⑧SceneModule 零测试（27 用例全在 wrapper，行为层 bug 恰在空白区）；⑨菜单 Tools/场景管理方案设置窗口 违反 Tools/Aesir/ 约定且窗口名不副实（30 行 InlineEditor 壳）。**P2** 重复 Additive 同路径孤儿实例（追踪一条、卸载清记录）、死 API 三件（BootstrapScene stale Scene 快照/BootstrapSceneAssetWrapper/GetTotalLoadingProgress 全仓零消费者）、UnloadAllAddedScenes 无 op null 检查且无 onFailed、TryGetAddress XML"与 Address 一致"失实（Address 抛 Empty、TryGet 返 false）、PresetBootstrapSceneNames public static readonly string[] 可变公开、_instance 无 RIOLM 重置、引用悬空只有颜色无 InfoBox。**裁决点（修复 P0/P1 后单独过会）**：C1 SetActiveScene 透传 API（建议加）、C2 SceneLoadedEvent/SceneUnloadedEvent MiniEvent 集成（建议加）、C3 进度二选一（建议加 per-load onProgress 删 GetTotalLoadingProgress）、C4 BootstrapScene 删/BootstrapSceneAssetWrapper 留。极简裁定：数据层是正面教材，债全在行为层半成品——补 10 行高频 API 与删死面并行，不引入防御代码不做 async。**Why:** 用户可能要求后续修复，重做全模块审查需 40+ 工具调用。**How to apply:** 用户提"修 Scene 模块锐评发现的问题"时按此清单直接定位，无需重审；逐条修复后更新此条目。
- [2026-09-11 14:59:26] [project] 【本条目为 12:04 修复条目的验证收尾，合并更新】RAM Scene 模块锐评修复已全部落地并验证通过（2026-09-11）。**验证基线**：编译 0 错 0 警；Scene.Tests EditMode **45 Pass / 0 Fail / 2 Skip**（wrapper 25 + SceneModuleTests 20；2 Skip 为 Addressables 装包自适应用例属预期）；Tests asmdef 补 Sirenix×4 precompiledReferences（overrideReferences:true 时 SerializedMonoBehaviour 链必需，同 Runestone.AesirModules.Tests 先例）+ Runestone.AesirArchitecture 引用。测试踩坑：①AesirModulesDebug 富文本前缀日志（<color> 标签）与多行 Destroy 错误——LogAssert.Expect 纯字符串匹配不上，**一律用 Regex**；②反射驱动重复实例 Awake 在 EditMode 会抛 "Destroy may not be called from edit mode"——LogAssert.Expect(LogType.Error, Regex) 吞掉即可走通分支；③主实例挂非根物体可避开 DDOL 分支的 EditMode 场景迁移断言。**并行会话编译管线大坑（本次耗 2 小时排查）**：单程序集编译错误（先是 SceneModuleTests 的 CS0012/CS0121/CS1061，后是并行 UI 会话 UIModuleTests）会**冻结整个编译管线**——Bridge isCompiling 卡 started、refresh 超时 await-editor-idle、磁盘 DLL 已新但域重载永远不发生（md5 不变）；解法链=修掉冻结源 → osascript 按 PID 精确前置（tell process "Unity" set frontmost，比 activate 可靠）→ Assets/Refresh 菜单或 CompilationPipeline.RequestScriptCompilation() → 域重载后 custom_tools_reloaded 通知 → 反射验证域内类型存在再跑测试；strings/python 字节级搜 DLL 类型名可区分"磁盘 DLL 已含类型"与"域未重载"；Unity batchmode 同项目不可双开（项目锁）。**How to apply:** 后续并行会话抢编辑器时：先查 grep "error CS" Editor.log 归属文件 → 修冻结源 → PID 前置+RequestScriptCompilation → 等 custom_tools_reloaded → 跑测试；遗留两项不变（Samples/Scene/01_BasicUsage 示例、EditorSyncFromAsset 缓存均不做/待裁决）。

- [2026-09-11 13:06:21] [2026-09-11 13:15] [project] 【本条目覆盖 2026-09-11 10:18:53 "RAM ScriptDocGenerator 全模块锐评完成"条目，其待修清单已处置，勿再当待办】RAM ScriptDocGenerator 锐评清单已基本修复完毕（2026-09-11，按用户指令逐项修复，工作树未提交、与 Events/Scene/UI 并行会话改动同存）。已落地：**P0 全部 8 项**（事件/方法阈值 <=0、常量表 ||、属性 flag 守卫、引号转义、EOL 保持+//// 判定、Front Matter 无闭合返回 false、StartsWith、ReflectionUtility 去 catch）；**P1 部分**（IParameterData[] Parameters 参数链、六种 XML 标签全链路到数据类、Zensical 说明列全渲染+删 80 行字符串反解析、删 UITK 窗口；**未做：Default 生成器引擎合并**）；**P2 全部**（OnEnable 只清分析态、程序集下拉过滤、Odin 原生 Toast+文案、调试检查模式 debugInspectionMode 默认关 + NonSerialized+ShowInInspector 仅调试渲染 + 程序集条件 Warning InfoBox `@$value.ShowAssemblyDebugWarning` 三参构造、默认输出目录项目根 ScriptDocGenerator/（.gitignore 已登记）、Summary 工具 Remove 确认/无变化跳过/批量单次 Refresh）；**P3 全部**（中英 README/模块文档语义与测试表修正、Zensical 章节、死代码清理）。**Summary 工具新语义（用户裁决 A）**：[Summary] 特性为权威源——Sync 特性存在时回写 XML 对齐、无特性回退 XML 生成；Replace 特性优先；拼接实参不可解析按无特性回退。新增 DefaultScriptingAPIOutputTests（4）+ XmlSummaryToolTests 新语义 7 用例（总 25）。**验证状态**：我的改动 refresh 0 错 0 警；EditMode 510 pass/7 fail 中 6 fail 属并行 UI 会话 UIModuleTests、1 fail 是 DLL 陈旧（编译管线被并行 Scene 会话 SceneModuleTests 19 个编译错误冻结，我的 rename 用例与新增测试未进 DLL）；新语义经反射探针全部直接验证通过（条件矩阵 False/True/False、OnEnable 保留配置/清分析态、Odin ShowToast 签名 public 匹配、Preprocessor Replace 行为）。**Why:** Scene 会话修复后需重跑测试确认全绿；P1 引擎合并未做。**How to apply:** 剩余项=①Scene 修复后重跑 refresh+测试；②Default 生成器重写为 AppendMemberSection 配置（578→~80 行）；③文档站 API 页重生成吃进 Zensical 新说明列。踩坑：并行会话占编辑器时 exec_editor_script 排队/超时/结果丢失，反射探针比等测试全绿可靠；Unity 同项目不能双开 batchmode（项目锁）；TestRunnerApi 多次 Execute 会互相取消（单次 Execute 带全部程序集名）。
- [2026-09-11 15:04:04] [project] 【团结 2022.3.62 EditMode 测试方法论大坑位汇总（UI 会话实测，两小时排障代价）】① AesirModulesDebug 富文本前缀日志（<color> 标签）+多行 Destroy 错误——**LogAssert.Expect 纯字符串参数完全匹配不上，必须 new Regex(子串)**；但 Event 会话同款 Regex 用例通过而 UI 用例始终 Unhandled 的"玄学"另有真因（见④）。② 本引擎 LogAssert.ignoreFailingMessages 的 setter **自身会打 Log 级调试日志 "\nIgnoreFailingMessages:true/false"**——对日志做 IsEmpty 断言会误踩，改为过滤 LogType.Log 级别。③ EditMode 中 UIModule 的 Destroy 调用被引擎拒绝（"Destroy may not be called from edit mode" Error 但 no-op）——用例需 LogAssert/CaptureLogs 捕获该 Error，且被中止的克隆体悬在世界空间根（不在 UIRoot 子树，TearDown 连带销毁不可达）须 FindObjectsByType 扫描登记清理。④ **域重载锁死（本次最大坑）**：exec_editor_script 客户端超时被杀的 TestRunnerApi run 会遗留下未配对的 LockReloadAssemblies——此后磁盘 DLL 更新（python 字节级 UTF-16 搜断言文本可证实）但**域永不重载**，RequestScriptReload/CompilationPipeline.RequestScriptCompilation(CleanBuildCache)/touch 全部无效，测试永远跑旧代码且症状与"LogAssert 不匹配"高度相似（诊断关键：反射探测域内类型是否含新方法 + 比对堆栈行号）；解法=反射调 EditorApplication.UnlockReloadAssemblies()（私有静态，多解两次无害）+ 重新 RequestScriptReload + 激活编辑器（进程名是 **Tuanjie** 不是 Unity！osascript activate "Unity" 静默无效，须 `tell application "Tuanjie" to activate`，可用 System Events frontmost 验证）→ 落地标志是 custom_tools_reloaded 通知。⑤ TestRunnerApi.RegisterCallbacks **跨 submission 持久累积且不随域重载清理由 Bridge 重建**——历史回调会在每轮 run 里全部触发（日志混流多份 [PROBE-RESULT] 类前缀是已注销会话的遗留回调，非当前 run 结果），读测试结果只信当轮回调前缀与 RunFinished 汇总数字。⑥ TestRunnerApi 每次新建实例应 UnregisterCallbacks（团结签名 RegisterCallbacks/UnregisterCallbacks）防泄漏。**How to apply:** 跑测试发现"改了代码结果永远一样"时：先 python 字节级查磁盘 DLL 是否含新断言文本（区分"编译没跑"vs"域没重载"）→ 反射探测域内方法 → UnlockReloadAssemblies+RequestScriptReload+activate Tuanjie → 等 custom_tools_reloaded → 反射复验 → 再跑。UI 测试的 CaptureLogs 自捕获模式（logMessageReceived+ignoreFailingMessages 抑制）是日志断言的稳妥范式，UIModuleTests.cs 可复制。

### Reference
- [2026-08-15 22:20:34] AttributeOverviewPro 资产精简方案文档位于 Docs/AttributeOverviewPro-AssetReduction-Plan.md — 包含现状分析、可行性评估、子资产架构设计、详细实现步骤、验证步骤和备选方案。
- [2026-09-05 15:26:38] sync-samples 技能位于 .codely-cli/skills/sync-samples/ — **2026-09-05 起旧工作流已失效**：Assets/Samples 导入副本已从仓库删除，Samples 双目录新规为"包内 Samples/ 为编写主位，内容同步到包内 Samples~/ 发布镜像（含 .meta，GUID 与 UPM 导入链路一致）"；该技能的 Assets/Samples → Samples~ 扫描逻辑不再适用，同步改为直接 cp -R。

