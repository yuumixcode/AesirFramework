# 事件模块（Event Module）

## 概述

事件模块提供基于 Script 订阅的事件系统，实现业务模块间的发布-订阅解耦。

- **Script 订阅**：`AddListener<T>(subscriber, callback)` 注册委托，返回 `AutoRemoveListenerHandle` 精确退订；这是
  **唯一**的订阅形态
- 订阅按 4 档优先级稳定排序执行（同档按注册顺序）
- **订阅者过滤器**：`WithFilter` 链式声明"只让特定范围的订阅者收到"（精确投递）
- **取消传播**：`Cancel(this)` 中止本次分发（First/Last 档不受影响）
- **分发期可靠性**：快照迭代（回调内退订/注册不影响本趟分发）、支持跨实例重入发布（同一实例重入被拒绝）、自动清理已销毁订阅者（死引用）；可选分发耗时告警
- **SO 资产化**：`AesirEventArgsSO` 让事件可保存为 .asset 资源，`UnityEventOnAesirEvent` 桥接 Inspector
  UnityEvent
- **可观测性**：`AesirEventDiagnostics` 公开门面 + Monitor / Log / Tester 三个调试窗口

> **注意**：`AesirEventArgs` 是事件参数载体（类似 `EventArgs`），本身不持有监听者。订阅管理由 `EventModule`
> 的注册表负责。这与 `MiniEvent`（自身持有 `Action` 列表的自包含事件）在设计定位上不同。

> **设计裁决**：本模块曾提供 `[AesirListener]` 特性反射订阅，现已移除——理由见文末「为什么只保留 Script 订阅」。

## 核心类型

### AesirEventArgs

事件参数抽象基类。

```csharp
[Serializable]
public abstract class AesirEventArgs
{
    public object Sender { get; private set; }                 // 发布者，分发时写入
    public IReadOnlyList<ISubscriberFilter> Filters { get; }  // 过滤器列表（未添加时为 null）

    public bool IsCancelled { get; }                           // 本次分发是否被取消
    public object CancelledBy { get; }                         // 取消者

    public AesirEventArgs SetSender(object sender);            // 链式（发布者侧 API）
    public AesirEventArgs WithFilter(ISubscriberFilter filter);            // 链式添加过滤器
    public AesirEventArgs WithFilters(params ISubscriberFilter[] filters); // 链式添加一组过滤器
    public AesirEventArgs Cancel(object canceller);            // 取消本次传播
    public AesirEventArgs NonCancellable();                    // 使事件免疫取消
    public void Invoke();                                      // 用 Sender 触发
    public void Invoke(object sender);                         // 用指定 sender 触发
}
```

> **分发约定**（低概率场景用约定杜绝，不做防御兜底）：
> - **同一参数实例在分发期间不得被再次发布**——`EventModule` 会输出错误日志并拒绝（fail-fast）。事件链请使用新的参数实例，或经
>   `AesirTimer.NextFrame` 延后发布；发布**其他实例**（同型或异型）不受限制。
> - `SetSender` 是发布者侧 API，订阅者不应在分发期间调用。
> - 分发中途增删/替换发布者的 `Collider2D` 不被支持（空间过滤器的解析结果按趟缓存）。

### EventModule

MonoBehaviour 单例，直接继承 `AesirMonoBehaviour`，作为 `AesirModules` 子物体懒加载创建。

```csharp
// Script 订阅（返回 AutoRemoveListenerHandle，首选退订路径）
public static AutoRemoveListenerHandle AddListener<TEventArgs>(object subscriber, Action<TEventArgs> callback);
public static AutoRemoveListenerHandle AddListener<TEventArgs>(object subscriber, Action<TEventArgs> callback, SubscriberPriority priority);
public static AutoRemoveListenerHandle AddListener(object subscriber, AesirEventArgs eventArgs, Action<AesirEventArgs> callback);  // 非泛型版

// 按对象退订（退订方无句柄时的补充路径）
public static void RemoveListener<TEventArgs>(object subscriber);  // 精确退订该对象的某事件类型（O(1) 查表）
public static void RemoveListener(object subscriber);              // 批量退订该对象全部订阅（O(n) 冷路径）

// 事件触发
public static void InvokeEvent<TEventArgs>(object sender, TEventArgs eventArgs);

// 单例
public static EventModule Instance { get; }

// 可靠性配置（预放置实例可在 Inspector 调整）
// [SerializeField] float executionMsLimit = 0f;  — 分发耗时告警阈值（ms），0 = 关闭
```

