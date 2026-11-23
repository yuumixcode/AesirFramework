# 响应式与事件

Aesir Architecture 的通知体系由四块组成:`ObservableValue<T>` 响应式属性、可观察集合家族、`MiniEvent` 零分配事件、生命周期机制(MonoLifecycleProxy + PlayerLoop)。页尾附**事件机制决策表** —— 每个场景有且只有一个默认答案。

## ObservableValue\<T\> — 响应式属性

Model 持有可写实例,View 经 `IReadOnlyObservableValue<T>` 只读订阅,读写分离由类型系统闭环:

```csharp
// Model:持有可写实例,只读暴露
[SerializeField] ObservableValue<int> count = new ObservableValue<int>(0);
public IReadOnlyObservableValue<int> Count => count;

// 写入(值变化才通知)
count.Value++;
count.Value = 10;

// 静默设置(不触发通知,适合初始化 / 存档回填)
count.SetValueSilently(5);

// View:订阅即同步当前值,句柄绑定生命周期防泄漏
model.Count.AddListenerAndInvoke(OnCountChanged)
    .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
```

安装 Odin Inspector 时,`ObservableValue<T>` 有自定义 Drawer(AttributeProcessor 注入),可在 Inspector 直接编辑与预览值变更。

## 可观察集合家族

`ObservableList<T>` / `ObservableDictionary<TKey, TValue>` / `ObservableHashSet<T>` —— 组合 BCL 集合存储 + MiniEvent 零分配事件,与 ObservableValue 同一套读写分离与句柄模式。单一变更通知 `AddListener`:无变更不通知、批量操作逐项通知、Sort / Reverse / Clear 走 Reset,监听句柄可绑定 Unity 生命周期自动移除。队列等其他集合形态用上游 Cysharp.ObservableCollections。

Action 载荷表、各集合专属能力、句柄生命周期与上游共存说明见[可观察集合](observable-collections.md)。

## MiniEvent — 零分配轻量事件

直接多播调用、Invoke 路径稳态零分配,异常语义 = 原生 C# 事件(fail-fast,一个监听者抛异常会中断后续监听者):

```csharp
MiniEvent doorOpened = new MiniEvent();
MiniEvent<int> scoreChanged = new MiniEvent<int>();

// AddListener 返回 AutoRemoveListenerHandle —— 泄漏防护是 API 形态
doorOpened.AddListener(OnDoorOpened)
    .RemoveListenerWhenGameObjectOnDestroyed(gameObject);

doorOpened.Invoke();
scoreChanged.Invoke(10);
```

`RemoveListenerWhen*` 扩展家族:

| 扩展方法 | 触发时机 |
|----------|---------|
| `RemoveListenerWhenGameObjectOnDestroyed` | GameObject 销毁时 |
| `RemoveListenerWhenGameObjectOnDisable` | GameObject 禁用时(UI 面板推荐) |
| `RemoveListenerWhenOnSceneUnloaded` | 场景卸载时(按 `Scene.handle` 分桶,场景 A 卸载不误杀场景 B) |

多参数载荷用 struct 包裹成单参事件。**监听回调不应抛异常**(框架约定):业务异常在回调内部自行 try-catch。

## MonoLifecycleProxy — 生命周期代理

将 Unity 原生回调统一为可订阅的 MiniEvent,任意对象(不必是 MonoBehaviour)都能接收生命周期:

```csharp
// 方式 1:按事件订阅(order 越小越先执行,同 order 按注册顺序 —— 稳定排序)
MonoLifecycleProxy.Instance.AddListener(MonoLifecycleEvent.Update, MyTick, order: 0);

// 方式 2:接口自动注册(实现 ICustomUpdate / ICustomFixedUpdate 等 8 个接口之一)
MonoLifecycleProxy.Register(this as MonoBehaviour);   // 扩展方法形式已于 0.32.0 整体移除
```

可用事件:`FixedUpdate`、`BeforeUpdate`、`Update`、`LateUpdate`、`AfterUpdate`、`OnApplicationFocus`、`OnApplicationPause`、`OnApplicationQuit` —— 其中 `BeforeUpdate` / `AfterUpdate` 由 PlayerLoop 驱动,其余由 Unity 原生回调触发。

