---
title: WorldSpaceUICanvas
description: "Runestone.AesirModules.WorldSpaceUICanvas 的 API 文档"
---

# `WorldSpaceUICanvas`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.Component` → `UnityEngine.Behaviour` → `UnityEngine.MonoBehaviour` → `Sirenix.OdinInspector.SerializedMonoBehaviour` → `Runestone.AesirArchitecture.AesirMonoBehaviour` → `WorldSpaceUICanvas`

**实现接口:** `Sirenix.Serialization.ISupportsPrefabSerialization`，`UnityEngine.ISerializationCallbackReceiver`

## 声明

``` csharp
[AddComponentMenu]
[DisallowMultipleComponent]
public sealed class WorldSpaceUICanvas : Runestone.AesirArchitecture.AesirMonoBehaviour, 
Sirenix.Serialization.ISupportsPrefabSerialization, 
UnityEngine.ISerializationCallbackReceiver
```

世界空间 UI Canvas 预设组件。挂在场景物体（如角色头顶、交互点）上即可获得一个 WorldSpace 渲染根，UI 随该物体在世界空间中的位置移动； 也可在运行时由 CreateWorldSpaceCanvas 挂到任意 Transform 上。

**备注**

与世界空间跟随目标的两种关系：物体是 Canvas 的父级时直接随父变换移动（如角色头顶血条—— Canvas 挂在角色子节点，不指定 FollowTarget）；物体是 Canvas 的兄弟节点时 指定 FollowTarget，由本组件每帧把 Canvas 的父级变换同步到目标。 两种情形都支持 PositionOffset（局部偏移）与 FaceCamera（面向相机）。

