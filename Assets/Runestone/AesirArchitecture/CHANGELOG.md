# Changelog

本项目的所有重要变更均会记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/)。

## [0.17.0] - 2026-09-06

### Added

- `InternalContextAttribute`：标记框架内部 Context（示例 / 测试等非用户工作流用途）的元数据特性；AesirModules Binder 的「Context 类型」选择器会跳过被标记的类型
- 包内全部示例与测试 Context（含嵌套）标注 `[InternalContext]`

### Fixed

- **示例场景无法运行（0.14.0 起回归）** — 示例程序集从 Editor-only（`includePlatforms`）改为运行时程序集 + 示例脚本整文件 `#if UNITY_EDITOR` 包裹：Editor-only asmdef 的 MonoBehaviour 被 Unity 判定为"编辑器脚本"、禁止挂载场景物体，导致全部示例场景组件 Missing Script；重构后编辑器内正常编译、挂载与 Play Mode 运行，玩家构建仍整体剔除（示例类型 0 入包）

## [Unreleased]

### 规划中

- ScriptableObject 可视化配置层
- Editor 工具链（SO Inspector / MVP 脚手架 / 模块可视化）
- 运行时集合（RuntimeSet）

## [0.16.2] - 2026-09-06

### Changed

- **`LICENSE.md` 与 `Third Party Notices.md` 移至包根** — 对齐 UPM 包根约定（Package Manager 识别包根的 Third Party Notices），包 README 的 LICENSE 徽章链接随之生效；版权行统一为 Runestone Yuumix

## [0.16.1] - 2026-09-06

### Changed

- **Third Party Notices 收录 ObservableCollections（Cysharp，MIT）** — 设计参考条目：可观察集合高级能力（Move / Sort / 同步视图 / R3 集成）的推荐替代库；注明仅设计参考、未包含源码
- 根 README（中英）推荐链接新增 ObservableCollections

## [0.16.0] - 2026-09-06

### Changed

- 版本号与 Aesir Modules 同步更新至 `0.16.0`，本包本版本无功能性变更

## [0.15.0] - 2026-09-05

### Added

- **可观察集合 `ObservableHashSet<T>`** — 与 List / Dictionary 同构：组合 `HashSet<T>` 存储 + `MiniEvent` 零分配事件；事件负载单值直传 `Action<T>`（对齐 Dictionary 的 KeyValuePair 直传先例）；只读接口 `IReadOnlyObservableHashSet<T>` 为不变型（接口自行声明的 `Contains(T)` 处于输入位）；新增 ObservableHashSetTests 14 个
- **RuntimeInitializeLoadType 示例** — 编辑器窗口演示 `RuntimeInitializeLoadType` 五个初始化时机的执行顺序与静态重置最佳实践（附讲解文档）；菜单归位 `Tools/Aesir/Architecture/Samples/RuntimeInitializeLoadType`；ScriptableSingleton + `[FilePath]` 落盘统一 `ScriptableSingleton/` 前缀（`.gitignore` 忽略运行时生成目录）

### Changed

- **包内更新器大陆优化（jsDelivr 方案）** — 版本检测三级兜底：jsDelivr 四域名（cdn / testingcf / gcore / fastly）拉取仓库内 `.github/update-info.json` → GitHub Releases API → `releases/latest` 302 重定向探测（完全绕开 API 限流）；unitypackage 下载仍走 GitHub Release 直链
- **ObservableDictionary.Remove(TKey) 优化** — 改用 `Dictionary.Remove(key, out TValue)` 单次哈希查找取回旧值
- **Odin 编辑器处理器命名空间对齐** — `Editor.OdinIntegration` → `Editor.OdinInspector`（与 0.9.0 程序集/目录改名对齐；纯命名空间标识符变更）
- **文档核查整理** — 包中英 README 修正过时描述（GenericLocator 全局单例旧述、结构树失效条目），补包内更新器与 unitypackage 安装说明；Skill core-api.md `GameContext.Interface` → `.Instance`

### Fixed

- **PlaneWar 场景引用修复工具** — 原硬编码 `Assets/Samples/.../0.12.0` 路径在 Samples 双目录迁移后失效，改为 `FindAssets` 动态定位（兼容包内 `Samples/` 与 UPM 导入副本两种布局），预制体缺失时报错而非静默跳过

> 注：本文件曾漏记 0.5.0 – 0.14.0 的逐版本条目，该区间历史见仓库根 CHANGELOG（聚合视图）。

## [0.14.0] - 2026-09-05

### Changed（仓库结构调整）

- **AesirFramework 转型** — AesirInspector 已从本仓库移出，作为面向 Odin Inspector 开发者的学习工具包独立发布；本仓库更名定位为 AesirFramework，由 Aesir Architecture 与 Aesir Modules 组成
- **Samples 双目录结构** — 包内同时提供 `Samples/`（仓库内直接可见可运行）与 `Samples~/`（Git URL 安装后经 Package Manager 按需导入的源镜像）；以 `Samples/` 为编写主位，内容同步至 `Samples~`，两份内容保持一致
- **示例程序集构建剔除** — 全部示例程序集（asmdef）`includePlatforms` 收窄为 `Editor`，示例脚本与资产（无 Resources 目录、无构建场景引用）不会进入玩家构建包
- **示例命名空间统一** — 计数器六档与 PlaneWar 示例统一为 `Runestone.AesirArchitecture.Samples.<示例名>`（MvcQuick / MvcStandard / MvcStrict / MvpQuick / MvpStandard / MvpStrict / PlaneWarMono）；`MiniEvent` 与 `ObservableValue` 两示例因命名空间段与所演示的框架类型同名冲突（CS0118），保留 `Runestone.AesirArchitecture.Samples` 前缀命名空间

### Added

- **PlaneWar 实战示例（Mono 版）** — 纵版射击飞机大战：自包含素材、得分 HUD、三型敌机与重开流程；已注册进 package.json samples，并纳入 Samples 双目录同步

## [0.13.0] - 2026-09-03

### Added

