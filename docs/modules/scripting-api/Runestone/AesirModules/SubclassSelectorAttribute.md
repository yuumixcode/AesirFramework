---
title: SubclassSelectorAttribute
description: "Runestone.AesirModules.SubclassSelectorAttribute 的 API 文档"
---

# `SubclassSelectorAttribute`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `System.Attribute` → `UnityEngine.PropertyAttribute` → `SubclassSelectorAttribute`

**实现接口:** `System.Runtime.InteropServices._Attribute`

## 声明

``` csharp
public class SubclassSelectorAttribute : UnityEngine.PropertyAttribute, 
System.Runtime.InteropServices._Attribute
```

标记 [SerializeReference] 字段在 Inspector 中使用子类下拉选择器。
点击下拉按钮弹出字段声明类型的全部可选子类（按命名空间分组）， 选择后自动创建实例并显示其序列化字段。供 AesirEventArgsSO、 UnityEventOnAesirEvent 等需要 Inspector 配置 AesirEventArgs 具体子类的场景使用。

候选类型要求：非抽象、非泛型定义、继承 [Serializable]（自身或基类声明均可—— 基类已标注时派生类无需重复标注，与 Unity 序列化规则一致）、 非 Object 派生；标有 ExcludeSubclassSelectorAttribute 的类型不出现在下拉中。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`SubclassSelectorAttribute()`](#constructor-subclassselectorattribute) | — |

</div>

### SubclassSelectorAttribute() {#constructor-subclassselectorattribute}

``` csharp
public SubclassSelectorAttribute()
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `order` | — | `PropertyAttribute` |
| `TypeId` | — | `Attribute` |

</div>

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `Attribute` |
| `GetHashCode()` | — | `Attribute` |
| `IsDefaultAttribute()` | — | `Attribute` |
| `Match(object)` | — | `Attribute` |
| `ToString()` | — | `object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
