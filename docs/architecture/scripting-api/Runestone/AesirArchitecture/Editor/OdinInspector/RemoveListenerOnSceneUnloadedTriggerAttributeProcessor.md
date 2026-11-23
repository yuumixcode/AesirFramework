---
title: RemoveListenerOnSceneUnloadedTriggerAttributeProcessor
description: "Runestone.AesirArchitecture.Editor.OdinInspector.RemoveListenerOnSceneUnloadedTriggerAttributeProcessor 的 API 文档"
---

# `RemoveListenerOnSceneUnloadedTriggerAttributeProcessor`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor.OdinInspector`
    - **程序集:** `Runestone.AesirArchitecture.Editor.OdinInspector`

**继承链:** `System.Object` → `Sirenix.OdinInspector.Editor.OdinAttributeProcessor` → `Sirenix.OdinInspector.Editor.OdinAttributeProcessor<RemoveListenerOnSceneUnloadedTrigger>` → `RemoveListenerOnSceneUnloadedTriggerAttributeProcessor`

**实现接口:** `Sirenix.Utilities.Editor.IHideObjectMembers`

## 声明

``` csharp
internal sealed class RemoveListenerOnSceneUnloadedTriggerAttributeProcessor : Sirenix.OdinInspector.Editor.OdinAttributeProcessor<RemoveListenerOnSceneUnloadedTrigger>, 
Sirenix.Utilities.Editor.IHideObjectMembers
```

为 RemoveListenerOnSceneUnloadedTrigger 类提供 Odin Inspector 属性处理器

**备注**

利用 Odin Attribute Processor 机制，在不修改 RemoveListenerOnSceneUnloadedTrigger 代码的前提下， 为其在 Inspector 中自动添加 InfoBoxAttribute 警告信息框， 提示宿主 DDOL 决策与本组件生命周期的联动约束： 本组件挂载于 [Aesir Architecture] 宿主，若宿主的 dontDestroyOnLoad 被关闭， 宿主（含本组件）会随所在场景卸载销毁，此后其他场景卸载将不再自动清理监听 — 使用该配置时需自行处理多场景叠加（Additive）加载下的生命周期。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`RemoveListenerOnSceneUnloadedTriggerAttributeProcessor()`](#constructor-removelisteneronsceneunloadedtriggerattributeprocessor) | — |

</div>

### RemoveListenerOnSceneUnloadedTriggerAttributeProcessor() {#constructor-removelisteneronsceneunloadedtriggerattributeprocessor}

``` csharp
public RemoveListenerOnSceneUnloadedTriggerAttributeProcessor()
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `ProcessSelfAttributes(InspectorProperty, List<Attribute>)` | 处理类自身的特性，添加宿主 DDOL 关闭风险警告信息框（仅当宿主开关关闭时显示） | `RemoveListenerOnSceneUnloadedTriggerAttributeProcessor` |
| `CanProcessChildMemberAttributes(InspectorProperty, MemberInfo)` | — | `OdinAttributeProcessor` |
| `CanProcessSelfAttributes(InspectorProperty)` | — | `OdinAttributeProcessor` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `ProcessChildMemberAttributes(InspectorProperty, MemberInfo, List<Attribute>)` | — | `OdinAttributeProcessor` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
