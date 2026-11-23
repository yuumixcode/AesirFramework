---
title: AutoRemoveListenerHandle
description: "Runestone.AesirArchitecture.AutoRemoveListenerHandle 的 API 文档"
---

# `AutoRemoveListenerHandle`

!!! note ""

    - **种类:** `struct`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `System.ValueType` → `AutoRemoveListenerHandle`

**实现接口:** `System.IDisposable`

## 声明

``` csharp
public struct AutoRemoveListenerHandle : System.ValueType, 
System.IDisposable
```

自动移除监听句柄。包装注销回调。

**备注**

实现 IDisposable 的 struct，配合 using 语句可在作用域结束时自动移除监听， 避免因忘记调用 RemoveListener 而导致的内存泄漏。
内部持有 Action 回调，Dispose 后将回调置空， 因此重复调用 Dispose 是安全的，不会抛出异常。

该句柄由 AddListener 和 ObservableValue<T>.AddListener 返回。

每个句柄只对应一次 AddListener 调用：句柄捕获的是"本次注册所传入的回调"， 注销时执行一次等价的 RemoveListener。这与原生 C# 事件的语义一致 （+= 允许同一方法组重复入列，-= 只摘除一个匹配项）—— 同一回调注册 N 次会得到 N 个句柄，需 Dispose 全部 N 个才彻底退订。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AutoRemoveListenerHandle(Action)`](#constructor-autoremovelistenerhandle-action) | — |

</div>

### AutoRemoveListenerHandle(Action) {#constructor-autoremovelistenerhandle-action}

``` csharp
public AutoRemoveListenerHandle(Action removeListenerCallback)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `removeListenerCallback` | `Action` | — |

</div>

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Dispose()`](#method-dispose) | 执行移除监听，重复调用安全 |

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

### Dispose() {#method-dispose}

执行移除监听，重复调用安全

**备注**

调用内部回调后将其置空，确保重复调用 Dispose 不会再次触发注销逻辑， 也不会抛出 NullReferenceException。这使得在 using 语句和手动调用 混合使用时无需额外判空。

``` csharp
public void Dispose()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
