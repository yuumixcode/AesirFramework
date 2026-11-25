---
title: ObservableHashSet<T>.Enumerator<T>
description: "Runestone.AesirArchitecture.ObservableHashSet<T>.Enumerator<T> 的 API 文档"
---

# `ObservableHashSet<T>.Enumerator<T>`

!!! note ""

    - **种类:** `struct`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `System.ValueType` → `ObservableHashSet<T>.Enumerator<T>`

**实现接口:** `System.Collections.Generic.IEnumerator<T>`，`System.Collections.IEnumerator`，`System.IDisposable`

**类型参数**

- `T` — 元素类型

## 声明

``` csharp
public struct ObservableHashSet<T>.Enumerator<T> : System.ValueType, 
System.Collections.Generic.IEnumerator<T>, 
System.Collections.IEnumerator, 
System.IDisposable 
```

可观察哈希集合实现。
Model 层持有可写实例，View 层通过 IReadOnlyObservableHashSet{T} 只读订阅。

**备注**

内部组合 HashSet{T} 存储元素，变更通知经 MiniEvent{T} 分发——Invoke 路径零分配 （直接多播调用）。注意：订阅路径（AddListener / 句柄创建）有与监听者数量成正比的委托分配， 勿在每帧订阅场景使用。
[SerializeField] 标记 set 字段——Unity 原生不序列化 HashSet{T}（该标记对 Unity 序列化无效果）， 安装 Odin Inspector 且宿主走 Odin 序列化（如 AesirMonoBehaviour 派生组件）时， 该字段可被 Odin 序列化管线接管，便于在 Inspector 中编辑初始元素（与 ObservableDictionary{TKey, TValue} 行为一致）。

变更通知为单一事件（AddListener）：写操作完成后才触发，监听者回调中读取到的集合已是变更后的状态； 无变更的操作不通知（Add 重复元素、Remove 不存在的元素、Clear 空集合）； 批量操作（AddRange / RemoveRange）逐项通知实际变更的元素； Clear 以 Reset 通知。 集合无索引概念，载荷索引固定 -1。

遍历性能：foreach 具体类型走结构体枚举器，零分配；通过 IReadOnlyObservableHashSet{T} / IEnumerable{T} 接口遍历会装箱一次枚举器（与 BCL HashSet{T} 行为一致）。

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Current`](#property-current) | 获取当前位置的元素。 |

</div>

### Current {#property-current}

获取当前位置的元素。

``` csharp
public T Current { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`MoveNext()`](#method-movenext) | 前进到下一个元素。 |

</div>

**继承的方法**

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

### MoveNext() {#method-movenext}

前进到下一个元素。

``` csharp
public bool MoveNext()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 存在下一个元素返回 true，遍历结束返回 false。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
