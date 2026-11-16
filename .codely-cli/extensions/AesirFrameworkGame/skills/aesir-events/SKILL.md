---
name: aesir-events
description: Aesir Modules 的事件模块（EventModule）编码指南。为 Unity/团结引擎项目做模块间解耦、发布-订阅时使用——[AesirListener] 特性订阅与脚本 Lambda 订阅双轨、四档订阅优先级、订阅者过滤器（WithTag / InsideCollider2D / SameSceneAsEmitter / OnlySelf / WithPriority）精确投递、快照迭代与重入安全、死引用自动清理、AesirEventArgsSO 事件资产化与 UnityEvent 桥接。触发场景：用户提到 EventModule、AesirEventArgs、AesirListener、发布订阅、事件、广播、监听、EventModule.AddListener、OnXxxEventArgs、SO 事件、UnityEventOnAesirEvent，或要求"在模块之间发消息"、"UI 响应游戏事件"、"让敌人只收到范围内的爆炸"。
---

# Aesir 事件模块（EventModule）

双轨订阅的发布-订阅系统：**Attribute 静态订阅**（`[AesirListener]` 反射扫描）+ **Script 动态订阅**（Lambda 委托 + 句柄）。两轨共存于独立注册表，分发时合并进快照、按优先级稳定排序执行。

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

// 2. Attribute 订阅（方法特性，AllowMultiple）
public sealed class ScoreView : MonoBehaviour
{
    void OnEnable()  => EventModule.AddListener(this);   // 反射扫描本对象全部 [AesirListener]
    void OnDisable() => EventModule.RemoveListener(this); // 移除该对象全部绑定（两轨）

    [AesirListener]
    void OnPlayerScored(PlayerScoredEventArgs e) => UpdateScore(e.points);
}

// 3. 发布（发布者默认是事件参数自己）
new PlayerScoredEventArgs { points = 10 }.Invoke();
new PlayerScoredEventArgs { points = 10 }.Invoke(this);              // 指定 Sender
EventModule.InvokeEvent(this, new PlayerScoredEventArgs { points = 10 });
```

脚本订阅（返回句柄，只移除这一条）：

```csharp
AutoRemoveListenerHandle _handle;

void OnEnable() => _handle = EventModule.AddListener<PlayerScoredEventArgs>(this, e => UpdateScore(e.points));
void OnDisable() => _handle.Dispose();   // 重复 Dispose 安全
```

## 优先级（4 档）

`SubscriberPriority`：`First` → `High` → `Medium` → `Last`。**Attribute 默认 High，Script 默认 Medium**；同档按注册顺序（`InsertionIndex`）。

```csharp
[AesirListener(SubscriberPriority.First)]
void OnBoot(BootEventArgs e) { }

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

## 可靠性与性能语义（写代码时需要知道的）

- **快照迭代**：回调内退订/新增只改注册表，本趟仍按快照跑完（退订者本趟仍收到，新增者下趟起收到），不会跳人。
- **支持重入发布**：回调内可同步再发布事件（同型或异型）；重入层用独立局部列表/参数数组，外层剩余订阅者仍收到正确的外层参数。代价是重入层一次小分配，顶层分发零列表分配。
- **死引用自动清理**：订阅者被 Destroy 后绑定残留会在下次分发循环结束后移除（编辑器内 Warning 提示检查退订，玩家构建静默）。**但该事件类型不再被发布时不会清理**——按需发布即无累积。
- **热路径零反射**：`StaticBindingInfo` 在注册期（冷路径）用表达式树把 `MethodInfo` 编译成委托，分发期直接调用（比 `MethodInfo.Invoke` 快约 20–40 倍）；绑定键按类型缓存字符串，稳态零分配。
- **可选耗时告警**：预放置实例的 `executionMsLimit`（ms，默认 0 关闭），超阈值输出 Warning；仅顶层分发计时。

## SO 资产化（策划/美术零代码配置）

- `AesirEventArgsSO`：`Create → Aesir → Event Module → AesirEventArgsSO`，Inspector 经 `SubclassSelector` 选具体参数子类并填载荷；`Raise()` 或 Inspector 按钮触发，**发布者是资产本身**。
- `UnityEventOnAesirEvent`：挂组件 → 下拉选事件类型 → 在 `On Raised` 绑任意 UnityEvent 回调（播音效、激活物体）。运行时改事件类型需**重新启用组件**才重新订阅。
- `[ExcludeSubclassSelector]` 可把类型从下拉中排除。

## 坑位

1. **命名要分清**：只作数据载体 → `XxxEventArgs`；自身持监听者且有 `Invoke` 能力 → `XxxEvent`。`AesirEventArgs` 不持有监听者，与 `MiniEvent<T>`（自包含）定位不同。
2. **退订路径成对**：Attribute 用 `RemoveListener(this)`；Script 用句柄 `Dispose()`。语义上是"每人一条"的场景别混用两种。
3. **过滤器随事件参数实例走**：`Clone()` 共享同一列表；**缓存复用的参数实例（如 SO 资产）会保留已加的过滤器**，下次发布仍生效。
4. **`[AesirListener]` 方法签名要能被扫描到**（实例方法、参数类型与事件类型一致）；改方法签名后要重新 `AddListener` 才生效。
5. **跨场景传数据不要自己造机制**：事件模块 + 共享 Model 就是唯一答案（见 `aesir-scene`）。

## 设计边界（不要尝试）

不做事件总线/EventChannel、不做元事件（OnEventRaised 等）与 DefaultChannel 频道标签（依赖编辑器工具链，暂缓）。组件内一对一状态通知用 `ObservableValue<T>`；无参数的一次性回调用 `MiniEvent`。

## 详情与示例

- 完整机制（表达式树优化实测数据、双注册表结构）：包内 `Documentation~/event-module.md`。
- 可运行示例：`Samples/Events/01_KeyPress`（基本发布-订阅）、`02_Filters`（WithTag + InsideCollider2D 双重过滤 / OnlySelf，场景内置对照组）、`03_SOAsset`（SO 资产 + UnityEvent 零代码桥接）。
