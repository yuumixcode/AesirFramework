---
title: AesirEventUtility
description: "Runestone.AesirModules.AesirEventUtility 的 API 文档"
---

# `AesirEventUtility`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `AesirEventUtility`

## 声明

``` csharp
public static class AesirEventUtility
```

事件模块静态工具方法：Unity null 检测与 GameObject 解析， 供订阅者过滤器与诊断门面解析发布者/订阅者。

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`IsObjectUnityNull(object)`](#method-isobjectunitynull-object) | 检测对象是否为 Unity 假 null（已销毁但引用未置空）。 |
| [`TryGetGameObject(object, ref GameObject)`](#method-trygetgameobject-object-ref-gameobject) | 尝试将对象解析为 GameObject。支持 GameObject 本身与任意 Component（含 MonoBehaviour）。供订阅者过滤器解析发布者/订阅者。 |

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

### IsObjectUnityNull(object) {#method-isobjectunitynull-object}

检测对象是否为 Unity 假 null（已销毁但引用未置空）。

``` csharp
public static bool IsObjectUnityNull(object obj)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `obj` | `object` | 待检测的对象。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 如果对象为 null 或已销毁的 Unity 对象，返回 true。 |

</div>

### TryGetGameObject(object, ref GameObject) {#method-trygetgameobject-object-ref-gameobject}

尝试将对象解析为 GameObject。支持 GameObject 本身与任意 Component（含 MonoBehaviour）。供订阅者过滤器解析发布者/订阅者。

``` csharp
public static bool TryGetGameObject(object obj, out ref GameObject gameObject)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `obj` | `object` | 待解析对象。 |
| `gameObject` | `ref GameObject` | 解析出的 GameObject；解析失败时为 null。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 成功解析返回 true。已销毁的 Unity 对象（假 null）与纯 C# 对象均返回 false。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
