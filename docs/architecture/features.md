# Aesir Architecture 特性一览

## ObservableValue\<T\> — 响应式属性

Model 持有可写实例,View 经 `IReadOnlyObservableValue<T>` 只读订阅,读写分离由类型系统闭环:

```csharp
// Model 内部持有与写入
[SerializeField] ObservableValue<int> count = new ObservableValue<int>(0);
public IReadOnlyObservableValue<int> Count => count;
count.Value++;            // 写入并通知

// View 订阅(推荐 AddListenerAndInvoke:订阅即同步初始值)
model.Count.AddListenerAndInvoke(OnCountChanged)
    .RemoveListenerWhenGameObjectOnDestroyed(gameObject);

// 静默设置(不触发通知)
count.SetValueSilently(5);
```

**可观察集合家族**:`ObservableList<T>` / `ObservableDictionary<TKey, TValue>` / `ObservableHashSet<T>` 提供单轨变更通知 `AddListener`——无变更不通知、批量操作逐项通知、Sort / Reverse / Clear 走 Reset、监听句柄可绑定 Unity 生命周期自动移除,与 ObservableValue 同一套读写分离与句柄模式。队列等其他集合形态与同步视图 / R3 集成等高级能力不做,需要时推荐 [Cysharp/ObservableCollections](https://github.com/Cysharp/ObservableCollections)——两者可共存(程序集与命名空间完全隔离,同一项目可同时安装)。

## MiniEvent — 零分配轻量事件

直接多播调用、Invoke 路径零分配,异常语义 = 原生 C# 事件(fail-fast)。`AddListener` 返回 `AutoRemoveListenerHandle`,泄漏防护是 API 形态:

```csharp
MiniEvent doorOpened = new MiniEvent();
doorOpened.AddListener(OnDoorOpened)
    .RemoveListenerWhenGameObjectOnDestroyed(gameObject);   // 或 RemoveListenerWhenGameObjectOnDisable(UI 面板)
doorOpened.Invoke();
```

多参数载荷用 struct 包裹成单参事件。**监听回调不应抛异常**(框架约定):一个监听者抛异常会中断同事件后续监听者,业务异常在回调内部自行处理。

## 通知机制选型

每个场景有且只有一个默认答案 —— 不要在选择中瘫痪:

| 场景 | 推荐 | 理由 |
|------|------|------|
| Model 状态变了要通知外界(血量、分数) | `ObservableValue<T>` | 状态即数据;`AddListenerAndInvoke` 保证新订阅者拿到当前值 |
| 一次性 / 瞬时通知,无状态("门开了") | `MiniEvent` / `MiniEvent<T>` | 零分配直调,句柄即泄漏防护 |
| View 把输入暴露给 Presenter(MVP 内部) | 原生 C# `event` | 编译期限制外部只能 `+=` / `-=` |
| Inspector 拖拽连线的 UI 交互 | `UnityEvent` | 可视化配置,美术 / 策划可调 |

反模式:用 `public Action Xxx { get; set; }` 代替 `event`(外部可整体替换 / 置空 / 触发);用 MiniEvent 承载持续变化的状态(新订阅者拿不到当前值)。

## PlayerLoop 原生生命周期 {#playerloop-hooks}

`AesirPlayerLoop` 将自定义子系统注入 Unity PlayerLoop,无需 MonoBehaviour:

```csharp
AesirPlayerLoop.Register(
    AesirLifecyclePhase.BeforeUpdate, MyFrameCallback);

// 第三方 SDK 修改 PlayerLoop 后调用一次即可自愈(Register 期也会自动检测,检测按帧节流)
AesirPlayerLoop.EnsureInjected();
```

可用阶段:`BeforeUpdate`(Update 前)、`AfterUpdate`(PostLateUpdate 后)。

## 延时与下一帧

`AesirTimer` 是框架驱动的倒计时器:每个计时器是**一个独立实例**,创建即开始、到期自动出队,可随时查询 `Remaining` / `Progress`。它经 [PlayerLoop 帧钩子](#playerloop-hooks) 推进(结算顺序取 `int.MinValue`,先于任何用户钩子),因此天然获得注入自愈,既不需要场景物体也不需要协程:

```csharp
// ① 一次性延时
AesirTimer.Countdown(3f, () => model.Refresh());

// ② 冷却 + 进度条(UI 每帧读 Progress,不需要有人驱动)
_cooldown = AesirTimer.Countdown(5f);
float progress = _cooldown.Progress;   // 0 → 1
_cooldown.Restart();                   // 复用同一实例

// ③ 下一帧执行一次
AesirTimer.NextFrame(() => RebuildLayout());

// ④ 不受 Time.timeScale 影响(暂停菜单 / UI 动画)
AesirTimer.Countdown(0.2f, FadeOut, unscaled: true);
```

**选路径只看一个问题:你有每帧回调吗?**

| 场景 | 用什么 |
|------|--------|
| MonoBehaviour 内部的计时(每帧判定) | 本地 `float` 字段 + `Update` |
| 纯 C# 类需要每帧逻辑 | `MonoLifecycleProxy` + `ICustomUpdate` |
| 纯 C# 类只需要"到点做一件事" / "冷却进度" | `AesirTimer` |
| 需要协程语义(多段等待、依赖对象或动画) | 驱动方挂 GameObject 用协程 |

设计边界:没有 `Start` / `Pause` / `Resume` / `Tick`(工厂方法即开始,暂停靠 `Time.timeScale`);不提供正计时秒表(用 `Time.time` 差值,永不结束的计时器是泄漏源);不做对象池,也不做每帧重复回调的 `Repeat`(到期回调里重建即可)。
## MonoLifecycleProxy — 生命周期代理

将 Unity 原生回调统一为可订阅的 MiniEvent;调用期增删监听为**快照语义**(挂起队列趟末应用,对齐原生多播委托),稳态零分配。仓库级完整游戏案例 `Assets/PlaneWar`(Mono 版)即用原生回调对照这一能力。

## DDOL 显式决策

根单例(AesirArchitecture 等)的 `dontDestroyOnLoad` 序列化字段统一控制预放置 / 运行时两种来源:默认跨场景持久;关闭时随场景卸载销毁(Inspector 警告 + 运行时提醒),多场景叠加加载自行处理。

## Domain Reload 安全(铁律)

静态变量全部显式重置:非泛型类内 `[RuntimeInitializeOnLoadMethod]` 自重置;泛型类经 `ResetStaticsAssistant.Register()` 注册(泛型类 RIOLM 被 Unity 静默跳过,助手补位)。反复进出 Play Mode 无残留。

## 纯 C# 核心 + MonoBehaviour 适配层

Engine 层零 MonoBehaviour 依赖,适配层按需选用:

| 基类 | 能力 | 用途 |
|------|------|------|
| `MonoView<T>` | GetModel, GetService(只读) | MVC Standard / Strict 的 View |
| `MonoViewController<T>` | + ExecuteCommand, ExecuteQuery | MVC Quick 的 View 兼 Controller |
| `AesirView<T>` / `AesirViewController<T>` | 同上 + Odin 增强 | 需要 Odin 时(功能相同,体验更好) |

## GenericLocator\<T\> — 类型键控定位器

按类型注册 / 查询的通用定位器,按注册顺序保序,Context 内部与独立场景均可使用。查询面含 `Get<T>` / `GetAll()` 与诊断用 `GetAllEntries()`(注册键值对),两者均返回物化快照——枚举期间注册 / 注销不会抛"集合已修改"。

## 包内更新器

`Tools → Aesir → Check for Updates`:多源版本检测(直连 GitHub 三层 → 镜像站 → CDN 中转,大陆友好)、按"上次安装清单 − 新版清单"精确差集清理残留(导入成功之后执行)、不误伤用户新增文件、域重载中断安全。

## Odin Inspector 可选集成

经 `ODIN_INSPECTOR` 条件编译,独立 asmdef 隔离。**三条铁律**:核心架构流程闭环(Context 注册 → 初始化 → Command/Query → ObservableValue 通知)无 Odin 可完整运行;调试体验优化品可用 Odin;样式优化与代码逻辑分离(AttributeProcessor 动态注入,运行时程序集零样式特性)。