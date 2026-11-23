---
title: AesirEventArgsSO
description: "Runestone.AesirModules.AesirEventArgsSO 的 API 文档"
---

# `AesirEventArgsSO`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `AesirEventArgsSO`

## 声明

``` csharp
[CreateAssetMenu]
public class AesirEventArgsSO : UnityEngine.ScriptableObject
```

AesirEventArgs 的 ScriptableObject 包装，让事件可保存为 .asset 资源， 在 Inspector 中配置载荷并由非程序员触发。
通过 Project 右键菜单 Create → Aesir → Event Module → AesirEventArgsSO 创建； 事件参数字段经 SubclassSelectorAttribute 下拉选择具体子类并配置其字段。

触发方式：Raise（代码 / Inspector 运行时按钮）、 或经其他 Inspector 可绑定入口（如 UnityEvent 调用）触发。 触发时以本资产为发布者（Sender）。

注意：Raise 发布的是资产内配置的同一个参数实例，订阅者不应修改事件参数载荷； 参数实例上经 WithFilter 添加的过滤器会跨次触发保留。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirEventArgsSO()`](#constructor-aesireventargsso) | — |

</div>

### AesirEventArgsSO() {#constructor-aesireventargsso}

``` csharp
public AesirEventArgsSO()
```

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`EventArgs`](#property-eventargs) | 配置的事件参数实例（只读）。未选择类型时为 null。 |

</div>

**继承的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `hideFlags` | — | `Object` |
| `name` | — | `Object` |

</div>

### EventArgs {#property-eventargs}

配置的事件参数实例（只读）。未选择类型时为 null。

``` csharp
public AesirEventArgs EventArgs { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Raise()`](#method-raise) | 以本资产为发布者触发事件。 未配置事件参数时输出错误日志并跳过。 |

</div>

**继承的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `GetInstanceID()` | — | `Object` |
| `Equals(object)` | — | `Object` |
| `GetHashCode()` | — | `Object` |
| `ToString()` | — | `Object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
| `SetDirty()` | — | `ScriptableObject` |

</div>

### Raise() {#method-raise}

以本资产为发布者触发事件。 未配置事件参数时输出错误日志并跳过。

``` csharp
public void Raise()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
