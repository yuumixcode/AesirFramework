---
title: BinderAssistant
description: "Runestone.AesirModules.BinderAssistant 的 API 文档"
---

# `BinderAssistant`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.Component` → `UnityEngine.Behaviour` → `UnityEngine.MonoBehaviour` → `Sirenix.OdinInspector.SerializedMonoBehaviour` → `Runestone.AesirArchitecture.AesirMonoBehaviour` → `BinderAssistant`

**实现接口:** `Sirenix.Serialization.ISupportsPrefabSerialization`，`UnityEngine.ISerializationCallbackReceiver`

## 声明

``` csharp
[DetailedInfoBox]
[DisallowMultipleComponent]
public class BinderAssistant : Runestone.AesirArchitecture.AesirMonoBehaviour, 
Sirenix.Serialization.ISupportsPrefabSerialization, 
UnityEngine.ISerializationCallbackReceiver
```

Object Binder 核心组件。挂载在根 UI 物体（面板或 Canvas 根窗口）上，统一配置所有要绑定的子组件并一键生成绑定脚本。
工作流程： 1. 在需要绑定引用的子物体上添加 BinderTag 组件标记（默认绑定 1 个组件），用数量声明要绑定的组件个数。 2. 在本组件上点击「构建绑定单元」——按标记增量更新 BinderInfo 列表 （新增缺失单元、刷新已有单元路径、移除标记已删除或数量缩减的单元），每个单元记录 组件类型、字段名、绑定路径等配置。 3. 点击「生成脚本」，按生成模式产出代码： - PartialClass：*.generated.cs（自动维护，整体覆盖）+ *.cs（开发者手写业务逻辑的 partial 类，仅首次生成，不会被覆盖）； - SameScriptIncrement：只替换目标脚本 *.cs 内「绑定字段（自动生成）」region 的内容（字段 + BindComponents() 方法）， region 外的内容归开发者所有；文件不存在时先创建脚手架。 4. 编译完成后自动把生成脚本挂载到当前 GameObject 并执行一次绑定。