本组件只提供渲染根，不参与 UIModule 的面板 / 窗口注册表与生命周期： 世界空间 UI 的生命周期与宿主场景物体绑定，由业务代码直接驱动子节点内容。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`WorldSpaceUICanvas()`](#constructor-worldspaceuicanvas) | — |

</div>

### WorldSpaceUICanvas() {#constructor-worldspaceuicanvas}

``` csharp
public WorldSpaceUICanvas()
```

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Canvas`](#property-canvas) | — |
| [`FollowTarget`](#property-followtarget) | 当前跟随的世界空间对象（可运行时赋值切换目标，null 表示跟随父变换）。 |
| [`CanvasConfig`](#property-canvasconfig) | 本组件使用的世界空间 Canvas 配置。 |
| [`PositionOffset`](#property-positionoffset) | 相对跟随目标的局部位置偏移。 |
| [`Billboard`](#property-billboard) | 面向 UI 相机的具体模式。 |
| [`FaceCamera`](#property-facecamera) | 是否每帧面向 UI 相机。 |
| [`FlipFacing`](#property-flipfacing) | 是否翻转朝向（默认 true = UI 正面朝向相机；false = Canvas +Z 沿相机→Canvas 的 Unity 广告牌惯例）。 |
| [`SortingOrder`](#property-sortingorder) | Canvas 的 sortingOrder。 |

</div>

**继承的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `destroyCancellationToken` | — | `MonoBehaviour` |
| `gameObject` | — | `Component` |
| `hideFlags` | — | `Object` |
| `transform` | — | `Component` |
| `enabled` | — | `Behaviour` |
| `isActiveAndEnabled` | — | `Behaviour` |
| `runInEditMode` | — | `MonoBehaviour` |
| `useGUILayout` | — | `MonoBehaviour` |
| `name` | — | `Object` |
| `tag` | — | `Component` |
| `animation` | — | `Component` |
| `audio` | — | `Component` |
| `camera` | — | `Component` |
| `collider` | — | `Component` |
| `collider2D` | — | `Component` |
| `constantForce` | — | `Component` |
| `hingeJoint` | — | `Component` |
| `light` | — | `Component` |
| `networkView` | — | `Component` |
| `particleSystem` | — | `Component` |
| `renderer` | — | `Component` |
| `rigidbody` | — | `Component` |
| `rigidbody2D` | — | `Component` |

</div>

### Canvas {#property-canvas}

``` csharp
public Canvas Canvas { get; }
```

### FollowTarget {#property-followtarget}

当前跟随的世界空间对象（可运行时赋值切换目标，null 表示跟随父变换）。

``` csharp
public Transform FollowTarget { get; set; }
```

### CanvasConfig {#property-canvasconfig}

本组件使用的世界空间 Canvas 配置。

``` csharp
public UICanvasConfigSO CanvasConfig { get; }
```

### PositionOffset {#property-positionoffset}

相对跟随目标的局部位置偏移。

``` csharp
public Vector3 PositionOffset { get; set; }
```

### Billboard {#property-billboard}

面向 UI 相机的具体模式。

``` csharp
public WorldSpaceUICanvas.WorldSpaceBillboardMode Billboard { get; set; }
```

### FaceCamera {#property-facecamera}

是否每帧面向 UI 相机。

``` csharp
public bool FaceCamera { get; set; }
```

### FlipFacing {#property-flipfacing}

是否翻转朝向（默认 true = UI 正面朝向相机；false = Canvas +Z 沿相机→Canvas 的 Unity 广告牌惯例）。

``` csharp
public bool FlipFacing { get; set; }
```

### SortingOrder {#property-sortingorder}

Canvas 的 sortingOrder。

``` csharp
public int SortingOrder { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Build(UICanvasConfigSO)`](#method-build-uicanvasconfigso) | 用当前配置重新构建并接线 Canvas（渲染模式 / 缩放密度 / 射线检测 / sortingOrder / UI 层）。 通常在 Awake 中自动调用；运行时修改配置字段后可手动调用本方法即时生效。 |

</div>

**继承的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetComponent(Type)` | — | `Component` |
| `GetComponent(string)` | — | `Component` |
| `GetComponentInChildren(Type)` | — | `Component` |
| `GetComponentInChildren(Type, bool)` | — | `Component` |
| `GetComponentInParent(Type)` | — | `Component` |
| `GetComponentInParent(Type, bool)` | — | `Component` |
| `GetComponents(Type)` | — | `Component` |
| `GetComponentsInChildren(Type)` | — | `Component` |
| `GetComponentsInChildren(Type, bool)` | — | `Component` |
| `GetComponentsInParent(Type)` | — | `Component` |
| `GetComponentsInParent(Type, bool)` | — | `Component` |
| `StartCoroutine(IEnumerator)` | — | `MonoBehaviour` |
| `StartCoroutine(string)` | — | `MonoBehaviour` |
| `StartCoroutine(string, object)` | — | `MonoBehaviour` |
| `GetComponent()` | — | `Component` |
| `GetComponentInChildren()` | — | `Component` |
| `GetComponentInChildren(bool)` | — | `Component` |
| `GetComponentInParent()` | — | `Component` |
| `GetComponentInParent(bool)` | — | `Component` |
| `GetComponents()` | — | `Component` |
| `GetComponentsInChildren()` | — | `Component` |
| `GetComponentsInChildren(bool)` | — | `Component` |
| `GetComponentsInParent()` | — | `Component` |
| `GetComponentsInParent(bool)` | — | `Component` |
| `GetType()` | — | `object` |
| `CompareTag(string)` | — | `Component` |
| `IsInvoking()` | — | `MonoBehaviour` |
| `IsInvoking(string)` | — | `MonoBehaviour` |
| `TryGetComponent(Type, ref Component)` | — | `Component` |
| `TryGetComponent(ref T)` | — | `Component` |
| `GetComponentIndex()` | — | `Component` |
| `GetInstanceID()` | — | `Object` |
| `Equals(object)` | — | `Object` |
| `GetHashCode()` | — | `Object` |
| `ToString()` | — | `Object` |
| `BroadcastMessage(string)` | — | `Component` |
| `BroadcastMessage(string, SendMessageOptions)` | — | `Component` |
| `BroadcastMessage(string, object)` | — | `Component` |
| `BroadcastMessage(string, object, SendMessageOptions)` | — | `Component` |
| `CancelInvoke()` | — | `MonoBehaviour` |
| `CancelInvoke(string)` | — | `MonoBehaviour` |
| `GetComponents(Type, List<Component>)` | — | `Component` |
| `GetComponents(List<T>)` | — | `Component` |
| `GetComponentsInChildren(List<T>)` | — | `Component` |
| `GetComponentsInChildren(bool, List<T>)` | — | `Component` |
| `GetComponentsInParent(bool, List<T>)` | — | `Component` |
| `Invoke(string, float)` | — | `MonoBehaviour` |
| `InvokeRepeating(string, float, float)` | — | `MonoBehaviour` |
| `SendMessage(string)` | — | `Component` |
| `SendMessage(string, SendMessageOptions)` | — | `Component` |
| `SendMessage(string, object)` | — | `Component` |
| `SendMessage(string, object, SendMessageOptions)` | — | `Component` |
| `SendMessageUpwards(string)` | — | `Component` |
| `SendMessageUpwards(string, SendMessageOptions)` | — | `Component` |
| `SendMessageUpwards(string, object)` | — | `Component` |
| `SendMessageUpwards(string, object, SendMessageOptions)` | — | `Component` |
| `StopAllCoroutines()` | — | `MonoBehaviour` |
| `StopCoroutine(Coroutine)` | — | `MonoBehaviour` |
| `StopCoroutine(IEnumerator)` | — | `MonoBehaviour` |
| `StopCoroutine(string)` | — | `MonoBehaviour` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
| `OnAfterDeserialize()` | — | `SerializedMonoBehaviour` |
| `OnBeforeSerialize()` | — | `SerializedMonoBehaviour` |
| `StartCoroutine_Auto(IEnumerator)` | — | `MonoBehaviour` |

</div>

### Build(UICanvasConfigSO) {#method-build-uicanvasconfigso}

用当前配置重新构建并接线 Canvas（渲染模式 / 缩放密度 / 射线检测 / sortingOrder / UI 层）。 通常在 Awake 中自动调用；运行时修改配置字段后可手动调用本方法即时生效。

``` csharp
public void Build(UICanvasConfigSO overrideConfig = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `overrideConfig` | `UICanvasConfigSO` | 可选的配置覆盖。非 null 时写入序列化字段并作为本次配置来源，便于运行时换配置。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