### 绑定信息（internal 实现细节）

订阅绑定由 internal 的 `BindingInfo` 基类 + `ScriptBindingInfo<T>` 承载：持有 `Action<T>`
直接委托，注册后直接调用，无反射、无表达式树、无参数数组。**包外不可见**——订阅/退订必须经 `EventModule` 公开
API，诊断展示所需的信息（事件类型、监听成员名、优先级、存活状态）经 `AesirEventDiagnostics` 的快照公开。

### SubscriberPriority

```csharp
public enum SubscriberPriority
{
    First,   // 最前 — 前置校验；不受取消传播影响
    High,    // 高优先级（需显式指定）
    Medium,  // 中优先级 — Script 订阅默认值
    Last     // 最后 — 收尾/清理；不受取消传播影响
}
```

## 使用方式

### 1. 定义事件参数

```csharp
using System;
using Runestone.AesirModules;

[Serializable]
public class OnPlayerScored : AesirEventArgs
{
    public int points;
    public string playerName;
}
```

### 2. Script 订阅

```csharp
using UnityEngine;
using Runestone.AesirModules;
using Runestone.AesirArchitecture;

public class ScoreController : MonoBehaviour
{
    AutoRemoveListenerHandle _handle;

    void OnEnable() =>
        _handle = EventModule.AddListener<OnPlayerScored>(this, e =>
            Debug.Log($"Score: {e.points}"));

    void OnDisable() => _handle.Dispose();
}
```

一个方法监听多种事件 = 多条 `AddListener<T>`，各自持有句柄：

```csharp
AutoRemoveListenerHandle _scoreHandle;
AutoRemoveListenerHandle _levelHandle;

void OnEnable()
{
    _scoreHandle = EventModule.AddListener<OnPlayerScored>(this, OnScored);
    _levelHandle = EventModule.AddListener<OnLevelLoaded>(this, OnLevel);
}
void OnDisable() { _scoreHandle.Dispose(); _levelHandle.Dispose(); }
```

### 3. 无句柄退订（外部解绑）

退订方拿不到句柄时（诊断工具强制解绑、临时屏蔽某对象）：

```csharp
// 只退该对象对 OnPlayerScored 的订阅
EventModule.RemoveListener<OnPlayerScored>(someSubscriber);

// 退该对象的全部订阅
EventModule.RemoveListener(someSubscriber);
```

### 4. 发布事件

```csharp
// 链式调用
new OnPlayerScored { points = 10, playerName = "Player1" }.Invoke(this);

// 直接调用
EventModule.InvokeEvent(this, new OnPlayerScored { points = 10 });
```

### 5. 指定优先级

```csharp
EventModule.AddListener<OnPlayerScored>(this, e => { ... }, SubscriberPriority.First);
EventModule.AddListener<OnPlayerScored>(this, e => { ... }, SubscriberPriority.Last);
```

### 6. 订阅者过滤器（精确投递）

发布时通过 `WithFilter` 链式声明过滤器，分发时逐订阅者检查，全部通过才投递：

```csharp
using Runestone.AesirModules;

// 只有 Tag 为 "Enemy" 的订阅者收到
new OnExplosion().WithFilter(new WithTag("Enemy")).Invoke(this);

// 组合过滤：爆炸半径内的敌人（两个过滤器都须通过）
new OnExplosion()
    .WithFilter(new InsideCollider2D())
    .WithFilter(new WithTag("Enemy"))
    .Invoke(this);

// 只通知与发布者同场景 / 同家族的订阅者
new OnLevelLoaded().WithFilter(new SameSceneAsEmitter()).Invoke(this);
new OnChildActivated().WithFilter(new OnlySelf()).Invoke(this);

// 只通知某一优先级档位的订阅者
new OnTick().WithFilter(new WithPriority(SubscriberPriority.Medium)).Invoke(this);
```

内建过滤器：

