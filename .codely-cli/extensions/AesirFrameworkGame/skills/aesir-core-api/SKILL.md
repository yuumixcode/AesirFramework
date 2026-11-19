---
name: aesir-core-api
description: Aesir Architecture（RAA）核心 API 速查。用 Aesir 写状态通知、事件、计时或帧回调时加载——ObservableValue<T> 的订阅与静默写入、可观察集合（List/Dictionary/HashSet）的单轨变更通知、MiniEvent / MiniEvent<T> 与自动注销句柄、AbstractContext 生命周期与禁忌、AesirTimer（延时/冷却/重复/暂停/unscaled）与四条时间路径选择、AesirPlayerLoop 帧回调、能力矩阵。触发场景：用户提到 ObservableValue、AddListenerAndInvoke、SetValueSilently、MiniEvent、AutoRemoveListenerHandle、AesirTimer、Countdown、Every、冷却、进度条计时、PlayerLoop、ObservableList，或问"状态变了怎么通知"、"怎么做技能冷却"、"不用协程怎么延时"。
---

# RAA 核心 API 速查

框架的"辅助模块"层。主架构（Context / Model / View / Controller / Presenter / Command / Query）见 `aesir-framework` 与 `aesir-tiers`。

> 完整签名与实测细节见 [references/core-api.md](references/core-api.md)。**档位口径**：默认 Quick 档直接暴露可写 `ObservableValue`；下文 `IReadOnlyObservableValue` 是 Standard/Strict 的升档写法。

## ObservableValue\<T\> — 响应式属性

```csharp
ObservableValue<int> count = new ObservableValue<int>(0);   // Model 持有
count.Value = 10;                                            // 写
int now = count.Value;                                       // 读
count.SetValueSilently(5);                                   // 静默写（不通知）

public IReadOnlyObservableValue<int> Count => count;         // Standard+ 的只读暴露

// 订阅：AddListenerAndInvoke 会立即推一次当前值（推荐）
model.Hp.AddListenerAndInvoke(OnHpChanged)
    .RemoveListenerWhenGameObjectOnDestroyed(gameObject);

void OnHpChanged(int newValue) { /* 新值，单参数 */ }
```

## 可观察集合（ObservableList / Dictionary / HashSet）

单轨变更通知：`AddListener` / `RemoveListener`，载荷 `CollectionChangedEventArgs<T>`，返回 `AutoRemoveListenerHandle`（同样绑生命周期）。
语义：**无变更不通知**、批量操作逐项通知、字典值更新算 Replace、`Move` 是单事件、`Sort` / `Reverse` / `Clear` 统一走 Reset。
队列、过滤视图、环形缓冲等高级能力用上游 Cysharp.ObservableCollections；包内只内置高频子集（详见 `Documentation~/observable-collections.md`）。

## MiniEvent / MiniEvent\<T\> — 轻量事件

```csharp
MiniEvent doorOpened = new MiniEvent();
doorOpened.AddListener(OnDoorOpened).RemoveListenerWhenGameObjectOnDestroyed(gameObject);
doorOpened.Invoke();

MiniEvent<int> scoreChanged = new MiniEvent<int>();
scoreChanged.Invoke(100);
```

- Invoke 路径**零分配**；`AddListener` 有与监听者数量成正比的少量分配——**别在每帧路径反复增删监听**。
- 多参数载荷用 `struct` 包裹成单参事件。
- **异常语义 = 原生 C# 事件（fail-fast）**：一个监听者抛异常会中断后续监听者 → 回调内不要抛。
- 句柄：`RemoveListenerWhenGameObjectOnDestroyed/OnDisable(gameObject)`，或手动 `RemoveListener()`。

## AbstractContext\<T\> — 上下文

注册（被依赖的先注册）→ 按注册顺序初始化 Model → Service → Dispose 逆序。

```csharp
public sealed class GameContext : AbstractContext<GameContext>
{
    protected override void Configure() { RegisterModel(new PlayerModel()); }
}
GameContext.Instance.GetModel<PlayerModel>();   // 懒加载单例
```

- **`Configure()` 中禁止访问 `Instance`**（递归创建第二个上下文）。
- `GetModel` / `GetService` 未注册时抛带修复提示的 `InvalidOperationException`，不返回 null。
- `Instance` 是**纯 C# 单例，不随场景卸载销毁**：重开流程先 `GameContext.Instance.Dispose()` 再 `SceneModule.ReloadScene()`。
- 模块间互相访问放 `OnInitialize()`（此时全部注册完毕），不要在 `Configure()` 里。

