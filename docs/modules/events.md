# 事件模块

事件模块提供基于 **Script 订阅**的发布-订阅系统,用于业务模块间解耦:

- **Script 订阅** —— `AddListener<T>(obj, callback)` 注册委托,返回 `AutoRemoveListenerHandle` 精确退订;这是**唯一**的订阅形态(曾提供的 `[AesirListener]` 特性反射订阅已移除)
- 订阅按 **4 档优先级**稳定执行(同档按注册顺序);顺序在注册期维护,分发零排序
- **订阅者过滤器** —— `WithFilter` 链式声明"只让特定范围的订阅者收到"(精确投递)
- **取消传播** —— `Cancel(this)` 中止本次分发(First / Last 档不受影响),`NonCancellable()` 免疫取消
- **分发期可靠性** —— 快照迭代(回调内退订/注册不影响本趟分发)、跨实例重入安全(同一实例重入被拒绝)、已销毁订阅者自动清理(死引用)+ 可选分发耗时告警
- **SO 资产化** —— `AesirEventArgsSO` 让事件可保存为 .asset 资源,配合 `UnityEventOnAesirEvent` 桥接组件,非程序员可在 Inspector 配置事件
- **可观测性** —— 公开诊断门面 `AesirEventDiagnostics`(订阅树查询 + 链路快照)+ Monitor / Log / Tester 三个调试窗口,零 Odin 依赖

!!! note "与 MiniEvent 的定位差异"
    `AesirEventArgs` 只是**事件参数载体**(类似 `EventArgs`),本身不持有监听者;订阅管理由 `EventModule` 的注册表负责。这与 `MiniEvent`(自身持有监听列表的自包含事件)在设计定位上不同 —— 事件参数载体命名 `XxxEventArgs`,自包含事件才命名 `XxxEvent`。

## 使用方式

### 1. 定义事件参数

```csharp
using Runestone.AesirModules;

public class OnPlayerScored : AesirEventArgs
{
    public int points;
    public string playerName;
}
```

### 2. Script 订阅(唯一订阅形态)

```csharp
AutoRemoveListenerHandle _handle;

void OnEnable() =>
    _handle = EventModule.AddListener<OnPlayerScored>(this, e => Debug.Log(e.points));

void OnDisable() => _handle.Dispose();   // 重复调用安全
```

一个方法监听多种事件 = 多条 `AddListener<T>`,各自持有句柄。退订方拿不到句柄时(诊断工具强制解绑、外部临时屏蔽):

```csharp
EventModule.RemoveListener<OnPlayerScored>(subscriber);  // 精确退该对象某事件类型(O(1) 查表)
EventModule.RemoveListener(subscriber);                  // 批量退该对象全部订阅(O(n) 冷路径)
```

### 3. 发布事件

```csharp
// 链式(sender 即发布者)
new OnPlayerScored { points = 10, playerName = "Player1" }.Invoke(this);

// 或直接调用
EventModule.InvokeEvent(this, new OnPlayerScored { points = 10 });
```

### 4. 指定优先级

```csharp
EventModule.AddListener<OnPlayerScored>(this, e => { ... }, SubscriberPriority.First);
EventModule.AddListener<OnPlayerScored>(this, e => { ... }, SubscriberPriority.Last);
```

### 5. 订阅者过滤器(精确投递)

发布时经 `WithFilter` 链式声明过滤器,分发时逐订阅者检查,**全部通过才投递**:

```csharp
// 只有 Tag 为 "Enemy" 的订阅者收到
new OnExplosion().WithFilter(new WithTag("Enemy")).Invoke(this);

// 组合过滤:爆炸半径内的敌人(两个过滤器都须通过)
new OnExplosion()
    .WithFilter(new InsideCollider2D())
    .WithFilter(new WithTag("Enemy"))
    .Invoke(this);

// 多场景叠加:只通知与发布者同场景的订阅者
new OnLevelLoaded().WithFilter(new SameSceneAsEmitter()).Invoke(this);

// 家族命令:只通知自身 / 子树 / 父级链上的订阅者
new OnChildActivated().WithFilter(new OnlySelf()).Invoke(this);
```

内建过滤器:

| 过滤器 | 语义 |
|--------|------|
| `WithTag(tag)` | 仅 Tag 匹配的订阅者收到(tag 须非 null 非空串,构造期 fail-fast 抛出) |
| `WithPriority(priority)` | 仅绑定在指定优先级档位的订阅者收到 |
| `SameSceneAsEmitter` | 仅与发布者同场景的订阅者收到(适配多场景叠加加载) |
| `OnlySelf` | 仅发布者自身 / 子树 / 父级链上的订阅者收到 |
| `InsideCollider2D` | 仅位于发布者 Collider2D 范围内的订阅者收到(空间局域广播) |

