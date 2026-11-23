---
title: AesirBaseWindow
description: "Runestone.AesirModules.AesirBaseWindow 的 API 文档"
---

# `AesirBaseWindow`

!!! note ""

    - **种类:** `abstract class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.Component` → `UnityEngine.Behaviour` → `UnityEngine.MonoBehaviour` → `Sirenix.OdinInspector.SerializedMonoBehaviour` → `Runestone.AesirArchitecture.AesirMonoBehaviour` → `AesirBaseWindow`

**实现接口:** `Sirenix.Serialization.ISupportsPrefabSerialization`，`Runestone.AesirModules.IUIWindow`，`UnityEngine.ISerializationCallbackReceiver`

## 声明

``` csharp
public abstract class AesirBaseWindow : Runestone.AesirArchitecture.AesirMonoBehaviour, 
Sirenix.Serialization.ISupportsPrefabSerialization, 
Runestone.AesirModules.IUIWindow, 
UnityEngine.ISerializationCallbackReceiver
```

Canvas 根 UI 窗口基类。窗口预制体根节点自带 Canvas（独立渲染根）， 由 UIModule 实例化后直接挂载到 UIRoot 下，按 SortingOrder 排序， 默认恒在全部面板层之上。子类覆写生命周期虚方法，通过 Context 访问 Model/Service。

**备注**

预制体内部结构约定： XxxWindow（根：Canvas + CanvasScaler + GraphicRaycaster + 窗口脚本） ├── Mask 蒙版：Image 全屏拉伸（raycastTarget 拦截其下一切 UI 的射线）+ 可选 Button（承接点击） └── Content 实际 UI 元素容器（框架不触碰，仅作为结构约定） 蒙版显隐由 UIModule 按 UIMaskMode 统一调度； 蒙版点击经 Button 接线回调 OnMaskClicked。

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Canvas`](#property-canvas) | — |
| [`DestroyOnHide`](#property-destroyonhide) | 关闭时是否销毁并回收实例。 |
| [`IsOpen`](#property-isopen) | 当前是否处于打开状态。由 UIModule 驱动。 |
| [`SortingOrder`](#property-sortingorder) | 窗口根 Canvas 的 sortingOrder。 |

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

### DestroyOnHide {#property-destroyonhide}

关闭时是否销毁并回收实例。

``` csharp
public bool DestroyOnHide { get; }
```

### IsOpen {#property-isopen}

当前是否处于打开状态。由 UIModule 驱动。

``` csharp
public bool IsOpen { get; private set; }
```

### SortingOrder {#property-sortingorder}

窗口根 Canvas 的 sortingOrder。

``` csharp
public int SortingOrder { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`OnClose()`](#method-onclose) | 窗口经 CloseWindow 受控销毁前调用（DestroyOnHide 为 true 的关闭路径）。 子类可覆写释放资源、解绑事件等。 |
| [`OnDestroy()`](#method-ondestroy) | 实例被销毁时的反清理回调（向 UIModule 注销注册表条目并重算蒙版）。 |
| [`OnHide()`](#method-onhide) | 窗口被隐藏时调用（默认不销毁实例）。子类可覆写清理显示状态。 |
| [`OnInit()`](#method-oninit) | 窗口首次实例化后调用一次。子类可覆写进行一次性初始化。 |
| [`OnMaskClicked()`](#method-onmaskclicked) | 蒙版被点击时回调。默认按 closeOnMaskClick 决定是否关闭本窗口； 子类可覆写实现自定义行为（如提示"先完成当前操作"）。 |
| [`OnShow(object)`](#method-onshow-object) | 每次打开时调用（含首次）。默认实现为 gameObject.SetActive(true)。 |
| [`CloseSelf()`](#method-closeself) | 便捷关闭自身，等价于 UIModule.Instance.CloseWindow(GetType())。 走非创建式获取：窗口销毁/场景卸载阶段 UIModule 可能已随之消失，此时"关闭自己"无对象可关， 静默返回即可——若走 Instance 会重建 DDOL 宿主（详见 TryGetExisting 的说明）。 |

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

窗口经 CloseWindow 受控销毁前调用（DestroyOnHide 为 true 的关闭路径）。 子类可覆写释放资源、解绑事件等。

**备注**

仅受控销毁路径调用本方法；场景卸载、外部 Destroy 等非受控销毁只触发 OnDestroy。 因此事件解绑、订阅释放等必须放在 OnDestroy 中（或两处都写）， 仅写在本方法会在场景卸载时泄漏。

``` csharp
protected virtual void OnClose()
```

### OnDestroy() {#method-ondestroy}

实例被销毁时的反清理回调（向 UIModule 注销注册表条目并重算蒙版）。

**备注**

子类覆写本方法时必须调用 base.OnDestroy()：Unity 的消息派发只调用最派生类型的声明， 不调用 base 会使注册表残留已销毁实例。注册表另有自愈兜底 （命中已销毁条目时按未注册处理并重建），但仍应保留 base 调用以维持时序与开销。

``` csharp
protected virtual void OnDestroy()
```

### OnHide() {#method-onhide}

窗口被隐藏时调用（默认不销毁实例）。子类可覆写清理显示状态。

``` csharp
protected virtual void OnHide()
```

### OnInit() {#method-oninit}

窗口首次实例化后调用一次。子类可覆写进行一次性初始化。

``` csharp
protected virtual void OnInit()
```

### OnMaskClicked() {#method-onmaskclicked}

蒙版被点击时回调。默认按 closeOnMaskClick 决定是否关闭本窗口； 子类可覆写实现自定义行为（如提示"先完成当前操作"）。

``` csharp
protected virtual void OnMaskClicked()
```

### OnShow(object) {#method-onshow-object}

每次打开时调用（含首次）。默认实现为 gameObject.SetActive(true)。

``` csharp
protected virtual void OnShow(object payload)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `payload` | `object` | 外部传入的数据。 |

</div>

### CloseSelf() {#method-closeself}

便捷关闭自身，等价于 UIModule.Instance.CloseWindow(GetType())。
走非创建式获取：窗口销毁/场景卸载阶段 UIModule 可能已随之消失，此时"关闭自己"无对象可关， 静默返回即可——若走 Instance 会重建 DDOL 宿主（详见 TryGetExisting 的说明）。

``` csharp
protected void CloseSelf()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
