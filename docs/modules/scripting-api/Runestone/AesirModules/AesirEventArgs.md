---
title: AesirEventArgs
description: "Runestone.AesirModules.AesirEventArgs 的 API 文档"
---

# `AesirEventArgs`

!!! note ""

    - **种类:** `abstract class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `AesirEventArgs`

## 声明

``` csharp
[Serializable]
public abstract class AesirEventArgs
```

事件参数基类。所有自定义事件参数继承此类，作为事件数据载体在 EventModule 中传递。
注意：AesirEventArgs 本身不持有监听者，仅作为参数实例。 订阅管理由 EventModule 的注册表负责。

通过 WithFilter / WithFilters 可链式声明订阅者过滤器， 实现"只让特定范围的订阅者收到"的精确投递。

通过 Cancel 可中止本次传播（后续 High/Medium 档订阅者不再收到）， First / Last 档不受影响——用于前置校验与收尾清理。

分发约定：同一参数实例在分发期间不得被再次发布（EventModule 会拒绝并报错）； 事件链请使用新的参数实例，或经 AesirTimer.NextFrame 延后发布。 SetSender 是发布者侧 API，订阅者不应在分发期间调用。

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Filters`](#property-filters) | 只读过滤器列表视图。未添加任何过滤器时为 null。 |
| [`IsCancelled`](#property-iscancelled) | 本次分发是否已被取消。由 Cancel 置位，由 EventModule 在每趟分发开始时复位。 取消只影响 High 与 Medium 档的后续订阅者；First / Last 两档始终收到（分别承担前置校验与收尾清理）。 |
| [`IsNonCancellable`](#property-isnoncancellable) | 是否禁止取消。为 true 时 Cancel 不生效。 由 NonCancellable 链式开启，用于"必须送达所有订阅者"的关键事件。 |
| [`CancelledBy`](#property-cancelledby) | 取消本次传播的订阅者。未取消时为 null。 |
| [`Sender`](#property-sender) | 事件发布者。由 EventModule 在分发时写入。 |

</div>

### Filters {#property-filters}

只读过滤器列表视图。未添加任何过滤器时为 null。

``` csharp
public IReadOnlyList<ISubscriberFilter> Filters { get; }
```

### IsCancelled {#property-iscancelled}

本次分发是否已被取消。由 Cancel 置位，由 EventModule 在每趟分发开始时复位。
取消只影响 High 与 Medium 档的后续订阅者；First / Last 两档始终收到（分别承担前置校验与收尾清理）。

``` csharp
public bool IsCancelled { get; internal set; }
```

### IsNonCancellable {#property-isnoncancellable}

是否禁止取消。为 true 时 Cancel 不生效。 由 NonCancellable 链式开启，用于"必须送达所有订阅者"的关键事件。

``` csharp
public bool IsNonCancellable { get; private set; }
```

### CancelledBy {#property-cancelledby}

取消本次传播的订阅者。未取消时为 null。

``` csharp
public object CancelledBy { get; internal set; }
```

### Sender {#property-sender}

事件发布者。由 EventModule 在分发时写入。

``` csharp
public object Sender { get; private set; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Cancel(object)`](#method-cancel-object) | 取消本次事件传播：在当前订阅者之后，High 与 Medium 档尚未执行的订阅者将不再收到事件； First / Last 档不受影响。 典型用途：输入吞噬、伤害减免链、互斥 UI 抢先响应。若事件经 NonCancellable 标记为不可取消，本调用无效果（不抛异常）。 |
| [`NonCancellable()`](#method-noncancellable) | 将事件标记为不可取消：此后 Cancel 不再生效。 用于"必须送达所有订阅者"的关键事件（如结算、存档）。 |
| [`SetSender(object)`](#method-setsender-object) | 设置事件发布者。发布者侧 API：在 Invoke() 前声明发布者， 或由 EventModule 在分发开始时写入；订阅者不应在分发期间调用。 |
| [`WithFilter(ISubscriberFilter)`](#method-withfilter-isubscriberfilter) | 链式添加一个订阅者过滤器。多次调用依次叠加，分发时全部过滤器通过才投递。 过滤器随事件参数实例存在：缓存复用的参数实例（如 AesirEventArgsSO）会保留已添加的过滤器。 |
| [`WithFilters(ISubscriberFilter[])`](#method-withfilters-isubscriberfilter) | 链式添加一组订阅者过滤器。 |
| [`Invoke()`](#method-invoke) | 使用已设置的发布者触发事件。需先通过 SetSender 设置发布者。 |
| [`Invoke(object)`](#method-invoke-object) | 使用指定发布者触发事件。 |

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

### Cancel(object) {#method-cancel-object}

取消本次事件传播：在当前订阅者之后，High 与 Medium 档尚未执行的订阅者将不再收到事件； First / Last 档不受影响。
典型用途：输入吞噬、伤害减免链、互斥 UI 抢先响应。若事件经 NonCancellable 标记为不可取消，本调用无效果（不抛异常）。

``` csharp
public AesirEventArgs Cancel(object canceller)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `canceller` | `object` | 取消者（通常是订阅者自身），写入 CancelledBy 供诊断查看。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AesirEventArgs` | 当前事件参数实例（支持链式调用）。 |

</div>

### NonCancellable() {#method-noncancellable}

将事件标记为不可取消：此后 Cancel 不再生效。 用于"必须送达所有订阅者"的关键事件（如结算、存档）。

``` csharp
public AesirEventArgs NonCancellable()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AesirEventArgs` | 当前事件参数实例（支持链式调用）。 |

</div>

### SetSender(object) {#method-setsender-object}

设置事件发布者。发布者侧 API：在 Invoke() 前声明发布者， 或由 EventModule 在分发开始时写入；订阅者不应在分发期间调用。

``` csharp
public AesirEventArgs SetSender(object sender)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sender` | `object` | 发布者对象。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AesirEventArgs` | 当前事件参数实例（支持链式调用）。 |

</div>

### WithFilter(ISubscriberFilter) {#method-withfilter-isubscriberfilter}

链式添加一个订阅者过滤器。多次调用依次叠加，分发时全部过滤器通过才投递。 过滤器随事件参数实例存在：缓存复用的参数实例（如 AesirEventArgsSO）会保留已添加的过滤器。

``` csharp
public AesirEventArgs WithFilter(ISubscriberFilter filter)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `filter` | `ISubscriberFilter` | 过滤器实例。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AesirEventArgs` | 当前事件参数实例（支持链式调用）。 |

</div>

### WithFilters(ISubscriberFilter[]) {#method-withfilters-isubscriberfilter}

链式添加一组订阅者过滤器。

``` csharp
public AesirEventArgs WithFilters(params ISubscriberFilter[] filters)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `filters` | `ISubscriberFilter[]` | 过滤器实例数组。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AesirEventArgs` | 当前事件参数实例（支持链式调用）。 |

</div>

### Invoke() {#method-invoke}

使用已设置的发布者触发事件。需先通过 SetSender 设置发布者。

``` csharp
public void Invoke()
```

### Invoke(object) {#method-invoke-object}

使用指定发布者触发事件。

``` csharp
public void Invoke(object sender)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sender` | `object` | 发布者对象。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