- **可观察集合 `ObservableList<T>` / `ObservableDictionary<TKey, TValue>`** — 组合 `List<T>` / `Dictionary<TKey, TValue>` 存储 + `MiniEvent` 零分配事件（与 `ObservableValue<T>` 同一套读写分离与事件模式）：
  - List 事件：Added / Removed / Replaced / Cleared；Dictionary 事件：Added / Removed / Updated / Cleared
  - 结构体事件参数 `CollectionAddEventArgs<T>` / `CollectionRemoveEventArgs<T>` / `CollectionReplaceEventArgs<T>` / `DictionaryUpdateEventArgs<TKey, TValue>`（readonly struct，零分配）
  - 只读接口 `IReadOnlyObservableList<T>` / `IReadOnlyObservableDictionary<TKey, TValue>` 为不变型（无 `out` 协变——结构体事件参数与协变冲突，CS1961），Model 持有可写实例、View 按只读接口订阅，可写接口为 `IObservableList<T>` / `IObservableDictionary<TKey, TValue>`
  - 监听 API 与 ObservableValue 一致：`AddXxxListener` 返回 `AutoRemoveListenerHandle` 自动清理，`RemoveXxxListener` 手动移除
  - 新增测试 25 个（ObservableListTests 11 + ObservableDictionaryTests 11 + ContextReplacementWarningTests 3）

### Changed

