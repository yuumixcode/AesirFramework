---
title: UIRoot
description: "Runestone.AesirModules.UIRoot 的 API 文档"
---

# `UIRoot`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.Component` → `UnityEngine.Behaviour` → `UnityEngine.MonoBehaviour` → `Sirenix.OdinInspector.SerializedMonoBehaviour` → `Runestone.AesirArchitecture.AesirMonoBehaviour` → `UIRoot`

**实现接口:** `Sirenix.Serialization.ISupportsPrefabSerialization`，`UnityEngine.ISerializationCallbackReceiver`

## 声明

``` csharp
[DisallowMultipleComponent]
[DefaultExecutionOrder]
public class UIRoot : Runestone.AesirArchitecture.AesirMonoBehaviour, 
Sirenix.Serialization.ISupportsPrefabSerialization, 
UnityEngine.ISerializationCallbackReceiver
```

UI 根节点组件。 负责创建 UICamera、EventSystem、分层 Canvas 以及应用 Canvas 统一配置。

**备注**

是否加入 DontDestroyOnLoad 场景由序列化字段 dontDestroyOnLoad 统一控制， 场景预放置与运行时创建两种来源共用同一份决策（默认勾选，跨场景持久；预放置为子物体时本字段不参与判断，DDOL 跟随宿主）。 取消勾选时实例保留在所在场景、随场景卸载销毁——必须自行处理多场景叠加（Additive）加载下的生命周期管理。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`UIRoot()`](#constructor-uiroot) | — |

</div>

### UIRoot() {#constructor-uiroot}

``` csharp
public UIRoot()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`DefaultCanvasConfigPath`](#field-defaultcanvasconfigpath) | — |

</div>

### DefaultCanvasConfigPath {#field-defaultcanvasconfigpath}

``` csharp
public const string DefaultCanvasConfigPath = "Assets/Resources/UIConfig_Default/Default_UICanvasConfig.asset";
```

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`UICamera`](#property-uicamera) | UI 专用正交相机（由 EnsureUICamera 构建）。 |
| [`WorldSpaceCanvasConfig`](#property-worldspacecanvasconfig) | 世界空间 Canvas 的统一配置（WorldSpaceUICanvas 接线共用）。 未设置时回退 CreateWorldSpaceDefault 的内存预设。 与 CanvasConfig（仅管屏幕空间层 Canvas 与窗口根 Canvas）分属两条独立配置链。 |
| [`CreateInputModule`](#property-createinputmodule) | 自定义输入模块创建回调。 由 Runestone.AesirModules.InputSystem 程序集在 InputSystem 启用时注册， 使用 InputSystemUIInputModule 替代默认的 StandaloneInputModule。 为 null 时使用 StandaloneInputModule。 |
| [`Instance`](#property-instance) | — |

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

### UICamera {#property-uicamera}

UI 专用正交相机（由 EnsureUICamera 构建）。

``` csharp
public Camera UICamera { get; }
```

### WorldSpaceCanvasConfig {#property-worldspacecanvasconfig}

世界空间 Canvas 的统一配置（WorldSpaceUICanvas 接线共用）。 未设置时回退 CreateWorldSpaceDefault 的内存预设。 与 CanvasConfig（仅管屏幕空间层 Canvas 与窗口根 Canvas）分属两条独立配置链。

``` csharp
public UICanvasConfigSO WorldSpaceCanvasConfig { get; }
```

### CreateInputModule {#property-createinputmodule}

自定义输入模块创建回调。 由 Runestone.AesirModules.InputSystem 程序集在 InputSystem 启用时注册， 使用 InputSystemUIInputModule 替代默认的 StandaloneInputModule。 为 null 时使用 StandaloneInputModule。

``` csharp
public static Action<GameObject> CreateInputModule { get; set; }
```

### Instance {#property-instance}

``` csharp
public static UIRoot Instance { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetLayerRoot(UILayer)`](#method-getlayerroot-uilayer) | 获取指定层级的根 Transform。层 Canvas 引用缺失（子物体被删除或引用丢失）时记录错误并返回 null； 子物体被重命名不受影响（引用与名称解耦）。 |
| [`CreateWorldSpaceCanvas(Transform, string, UICanvasConfigSO)`](#method-createworldspacecanvas-transform-string-uicanvasconfigso) | 运行时创建一个挂在 worldTarget 下的世界空间 UI Canvas 预设。 在目标物体下新建子节点并挂 WorldSpaceUICanvas——Canvas 随目标的世界变换移动。 需要相对目标的偏移或面向相机时，取返回组件设置 PositionOffset / FaceCamera（或手动调用 Build 重建）。  本方法与 Build 走同一套配置路径：优先使用显式传入的 config， 其次 UIRoot 的 WorldSpaceCanvasConfig，最后内存默认世界空间预设。 |
| [`Build()`](#method-build) | 构建 UI 层级结构。幂等调用：引用非空的物体直接跳过（子物体重命名不受影响）， 引用缺失时按约定名回收旧版已搭建的子物体，不会重复创建。 与运行时 Initialize 走同一套配置路径： 优先应用 Inspector 已序列化的 uiCanvasConfigSO，未设置时使用静态缓存的默认配置。 |
| [`CreateAndLoadCanvasConfigAsset()`](#method-createandloadcanvasconfigasset) | Inspector 按钮（Odin）：确保默认资产存在并加载到 [UIRoot]（幂等）。 |
| [`EnsureDefaultCanvasConfigAsset()`](#method-ensuredefaultcanvasconfigasset) | 确保默认 UICanvasConfig 资产存在：已存在则直接加载返回，不存在则创建目录与资产。 幂等操作，供 Inspector 按钮（CreateAndLoadCanvasConfigAsset）与 Assets/Create 菜单（UIModuleMenuItems）共用，避免两份等价实现产生行为漂移。 |
| [`TryGetExisting()`](#method-trygetexisting) | 非创建式获取已存在的 UIRoot 实例：场景中无预放置实例时返回 null， 不经 Instance 懒创建宿主树。 供世界空间 UI 等在场景卸载 / 编辑器探测阶段取相机与配置的对象使用—— 这类调用若走 Instance 会在"本来没有 UIRoot"时凭空造出 [UIRoot] 物体。 |

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

### GetLayerRoot(UILayer) {#method-getlayerroot-uilayer}

获取指定层级的根 Transform。层 Canvas 引用缺失（子物体被删除或引用丢失）时记录错误并返回 null； 子物体被重命名不受影响（引用与名称解耦）。

``` csharp
public Transform GetLayerRoot(UILayer layer)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `layer` | `UILayer` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `Transform` | — |

</div>

### CreateWorldSpaceCanvas(Transform, string, UICanvasConfigSO) {#method-createworldspacecanvas-transform-string-uicanvasconfigso}

运行时创建一个挂在 worldTarget 下的世界空间 UI Canvas 预设。
在目标物体下新建子节点并挂 WorldSpaceUICanvas——Canvas 随目标的世界变换移动。 需要相对目标的偏移或面向相机时，取返回组件设置 PositionOffset / FaceCamera（或手动调用 Build 重建）。

本方法与 Build 走同一套配置路径：优先使用显式传入的 config， 其次 UIRoot 的 WorldSpaceCanvasConfig，最后内存默认世界空间预设。

``` csharp
public WorldSpaceUICanvas CreateWorldSpaceCanvas(Transform worldTarget, string canvasName = "WorldSpaceCanvas", UICanvasConfigSO config = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `worldTarget` | `Transform` | 世界空间跟随目标（场景中的任意 Transform）。 |
| `canvasName` | `string` | 新建 Canvas 节点的名称，默认 "WorldSpaceCanvas"。 |
| `config` | `UICanvasConfigSO` | 可选的世界空间 Canvas 配置；为 null 时按上述回退链解析。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `WorldSpaceUICanvas` | 新建的 WorldSpaceUICanvas 组件；目标为空时返回 null。 |

</div>

### Build() {#method-build}

构建 UI 层级结构。幂等调用：引用非空的物体直接跳过（子物体重命名不受影响）， 引用缺失时按约定名回收旧版已搭建的子物体，不会重复创建。 与运行时 Initialize 走同一套配置路径： 优先应用 Inspector 已序列化的 uiCanvasConfigSO，未设置时使用静态缓存的默认配置。

``` csharp
public void Build()
```

### CreateAndLoadCanvasConfigAsset() {#method-createandloadcanvasconfigasset}

Inspector 按钮（Odin）：确保默认资产存在并加载到 [UIRoot]（幂等）。

``` csharp
public void CreateAndLoadCanvasConfigAsset()
```

### EnsureDefaultCanvasConfigAsset() {#method-ensuredefaultcanvasconfigasset}

确保默认 UICanvasConfig 资产存在：已存在则直接加载返回，不存在则创建目录与资产。 幂等操作，供 Inspector 按钮（CreateAndLoadCanvasConfigAsset）与 Assets/Create 菜单（UIModuleMenuItems）共用，避免两份等价实现产生行为漂移。

``` csharp
public static UICanvasConfigSO EnsureDefaultCanvasConfigAsset()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `UICanvasConfigSO` | — |

</div>

### TryGetExisting() {#method-trygetexisting}

非创建式获取已存在的 UIRoot 实例：场景中无预放置实例时返回 null， 不经 Instance 懒创建宿主树。
供世界空间 UI 等在场景卸载 / 编辑器探测阶段取相机与配置的对象使用—— 这类调用若走 Instance 会在"本来没有 UIRoot"时凭空造出 [UIRoot] 物体。

``` csharp
public static UIRoot TryGetExisting()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `UIRoot` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
