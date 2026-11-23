---
title: EventModule
description: "Runestone.AesirModules.EventModule 的 API 文档"
---

# `EventModule`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.Component` → `UnityEngine.Behaviour` → `UnityEngine.MonoBehaviour` → `Sirenix.OdinInspector.SerializedMonoBehaviour` → `Runestone.AesirArchitecture.AesirMonoBehaviour` → `EventModule`

**实现接口:** `Sirenix.Serialization.ISupportsPrefabSerialization`，`UnityEngine.ISerializationCallbackReceiver`

## 声明

``` csharp
[DisallowMultipleComponent]
[DefaultExecutionOrder]
[AddComponentMenu]
public class EventModule : Runestone.AesirArchitecture.AesirMonoBehaviour, 
Sirenix.Serialization.ISupportsPrefabSerialization, 
UnityEngine.ISerializationCallbackReceiver
```

事件模块（MonoBehaviour 单例）。 通过 AddListener{TEventArgs}(object, Action{TEventArgs}) 建立 Script 订阅， 通过 InvokeEvent{TEventArgs} 分发事件。
订阅方持有 AutoRemoveListenerHandle 精确退订；分发按 4 档优先级稳定执行 （注册表在注册期维护有序：Priority 升序、同档按注册顺序，分发侧零排序）。 分发期可靠性：自动检测并清理已销毁的 Unity 对象订阅者（死引用）； 支持 WithFilter 声明的订阅者过滤器实现精确投递； 可通过 executionMsLimit 开启分发耗时告警；支持 Cancel 取消传播。

快照语义：每趟分发基于注册表快照迭代——回调内退订/注册只影响后续分发，不干扰本趟。 重入约定：订阅者回调内同步发布其他参数实例（同型或异型）是安全的， 重入层使用按深度复用的独立迭代缓冲区；同一参数实例在分发期间不得被再次发布 （fail-fast 拒绝并输出错误日志），事件链请使用新实例或经 AesirTimer.NextFrame 延后发布。 性能计时与诊断采集仅对顶层分发生效。