调用期增删监听为**快照语义**(挂起队列趟末按发生顺序应用,对齐原生多播委托):监听者在回调中安全地增删监听,不会跳帧或抛集合修改异常。

## AesirPlayerLoop — 游戏级帧钩子

无需 MonoBehaviour,把回调注入 Unity PlayerLoop 的两个阶段:

| 阶段 | 插入点 | 典型用途 |
|------|--------|---------|
| `BeforeUpdate` | `PlayerLoop.Update` 子系统**之前** | 架构逻辑优先运算 |
| `AfterUpdate` | `PlayerLoop.PostLateUpdate` 子系统**之后** | 读取当前帧最终状态 |

```csharp
AesirPlayerLoop.Register(
    AesirLifecyclePhase.BeforeUpdate, MyFrameCallback, order: 0);

// 持有者销毁前必须注销(传入同一委托实例,匿名函数无法注销)
AesirPlayerLoop.Unregister(
    AesirLifecyclePhase.BeforeUpdate, MyFrameCallback);
```

特性:

- **注入自愈** —— 第三方 SDK 用缓存副本 `SetPlayerLoop` 会抹掉框架注入点;`EnsureInjected()` 在域加载时与每次注册时自动补插(检测按帧节流,同一帧内至多复查一次),也可手动调用
- **稳定排序** —— `order` 越小越先执行;同 order 按注册顺序(插入序号次级键)
- **待处理命令** —— 遍历期间 Register / Unregister 不立即生效,缓存到趟末统一执行

需要更自由的 PlayerLoop 插入点时用底层工具 `PlayerLoopUtility`(`InsertSystemBefore<T>` / `InsertSystemAfter<T>` / `ContainsSystem<T>`)。

## 事件机制决策表

| 场景 | 唯一推荐 | 一句话理由 |
|------|---------|-----------|
| Model 状态变了要通知外界(血量、金币、分数) | `ObservableValue<T>` | 状态即数据,变化自动通知;`AddListenerAndInvoke` 保证新订阅者拿到当前值 |
| 一次性 / 瞬时通知,无状态("门开了""怪死了") | `MiniEvent` / `MiniEvent<T>` | 零分配直调,句柄即泄漏防护 |
| View 把输入暴露给 Presenter(MVP 内部) | 原生 C# `event` | 编译期限制外部只能 `+=` / `-=` |
| Inspector 拖拽连线的 UI 交互(Button.onClick) | `UnityEvent` | 编辑器可视化配置,美术 / 策划可调 |

### 反模式

| 反模式 | 问题 | 正确做法 |
|--------|------|---------|
| `public Action Xxx { get; set; }` 代替 `event` | 外部可整体替换 / 置空 / 触发事件链 | `event Action Xxx;` |
| 用 MiniEvent 承载持续变化的状态 | 新订阅者拿不到当前值 | `ObservableValue<T>` + `AddListenerAndInvoke` |
| 用 UnityEvent 做跨模块逻辑通知 | 配置藏在 Inspector,代码无法搜索 / diff | 代码内 MiniEvent 或 ObservableValue |
| Model 直接持有 UnityEvent 字段 | 数据层与编辑器配置耦合,破坏纯 C# 可测试性 | `ObservableValue<T>` |

### 与 MVC / MVP 三档的关系

| 档位 | 通知机制 |
|------|---------|
| MVC 快捷 / 标准 | `ObservableValue`(订阅刷新) |
| MVC 严格 | `ObservableValue`(原始值订阅刷新)+ Query(加工值场景) |
| MVP 全档 | C# `event`(View→Presenter 输入)+ `ObservableValue`(Presenter 读 Model) |

## 继续阅读

- [架构概念](concepts.md) —— Context 与 Domain Reload 安全
- [示例总览](../product/samples.md) —— MiniEvent / ObservableValue 独立示例,以及仓库级完整游戏案例 PlaneWar(不随包分发)
