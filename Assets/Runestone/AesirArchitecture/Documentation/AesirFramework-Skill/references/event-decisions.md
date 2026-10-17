# 事件机制决策表

> Aesir 中实际存在四种"通知"机制。对初学者，**每个场景有且只有一个默认答案**——不要在选择中瘫痪。

## 决策表

| 场景 | 唯一推荐 | 一句话理由 |
|------|---------|-----------|
| **Model 里的状态变了要通知外界**（血量、金币、分数） | `ObservableValue<T>` | 状态即数据，变化自动通知；订阅者经 `AddListenerAndInvoke` 拿当前值 |
| **一次性/瞬时通知，无状态**（"门开了""怪死了""关卡完成"） | `MiniEvent` / `MiniEvent<T>` | 零分配直调，`AddListener` 返回自动注销句柄，泄漏防护是 API 形态 |
| **View 把自己的输入暴露给 Presenter（MVP 内部）** | 原生 C# `event` | 编译期限制外部只能 `+=`/`-=`，不可整体替换/置空/外部触发 |
| **Inspector 里拖拽连线的 UI 交互**（Button.onClick） | `UnityEvent` | 编辑器可视化配置，美术/策划可调 |

## 反模式（不要这么用）

| 反模式 | 问题 | 正确做法 |
|--------|------|----------|
| 用 `Action` 属性（`public Action Xxx { get; set; }`）代替 `event` | 外部可 `=` 整体替换、`= null` 清空、甚至 `Invoke` 触发别人的事件链 | `event Action Xxx;` |
| 用 `MiniEvent` 承载会持续变化的状态 | 丢失"当前值"语义，新订阅者拿不到初始值 | `ObservableValue<T>` + `AddListenerAndInvoke` |
| 用 `UnityEvent` 做跨模块逻辑通知 | 配置藏在 Inspector 里，代码无法搜索/diff，AI 无法处理 | 代码内 `MiniEvent` 或 `ObservableValue` |
| Model 直接持有 `UnityEvent` 字段 | 数据层与编辑器配置耦合，纯 C# 可测试性被破坏 | `ObservableValue<T>` |

## 与档位的关系

| 档位 | 通知机制 |
|------|---------|
| Quick（默认档） | `ObservableValue` 直接暴露 + 直写直读（订阅刷新） |
| Standard / Strict（MVC） | `ObservableValue` 订阅刷新；Query 仅用于加工值（返回原始值直接订阅，无需 Query） |
| MVP 全档 | C# `event`（View→Presenter 输入）+ `ObservableValue`（Presenter 读 Model） |

## EventModule（RAM 事件模块）什么时候用

只用于**跨系统、发送方不该知道接收方**的场合（成就/播报/全局音效）——不要用它替代 Model 状态通知。用法见 [modules-api.md](modules-api.md) 的 Event 模块节。