| 过滤器                   | 语义                                                                   |
|--------------------------|------------------------------------------------------------------------|
| `WithTag(tag)`           | 仅 Tag 匹配的订阅者收到（tag 须非 null 非空串，构造期 fail-fast 抛出） |
| `WithPriority(priority)` | 仅绑定在指定优先级档位的订阅者收到                                     |
| `SameSceneAsEmitter`     | 仅与发布者同场景的订阅者收到（适配多场景叠加加载）                     |
| `OnlySelf`               | 仅发布者自身/子树/父级链上的订阅者收到                                 |
| `InsideCollider2D`       | 仅位于发布者 Collider2D 范围内的订阅者收到（空间局域广播）             |

自定义过滤器：实现 `ISubscriberFilter.ShouldReceive(AesirEventArgs, object subscriber, SubscriberPriority)` 即可。

> **fail-closed 约定**：过滤器无法解析对象（发布者/订阅者不是 GameObject 或
> Component、已销毁）时按"不通过"处理，避免过滤条件失效导致事件意外扩散。过滤器抛出的异常与订阅者回调一样被分发隔离（不影响其他订阅者）。
>
> **生命周期**：过滤器随事件参数实例存在；缓存复用的参数实例（如 `AesirEventArgsSO`）会保留已添加的过滤器。

### 7. SO 资产化（Inspector 配置事件）

**AesirEventArgsSO** — 事件参数的 ScriptableObject 包装：

- Project 右键 `Create → Aesir → Event Module → AesirEventArgsSO` 创建资产
- Inspector 中经 `SubclassSelector` 下拉选择具体事件参数子类并配置载荷字段
- 运行时点击 Inspector 的「触发事件（Raise）」按钮，或代码调用 `Raise()`；发布者为资产本身

```csharp
// 代码触发（发布者 = SO 资产）
scoredEventAsset.Raise();
```

**UnityEventOnAesirEvent** — 桥接组件，非程序员在 Inspector 串联事件回调：

1. 挂载组件（Add Component → Aesir Modules/UnityEvent On AesirEvent）
2. 经下拉选择监听的事件类型
3. 在 On Raised 中绑定任意 UnityEvent 回调（播放音效、激活物体等）

> `SubclassSelector`（UI Toolkit PropertyDrawer）对任意 `[SerializeReference]` 字段生效；
> `ExcludeSubclassSelector` 特性可把不想出现在下拉中的类型排除。运行时修改事件类型需重新启用组件使订阅生效。

## 架构设计

### 单注册表（Type 键 + 注册期有序）

只有一张 Script 订阅注册表，以事件参数类型为键，列表在**注册时**维护有序（Priority 升序、同档按注册顺序），分发直接取用：

```
Bindings (Dictionary<Type, List<BindingInfo>>)
  └─ ScriptBindingInfo<T> (Action<T> 委托，internal)

RaiseEvent:
  1. 同实例重入守卫（分发中的实例再次发布 → 报错拒绝）
  2. 按事件类型 O(1) 取订阅者列表（已有序，无需排序）
  3. 拷贝进迭代快照（顶层复用迭代缓冲区；重入层用按深度复用的独立缓冲区）
  4. 逐订阅者：死引用检查 → 取消检查 → 过滤器检查 → 委托直调
  5. 循环外：移除本轮收集的死绑定；超阈值输出耗时告警（仅顶层分发计时）
```

### 快照迭代与重入约定

分发循环基于注册表快照执行，两个语义由此保证：

- **回调内退订/注册不影响本趟分发**——订阅者回调内 `RemoveListener` / 句柄 `Dispose` /
  新增订阅，只修改注册表本身，本趟迭代仍按快照执行完毕（退订的订阅者本趟仍会收到，新增的订阅者从下趟开始收到），不会出现"退订后续订阅者导致跳过一个"的索引位移。
- **回调内可安全同步发布其他参数实例（跨实例重入）**
  ——同型或异型均可，重入层使用按深度复用的独立迭代缓冲区与死绑定收集，外层循环继续时剩余订阅者仍收到正确的外层参数。缓冲区池随最大嵌套深度增长一次，之后稳态零分配。

