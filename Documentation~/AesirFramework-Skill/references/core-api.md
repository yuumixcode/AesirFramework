# 核心 API 参考

> RAA 辅助模块的 API 速查。主架构 API（Context、Model、View、Controller、Presenter、Command、Query）见 SKILL.md。
> **档位口径（2026-10-15 起）**：默认最低档（Quick）直接暴露可写 `ObservableValue`、View 直写直读；下文 `IReadOnlyObservableValue` 只读包装是 Standard/Strict 档的升档写法，默认不使用。

## ObservableValue\<T\> — 响应式属性

Model 持有可写实例，View 通过 `IReadOnlyObservableValue<T>` 订阅。

### 基本用法

```csharp
// 创建（初始值 0）
ObservableValue<int> count = new ObservableValue<int>(0);

// 写入（Model 内部）
count.Value = 10;
count.Value++;

// 读取当前值
int current = count.Value;

// 对外只读暴露
public IReadOnlyObservableValue<int> Count => count;
```

### 订阅

```csharp
// 订阅 + 立即触发一次（推荐——确保新订阅者拿到当前值）
model.Count.AddListenerAndInvoke(OnCountChanged)
    .RemoveListenerWhenGameObjectOnDestroyed(gameObject);

// 仅订阅不立即触发
model.Count.AddListener(OnCountChanged)
    .RemoveListenerWhenGameObjectOnDestroyed(gameObject);

// 静默设置值（不触发通知）
count.SetValueSilently(5);
```

### 回调签名

```csharp
// ObservableValue<T> 的监听签名：void(T newValue) — 单参数，新值
void OnCountChanged(int newValue)
{
    // newValue 是变更后的当前值
}
```

## MiniEvent / MiniEvent\<T\> — 轻量事件

> Invoke 路径零分配（直接多播调用）；订阅 / 退订路径有与监听者数量成正比的少量委托分配——仅适合低频订阅，勿在每帧路径反复增删监听。

### 基本用法

```csharp
// 无参事件
MiniEvent doorOpened = new MiniEvent();
doorOpened.AddListener(OnDoorOpened)
    .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
doorOpened.Invoke();

// 单参事件
MiniEvent<int> scoreChanged = new MiniEvent<int>();
scoreChanged.AddListener(OnScoreChanged)
    .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
scoreChanged.Invoke(100);
```

### 自动注销句柄

```csharp
// AddListener 返回 AutoRemoveListenerHandle
var handle = myEvent.AddListener(callback);

// 绑定生命周期
handle.RemoveListenerWhenGameObjectOnDestroyed(gameObject);  // GameObject 销毁时
handle.RemoveListenerWhenGameObjectOnDisable(gameObject);     // GameObject 禁用时（UI 面板等）

// 手动注销
handle.RemoveListener();
```

### 多参数事件

```csharp
// 多参数载荷用 struct 包裹，作为单参事件
public struct DamageInfo
{
    public int Amount;
    public GameObject Source;
}

MiniEvent<DamageInfo> damageDealt = new MiniEvent<DamageInfo>();
damageDealt.Invoke(new DamageInfo { Amount = 50, Source = attacker });
```

> **异常语义**：MiniEvent 的 Invoke 为零分配直调，异常语义 = 原生 C# 事件（fail-fast）。一个监听者抛异常会中断后续监听者。监听回调不应抛异常。

## AbstractContext\<T\> — 上下文

### 生命周期

```csharp
public class GameContext : AbstractContext<GameContext>
{
    protected override void Configure()
    {
        // 注册 Model 和 Service（被依赖的先注册）
        RegisterModel<IGameModel>(new GameModel());
        RegisterService<IAudioService>(new AudioService());
    }
}

// 访问单例（懒加载，初始化成功后才赋值）
GameContext.Instance.GetModel<IGameModel>();

// 模块 OnInitialize（全部注册完毕后调用）
public class GameModel : AbstractModel, IGameModel
{
    protected override void OnInitialize()
    {
        // 此时可安全访问其他已注册的 Model/Service
    }
}
```

### 禁忌

- **Configure() 中禁止访问 `Instance`**——会递归创建第二个上下文
- **Register 与 Get 类型参数必须一致**——按接口注册就按接口获取
- **被依赖的模块先注册**——框架按注册顺序初始化 Model → Service

### 摘除与替换（测试/调试用途）

