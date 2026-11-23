---
title: UIModule
description: "Runestone.AesirModules.UIModule 的 API 文档"
---

# `UIModule`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.Component` → `UnityEngine.Behaviour` → `UnityEngine.MonoBehaviour` → `Sirenix.OdinInspector.SerializedMonoBehaviour` → `Runestone.AesirArchitecture.AesirMonoBehaviour` → `UIModule`

**实现接口:** `Sirenix.Serialization.ISupportsPrefabSerialization`，`UnityEngine.ISerializationCallbackReceiver`

## 声明

``` csharp
[DisallowMultipleComponent]
[DefaultExecutionOrder]
public class UIModule : Runestone.AesirArchitecture.AesirMonoBehaviour, 
Sirenix.Serialization.ISupportsPrefabSerialization, 
UnityEngine.ISerializationCallbackReceiver
```

UI 管理器（MonoBehaviour 单例）。 负责面板生命周期管理，UI 根节点构建委托给 UIRoot。

**备注**

是否加入 DontDestroyOnLoad 场景由序列化字段 dontDestroyOnLoad 控制， 仅在本物体为根物体（场景预放置）时生效；运行时自动创建的实例挂载在 AesirModules 宿主下， 实际是否 DDOL 跟随宿主的 dontDestroyOnLoad 决策。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`UIModule()`](#constructor-uimodule) | — |

</div>

### UIModule() {#constructor-uimodule}

``` csharp
public UIModule()
```

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`UICamera`](#property-uicamera) | UI 专用相机。正交、depth=1、cullingMask=含 UI 层 (5) 和 TransparentFX 层 (1)。 |
| [`MaskMode`](#property-maskmode) | 窗口蒙版调度模式。初值来自 UIModuleConfigSO（在 Project 窗口编辑配置资产即可调整， 不要求预放置本组件）；运行时可切换，切换后立即重算全部窗口蒙版。 |
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

### UICamera {#property-uicamera}

UI 专用相机。正交、depth=1、cullingMask=含 UI 层 (5) 和 TransparentFX 层 (1)。

**备注**

UIRoot 缺失或已销毁时返回 null：显式 == null 走 Unity 假 null 语义， 避免 ?. 绕过 Object 的重载 == 而返回已销毁对象的相机。

``` csharp
public Camera UICamera { get; }
```

### MaskMode {#property-maskmode}

窗口蒙版调度模式。初值来自 UIModuleConfigSO（在 Project 窗口编辑配置资产即可调整， 不要求预放置本组件）；运行时可切换，切换后立即重算全部窗口蒙版。

``` csharp
public UIMaskMode MaskMode { get; set; }
```

### Instance {#property-instance}

全局单例入口。 优先在已加载场景中查找预放置的实例；未找到时在 AesirModules（DDOL）下创建子物体。

``` csharp
public static UIModule Instance { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ShowPanel(Type, object, string)`](#method-showpanel-type-object-string) | 打开面板。已存在（激活或停用）则置顶并重新 Show；不存在则实例化并驱动生命周期。 新面板以停用状态实例化，按 挂层 → Initialize → Show 顺序驱动， Awake/OnEnable 推迟到 Show 内部激活时才触发，保证 OnEnable 可安全访问 OnInit 之后才有值的引用。  面板注册表以实例的实际类型为键：以基类类型调用且注册表已存在派生实例时记录错误并返回 null （不会重复实例化）；需以实际类型（或面板内 HideSelf）操作。  面板所属层的 Canvas 缺失（UIRoot 层级结构性损坏）时记录错误并中止本次显示，不保留半挂载实例。 |
| [`GetWindow(Type)`](#method-getwindow-type) | 获取已注册的窗口实例。键为窗口实例的实际类型；精确未命中时静默返回 null， 仅当注册表存在派生实例（疑似以基类类型误查）时记录键语义警告。 |
| [`OpenWindow(Type, object, string)`](#method-openwindow-type-object-string) | 打开窗口。不存在（激活或停用）则实例化并驱动生命周期；已存在则置顶并重新 Show。 新窗口以停用状态实例化，按 挂载 UIRoot → 接线根 Canvas（相机/渲染模式/统一缩放配置/sortingOrder）→ 递归设 UI 层 → Initialize → Show 顺序驱动； Awake/OnEnable 推迟到 Show 内部激活时才触发（与面板同一契约）。  窗口注册表以实例的实际类型为键：以基类类型调用且注册表已存在派生实例时记录错误并返回 null； 根节点缺少 Canvas 组件（违反 Canvas 根约定）时记录错误并中止，不保留半挂载实例。 |
| [`GetPanel()`](#method-getpanel) | 获取已注册的面板实例。键为面板实例的实际类型；精确未命中时静默返回 null， 仅当注册表存在派生实例（疑似以基类类型误查）时记录键语义警告。 |
| [`OpenWindow(object, string)`](#method-openwindow-object-string) | 打开窗口（泛型）。不存在则实例化并驱动生命周期；已存在则置顶并重新 Show。 |
| [`ShowPanel(object, string)`](#method-showpanel-object-string) | 打开面板（泛型）。已存在则置顶并重新 Show；不存在则实例化并驱动生命周期。 |
| [`ShowPanel(TPayload, string)`](#method-showpanel-tpayload-string) | 打开面板（泛型 + 强类型 payload）。payload 以泛型参数传递，调用侧获得编译期类型约束； 面板内部仍经 Show(object) 接收后按需转换（运行时类型安全仍由面板内转换保证）。 |
| [`ContainPrefabAsset()`](#method-containprefabasset) | — |
| [`ContainWindowPrefabAsset(Type)`](#method-containwindowprefabasset-type) | 窗口类型对应的预制体是否已注册（与面板共用同一份预制体注册表）。 |
| [`PrewarmPanel(Type, string)`](#method-prewarmpanel-type-string) | 预热面板。预实例化并隐藏面板，后续 ShowPanel(Type, object, string) 直接复用， 避免首次打开时的实例化卡顿。 面板以停用状态实例化，预热期不触发 Awake/OnEnable，待首次 Show 时再激活。 |
| [`PrewarmPanel(string)`](#method-prewarmpanel-string) | 预热面板（泛型）。预实例化并隐藏面板，后续 ShowPanel{T}(object, string) 直接复用， 避免首次打开时的实例化卡顿。 |
| [`PrewarmWindow(Type, string)`](#method-prewarmwindow-type-string) | 预热窗口。预实例化并隐藏窗口，后续 OpenWindow(Type, object, string) 直接复用， 避免首次打开时的实例化卡顿。 窗口以停用状态实例化，预热期不触发 Awake/OnEnable，待首次打开时再激活（与面板同一契约）。 |
| [`CloseWindow(Type)`](#method-closewindow-type) | 关闭窗口。按 DestroyOnHide 决定销毁或隐藏； 键语义与幂等约定与 HidePanel(Type) 一致。 |
| [`CloseWindow()`](#method-closewindow) | 关闭窗口（泛型）。按 DestroyOnHide 决定销毁或隐藏。 |
| [`HidePanel(Type)`](#method-hidepanel-type) | 关闭面板。按 DestroyOnHide 决定销毁或隐藏。 注册表以面板实例的实际类型为键：以基类类型调用且注册表已存在派生实例时记录警告提示键语义； 无关联实例时按幂等语义静默返回。已停用（或未显示）的面板重复关闭同样为幂等操作。 |
| [`HidePanel()`](#method-hidepanel) | 关闭面板（泛型）。按 DestroyOnHide 决定销毁或隐藏。 |
| [`PrewarmAll(Action)`](#method-prewarmall-action) | 预热所有已注册的面板，逐帧实例化以分摊性能开销。 |
| [`RegisterAssetLoader(IUIAssetLoader)`](#method-registerassetloader-iuiassetloader) | 替换默认的面板资源加载器。 |
| [`RegisterPanelPrefab(Type, GameObject)`](#method-registerpanelprefab-type-gameobject) | 注册面板类型对应的预制体。 |
| [`RegisterPanelPrefab(GameObject)`](#method-registerpanelprefab-gameobject) | 注册面板类型对应的预制体（泛型版本）。 |
| [`RegisterWindowPrefab(Type, GameObject)`](#method-registerwindowprefab-type-gameobject) | 注册窗口类型对应的预制体（与面板共用同一份预制体注册表，类型系统天然分桶）。 |
| [`Get()`](#method-get) | 静态快捷：获取已注册的面板实例。 |
| [`GetWindow()`](#method-getwindow) | 静态快捷：获取已注册的窗口实例。 |
| [`Open(object, string)`](#method-open-object-string) | 静态快捷：打开窗口。 |
| [`Open(string)`](#method-open-string) | 静态快捷：打开窗口（资源路径版本）。Open<T>("UI/Xxx") 的字符串经本重载进入 path 形参——消除位置参数版中路径被 object payload 吞掉的陷阱（同 Show{T}(string)）。 |
| [`Show(object, string)`](#method-show-object-string) | 静态快捷：打开面板。 |
| [`Show(string)`](#method-show-string) | 静态快捷：打开面板（资源路径版本）。Show<T>("UI/Xxx") 的字符串经本重载进入 path 形参——消除位置参数版中路径被 object payload 吞掉、 随后报"未注册预制体且未提供 path"且不揭示真实原因的陷阱。 |
| [`Show(TPayload, string)`](#method-show-tpayload-string) | 静态快捷：打开面板（强类型 payload 版本，同 ShowPanel{TPanel, TPayload}(TPayload, string)）。 |
| [`ContainPrefab()`](#method-containprefab) | 静态快捷：面板类型对应的预制体是否已注册。 |
| [`ContainWindowPrefab()`](#method-containwindowprefab) | 静态快捷：窗口类型对应的预制体是否已注册。 |
| [`Prewarm(string)`](#method-prewarm-string) | 静态快捷：预热面板。 |
| [`PrewarmWindow(string)`](#method-prewarmwindow-string) | 静态快捷：预热窗口。 |
| [`Close()`](#method-close) | 静态快捷：关闭窗口。 |
| [`Hide()`](#method-hide) | 静态快捷：关闭面板。 |
| [`RegisterPrefab(GameObject)`](#method-registerprefab-gameobject) | 静态快捷：注册面板预制体。 |
| [`RegisterPrefab(T)`](#method-registerprefab-t) | 静态快捷：注册面板预制体。 |
| [`RegisterWindowPrefab(GameObject)`](#method-registerwindowprefab-gameobject) | 静态快捷：注册窗口类型对应的预制体。 |

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

### ShowPanel(Type, object, string) {#method-showpanel-type-object-string}

打开面板。已存在（激活或停用）则置顶并重新 Show；不存在则实例化并驱动生命周期。
新面板以停用状态实例化，按 挂层 → Initialize → Show 顺序驱动， Awake/OnEnable 推迟到 Show 内部激活时才触发，保证 OnEnable 可安全访问 OnInit 之后才有值的引用。

面板注册表以实例的实际类型为键：以基类类型调用且注册表已存在派生实例时记录错误并返回 null （不会重复实例化）；需以实际类型（或面板内 HideSelf）操作。

面板所属层的 Canvas 缺失（UIRoot 层级结构性损坏）时记录错误并中止本次显示，不保留半挂载实例。

``` csharp
public IUIPanel ShowPanel(Type panelType, object payload = null, string path = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `panelType` | `Type` | 面板类型。 |
| `payload` | `object` | 传递给 OnShow 的数据。 |
| `path` | `string` | 可选的资源路径。注册表中不存在时通过加载器加载，加载后自动注册到注册表。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `IUIPanel` | 面板实例，失败返回 null。 |

</div>

### GetWindow(Type) {#method-getwindow-type}

获取已注册的窗口实例。键为窗口实例的实际类型；精确未命中时静默返回 null， 仅当注册表存在派生实例（疑似以基类类型误查）时记录键语义警告。

**备注**

纯查询不创建模块实例（理由同 Get{T}）。

``` csharp
public IUIWindow GetWindow(Type windowType)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `windowType` | `Type` | 窗口类型。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `IUIWindow` | 窗口实例，未注册返回 null。 |

</div>

### OpenWindow(Type, object, string) {#method-openwindow-type-object-string}

打开窗口。不存在（激活或停用）则实例化并驱动生命周期；已存在则置顶并重新 Show。
新窗口以停用状态实例化，按 挂载 UIRoot → 接线根 Canvas（相机/渲染模式/统一缩放配置/sortingOrder）→ 递归设 UI 层 → Initialize → Show 顺序驱动； Awake/OnEnable 推迟到 Show 内部激活时才触发（与面板同一契约）。

窗口注册表以实例的实际类型为键：以基类类型调用且注册表已存在派生实例时记录错误并返回 null； 根节点缺少 Canvas 组件（违反 Canvas 根约定）时记录错误并中止，不保留半挂载实例。

``` csharp
public IUIWindow OpenWindow(Type windowType, object payload = null, string path = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `windowType` | `Type` | 窗口类型。 |
| `payload` | `object` | 传递给 OnShow 的数据。 |
| `path` | `string` | 可选的资源路径。注册表中不存在时通过加载器加载，加载后自动注册到注册表。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `IUIWindow` | 窗口实例，失败返回 null。 |

</div>

### GetPanel() {#method-getpanel}

获取已注册的面板实例。键为面板实例的实际类型；精确未命中时静默返回 null， 仅当注册表存在派生实例（疑似以基类类型误查）时记录键语义警告。

``` csharp
public T GetPanel<T>()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `T` | 面板实例，未注册返回 null。 |

</div>

### OpenWindow(object, string) {#method-openwindow-object-string}

打开窗口（泛型）。不存在则实例化并驱动生命周期；已存在则置顶并重新 Show。

``` csharp
public T OpenWindow<T>(object payload = null, string path = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `payload` | `object` | 传递给 OnShow 的数据。 |
| `path` | `string` | 可选的资源路径。注册表中不存在时通过加载器加载，加载后自动注册到注册表。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `T` | 窗口实例，失败返回 null。 |

</div>

### ShowPanel(object, string) {#method-showpanel-object-string}

打开面板（泛型）。已存在则置顶并重新 Show；不存在则实例化并驱动生命周期。

``` csharp
public T ShowPanel<T>(object payload = null, string path = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `payload` | `object` | 传递给 OnShow 的数据。 |
| `path` | `string` | 可选的资源路径。注册表中不存在时通过加载器加载，加载后自动注册到注册表。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `T` | 面板实例，失败返回 null。 |

</div>

### ShowPanel(TPayload, string) {#method-showpanel-tpayload-string}

打开面板（泛型 + 强类型 payload）。payload 以泛型参数传递，调用侧获得编译期类型约束； 面板内部仍经 Show(object) 接收后按需转换（运行时类型安全仍由面板内转换保证）。

``` csharp
public TPanel ShowPanel<TPanel, TPayload>(TPayload payload, string path = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `payload` | `TPayload` | 传递给 OnShow 的强类型数据。 |
| `path` | `string` | 可选的资源路径。注册表中不存在时通过加载器加载，加载后自动注册到注册表。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `TPanel` | 面板实例，失败返回 null。 |

</div>

### ContainPrefabAsset() {#method-containprefabasset}

``` csharp
public bool ContainPrefabAsset<T>()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### ContainWindowPrefabAsset(Type) {#method-containwindowprefabasset-type}

窗口类型对应的预制体是否已注册（与面板共用同一份预制体注册表）。

``` csharp
public bool ContainWindowPrefabAsset(Type windowType)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `windowType` | `Type` | 窗口类型。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### PrewarmPanel(Type, string) {#method-prewarmpanel-type-string}

预热面板。预实例化并隐藏面板，后续 ShowPanel(Type, object, string) 直接复用， 避免首次打开时的实例化卡顿。
面板以停用状态实例化，预热期不触发 Awake/OnEnable，待首次 Show 时再激活。

``` csharp
public bool PrewarmPanel(Type panelType, string path = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `panelType` | `Type` | 面板类型。 |
| `path` | `string` | 可选的资源路径。注册表中不存在时通过加载器加载，加载后自动注册到注册表。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 预热成功或面板已存在时返回 true。 |

</div>

### PrewarmPanel(string) {#method-prewarmpanel-string}

预热面板（泛型）。预实例化并隐藏面板，后续 ShowPanel{T}(object, string) 直接复用， 避免首次打开时的实例化卡顿。

``` csharp
public bool PrewarmPanel<T>(string path = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `path` | `string` | 可选的资源路径。注册表中不存在时通过加载器加载，加载后自动注册到注册表。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 预热成功或面板已存在时返回 true。 |

</div>

### PrewarmWindow(Type, string) {#method-prewarmwindow-type-string}

预热窗口。预实例化并隐藏窗口，后续 OpenWindow(Type, object, string) 直接复用， 避免首次打开时的实例化卡顿。
窗口以停用状态实例化，预热期不触发 Awake/OnEnable，待首次打开时再激活（与面板同一契约）。

``` csharp
public bool PrewarmWindow(Type windowType, string path = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `windowType` | `Type` | 窗口类型。 |
| `path` | `string` | 可选的资源路径。注册表中不存在时通过加载器加载，加载后自动注册到注册表。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 预热成功或窗口已存在时返回 true。 |

</div>

### CloseWindow(Type) {#method-closewindow-type}

关闭窗口。按 DestroyOnHide 决定销毁或隐藏； 键语义与幂等约定与 HidePanel(Type) 一致。

``` csharp
public void CloseWindow(Type windowType)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `windowType` | `Type` | 窗口类型。 |

</div>

### CloseWindow() {#method-closewindow}

关闭窗口（泛型）。按 DestroyOnHide 决定销毁或隐藏。

``` csharp
public void CloseWindow<T>()
```

### HidePanel(Type) {#method-hidepanel-type}

关闭面板。按 DestroyOnHide 决定销毁或隐藏。
注册表以面板实例的实际类型为键：以基类类型调用且注册表已存在派生实例时记录警告提示键语义； 无关联实例时按幂等语义静默返回。已停用（或未显示）的面板重复关闭同样为幂等操作。

``` csharp
public void HidePanel(Type panelType)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `panelType` | `Type` | 面板类型。 |

</div>

### HidePanel() {#method-hidepanel}

关闭面板（泛型）。按 DestroyOnHide 决定销毁或隐藏。

``` csharp
public void HidePanel<T>()
```

### PrewarmAll(Action) {#method-prewarmall-action}

预热所有已注册的面板，逐帧实例化以分摊性能开销。

``` csharp
public void PrewarmAll(Action onComplete = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `onComplete` | `Action` | 全部预热完成后的回调（可为空）。 |

</div>

### RegisterAssetLoader(IUIAssetLoader) {#method-registerassetloader-iuiassetloader}

替换默认的面板资源加载器。

``` csharp
public void RegisterAssetLoader(IUIAssetLoader loader)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `loader` | `IUIAssetLoader` | 自定义加载器。加载契约为同步语义（如同步缓存、Resources）；Addressables 等异步管线需自行预加载后同步返回。为空时记录错误并保留现有加载器。 |

</div>

### RegisterPanelPrefab(Type, GameObject) {#method-registerpanelprefab-type-gameobject}

注册面板类型对应的预制体。

``` csharp
public void RegisterPanelPrefab(Type panelType, GameObject prefab)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `panelType` | `Type` | 面板类型。 |
| `prefab` | `GameObject` | 面板预制体。 |

</div>

### RegisterPanelPrefab(GameObject) {#method-registerpanelprefab-gameobject}

注册面板类型对应的预制体（泛型版本）。

``` csharp
public void RegisterPanelPrefab<T>(GameObject prefab)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `prefab` | `GameObject` | 面板预制体。 |

</div>

### RegisterWindowPrefab(Type, GameObject) {#method-registerwindowprefab-type-gameobject}

注册窗口类型对应的预制体（与面板共用同一份预制体注册表，类型系统天然分桶）。

``` csharp
public void RegisterWindowPrefab(Type windowType, GameObject prefab)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `windowType` | `Type` | 窗口类型。 |
| `prefab` | `GameObject` | 窗口预制体。 |

</div>

### Get() {#method-get}

静态快捷：获取已注册的面板实例。

**备注**

纯查询不创建模块实例：无实例（如场景卸载、启动即查询、编辑器期探测）时返回 null， 不再凭空造出 [Aesir Modules] 宿主树。

``` csharp
public static T Get<T>()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `T` | — |

</div>

### GetWindow() {#method-getwindow}

静态快捷：获取已注册的窗口实例。

**备注**

纯查询不创建模块实例（理由同 Get{T}）。

``` csharp
public static T GetWindow<T>()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `T` | 窗口实例，未注册返回 null。 |

</div>

### Open(object, string) {#method-open-object-string}

静态快捷：打开窗口。

``` csharp
public static T Open<T>(object payload = null, string path = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `payload` | `object` | 传递给 OnShow 的数据。 |
| `path` | `string` | 可选的资源路径。注册表中不存在时通过加载器加载，加载后自动注册到注册表。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `T` | 窗口实例，失败返回 null。 |

</div>

### Open(string) {#method-open-string}

静态快捷：打开窗口（资源路径版本）。Open<T>("UI/Xxx") 的字符串经本重载进入 path 形参——消除位置参数版中路径被 object payload 吞掉的陷阱（同 Show{T}(string)）。

``` csharp
public static T Open<T>(string path)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `path` | `string` | 资源路径，用于加载预制体。使用 UIAssetLoader 加载。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `T` | 窗口实例，失败返回 null。 |

</div>

### Show(object, string) {#method-show-object-string}

静态快捷：打开面板。

``` csharp
public static T Show<T>(object payload = null, string path = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `payload` | `object` | 传递给 OnShow 的数据。 |
| `path` | `string` | 资源路径，用于加载预制体。使用 UIAssetLoader 加载。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `T` | 面板实例。 |

</div>

### Show(string) {#method-show-string}

静态快捷：打开面板（资源路径版本）。Show<T>("UI/Xxx") 的字符串经本重载进入 path 形参——消除位置参数版中路径被 object payload 吞掉、 随后报"未注册预制体且未提供 path"且不揭示真实原因的陷阱。

``` csharp
public static T Show<T>(string path)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `path` | `string` | 资源路径，用于加载预制体。使用 UIAssetLoader 加载。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `T` | 面板实例。 |

</div>

### Show(TPayload, string) {#method-show-tpayload-string}

静态快捷：打开面板（强类型 payload 版本，同 ShowPanel{TPanel, TPayload}(TPayload, string)）。

``` csharp
public static TPanel Show<TPanel, TPayload>(TPayload payload, string path = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `payload` | `TPayload` | 传递给 OnShow 的强类型数据。 |
| `path` | `string` | 资源路径，用于加载预制体。使用 UIAssetLoader 加载。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `TPanel` | 面板实例，失败返回 null。 |

</div>

### ContainPrefab() {#method-containprefab}

静态快捷：面板类型对应的预制体是否已注册。

**备注**

纯查询不创建模块实例（理由同 Get{T}）。

``` csharp
public static bool ContainPrefab<T>()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### ContainWindowPrefab() {#method-containwindowprefab}

静态快捷：窗口类型对应的预制体是否已注册。

**备注**

纯查询不创建模块实例（理由同 Get{T}）。

``` csharp
public static bool ContainWindowPrefab<T>()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### Prewarm(string) {#method-prewarm-string}

静态快捷：预热面板。

``` csharp
public static bool Prewarm<T>(string path = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `path` | `string` | 可选的资源路径。注册表中不存在时通过加载器加载，加载后自动注册到注册表。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 预热成功或面板已存在时返回 true。 |

</div>

### PrewarmWindow(string) {#method-prewarmwindow-string}

静态快捷：预热窗口。

``` csharp
public static bool PrewarmWindow<T>(string path = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `path` | `string` | 可选的资源路径。注册表中不存在时通过加载器加载，加载后自动注册到注册表。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 预热成功或窗口已存在时返回 true。 |

</div>

### Close() {#method-close}

静态快捷：关闭窗口。

``` csharp
public static void Close<T>()
```

### Hide() {#method-hide}

静态快捷：关闭面板。

``` csharp
public static void Hide<T>()
```

### RegisterPrefab(GameObject) {#method-registerprefab-gameobject}

静态快捷：注册面板预制体。

``` csharp
public static void RegisterPrefab<T>(GameObject prefab)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `prefab` | `GameObject` | 面板预制体。 |

</div>

### RegisterPrefab(T) {#method-registerprefab-t}

静态快捷：注册面板预制体。

``` csharp
public static void RegisterPrefab<T>(T prefab)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `prefab` | `T` | 面板预制体。 |

</div>

### RegisterWindowPrefab(GameObject) {#method-registerwindowprefab-gameobject}

静态快捷：注册窗口类型对应的预制体。

``` csharp
public static void RegisterWindowPrefab<T>(GameObject prefab)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `prefab` | `GameObject` | 窗口预制体。 |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
