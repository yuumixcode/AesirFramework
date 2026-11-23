---
title: RemoveListenerOnSceneUnloadedTrigger
description: "Runestone.AesirArchitecture.RemoveListenerOnSceneUnloadedTrigger 的 API 文档"
---

# `RemoveListenerOnSceneUnloadedTrigger`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.Component` → `UnityEngine.Behaviour` → `UnityEngine.MonoBehaviour` → `Sirenix.OdinInspector.SerializedMonoBehaviour` → `Runestone.AesirArchitecture.AesirMonoBehaviour` → `RemoveListenerOnSceneUnloadedTrigger`

**实现接口:** `Sirenix.Serialization.ISupportsPrefabSerialization`，`UnityEngine.ISerializationCallbackReceiver`

## 声明

``` csharp
[DisallowMultipleComponent]
public sealed class RemoveListenerOnSceneUnloadedTrigger : Runestone.AesirArchitecture.AesirMonoBehaviour, 
Sirenix.Serialization.ISupportsPrefabSerialization, 
UnityEngine.ISerializationCallbackReceiver
```

任意场景卸载时自动移除该场景注册的监听。按场景句柄（handle）分桶， 场景 A 卸载不会误杀场景 B 的监听。
挂载在 [Aesir Architecture] GameObject 上，通过 Instance 访问。

**备注**

相比按场景名分桶，句柄分桶保证：不同路径下的同名场景各持唯一句柄、互不共享桶； 场景卸载后重新加载会获得新句柄，不存在旧桶残留。
通过 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] 在每次域加载时重置静态单例字段，确保在编辑器关闭 Domain Reload 时不残留上一次 Play 会话的旧引用。

在 Awake 中订阅 SceneManager.sceneUnloaded，在 OnDestroy 中取消订阅， 避免组件销毁后仍接收场景卸载事件。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`RemoveListenerOnSceneUnloadedTrigger()`](#constructor-removelisteneronsceneunloadedtrigger) | — |

</div>

### RemoveListenerOnSceneUnloadedTrigger() {#constructor-removelisteneronsceneunloadedtrigger}

``` csharp
public RemoveListenerOnSceneUnloadedTrigger()
```

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Instance`](#property-instance) | 获取全局唯一的场景卸载监听移除器实例 |

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

获取全局唯一的场景卸载监听移除器实例

**备注**

优先在已加载场景中查找预放置的实例；未找到时通过 GetOrAddComponent{T} 挂载到 [Aesir Architecture] GameObject 上，复用架构宿主对象。

``` csharp
public static RemoveListenerOnSceneUnloadedTrigger Instance { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AddRemoveListenerHandle(AutoRemoveListenerHandle)`](#method-addremovelistenerhandle-autoremovelistenerhandle) | 添加监听句柄，使其在当前活动场景卸载时自动移除 |
| [`AddRemoveListenerHandle(Scene, AutoRemoveListenerHandle)`](#method-addremovelistenerhandle-scene-autoremovelistenerhandle) | 添加监听句柄，使其在指定场景卸载时自动移除 |

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

### AddRemoveListenerHandle(AutoRemoveListenerHandle) {#method-addremovelistenerhandle-autoremovelistenerhandle}

添加监听句柄，使其在当前活动场景卸载时自动移除

**备注**

以调用时的 SceneManager.GetActiveScene() 作为分桶依据。 additive 多场景流程中活动场景不一定是监听者实际所在场景，此时请改用 AddRemoveListenerHandle(Scene, AutoRemoveListenerHandle) 显式指定归属场景。

``` csharp
public void AddRemoveListenerHandle(AutoRemoveListenerHandle handle)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `handle` | `AutoRemoveListenerHandle` | 要注册的自动移除监听句柄，封装了目标监听与移除委托 |

</div>

### AddRemoveListenerHandle(Scene, AutoRemoveListenerHandle) {#method-addremovelistenerhandle-scene-autoremovelistenerhandle}

添加监听句柄，使其在指定场景卸载时自动移除

**备注**

以 scene 的 handle 作为分桶键，将句柄归入该场景的集合。 当对应场景卸载时，仅移除该桶中的监听。additive 多场景流程中应传入监听者实际所在场景， 避免误入活动场景的桶导致监听被提前移除或永不清理。

``` csharp
public void AddRemoveListenerHandle(Scene scene, AutoRemoveListenerHandle handle)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `scene` | `Scene` | 监听归属的场景，按其 handle 分桶 |
| `handle` | `AutoRemoveListenerHandle` | 要注册的自动移除监听句柄，封装了目标监听与移除委托 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
