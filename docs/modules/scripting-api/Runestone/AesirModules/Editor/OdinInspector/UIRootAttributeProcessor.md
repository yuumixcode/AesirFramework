---
title: UIRootAttributeProcessor
description: "Runestone.AesirModules.Editor.OdinInspector.UIRootAttributeProcessor 的 API 文档"
---

# `UIRootAttributeProcessor`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.Editor.OdinInspector`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `Sirenix.OdinInspector.Editor.OdinAttributeProcessor` → `Sirenix.OdinInspector.Editor.OdinAttributeProcessor<UIRoot>` → `UIRootAttributeProcessor`

**实现接口:** `Sirenix.Utilities.Editor.IHideObjectMembers`

## 声明

``` csharp
public class UIRootAttributeProcessor : Sirenix.OdinInspector.Editor.OdinAttributeProcessor<UIRoot>, 
Sirenix.Utilities.Editor.IHideObjectMembers
```

为 UIRoot 提供 Odin Inspector 属性处理器， 动态注入 Inspector 显示特性，使 Runtime 程序集零 Odin 依赖。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`UIRootAttributeProcessor()`](#constructor-uirootattributeprocessor) | — |

</div>

### UIRootAttributeProcessor() {#constructor-uirootattributeprocessor}

``` csharp
public UIRootAttributeProcessor()
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `ProcessChildMemberAttributes(InspectorProperty, MemberInfo, List<Attribute>)` | — | `UIRootAttributeProcessor` |
| `CanProcessChildMemberAttributes(InspectorProperty, MemberInfo)` | — | `OdinAttributeProcessor` |
| `CanProcessSelfAttributes(InspectorProperty)` | — | `OdinAttributeProcessor` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `ProcessSelfAttributes(InspectorProperty, List<Attribute>)` | — | `OdinAttributeProcessor` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
