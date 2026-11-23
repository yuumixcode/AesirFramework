---
title: BinderInfo
description: "Runestone.AesirModules.BinderInfo 的 API 文档"
---

# `BinderInfo`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `BinderInfo`

## 声明

``` csharp
[Serializable]
public class BinderInfo
```

绑定单元数据。描述一条要绑定到生成脚本字段的组件引用信息。
在 BinderAssistant 的「绑定单元列表」中以表格形式配置， 由「构建绑定单元」按子物体上的 BinderTag 标记增量维护； 每条记录对应生成脚本「绑定字段（自动生成）」region 中的一个 [SerializeField] 字段和 BindComponents() 中的一行赋值代码。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`BinderInfo()`](#constructor-binderinfo) | 供序列化创建实例使用的无参构造。 |
| [`BinderInfo(BinderAssistant, BinderTag)`](#constructor-binderinfo-binderassistant-bindertag) | 由 BinderAssistant 扫描 BinderTag 时构造，自动计算路径并生成默认字段名。 |

</div>

### BinderInfo() {#constructor-binderinfo}

供序列化创建实例使用的无参构造。

``` csharp
public BinderInfo()
```

### BinderInfo(BinderAssistant, BinderTag) {#constructor-binderinfo-binderassistant-bindertag}

由 BinderAssistant 扫描 BinderTag 时构造，自动计算路径并生成默认字段名。

``` csharp
public BinderInfo(BinderAssistant assistant, BinderTag tagObj)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `assistant` | `BinderAssistant` | — |
| `tagObj` | `BinderTag` | — |

</div>

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`LabelObj`](#field-labelobj) | 被绑定组件所在的 GameObject 引用。 |
| [`ComponentFullName`](#field-componentfullname) | 组件类型的完整名称（含命名空间），作为代码生成时 GetComponent<T>() 的泛型参数； 选择 GameObject 表示绑定物体本身。 |
| [`FieldName`](#field-fieldname) | 生成脚本中的字段名（camelCase）。默认值为「物体名_类型简称」，可手动修改； 重复字段名会在校验时报错。 |
| [`HierarchyPath`](#field-hierarchypath) | 相对于 BinderAssistant 的 transform.Find() 路径， 由 UpdatePath 自动计算；空字符串表示绑定 Assistant 自身所在物体。 |

</div>

### LabelObj {#field-labelobj}

被绑定组件所在的 GameObject 引用。

``` csharp
[TableColumnWidth]
[LabelText]
public GameObject LabelObj;
```

### ComponentFullName {#field-componentfullname}

组件类型的完整名称（含命名空间），作为代码生成时 GetComponent<T>() 的泛型参数； 选择 GameObject 表示绑定物体本身。

``` csharp
[TableColumnWidth]
[LabelText]
[ValueDropdown]
public string ComponentFullName;
```

### FieldName {#field-fieldname}

生成脚本中的字段名（camelCase）。默认值为「物体名_类型简称」，可手动修改； 重复字段名会在校验时报错。

``` csharp
[TableColumnWidth]
[LabelText]
[InlineButton]
public string FieldName;
```

### HierarchyPath {#field-hierarchypath}

相对于 BinderAssistant 的 transform.Find() 路径， 由 UpdatePath 自动计算；空字符串表示绑定 Assistant 自身所在物体。

``` csharp
[LabelText]
[DisplayAsString]
public string HierarchyPath;
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`DefaultFieldName()`](#method-defaultfieldname) | 将字段名重置为默认值: 「物体名_类型简称」的 camelCase 形式（如 playButton_Button）。 可能与其他单元重名，重名会在校验时报错。 仅编辑器可用：依赖运行时可见程序集 Runestone.AesirModules.OdinInspector 内、 由 #if UNITY_EDITOR 收拢的代码生成器。 |
| [`UpdatePath(BinderAssistant)`](#method-updatepath-binderassistant) | 更新相对于 BinderAssistant 的层级路径。 物体丢失或缺少 BinderTag 标记时置空，交由校验报错。 |

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

### DefaultFieldName() {#method-defaultfieldname}

将字段名重置为默认值: 「物体名_类型简称」的 camelCase 形式（如 playButton_Button）。 可能与其他单元重名，重名会在校验时报错。
仅编辑器可用：依赖运行时可见程序集 Runestone.AesirModules.OdinInspector 内、 由 #if UNITY_EDITOR 收拢的代码生成器。

``` csharp
public void DefaultFieldName()
```

### UpdatePath(BinderAssistant) {#method-updatepath-binderassistant}

更新相对于 BinderAssistant 的层级路径。 物体丢失或缺少 BinderTag 标记时置空，交由校验报错。

``` csharp
public void UpdatePath(BinderAssistant assistant)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `assistant` | `BinderAssistant` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
