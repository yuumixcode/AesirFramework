---
name: aesir-tiers
description: Aesir Architecture（RAA）的六档分级与模板。用 Aesir 写业务代码需要选档位、升档、写 Command/Query、或需要 MVC 与 MVP 的可运行模板时加载——Quick/Standard/Strict 三档的 Model 注册方式、暴露面、写入路径与文件数对照，升档触发条件，View 基类（MonoViewController / MonoView）选择，以及六份逐行讲解的模板（MVC 三档 + MVP 三档）。触发场景：用户问"用 Quick 还是 Standard/Strict"、"怎么升档"、"什么时候要 Command/Query"、"用 MVP 怎么写"、"给我完整模板"，或要求按严格分层重构。
---

# Aesir 六档分级（默认 Quick，按需升档）

RAA 的"渐进式"落在这里：同一个业务可以停在三个档位中的任意一个，**升档是加法，不是重写**。

> 入口与 Quick 三件套最小闭环见 `aesir-framework/SKILL.md`；选 MVC 还是 MVP、用哪种通知机制见 `aesir-decisions/SKILL.md`。

## 档位速查

| 档位 | Model 注册 | Model 暴露面 | 写入路径 | 文件数 |
|------|-----------|-------------|---------|--------|
| **Quick（默认）** | 具体类 | 可写 `ObservableValue` 直接暴露 | 直改 `.Value` | ~3-4 |
| Standard | 具体类 | 只读接口 `IReadOnlyObservableValue<T>` + 写方法 | 调 Model 写方法 | ~4-5 |
| Strict | 接口 | 只读接口 + 写方法 | `ExecuteCommand` | ~10-11 |

**升档触发条件**（满足其一就该升，别提前）：
- 写入逻辑 > 30 行 → Standard（把散落的 `model.X.Value = ...` 收成写方法）
- 跨多个 View 执行同一操作 → Standard
- 运行期需要替换实现 / 需要单测 Model → Strict（接口注册 + Command/Query）

## MVC 与 MVP 的差别

| | MVC（首推） | MVP（可选，Unity 官方最佳实践范式） |
|---|---|---|
| 入口 | `IController`（View 兼 Controller，Quick 档直接写） | `IPresenter`（View 完全被动，Presenter 推送刷新） |
| View | `MonoViewController<T>`（Quick）/ `MonoView<T>`（Standard+） | 纯 `MonoBehaviour` + 手写 `IXxxView` 契约 |
| 数据流 | View 直接读 Model、订阅刷新 | View 不自取数据，Presenter 读 Model 后 push |

MVVM **暂不提供**（架构决策，非遗漏）——理由与重评触发条件见 `aesir-decisions`。

## View 基类对照

| 基类 | 能力 | 用途 |
|------|------|------|
| `MonoView<T>` | GetModel / GetService（只读） | MVC Standard / Strict 的 View |
| `MonoViewController<T>` | + ExecuteCommand / ExecuteQuery | MVC Quick 的 View 兼 Controller |
| `AesirView<T>` / `AesirViewController<T>` | 同上 + Odin Inspector 增强 | 装了 Odin 时的同名替代 |

## 模板（按需加载，不要一次全读）

| 文件 | 内容 | 何时加载 |
|------|------|---------|
| [mvc-quick.md](references/mvc-quick.md) | 默认档完整模板（Context + Model + Panel 三件套，逐行讲解） | 写第一个系统时 |
| [mvc-standard.md](references/mvc-standard.md) | 只读暴露 + 写方法 + View 分离 | 升到 Standard 时 |
| [mvc-strict.md](references/mvc-strict.md) | 接口注册 + Command 写 + Query 读 + 窄接口 | 升到 Strict 时 |
| [mvp-quick.md](references/mvp-quick.md) | Presenter 直写直推、View 纯 MonoBehaviour | 选 MVP 且快速开发时 |
| [mvp-standard.md](references/mvp-standard.md) | View 契约 `IXxxView` + 只读 Model | 选 MVP 且要可替换 View 时 |
| [mvp-strict.md](references/mvp-strict.md) | 接口注册 + Command/Query + 窄接口持 Presenter | 选 MVP 且严格分层时 |

## 升档怎么做（口诀）

1. **收窄暴露**：`ObservableValue<T>` 字段转 private，对外给 `IReadOnlyObservableValue<T>` 属性。
2. **收写入**：把 `model.X.Value = v` 收成 `model.SetX(v)`；Model 内部自己算派生值。
3. **再叠接口**（可选）：`ICounterModel` 注册 + 获取两边一致（**Register 与 Get 必须用相同类型参数**）。
4. **再叠 Command/Query**（可选）：写走 `ExecuteCommand(new XxxCommand(...))`，读走 `ExecuteQuery(new GetXxxQuery())`——**只有加工/聚合值才需要 Query**，直接读原始值用订阅就行。
