---
title: AesirEventDiagnostics
description: "Runestone.AesirModules.AesirEventDiagnostics 的 API 文档"
---

# `AesirEventDiagnostics`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `AesirEventDiagnostics`

## 声明

``` csharp
public static class AesirEventDiagnostics
```

事件模块诊断门面：对外暴露运行时订阅状态查询与事件链路推送，全部为公开 API。
设计定位：数据层与视图层解耦的唯一契约。本类只依赖运行时类型，不引用任何编辑器或 Odin 代码，因此未安装 Odin 的消费者可直接调用这些 API 自建监视面板；有 Odin 时编辑器窗口 也只是同一套 API 的另一个消费者。

性能：采集默认在编辑器与 Development Build 开启、正式包关闭，由 Enabled 控制。 关闭时 EventModule 的分发热路径只多一次静态 bool 读取，不分配、不构造快照。 开启时快照写入固定容量环形缓冲（struct 覆盖槽位，无逐事件分配）。

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`DefaultCapacity`](#field-defaultcapacity) | 环形缓冲默认容量。 |

</div>

### DefaultCapacity {#field-defaultcapacity}

环形缓冲默认容量。

``` csharp
public const int DefaultCapacity = 256;
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Enabled`](#property-enabled) | 是否采集诊断数据。默认：编辑器与 Development Build 为 true，正式包为 false。 可在运行时切换；关闭后不再产生新快照，已有快照保留至 ClearTraces。 |
| [`TraceCapacity`](#property-tracecapacity) | 环形缓冲容量。设置时会重建缓冲并保留最近 min(原数量, 新容量) 条快照。 |
| [`TraceCount`](#property-tracecount) | 当前缓冲中的快照数量。 |

</div>

### Enabled {#property-enabled}

是否采集诊断数据。默认：编辑器与 Development Build 为 true，正式包为 false。 可在运行时切换；关闭后不再产生新快照，已有快照保留至 ClearTraces。

``` csharp
public static bool Enabled { get; set; } = true;
```

### TraceCapacity {#property-tracecapacity}

环形缓冲容量。设置时会重建缓冲并保留最近 min(原数量, 新容量) 条快照。

``` csharp
public static int TraceCapacity { get; set; } = 256;
```

### TraceCount {#property-tracecount}

当前缓冲中的快照数量。

``` csharp
public static int TraceCount { get; }
```

## 事件

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`TraceProduced`](#event-traceproduced) | 新快照产生时触发。订阅此事件以实现实时监视面板；回调在主线程分发结束时同步调用。 回调抛出的异常不会传播回事件分发（由门面隔离并记录），避免诊断设施影响业务。 |

</div>

### TraceProduced {#event-traceproduced}

新快照产生时触发。订阅此事件以实现实时监视面板；回调在主线程分发结束时同步调用。
回调抛出的异常不会传播回事件分发（由门面隔离并记录），避免诊断设施影响业务。

``` csharp
public static event Action<AesirEventTraceSnapshot> TraceProduced;
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetSubscriptions()`](#method-getsubscriptions) | 取所有已注册事件类型的订阅汇总（冷路径，供监视面板一次性拉取）。 |
| [`GetRecentTraces(int)`](#method-getrecenttraces-int) | 取最近的链路快照（最新在前）。max 小于 0 时返回全部。 返回的是新分配的数组（冷路径，供面板刷新调用）。 |
| [`GetSubscribers(Type)`](#method-getsubscribers-type) | 取指定事件类型的订阅者明细（冷路径）。事件类型无订阅时返回空数组。 |
| [`HasSubscribers(Type)`](#method-hassubscribers-type) | 指定事件类型是否有任何订阅者。 |
| [`GetSubscriberCount(Type)`](#method-getsubscribercount-type) | 指定事件类型的订阅者总数。 |
| [`ClearTraces()`](#method-cleartraces) | 清空所有链路快照。 |

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

### GetSubscriptions() {#method-getsubscriptions}

取所有已注册事件类型的订阅汇总（冷路径，供监视面板一次性拉取）。

``` csharp
public static AesirEventSubscriptionSnapshot[] GetSubscriptions()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AesirEventSubscriptionSnapshot[]` | — |

</div>

### GetRecentTraces(int) {#method-getrecenttraces-int}

取最近的链路快照（最新在前）。max 小于 0 时返回全部。 返回的是新分配的数组（冷路径，供面板刷新调用）。

``` csharp
public static AesirEventTraceSnapshot[] GetRecentTraces(int max = -1)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `max` | `int` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AesirEventTraceSnapshot[]` | — |

</div>

### GetSubscribers(Type) {#method-getsubscribers-type}

取指定事件类型的订阅者明细（冷路径）。事件类型无订阅时返回空数组。

``` csharp
public static AesirSubscriberSnapshot[] GetSubscribers(Type eventType)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `eventType` | `Type` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AesirSubscriberSnapshot[]` | — |

</div>

### HasSubscribers(Type) {#method-hassubscribers-type}

指定事件类型是否有任何订阅者。

``` csharp
public static bool HasSubscribers(Type eventType)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `eventType` | `Type` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### GetSubscriberCount(Type) {#method-getsubscribercount-type}

指定事件类型的订阅者总数。

``` csharp
public static int GetSubscriberCount(Type eventType)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `eventType` | `Type` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `int` | — |

</div>

### ClearTraces() {#method-cleartraces}

清空所有链路快照。

``` csharp
public static void ClearTraces()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