**同一参数实例的重入发布被拒绝（fail-fast）**：分发期间再次发布同一实例（如在订阅者回调里对正在分发的 `e` 调
`Invoke`，或重入触发同一个 `AesirEventArgsSO`
资产）会输出错误日志并跳过。这是"低概率问题用约定杜绝、不加防御兜底"的设计裁决——原实现为这个极低频场景维护逐订阅者
Sender 校正，已删除。事件链的正确写法：

```csharp
// ✅ 新实例
EventModule.AddListener<EnemyDiedEventArgs>(this, e =>
    new ScoreChangedEventArgs { delta = e.score }.Invoke(this));

// ✅ 延后一帧（RAA 计时器）
EventModule.AddListener<EnemyDiedEventArgs>(this, e =>
    AesirTimer.NextFrame(() => new ScoreChangedEventArgs { delta = e.score }.Invoke(this)));

// ❌ 同一实例再发一次——被拒绝并报错
EventModule.AddListener<EnemyDiedEventArgs>(this, e => e.Invoke(this));
```

### 死引用清理

订阅者 GameObject 被 Destroy 后绑定仍残留在注册表。分发循环内把 Unity 假 null 的订阅者收集到复用列表，
**循环结束后**从注册表移除并输出 Warning 提示检查退订遗漏（编辑器内；玩家构建中静默清理）。同一订阅者的事件类型不再被发布时，其死绑定不会清理——按需发布即可，无泄漏累积。

### 性能模型

**热路径（分发期）零反射、零排序、零字符串分配、零装箱**：

- 注册表以 `Type` 为键（`RuntimeTypeHandle` 哈希），无字符串键拼接与缓存；
- 顺序在注册期维护（有序插入，冷路径），分发侧零排序零比较；
- 委托直调收 `AesirEventArgs`，无 `object[]` 参数数组与类型模式检查；
- 顶层分发复用迭代缓冲区与死绑定收集列表，重入层用按深度复用的缓冲区池（Clear 保留容量，稳态零分配）；
- 空间过滤器的发布者 GameObject / Collider2D 按趟解析一次、N 个订阅者共享；
- 无实例时 `TryGetExisting` 走负缓存快速路径（每个"实例缺失期"至多一次全场景查找）；
- 过滤器列表懒分配（未声明过滤器时为 null，零开销）；诊断关闭时只多一次静态 bool 读取。

| 操作             | 成本量级                                    |
|------------------|---------------------------------------------|
| 委托直调         | ~4ns/次                                     |
| Type 键字典查询  | O(1)，零分配                                |
| 注册（有序插入） | 同档追加 O(1)；跨档插入 O(n) 移位（冷路径） |

对比：曾有过的 `[AesirListener]` 特性订阅在绑定期需 `GetMethods` + `GetCustomAttributes`（~
50µs/对象）、表达式树首次编译（1-3ms/方法）、维护一整套缓存基础设施——这些开销已随特性订阅移除而消失。

**性能回归测试**（`Tests/Editor/EventModuleTests.cs`）：

- `Dispatch_PriorityTiers_RegisteredOutOfOrder_StillSorted` — 乱序注册仍全序执行（锁定注册期有序插入）
- `Dispatch_ThousandSubscribers_CompletesWithinGenerousBudget` — 1000 订阅者单次分发的信息性软门槛

### 性能监控

`executionMsLimit`（毫秒，默认 0 = 关闭）为分发耗时告警阈值。开启后单次分发超过阈值输出
Warning（含事件名、耗时与订阅者数量）。计时复用静态 `Stopwatch`，零稳态分配；**仅顶层分发计时**——重入层不计时，避免嵌套分发的
Restart/Stop 互相破坏计时。

### 退订

- `AutoRemoveListenerHandle.Dispose()` — 首选，精确移除单条订阅（重复调用安全）
- `RemoveListener<T>(subscriber)` — 无句柄时精确退订某对象的某事件类型
- `RemoveListener(subscriber)` — 无句柄时批量退订该对象全部订阅（冷路径 O(n)）
- 遗漏退订且订阅者被销毁 → 下次分发自动清理（死引用清理兜底）

## 取消传播

订阅者可在回调内调用 `e.Cancel(this)` 中止本次分发：

- `First` / `Last` 两档**始终收到**（分别承担前置校验与收尾清理，不受取消影响）；
- `High` / `Medium` 档中，取消之后的剩余订阅者被跳过；
- `NonCancellable()` 让事件免疫取消（结算、存档等必须送达所有订阅者的场景）；
- 每趟分发自动复位取消状态，缓存复用的参数实例（`AesirEventArgsSO`）不会带入上一趟痕迹。