- **Context 动态替换模块输出 Warning** — `RegisterModel` / `RegisterService` 键命中已有实例时输出一条 Warning 日志（`LogReplacementWarning`），提醒旧实例被 Dispose 后其上的事件订阅（MiniEvent / ObservableValue 等）不会自动迁移；首次注册不输出日志，替换本身仍为合法操作（测试/调试用途）
- **README 中英文同步** — 特性清单新增"可观察集合"条目；设计边界新增"集合可观察全家桶不做"（Move、Sort、同步视图、R3 集成等高级能力推荐 [Cysharp.ObservableCollections](https://github.com/Cysharp/ObservableCollections)）；目录结构树 Observable/ 注释更新

## [0.12.0] - 2026-08-22

### Added

- **AI Skill 文档集** — `Documentation~/AesirArchitecture-Skill/` 新增 AI 编码指南，供 AI 助手按 RAA 架构规范快速生成代码：
  - `SKILL.md` — 主指南：MVC/MVP 模式决策树、Quick/Standard/Strict 三档分级表、核心 API 速查、事件机制决策表、10 条关键约定、命名规范与文件组织指南
  - `references/mvc-quick.md` / `mvc-standard.md` / `mvc-strict.md` — MVC 三档完整代码模板（Context + Model + View + Controller + Command + Query）
  - `references/mvp-quick.md` / `mvp-standard.md` / `mvp-strict.md` — MVP 三档完整代码模板（Context + Model + View + Presenter + Command + Query）
  - `references/core-api.md` — 辅助模块 API 速查（ObservableValue、MiniEvent、Context、GenericLocator、PlayerLoop 等）

## [0.11.0] - 2026-08-22

### Breaking Changes

- **DDOL 机制重设计：预放置/运行时创建统一由 `dontDestroyOnLoad` 序列化字段控制** — `AesirArchitecture` 新增 `[SerializeField] bool dontDestroyOnLoad = true`，两种实例来源共用同一份决策：默认勾选时（含场景预放置实例）一律在 Awake 加入 DontDestroyOnLoad 场景（此前预放置实例保留在场景中）；取消勾选时实例保留在所在场景、随场景卸载销毁，必须自行处理多场景叠加（Additive）加载——Inspector 显示警告 InfoBox（Odin），运行时输出提醒日志（仅编辑器期）。移除 `static bool _pendingDontDestroyOnLoad`（字段默认值天然覆盖运行时创建路径）与 `_isPrePlaced` / `IsPrePlacedFieldName`（警告条件改绑新字段）；`RemoveListenerOnSceneUnloadedTrigger` 的 Inspector 警告文案同步改为"宿主 dontDestroyOnLoad 关闭"语义。测试更新：`ResetStatics` 测试去除 pending 字段断言，新增 `DontDestroyOnLoad` 默认值测试（41 → 42）
- **MVP 快捷档更名：`Counter-Mvp-Simple` → `Counter-Mvp-Quick`** — 档位命名与 MVC 对齐（简单档 → 快捷档）；类名 `SampleMvpSimpleCounter*` → `SampleMvpQuickCounter*`、asmdef `MvpSimple` → `MvpQuick`、场景与预制体同步更名（GUID 不变，已导入副本的引用不断）。已导入 0.10.0 示例的项目请删除旧导入副本后重新导入
- **MVP View 统一为纯 `MonoBehaviour`** — 三档 View 不再继承 `MonoView<T>`：被动视图不携带任何 Context 能力，与 View 契约"不继承 `IView`"的 MVP 边界闭环；`ISampleMvpSimpleCounterView` 接口移除（快捷档零接口抽象，Presenter 直接持有具体面板类），标准档起 View 契约以 `IXxxView` 接口形式存在

### Added（渐进式示例家族扩为六档）

- **示例家族从 4 个扩为 6 个渐进档位**（MVC / MVP 各三档）：
  - `Counter-Mvc-Standard`（MVC-2 标准档，新设）— Model 只读暴露 + 写方法（具体类注册）；View 与 Controller 分离实例、共享同一 Model 实例，写方法直调不经 Command（第二课）
  - `Counter-Mvp-Standard`（MVP-2 标准档，新设）— Model 只读暴露 + 写方法（具体类注册）；Presenter 直调写方法 + Model 直读推送（第二课）
  - `Counter-Mvc-Quick`（MVC-1 快捷档）— 移除 Model 接口（`ISampleMvcQuickCounterModel`），快捷档零接口抽象（第一课）
  - `Counter-Mvc-Strict`（MVC-3 严格档）— Model 接口注册 + 只读暴露 + 写方法；View 与 Controller 分离，写入经 Command、加工读取经 Query（第三课）
  - `Counter-Mvp-Quick`（MVP-1 快捷档）— Presenter 直改可写 ObservableValue（零接口抽象）并推送刷新被动 View（第一课）
  - `Counter-Mvp-Strict`（MVP-3 严格档）— Command 写 + Query 读；View 按业务窄接口持有 Presenter（第三课）
- **MVC-3 严格档重设计** — View 按接口类型持有 Model 并订阅刷新（撤销"View 零持有 + Query 拉取"旧口径）；Controller 抽为纯 C# 类（`IController<T>`），由 View 在 Start 中 new 出，写入经 Command；Query 收窄为加工值场景（`GetRoundedCountQuery` 十位四舍五入示例替代 `GetCounterValueQuery`），返回原始值无需 Query；三档 View 统一 Start 中 GetModel 缓存 + 事件订阅的初始化口径
- **严格档双接口设计** — MVC-3 新增 `ISampleMvcStrictCounterController` 业务窄接口（独立文件，不继承 `IController`）；MVP-3 `ISampleMvpStrictCounterPresenter` 改为业务窄接口（`SyncInitialValue` + `IDisposable`，不继承 `IPresenter`）。View 按窄接口存储 Controller / Presenter（与 Model 接口存储对称），类型层面拿不到 `ExecuteCommand` / `GetModel` 等框架能力——读写分离由类型系统闭环
- **DDOL 开关字段级 InfoBox** — `dontDestroyOnLoad` 字段的 `[Tooltip]` 迁移为 AttributeProcessor 经 `ProcessChildMemberAttributes` 注入的 Info 级信息框（取值含义三行说明，恒显示），运行时程序集零 Inspector 样式特性

### Fixed

- **DDOL 警告 InfoBox 可见性反转** — Odin `InfoBoxAttribute` 的 `visibleIfMemberName` 对 bool 成员为"true 时显示"语义（与 ShowIf 一致），旧写法导致警告在开关开启（安全态）时显示、关闭（风险态）时反而静默；改用 `"@!" + 字段名` 表达式反转，关闭开关时正确显示警告（PropertyTree + drawer 链探针验证 8/8 PASS）

### Changed（文档与格式）

- **README 新增「示例（Samples）」总览节** — 8 个示例按 MVC 系列 / MVP 系列 / 工具类三组呈现双三档对照表（Model 暴露面 + 读写路径 + View 边界）；项目结构树同步；移除「与 QFramework 的差异」对比章节
- **英文 README 同步** — 示例总览节 + 安装 URL 修正补 `?path=Assets/Runestone/AesirArchitecture` 参数（与中文版及 monorepo 安装规范一致）
- **package.json samples 六档条目** — MVP-Quick 更名 + MVC-Standard / MVP-Standard 新增，8 个示例齐整
- **《事件机制决策表》MVC-3 口径修正** — MVC-3 原始值仍走 ObservableValue 订阅刷新，Query 仅用于加工值（如四舍五入）
- **全包 XML 注释与代码格式统一** — `<see />` 闭合空格、`<para>` / `<item>` 缩进、空行与断行规范；示例 asmdef 缩进统一

## [0.10.0] - 2026-08-19

### Breaking Changes

- **`AbstractContext<T>.Interface` 更名 `Instance`，返回类型 `IContext` → `T`** — 消除与 C# 关键字 `interface` 的术语混淆；返回具体类型使 Context 子类自定义成员免强转（协变友好：管道场景自动向上转型）。迁移：全局替换 `.Interface` → `.Instance`；此前依赖 `((T)Interface)` 强转的代码可直接去掉强转

### Fixed（框架运行时一致性）

- **`AesirArchitecture` 根单例补类内静态重置** — 非泛型类按铁律类内声明 `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]`，此前依赖 Unity fake-null 隐式救援（"碰巧正确"而非"按原则正确"）
- **`GenericLocator<T>` 保序 + `AbstractContext.Dispose` 真逆序** — 增加插入序 `List<Type>`，`GetAll()` 按注册顺序枚举（不再依赖 `Dictionary` 枚举顺序这一无契约保证的实现细节）；`Dispose()` 各容器内部按注册逆序销毁（此前为正序，与自身注释"逆序"矛盾）
- **未注册异常近失识别** — `GetModel<T>` / `GetService<T>` 未命中时，若已注册实例中存在可赋值给查询类型的条目，异常消息追加「Register 与 Get 必须使用相同类型参数」提示（典型触发：按实现类注册、按接口查询）
- **`IModel` 注释纠错** — 能力列表去除误述的 `GetService`（接口实际未继承 `ICanGetService`）

### Fixed（示例与框架承诺对齐）

- **MVP 被动视图接口去 `IView`** — `ISampleMvpCounterView` 不再继承 `IView`（`IView` 自带 `GetModel` 能力，"被动视图不访问 Model"从接口层面不成立）
- **MVP 事件化** — View 契约 `Action` 属性改为 `event`（公开 setter 允许外部替换/置空/Invoke 委托链，event 编译期限制只能 `+=`/`-=`）
- **按钮监听精确配对** — 示例 `OnDisable` 的 `RemoveAllListeners()` 改为与 `OnEnable` 逐一对称的 `RemoveListener(...)`（一刀切会误清其他系统挂的监听）
- **MVP 标准档写入改走 Command** — Presenter 直写 Model 改为 `ExecuteCommand`（与 MVC 标准档共享"表现层写入必经 Command"铁律）
- **场景初始值同步** — MVC 订阅改 `AddListenerAndInvoke`（订阅即同步初始值）、MVP 经 `SyncInitialValue()` 主动推送，消除场景残留文本与 Model 初始值不一致
- **`IModel` 能力注释** — 服务定位属性（`Model => this.GetModel<T>()`）补每帧路径警告

### Added（渐进式示例家族）

- **示例家族从 2 个扩为 6 个渐进档位**（MVC/MVP 各三档）：
  - `Counter-Mvc-Quick`（MVC-1 快捷档）— `MonoViewController<T>` 直写直读，~5 文件最小闭环（第一课）
  - `Counter-MVC`（MVC-2 标准档，已移除）— Command 写入 + 独立 Controller
  - `Counter-Mvc-Strict`（MVC-3 严格档）— 只读 Model + Command 写 + Query 读，View 零持有（进阶）
  - `Counter-Mvp-Simple`（MVP-1 简单档）— Presenter 直写 Model
  - `Counter-MVP`（MVP-2 标准档，已移除）— Presenter 走 Command
  - `Counter-Mvp-Strict`（MVP-3 严格档）— Command 写 + Query 读
- **Model 暴露面分档** — 通常档（快捷/标准）直接暴露可写 `ObservableValue<T>`；严格档收窄为 `IReadOnlyObservableValue<T>` 只读接口 + 写方法；全家族 Model 统一为 `[SerializeField]` 字段 + 属性转发形式（可被 Unity 原生与 Odin 序列化显示）
- **`OrderingAndLifecycleTests`** — 新增 7 个 EditMode 测试（保序/逆序/近失识别/根单例静态重置），测试总数 34 → 41
- **《事件机制决策表》《常见陷阱清单》** — `Documentation~/` 新增两份教学文档

### Changed（文档）

- **README 快速开始对齐真实示例** — `MonoView<T>` + 无参 Controller（此前 `AesirView` + 构造注入两张皮）；Model 示例改可写 `ObservableValue`（与通常档示例现实一致）
- **写入约定三档口径** — 设计边界表新增「写入纪律档位」：快捷/简单档直写合法，标准档起表现层写入必经 Command，严格档只读 + 写方法，Service 可直写
- **三档渐进路径** — 快速开始后新增三课路径说明（快捷 → 标准 → 严格）
- **设计原则第 7/8 条** — 新增「Inspector 精简原则（AI 优先）」与「Odin 三条铁律」（核心闭环不依赖 Odin / 调试器等体验优化品可用 / 样式与逻辑分离）
- **Odin 定位澄清** — README 安装节补「Odin 为可选增强，非运行前置」；清除全文中「Odin 可选」与「Odin 必需」的冲突表述
- **英文 README 同步** — 快速开始代码逐字一致 + 设计边界/原则对齐

## [0.9.0] - 2026-08-15

### Changed

- **MVC 优先定位** — package.json description 从"渐进式 MVP/MVC 架构框架"改为"渐进式 MVC 架构框架"；README/README_EN 概述与特性列表同步调整，`IController`（MVC）为推荐入口，`IPresenter`（MVP）为可选严格模式
- **目录重构：Core / Modules / Common 三层分离** — Runtime/ 下从 `Component/` + `Engine/` 两级扁平结构改为三层：`Core/`（含 `Component/` 与 `Engine/`，核心 Context 上下文与 MVC/MVP 架构）、`Modules/`（5 个辅助模块：`Event/`、`CustomLifecycle/`、`Locator/`、`Observable/`、`Utilities/`）、`Common/`（框架基础设施：`AesirArchitecture`、`AesirMonoBehaviour`、`AesirArchitectureDebug`、`AssemblyInfo`、`ResetStaticsAssistant`）
- **极简化：事件系统统一原生 C# 语义** — `MiniEvent.Invoke` / `MiniEvent<T>.Invoke` 恢复直接多播调用（零分配，恢复核心卖点）；移除 `MonoLifecycleProxy.InvokeEvent` 与 `AesirArchitecturePlayerLoop.InvokeHooks` 的逐回调 try-catch。监听回调不应抛异常为框架约定（fail-fast），异常直接向上传播由 Unity 记日志
- **极简化：初始化失败不做回滚** — `AbstractContext<T>.Interface` 改为 `Initialize()` 成功后才写入静态字段 `_instance`，失败不缓存、根因异常每次抛出；`Initialize` 移除快照跟踪与逆序回滚 Dispose（初始化失败属启动期编程错误，半成品模块随实例交由 GC 回收）。Configure 及模块初始化中禁止重入 `Interface`（约定）
- **极简化：移除 PlayerLoop 周期性自愈轮询** — 删除 `MonoLifecycleProxy` 每 120 帧的注入检测；保留 `AesirArchitecturePlayerLoop.EnsureInjected()` 公开 API 与 `Register` 注册时的自动检测。第三方 SDK 覆盖 PlayerLoop 后需手动调用一次 `EnsureInjected()`
- **静态变量重置职责拆分** — 非泛型单例（`MonoLifecycleProxy`、`RemoveListenerOnSceneUnloadedTrigger`）改为类内直接声明 `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` 重置方法，不再经中心助手注册；`ResetStaticsAssistant` 收窄为仅服务泛型类——泛型类中的 RIOLM 会被 Unity 静默跳过（不执行也不报错，Unity 2022.3 实测），无法在自身内部声明重置入口，`AbstractContext<T>` 维持经助手注册 `_instance = null` 回调的原有方式
- **`IContext.GetModel` / `GetService` 未注册校验** — 未注册时抛出含类型名与修复提示的 `InvalidOperationException`，不再返回 null（消除 NRE 延迟爆发与报错点/根因分离）；`CapabilityExtensions.GetModel/GetService` 的 null 分支随之移除，仅保留 `Initialized` 校验
- **package.json description 修正** — "类型化事件总线"改为"轻量事件（MiniEvent）与响应式属性（ObservableValue）"，与实际能力对齐（事件总线已于 0.3.2 后移除）
- **Odin 程序集重命名** — `OdinIntegration` → `OdinInspector`（三包统一）：Runtime `Runestone.AesirArchitecture.OdinInspector`、Editor `Runestone.AesirArchitecture.Editor.OdinInspector`；同步目录、asmdef 引用与 `InternalsVisibleTo`
- **场景卸载分桶改 `Scene.handle`** — `RemoveListenerOnSceneUnloadedTrigger` 从按场景名分桶改为按 `Scene.handle`（int）分桶，消除同名场景误清；新增 `RemoveListenerExtensions` 的 `(Scene)` / `(MonoBehaviour)` 显式归桶重载
- **预放置实例风险 Warning InfoBox** — `AesirArchitecture`、`MonoLifecycleProxy`、`RemoveListenerOnSceneUnloadedTrigger` 三个组件通过 Odin AttributeProcessor 注入 Warning 级 InfoBox，提示预放置宿主的生命周期约束
- **PlayerLoop 注入自愈** — 新增 `AesirArchitecturePlayerLoop.EnsureInjected()` 公开 API（`ContainsSystem` 检测 + 仅补插缺失注入点）；`Register` 注册回调时自动检测
- **Context 初始化失败不缓存半成品** — `Interface` getter 初始化失败时不缓存单例，根因异常每次访问重复抛出

### Removed

- **`ModelReplaced` / `ServiceReplaced` 替换通知事件** — 运行时替换 Model/Service 属测试/调试用途，不应徒增框架事件面；测试环节自行处理订阅迁移。`RegisterModel`/`RegisterService` 覆盖注册仍会 Dispose 旧实例
- **`GenericLocator<T>.Global`** — 全局定位器单例属性，全仓库无调用方，按极简原则移除（连带 `Dispose` 中的全局引用清除逻辑与 `IGenericLocator<T>` remarks 中的相关描述）
- **`GenericLocator<T>.GetRegistry()`** — 泄漏底层字典引用的封装漏洞，全仓库无调用方，删除（同步删除 `IGenericLocator<T>` 接口声明）

### Added

- **`GenericLocatorTests` / `ObservableValueTests`** — EditMode 单测覆盖 Locator 注册/覆盖/键语义/注销/清空与 ObservableValue 值比较/静默设置/立即触发/强制刷新/引用相等；`MiniEventTests` 重写为原生事件语义（含 fail-fast 锁定测试）与句柄行为
- **`RuntimeInitializeOnLoadMethod 指南`** — `Docs/Unity-RuntimeInitializeOnLoadMethod-指南.md`，记录各时机语义、Domain Reload 静态重置实践与泛型类 RIOLM 陷阱
- **`极简分析与改进计划`** — `Docs/AesirArchitecture-极简分析与改进计划.md`，裁决表与实施记录

## [0.8.0] - 2026-08-06

### Fixed

- **MonoLifecycleProxy 排序 Bug 修复** — `InvokeEvent` 原先排序 `_sortedListeners` 后仍调用 `MiniEvent.Invoke()`（按注册顺序），导致 `order` 参数无效。改为直接遍历 `_sortedListeners` 按排序结果调用回调。移除 `MiniEvent` 死代码（`_events` 字典、`GetOrCreateEvent` 方法）

### Removed

- **移除 `BeforeFixedUpdate` 事件** — 该事件通过 PlayerLoop 每帧触发，但 `FixedUpdate` 并非每帧执行，语义误导且无实际使用。涉及删除 `MonoLifecycleEvent.BeforeFixedUpdate` 枚举值、`AesirArchitectureLifecyclePhase.BeforeFixedUpdate` 枚举值、PlayerLoop 注入逻辑、`ICustomBeforeFixedUpdate` 接口、`MonoLifecycleProxy` 中对应的注册/注销分支

### Changed

- **`AesirArchitectureLifeCyclePhase` → `AesirArchitectureLifecyclePhase`** — 统一拼写为 Lifecycle（一个单词）
- **`FindFirstObjectByType` → `FindAnyObjectByType`** — 后者不依赖 InstanceID 排序，在 Unity 6 中向前兼容
- **`ClearAllListeners` 不再注销 PlayerLoop** — PlayerLoop 注销移至 `OnDestroy`，避免测试间 PlayerLoop 注册丢失
- **新增 `MonoLifecycleProxyTests`** — PlayMode 测试覆盖订阅、排序、稳定排序、句柄取消订阅、监听者数量、全帧级事件顺序

## [0.7.0] - 2026-08-05

### Changed

- **单例模式重构：预放置优先** — 所有 MonoBehaviour 单例（`AesirArchitecture`、`MonoLifecycleProxy`、`RemoveListenerOnSceneUnloadedTrigger`）的 `Instance` getter 优先通过 `FindAnyObjectByType` 搜索已加载场景中预放置的实例，未找到时才运行时创建
- **条件式 DontDestroyOnLoad** — `AesirArchitecture` 新增 `static bool _createdByRuntime` 标志，仅运行时创建的实例调用 `DontDestroyOnLoad`，场景中预放置的实例保留在场景中随场景生命周期销毁
- **移除 Bootstrap 自动初始化** — 移除 `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)] Bootstrap()` 方法，因为它会在场景加载前创建 DDOL 实例，导致预放置实例 `Awake` 时发现 `_instance` 已存在而自毁

### Removed

- `AesirArchitecture.Bootstrap()` 方法及其 `[RuntimeInitializeOnLoadMethod]` 特性

## [0.6.0] - 2026-08-05

### Added

- **MonoLifecycleProxy 生命周期代理** — 全局单例组件，挂载在 [Aesir Architecture] GameObject 上，将 Unity 原生生命周期回调（Start / FixedUpdate / Update / LateUpdate / OnApplicationFocus / OnApplicationPause）和自定义 PlayerLoop 阶段（BeforeUpdate / AfterUpdate）统一为可订阅的 MiniEvent。支持 Order + InsertionIndex 稳定排序，物体销毁时自动从代理取消订阅
- **ICustomXXX 生命周期接口** — 8 个接口（ICustomStart / ICustomFixedUpdate / ICustomBeforeUpdate / ICustomUpdate / ICustomLateUpdate / ICustomAfterUpdate / ICustomOnApplicationFocus / ICustomOnApplicationPause），实现后通过 `RegisterAuto()` 自动注册到匹配事件
- **MonoLifecycleProxyExtensions 扩展方法** — MonoBehaviour / GameObject / object 的 `AddListener` / `RemoveListener` / `RegisterLifecycle` 扩展方法，支持纯 C# 类快捷注册
- **全包 XML 文档注释增强** — 69 个 .cs 文件补充完整详细的 XML 文档注释，覆盖 `<remarks>` / `<param>` / `<typeparam>` / `<returns>` / `<exception>` / `<seealso>` 标签

### Changed

- **MonoLifecycleEvent 移除低实用性事件** — 移除 Awake / OnEnable / OnDisable / OnDestroy（代理是 DontDestroyOnLoad 单例，这四个回调无法被外部有效订阅），新增 OnApplicationFocus / OnApplicationPause（焦点和暂停是高频需求）
- **Samples 目录版本对齐** — `Assets/Samples/Aesir Architecture/0.5.0/` → `0.6.0/`

## [0.5.0] - 2026-08-01

### Fixed

- **单例竞争修复**：`AesirArchitecture` 重复实例 `Destroy` 后提前 `return`，避免继续执行赋值和 `DontDestroyOnLoad`；`OnDestroy` 仅在 `_instance == this` 时清空，避免销毁非自身实例时误清
- **RemoveListenerTrigger**：移除 `[DisallowMultipleComponent]` 限制

## [0.4.2] - 2026-07-24

### Fixed

- **GetModel / GetService 初始化状态校验** — 在 `GetModel<T>()` 和 `GetService<T>()` 扩展方法中，null 检查之后新增 `Initialized` 状态检查。若目标已注册但尚未初始化，抛出 `InvalidOperationException` 并报告调用者类型和目标类型，提示注册顺序错误或循环依赖。此前获取已注册但未初始化的模块会静默返回半成品实例，可能导致难以排查的运行时错误

## [0.4.1] - 2026-07-24

### Changed

- **Samples 代码优化**：Counter-MVC 的 View（`SampleMvcCounterMainPanel`）和 Counter-MVP 的 Presenter（`SampleMvpCounterPresenter`）从缓存 Model 字段引用改为属性每次获取（`=> this.GetModel<T>()`），并添加注释说明此模式支持运行时动态替换 Model/Service，旧实例可被 GC 正常回收
- **Samples 版本文件夹**：`Assets/Samples/Aesir Architecture/0.3.2/` → `0.4.0/`，与 `package.json` 版本对齐

## [0.4.0] - 2026-07-24

### Added

- **Query 查询系统** — 新增 `IQuery<TResult>` / `AbstractQuery<TResult>` / `ICanExecuteQuery` 能力接口及 `ExecuteQuery` 扩展方法，实现 CQRS 读写分离。Controller 和 Presenter 同时具备 ExecuteCommand + ExecuteQuery 能力，Service 保持不参与 Command/Query 执行

## [0.3.2] - 2026-07-03

### Added

- **无参事件发布重载** — `MiniEventBus`、`IContext`、`AbstractContext<T>`、`CapabilityExtensions` 新增 `InvokeEvent<T>() where T : IEventArgs, new()` 重载，内部创建默认实例后转发给带参版本，简化无参事件的发布调用

## [0.3.1] - 2026-07-03

### Added

- **AesirViewController\<T\>** — View + Controller 双角色合一的 MonoBehaviour 基类，自动绑定对应 Context 类型的接口实例，简化 MVC 模式下的 View 搭建

### Changed

- **IController 移除 ICanAddListener** — Controller 定位为命令执行入口，不应被动监听事件。事件监听应由 Service 或 View 负责。能力矩阵同步更新

## [0.3.0] - 2026-07-03

### Changed

- **Engine 层彻底脱离 Component 层** — 移除 `ContextBoard` 和 `MiniEventBusBoard` 两个 MonoBehaviour 组件，移除 `AbstractContext` 中所有对 `ContextBoard.Instance.AddContext()` 的调用。Engine 层现在是真正的纯 C#，不依赖任何 Component 层类型
- **移除声明式依赖校验** — 移除 `IModel.GetDependencies()` / `IService.GetDependencies()` 和 `ContextDependencyAssistant` 类，改用 `GetModel<T>()` / `GetService<T>()` 扩展方法中的运行时错误日志。未注册时抛出含调用者类型和目标类型信息的 `InvalidOperationException`，兼容运行时替换 Model 的调试模式
- **提取 AbstractSubmodule 基类** — 将 `AbstractModel` 和 `AbstractService` 的公共生命周期逻辑（`_context`、`SetContext`、`Initialized`、`Initialize`、`Dispose`、`OnInitialize`、`OnDispose`）提取到 `AbstractSubmodule` 基类，消除代码重复
- **能力接口文件合并** — 将 13 个碎片化的 `ICan*` 接口和扩展方法文件合并为 `Capabilities.cs` 和 `CapabilityExtensions.cs`，逻辑零改动
- **MiniEventBus.EventDictionary 封装** — 从 `public` 改为 `private`，值类型从 `object` 改为 `Delegate`，移除 `Clear()` 中对 `Delegate` 的无效 `IDisposable` 转型
- **IService 角色定位明确** — 修正注释，明确定位为万能协调层（能读写 Model、调用其他 Service、监听和发布事件），确认不包含 `ICanExecuteCommand`
- **IView 描述修正** — 移除"只读"描述，明确 View 可通过事件向上通信、可读取 Model/Service、不可执行 Command 或修改 Model 状态
- **GenericResetStaticsAssistant → ResetStaticsAssistant** — 重命名，移除 "Generic" 前缀，类名更简洁
- **AbstractContext 不再注册到 GenericLocator\<IContext\>** — 移除 `Interface` 属性中的 `GenericLocator<IContext>.Global.Register/Unregister` 调用

### Added

- **运行时错误日志** — `GetModel<T>()` 和 `GetService<T>()` 扩展方法在目标未注册时抛出含调用者类型和目标类型信息的异常，格式示例：`[InventoryModel] 尝试获取 Model [ItemDefinitionModel]，但该 Model 未在 Context 中注册`
- **AbstractSubmodule** — 子模块统一基类，承载 Model 和 Service 的公共生命周期逻辑

### Fixed

- **RemoveListenerOnSceneUnloadedTrigger 域重置** — 静态字段 `_instance` 未注册到 `ResetStaticsAssistant`，Disable Domain Reload 下反复进出 Play Mode 会残留旧引用。已通过静态构造函数注册回调修复

### Removed

- **ContextBoard** — 上下文看板 MonoBehaviour 组件（Engine 层不再依赖 Component 层）
- **MiniEventBusBoard** — 事件总线看板 MonoBehaviour 组件
- **ContextBoardAttributeProcessor / MiniEventBusBoardAttributeProcessor** — 对应 Odin AttributeProcessor
- **ContextDependencyAssistant** — 依赖项校验辅助类
- **Locator / ILocator** — 非泛型定位器死代码（框架内零引用）
- **IMiniEventBus** — 事件总线接口（`MiniEventBus` 不再实现接口）
- **EventRegistrationInfo** — 事件注册信息数据类（随 `MiniEventBusBoard` 移除）
- **AbstractModelAttributeProcessor** — Model 属性处理器（随可视化组件移除）

## [0.2.1] - 2026-07-02

### Added

- **第三方说明文档** — 新增 `Third Party Notices.md`（UPM 标准），声明 QFramework MIT 许可证归属

## [0.2.0] - 2026-07-02

### Changed

- **事件 API 重命名** — `Subscribe` → `AddListener`、`Unsubscribe` → `RemoveListener`、`SubscribeAndInvoke` → `AddListenerAndInvoke`，对齐 Unity `UnityEvent.AddListener` / `RemoveListener` 命名习惯。同步重命名 `AutoUnsubscribeHandle` → `AutoRemoveListenerHandle`、`UnsubscribeExtensions` → `RemoveListenerExtensions`、`UnsubscribeHandleCollection` → `RemoveListenerHandleCollection`、`UnsubscribeInvoker` → `RemoveListenerTrigger`、`UnsubscribeOnDestroyInvoker` → `RemoveListenerOnDestroyTrigger`、`UnsubscribeOnDisableInvoker` → `RemoveListenerOnDisableTrigger`、`UnsubscribeOnSceneUnloadedInvoker` → `RemoveListenerOnSceneUnloadedTrigger`，扩展方法 `UnsubscribeWhenXxx` → `RemoveListenerWhenXxx`，`UnsubscribeAll` → `RemoveAllListeners`
- **能力接口简化** — `ICanSubscribeWithContext` → `ICanAddListener`（移除 "WithContext" 后缀）、`ICanInvokeWithContext` → `ICanInvokeEvent`，对应扩展方法 `CanSubscribeWithContextExtensions` → `CanAddListenerExtensions`、`CanInvokeWithContextExtensions` → `CanInvokeExtensions`
- **事件发布方法重命名** — `Invoke<T>()` → `InvokeEvent<T>()`，`IContext.Invoke` → `IContext.InvokeEvent`，语义更明确地表达"发布事件"而非泛型方法调用
- **System → Service 重命名** — 架构层 System 概念全面替换为 Service：`ISystem` → `IService`、`AbstractSystem` → `AbstractService`、`RegisterSystem` → `RegisterService`、`GetSystem` → `GetService`、`ICanGetSystem` → `ICanGetService`、`CanGetSystemExtensions` → `CanGetServiceExtensions`
- **GetContext() → Context 属性** — `IContextHolder` 的 `GetContext()` 方法改为 `Context` 只读属性，语义更清晰地表达"持有一个 Context"
- **Context 类重命名** — `Context<T>` → `AbstractContext<T>`（统一 `Abstract*` 命名规范）、`BaseContext` → `AbstractContext`（同上）、`MockContext` → `FakeContext`（按 Fowler 测试替身分类，它是 Fake 而非 Mock）
- **ContextResetAssistant → GenericResetStaticsAssistant** — 重命名为泛型静态变量重置助手，不再局限于 Context 类型
- **AesirArchitectureLifecycle → AesirArchitecturePlayerLoop** — 重命名 PlayerLoop 生命周期管理类，更准确描述其职责
- **AssemblyVisibleSettings → AssemblyInfo** — 重命名程序集可见性声明文件
- **View 基类拆分** — `AbstractView<T>` 拆分为 `AesirView<T>`（继承 `AesirMonoBehaviour`，自动支持 Odin Inspector 序列化）和 `MonoView<T>`（继承 `MonoBehaviour`，无 Odin 依赖）
- **SingletonMonoBehaviour → AesirMonoBehaviour + AesirArchitecture** — 移除泛型单例基类 `SingletonMonoBehaviour<T>`，新增 `AesirMonoBehaviour`（根据运行环境自动选择序列化方式的基类）和 `AesirArchitecture`（框架 MonoBehaviour 单例入口）
- **Container → GenericLocator\<T\>** — 移除 `Container<T>` 类，引入 `GenericLocator<T>` / `IGenericLocator<T>` / `ILocator` 泛型定位器体系，支持类型注册/查询/全局单例
- **目录三级重构** — `Runtime/` 下按 `Engine/`（纯 C# + 使用 UnityEngine API，不依赖 MonoBehaviour）、`Component/`（MonoBehaviour 组件）、`OdinIntergration/`（独立程序集，依赖 Odin Inspector）三级分离

### Added

- **GenericLocator\<T\>** — 泛型对象定位器，按类型注册、查询与获取对象实例，支持全局单例 `GenericLocator<T>.Global`，兼容 Domain Reload
- **ILocator / IGenericLocator\<T\>** — 定位器接口抽象，提供 `Register` / `Get` / `TryGet` / `IsRegistered` / `Unregister` / `GetByType` / `GetAll` 完整 API
- **ContextBoard** — 上下文看板 MonoBehaviour 组件，在 Inspector 中以字典形式展示每个 Context 的 Model 和 Service 列表
- **MiniEventBusBoard** — 事件总线看板 MonoBehaviour 组件，在 Inspector 中展示当前事件注册状态（事件类型、监听者列表）
- **ContextDependencyAssistant** — 依赖项校验辅助类，提供 Model 和 Service 的依赖类型检查与初始化状态检查，支持声明式依赖 `GetDependencies()`
- **AesirMonoBehaviour** — RAA 架构标准 MonoBehaviour 基类，根据运行环境（编辑器/运行时、是否安装 Odin Inspector）自动选择 `SerializedMonoBehaviour` 或 `MonoBehaviour` 作为基类
- **AesirScriptableObject** — RAA 架构标准 ScriptableObject 基类，同样根据运行环境自动选择序列化方式
- **IModel / IService 依赖声明** — `IModel` 和 `IService` 新增 `GetDependencies()` 方法，返回 `HashSet<Type>` 声明依赖的其他模块类型，注册时自动校验依赖是否已初始化
- **IView 能力扩展** — `IView` 新增 `ICanGetService` 和 `ICanInvokeEvent` 能力，View 层现在可以获取 Service 和发布事件
- **IContext 事件方法** — `IContext` 接口直接提供 `AddListener<T>` / `RemoveListener<T>` / `InvokeEvent<T>` 方法，事件操作不再依赖独立的事件总线属性
- **AbstractContext\<T\> 全局定位** — `AbstractContext<T>.Interface` 通过 `GenericLocator<IContext>.Global` 管理单例，注册到 `ContextBoard` 可视化
- **MiniEventBus.Global** — 全局事件总线单例，`AbstractContext` 的事件操作直接路由到 `MiniEventBus.Global`
- **MiniEventBusBoard** — 事件注册信息看板，Inspector 中可视化展示当前所有事件类型及其监听者
- **EventRegistrationInfo** — 事件注册信息数据类，记录事件类型和监听者列表
- **Odin Inspector AttributeProcessor 扩展** — 新增 `ContextBoardAttributeProcessor`、`MiniEventBusBoardAttributeProcessor`、`AbstractModelAttributeProcessor` 等 Odin 属性处理器
- **Editor 脚本符号管理** — 新增 `EnsureAesirArchitectureDefine` 和 `ScriptingSymbolUtility`，自动管理 `ODIN_INSPECTOR` 等编译符号
- **Documentation~/Books/** — 新增设计模式与 SOLID、ScriptableObject 模块化架构参考电子书
- **Documentation~/Manuals/mini-event-module-manual.md** — 新增 MiniEvent 模块完整使用手册
- **Documentation~/FAQ/why-fake-not-mock-context.md** — 新增 FakeContext 命名决策文档

### Removed

- **Query 系统** — 移除 `IQuery<TResult>` / `IAsyncQuery<TResult>` / `AbstractQuery<T>` / `AbstractAsyncQuery<T>` 及对应的能力接口 `ICanExecuteQuery` 和扩展方法，保持架构核心简单。读操作直接通过 `GetModel<T>()` 访问 Model 数据
- **FakeContext** — 移除测试用 Fake 上下文类（`MockContext` / `FakeContext`），测试改为直接实例化 `AbstractContext<T>` 子类或使用 `GenericLocator` 隔离
- **Container\<T\>** — 移除旧的模块容器类，由 `GenericLocator<T>` 替代
- **SingletonMonoBehaviour\<T\>** — 移除泛型单例基类，由 `AesirMonoBehaviour` + `AesirArchitecture` 替代
- **event-system-critique.md** — 移除旧的事件系统设计文档（内容已整合到 Manuals 和 FAQ 中）
- **旧目录结构** — 移除 `Runtime/Core/`、`Runtime/Command/`、`Runtime/Event/`、`Runtime/Observable/`、`Runtime/Utilities/` 等平铺目录

## [0.1.1] - 2026-06-24

### Changed

- **事件系统重构** — 移除 `ISubscribe` / `IUnsubscribe` 接口，简化事件系统设计，直接使用委托字段和扩展方法
- **MiniEventHub → MiniEventBus** — 重命名并优化实现，提升 API 一致性
- **ObservableValue 目录调整** — 从 `Runtime/Observer/` 移至 `Runtime/Observable/`，更符合 C# 命名习惯
- **ContextBase → AbstractContext** — 重命名统一上下文基类命名规范，对齐 `Abstract*` 前缀约定
- **AssemblyInfo → AssemblyVisibleSettings** — 重命名更准确描述文件用途

### Added

- **AbstractService** — 新增服务层标准基类，提供 `Initialize` / `Dispose` 生命周期管理
- **代码样式指南** — 新增 `Documentation~/Rules/raa-code-style.md`，完整定义 RAA 代码规范
- **事件系统设计文档** — 新增 `Documentation~/event-system-critique.md`，记录事件系统设计决策
- **IUnsubscribe 移除决策文档** — 新增 `Documentation~/why-remove-iunsubscribe.md`，说明移除接口的思考过程
- **Editor 端单元测试** — 将部分测试从 Runtime 移至 Editor，提升测试分类合理性
- **UnityEngineObjectCheckNullTests** — 新增 Unity 对象空检查测试

### Removed

- **ISubscribe / IUnsubscribe 接口** — 简化事件系统，直接使用委托字段
- **MiniEventHub** — 重构为 `MiniEventBus`

## [0.1.0] - 2026-06-21

### Added

- **Context 架构根** — `Context<T>` 泛型静态单例，支持 `RegisterModel` / `RegisterService` 模块注册，`FakeContext` 用于测试隔离
- **能力接口组合系统** — `ICanGetModel`、`ICanExecuteCommand`、`ICanSubscribeWithContext` 等能力标记接口，组合出 `IModel` / `IService` / `IView` / `IController` / `IPresenter`
- **PlayerLoop 原生生命周期** — `AesirArchitectureLifecycle` 将自定义子系统注入 Unity PlayerLoop，提供 `BeforeUpdate` / `AfterUpdate` 帧回调
- **CQRS 命令/查询分离** — `ICommand` 写操作 + `IQuery<TResult>` 读操作
- **ObservableValue 响应式属性** — `ObservableValue<T>` 可写 + `IReadOnlyObservableValue<out T>` 协变只读接口
- **MiniEventHub 类型事件总线** — 按事件类型注册/发布，支持 `IUnsubscribe` 自动注销句柄与 GameObject / 场景生命周期绑定
- **Domain Reload 安全** — 静态变量通过 `[RuntimeInitializeOnLoadMethod]` 显式重置
- **AesirArchitectureLog 统一日志** — 条件编译统一日志工具，禁止直接使用 `Debug.Log`
- **MonoBehaviour 适配层** — `AbstractView<T>` 作为纯 C# 核心与 Unity 之间的适配
- **Odin Inspector 集成**（可选） — `ObservableValueAttributeProcessor` 属性注入
- **Roslyn Analyzer** — AESIR001 规则：引用类型使用 `ObservableValue<T>` 时未实现 `IEquatable<T>` 编译警告
- **单元测试** — Context / Container / ObservableValue / Lifecycle 覆盖测试
- **示例** — UI Counter（MVC）、UI Counter（MVP）、ObservableValue（Odin Inspector）

### Changed

- `IService` 继承 `ICanInitialize`，获得 Initialize / Dispose 能力
- `AbstractContext` 服务生命周期管理：`RegisterService` 上下文已初始化时立即初始化服务，`GetService` 按需初始化，`Dispose` 逆序释放 Service（先 Service 后 Model）
- 移除 `ObservableValueDrawer`，Odin 集成仅保留 `ObservableValueAttributeProcessor` 属性注入
