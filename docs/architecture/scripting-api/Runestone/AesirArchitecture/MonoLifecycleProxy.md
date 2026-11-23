---
title: MonoLifecycleProxy
description: "Runestone.AesirArchitecture.MonoLifecycleProxy 的 API 文档"
---

# `MonoLifecycleProxy`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.Component` → `UnityEngine.Behaviour` → `UnityEngine.MonoBehaviour` → `Sirenix.OdinInspector.SerializedMonoBehaviour` → `Runestone.AesirArchitecture.AesirMonoBehaviour` → `MonoLifecycleProxy`

**实现接口:** `Sirenix.Serialization.ISupportsPrefabSerialization`，`UnityEngine.ISerializationCallbackReceiver`

## 声明

``` csharp
[DisallowMultipleComponent]
[DefaultExecutionOrder]
public sealed class MonoLifecycleProxy : Runestone.AesirArchitecture.AesirMonoBehaviour, 
Sirenix.Serialization.ISupportsPrefabSerialization, 
UnityEngine.ISerializationCallbackReceiver
```

Mono 生命周期事件代理。作为全局单例挂载在 [Aesir Architecture] GameObject 上， 将 Unity 原生生命周期回调和自定义 PlayerLoop 阶段统一为可订阅的有序事件。

**备注**

通过 Instance 访问实例方法 AddListener、RemoveListener 等。

可排序监听列表：每个事件维护一个 List{T} 存储 ListenerEntry， 使用 Order + InsertionIndex 稳定排序，按排序结果依次调用回调。 与 AesirPlayerLoop 的排序机制一致。

快照语义：与原生 C# 多播委托一致，每趟遍历基于调用开始时的监听列表进行。 回调执行中发起的 AddListener / RemoveListener 等变更进入挂起队列， 本趟结束后按发生顺序统一应用——调用期间新增的监听下一趟才生效，被移除的监听若尚未执行 仍会在本趟执行一次（随后失效），且增删不会导致其他监听被跳过或重复执行。

自动取消订阅：通过 Register(MonoBehaviour) 注册的 MonoBehaviour， 其所有监听句柄会绑定到目标 GameObject 的 OnDestroy 事件，物体销毁时自动从代理中取消订阅。 非 MonoBehaviour 对象通过 Register(object) 注册，返回组合句柄由调用方管理生命周期。

PlayerLoop 集成：BeforeUpdate 和 AfterUpdate 通过注册到 AesirPlayerLoop 实现，Awake 时注册、OnDestroy 时注销。

ICustomXXX 自动注册：调用 RegisterAuto(object) 传入实现了任意 ICustomXXX 接口的对象（MonoBehaviour 或纯 C# 类均可），代理会自动扫描并注册所有对应方法到匹配的生命周期事件。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`MonoLifecycleProxy()`](#constructor-monolifecycleproxy) | — |

</div>

### MonoLifecycleProxy() {#constructor-monolifecycleproxy}

