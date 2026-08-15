# RuntimeInitializeOnLoadMethod 与 Domain Reload 安全指南

> **适用版本**：Unity 2022.3 LTS（团结引擎等效版本）
> **文档日期**：2026-08-15
> **文档目的**：阐述 `[RuntimeInitializeOnLoadMethod]` 各时机的语义、编辑器与 Player 构建的行为差异，以及框架静态变量重置的工程实践。

---

## 一、官方文档来源

| 内容 | 链接 |
|------|------|
| 属性主文档（含执行顺序） | https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RuntimeInitializeOnLoadMethodAttribute.html |
| 枚举 `RuntimeInitializeLoadType` | https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RuntimeInitializeLoadType.html |
| 枚举值 `SubsystemRegistration` | https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RuntimeInitializeLoadType.SubsystemRegistration.html |
| 构造函数文档 | https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RuntimeInitializeOnLoadMethodAttribute-ctor.html |
| Unity 用户手册 2022.3 首页 | https://docs.unity3d.com/2022.3/Documentation/Manual/index.html |

> **版本提示**：Unity 6+（6000.x）文档已将 `SubsystemRegistration` 标记为 legacy 并推荐新代码改用 `OnCodeInitializingAttribute`。本项目基于 Unity 2022.3 LTS，仍使用 `SubsystemRegistration`，语义稳定且无废弃风险。

---

## 二、属性概述

`[RuntimeInitializeOnLoadMethod]` 标记一个静态方法，使其在运行时启动时被调用。不依赖任何特定场景或 GameObject，适合执行全局初始化逻辑。

可传入 `RuntimeInitializeLoadType` 枚举参数控制调用时机：

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
static void ResetStatics()
{
    _instance = null;
}
```

若不传参数，默认为 `AfterSceneLoad`。

---

## 三、执行顺序（官方原文翻译）

Player 构建启动时的完整序列：

```
1. 底层系统初始化（窗口、程序集、图形等）
2. SubsystemRegistration 与 AfterAssembliesLoaded 回调被调用
3. 更多初始化（输入系统等）
4. BeforeSplashScreen 回调被调用
5. 第一个场景开始加载
6. BeforeSceneLoad 回调被调用
   — 场景对象已加载到内存，但 Awake() 尚未调用
   — 所有对象在此阶段视为 inactive
7. Awake() 和 OnEnable() 在 MonoBehaviour 上被调用
8. AfterSceneLoad 回调被调用
   — 场景对象完全加载并设置完毕
   — active 对象可用 FindObjectsByType 查找
```

### 各枚举值汇总

| 枚举值 | 调用时机 | 场景对象 | Awake 已调用 |
|--------|---------|---------|-------------|
| `SubsystemRegistration` | 运行时启动时，**最早的回调点** | 未加载 | 否 |
| `AfterAssembliesLoaded` | 所有程序集加载完成、预加载资源初始化后 | 未加载 | 否 |
| `BeforeSplashScreen` | 启动画面显示之前 | 未加载 | 否 |
| `BeforeSceneLoad` | 场景对象已加载到内存 | 已加载 | 否 |
| `AfterSceneLoad` | **默认值**。场景加载且 Awake 之后 | 已加载 | 是 |

> 同一 `RuntimeInitializeLoadType` 下注册的多个方法，**执行顺序不保证固定**。

---

## 四、编辑器中的行为

### 官方原文

> *"Specifies when to get a callback during the startup of the runtime **or when entering play mode in the Editor**."*
> （指定在运行时启动**或进入编辑器 Play 模式**时获取回调的时机。）

> *"The above details are when starting up a Player build. **When entering Play mode in the Editor the same invocations are ensured.**"*
> （以上是 Player 构建启动时的细节。**进入编辑器 Play 模式时同样会触发这些调用**。）

### 含义

- 编辑器中点击 **Play** 按钮时，上述 5 个回调全部照常触发，执行顺序与 Player 构建一致。
- **无论 Domain Reload 是否开启**，`[RuntimeInitializeOnLoadMethod]` 都会在进入 Play 模式时执行。

---

## 五、Domain Reload 与静态变量重置

### 背景

Unity 默认在进入 Play 模式时执行 **Domain Reload**（域重载）：卸载当前程序集域、重新加载，所有静态字段被清零。

但项目可以关闭 Domain Reload（`Project Settings → Editor → Enter Play Mode Options → Reload Domain` 取消勾选）以加快进入 Play 模式的速度。此时：

- **静态字段不会被运行时自动清零**，上一轮 Play 模式的残留数据会保留。
- 若不在切入点手动重置，会导致：
  - 单例引用指向已销毁的 `UnityEngine.Object`（Unity 空引用）
  - 事件订阅列表残留已失效的委托
  - 计数器、标志位等跨会话累积

### 推荐的重置时机

**`SubsystemRegistration`** 是重置静态变量的推荐时机，理由：

1. **最早执行**：在任何场景加载、`Awake()`、`OnEnable()` 之前，确保后续代码拿到的是干净状态。
2. **每次进入 Play 模式都触发**：无论 Domain Reload 开关状态，编辑器与 Player 构建一致。
3. **语义匹配**：`SubsystemRegistration` 的语义即"子系统注册阶段"，静态状态重置属于子系统初始化的一部分。

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
static void ResetStatics()
{
    _instance = null;
    _eventListeners = null;
}
```

