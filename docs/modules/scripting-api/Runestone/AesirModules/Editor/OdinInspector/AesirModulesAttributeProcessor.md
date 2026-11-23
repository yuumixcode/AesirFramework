---
title: AesirModulesAttributeProcessor
description: "Runestone.AesirModules.Editor.OdinInspector.AesirModulesAttributeProcessor 的 API 文档"
---

# `AesirModulesAttributeProcessor`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.Editor.OdinInspector`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `Sirenix.OdinInspector.Editor.OdinAttributeProcessor` → `Sirenix.OdinInspector.Editor.OdinAttributeProcessor<AesirModules>` → `AesirModulesAttributeProcessor`

**实现接口:** `Sirenix.Utilities.Editor.IHideObjectMembers`

## 声明

``` csharp
public class AesirModulesAttributeProcessor : Sirenix.OdinInspector.Editor.OdinAttributeProcessor<AesirModules>, 
Sirenix.Utilities.Editor.IHideObjectMembers
```

为 AesirModules 提供 Odin Inspector 属性处理器， 动态注入 DDOL 开关的说明与警告信息框，使 Runtime 程序集零 Odin 依赖。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirModulesAttributeProcessor()`](#constructor-aesirmodulesattributeprocessor) | — |

</div>

### AesirModulesAttributeProcessor() {#constructor-aesirmodulesattributeprocessor}

``` csharp
public AesirModulesAttributeProcessor()
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `ProcessChildMemberAttributes(InspectorProperty, MemberInfo, List<Attribute>)` | 处理子成员的特性，为 DDOL 开关字段动态注入取值含义说明（Info 级信息框，恒显示） | `AesirModulesAttributeProcessor` |
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
