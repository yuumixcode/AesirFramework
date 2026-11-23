---
title: AesirBasePanel
description: "Runestone.AesirModules.AesirBasePanel 的 API 文档"
---

# `AesirBasePanel`

!!! note ""

    - **种类:** `abstract class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.Component` → `UnityEngine.Behaviour` → `UnityEngine.MonoBehaviour` → `Sirenix.OdinInspector.SerializedMonoBehaviour` → `Runestone.AesirArchitecture.AesirMonoBehaviour` → `AesirBasePanel`

**实现接口:** `Sirenix.Serialization.ISupportsPrefabSerialization`，`Runestone.AesirModules.IUIPanel`，`UnityEngine.ISerializationCallbackReceiver`

## 声明

``` csharp
public abstract class AesirBasePanel : Runestone.AesirArchitecture.AesirMonoBehaviour, 
Sirenix.Serialization.ISupportsPrefabSerialization, 
Runestone.AesirModules.IUIPanel, 
UnityEngine.ISerializationCallbackReceiver
```

UI 面板基类。子类覆写生命周期虚方法，通过 Context 访问 Model/Service。

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Layer`](#property-layer) | 面板所在的 UI 层级。 |
| [`DestroyOnHide`](#property-destroyonhide) | 隐藏时是否销毁并回收实例。 |
| [`IsOpen`](#property-isopen) | 当前是否处于显示状态。由 UIModule 驱动。 |

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

### Layer {#property-layer}

面板所在的 UI 层级。

``` csharp
public UILayer Layer { get; }
```

### DestroyOnHide {#property-destroyonhide}

隐藏时是否销毁并回收实例。

``` csharp
public bool DestroyOnHide { get; }
```

### IsOpen {#property-isopen}

当前是否处于显示状态。由 UIModule 驱动。

``` csharp
public bool IsOpen { get; private set; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`OnClose()`](#method-onclose) | 面板经 HidePanel 受控销毁前调用（DestroyOnHide 为 true 的关闭路径）。 子类可覆写释放资源、解绑事件等。 |
| [`OnDestroy()`](#method-ondestroy) | 实例被销毁时的反清理回调（向 UIModule 注销注册表条目）。 |
| [`OnHide()`](#method-onhide) | 面板被隐藏时调用（默认不销毁实例）。子类可覆写清理显示状态。 |
| [`OnInit()`](#method-oninit) | 面板首次实例化后调用一次。子类可覆写进行一次性初始化。 |
| [`OnShow(object)`](#method-onshow-object) | 每次显示时调用（含首次）。默认实现为 gameObject.SetActive(true)。 |
| [`HideSelf()`](#method-hideself) | 便捷关闭自身，等价于 UIModule.Instance.HidePanel(GetType())。 走非创建式获取：面板销毁/场景卸载阶段 UIModule 可能已随之消失，此时"关闭自己"无对象可关， 静默返回即可——若走 Instance 会重建 DDOL 宿主（详见 TryGetExisting 的说明）。 |

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

### OnClose() {#method-onclose}

面板经 HidePanel 受控销毁前调用（DestroyOnHide 为 true 的关闭路径）。 子类可覆写释放资源、解绑事件等。

**备注**

仅受控销毁路径调用本方法；场景卸载、外部 Destroy 等非受控销毁只触发 OnDestroy。 因此事件解绑、订阅释放等必须放在 OnDestroy 中（或两处都写）， 仅写在本方法会在场景卸载时泄漏。

``` csharp
protected virtual void OnClose()
```

### OnDestroy() {#method-ondestroy}

实例被销毁时的反清理回调（向 UIModule 注销注册表条目）。

**备注**

子类覆写本方法时必须调用 base.OnDestroy()：Unity 的消息派发只调用最派生类型的声明， 不调用 base 会使注册表残留已销毁实例。注册表另有自愈兜底 （命中已销毁条目时按未注册处理并重建），但仍应保留 base 调用以维持时序与开销。

``` csharp
protected virtual void OnDestroy()
```

### OnHide() {#method-onhide}

面板被隐藏时调用（默认不销毁实例）。子类可覆写清理显示状态。

``` csharp
protected virtual void OnHide()
```

### OnInit() {#method-oninit}

面板首次实例化后调用一次。子类可覆写进行一次性初始化。

``` csharp
protected virtual void OnInit()
```

### OnShow(object) {#method-onshow-object}

每次显示时调用（含首次）。默认实现为 gameObject.SetActive(true)。

``` csharp
protected virtual void OnShow(object payload)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `payload` | `object` | 外部传入的数据。 |

</div>

### HideSelf() {#method-hideself}

便捷关闭自身，等价于 UIModule.Instance.HidePanel(GetType())。
走非创建式获取：面板销毁/场景卸载阶段 UIModule 可能已随之消失，此时"关闭自己"无对象可关， 静默返回即可——若走 Instance 会重建 DDOL 宿主（详见 TryGetExisting 的说明）。

``` csharp
protected void HideSelf()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