```csharp
void OnBeforeClose(OnPanelClose e)
{
    if (!CanClose)
    {
        e.Cancel(this); // 阻止后续 High/Medium 档订阅者关闭面板
    }
}

// 订阅时把该方法放到 High 档（前置校验语义）
_handle = EventModule.AddListener<OnPanelClose>(this, OnBeforeClose, SubscriberPriority.High);
```

典型用途：输入吞噬、伤害减免链、互斥 UI 抢先响应。

## 可观测性与调试

`AesirEventDiagnostics` 是全部监控数据的**公开门面**，只依赖运行时类型、零 Odin 依赖——未安装 Odin
的开发者可直接用它自建监视面板。

### 订阅树查询（拉取）

```csharp
AesirEventSubscriptionSnapshot[] subs = AesirEventDiagnostics.GetSubscriptions();
AesirSubscriberSnapshot[] listeners = AesirEventDiagnostics.GetSubscribers(typeof(OnPlayerScored));
int count = AesirEventDiagnostics.GetSubscriberCount(typeof(OnPlayerScored));
bool any = AesirEventDiagnostics.HasSubscribers(typeof(OnPlayerScored));
```

快照为只读 struct，含事件类型、订阅者实例、监听成员名、优先级、是否存活。

### 链路快照（推送 + 环形缓冲）

```csharp
// 实时推送：订阅此事件即可制作监视面板
AesirEventDiagnostics.TraceProduced += snap => { /* 处理 snap */ };

// 回看历史：最新在前，环形缓冲有硬容量上限
AesirEventTraceSnapshot[] recent = AesirEventDiagnostics.GetRecentTraces(max: 50);

AesirEventDiagnostics.TraceCapacity = 256;      // 容量（防日志无限增长）
AesirEventDiagnostics.Enabled = true;           // 采集总开关
AesirEventDiagnostics.ClearTraces();            // 清空
```

`AesirEventTraceSnapshot` 记录种类（Raised / SubscriberBound / SubscriberUnbound）、时间戳（**UTC**，展示时
`ToLocalTime()`）、事件类型、参与者、命中订阅者数、耗时、是否取消、过滤器数。参与者名称在首次采集时弱表缓存（运行期改名不反映）。采集默认在编辑器与
Development Build 开启、正式包关闭；关闭时分发热路径零分配。

### 内置调试窗口

菜单 `Tools → Aesir → Modules`：

| 窗口              | 用途                                                               |
|-------------------|--------------------------------------------------------------------|
| **Event Monitor** | 实时展示"哪些事件有哪些订阅者"（类型 / 优先级 / 存活）             |
| **Event Log**     | 实时展示触发 / 绑定 / 解绑链路，含命中数、耗时、取消状态，容量上限 |
| **Event Tester**  | 选事件类型、填载荷、手动触发（仅 Play 模式）                       |

每个窗口都有 UI Toolkit 版（核心 Editor 程序集、零 Odin）与 Odin 版（经静态委托路由）。装有 Odin 且开启 Aesir Odin
样式时自动用 Odin 版（表格 + 即时刷新）；未装 Odin 或关闭样式时落回 UI Toolkit 版，功能等价。

## 为什么只保留 Script 订阅（移除特性的理由）

本模块早期提供 `[AesirListener]` 特性反射订阅：在方法上标注特性，`AddListener(obj)` 反射扫描对象全部方法完成注册。现已
**彻底移除**，只保留 Script 订阅。理由：

1. **收益仅为省几行代码**。特性订阅的全部优势——声明就近、一个方法监听多事件、一次注册全部——本质都是"把 N 条
   AddListener 压成 N 个标注 + 1 条注册"。它能做到的事，Script 订阅全都能做到；没有独占能力。
2. **性能更差且随对象数线性增长**。绑定期 `GetMethods` + 逐方法 `GetCustomAttributes` + LINQ ≈ 50µs/对象，
   **无类型缓存**——1000 个持续生成的对象 ≈ 50ms 纯反射，表现为"每帧都在跑启动反射"；还有表达式树首次编译
   1-3ms/方法、`RemoveListener(obj)` 的 O(n) + `Keys.ToList()` 分配。
