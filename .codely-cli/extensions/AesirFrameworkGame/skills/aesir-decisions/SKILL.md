---
name: aesir-decisions
description: Aesir 的两张技术决策表——用 MVC 还是 MVP、以及四种通知机制该选哪个。选架构模式、用户提出 MVVM、纠结"状态变化用什么通知"、"一次性事件用什么"、"要不要上 EventModule"时加载。含模式选择速查（MVC 首推 / MVP 可选 / 暂不提供 MVVM 及重评触发条件）、通知机制决策表（ObservableValue / MiniEvent / C# event / UnityEvent）、四条反模式与档位对应关系。触发场景：用户问"用 MVC 还是 MVP"、"能不能用 MVVM"、"为什么不用 MVVM"、"状态变了怎么通知 UI"、"用事件还是 ObservableValue"、"什么时候用 EventModule"。
---

# Aesir 决策表（模式 + 通知机制）

两张表覆盖绝大多数"我该用哪个"的犹豫。本文是模式选择的唯一权威口径。

## 一、模式选择

| 场景 | 选择 | 理由 |
|------|------|------|
| 默认 / 快速开发 / jam 开工 | **MVC Quick** | 最少概念跑通数据驱动 UI 闭环，框架默认档 |
| Model 需要封装修改入口 | MVC Standard | 只读暴露 + 写方法，升档是加法 |
| 读写全解耦 / 运行期替换实现 | MVC Strict | 接口注册 + Command/Query |
| View 需要完全被动、中介可独立测试 | **MVP**（三档同构） | Unity 官方最佳实践所教授的严格分层范式 |
| 用户要求"用 MVVM 写" | 说明本决策，引导到 MVC（默认）或 MVP | 见下节；不擅自引入 ViewModel 层或绑定引擎 |

六档与模板见 `aesir-tiers`。

### 为什么暂不提供 MVVM（2026-11-16 裁决）

1. **定位契合**：Aesir 是 game 快速使用框架，MVC-first 与官方教学口径一致；MVP 作为可选升档保留。
2. **MVVM 的核心增量是数据绑定引擎**（View ↔ ViewModel 自动同步）。Aesir 的基调是"Unity 原生优先、不构建与引擎平行的自建体系"——`ObservableValue<T>` 已提供状态通知，MVC 的 View 自订阅 / MVP 的 Presenter 推送已覆盖表现层刷新。自建绑定引擎的收益不抵概念与维护成本。
3. **并非否定 MVVM**：业务侧仍可用 `ObservableValue<T>` 自行按 MVVM 风格组织代码，只是框架包不提供一等公民支持（无 `IViewModel` / 无绑定引擎 / 无官方模板）。
4. **重评触发条件**（满足其一）：UI Toolkit 运行时绑定成为主力 UI 方案 / 大型数据驱动 UI 消除订阅胶水成为普遍诉求 / 社区出现明确且广泛的 MVVM 需求。
5. **AI 行为**：用户要求 MVVM → 说明决策 + 引导 MVC/MVP，不擅自加层。

完整调研（Unity 官方教程与手册证据、社区实践现状）见 [references/mode-decisions.md](references/mode-decisions.md)。

## 二、通知机制

**每个场景有且只有一个默认答案**，不要在选择中瘫痪。

| 场景 | 唯一推荐 | 一句话理由 |
|------|---------|-----------|
| **Model 状态变了要通知外界**（血量/金币/分数） | `ObservableValue<T>` | 状态即数据，变化自动通知；用 `AddListenerAndInvoke` 拿当前值 |
| **一次性/瞬时通知，无状态**（"门开了""怪死了""关卡完成"） | `MiniEvent` / `MiniEvent<T>` | 零分配直调，句柄即泄漏防护 |
| **View 把自己的输入暴露给 Presenter**（MVP 内部） | 原生 C# `event` | 编译期限制外部只能 `+=`/`-=` |
| **Inspector 里拖拽连线的 UI 交互**（Button.onClick） | `UnityEvent` | 编辑器可视化配置，美术/策划可调 |
| **跨系统广播**（成就/全局播报/全局音效） | RAM `EventModule` | 发送方不该知道接收方——见 `aesir-events` 技能 |

### 反模式

| 反模式 | 问题 | 正确做法 |
|--------|------|----------|
| `public Action Xxx { get; set; }` 代替 `event` | 外部可整体替换 / 置空 / 触发别人的事件链 | `event Action Xxx;` |
| 用 `MiniEvent` 承载持续变化的状态 | 丢失"当前值"，新订阅者拿不到初始值 | `ObservableValue<T>` + `AddListenerAndInvoke` |
| 用 `UnityEvent` 做跨模块逻辑通知 | 配置藏在 Inspector，代码不可搜索/diff | 代码内 `MiniEvent` / `ObservableValue` |
| Model 直接持有 `UnityEvent` 字段 | 数据层与编辑器配置耦合，纯 C# 可测性被破坏 | `ObservableValue<T>` |

### 与档位的关系

| 档位 | 通知机制 |
|------|---------|
| Quick | `ObservableValue` 直接暴露 + 直写直读（订阅刷新） |
| Standard / Strict（MVC） | `ObservableValue` 订阅刷新；Query 仅用于加工值 |
| MVP 全档 | C# `event`（View→Presenter 输入）+ `ObservableValue`（Presenter 读 Model） |

决策表原文见 [references/event-decisions.md](references/event-decisions.md)。
