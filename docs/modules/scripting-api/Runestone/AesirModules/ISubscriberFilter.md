---
title: ISubscriberFilter
description: "Runestone.AesirModules.ISubscriberFilter 的 API 文档"
---

# `ISubscriberFilter`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

## 声明

``` csharp
public interface ISubscriberFilter
```

订阅者过滤器策略接口。发布者通过 WithFilter 声明过滤器， EventModule 分发时对每个订阅者逐个检查，全部过滤器通过才投递。
约定：过滤器无法解析对象（如发布者不是场景对象、订阅者不是 GameObject/Component）时 按"不通过"处理（fail-closed），避免过滤条件失效导致事件意外扩散。

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ShouldReceive(AesirEventArgs, object, SubscriberPriority)`](#method-shouldreceive-aesireventargs-object-subscriberpriority) | 判断订阅者是否应收到本次事件。 |

</div>

### ShouldReceive(AesirEventArgs, object, SubscriberPriority) {#method-shouldreceive-aesireventargs-object-subscriberpriority}

判断订阅者是否应收到本次事件。

``` csharp
public abstract bool ShouldReceive(AesirEventArgs eventArgs, object subscriber, SubscriberPriority priority)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `eventArgs` | `AesirEventArgs` | 正在分发的事件参数，可通过 Sender 获取发布者。 |
| `subscriber` | `object` | 待检查的订阅者对象。 |
| `priority` | `SubscriberPriority` | 该订阅绑定的优先级档位。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | true 表示投递给该订阅者；false 表示拦截。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
