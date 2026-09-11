# 事件模块（Event Module）

> ⚠️ **实验性模块**：尚未在实际项目中验证，API 可能调整。

## 概述

事件模块提供基于双轨订阅的事件系统，实现业务模块间的发布-订阅解耦。

- **Attribute 订阅**：`[AesirListener]` 特性标记方法，`AddListener(obj)` 反射扫描注册
- **Script 订阅**：`AddListener<T>(obj, callback)` 动态注册 Lambda 委托，返回 `AutoRemoveListenerHandle`
- 两种订阅共存于独立注册表，分发时合并并按 4 档优先级排序执行
- **订阅者过滤器**：`WithFilter` 链式声明"只让特定范围的订阅者收到"（精确投递）
- **分发期可靠性**：自动清理已销毁订阅者（死引用）；可选分发耗时告警
- **SO 资产化**：`AesirEventArgsSO` 让事件可保存为 .asset 资源，`UnityEventOnAesirEvent` 桥接 Inspector UnityEvent

> **注意**：`AesirEventArgs` 是事件参数载体（类似 `EventArgs`），本身不持有监听者。订阅管理由 `EventModule` 的双注册表负责。这与 `MiniEvent`（自身持有 `Action` 列表的自包含事件）在设计定位上不同。

## 核心类型

### AesirEventArgs

事件参数抽象基类。

```csharp
[Serializable]
public abstract class AesirEventArgs : ICloneable
{
    public object Sender { get; private set; }                 // 发布者，分发时写入
    public IReadOnlyList<ISubscriberFilter> Filters { get; }  // 过滤器列表（未添加时为 null）

    public AesirEventArgs SetSender(object sender);   // 链式
    public AesirEventArgs WithFilter(ISubscriberFilter filter);            // 链式添加过滤器
    public AesirEventArgs WithFilters(params ISubscriberFilter[] filters); // 链式添加一组过滤器
    public void Invoke();                               // 用 Sender 触发
    public void Invoke(object sender);                 // 用指定 sender 触发
    public virtual object Clone();                     // 浅拷贝
}
```

### AesirListenerAttribute

方法特性，标记该方法监听指定事件参数类型。`AllowMultiple = true`：同一方法可标注多个特性监听多种事件。

```csharp
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class AesirListenerAttribute : Attribute
{
    public Type EventType { get; set; }                       // 显式事件类型（可选）
    public SubscriberPriority Priority { get; set; } = High;  // 优先级

    public AesirListenerAttribute();
    public AesirListenerAttribute(Type eventType);
    public AesirListenerAttribute(SubscriberPriority priority);
    public AesirListenerAttribute(Type eventType, SubscriberPriority priority);
}
```

### EventModule

MonoBehaviour 单例，直接继承 `AesirMonoBehaviour`，作为 `AesirModules` 子物体懒加载创建。

```csharp
// Attribute 订阅
public static void AddListener(object subscriber);
public static void RemoveListener(object subscriber);

// Script 订阅（返回 AutoRemoveListenerHandle）
public static AutoRemoveListenerHandle AddListener<TEventArgs>(object subscriber, Action<TEventArgs> callback);
public static AutoRemoveListenerHandle AddListener<TEventArgs>(object subscriber, Action<TEventArgs> callback, SubscriberPriority priority);
public static AutoRemoveListenerHandle AddListener(object subscriber, AesirEventArgs eventArgs, Action<AesirEventArgs> callback);  // 非泛型版

// 事件触发
public static void InvokeEvent<TEventArgs>(object sender, TEventArgs eventArgs);

// 单例
public static EventModule Instance { get; }

// 可靠性配置（预放置实例可在 Inspector 调整）
// [SerializeField] float executionMsLimit = 0f;  — 分发耗时告警阈值（ms），0 = 关闭
```

### BindingInfo

绑定信息基类，Attribute 与 Script 订阅的共同部分：

```csharp
public abstract class BindingInfo
{
    public string BindingKey { get; protected set; }
    public object Subscriber { get; protected set; }
    public SubscriberPriority Priority { get; protected set; }
    public abstract void Invoke(object[] args = null);
}
```

- **StaticBindingInfo** — 持有 `MethodInfo` + 表达式树编译委托。注册时通过 `Expression.Lambda.Compile()` 将反射方法编译为 `Action<object, object[]>` 委托，分发时零反射调用
- **DynamicBindingInfo\<TEventArgs\>** — 持有 `Action<TEventArgs>` 直接委托，无需表达式树

### SubscriberPriority

