---
title: ObservableValueAttributeProcessor<T>
description: "Runestone.AesirArchitecture.Editor.OdinInspector.ObservableValueAttributeProcessor<T> 的 API 文档"
---

# `ObservableValueAttributeProcessor<T>`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor.OdinInspector`
    - **程序集:** `Runestone.AesirArchitecture.Editor.OdinInspector`

**继承链:** `System.Object` → `Sirenix.OdinInspector.Editor.OdinAttributeProcessor` → `Sirenix.OdinInspector.Editor.OdinAttributeProcessor<ObservableValue<T>>` → `ObservableValueAttributeProcessor<T>`

**实现接口:** `Sirenix.Utilities.Editor.IHideObjectMembers`

**类型参数**

- `T` — ObservableValue 所包装的值类型

## 声明

``` csharp
internal sealed class ObservableValueAttributeProcessor<T> : Sirenix.OdinInspector.Editor.OdinAttributeProcessor<ObservableValue<T>>, 
Sirenix.Utilities.Editor.IHideObjectMembers 
```

为泛型 ObservableValue 提供的 Odin Inspector 特性处理器，用于优化其在面板上的展示效果。

**备注**

通过 Odin AttributeProcessor 机制，在不修改 ObservableValue{T} 类代码的前提下， 为其在 Inspector 中自动添加展示与响应特性：
ProcessSelfAttributes：添加 [HideLabel] 隐藏默认标签并配合 [InlineProperty] 内联展示， 使 ObservableValue{T} 在 Inspector 中以紧凑形式呈现，避免不必要的嵌套层级。

ProcessChildMemberAttributes：通过成员名匹配定位到内部 value 字段后， 添加 [OnValueChanged] 特性使其在 Inspector 编辑时自动调用 InvokeEvent() 触发变更通知， 实现 Inspector 中直接编辑值即可触发响应式更新，无需运行代码。

成员名匹配使用 PrivateValueFieldName 和 InvokeMethodName 常量， 避免硬编码字符串导致重构时不一致的风险。

[LabelText] 使用 Odin 表达式 @$property.Parent.Name 动态显示所属属性名， 使每个 ObservableValue 字段在 Inspector 中都有语义化的标签。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ObservableValueAttributeProcessor()`](#constructor-observablevalueattributeprocessor) | — |

</div>

### ObservableValueAttributeProcessor() {#constructor-observablevalueattributeprocessor}

``` csharp
public ObservableValueAttributeProcessor<T>()
```

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `ProcessChildMemberAttributes(InspectorProperty, MemberInfo, List<Attribute>)` | 处理子成员特性，当值在 Inspector 中被修改时自动触发变更通知事件 | `ObservableValueAttributeProcessor<T>` |
| `ProcessSelfAttributes(InspectorProperty, List<Attribute>)` | 处理类自身的特性，隐藏标签并使其内联展示 | `ObservableValueAttributeProcessor<T>` |
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
