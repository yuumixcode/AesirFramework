---
title: AesirPlayerLoop
description: "Runestone.AesirArchitecture.AesirPlayerLoop 的 API 文档"
---

# `AesirPlayerLoop`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `AesirPlayerLoop`

## 声明

``` csharp
public static class AesirPlayerLoop
```

基于 PlayerLoop 的生命周期钩子系统，无需 MonoBehaviour 即可接入游戏级帧回调。
通过 Register 注册回调，order 越小越先执行；系统自动在域加载时注入 PlayerLoop。

注入自愈：PlayerLoop 注入可能被第三方 SDK 用其缓存的副本调用 PlayerLoop.SetPlayerLoop 覆盖， 导致钩子静默失效。框架通过 EnsureInjected 自愈：域加载时与每次 Register 时 检测并补插缺失的注入点（注册即自愈，检测每帧至多一次）；用户也可手动调用。

**备注**

待处理命令机制：在遍历回调执行期间，如果有 Register 或 Unregister 调用， 不会直接修改回调集合（否则会抛出 InvalidOperationException）， 而是将操作缓存到 PendingCommands 列表中，待当前遍历结束后统一执行。

稳定排序机制：回调列表使用 Order 字段进行优先级排序，Order 越小越先执行。 当多个回调的 Order 相同时，使用 InsertionIndex（插入顺序自增序号）作为次级排序键， 确保相同优先级的回调按注册顺序执行，排序结果稳定可预期。

域加载安全：通过 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] 在 Unity 的子系统注册阶段自动注入 PlayerLoop，该阶段早于场景加载和脚本初始化， 确保在 Disable Domain Reload 模式下也能正确重建钩子系统。

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Register(AesirLifecyclePhase, Action, int)`](#method-register-aesirlifecyclephase-action-int) | 注册回调，order 越小越先执行，默认 0。 返回 AutoRemoveListenerHandle，Dispose 时自动注销本次注册，与全框架监听句柄风格一致。 忽略返回值的调用方须在持有者销毁前手动调用 Unregister 注销——匿名委托无法经 Unregister 定位注销，只能依赖返回的句柄；若均未注销，回调将永久残留并阻止目标对象被回收。 |
| [`EnsureInjected()`](#method-ensureinjected) | 确保两个注入点存在于当前 PlayerLoop。已存在时为空操作，缺失时重新注入。 |
| [`Unregister(AesirLifecyclePhase, Action)`](#method-unregister-aesirlifecyclephase-action) | 注销回调。 必须传入注册时的同一委托实例，匿名函数无法通过此方法注销。 |

</div>

**继承的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

### Register(AesirLifecyclePhase, Action, int) {#method-register-aesirlifecyclephase-action-int}

注册回调，order 越小越先执行，默认 0。
返回 AutoRemoveListenerHandle，Dispose 时自动注销本次注册，与全框架监听句柄风格一致。 忽略返回值的调用方须在持有者销毁前手动调用 Unregister 注销——匿名委托无法经 Unregister 定位注销，只能依赖返回的句柄；若均未注销，回调将永久残留并阻止目标对象被回收。

``` csharp
public static AutoRemoveListenerHandle Register(AesirLifecyclePhase phase, Action callback, int order = 0)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `phase` | `AesirLifecyclePhase` | 目标生命周期阶段，决定回调在哪一帧阶段执行 |
| `callback` | `Action` | 每帧执行的回调委托，必须为非空委托实例 |
| `order` | `int` | 执行优先级，值越小越先执行；同 order 时按注册顺序执行 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AutoRemoveListenerHandle` | 自动注销句柄，Dispose 时注销本次注册（与手动 Unregister 等效，重复调用安全） |

</div>

### EnsureInjected() {#method-ensureinjected}

确保两个注入点存在于当前 PlayerLoop。已存在时为空操作，缺失时重新注入。

**备注**

PlayerLoop 注入的自愈入口，幂等可重复调用。第三方 SDK 若使用其缓存的 PlayerLoop 副本调用 PlayerLoop.SetPlayerLoop，会连同框架注入的两个子系统一起抹掉， 导致 BeforeUpdate / AfterUpdate 钩子静默失效。 此方法通过 ContainsSystem{TTarget} 检测后仅补插缺失的子系统， 并保留当前 PlayerLoop 中第三方已有的其他修改。调用时机： Initialize 在域加载时调用（无条件检测）； Register 每帧至多调用一次（同一帧内的重复注册复用检测结果）； 用户在已知第三方 SDK 修改 PlayerLoop 后也可手动调用（无条件检测）。
手动调用本方法不受 Register 的每帧限流约束，可在同一帧内立即触发一次完整检测。

为什么要限流：本方法内两次 ContainsSystem<T> 各自调用一次 PlayerLoop.GetCurrentPlayerLoop()，会把整棵 PlayerLoop 树从原生侧完整 marshall 到托管对象 （每层一次 PlayerLoopSystem[] 分配）。注册是启动期冷路径，若逐次自愈，N 个注册方就是 2N 次全树拷贝； 且稳态（每帧新增注册的运行期场景）会因每次注册都做整树封送而无法守住"零分配"承诺 （限流消除的正是这项自愈检测成本，Register 返回句柄时的闭包分配不在其列）。 故 Register 经 EnsureInjectedIfStaleThisFrame 以 Time.frameCount 限流： 同一帧内只有第一次注册付检测成本，跨帧的第一次注册必定重新检测——第三方 SDK 覆盖 PlayerLoop 后， 最迟下一帧的注册即完成自愈，不会留下永久失效的窗口。

``` csharp
public static void EnsureInjected()
```

### Unregister(AesirLifecyclePhase, Action) {#method-unregister-aesirlifecyclephase-action}

注销回调。
必须传入注册时的同一委托实例，匿名函数无法通过此方法注销。

**备注**

若在回调遍历期间调用此方法，注销操作不会立即执行，而是被缓存到待处理命令列表中， 待当前阶段所有回调遍历结束后才统一执行，以避免遍历期间修改集合导致异常。

``` csharp
public static void Unregister(AesirLifecyclePhase phase, Action callback)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `phase` | `AesirLifecyclePhase` | 目标生命周期阶段 |
| `callback` | `Action` | 要注销的回调委托，必须与注册时传入的实例相同 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
