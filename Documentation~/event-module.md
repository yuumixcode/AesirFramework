# 事件模块（Event Module）

> 版本：V1（MVP 最小闭环）  
> 状态：已实现

## 概述

事件模块提供基于反射绑定的事件系统，实现业务模块间的发布-订阅解耦。订阅者通过 `[AesirListener]` 特性标记方法，发布者通过 `AesirEventArgs.Invoke()` 触发事件，`EventModule` 单例负责事件分发。

> **注意**：`AesirEventArgs` 是事件参数载体（类似 `EventArgs`），本身不持有监听者。订阅管理由 `EventModule` 的 `BindingRegistry` 负责。这与 `MiniEvent`（自身持有 `Action` 列表的自包含事件）在设计定位上不同。

### 设计目标

- **解耦**：发布者和订阅者不直接引用，通过事件参数类型松耦合
- **静态订阅**：`[AesirListener]` 特性标记即订阅，无需手写注册代码
- **Unity 原生 API 风格**：`AddListener` / `RemoveListener` / `InvokeEvent` 贴合 Unity 习惯
- **安全**：异常不冒泡，已销毁订阅者自动跳过

### V1 范围

| 能力 | V1 | V2+ |
|------|----|-----|
| 静态绑定 `[AesirListener]` | ✅ | — |
| 发布 `Invoke` / `InvokeEvent` | ✅ | — |
| 退订 `RemoveListener` | ✅ | — |
| 2 档优先级（High/Medium） | ✅ 占位 | V2 扩 5 档 |
| 5 阶段分阶段分发 | ❌ | V2 |
| 链式 API（Shared/Unique/Seal） | ❌ | V2 |
| StopPropagation 取消传播 | ❌ | V2 |
| 动态订阅 `Listen<T>` | ❌ | V2 |
| PublishDelayed 延迟发布 | ❌ | V2 |
| 过滤器 `ISubscriberFilter` | ❌ | V3 |
| 系统事件 | ❌ | V4 |
| ScriptableObject 包装 | ❌ | V5 |

## 核心类型

### AesirEventArgs

事件参数抽象基类。所有自定义事件参数继承此类，作为数据载体在 `EventModule` 中传递。

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
    public Type EventID { get; set; }                       // 显式事件类型（可选）
    public SubscriberPriority Priority { get; set; } = High; // 优先级

    public AesirListenerAttribute();
    public AesirListenerAttribute(Type eventID);
}
```

**规则**：
- `EventID` 为 null 时，从方法第一个参数推断事件参数类型
- 零参数方法必须显式指定 `EventID`
- 同一方法可标多个 `[AesirListener]` 监听多个事件类型
- 方法可为 public 或 private

### EventModule

MonoBehaviour 单例，作为 `AesirModules` 子物体懒加载创建。

```csharp
// 静态 API
public static bool IsInitialized { get; }
public static EventModule Instance { get; }

public static void AddListener(object subscriber);
public static void RemoveListener(object subscriber);
public static void InvokeEvent<TEventArgs>(object sender, TEventArgs eventArgs) where TEventArgs : AesirEventArgs;
```

### AbstractAttributeBound\<TAttribute\>

泛型反射绑定基类。`EventModule` 继承此类，通过反射扫描订阅者上标有 `TAttribute` 的方法。

**核心成员**：
- `BindingRegistry` — `Dictionary<string, List<BindingInfo>>`，以事件类型 AssemblyQualifiedName 为键
- `Bind(object)` — 反射扫描并注册
- `Unbind(object)` — 移除订阅者的所有绑定
- `BindingInfo`（嵌套抽象类）— 描述方法与事件类型的绑定关系

### SubscriberPriority

```csharp
public enum SubscriberPriority
{
    High,    // 静态订阅默认
    Medium   // 动态订阅默认（V1 占位）
}
```

### AesirEventUtility

```csharp
public static bool IsObjectUnityNull(object obj);
public static string GetEventBindingKey(AesirEventArgs eventArgs);
public static string GetEventBindingKey<TEventArgs>() where TEventArgs : AesirEventArgs;
public static string GetEventName<TEventArgs>() where TEventArgs : AesirEventArgs;
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