```csharp
// 按类型键摘除：被摘除实例被 Dispose（订阅不迁移），未注册时幂等无操作
GameContext.Instance.UnregisterModel<IGameModel>();
GameContext.Instance.UnregisterService<IAudioService>();

// 运行时替换：输出 Warning 并 Dispose 旧实例后覆盖
GameContext.Instance.RegisterModel<IGameModel>(new GameModel());
```

## GenericLocator\<T\> — 类型键控定位器

```csharp
// 注册
GenericLocator<IAudioService>.Register(new AudioService());

// 获取
var audio = GenericLocator<IAudioService>.Get();

// 注销
GenericLocator<IAudioService>.Unregister();
```

## PlayerLoop 生命周期注入

```csharp
// 注册帧回调（不需要 MonoBehaviour）
// Register 返回 AutoRemoveListenerHandle：Dispose 时自动注销（匿名委托只能靠它注销）
var handle = AesirPlayerLoop.Register(AesirLifecyclePhase.BeforeUpdate, MyFrameCallback);
AesirPlayerLoop.Register(AesirLifecyclePhase.AfterUpdate, MyAfterUpdateCallback);

// 注销（二选一：句柄 Dispose 或 Unregister 同一委托实例）
handle.Dispose();
AesirPlayerLoop.Unregister(AesirLifecyclePhase.BeforeUpdate, MyFrameCallback);

// 确保已注入（第三方 SDK 修改 PlayerLoop 后调用一次）
AesirPlayerLoop.EnsureInjected();
```

> `Register` 注册回调时会自动检测 PlayerLoop 注入状态并补插。
> 可用阶段：`BeforeUpdate`（Update 前）、`AfterUpdate`（PostLateUpdate 后）。

## AesirTimer — 计时器原语

框架驱动的计时器（`Runtime/Modules/Timer`）：创建即开始、到期自动出队或按间隔重复，**每个计时器是一个独立实例**，可随时查询 `Remaining` / `Progress` / `RemainingExecutions`。
不需要 MonoBehaviour、也不需要协程，供纯 C# 的 Model / Service / Command 使用。

```csharp
// ① 一次性延时
AesirTimer.Countdown(3f, () => _model.Refresh());

// ② 技能冷却 + 进度条（View 每帧读 Progress，不需要有人驱动）
_cooldown = AesirTimer.Countdown(5f);
float progress = _cooldown.Progress;   // 0 → 1
bool ready = _cooldown.IsFinished;
_cooldown.Restart();                   // 复用同一实例，不换引用

// ③ 下一帧执行一次
AesirTimer.NextFrame(() => RebuildLayout());

// ④ 每 0.5 秒结算一次伤害，共 6 次（省略 repeatCount 即无限）
AesirTimer.Every(0.5f, () => _model.TickDamage(), repeatCount: 6);

// ⑤ 实例级暂停（全局暂停仍用 Time.timeScale）
_cooldown.Pause();
_cooldown.Resume();

// ⑥ 不受 Time.timeScale 影响（暂停菜单 / UI 动画）
AesirTimer.Countdown(0.2f, FadeOut, unscaled: true);

// ⑦ 中途取消
var timer = AesirTimer.Countdown(10f, OnTimeout);
timer.Cancel();                        // 与 timer.Dispose() 等价
```

**选路径只看一个问题：你有每帧回调吗？**

| 场景 | 用什么 |
|------|--------|
| MonoBehaviour 内部的计时（每帧判定） | 本地 `float` 字段 + `Update` |
| 纯 C# 类需要每帧逻辑 | `MonoLifecycleProxy` + `ICustomUpdate` |
| 纯 C# 类只需要"到点做一件事" / "冷却进度" / "按间隔重复" | **`AesirTimer`** |
| 需要协程语义（多段等待、依赖对象或动画） | 驱动方挂 GameObject 用协程 |