自定义过滤器实现 `ISubscriberFilter.ShouldReceive(args, subscriber, priority)` 即可。

!!! note "fail-closed 约定"
    过滤器无法解析对象(发布者 / 订阅者不是 GameObject 或 Component、已销毁)时按"不通过"处理,避免过滤条件失效导致事件意外扩散。

### 6. 取消传播

订阅者可在回调内调用 `e.Cancel(this)` 中止本次分发:`First` / `Last` 两档**始终收到**(前置校验 / 收尾清理),`High` / `Medium` 档取消之后的订阅者跳过;`NonCancellable()` 让事件免疫取消(结算、存档等必须送达的场景)。每趟分发自动复位取消状态,缓存复用的 SO 参数实例不会带入上一趟痕迹。典型用途:输入吞噬、伤害减免链、互斥 UI 抢先响应。

```csharp
void OnBeforeClose(OnPanelClose e)
{
    if (!CanClose) e.Cancel(this);   // 阻止后续 High/Medium 档订阅者关闭面板
}
```

### 7. SO 资产化(Inspector 配置事件)

**`AesirEventArgsSO`** —— 事件参数的 ScriptableObject 包装:Project 右键 `Create → Aesir → Event Module → AesirEventArgsSO` 创建资产,Inspector 中经 `SubclassSelector` 下拉选择事件参数子类并配置载荷,运行时点击「触发事件(Raise)」按钮或代码调用 `Raise()`(发布者为资产本身)。

**`UnityEventOnAesirEvent`** —— UnityEvent 桥接组件:挂载后经下拉选择监听的事件类型,在 On Raised 中绑定任意 UnityEvent 回调(播放音效、激活物体等),非程序员零代码串联事件。

```csharp
// 代码触发(发布者 = SO 资产本身)
scoredEventAsset.Raise();
```

> `SubclassSelector` 对任意 `[SerializeReference]` 字段生效;`ExcludeSubclassSelector` 可把不想出现在下拉中的类型排除。

## SubscriberPriority —— 4 档优先级

| 档位 | 执行时机 | 默认归属 |
|------|---------|---------|
| `First` | 比所有默认档位更早,常规处理前运行;不受取消影响 | — |
| `High` | 高优先级(需显式指定) | — |
| `Medium` | 中优先级 | **Script 订阅默认值** |
| `Last` | 所有订阅者之后,收尾 / 清理;不受取消影响 | — |

## 架构设计

### 单注册表(Type 键 + 注册期有序)

注册表是 `internal` 实现细节:订阅 / 退订必须经 EventModule 公开 API(走同一套注册与死引用清理),包外代码不可直接读取(包内测试经 `InternalsVisibleTo` 访问)。绑定信息(`BindingInfo` / `ScriptBindingInfo<T>`)同为 internal,诊断展示所需信息经 `AesirEventDiagnostics` 快照公开。

```
Bindings (Dictionary<Type, List<BindingInfo>>)   // internal,注册期有序
  └─ ScriptBindingInfo<T> — Action<T> 直接委托

分发流程:
  1. 同实例重入守卫(分发中的实例再次发布 → 报错拒绝)
  2. 按事件类型 O(1) 取订阅者列表(已有序,无需排序)
  3. 拷贝进迭代快照(顶层复用迭代缓冲区;重入层用按深度复用的独立缓冲区)
  4. 逐订阅者:死引用检查 → 取消检查 → 过滤器检查 → 委托直调
  5. 循环外:移除本轮收集的死绑定;超过阈值输出耗时告警(仅顶层分发计时)
```

### 快照迭代与重入约定

分发循环基于注册表快照执行,两个语义由此保证:

- **回调内退订/注册不影响本趟分发** —— 订阅者回调内 `RemoveListener` / 句柄 `Dispose` / 新增订阅只修改注册表本身,本趟迭代仍按快照执行完毕(退订的订阅者本趟仍会收到,新增的从下趟开始收到),不会出现"退订后续订阅者导致跳过一个"的索引位移。
- **回调内可安全同步发布其他参数实例(跨实例重入)** —— 同型或异型均可,重入层使用按深度复用的独立迭代缓冲区与死绑定收集,外层循环继续时剩余订阅者仍收到正确的外层参数;缓冲区池随最大嵌套深度增长一次,之后稳态零分配。

!!! warning "分发约定(低概率场景用约定杜绝,不做防御兜底)"
    - **同一参数实例在分发期间不得被再次发布** —— 会输出错误日志并拒绝(fail-fast)。事件链请使用新的参数实例,或经 `AesirTimer.NextFrame` 延后一帧发布。
    - `SetSender` 是发布者侧 API,订阅者不应在分发期间调用。
    - 分发中途增删 / 替换发布者的 `Collider2D` 不被支持(空间过滤器的解析结果按趟缓存)。

