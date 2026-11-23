---
title: UICanvasConfigSO
description: "Runestone.AesirModules.UICanvasConfigSO 的 API 文档"
---

# `UICanvasConfigSO`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `Sirenix.OdinInspector.SerializedScriptableObject` → `Runestone.AesirArchitecture.AesirScriptableObject` → `UICanvasConfigSO`

**实现接口:** `UnityEngine.ISerializationCallbackReceiver`

## 声明

``` csharp
public class UICanvasConfigSO : Runestone.AesirArchitecture.AesirScriptableObject, 
UnityEngine.ISerializationCallbackReceiver
```

UI Canvas 配置资产。创建路径：Assets → Create → Aesir Modules → UI → Default UICanvasConfig。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`UICanvasConfigSO()`](#constructor-uicanvasconfigso) | — |

</div>

### UICanvasConfigSO() {#constructor-uicanvasconfigso}

``` csharp
public UICanvasConfigSO()
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `hideFlags` | — | `Object` |
| `name` | — | `Object` |

</div>

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ApplyToCanvas(Canvas)`](#method-applytocanvas-canvas) | 将本资产的所有配置统一应用到目标 Canvas 及其关联的 CanvasScaler 和 GraphicRaycaster。 世界空间渲染模式（WorldSpace）下由 Canvas 自身驱动缩放， 本方法跳过 CanvasScaler 的 uiScaleMode 写入——避免 Scale With Screen Size 等屏幕空间模式 在世界空间下产生非预期的面板尺寸；世界空间缩放字段改由 ApplyToWorldSpaceCanvas 统一配置。 |
| [`ApplyToWorldSpaceCanvas(Canvas)`](#method-applytoworldspacecanvas-canvas) | 将本资产的世界空间预设应用到目标 Canvas。 设置渲染模式为 WorldSpace，把 RectTransform 转为等比例缩放的正方形像素网格 （世界空间 UGUI 的标准做法，避免非等比宽高导致 UI 元素拉伸变形），并按 worldSpaceDynamicPixelsPerUnit / worldSpaceReferencePixelsPerUnit 配置密度。 不做世界空间定位、不指定相机——跟随目标由 WorldSpaceUICanvas 负责。  若 Canvas 上有 CanvasScaler，其 uiScaleMode 会被设为 ConstantPixelSize（世界空间下其它模式无意义）； GraphicRaycaster 沿用本资产的射线相关字段。 |
| [`CreateDefault()`](#method-createdefault) | — |
| [`CreateWorldSpaceDefault()`](#method-createworldspacedefault) | 创建一份用于世界空间 UI 的配置实例（不持久化为资产）。 与 CreateDefault（屏幕空间预设）平行：渲染模式为 WorldSpace， 缩放模式为 ConstantPixelSize（世界空间下 Screen Space 缩放模式无意义）， 其余射线检测与着色器通道字段沿用默认值。可直接赋给 WorldSpaceCanvasConfig， 或由自定义配置加载器返回。 |

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
| `OnAfterDeserialize()` | — | `SerializedScriptableObject` |
| `OnBeforeSerialize()` | — | `SerializedScriptableObject` |
| `SetDirty()` | — | `ScriptableObject` |

</div>

### ApplyToCanvas(Canvas) {#method-applytocanvas-canvas}

将本资产的所有配置统一应用到目标 Canvas 及其关联的 CanvasScaler 和 GraphicRaycaster。
世界空间渲染模式（WorldSpace）下由 Canvas 自身驱动缩放， 本方法跳过 CanvasScaler 的 uiScaleMode 写入——避免 Scale With Screen Size 等屏幕空间模式 在世界空间下产生非预期的面板尺寸；世界空间缩放字段改由 ApplyToWorldSpaceCanvas 统一配置。

``` csharp
public void ApplyToCanvas(Canvas canvas)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `canvas` | `Canvas` | 目标 Canvas。 |

</div>

### ApplyToWorldSpaceCanvas(Canvas) {#method-applytoworldspacecanvas-canvas}

将本资产的世界空间预设应用到目标 Canvas。
设置渲染模式为 WorldSpace，把 RectTransform 转为等比例缩放的正方形像素网格 （世界空间 UGUI 的标准做法，避免非等比宽高导致 UI 元素拉伸变形），并按 worldSpaceDynamicPixelsPerUnit / worldSpaceReferencePixelsPerUnit 配置密度。 不做世界空间定位、不指定相机——跟随目标由 WorldSpaceUICanvas 负责。

若 Canvas 上有 CanvasScaler，其 uiScaleMode 会被设为 ConstantPixelSize（世界空间下其它模式无意义）； GraphicRaycaster 沿用本资产的射线相关字段。

``` csharp
public void ApplyToWorldSpaceCanvas(Canvas canvas)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `canvas` | `Canvas` | 目标 Canvas，通常挂在 WorldSpaceUICanvas 所在物体上。 |

</div>

### CreateDefault() {#method-createdefault}

``` csharp
public static UICanvasConfigSO CreateDefault()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `UICanvasConfigSO` | — |

</div>

### CreateWorldSpaceDefault() {#method-createworldspacedefault}

创建一份用于世界空间 UI 的配置实例（不持久化为资产）。
与 CreateDefault（屏幕空间预设）平行：渲染模式为 WorldSpace， 缩放模式为 ConstantPixelSize（世界空间下 Screen Space 缩放模式无意义）， 其余射线检测与着色器通道字段沿用默认值。可直接赋给 WorldSpaceCanvasConfig， 或由自定义配置加载器返回。

``` csharp
public static UICanvasConfigSO CreateWorldSpaceDefault()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `UICanvasConfigSO` | 已应用世界空间预设的配置实例。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
