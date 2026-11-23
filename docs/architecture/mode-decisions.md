# 模式选择决策(MVC / MVP,以及为什么没有 MVVM)

> 2026-11-16 定稿。框架提供的模式:**MVC(首推)** 与 **MVP(可选)**。
> **暂不提供 MVVM** —— 这是经调研评估后的架构决策,不是遗漏。本页是模式选择的唯一权威口径。

## 模式选择速查

| 场景 | 选择 | 理由 |
|------|------|------|
| 默认 / 快速开发 / jam 开工 | **MVC Quick** | 最少概念跑通数据驱动 UI 闭环,框架默认档 |
| Model 需要封装修改入口 | MVC Standard | 只读暴露 + 写方法,升档是加法 |
| 读写全解耦 / 运行期替换实现 | MVC Strict | 接口注册 + Command/Query |
| View 需要完全被动、中介可独立测试 | **MVP**(三档同构) | Unity 官方最佳实践所教授的严格分层范式 |

MVC 与 MVP 各三档的逐课对照见[快速开始](getting-started.md)与[示例总览](../product/samples.md)。

## Unity 官方立场(调研证据,2026-11-16)

1. **官方最佳实践只教 MVC/MVP,未正式推荐 MVVM**:
    - 官方教程 [Build a modular codebase with MVC and MVP programming patterns](https://learn.unity.com/tutorial/build-a-modular-codebase-with-mvc-and-mvp-programming-patterns)(Unity Technologies);
    - 官方视频系列 [Game programming patterns](https://unity.com/cn/resources/game-programming-patterns-video-series)(MVP 篇配 QuizU 示例);
    - [Unity 6 手册 Gameplay programming](https://docs.unity.com/en-us/engine/6000.6/manual/scripting) 亦只链接 MVP 视频。
2. **官方立场是"模式是工具不是教条"**:MVP 视频原话 —— "MVP 只是分离关注点的几种模式之一,还有 MVC 等变体……并非每个项目都需要 MVP 或其变体的正式结构……模式是你的工具,它们不是你的老板"。
3. **MVVM 在 Unity 生态是社区实践**(uMVVM、UniVue、CommunityToolkit.Mvvm 等);官方 [UI Toolkit](https://docs.unity3d.com/Manual/UIElements.html) 的[运行时数据绑定](https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-runtime-binding-define-data-source.html)在机制上是 MVVM 取向,但官方不以 MVVM 命名推广。

## 为什么暂不提供 MVVM

1. **定位契合**:Aesir 是 game 快速使用框架,MVC-first 与官方教学口径一致;MVP 是官方最佳实践演示的严格分层范式,作为可选升档保留。
2. **MVVM 的核心增量是数据绑定引擎**(View ↔ ViewModel 自动同步)。Aesir 的设计基调是"Unity 原生优先、不构建与引擎平行的自建体系":`ObservableValue<T>` 已提供状态通知,MVC 的 View 自订阅 / MVP 的 Presenter 推送已覆盖表现层刷新需求。自建绑定引擎违背轻量定位,收益不抵概念与维护成本(三档示例、三档模板、测试、双份镜像、九档同构审查)。
3. **并非否定 MVVM**:表现状态独立、可测试等诉求在现有档位内可达成。业务侧仍可用 `ObservableValue<T>` 自行按 MVVM 风格组织代码(框架不禁止),只是框架包不提供一等公民支持 —— 无 `IViewModel`、无绑定引擎、无官方模板。
4. **重评触发条件**(满足其一时重新评估):
    - 项目转向 UI Toolkit 运行时绑定作为主力 UI 方案;
    - 大型数据驱动 UI 需要消除订阅胶水代码成为普遍诉求;
    - 社区出现明确且广泛的 MVVM 需求。

## 一句话总结

> 快速开发首推 MVC;需要官方范式的严格分层选 MVP;MVVM 的绑定引擎与框架轻量定位不符,暂不提供,触发条件满足时重评。

## 继续阅读

- [架构概览](index.md) — 角色能力接口与三档渐进路径
- [架构概念](concepts.md) — 能力接口组合与基类映射
- [示例总览](../product/samples.md) — 计数器六档(MVC / MVP 各三课)