### 死引用清理

订阅者 GameObject 被 Destroy 后绑定仍会残留在注册表。分发循环内把 Unity 假 null 的订阅者收集到复用列表,**循环结束后**从注册表移除,并在编辑器输出 Warning 提示检查退订遗漏(玩家构建中静默清理)。该事件类型不再被发布时其死绑定不会清理 —— 按需发布即无泄漏累积。

### 性能模型(冷 / 热路径)

**热路径**(分发期)零反射、零排序、零字符串分配、零装箱:

| 路径 | 操作 | 成本量级 |
|------|------|---------|
| 冷 | 注册(有序插入) | 同档追加 O(1);跨档插入 O(n) 移位 |
| 热 | 委托直调(vs `MethodInfo.Invoke` ~300ns) | ~4ns/次 |
| 热 | Type 键字典查询(`RuntimeTypeHandle` 哈希) | O(1),零分配 |

- 注册表以 `Type` 为键,无字符串键拼接与缓存;顺序在注册期维护,分发侧零排序零比较;
- 委托直调收 `AesirEventArgs`,无 `object[]` 参数数组与类型模式检查;
- 顶层分发复用迭代缓冲区与死绑定收集列表,重入层用按深度复用的缓冲区池(Clear 保留容量,稳态零分配);
- 空间过滤器的发布者 GameObject / Collider2D 按趟解析一次、N 个订阅者共享;
- 无实例时走负缓存快速路径(每个"实例缺失期"至多一次全场景查找);
- 过滤器列表懒分配(未声明过滤器时为 null,零开销);诊断关闭时只多一次静态 bool 读取。

性能特征由 EditMode 回归测试锁定(乱序注册全序执行 + 千订阅者量级软门槛)。

### 性能监控

`executionMsLimit`(毫秒,默认 0 = 关闭)为单次分发耗时告警阈值,超过输出 Warning(含事件名、耗时与订阅者数量)。**仅顶层分发计时** —— 重入层不计时,避免嵌套分发的 Restart/Stop 互相破坏计时。

## 可观测性与调试

`AesirEventDiagnostics` 是全部监控数据的**公开门面**,运行时零 Odin 依赖 —— 未安装 Odin 的开发者可直接用它自建监视面板:

```csharp
// 订阅树查询
AesirEventSubscriptionSnapshot[] subs = AesirEventDiagnostics.GetSubscriptions();
AesirSubscriberSnapshot[] listeners = AesirEventDiagnostics.GetSubscribers(typeof(OnPlayerScored));

// 链路快照:实时推送 + 环形缓冲回看(时间戳为 UTC,展示时 ToLocalTime)
AesirEventDiagnostics.TraceProduced += snap => { /* ... */ };
AesirEventTraceSnapshot[] recent = AesirEventDiagnostics.GetRecentTraces(max: 50);
```

采集默认在编辑器与 Development Build 开启、正式包关闭;参与者名称首次采集时弱表缓存(运行期改名不反映)。内置三个调试窗口(`Tools → Aesir → Modules`):**Event Monitor**(订阅树)、**Event Log**(触发 / 绑定 / 解绑链路)、**Event Tester**(手动触发,仅 Play 模式);UI Toolkit 版零 Odin,装 Odin 且开启 Aesir Odin 样式时自动路由到 Odin 版,功能等价。

## 示例

Package Manager → Aesir Modules → Samples 导入:

| 示例 | 说明 |
|------|------|
| `Events/01_KeyPress` | 基本发布-订阅:按键发布事件、Script 订阅 + 句柄退订 |
| `Events/02_Filters` | 订阅者过滤器对照:Space 发布 `WithTag`+`InsideCollider2D` 双重过滤警报、R 发布 `OnlySelf` 家族命令,场景内置圈内 / 圈外 / 无标签多组对照 |
| `Events/03_SOAsset` | SO 资产化:`ScoreEventAsset.asset` 配置事件载荷,`UnityEventOnAesirEvent` 在 Inspector 零代码串联 UnityEvent 回调 |
| `Events/04_Cancellation` | 取消传播:四档优先级订阅同一伤害事件,Space 观察执行链,Shift 对照不可取消事件 |
| `Events/05_Diagnostics` | 诊断 API:订阅 `TraceProduced` 实时展示链路快照 |

## 继续阅读

- [特性一览](features.md) —— 五模块速览
- [响应式与事件](../architecture/observable.md) —— MiniEvent / ObservableValue 决策表