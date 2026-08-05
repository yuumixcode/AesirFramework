# Changelog

本项目的所有重要变更均会记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/)。

## [0.7.0] - 2026-08-05

### Changed

- **单例模式重构：预放置优先** — 所有 MonoBehaviour 单例（`AesirArchitecture`、`MonoLifecycleProxy`、`RemoveListenerOnSceneUnloadedTrigger`）的 `Instance` getter 优先通过 `FindFirstObjectByType` 搜索已加载场景中预放置的实例，未找到时才运行时创建
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

## [Unreleased]

### 规划中

- ScriptableObject 可视化配置层
- SO EventChannel 事件通道
- Editor 工具链（SO Inspector / MVP 脚手架 / 模块可视化）
- 运行时集合（RuntimeSet）

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
- **AesirArchitectureLifeCycle → AesirArchitecturePlayerLoop** — 重命名 PlayerLoop 生命周期管理类，更准确描述其职责
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
- **AesirMonoBehaviour** — RAA 框架标准 MonoBehaviour 基类，根据运行环境（编辑器/运行时、是否安装 Odin Inspector）自动选择 `SerializedMonoBehaviour` 或 `MonoBehaviour` 作为基类
- **AesirScriptableObject** — RAA 框架标准 ScriptableObject 基类，同样根据运行环境自动选择序列化方式
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
- **PlayerLoop 原生生命周期** — `AesirArchitectureLifeCycle` 将自定义子系统注入 Unity PlayerLoop，提供 `BeforeUpdate` / `AfterUpdate` 帧回调
- **CQRS 命令/查询分离** — `ICommand` 写操作 + `IQuery<TResult>` 读操作
- **ObservableValue 响应式属性** — `ObservableValue<T>` 可写 + `IReadOnlyObservableValue<out T>` 协变只读接口
- **MiniEventHub 类型事件总线** — 按事件类型注册/发布，支持 `IUnsubscribe` 自动注销句柄与 GameObject / 场景生命周期绑定
- **Domain Reload 安全** — 静态变量通过 `[RuntimeInitializeOnLoadMethod]` 显式重置
- **AesirArchitectureLog 统一日志** — 条件编译统一日志工具，禁止直接使用 `Debug.Log`
- **MonoBehaviour 适配层** — `AbstractView<T>` 作为纯 C# 核心与 Unity 之间的适配
- **Odin Inspector 集成**（可选） — `ObservableValueAttributeProcessor` 属性注入
- **Roslyn Analyzer** — AESIR001 规则：引用类型使用 `ObservableValue<T>` 时未实现 `IEquatable<T>` 编译警告
- **单元测试** — Context / Container / ObservableValue / LifeCycle 覆盖测试
- **示例** — UI Counter（MVC）、UI Counter（MVP）、ObservableValue（Odin Inspector）

### Changed

- `IService` 继承 `ICanInitialize`，获得 Initialize / Dispose 能力
- `AbstractContext` 服务生命周期管理：`RegisterService` 上下文已初始化时立即初始化服务，`GetService` 按需初始化，`Dispose` 逆序释放 Service（先 Service 后 Model）
- 移除 `ObservableValueDrawer`，Odin 集成仅保留 `ObservableValueAttributeProcessor` 属性注入
