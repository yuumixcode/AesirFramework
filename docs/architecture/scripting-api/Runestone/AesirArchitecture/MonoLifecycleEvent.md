---
title: MonoLifecycleEvent
description: "Runestone.AesirArchitecture.MonoLifecycleEvent 的 API 文档"
---

# `MonoLifecycleEvent`

!!! note ""

    - **种类:** `enum`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `System.ValueType` → `System.Enum` → `MonoLifecycleEvent`

**实现接口:** `System.IFormattable`，`System.IComparable`，`System.IConvertible`

## 声明

``` csharp
public enum MonoLifecycleEvent : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible
```

Mono 生命周期事件类型，涵盖 Unity 原生生命周期回调和自定义 PlayerLoop 阶段。

**备注**

枚举值按 Unity 执行顺序排列，订阅者可监听任意阶段的事件。
不包含 Awake / OnEnable / OnDisable / OnDestroy / Start 事件——因为 MonoLifecycleProxy 是挂载在 DontDestroyOnLoad GameObject 上的懒创建单例， 这些回调仅在代理自身创建或应用退出时触发，外部无法有效订阅。

BeforeUpdate 和 AfterUpdate 由 AesirPlayerLoop 驱动，分别对应每帧 Update 之前和 PostLateUpdate 之后。 其余事件由 MonoLifecycleProxy 在对应 Unity 回调中直接触发。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AfterUpdate`](#field-afterupdate) | 自定义 PlayerLoop 阶段：在 PostLateUpdate 之后执行 |
| [`BeforeUpdate`](#field-beforeupdate) | 自定义 PlayerLoop 阶段：在 Update 之前执行 |
| [`FixedUpdate`](#field-fixedupdate) | MonoBehaviour.FixedUpdate — 物理帧 |
| [`LateUpdate`](#field-lateupdate) | MonoBehaviour.LateUpdate — 每帧后处理 |
| [`OnApplicationFocus`](#field-onapplicationfocus) | MonoBehaviour.OnApplicationFocus — 应用获得或失去焦点 |
| [`OnApplicationPause`](#field-onapplicationpause) | MonoBehaviour.OnApplicationPause — 应用被系统暂停或恢复 |
| [`OnApplicationQuit`](#field-onapplicationquit) | MonoBehaviour.OnApplicationQuit — 应用退出 |
| [`Update`](#field-update) | MonoBehaviour.Update — 每帧逻辑更新 |

</div>

### AfterUpdate {#field-afterupdate}

自定义 PlayerLoop 阶段：在 PostLateUpdate 之后执行

**备注**

由 AesirPlayerLoop 的 AfterUpdate 阶段驱动。
常见场景：帧结束状态快照、性能采样、延迟队列执行、读取当前帧所有模块的最终状态。

``` csharp
public const MonoLifecycleEvent AfterUpdate;
```

### BeforeUpdate {#field-beforeupdate}

自定义 PlayerLoop 阶段：在 Update 之前执行

**备注**

由 AesirPlayerLoop 的 BeforeUpdate 阶段驱动。
常见场景：输入采样、帧前状态快照、在所有 Update 逻辑之前执行的高优先级预处理。

``` csharp
public const MonoLifecycleEvent BeforeUpdate;
```

### FixedUpdate {#field-fixedupdate}

MonoBehaviour.FixedUpdate — 物理帧

**备注**

常见场景：Rigidbody 位移、物理射线检测累积、固定时间步长的力学计算。

``` csharp
public const MonoLifecycleEvent FixedUpdate;
```

### LateUpdate {#field-lateupdate}

MonoBehaviour.LateUpdate — 每帧后处理

**备注**

常见场景：相机跟随目标、动画后处理、在所有 Update 完成后读取最终位置。

``` csharp
public const MonoLifecycleEvent LateUpdate;
```

### OnApplicationFocus {#field-onapplicationfocus}

MonoBehaviour.OnApplicationFocus — 应用获得或失去焦点

**备注**

常见场景：失去焦点时暂停音频和动画、获得焦点时恢复；桌面端窗口最小化时降低渲染频率。
回调中可通过 Application.isFocused 判断当前焦点状态。

``` csharp
public const MonoLifecycleEvent OnApplicationFocus;
```

### OnApplicationPause {#field-onapplicationpause}

MonoBehaviour.OnApplicationPause — 应用被系统暂停或恢复

**备注**

常见场景：移动端切后台时保存游戏进度、暂停网络请求并断开服务器连接、恢复时重新登录。
回调中可通过 Application.isPaused 判断当前暂停状态。

``` csharp
public const MonoLifecycleEvent OnApplicationPause;
```

### OnApplicationQuit {#field-onapplicationquit}

MonoBehaviour.OnApplicationQuit — 应用退出

**备注**

常见场景：保存游戏进度到本地、断开服务器连接、释放非托管资源、写入日志。
仅在编辑器中退出 Play Mode 或独立构建应用退出时触发一次。

``` csharp
public const MonoLifecycleEvent OnApplicationQuit;
```

### Update {#field-update}

MonoBehaviour.Update — 每帧逻辑更新

**备注**

常见场景：游戏逻辑更新、输入处理、状态机推进、计时器递减。

``` csharp
public const MonoLifecycleEvent Update;
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `HasFlag(Enum)` | — | `Enum` |
| `Equals(object)` | — | `Enum` |
| `GetHashCode()` | — | `Enum` |
| `ToString()` | — | `Enum` |
| `ToString(string)` | — | `Enum` |
| `GetTypeCode()` | — | `Enum` |
| `CompareTo(object)` | — | `Enum` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
| `ToString(IFormatProvider)` | — | `Enum` |
| `ToString(string, IFormatProvider)` | — | `Enum` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
