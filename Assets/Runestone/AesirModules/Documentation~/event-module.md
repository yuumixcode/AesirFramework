# 事件模块（Event Module）

> ⚠️ **实验性模块**：尚未在实际项目中验证，API 可能调整。

## 概述

事件模块提供基于双轨订阅的事件系统，实现业务模块间的发布-订阅解耦。

- **Attribute 订阅**：`[AesirListener]` 特性标记方法，`AddListener(obj)` 反射扫描注册
- **Script 订阅**：`AddListener<T>(obj, callback)` 动态注册 Lambda 委托，返回 `AutoRemoveListenerHandle`
- 两种订阅共存于独立注册表，分发时合并并按 5 档优先级排序执行

> **注意**：`AesirEventArgs` 是事件参数载体（类似 `EventArgs`），本身不持有监听者。订阅管理由 `EventModule` 的双注册表负责。这与 `MiniEvent`（自身持有 `Action` 列表的自包含事件）在设计定位上不同。

## 核心类型

### AesirEventArgs

事件参数抽象基类。

```csharp
[Serializable]
public abstract class AesirEventArgs : ICloneable
{
    public object Sender { get; private set; }

    public AesirEventArgs SetSender(object sender);  // 链式
    public void Invoke();                               // 用 Sender 触发
    public void Invoke(object sender);                 // 用指定 sender 触发
    public virtual object Clone();                     // 浅拷贝
}
```

### AesirListenerAttribute

方法特性，标记该方法监听指定事件参数类型。

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

// 事件触发
public static void InvokeEvent<TEventArgs>(object sender, TEventArgs eventArgs);

// 单例
public static EventModule Instance { get; }
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
    First,   // 前 — 比所有默认档位更早
    High,    // Attribute 订阅默认值
    Medium,  // Script 订阅默认值
    Low,     // 后 — 比所有默认档位更晚
    Last     // 最后 — 收尾/清理
}
```

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
  4. 依次调用（复用 object[] 参数数组）
```

### 表达式树优化

`StaticBindingInfo` 在注册时（`Bind` / `OnEnable`，冷路径）通过 `Expression.Lambda.Compile()` 将 `MethodInfo` 编译为 `Action<object, object[]>` 委托。之后每次分发（热路径）直接委托调用，比 `MethodInfo.Invoke` 快 20-40 倍。

### 退订

- `RemoveListener(obj)` — 移除该对象的全部绑定（含 Attribute + Script）
- `AutoRemoveListenerHandle.Dispose()` — 仅移除单条 Script 绑定（重复调用安全）

## 目录结构

```
Runtime/Events/
├── AesirEventArgs.cs              # 事件参数基类
├── AesirListenerAttribute.cs      # 订阅者特性
├── AesirEventUtility.cs           # 静态工具
├── BindingInfo.cs                 # 绑定信息基类 + StaticBindingInfo + DynamicBindingInfo<T>
├── Component/
│   └── EventModule.cs             # 事件模块单例
└── SubscriberPriority.cs          # 优先级枚举（5 档）

Samples~/Events/01_KeyPress/
├── OnKeyPressed.cs                # 示例事件参数
├── EventEmitter.cs                # 按键发布者
├── KeyPressSubscriber.cs          # 静态订阅者
└── Runestone.AesirModules.Samples.Events.KeyPress.asmdef
```

## 后续规划

见 [Docs/EventModule/Feature-Roadmap.md](../../../../../Docs/EventModule/Feature-Roadmap.md)。
