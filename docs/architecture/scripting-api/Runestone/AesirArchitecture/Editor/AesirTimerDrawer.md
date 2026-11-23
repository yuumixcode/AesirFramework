---
title: AesirTimerDrawer
description: "Runestone.AesirArchitecture.Editor.AesirTimerDrawer 的 API 文档"
---

# `AesirTimerDrawer`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `UnityEditor.GUIDrawer` → `UnityEditor.PropertyDrawer` → `AesirTimerDrawer`

## 声明

``` csharp
[CustomPropertyDrawer]
internal sealed class AesirTimerDrawer : UnityEditor.PropertyDrawer
```

AesirTimer 字段的只读检视：状态词（着色）+ 进度条 + 单行摘要。

**备注**

生效前提：字段必须是 Unity 的序列化属性。AesirTimer 是普通引用类型， 原生 Inspector 只为 [SerializeReference] 字段建立托管引用属性；写成普通 public AesirTimer 时它根本不在属性树里（Unity 不绘制不可序列化的类型）， 本绘制器也就不会被调用。零侵入的查看路径是 Tools/Aesir/Architecture/Timer Debugger 窗口；想在自己组件的面板里看计时器， 写自定义 Inspector（见 Samples/AesirTimer 的示例）。

取值方式：走 fieldInfo 反射而不是 SerializedProperty.managedReferenceValue——后者的 getter 会为 null 引用 即时实例化对象（看一眼面板就凭空多出一个计时器实例）。 因此只在字段深度为 0（直属宿主对象）时读取；集合元素等位置退化为创建指引文本。

绘制外观来自 AesirTimerSummaryView，措辞来自 AesirTimerText， 本类只负责把字段值抓成 AesirTimerSnapshot。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirTimerDrawer()`](#constructor-aesirtimerdrawer) | — |

</div>

### AesirTimerDrawer() {#constructor-aesirtimerdrawer}

``` csharp
public AesirTimerDrawer()
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
| `CreatePropertyGUI(SerializedProperty)` | UI Toolkit 绘制入口（复刻 AesirTimerOdinDrawer 的两行摘要）：状态词着色 + 进度条 + 单行摘要。 | `AesirTimerDrawer` |
| `GetPropertyHeight(SerializedProperty, GUIContent)` | — | `AesirTimerDrawer` |
| `OnGUI(Rect, SerializedProperty, GUIContent)` | — | `AesirTimerDrawer` |
| `CanCacheInspectorGUI(SerializedProperty)` | — | `PropertyDrawer` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