### 2. 订阅事件

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

### 3. 发布事件

```csharp
// 链式调用
new OnPlayerScored { points = 10, playerName = "Player1" }.Invoke(this);

// 直接调用
EventModule.InvokeEvent(this, new OnPlayerScored { points = 10 });
```

### 4. 零参数方法

```csharp
[AesirListener(typeof(OnKeyPressed))]
private void OnKeyPressed()
{
    Debug.Log("按键被按下");
}
```

### 5. 多事件监听

```csharp
[AesirListener]  // 监听 OnPlayerScored（从参数推断）
[AesirListener(typeof(OnGameStart))]  // 监听 OnGameStart（零参数）
private void OnEvent(AesirEventArgs e)
{
    Debug.Log($"收到事件：{e.GetType().Name}");
}
```

## 架构设计

### 单例管理

`EventModule` 作为 `AesirModules` 的子物体存在：

```
[Aesir Modules] (DontDestroyOnLoad)
  └─ EventModule (子物体)
```

- 首次调用 `EventModule.AddListener()` 时，通过 `AesirModules.GetOrAddChild<EventModule>()` 懒加载创建
- `InvokeEvent` 检查 `IsInitialized`，未初始化时 LogError（不自动创建）
- `RemoveListener` 在未初始化时安全跳过

### 事件分发流程

```
发布者 new MyEventArgs().Invoke(this)
  → EventModule.InvokeEvent(sender, eventArgs)
    1. 校验 sender 非 null
    2. 写入 Sender (eventArgs.SetSender)
    3. 查找 BindingRegistry[事件参数类型]
    4. 遍历订阅者：
       - 跳过已销毁的订阅者（Unity null 检测）
       - 调用 binding.Invoke(new[] { eventArgs })
       - try/catch 异常不冒泡，LogError 后继续
```

### BindingKey 规则

使用事件参数类型的 `AssemblyQualifiedName` 作为键，保证跨程序集稳定：

```
OnKeyPressed → "Runestone.AesirModules.OnKeyPressed, Runestone.AesirModules"
```

### 异常处理

- 订阅者方法抛出异常时，`TargetInvocationException` 被捕获并解包
- 异常只 LogError，不中断其他订阅者的调用
- 已销毁的 Unity 对象订阅者被自动跳过（V4 将添加清理和警告）

## 目录结构

```
Runtime/Events/
├── AesirEventArgs.cs              # 事件参数基类
├── AesirListenerAttribute.cs      # 订阅者特性
├── AesirEventUtility.cs           # 静态工具
├── AbstractAttributeBound.cs       # 反射绑定基类
├── EventModule.cs                  # 事件模块单例
└── SubscriberPriority.cs           # 优先级枚举

Samples~/Events/01_KeyPress/
├── OnKeyPressed.cs                 # 示例事件参数
├── EventEmitter.cs                 # 按键发布者
├── KeyPressSubscriber.cs           # 静态订阅者
└── Runestone.AesirModules.Samples.Events.KeyPress.asmdef
```

## 后续版本规划

| 版本 | 核心能力 |
|------|---------|
| V2 | 5 档 Priority、链式 API（Shared/Unique/Seal）、StopPropagation、动态订阅 Listen\<T\>、PublishDelayed、5 阶段分阶段分发 |
| V3 | ISubscriberFilter 过滤器、6 个内置过滤器、DefaultChannel |
| V4 | 4 个 SystemEvent 元事件、EnsureSingleInstance、性能监控、死引用清理 |
| V5 | GameEventSO + SubclassSelector + UnityEvent 桥接 |
| V6 | PublishOnPlayback Animator 集成 |
| V7 | 编辑器工具链（Log/Monitor/Tester/Detail/Actors） |
| V8 | Welcome 引导 + 文档系统 + Hierarchy 菜单 |
