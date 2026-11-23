---
title: SceneAssetWrapperAttributeProcessor
description: "Runestone.AesirModules.Editor.OdinInspector.SceneAssetWrapperAttributeProcessor 的 API 文档"
---

# `SceneAssetWrapperAttributeProcessor`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.Editor.OdinInspector`
    - **程序集:** `Runestone.AesirModules.Editor.OdinInspector`

**继承链:** `System.Object` → `Sirenix.OdinInspector.Editor.OdinAttributeProcessor` → `Sirenix.OdinInspector.Editor.OdinAttributeProcessor<SceneAssetWrapper>` → `SceneAssetWrapperAttributeProcessor`

**实现接口:** `Sirenix.Utilities.Editor.IHideObjectMembers`

## 声明

``` csharp
public class SceneAssetWrapperAttributeProcessor : Sirenix.OdinInspector.Editor.OdinAttributeProcessor<SceneAssetWrapper>, 
Sirenix.Utilities.Editor.IHideObjectMembers
```

为 SceneAssetWrapper 提供 Odin Inspector 属性处理器， 动态注入 Inspector 显示特性，使 Runtime 程序集零 Odin 依赖。
工具箱对位 Eflatun.SceneReference 的内联工具（三态着色 + 修复按钮）： 未加入 BuildSettings（红 + 添加按钮）、已加入但被禁用（黄 + 启用按钮）、 Addressable 场景（青色 + 说明框）、引用悬空（红色）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`SceneAssetWrapperAttributeProcessor()`](#constructor-sceneassetwrapperattributeprocessor) | — |

</div>

### SceneAssetWrapperAttributeProcessor() {#constructor-sceneassetwrapperattributeprocessor}

``` csharp
public SceneAssetWrapperAttributeProcessor()
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `ProcessChildMemberAttributes(InspectorProperty, MemberInfo, List<Attribute>)` | 为各成员注入 Odin 显示特性。 | `SceneAssetWrapperAttributeProcessor` |
| `ProcessSelfAttributes(InspectorProperty, List<Attribute>)` | 为 SceneAssetWrapper 字段本身注入特性（内联显示、校验提示框）。 | `SceneAssetWrapperAttributeProcessor` |
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