### 为什么不用其他时机

| 时机 | 不推荐原因 |
|------|-----------|
| `AfterAssembliesLoaded` | 语义偏"程序集加载完成后的资源初始化"，非状态重置；晚于 `SubsystemRegistration`，无收益 |
| `BeforeSceneLoad` | 场景已加载、`Awake` 即将调用——太晚，若单例在 `Awake` 中被访问则已残留旧值 |
| `AfterSceneLoad` | 默认值但更晚，`Awake` 与 `OnEnable` 已执行完毕，重置无意义 |

---

## 六、泛型类的陷阱

### 问题

Unity 2022.3 中，**泛型类中声明的 `[RuntimeInitializeOnLoadMethod]` 静态方法会被静默跳过**——不执行、不报错、不警告。

```csharp
// ❌ 不会被 Unity 调用
public abstract class AbstractContext<T> where T : AbstractContext<T>, new()
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()  // 泛型类中 → 静默失效
    {
        _instance = null;
    }
}
```

### 原因

Unity 的 `[RuntimeInitializeOnLoadMethod]` 扫描机制在构建时通过反射收集标记方法。泛型类（尤其是开放式泛型 `AbstractContext<>`）的静态方法不在扫描结果中——Unity 只为**封闭式泛型**（如 `AbstractContext<MyContext>`）或**非泛型类**生成静态构造触发。

### 解决方案

将重置逻辑放到**非泛型类**中：

```csharp
// ✅ 非泛型类 → Unity 正常调用
internal static class ContextSingletonStore
{
    static readonly Dictionary<Type, IContext> Instances = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Instances.Clear();
    }
}
```

---

## 七、框架中的实践

### 非泛型 MonoBehaviour 单例

直接在类内声明 `[RuntimeInitializeOnLoadMethod]` 重置方法：

```csharp
// MonoLifecycleProxy.cs
[DisallowMultipleComponent]
public sealed class MonoLifecycleProxy : AesirMonoBehaviour
{
    static MonoLifecycleProxy _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        _instance?.ClearAllListeners();
        _instance = null;
    }
}
```

### 泛型 C# 单例（AbstractContext&lt;T&gt;）

泛型类无法自身声明有效的 RIOLM，经助手注册重置回调：

```csharp
// AbstractContext.cs
public abstract class AbstractContext<T> : IContext where T : AbstractContext<T>, new()
{
    static T _instance;

    static AbstractContext()
    {
        // 静态构造函数在首次访问该封闭泛型类型时执行
        // 通过助手注册 _instance = null 回调
        ResetStaticsAssistant.Register(() => _instance = null);
    }
}
```

`ResetStaticsAssistant` 本身是非泛型类，其 `[RuntimeInitializeOnLoadMethod]` 正常生效：

```csharp
// ResetStaticsAssistant.cs
public static class ResetStaticsAssistant
{
    static readonly List<Action> Callbacks = new();

    public static void Register(Action callback) => Callbacks.Add(callback);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetAll()
    {
        foreach (var cb in Callbacks)
            cb?.Invoke();
    }
}
```

### PlayerLoop 注入

`AesirArchitecturePlayerLoop` 同样使用 `SubsystemRegistration` 时机注入 PlayerLoop 子系统与重置钩子状态：

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
static void Initialize()
{
    Reset();
    EnsureInjected();
}
```

---

## 八、验证清单

| 检查项 | 验证方法 |
|--------|---------|
| 关闭 Domain Reload 后单例不残留 | Project Settings → 关闭 Reload Domain → 反复进出 Play 模式 → 检查单例引用不指向已销毁对象 |
| 事件订阅列表不残留 | 退出 Play 模式后检查 `GetListeners().Length` 归零 |
| 泛型类 RIOLM 静默失效 | 在泛型类 RIOLM 方法内 `Debug.Log` → 运行后确认控制台无输出 |
| PlayerLoop 注入自愈 | 调用 `AesirArchitecturePlayerLoop.EnsureInjected()` 后 `ContainsSystem` 返回 true |
| EditMode 测试同域重跑稳定 | 测试夹具 `SetUp` 中调用 `ResetStaticsAssistant.ResetForTests()` 重置单例 |

---

## 九、参考

- Unity 2022.3 Scripting API — `RuntimeInitializeOnLoadMethodAttribute`
  https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RuntimeInitializeOnLoadMethodAttribute.html
- Unity 2022.3 Scripting API — `RuntimeInitializeLoadType`
  https://docs.unity3d.com/2022.3/Documentation/ScriptReference/RuntimeInitializeLoadType.html
- Unity 2022.3 User Manual — Domain Reloading
  https://docs.unity3d.com/2022.3/Documentation/Manual/domain-reloading.html
- 极简化分析与改进计划 — `Docs/AesirArchitecture-极简分析与改进计划.md`
