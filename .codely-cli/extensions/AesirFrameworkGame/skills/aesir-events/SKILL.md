---
name: aesir-events
description: Aesir Modules 事件模块（EventModule）编码指南。模块间解耦、发布-订阅时使用——AddListener<T> + 句柄退订、四档优先级、订阅者过滤器、取消传播、SO 事件资产化。触发词：EventModule、AesirEventArgs、发布订阅、移除订阅、SO 事件、在模块之间发消息、UI 响应游戏事件、让敌人只收到范围内的爆炸。
---

# Aesir 事件模块（EventModule）

Script 订阅的发布-订阅系统：`AddListener<T>(subscriber, callback)` 注册委托、返回 `AutoRemoveListenerHandle` 精确退订；分发按优先级稳定排序执行。**特性订阅（`[AesirListener]`）已移除**，只保留这一条路径（理由见文末）。

> 前置：本包依赖 Aesir Architecture（RAA）。**同一组件内的状态通知不要用本模块**——那是 `ObservableValue<T>` 的活；两者分工见 `aesir-framework` Skill 的"通知机制"小节。
>
> **缺包先过依赖门禁**：工程里没有 `cn.runestone.aesir.modules`（或连带缺 RAA）时，不要改用 `static event` 总线 + 手工注销顶替——先按 `aesir-router` 的「依赖门禁」探测 → 读记忆 → 询问安装（Git URL，CNB 优先、GitHub 兜底）。

## 默认路径：定义参数 → 订阅 → 发布

```csharp
// 1. 事件参数：继承 AesirEventArgs，[Serializable]，命名 XxxEventArgs
[Serializable]
public sealed class PlayerScoredEventArgs : AesirEventArgs
{
    public int points;
    public string playerName;
}

// 2. Script 订阅（唯一订阅形态；返回句柄，只移除这一条）
public sealed class ScoreView : MonoBehaviour
{
    AutoRemoveListenerHandle _handle;

    void OnEnable()  => _handle = EventModule.AddListener<PlayerScoredEventArgs>(this, e => UpdateScore(e.points));
    void OnDisable() => _handle.Dispose();   // 重复 Dispose 安全
}

// 3. 发布（发布者默认是事件参数自己 / 可指定 Sender）
new PlayerScoredEventArgs { points = 10 }.Invoke();
new PlayerScoredEventArgs { points = 10 }.Invoke(this);
EventModule.InvokeEvent(this, new PlayerScoredEventArgs { points = 10 });
```

一个方法监听多种事件 = 多条 `AddListener<T>`，各自持有句柄：

```csharp
AutoRemoveListenerHandle _scoreHandle;
AutoRemoveListenerHandle _levelHandle;
void OnEnable()
{
    _scoreHandle = EventModule.AddListener<PlayerScoredEventArgs>(this, e => UpdateScore(e.points));
    _levelHandle = EventModule.AddListener<LevelLoadedEventArgs>(this, e => OnLevel(e));
}
void OnDisable() { _scoreHandle.Dispose(); _levelHandle.Dispose(); }
```

**无句柄退订**（退订方拿不到句柄，如诊断工具/外部解绑）：

```csharp
EventModule.RemoveListener<PlayerScoredEventArgs>(subscriber);  // 精确退该对象某事件类型（O(1) 查表）
EventModule.RemoveListener(subscriber);                         // 批量退该对象全部订阅（O(n) 冷路径）
```

## 优先级（4 档）

`SubscriberPriority`：`First` → `High` → `Medium` → `Last`。**Script 默认 Medium**，其余档位需显式指定；同档按注册顺序（注册期有序插入，分发零排序）。First/Last 不受取消传播影响。

```csharp
EventModule.AddListener<BootEventArgs>(this, e => { }, SubscriberPriority.First);
EventModule.AddListener<TickEventArgs>(this, e => { }, SubscriberPriority.Last);
```

## 过滤器：只让特定范围的订阅者收到

发布时链式声明，分发前逐订阅者检查，**全部通过才投递**（AND 语义）：

```csharp
new ExplosionEventArgs().WithFilter(new WithTag("Enemy")).Invoke(this);
new ExplosionEventArgs()
    .WithFilter(new InsideCollider2D())      // 发布者 Collider2D 范围内
    .WithFilter(new WithTag("Enemy"))
    .Invoke(this);
new LevelLoadedEventArgs().WithFilter(new SameSceneAsEmitter()).Invoke(this);
new ChildActivatedEventArgs().WithFilter(new OnlySelf()).Invoke(this);
new TickEventArgs().WithFilter(new WithPriority(SubscriberPriority.Medium)).Invoke(this);
```

| 过滤器 | 语义 |
|---|---|
| `WithTag(tag)` | 仅 Tag 匹配的订阅者（tag 为空/空白在构造期 fail-fast） |
| `WithPriority(priority)` | 仅指定优先级档位上的订阅者 |
| `SameSceneAsEmitter` | 仅与发布者同场景的订阅者（多场景叠加加载时用） |
| `OnlySelf` | 仅发布者自身 / 子树 / 父级链上的订阅者 |
| `InsideCollider2D` | 仅位于发布者 Collider2D 范围内的订阅者（空间局域广播） |

自定义过滤器实现 `ISubscriberFilter.ShouldReceive(args, subscriber, priority)`。**fail-closed**：解析不出对象（不是 GameObject/Component、已销毁）时按"不通过"处理，避免过滤失效导致广播扩散。未声明过滤器时列表为 null、零开销。

