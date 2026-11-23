---
title: AesirArchitectureAttributeProcessor
description: "Runestone.AesirArchitecture.Editor.OdinInspector.AesirArchitectureAttributeProcessor 的 API 文档"
---

# `AesirArchitectureAttributeProcessor`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor.OdinInspector`
    - **程序集:** `Runestone.AesirArchitecture.Editor.OdinInspector`

**继承链:** `System.Object` → `Sirenix.OdinInspector.Editor.OdinAttributeProcessor` → `Sirenix.OdinInspector.Editor.OdinAttributeProcessor<AesirArchitecture>` → `AesirArchitectureAttributeProcessor`

**实现接口:** `Sirenix.Utilities.Editor.IHideObjectMembers`

## 声明

``` csharp
internal sealed class AesirArchitectureAttributeProcessor : Sirenix.OdinInspector.Editor.OdinAttributeProcessor<AesirArchitecture>, 
Sirenix.Utilities.Editor.IHideObjectMembers
```

为 AesirArchitecture 类动态添加特性。

**备注**

Inspector 呈现全部由本处理器动态注入（样式与逻辑分离），运行时程序集不持有任何 Inspector 样式特性： 类级 Info 信息框：类职责说明（恒显示）。 类级 Warning 信息框：DDOL 开关关闭时的多场景叠加风险提醒（仅在关闭时显示）。 字段级 Info 信息框：dontDestroyOnLoad 开关的取值含义说明（恒显示，替代运行时 Tooltip）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirArchitectureAttributeProcessor()`](#constructor-aesirarchitectureattributeprocessor) | — |

</div>

### AesirArchitectureAttributeProcessor() {#constructor-aesirarchitectureattributeprocessor}

``` csharp
public AesirArchitectureAttributeProcessor()
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `ProcessChildMemberAttributes(InspectorProperty, MemberInfo, List<Attribute>)` | 处理子成员的特性，为 DDOL 开关字段动态注入取值含义说明（Info 级信息框，恒显示） | `AesirArchitectureAttributeProcessor` |
| `ProcessSelfAttributes(InspectorProperty, List<Attribute>)` | 处理类自身的特性，添加描述信息框与 DDOL 关闭警告信息框 | `AesirArchitectureAttributeProcessor` |
| `CanProcessChildMemberAttributes(InspectorProperty, MemberInfo)` | — | `OdinAttributeProcessor` |
| `CanProcessSelfAttributes(InspectorProperty)` | — | `OdinAttributeProcessor` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
