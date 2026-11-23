---
title: AesirArchitecture
description: "Runestone.AesirArchitecture.AesirArchitecture 的 API 文档"
---

# `AesirArchitecture`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.Component` → `UnityEngine.Behaviour` → `UnityEngine.MonoBehaviour` → `Sirenix.OdinInspector.SerializedMonoBehaviour` → `Runestone.AesirArchitecture.AesirMonoBehaviour` → `AesirArchitecture`

**实现接口:** `Sirenix.Serialization.ISupportsPrefabSerialization`，`UnityEngine.ISerializationCallbackReceiver`

## 声明

``` csharp
[DefaultExecutionOrder]
public class AesirArchitecture : Runestone.AesirArchitecture.AesirMonoBehaviour, 
Sirenix.Serialization.ISupportsPrefabSerialization, 
UnityEngine.ISerializationCallbackReceiver
```

Aesir Architecture 接入 MonoBehaviour 生命周期的持久化物体对象。

**备注**

本物体是框架 Mono 组件（MonoLifecycleProxy、RemoveListenerOnSceneUnloadedTrigger 等）的 DDOL 宿主， 通过 [DefaultExecutionOrder(-999)] 确保其 Awake 在场景中其他 MonoBehaviour 之前执行， 令宿主尽早完成去重与 DDOL 决策。
注意：本物体不初始化任何架构数据——架构上下文（AbstractContext{T}）是纯 C# 懒加载单例， 首次访问 AbstractContext<T>.Instance 时自动创建并完成注册与初始化，不依赖本物体存在； 预放置本物体仅在使用上述宿主挂载型组件时才有必要。
是否加入 DontDestroyOnLoad 场景由序列化字段 dontDestroyOnLoad 统一控制， 场景预放置与运行时创建两种来源共用同一份决策（预放置为子物体时本字段不参与判断，DDOL 跟随宿主）： 默认（勾选）：根物体实例在 Awake 时加入 DontDestroyOnLoad 场景，跨场景持久存在。 取消勾选：实例保留在所在场景、随场景卸载销毁——必须自行处理多场景叠加（Additive）加载下的 生命周期管理。Inspector 会显示警告信息框提示。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirArchitecture()`](#constructor-aesirarchitecture) | — |

</div>

### AesirArchitecture() {#constructor-aesirarchitecture}

``` csharp
public AesirArchitecture()
```

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`DontDestroyOnLoad`](#property-dontdestroyonload) | 获取 DDOL 开关的当前取值（只读）。 |
| [`Instance`](#property-instance) | 获取全局唯一的架构宿主实例（框架根 GameObject 持有者，承载 [Aesir Architecture] 宿主物体） |
| [`DontDestroyOnLoadFieldName`](#property-dontdestroyonloadfieldname) | — |

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

### DontDestroyOnLoad {#property-dontdestroyonload}

获取 DDOL 开关的当前取值（只读）。

**备注**

供运行时查询宿主的跨场景持久化决策（如判断引用是否会随场景卸载失效）， 亦供编辑器条件提示（Odin AttributeProcessor 的可见性表达式）复用—— 与 DontDestroyOnLoadFieldName 同属编辑器协作锚点。 以 new 显式隐藏 DontDestroyOnLoad(Object) 静态方法（有意同名）。

``` csharp
public bool DontDestroyOnLoad { get; }
```

### Instance {#property-instance}

获取全局唯一的架构宿主实例（框架根 GameObject 持有者，承载 [Aesir Architecture] 宿主物体）

**备注**

优先在已加载场景中查找预放置的实例；未找到时运行时创建， 新实例依据 dontDestroyOnLoad 默认值（true）在 Awake 中自动加入 DontDestroyOnLoad 场景。

``` csharp
public static AesirArchitecture Instance { get; }
```

### DontDestroyOnLoadFieldName {#property-dontdestroyonloadfieldname}

``` csharp
public static string DontDestroyOnLoadFieldName { get; } = "dontDestroyOnLoad";
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetOrAddComponent()`](#method-getoraddcomponent) | 获取或为架构物体添加指定的组件类型 |

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

### GetOrAddComponent() {#method-getoraddcomponent}

获取或为架构物体添加指定的组件类型

``` csharp
public static T GetOrAddComponent<T>()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `T` | 架构 GameObject 上已存在或新添加的组件实例 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