## 可靠性与性能语义（要点）

- **快照迭代**：回调内退订/新增只改注册表，本趟按快照跑完，不跳人。
- **跨实例重入安全，同实例重入 fail-fast 拒绝**：事件链用新参数实例或 `AesirTimer.NextFrame` 延后一帧；`SetSender` 是发布者侧 API。
- **热路径零反射、零排序、零装箱**；死引用下次分发后自动清理（**该类型不再被发布时不清理**）。

完整语义（重入缓冲、死绑定清理时机、缓冲区复用、`executionMsLimit` 耗时告警）见 [references/dispatch-semantics.md](references/dispatch-semantics.md)。

## 取消传播与监听调试

- **取消传播**：订阅者调 `e.Cancel(this)` 中止本次分发——`First`/`Last` 两档始终收到（前置校验/收尾清理），`High`/`Medium` 档取消之后的订阅者跳过；`e.NonCancellable()` 让事件免疫取消。每趟分发自动复位取消状态，缓存复用的 SO 参数实例不会串味。用于输入吞噬、伤害减免链、互斥 UI 抢先响应。
- **公开诊断 API**：`AesirEventDiagnostics`（运行时零 Odin 依赖）提供订阅树查询（`GetSubscriptions` / `GetSubscribers` / `GetSubscriberCount` / `HasSubscribers`）与链路快照（`TraceProduced` 推送 + `GetRecentTraces` 环形缓冲 + `Enabled` / `TraceCapacity` / `ClearTraces`）。**监控数据全部公开**，没装 Odin 的开发者可直接据此自建监视面板。采集默认编辑器/Development Build 开启、正式包关闭，关闭时热路径零分配。
- **内置窗口**：`Tools/Aesir/Modules/Event Monitor | Event Log | Event Tester`。UI Toolkit 版零 Odin；装 Odin 且开 Aesir Odin 样式时经静态委托路由到 Odin 版（表格 + 即时刷新），功能等价。Tester 仅 Play 模式可触发。

## SO 资产化（策划/美术零代码配置）

- `AesirEventArgsSO`：`Create → Aesir → Event Module → AesirEventArgsSO`，Inspector 经 `SubclassSelector` 选具体参数子类并填载荷；`Raise()` 或 Inspector 按钮触发，**发布者是资产本身**。
- `UnityEventOnAesirEvent`：挂组件 → 下拉选事件类型 → 在 `On Raised` 绑任意 UnityEvent 回调（播音效、激活物体）。运行时改事件类型需**重新启用组件**才重新订阅。
- `[ExcludeSubclassSelector]` 可把类型从下拉中排除。

## 坑位

1. **命名要分清**：只作数据载体 → `XxxEventArgs`；自身持监听者且有 `Invoke` 能力 → `XxxEvent`。`AesirEventArgs` 不持有监听者，与 `MiniEvent<T>`（自包含）定位不同。
2. **退订用句柄**：`AddListener<T>` 返回的 `AutoRemoveListenerHandle` 在 `OnDisable` 中 `Dispose()`；多个订阅各自持有句柄。拿不到句柄时才用 `RemoveListener<T>(obj)` / `RemoveListener(obj)`（O(n) 冷路径，别放高频路径）。
3. **过滤器随事件参数实例走**：**缓存复用的参数实例（如 SO 资产）会保留已加的过滤器**，下次发布仍生效。
4. **callback 参数类型即事件类型**：`AddListener<T>` 的泛型由编译期约束，写错当场编译失败——这是相对旧特性订阅（运行期反射推断）的优势。
5. **跨场景传数据不要自己造机制**：事件模块 + 共享 Model 就是唯一答案（见 `aesir-scene`）。
6. **事件链不要复用正在分发的实例**：`e.Invoke(...)` 在 `e` 自己的订阅回调里会被拒绝（报错）；链式反应用新实例或 `AesirTimer.NextFrame`。分发中途也不要增删发布者的 Collider2D（空间过滤按趟缓存）。

## 设计边界（不要尝试）

不做事件总线/EventChannel、不做 DefaultChannel 频道标签（频道只服务于日志过滤，无路由价值）。元事件的调试职责已由公开诊断 API `AesirEventDiagnostics` + 三个内置窗口承担（零 Odin 依赖、数据公开）；不再单独定义 OnEventRaised 等系统事件类型，避免污染业务事件空间。组件内一对一状态通知用 `ObservableValue<T>`；无参数的一次性回调用 `MiniEvent`。

**特性订阅（`[AesirListener]`）已移除，不要尝试恢复**：它只为省几行 AddListener，却引入无缓存的绑定期反射（~50µs/对象）、表达式树编译缓存、O(n) 退订与运行期才发现的类型错误；Script 订阅 + 句柄已覆盖全部场景，且类型安全、零反射。完整裁决见包内 `Documentation~/event-module.md`「为什么只保留 Script 订阅」与私有笔记仓 `decisions/remove-attribute-subscription.md`。

## 详情与示例

- 完整机制（单注册表结构、性能实测、移除理由）：包内 `Documentation~/event-module.md`。
- 可运行示例：`Samples/Events/01_KeyPress`（基本发布-订阅）、`02_Filters`（WithTag + InsideCollider2D 双重过滤 / OnlySelf，场景内置对照组）、`03_SOAsset`（SO 资产 + UnityEvent 零代码桥接）。
