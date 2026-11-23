---
title: RemoveListenerExtensions
description: "Runestone.AesirArchitecture.RemoveListenerExtensions 的 API 文档"
---

# `RemoveListenerExtensions`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `RemoveListenerExtensions`

## 声明

``` csharp
[Extension]
public static class RemoveListenerExtensions
```

事件监听器自动移除扩展方法类，用于绑定移除操作到 Unity 生命周期

**备注**

提供一组扩展方法，将 AutoRemoveListenerHandle 绑定到 Unity 生命周期事件 （OnDestroy / OnDisable / SceneUnloaded），实现监听的自动清理， 避免因忘记手动移除监听而导致的内存泄漏。

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`RemoveListenerWhenGameObjectOnDestroyed(AutoRemoveListenerHandle, GameObject)`](#method-removelistenerwhengameobjectondestroyed-autoremovelistenerhandle-gameobject) | 当指定的 GameObject 被销毁时自动移除监听 |
| [`RemoveListenerWhenGameObjectOnDestroyed(AutoRemoveListenerHandle, MonoBehaviour)`](#method-removelistenerwhengameobjectondestroyed-autoremovelistenerhandle-monobehaviour) | 当指定的 MonoBehaviour 所属 GameObject 被销毁时自动移除监听 |
| [`RemoveListenerWhenGameObjectOnDisable(AutoRemoveListenerHandle, GameObject)`](#method-removelistenerwhengameobjectondisable-autoremovelistenerhandle-gameobject) | 当指定的 GameObject 被禁用（OnDisable）时自动移除监听 |
| [`RemoveListenerWhenGameObjectOnDisable(AutoRemoveListenerHandle, MonoBehaviour)`](#method-removelistenerwhengameobjectondisable-autoremovelistenerhandle-monobehaviour) | 当指定的 MonoBehaviour 所属 GameObject 被禁用（OnDisable）时自动移除监听 |
| [`RemoveListenerWhenOnSceneUnloaded(AutoRemoveListenerHandle)`](#method-removelistenerwhenonsceneunloaded-autoremovelistenerhandle) | 当当前活动场景卸载时自动移除监听 |
| [`RemoveListenerWhenOnSceneUnloaded(AutoRemoveListenerHandle, MonoBehaviour)`](#method-removelistenerwhenonsceneunloaded-autoremovelistenerhandle-monobehaviour) | 当监听者所在 GameObject 所属的场景卸载时自动移除监听 |
| [`RemoveListenerWhenOnSceneUnloaded(AutoRemoveListenerHandle, Scene)`](#method-removelistenerwhenonsceneunloaded-autoremovelistenerhandle-scene) | 当指定场景卸载时自动移除监听 |

</div>

**继承的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

### RemoveListenerWhenGameObjectOnDestroyed(AutoRemoveListenerHandle, GameObject) {#method-removelistenerwhengameobjectondestroyed-autoremovelistenerhandle-gameobject}

当指定的 GameObject 被销毁时自动移除监听

``` csharp
[Extension]
[Ext] public static void RemoveListenerWhenGameObjectOnDestroyed(this AutoRemoveListenerHandle removeListener, GameObject gameObject)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `removeListener` | `AutoRemoveListenerHandle` | 要绑定到销毁事件的监听句柄 |
| `gameObject` | `GameObject` | 监听生命周期所依附的 GameObject |

</div>

### RemoveListenerWhenGameObjectOnDestroyed(AutoRemoveListenerHandle, MonoBehaviour) {#method-removelistenerwhengameobjectondestroyed-autoremovelistenerhandle-monobehaviour}

当指定的 MonoBehaviour 所属 GameObject 被销毁时自动移除监听

``` csharp
[Extension]
[Ext] public static void RemoveListenerWhenGameObjectOnDestroyed(this AutoRemoveListenerHandle removeListener, MonoBehaviour mono)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `removeListener` | `AutoRemoveListenerHandle` | 要绑定到销毁事件的监听句柄 |
| `mono` | `MonoBehaviour` | 监听生命周期所依附的 MonoBehaviour |

</div>

### RemoveListenerWhenGameObjectOnDisable(AutoRemoveListenerHandle, GameObject) {#method-removelistenerwhengameobjectondisable-autoremovelistenerhandle-gameobject}

当指定的 GameObject 被禁用（OnDisable）时自动移除监听

``` csharp
[Extension]
[Ext] public static void RemoveListenerWhenGameObjectOnDisable(this AutoRemoveListenerHandle removeListener, GameObject gameObject)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `removeListener` | `AutoRemoveListenerHandle` | 要绑定到禁用事件的监听句柄 |
| `gameObject` | `GameObject` | 监听生命周期所依附的 GameObject |

</div>

### RemoveListenerWhenGameObjectOnDisable(AutoRemoveListenerHandle, MonoBehaviour) {#method-removelistenerwhengameobjectondisable-autoremovelistenerhandle-monobehaviour}

当指定的 MonoBehaviour 所属 GameObject 被禁用（OnDisable）时自动移除监听

``` csharp
[Extension]
[Ext] public static void RemoveListenerWhenGameObjectOnDisable(this AutoRemoveListenerHandle removeListener, MonoBehaviour mono)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `removeListener` | `AutoRemoveListenerHandle` | 要绑定到禁用事件的监听句柄 |
| `mono` | `MonoBehaviour` | 监听生命周期所依附的 MonoBehaviour |

</div>

### RemoveListenerWhenOnSceneUnloaded(AutoRemoveListenerHandle) {#method-removelistenerwhenonsceneunloaded-autoremovelistenerhandle}

当当前活动场景卸载时自动移除监听

**备注**

使用 Instance 单例进行管理， 该单例按场景句柄分桶存储监听句柄，在对应场景卸载时批量移除该场景下的所有监听， 避免全局遍历带来的性能开销。
additive 多场景流程中活动场景不一定是监听者实际所在场景， 此时请改用 RemoveListenerWhenOnSceneUnloaded(AutoRemoveListenerHandle, Scene) 或 RemoveListenerWhenOnSceneUnloaded(AutoRemoveListenerHandle, MonoBehaviour) 显式指定归属场景。

``` csharp
[Extension]
[Ext] public static void RemoveListenerWhenOnSceneUnloaded(this AutoRemoveListenerHandle removeListener)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `removeListener` | `AutoRemoveListenerHandle` | 要绑定到场景卸载事件的监听句柄 |

</div>

### RemoveListenerWhenOnSceneUnloaded(AutoRemoveListenerHandle, MonoBehaviour) {#method-removelistenerwhenonsceneunloaded-autoremovelistenerhandle-monobehaviour}

当监听者所在 GameObject 所属的场景卸载时自动移除监听

**备注**

以 mono 所在 GameObject 的场景作为归属场景分桶， 适合 additive 多场景流程：即使当前活动场景并非监听者所在场景，也能正确归桶。

``` csharp
[Extension]
[Ext] public static void RemoveListenerWhenOnSceneUnloaded(this AutoRemoveListenerHandle removeListener, MonoBehaviour mono)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `removeListener` | `AutoRemoveListenerHandle` | 要绑定到场景卸载事件的监听句柄 |
| `mono` | `MonoBehaviour` | 监听者，按其 GameObject 所在场景归桶 |

</div>

### RemoveListenerWhenOnSceneUnloaded(AutoRemoveListenerHandle, Scene) {#method-removelistenerwhenonsceneunloaded-autoremovelistenerhandle-scene}

当指定场景卸载时自动移除监听

**备注**

按指定场景的 handle 分桶，适合 additive 多场景流程： 显式传入监听者实际所在的场景，避免无参版本按活动场景归桶导致的误清理。

``` csharp
[Extension]
[Ext] public static void RemoveListenerWhenOnSceneUnloaded(this AutoRemoveListenerHandle removeListener, Scene scene)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `removeListener` | `AutoRemoveListenerHandle` | 要绑定到场景卸载事件的监听句柄 |
| `scene` | `Scene` | 监听归属的场景，卸载该场景时移除监听 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