3. **类型安全更弱**。Script 订阅的 callback 参数由泛型编译期约束；特性订阅靠反射从首参推断事件类型 +
   字符串键匹配，签名错误（多参数 / ref-out / 类型不符）要到 `OnEnable` 反射扫描时才暴露。
4. **维护面与框架定位冲突**。签名校验、类型推断、表达式树编译与缓存、去重、多套 Warning
   文本——整套基础设施只为"省几行"服务，与 RAA/RAM 的极简基调相悖。
5. **AI 时代学习成本被放大**。AI 写 `AddListener<T>(this, e => ...)`
   完全自然，不需要记特性名、不受签名约束；特性的"声明式美感"对 AI 零增益。学习成本高的机制会被开发者用 AI 直接绕过。

**迁移**：把 `[AesirListener] void OnXxx(MyArgs e)` 改成 `EventModule.AddListener<MyArgs>(this, e => { ... })`
并持有句柄，`OnDisable` 中 `Dispose()`；退订方无句柄时用 `RemoveListener<T>(subscriber)` 或
`RemoveListener(subscriber)`。完整裁决见私有笔记仓 `decisions/remove-attribute-subscription.md`。

## 目录结构

```
Runtime/Events/
├── AesirEventArgs.cs              # 事件参数基类（Sender + WithFilter + Cancel 链式 API）
├── AesirEventArgsSO.cs            # SO 资产包装（CreateAssetMenu + Raise）
├── AesirEventUtility.cs           # 静态工具（Unity null 检测、GameObject 解析）
├── AesirEventDiagnostics.cs       # 诊断公开门面（订阅查询 + 链路快照 + 环形缓冲）
├── BindingInfo.cs                 # 绑定信息（internal）：基类 + ScriptBindingInfo<T>
├── ISubscriberFilter.cs           # 过滤器策略接口
├── SubscriberFilters.cs          # 内建过滤器（WithTag/WithPriority/SameSceneAsEmitter/InsideCollider2D/OnlySelf）
├── SubclassSelectorAttribute.cs   # [SerializeReference] 子类下拉特性 + 排除特性
├── Component/
│   ├── EventModule.cs             # 事件模块单例（死引用清理 + 过滤器检查 + 取消传播 + 性能监控）
│   └── UnityEventOnAesirEvent.cs  # UnityEvent 桥接组件
└── SubscriberPriority.cs          # 优先级枚举（4 档：First/High/Medium/Last）

Editor/Events/
├── SubclassSelectorDrawer.cs      # 子类下拉 PropertyDrawer（UI Toolkit）
├── AesirEventArgsSOEditor.cs      # SO 自定义 Inspector（运行时 Raise 按钮）
├── EventDiagnosticsWindowHelper.cs # 诊断窗口 UI helper
├── AesirEventMonitorWindow.cs     # 订阅监视窗口（UI Toolkit）
├── AesirEventLogWindow.cs         # 事件日志窗口（UI Toolkit）
└── AesirEventTesterWindow.cs      # 手动触发窗口（UI Toolkit）
Editor/Integration/OdinInspector/Events/
├── AesirEventMonitorWindowOdin.cs # 订阅监视窗口（Odin，静态委托路由）
├── AesirEventLogWindowOdin.cs     # 事件日志窗口（Odin）
└── AesirEventTesterWindowOdin.cs  # 手动触发窗口（Odin）

Samples/Events/
├── 01_KeyPress/           # 基本发布-订阅（Script 订阅 + 句柄，纯脚本示例）
├── 02_Filters/            # 订阅者过滤器（含 SampleForEventFilters.unity 对照场景：
│   │                      #   Space = WithTag+InsideCollider2D 双重过滤警报，R = OnlySelf 家族命令）
│   └── Scene/
├── 03_SOAsset/            # SO 资产化（ScoreEventAsset.asset 配置事件载荷，
│   │                      #   SampleForEventSOAsset.unity 演示 UnityEventOnAesirEvent 零代码桥接）
│   └── Scene/
└── ...（各示例目录含独立 asmdef，运行时程序集 + 整文件 #if UNITY_EDITOR，构建剔除）
```