作为 AesirModules 的子物体存在，由 GetOrAddChild{T} 懒加载创建。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`EventModule()`](#constructor-eventmodule) | — |

</div>

### EventModule() {#constructor-eventmodule}

``` csharp
public EventModule()
```

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Instance`](#property-instance) | 全局单例入口。 优先在已加载场景中查找预放置的实例；未找到时在 AesirModules（DDOL）下创建子物体。 |

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

### Instance {#property-instance}

全局单例入口。 优先在已加载场景中查找预放置的实例；未找到时在 AesirModules（DDOL）下创建子物体。

``` csharp
public static EventModule Instance { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AddListener(object, AesirEventArgs, Action<AesirEventArgs>)`](#method-addlistener-object-aesireventargs-action-aesireventargs) | 添加 Script 订阅（非泛型版）。通过事件参数实例推断类型。 默认优先级 Medium。 |
| [`AddListener(object, AesirEventArgs, SubscriberPriority, Action<AesirEventArgs>)`](#method-addlistener-object-aesireventargs-subscriberpriority-action-aesireventargs) | 添加 Script 订阅（非泛型版），指定优先级。 |
| [`AddListener(object, Action<TEventArgs>)`](#method-addlistener-object-action-teventargs) | 添加 Script 订阅。通过 Lambda 委托监听指定事件类型，返回自动移除句柄。 Dispose 或 using 块结束时自动注销。 默认优先级 Medium。 |
| [`AddListener(object, Action<TEventArgs>, SubscriberPriority)`](#method-addlistener-object-action-teventargs-subscriberpriority) | 添加 Script 订阅，指定优先级。返回自动移除句柄。 |
| [`InvokeEvent(object, TEventArgs)`](#method-invokeevent-object-teventargs) | 触发事件。取注册表订阅者，按注册期维护的优先级顺序依次调用。 走非创建式获取：订阅表是实例字段，因此"不存在实例"等价于"不存在任何订阅者"， 分发无事可做——若走 Instance 会在场景卸载等时机重建 DDOL 宿主 （详见 TryGetExisting 的说明）。 |
| [`RemoveListener(object)`](#method-removelistener-object) | 移除指定对象的全部 Script 订阅（O(n) 冷路径，批量逃生舱）。 用于退订方无句柄、且需要一次性清空某对象全部订阅的场景。 持有句柄时请逐个 Dispose；只需退订单个事件时用 RemoveListener{TEventArgs}(object)。 |
| [`RemoveListener(object)`](#method-removelistener-object) | 移除指定对象的全部 Script 订阅（O(n) 冷路径，批量逃生舱）。 用于退订方无句柄、且需要一次性清空某对象全部订阅的场景。 持有句柄时请逐个 Dispose；只需退订单个事件时用 RemoveListener{TEventArgs}(object)。 |

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

### AddListener(object, AesirEventArgs, Action<AesirEventArgs>) {#method-addlistener-object-aesireventargs-action-aesireventargs}

添加 Script 订阅（非泛型版）。通过事件参数实例推断类型。 默认优先级 Medium。

``` csharp
public static AutoRemoveListenerHandle AddListener(object subscriber, AesirEventArgs eventArgs, Action<AesirEventArgs> callback)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `subscriber` | `object` | — |
| `eventArgs` | `AesirEventArgs` | — |
| `callback` | `Action<AesirEventArgs>` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AutoRemoveListenerHandle` | — |

</div>

### AddListener(object, AesirEventArgs, SubscriberPriority, Action<AesirEventArgs>) {#method-addlistener-object-aesireventargs-subscriberpriority-action-aesireventargs}

添加 Script 订阅（非泛型版），指定优先级。

``` csharp
public static AutoRemoveListenerHandle AddListener(object subscriber, AesirEventArgs eventArgs, SubscriberPriority priority, Action<AesirEventArgs> callback)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `subscriber` | `object` | — |
| `eventArgs` | `AesirEventArgs` | — |
| `priority` | `SubscriberPriority` | — |
| `callback` | `Action<AesirEventArgs>` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AutoRemoveListenerHandle` | — |

</div>

### AddListener(object, Action<TEventArgs>) {#method-addlistener-object-action-teventargs}

添加 Script 订阅。通过 Lambda 委托监听指定事件类型，返回自动移除句柄。 Dispose 或 using 块结束时自动注销。 默认优先级 Medium。

``` csharp
public static AutoRemoveListenerHandle AddListener<TEventArgs>(object subscriber, Action<TEventArgs> callback)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `subscriber` | `object` | — |
| `callback` | `Action<TEventArgs>` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AutoRemoveListenerHandle` | — |

</div>

### AddListener(object, Action<TEventArgs>, SubscriberPriority) {#method-addlistener-object-action-teventargs-subscriberpriority}

添加 Script 订阅，指定优先级。返回自动移除句柄。

``` csharp
public static AutoRemoveListenerHandle AddListener<TEventArgs>(object subscriber, Action<TEventArgs> callback, SubscriberPriority priority)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `subscriber` | `object` | — |
| `callback` | `Action<TEventArgs>` | — |
| `priority` | `SubscriberPriority` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AutoRemoveListenerHandle` | — |

</div>

### InvokeEvent(object, TEventArgs) {#method-invokeevent-object-teventargs}

触发事件。取注册表订阅者，按注册期维护的优先级顺序依次调用。
走非创建式获取：订阅表是实例字段，因此"不存在实例"等价于"不存在任何订阅者"， 分发无事可做——若走 Instance 会在场景卸载等时机重建 DDOL 宿主 （详见 TryGetExisting 的说明）。

``` csharp
public static void InvokeEvent<TEventArgs>(object sender, TEventArgs eventArgs)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sender` | `object` | — |
| `eventArgs` | `TEventArgs` | — |

</div>

### RemoveListener(object) {#method-removelistener-object}

移除指定对象的全部 Script 订阅（O(n) 冷路径，批量逃生舱）。
用于退订方无句柄、且需要一次性清空某对象全部订阅的场景。 持有句柄时请逐个 Dispose；只需退订单个事件时用 RemoveListener{TEventArgs}(object)。

``` csharp
public static void RemoveListener(object subscriber)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `subscriber` | `object` | — |

</div>

### RemoveListener(object) {#method-removelistener-object}

移除指定对象的全部 Script 订阅（O(n) 冷路径，批量逃生舱）。
用于退订方无句柄、且需要一次性清空某对象全部订阅的场景。 持有句柄时请逐个 Dispose；只需退订单个事件时用 RemoveListener{TEventArgs}(object)。

``` csharp
public static void RemoveListener<TEventArgs>(object subscriber)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `subscriber` | `object` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