## AesirTimer — 计时原语（AbstractTimer + 六种具体形态）

静态工厂 `AesirTimer` 返回 `AbstractTimer` 子类；`AbstractTimer` 是抽象基类（模板方法固定调度引擎 + 共有状态 / 查询 / 生命周期），`AesirTimer` 是抽象门面（工厂 + 委托）。六种形态：`Countdown`→`CountdownTimer`、`NextFrame`→`NextFrameTimer`、`Every`→`RepeatingTimer`、`Interval`→`IntervalTimer`（总时长内每隔固定间隔）、`Frequency`→`FrequencyTimer`（每秒 N 次）、`Stopwatch`→`StopwatchTimer`（正计时读 `Elapsed`）。既有调用零改动。分层参考 [adammyhre/Unity-Improved-Timers](https://github.com/adammyhre/Unity-Improved-Timers)（学习致谢），实现为框架自有。

**选路径只看一个问题：你有每帧回调吗？**

| 场景 | 用什么 |
|------|--------|
| MonoBehaviour 内部的每帧计时 | 本地 `float` 字段 + `Update` |
| 纯 C# 类需要每帧逻辑 | `MonoLifecycleProxy` + `ICustomUpdate` |
| 纯 C# 类只需"到点做一件事 / 冷却进度 / 按间隔重复" | **`AesirTimer`** |
| 需要协程语义（多段等待、依赖动画） | 驱动方挂 GameObject 用协程 |

```csharp
AesirTimer.Countdown(3f, () => _model.Refresh());        // 一次性延时
_cooldown = AesirTimer.Countdown(5f);                    // 冷却：读 Progress / IsFinished，Restart() 复用
AesirTimer.NextFrame(() => RebuildLayout());             // 下一帧一次
AesirTimer.Every(0.5f, Tick, repeatCount: 6);            // 每 0.5s 一次，共 6 次（省略即无限）
AesirTimer.Interval(10f, 1f, () => _model.Spawn());      // 10 秒内每 1 秒触发
AesirTimer.Frequency(5, () => _model.Sample());          // 每秒 5 次
var sw = AesirTimer.Stopwatch(); float elapsed = sw.Elapsed; sw.Cancel(); // 秒表
_cooldown.Pause(); _cooldown.Resume();                   // 实例级暂停（全局暂停用 Time.timeScale）
AesirTimer.Countdown(0.2f, FadeOut, unscaled: true);     // 不受 timeScale 影响
timer.Cancel();                                          // 取消（等于 Dispose）
```

- 没有 `Start()` / `Tick()`：工厂即开始；到期自动出队、**不需要 Dispose 也不会泄漏**（但取消需持有引用）。
- `Every` 一帧卡顿跨多个间隔**只回调一次**（不补发）；`repeatCount < 0` 无限、`0` 一次也不执行；`Interval` 以总时长为终点、末尾不足一个间隔的余量不再触发。
- 正计时用 `AesirTimer.Stopwatch()` 读 `Elapsed`（只想要时间戳时用 `Time.time` 差值更省）；它无终点须手动 `Cancel`。无限 `Every` / `Frequency` 必须能 `Cancel`。
- 排查看 `Tools → Aesir → Architecture → Timer Debugger`（编辑模式无数据源，恒为零）。

## AesirPlayerLoop — 帧回调（不用 MonoBehaviour）

```csharp
var handle = AesirPlayerLoop.Register(AesirLifecyclePhase.BeforeUpdate, MyFrameCallback);
handle.Dispose();                       // 匿名委托只能靠句柄注销
AesirPlayerLoop.EnsureInjected();       // 第三方 SDK 改过 PlayerLoop 后手动补插
```

阶段：`BeforeUpdate`（Update 前）/ `AfterUpdate`（PostLateUpdate 后）。`Register` 会自动检测注入状态并补插。

## 能力矩阵

| 角色 | GetModel | GetService | ExecuteCommand | ExecuteQuery | Initialize | Dispose |
|------|:--------:|:---------:|:--------------:|:------------:|:----------:|:-------:|
| IModel | ✓ | | | | ✓ | ✓ |
| IService | ✓ | ✓ | | | ✓ | ✓ |
| IView | ✓ | ✓ | | | | |
| IController | ✓ | ✓ | ✓ | ✓ | | |
| IPresenter | ✓ | ✓ | ✓ | ✓ | | ✓ |
