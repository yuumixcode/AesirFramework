---
description: "AesirFramework 能力全景:两包分工、核心机制一览、可选集成与明确的设计边界。"
---

# 特性总览

AesirFramework 不是"Unity 全家桶",而是**一套可分层引入的工程骨架**:RAA 负责架构与运行时基础,RAM 在你需要 UI / 事件 / 音频 / 场景能力时补上来。

## 两包分工

| | **Aesir Architecture**(RAA) | **Aesir Modules**(RAM) |
| --- | --- | --- |
| 定位 | 渐进式 MVC / MVP 架构核心 | 建立在 RAA 之上的功能模块集合 |
| 依赖 | 无 | RAA |
| 组成 | Context、Model/Service、Command/Query、View/Controller/Presenter、响应式属性、轻量事件、PlayerLoop 生命周期、计时器 | UI 框架、事件模块、音频模块、场景模块、脚本文档生成 |
| 渐进档位 | 快捷 → 标准 → 严格(MVC/MVP 各三档) | 模块可单独删除,模块间零依赖 |
| 典型使用者 | 项目核心逻辑、数据层、表现层分层 | 需要现成 UI 栈 / 场景管理 / 音频门面的项目 |

## 核心机制

<div class="aesir-value-grid" markdown>

<div class="aesir-value-card" markdown>

**Context 架构根**

`AbstractContext<T>` 是纯 C# 静态单例,在 `Configure()` 里注册 Model / Service,首次访问 `Instance` 触发按序初始化。不用 GameObject,不用场景物体,也不做多实例。

</div>

<div class="aesir-value-card" markdown>

**能力接口组合**

View / Controller / Presenter 按需组合 `ICanGetModel`、`ICanExecuteCommand` 等细粒度能力,而不是继承大而全的基类。`ExecuteCommand` 是硬墙,`GetModel` 是建议性约束。

</div>

<div class="aesir-value-card" markdown>

**ObservableValue**

Model 持有可写实例,View 通过 `IReadOnlyObservableValue<T>` 订阅。`AddListenerAndInvoke` 一次写完"立即刷一次 + 后续订阅",返回句柄可绑定到 GameObject 生命周期。

</div>

<div class="aesir-value-card" markdown>

**MiniEvent**

零分配的轻量事件:直接多播调用,无装箱无字典。返回 `AutoRemoveListenerHandle` 自动退订;异常语义与原生 C# 事件一致(fail-fast)。

</div>

<div class="aesir-value-card" markdown>

**PlayerLoop 帧钩子**

`AesirPlayerLoop.Register(phase, callback)` 把纯 C# 对象挂进 Unity 的 PlayerLoop,不需要 MonoBehaviour 也能收到每帧回调。第三方 SDK 改过 PlayerLoop 后调用 `EnsureInjected()` 自愈。

</div>

<div class="aesir-value-card" markdown>

**计时器**

`AesirTimer.Countdown(5f, OnReady)` 让纯 C# 层直接延时、做冷却、跑完成度进度条:每个计时器是独立实例,到期自动出队,`Progress` 可直接喂 UI。另有 `AesirTimer.NextFrame(...)` 与不受 `Time.timeScale` 影响的 `unscaled` 选项。

</div>

<div class="aesir-value-card" markdown>

**Domain Reload 安全**

静态状态全部显式重置:非泛型单例类内自重置,泛型类经 `ResetStaticsAssistant` 补位(引擎对泛型类的 RIOLM 会静默跳过)。反复进出 Play Mode 无残留。

</div>

</div>

## 功能矩阵

| 领域 | RAA | RAM | 详见 |
| --- | --- | --- | --- |
| 分层架构 | Context、Model / Service、Command / Query、View / Controller / Presenter、六档渐进 | — | [架构概念](../architecture/concepts.md) |
| 数据驱动 UI | `ObservableValue`、可观察集合(List / Dictionary / HashSet) | — | [响应式与事件](../architecture/observable.md) |
| 事件 | `MiniEvent` / `MiniEvent<T>` | `EventModule`:Script 订阅、4 档优先级、订阅者过滤器、取消传播、SO 资产化、公开诊断 API | [事件模块](../modules/events.md) |
| UI | — | `UIRoot` 四层 Canvas、`UIModule` 面板生命周期、Canvas 根窗口、Binder 组件绑定 | [UI 框架](../modules/ui.md) |
| 场景 | — | `SceneModule` 加载 / 叠加 / 卸载、`SceneAssetWrapper`、协程与 UniTask 双驱动 | [场景模块](../modules/scene.md) |
| 音频 | — | SFX 独占音源轮询、BGM 淡入淡出、三通道音量持久化 | [音频模块](../modules/audio.md) |
| 文档 | — | Script Doc Generator(反射生成 API 文档)+ Summary 工具(需 Odin) | [Modules 特性](../modules/features.md) |
| 编辑器工具 | 更新器、Getting Started、锚点定位、构建剔除 | 场景设置窗口、依赖补全、Binder 工具链 | [包内更新器](../architecture/updater.md) |

## 设计边界

极简是 RAA 的第一排序标准,以下能力**明确不做**,避免你去找不存在的 API:

- **事件总线 / EventChannel** —— 跨模块通信用互相 `GetModel` + `ObservableValue` 订阅,或直接引用 `MiniEvent`
- **Context 多实例** —— CRTP 泛型单例;多存档 / 多房间在业务层建模
- **Command / Query 的池化、async、Undo/Redo** —— 保持同步、无缓存
- **时间调度的完整形态** —— 只保留 `AesirTimer`(一次性延时 / 冷却进度 / 下一帧);不做正计时秒表、每帧重复回调的 `Repeat`、对象池与暂停单个计时器
- **View 生命周期脚手架** —— 面板生命周期交由 RAM 的 `UIModule`
- **线程安全** —— 只保证主线程使用
- **对象池 / DI 容器 / AudioMixer / 3D 音频** —— 用上游或引擎原生能力

RAM 的边界更宽(功能模块的"面"是价值),但同样不做导航栈、per-panel Canvas、每音效 Stop 等低频能力。

## 继续阅读

- [示例总览](samples.md) —— 两包共 16 个可导入示例
- [版本与下载](releases.md) —— 版本策略与三种获取方式
- [路线图](roadmap.md) —— 已知缺口与后续计划
