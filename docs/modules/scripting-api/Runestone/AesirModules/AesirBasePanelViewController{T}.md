---
title: AesirBasePanelViewController<T>
description: "Runestone.AesirModules.AesirBasePanelViewController<T> 的 API 文档"
---

# `AesirBasePanelViewController<T>`

!!! note ""

    - **种类:** `abstract class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.Component` → `UnityEngine.Behaviour` → `UnityEngine.MonoBehaviour` → `Sirenix.OdinInspector.SerializedMonoBehaviour` → `Runestone.AesirArchitecture.AesirMonoBehaviour` → `Runestone.AesirModules.AesirBasePanel` → `AesirBasePanelViewController<T>`

**实现接口:** `Runestone.AesirArchitecture.ICanExecuteQuery`，`Runestone.AesirArchitecture.IController`，`Sirenix.Serialization.ISupportsPrefabSerialization`，`Runestone.AesirModules.IUIPanel`，`Runestone.AesirArchitecture.IContextHolder`，`UnityEngine.ISerializationCallbackReceiver`，`Runestone.AesirArchitecture.ICanGetModel`，`Runestone.AesirArchitecture.ICanGetService`，`Runestone.AesirArchitecture.ICanExecuteCommand`

**类型参数**

- `T` — 面板关联的 Context 类型，须继承 AbstractContext{T} 并具有无参构造函数。

## 声明

``` csharp
public abstract class AesirBasePanelViewController<T> : Runestone.AesirModules.AesirBasePanel, 
Runestone.AesirArchitecture.ICanExecuteQuery, 
Runestone.AesirArchitecture.IController, 
Sirenix.Serialization.ISupportsPrefabSerialization, 
Runestone.AesirModules.IUIPanel, 
Runestone.AesirArchitecture.IContextHolder, 
UnityEngine.ISerializationCallbackReceiver, 
Runestone.AesirArchitecture.ICanGetModel, 
Runestone.AesirArchitecture.ICanGetService, 
Runestone.AesirArchitecture.ICanExecuteCommand where T : new(), Runestone.AesirArchitecture.AbstractContext<T>
```

面板视图控制器基类（MVC 模式的 Controller 层，兼 View 职责）。
泛型参数 T 指定面板关联的 Context 类型， Context 作为 Model 和 Service 的聚合容器，在面板与业务逻辑之间充当数据中转站。

继承链：AesirBasePanelViewController{T} → AesirBasePanel → AesirMonoBehaviour。 子类通过 Context 属性（由 IController 能力组合提供）访问 Context 中持有的 Model / Service， 并可直接执行 Command / Query。
典型用法（通过 ICanExecuteCommand / ICanExecuteQuery 扩展方法分发命令与查询）： public class MyPanelViewController : AesirBasePanelViewController<MyPanelContext> { protected override void OnShow(object payload) { var model = this.GetModel<MyModel>(); UpdateUI(model); this.ExecuteCommand(new RefreshCommand()); } }

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `destroyCancellationToken` | — | `MonoBehaviour` |
| `gameObject` | — | `Component` |
| `hideFlags` | — | `Object` |
| `transform` | — | `Component` |
| `Layer` | 面板所在的 UI 层级。 | `AesirBasePanel` |
| `DestroyOnHide` | 隐藏时是否销毁并回收实例。 | `AesirBasePanel` |
| `IsOpen` | 当前是否处于显示状态。由 UIModule 驱动。 | `AesirBasePanel` |
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

## 方法

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
| `OnClose()` | 面板经 HidePanel 受控销毁前调用（DestroyOnHide 为 true 的关闭路径）。 子类可覆写释放资源、解绑事件等。 | `AesirBasePanel` |
| `OnDestroy()` | 实例被销毁时的反清理回调（向 UIModule 注销注册表条目）。 | `AesirBasePanel` |
| `OnHide()` | 面板被隐藏时调用（默认不销毁实例）。子类可覆写清理显示状态。 | `AesirBasePanel` |
| `OnInit()` | 面板首次实例化后调用一次。子类可覆写进行一次性初始化。 | `AesirBasePanel` |
| `OnShow(object)` | 每次显示时调用（含首次）。默认实现为 gameObject.SetActive(true)。 | `AesirBasePanel` |
| `HideSelf()` | 便捷关闭自身，等价于 UIModule.Instance.HidePanel(GetType())。 走非创建式获取：面板销毁/场景卸载阶段 UIModule 可能已随之消失，此时"关闭自己"无对象可关， 静默返回即可——若走 Instance 会重建 DDOL 宿主（详见 TryGetExisting 的说明）。 | `AesirBasePanel` |
| `StartCoroutine_Auto(IEnumerator)` | — | `MonoBehaviour` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