生成脚本的基类可从「基类」下拉中选择：内置 MonoBehaviour、 由 Binder 预选的 Aesir 面板/窗口家族（AesirBasePanel、AesirBasePanelView<T>、 AesirBasePanelViewController<T>、AesirBaseWindow、 AesirBaseWindowView<T>、AesirBaseWindowViewController<T>—— 核心程序集无法反向引用 Odin 程序集标注 BinderBaseTypeAttribute，故经 typeof 直接内置）， 以及所有被 BinderBaseTypeAttribute 标记的用户类 （选择 Aesir 泛型面板基类后在「Context 类型」下拉中选择项目内的 AbstractContext 派生类， 占位不会写进生成代码）。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`BinderAssistant()`](#constructor-binderassistant) | — |

</div>

### BinderAssistant() {#constructor-binderassistant}

``` csharp
public BinderAssistant()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ScriptMode`](#field-scriptmode) | 当前是否为 partial 分部类模式。 |
| [`Units`](#field-units) | — |
| [`CustomNamespaces`](#field-customnamespaces) | — |
| [`OpenAutoValidate`](#field-openautovalidate) | — |
| [`BaseType`](#field-basetype) | — |
| [`BaseTypeArguments`](#field-basetypearguments) | 生成时实际使用的泛型类型参数: Aesir 泛型 UI 基类取「Context 类型」下拉，其余取「泛型参数」文本。 |
| [`ContextTypeName`](#field-contexttypename) | — |
| [`FolderPath`](#field-folderpath) | — |
| [`PartialSuffix`](#field-partialsuffix) | — |
| [`ScriptName`](#field-scriptname) | — |
| [`TargetNamespace`](#field-targetnamespace) | — |

</div>

### ScriptMode {#field-scriptmode}

当前是否为 partial 分部类模式。

``` csharp
[FoldoutGroup]
[LabelText]
public BinderScriptMode ScriptMode;
```

### Units {#field-units}

``` csharp
[InfoBox]
[Title]
[TableList]
public List<BinderInfo> Units;
```

### CustomNamespaces {#field-customnamespaces}

``` csharp
[FoldoutGroup]
[LabelText]
public List<string> CustomNamespaces;
```

### OpenAutoValidate {#field-openautovalidate}

``` csharp
[PropertyOrder]
[HorizontalGroup]
[ToggleLeft]
[LabelText]
public bool OpenAutoValidate;
```

### BaseType {#field-basetype}

``` csharp
[FoldoutGroup]
[ValueDropdown]
[LabelText]
public string BaseType;
```

### BaseTypeArguments {#field-basetypearguments}

生成时实际使用的泛型类型参数: Aesir 泛型 UI 基类取「Context 类型」下拉，其余取「泛型参数」文本。

``` csharp
[FoldoutGroup]
[ShowIf]
[LabelText]
public string BaseTypeArguments;
```

### ContextTypeName {#field-contexttypename}

``` csharp
[FoldoutGroup]
[ShowIf]
[ValueDropdown]
[LabelText]
[InfoBox]
public string ContextTypeName;
```

### FolderPath {#field-folderpath}

``` csharp
[FoldoutGroup]
[LabelText]
[InlineButton]
[FolderPath]
public string FolderPath;
```

### PartialSuffix {#field-partialsuffix}

``` csharp
[FoldoutGroup]
[ShowIf]
[ValueDropdown]
[LabelText]
[UnityEngine.Tooltip("Rider 中 .designer.cs 是默认折叠的，Rider 用户推荐使用")]
[OnValueChanged]
public string PartialSuffix;
```

### ScriptName {#field-scriptname}

``` csharp
[FoldoutGroup]
[LabelText]
[InlineButton]
public string ScriptName;
```

### TargetNamespace {#field-targetnamespace}

``` csharp
[FoldoutGroup]
[LabelText]
[InlineButton]
public string TargetNamespace;
```

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`PartialSuffixList`](#property-partialsuffixlist) | — |
| [`HasError`](#property-haserror) | — |
| [`HierarchyPath`](#property-hierarchypath) | — |

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

### PartialSuffixList {#property-partialsuffixlist}

``` csharp
[FoldoutGroup]
[ShowIf]
[LabelText]
[ShowInInspector]
[OnValueChanged]
public List<string> PartialSuffixList { get; }
```

### HasError {#property-haserror}

``` csharp
[PropertyOrder]
[HorizontalGroup]
[ShowInInspector]
[ReadOnly]
[LabelText]
public bool HasError { get; private set; }
```

### HierarchyPath {#property-hierarchypath}

``` csharp
public string HierarchyPath { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetBaseTypes()`](#method-getbasetypes) | 可选基类下拉列表: 内置 MonoBehaviour，加上由 Binder 预选的 Aesir 面板/窗口家族 （AesirBasePanel / AesirBasePanelView<T> / AesirBasePanelViewController<T> / AesirBaseWindow / AesirBaseWindowView<T> / AesirBaseWindowViewController<T>—— 核心程序集无法反向引用 Odin 程序集标注 BinderBaseTypeAttribute，故经 typeof 直接内置）， 以及所有被 BinderBaseTypeAttribute 标记的 MonoBehaviour 派生类； 泛型基类以 <T> 占位形式提供，选择后需把占位替换为具体类型参数。 |
| [`GetContextTypeChoices()`](#method-getcontexttypechoices) | 「Context 类型」下拉列表: 项目内所有具体的 AbstractContext 派生类（CRTP 闭合类型）。 被 InternalContextAttribute 标记的框架内部 Context（示例 / 测试）不会出现。 |
| [`GetPartialSuffixOptions()`](#method-getpartialsuffixoptions) | 「生成文件后缀」下拉选项: 来自 ScriptableSingleton 持久化的后缀列表。 |

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

### GetBaseTypes() {#method-getbasetypes}

可选基类下拉列表: 内置 MonoBehaviour，加上由 Binder 预选的 Aesir 面板/窗口家族 （AesirBasePanel / AesirBasePanelView<T> / AesirBasePanelViewController<T> / AesirBaseWindow / AesirBaseWindowView<T> / AesirBaseWindowViewController<T>—— 核心程序集无法反向引用 Odin 程序集标注 BinderBaseTypeAttribute，故经 typeof 直接内置）， 以及所有被 BinderBaseTypeAttribute 标记的 MonoBehaviour 派生类； 泛型基类以 <T> 占位形式提供，选择后需把占位替换为具体类型参数。

``` csharp
public ValueDropdownList<string> GetBaseTypes()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `ValueDropdownList<string>` | — |

</div>

### GetContextTypeChoices() {#method-getcontexttypechoices}

「Context 类型」下拉列表: 项目内所有具体的 AbstractContext 派生类（CRTP 闭合类型）。 被 InternalContextAttribute 标记的框架内部 Context（示例 / 测试）不会出现。

``` csharp
public ValueDropdownList<string> GetContextTypeChoices()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `ValueDropdownList<string>` | — |

</div>

### GetPartialSuffixOptions() {#method-getpartialsuffixoptions}

「生成文件后缀」下拉选项: 来自 ScriptableSingleton 持久化的后缀列表。

``` csharp
public ValueDropdownList<string> GetPartialSuffixOptions()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `ValueDropdownList<string>` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