```csharp
public enum SubscriberPriority
{
    First,   // 最前 — 比所有默认档位更早
    High,    // 高优先级 — Attribute 订阅默认值
    Medium,  // 中优先级 — Script 订阅默认值
    Last     // 最后 — 收尾/清理
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

### 2. Attribute 订阅

```csharp
using UnityEngine;
using Runestone.AesirModules;

public class ScoreUI : MonoBehaviour
{
    void OnEnable()  => EventModule.AddListener(this);
    void OnDisable() => EventModule.RemoveListener(this);

    [AesirListener]
    private void OnPlayerScored(OnPlayerScored e)
    {
        Debug.Log($"[ScoreUI] {e.playerName} scored {e.points}");
    }
}
```

### 3. Script 订阅

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

### 4. 发布事件

```csharp
// 链式调用
new OnPlayerScored { points = 10, playerName = "Player1" }.Invoke(this);

// 直接调用
EventModule.InvokeEvent(this, new OnPlayerScored { points = 10 });
```

### 5. 指定优先级

```csharp
// Attribute — 构造函数指定
[AesirListener(SubscriberPriority.First)]
private void OnPlayerScored(OnPlayerScored e) { ... }

// Script — 参数指定
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

| 过滤器 | 语义 |
|--------|------|
| `WithTag(tag)` | 仅 Tag 匹配的订阅者收到 |
| `WithPriority(priority)` | 仅绑定在指定优先级档位的订阅者收到 |
| `SameSceneAsEmitter` | 仅与发布者同场景的订阅者收到（适配多场景叠加加载） |
| `OnlySelf` | 仅发布者自身/子树/父级链上的订阅者收到 |
| `InsideCollider2D` | 仅位于发布者 Collider2D 范围内的订阅者收到（空间局域广播） |

自定义过滤器：实现 `ISubscriberFilter.ShouldReceive(AesirEventArgs, object subscriber, SubscriberPriority)` 即可。

> **fail-closed 约定**：过滤器无法解析对象（发布者/订阅者不是 GameObject 或 Component、已销毁）时按"不通过"处理，避免过滤条件失效导致事件意外扩散。过滤器抛出的异常与订阅者回调一样被分发隔离（不影响其他订阅者）。
>
> **生命周期**：过滤器随事件参数实例存在，`Clone()` 共享同一列表；缓存复用的参数实例（如 `AesirEventArgsSO`）会保留已添加的过滤器。

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

> `SubclassSelector`（UI Toolkit PropertyDrawer）对任意 `[SerializeReference]` 字段生效；`ExcludeSubclassSelector` 特性可把不想出现在下拉中的类型排除。运行时修改事件类型需重新启用组件使订阅生效。

## 架构设计

### 双注册表

两种订阅分别存储于独立注册表，分发时合并：

```
AttributeBindings (Dictionary<string, List<BindingInfo>>)
  └─ StaticBindingInfo (MethodInfo + 表达式树委托)

DynamicBindings (Dictionary<string, List<BindingInfo>>)
  └─ DynamicBindingInfo<T> (Action<T> 委托)

RaiseEvent:
  1. 从两个注册表取订阅者列表
  2. 合并（仅在两个注册表都有数据时才创建新 List）
  3. 按优先级排序（count > 1 才排序）
  4. 逐订阅者：死引用检查 → 过滤器检查 → 调用（复用 object[] 参数数组）
  5. 循环外：移除本轮收集的死绑定；超阈值输出耗时告警
```

### 死引用清理

订阅者 GameObject 被 Destroy 后绑定仍残留在注册表（旧版本只跳过不清理，会永久累积）。现在分发循环内把 Unity 假 null 的订阅者收集到复用列表，**循环结束后**从双注册表移除并输出 Warning 提示检查退订遗漏（编辑器内；玩家构建中静默清理）。同一订阅者的事件类型不再被发布时，其死绑定不会清理——按需发布即可，无泄漏累积。

### 表达式树优化

`StaticBindingInfo` 在注册时（`Bind` / `OnEnable`，冷路径）通过 `Expression.Lambda.Compile()` 将 `MethodInfo` 编译为 `Action<object, object[]>` 委托。之后每次分发（热路径）直接委托调用，比 `MethodInfo.Invoke` 快 20-40 倍。

### 性能模型（冷/热路径）

反射只发生在**冷路径**（注册期，一次性成本），**热路径**（分发期）零反射、稳态零分配：

**冷路径（`AddListener` / `Bind`，每次订阅/退订）**：

| 反射操作 | 成本 |
|----------|------|
| `Bind`：`GetMethods` 方法数组 + `GetCustomAttributes` 特性实例化 + 表达式树编译 | ~50µs/订阅者（实测） |
| `StaticBindingInfo`：`Expression.Lambda.Compile()` | ~1-3ms/唯一方法，仅首次 |
| 非泛型 `AddListener` 重载：`Delegate.CreateDelegate` + `MakeGenericMethod` | 每次订阅一次 |
| `RemoveListener`：`Keys.ToList()` 键快照 | 每次退订一次分配 |

