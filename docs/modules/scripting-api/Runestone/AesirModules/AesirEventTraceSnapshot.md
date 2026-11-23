---
title: AesirEventTraceSnapshot
description: "Runestone.AesirModules.AesirEventTraceSnapshot 的 API 文档"
---

# `AesirEventTraceSnapshot`

!!! note ""

    - **种类:** `struct`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `System.ValueType` → `AesirEventTraceSnapshot`

## 声明

``` csharp
[IsReadOnly]
public struct AesirEventTraceSnapshot : System.ValueType
```

事件链路快照。诊断门面 AesirEventDiagnostics 推送与查询的最小不可变数据单元。
用 struct 存于固定容量环形缓冲，记录路径无托管分配；字段刻意保持精简， 只覆盖"谁在什么时刻触发/绑定了什么事件、命中多少订阅者"这一层可观测信息。 Timestamp 为 UTC 时间，展示时请自行 ToLocalTime()。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirEventTraceSnapshot(AesirEventTraceKind, string, Type, string, object, int, int, float, bool, int)`](#constructor-aesireventtracesnapshot-aesireventtracekind-string-type-string-object-int-int-float-bool-int) | — |

</div>

### AesirEventTraceSnapshot(AesirEventTraceKind, string, Type, string, object, int, int, float, bool, int) {#constructor-aesireventtracesnapshot-aesireventtracekind-string-type-string-object-int-int-float-bool-int}

``` csharp
public AesirEventTraceSnapshot(AesirEventTraceKind kind, string eventName, Type eventType, string actorName, object actor, int invokedCount, int totalSubscriberCount, float executionMs, bool cancelled, int filterCount)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `kind` | `AesirEventTraceKind` | — |
| `eventName` | `string` | — |
| `eventType` | `Type` | — |
| `actorName` | `string` | — |
| `actor` | `object` | — |
| `invokedCount` | `int` | — |
| `totalSubscriberCount` | `int` | — |
| `executionMs` | `float` | — |
| `cancelled` | `bool` | — |
| `filterCount` | `int` | — |

</div>

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Kind`](#field-kind) | — |
| [`Timestamp`](#field-timestamp) | — |
| [`EventType`](#field-eventtype) | — |
| [`Cancelled`](#field-cancelled) | — |
| [`ExecutionMs`](#field-executionms) | — |
| [`FilterCount`](#field-filtercount) | — |
| [`InvokedCount`](#field-invokedcount) | — |
| [`TotalSubscriberCount`](#field-totalsubscribercount) | — |
| [`Actor`](#field-actor) | — |
| [`ActorName`](#field-actorname) | — |
| [`EventName`](#field-eventname) | — |

</div>

### Kind {#field-kind}

``` csharp
public readonly AesirEventTraceKind Kind;
```

### Timestamp {#field-timestamp}

``` csharp
public readonly DateTime Timestamp;
```

### EventType {#field-eventtype}

``` csharp
public readonly Type EventType;
```

### Cancelled {#field-cancelled}

``` csharp
public readonly bool Cancelled;
```

### ExecutionMs {#field-executionms}

``` csharp
public readonly float ExecutionMs;
```

### FilterCount {#field-filtercount}

``` csharp
public readonly int FilterCount;
```

### InvokedCount {#field-invokedcount}

``` csharp
public readonly int InvokedCount;
```

### TotalSubscriberCount {#field-totalsubscribercount}

``` csharp
public readonly int TotalSubscriberCount;
```

### Actor {#field-actor}

``` csharp
public readonly object Actor;
```

### ActorName {#field-actorname}

``` csharp
public readonly string ActorName;
```

### EventName {#field-eventname}

``` csharp
public readonly string EventName;
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `ValueType` |
| `GetHashCode()` | — | `ValueType` |
| `ToString()` | — | `ValueType` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
