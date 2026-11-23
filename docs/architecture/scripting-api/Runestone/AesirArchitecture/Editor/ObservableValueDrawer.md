---
title: ObservableValueDrawer
description: "Runestone.AesirArchitecture.Editor.ObservableValueDrawer 的 API 文档"
---

# `ObservableValueDrawer`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `UnityEditor.GUIDrawer` → `UnityEditor.PropertyDrawer` → `ObservableValueDrawer`

## 声明

``` csharp
[CustomPropertyDrawer]
internal sealed class ObservableValueDrawer : UnityEditor.PropertyDrawer
```

ObservableValue{T} 的 UI Toolkit 属性绘制器（无 Odin 依赖）。

**备注**

复刻 ObservableValueAttributeProcessor 的四条 Odin 效果： HideLabel：根容器不画自身标签，改由内层 value 字段承载语义标签。 InlineProperty：直接内联 value 字段的 PropertyField，不产生嵌套 Foldout。 标签取父属性名（Odin 表达式 @$property.Parent.Name 的等价实现），用 displayName。 OnValueChanged：value 变更时调用 InvokeEvent 触发响应式通知。

开放泛型：[CustomPropertyDrawer] 绑定开放类型 ObservableValue<>，Unity 2022.3 会为所有封闭构造（int / float / Vector3 / 自定义 enum …）匹配本绘制器，已实测确认。

值与通知：Unity 序列化直接写入 value 字段、绕过 Value setter，故不会自发通知。这里用 TrackPropertyValue 监听 value 字段变更，反射拿到实例后调用 InvokeEvent（方法名取自 InvokeMethodName）补齐通知。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ObservableValueDrawer()`](#constructor-observablevaluedrawer) | — |

</div>

### ObservableValueDrawer() {#constructor-observablevaluedrawer}

``` csharp
public ObservableValueDrawer()
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `fieldInfo` | — | `PropertyDrawer` |
| `attribute` | — | `PropertyDrawer` |
| `preferredLabel` | — | `PropertyDrawer` |

</div>

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `CreatePropertyGUI(SerializedProperty)` | — | `ObservableValueDrawer` |
| `CanCacheInspectorGUI(SerializedProperty)` | — | `PropertyDrawer` |
| `Equals(object)` | — | `object` |
| `GetPropertyHeight(SerializedProperty, GUIContent)` | — | `PropertyDrawer` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `OnGUI(Rect, SerializedProperty, GUIContent)` | — | `PropertyDrawer` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