**热路径（`RaiseEvent` 分发）**：

| 操作 | 实测成本 |
|------|----------|
| 编译委托调用（vs `MethodInfo.Invoke` ~300ns） | ~4ns/次，加速 ~78 倍 |
| 绑定键查询（Type→string 缓存后；原生 `AssemblyQualifiedName` 拼接每次 ~1µs + 新分配字符串） | ~20ns 字典查询，零分配 |
| 1000 订阅者单次发布（含死引用检查、过滤器检查、排序、调用） | ~571µs |

热路径复用 `object[]` 参数数组、死绑定收集列表与静态 `Stopwatch`；过滤器列表懒分配（未声明过滤器时为 null，零开销）。

**性能回归测试**（`Tests/Editor/EventModuleTests.cs`，防止优化退化）：

- `GetEventBindingKey_Cached_SameStringInstancePerType` — 同类型键复用同一字符串实例（引用同一性断言，锁定热路径零字符串分配）
- `CompiledDelegate_InvokeIsFasterThanReflectionInvoke` — 20 万次迭代对比计时，断言编译委托快于 `MethodInfo.Invoke`

### 性能监控

`executionMsLimit`（毫秒，默认 0 = 关闭）为分发耗时告警阈值。开启后单次分发超过阈值输出 Warning（含事件名、耗时与订阅者数量）。计时复用静态 `Stopwatch`，零稳态分配。分发为同步非重入设计：订阅者回调内再次发布事件不受计时与参数数组复用保护（约定不在回调内同步发布事件）。

### 退订

- `RemoveListener(obj)` — 移除该对象的全部绑定（含 Attribute + Script）
- `AutoRemoveListenerHandle.Dispose()` — 仅移除单条 Script 绑定（重复调用安全）
- 遗漏退订且订阅者被销毁 → 下次分发自动清理（死引用清理兜底）

## 目录结构

```
Runtime/Events/
├── AesirEventArgs.cs              # 事件参数基类（Sender + WithFilter 链式 API）
├── AesirEventArgsSO.cs            # SO 资产包装（CreateAssetMenu + Raise）
├── AesirEventUtility.cs           # 静态工具（绑定键、Unity null 检测、GameObject 解析）
├── AesirListenerAttribute.cs      # 订阅者特性（AllowMultiple）
├── BindingInfo.cs                 # 绑定信息基类 + StaticBindingInfo + DynamicBindingInfo<T>
├── ISubscriberFilter.cs           # 过滤器策略接口
├── SubscriberFilters.cs          # 内建过滤器（WithTag/WithPriority/SameSceneAsEmitter/InsideCollider2D/OnlySelf）
├── SubclassSelectorAttribute.cs   # [SerializeReference] 子类下拉特性 + 排除特性
├── Component/
│   ├── EventModule.cs             # 事件模块单例（死引用清理 + 过滤器检查 + 性能监控）
│   └── UnityEventOnAesirEvent.cs  # UnityEvent 桥接组件
└── SubscriberPriority.cs          # 优先级枚举（4 档：First/High/Medium/Last）

Editor/Events/
├── SubclassSelectorDrawer.cs      # 子类下拉 PropertyDrawer（UI Toolkit）
└── AesirEventArgsSOEditor.cs      # SO 自定义 Inspector（运行时 Raise 按钮）

Samples/Events/
├── 01_KeyPress/           # 基本发布-订阅（[AesirListener] 静态订阅，纯脚本示例）
├── 02_Filters/            # 订阅者过滤器（含 SampleForEventFilters.unity 对照场景：
│   │                      #   Space = WithTag+InsideCollider2D 双重过滤警报，R = OnlySelf 家族命令）
│   └── Scene/
├── 03_SOAsset/            # SO 资产化（ScoreEventAsset.asset 配置事件载荷，
│   │                      #   SampleForEventSOAsset.unity 演示 UnityEventOnAesirEvent 零代码桥接）
│   └── Scene/
└── ...（各示例目录含独立 asmdef，运行时程序集 + 整文件 #if UNITY_EDITOR，构建剔除）
```

## 后续规划

见 [Docs/AesirModules/EventModule/Feature-Roadmap.md](../../../../Docs/AesirModules/EventModule/Feature-Roadmap.md)。

系统事件（OnObjectBound/OnEventRaised 等元事件）与 DefaultChannel 频道标签依赖编辑器工具链（Subscription Monitor / Event Log 等调试窗口），待工具链立项后一并设计。