``` csharp
public MonoLifecycleProxy()
```

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Instance`](#property-instance) | 获取全局唯一的 MonoLifecycleProxy 实例。 |

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

获取全局唯一的 MonoLifecycleProxy 实例。

**备注**

优先在已加载场景中查找预放置的实例；未找到时通过 GetOrAddComponent{T} 挂载到 [Aesir Architecture] GameObject 上，复用架构宿主对象。

``` csharp
public static MonoLifecycleProxy Instance { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AddListener(MonoLifecycleEvent, Action, int)`](#method-addlistener-monolifecycleevent-action-int) | 添加生命周期事件监听，返回可自动移除的监听句柄。 |
| [`RegisterAuto(object)`](#method-registerauto-object) | 快捷注册。扫描对象实现的所有 ICustomXXX 接口， 将对应方法自动注册到匹配的生命周期事件中，返回组合句柄。 |
| [`ClearAllListeners()`](#method-clearalllisteners) | 清空所有事件的监听者 |
| [`RemoveListener(MonoLifecycleEvent, Action)`](#method-removelistener-monolifecycleevent-action) | 移除指定事件的监听者 |
| [`Register(object)`](#method-register-object) | 快捷注册（任意对象）。扫描实现的所有 ICustomXXX 接口， 将对应方法自动注册到匹配的生命周期事件中。 |
| [`Register(MonoBehaviour)`](#method-register-monobehaviour) | 快捷注册（MonoBehaviour 专用）。扫描实现的所有 ICustomXXX 接口， 将对应方法自动注册到匹配的生命周期事件中，并绑定到目标 GameObject 的 OnDestroy 自动取消订阅。 |

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

### AddListener(MonoLifecycleEvent, Action, int) {#method-addlistener-monolifecycleevent-action-int}

添加生命周期事件监听，返回可自动移除的监听句柄。

**备注**

快照语义：在生命周期回调执行中调用时，本趟遍历不会包含新监听，变更延后至本趟结束统一应用。

``` csharp
public AutoRemoveListenerHandle AddListener(MonoLifecycleEvent evt, Action callback, int order = 0)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `evt` | `MonoLifecycleEvent` | 要订阅的生命周期事件类型 |
| `callback` | `Action` | 事件触发时执行的回调委托 |
| `order` | `int` | 执行优先级，值越小越先执行；同 order 时按注册顺序执行 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AutoRemoveListenerHandle` | 用于后续自动移除该监听的句柄 |

</div>

### RegisterAuto(object) {#method-registerauto-object}

快捷注册。扫描对象实现的所有 ICustomXXX 接口， 将对应方法自动注册到匹配的生命周期事件中，返回组合句柄。

**备注**

对于 MonoBehaviour，可使用静态 Register(MonoBehaviour) 替代， 后者会额外将句柄绑定到 GameObject 的 OnDestroy 自动取消订阅。 对于非 MonoBehaviour 对象，调用方需自行管理返回句柄的生命周期。

``` csharp
public AutoRemoveListenerHandle RegisterAuto(object obj)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `obj` | `object` | 实现了任意 ICustomXXX 接口的对象（MonoBehaviour 或纯 C# 类均可） |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AutoRemoveListenerHandle` | 组合句柄，Dispose 时一次性移除本次注册的所有监听；若对象未实现任何接口则返回默认句柄 |

</div>

### ClearAllListeners() {#method-clearalllisteners}

清空所有事件的监听者

**备注**

快照语义：在生命周期回调执行中调用时，本趟遍历继续执行完毕（基于旧列表）， 此前累积的挂起变更一并丢弃，之后新增的监听在本趟结束时正常应用。

``` csharp
public void ClearAllListeners()
```

### RemoveListener(MonoLifecycleEvent, Action) {#method-removelistener-monolifecycleevent-action}

移除指定事件的监听者

**备注**

快照语义：在生命周期回调执行中调用时，被移除的监听若尚未执行仍会在本趟执行一次，随后失效。

``` csharp
public void RemoveListener(MonoLifecycleEvent evt, Action callback)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `evt` | `MonoLifecycleEvent` | 目标生命周期事件类型 |
| `callback` | `Action` | 要移除的回调委托，必须与注册时传入的实例相同 |

</div>

### Register(object) {#method-register-object}

快捷注册（任意对象）。扫描实现的所有 ICustomXXX 接口， 将对应方法自动注册到匹配的生命周期事件中。

**备注**

适用于非 MonoBehaviour 的纯 C# 类。调用方负责在适当时机 Dispose 返回的句柄以取消订阅， 或配合 RemoveListenerExtensions 绑定到其他 Unity 生命周期事件。

``` csharp
public static AutoRemoveListenerHandle Register(object obj)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `obj` | `object` | 实现了任意 ICustomXXX 接口的对象 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AutoRemoveListenerHandle` | 组合句柄，Dispose 时一次性移除本次注册的所有监听 |

</div>

### Register(MonoBehaviour) {#method-register-monobehaviour}

快捷注册（MonoBehaviour 专用）。扫描实现的所有 ICustomXXX 接口， 将对应方法自动注册到匹配的生命周期事件中，并绑定到目标 GameObject 的 OnDestroy 自动取消订阅。

**备注**

适用于非 MonoBehaviour 的纯 C# 类。调用方负责在适当时机 Dispose 返回的句柄以取消订阅， 或配合 RemoveListenerExtensions 绑定到其他 Unity 生命周期事件。

``` csharp
public static void Register(MonoBehaviour mono)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `mono` | `MonoBehaviour` | 实现了任意 ICustomXXX 接口的 MonoBehaviour |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