> - 没有 `Start()` / `Tick()`：工厂方法即开始；全局暂停靠 `Time.timeScale`（默认时间源跟随缩放，`unscaled` 不受影响），单实例暂停用 `Pause()` / `Resume()`。
> - `Every` 的首次回调在 `interval` 之后；`repeatCount < 0` 无限、`0` 表示一次也不执行、有限次数用尽自动出队转 `IsFinished`。
> - `Every` **一帧卡顿跨越多个间隔只回调一次**（不补发），避免"越慢越忙"的回调雪崩。
> - `NextFrame` 落在"此后第一次结算"：在 `Update` / 协程 / 到期回调里创建 → 下一帧执行；在 `AfterAssembliesLoaded` 里创建 → 当帧结算。
> - 到期自动出队，**不需要 Dispose**，也不会泄漏；但取消必须持有引用（丢弃引用即放弃 `Cancel` 的能力）。
> - 同一次结算内多个到期按创建顺序回调；在到期回调里新建的计时器下一帧才推进。
> - 需要**正计时（秒表）**用 `Time.time` 差值，不要用计时器——永远不结束的计时器是泄漏源；无限重复的 `Every` 必须能在合适时机 `Cancel`。
> - 需要另一种时长就新建实例（框架不做对象池）；同一冷却位复用请用 `Restart()`（同时重置执行计数与暂停态）。
> - 运行时排查看 `Tools → Aesir → Architecture → Timer Debugger`（Odin / IMGUI 双版本）：状态、形态、时间源、剩余、进度、已执行 / 还剩几次、存活时长、回调归属，并可按时间或按帧调低拉取频率以降低调试开销；编辑模式下没有数据源，面板数据恒为零。
> - 想在自己组件的 Inspector 里看计时器：字段是普通（非 `[SerializeReference]`）引用时，给组件写自定义 Inspector，读 `Progress` / `Remaining` / `RemainingExecutions` 画即可（`Samples/AesirTimer` 的 `AesirTimerSampleInspector` 是完整范例：数据抽成一行结构，运行时面板与 Inspector 各用一套样式）。

## AesirArchitecture — MonoBehaviour 宿主单例

```csharp
// 场景预放置（推荐）：直接在场景中放 AesirArchitecture 组件
// 运行时创建：Instance getter 自动 FindAnyObjectByType，未找到时创建 + DDOL
```

> 本组件是框架 Mono 组件（MonoLifecycleProxy 等）的 DDOL 宿主，**不初始化任何架构数据**——Context 为纯 C# 懒加载单例，不依赖本组件即可工作。
> 预放置实例不 DDOL；运行时创建的实例 DDOL。

## MonoView\<T\> / MonoViewController\<T\> — MonoBehaviour 适配层

| 基类 | 能力 | 用途 |
|------|------|------|
| `MonoView<T>` | GetModel, GetService（只读） | MVC Standard/Strict 的 View |
| `MonoViewController<T>` | GetModel, GetService, ExecuteCommand, ExecuteQuery | MVC Quick 的 View 兼 Controller |
| `AesirView<T>` | 同 `MonoView<T>` + Odin 增强 | 需要 Odin Inspector 增强时 |
| `AesirViewController<T>` | 同 `MonoViewController<T>` + Odin 增强 | 需要 Odin Inspector 增强时 |

> 无 Odin 时用 `Mono*` 版本；有 Odin 时用 `Aesir*` 版本（功能相同，Inspector 体验更好）。

## 能力矩阵

| 角色 | GetModel | GetService | ExecuteCommand | ExecuteQuery | Initialize | Dispose |
|------|:--------:|:---------:|:--------------:|:------------:|:----------:|:-------:|
| IModel | ✓ | | | | ✓ | ✓ |
| IService | ✓ | ✓ | | | ✓ | ✓ |
| IView | ✓ | ✓ | | | | |
| IController | ✓ | ✓ | ✓ | ✓ | | |
| IPresenter | ✓ | ✓ | ✓ | ✓ | | ✓ |

## AbstractSubmodule — 统一子模块生命周期

Model 和 Service 的共享生命周期逻辑：

```csharp
public abstract class AbstractModel : AbstractSubmodule, IModel { ... }
public abstract class AbstractService : AbstractSubmodule, IService { ... }

// 子模块生命周期：
// Register → OnInitialize() → ... → OnDispose()
// Dispose 后 Initialized 重置为 false（已释放模块不再自称已初始化）
```

## ResetStaticsAssistant — 静态变量重置

```csharp
// 泛型类的静态变量在 Domain Reload 后会被 Unity 静默跳过重置
// 通过 Register 注册重置回调
public class MyGenericClass<T>
{
    static MyGenericClass<T> _instance;

    static MyGenericClass()
    {
        ResetStaticsAssistant.Register(() => _instance = null);
    }
}
```

> 非泛型单例类用 `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` 自重置即可。
