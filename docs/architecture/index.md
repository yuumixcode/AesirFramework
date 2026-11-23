# Aesir Architecture 概览

**Aesir Architecture(RAA)** 是以 **Unity 原生优先** 为核心理念的渐进式 MVC 架构框架。它不构建与引擎平行的自建体系,而是深度绑定 Unity 的 PlayerLoop、ScriptableObject、Editor API 等原生能力,在保持轻量的同时为中小型到中大型项目提供清晰的 MVC / MVP 分层。

框架以 **MVC 为主要模式**,`IController` 是推荐的快速开发入口;`IPresenter`(MVP)作为可选的严格分层模式 —— MVP 正是 Unity 官方最佳实践所教授的范式([官方教程](https://learn.unity.com/tutorial/build-a-modular-codebase-with-mvc-and-mvp-programming-patterns))。**暂不提供 MVVM**:其核心增量是数据绑定引擎,与框架轻量、Unity 原生优先的定位不符 —— 这是经调研评估后的架构决策,完整论述与重评触发条件见[模式选择决策](mode-decisions.md)。

## 架构角色与能力接口

框架采用**能力接口组合**模式 —— 每个角色通过组合细粒度能力接口(`ICanGetModel`、`ICanExecuteCommand` 等)按需暴露能力,而非继承大而全的基类接口:

| 角色 | 接口 | 能力 | 职责 |
|------|------|------|------|
| **Model** | `IModel` → `AbstractModel` | GetModel | 数据层;持有 `ObservableValue<T>`,修改必经写方法 |
| **Service** | `IService` → `AbstractService` | GetModel, GetService | 跨模块协调;可直写 Model,不能执行 Command/Query |
| **View** | `IView` | GetModel, GetService(只读) | 表现层;自订阅 Model 通知刷新 |
| **Controller** | `IController` | GetModel, GetService, **ExecuteCommand**, **ExecuteQuery** | MVC 模式入口(推荐) |
| **Presenter** | `IPresenter` | 全部 Controller + IDisposable | MVP 模式(可选,Unity 官方最佳实践范式);中介 Model ↔ View,View 被动 |
| **Command** | `ICommand` → `AbstractCommand` | `Execute()` | 写操作(同步、无返回值) |
| **Query** | `IQuery<TResult>` | `Execute() → TResult` | 读操作(无副作用),CQRS 风格 |

`AbstractContext<T>`(CRTP 泛型静态单例)是架构根:子类在 `Configure()` 中注册 Model / Service,`Instance` 首次访问触发初始化(按注册顺序初始化 Model → Service),未注册类型抛含修复提示的异常而非返回 null。

## 三档渐进路径(核心设计)

RAA 最鲜明的特征是**按档位渐进** —— 从最少概念跑通闭环,到读写全解耦的严格分层,每档只有一个核心增量。MVC 与 MVP 各三档逐课同构对照:

| 档位 | Model 暴露面 | MVC(View 自订阅) | MVP(View 被动、Presenter 推送) |
|------|-------------|--------------------|------------------------------|
| **第一课 · 快捷档** | 具体类注册,可写 `ObservableValue` 直改 | `MonoViewController<T>` 直改值 | Presenter 直改值并推送 |
| **第二课 · 标准档** | 只读暴露 + 写方法 | Controller 直调写方法 | Presenter 直调写方法 |
| **第三课 · 严格档** | 接口注册 + 只读暴露 + 写方法 | Command 写 + Query 加工读 | Command 写 + Query 读 |

快捷档直改合法、适合原型;**标准档封装修改入口(推荐起步)**;严格档读写全解耦、扩展性最好。View / Controller / Presenter 在严格档按**业务窄接口**存储(类型层面拿不到 `ExecuteCommand` 等框架能力),读写分离由类型系统闭环。

## 设计哲学

1. **Unity 原生优先** — 用引擎能力(PlayerLoop / ScriptableObject / Editor API),不自建平行体系
2. **极简边界** — 不做事件总线、Context 多实例、Command 池化 / async / Undo;低概率问题用文档约定与编辑期提示杜绝,不加运行时防御兜底
3. **样式与逻辑分离** — Inspector 呈现经 Odin AttributeProcessor 动态注入,运行时程序集零样式特性
4. **Domain Reload 安全(铁律)** — 静态变量全部显式重置,反复进出 Play Mode 无残留

## 设计边界

框架明确**不做**的事:事件总线 / EventChannel(跨模块通信用互相 GetModel + ObservableValue 订阅或 MiniEvent)、Context 多实例(多存档在业务层建模)、Command/Query 池化 / async / 队列 / Undo、线程安全(仅保证主线程使用)、MVVM(已评估暂不采纳,见[模式选择决策](mode-decisions.md))。完整边界与编写约定见包内 README 与[常见陷阱](../faq.md)。

## 继续阅读

- [快速开始](getting-started.md) — 安装与第一个 MVC 示例
- [特性一览](features.md) — ObservableValue / MiniEvent / PlayerLoop 等核心机制
- [内置功能模块](modules.md) — Event / Observable / CustomLifecycle / Locator / Utilities 五大模块详解
- [架构概念](concepts.md) — 能力接口组合 / Context 生命周期 / Domain Reload 安全
- [模式选择决策](mode-decisions.md) — MVC 首推 / MVP 官方范式 / 为什么暂不提供 MVVM
- [响应式与事件](observable.md) — 通知体系与事件机制决策表
- [可观察集合](observable-collections.md) — 四种高频集合与单轨变更通知
- [示例总览](../product/samples.md) — 计数器六档对照 + 工具类用法(11 个可导入示例;PlaneWar 为仓库级完整游戏案例,不随包分发)
- [包内更新器](updater.md) — unitypackage 安装的一键更新
- [兼容性](../start/compatibility.md) — Unity 版本 / Odin 可选集成