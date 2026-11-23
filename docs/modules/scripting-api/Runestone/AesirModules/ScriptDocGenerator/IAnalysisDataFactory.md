---
title: IAnalysisDataFactory
description: "Runestone.AesirModules.ScriptDocGenerator.IAnalysisDataFactory 的 API 文档"
---

# `IAnalysisDataFactory`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

## 声明

``` csharp
public interface IAnalysisDataFactory
```

解析数据工厂接口，自定义扩展解析数据工厂

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`CreateConstructorData(ConstructorInfo, IAttributeFilter)`](#method-createconstructordata-constructorinfo-iattributefilter) | 创建构造函数数据 |
| [`CreateEventData(EventInfo, IAttributeFilter)`](#method-createeventdata-eventinfo-iattributefilter) | — |
| [`CreateFieldData(FieldInfo, IAttributeFilter)`](#method-createfielddata-fieldinfo-iattributefilter) | — |
| [`CreateMethodData(MethodInfo, IAttributeFilter)`](#method-createmethoddata-methodinfo-iattributefilter) | — |
| [`CreatePropertyData(PropertyInfo, IAttributeFilter)`](#method-createpropertydata-propertyinfo-iattributefilter) | — |
| [`CreateTypeData(Type, IAnalysisDataFactory, IAttributeFilter)`](#method-createtypedata-type-ianalysisdatafactory-iattributefilter) | 创建类型数据 |

</div>

### CreateConstructorData(ConstructorInfo, IAttributeFilter) {#method-createconstructordata-constructorinfo-iattributefilter}

创建构造函数数据

``` csharp
public abstract IConstructorData CreateConstructorData(ConstructorInfo constructorInfo, IAttributeFilter filter = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `constructorInfo` | `ConstructorInfo` | — |
| `filter` | `IAttributeFilter` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `IConstructorData` | — |

</div>

### CreateEventData(EventInfo, IAttributeFilter) {#method-createeventdata-eventinfo-iattributefilter}

``` csharp
public abstract IEventData CreateEventData(EventInfo eventInfo, IAttributeFilter filter = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `eventInfo` | `EventInfo` | — |
| `filter` | `IAttributeFilter` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `IEventData` | — |

</div>

### CreateFieldData(FieldInfo, IAttributeFilter) {#method-createfielddata-fieldinfo-iattributefilter}

``` csharp
public abstract IFieldData CreateFieldData(FieldInfo fieldInfo, IAttributeFilter filter = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `fieldInfo` | `FieldInfo` | — |
| `filter` | `IAttributeFilter` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `IFieldData` | — |

</div>

### CreateMethodData(MethodInfo, IAttributeFilter) {#method-createmethoddata-methodinfo-iattributefilter}

``` csharp
public abstract IMethodData CreateMethodData(MethodInfo methodInfo, IAttributeFilter filter = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `methodInfo` | `MethodInfo` | — |
| `filter` | `IAttributeFilter` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `IMethodData` | — |

</div>

### CreatePropertyData(PropertyInfo, IAttributeFilter) {#method-createpropertydata-propertyinfo-iattributefilter}

``` csharp
public abstract IPropertyData CreatePropertyData(PropertyInfo propertyInfo, IAttributeFilter filter = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `propertyInfo` | `PropertyInfo` | — |
| `filter` | `IAttributeFilter` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `IPropertyData` | — |

</div>

### CreateTypeData(Type, IAnalysisDataFactory, IAttributeFilter) {#method-createtypedata-type-ianalysisdatafactory-iattributefilter}

创建类型数据

``` csharp
public abstract ITypeData CreateTypeData(Type type, IAnalysisDataFactory factory = null, IAttributeFilter filter = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | — |
| `factory` | `IAnalysisDataFactory` | — |
| `filter` | `IAttributeFilter` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `ITypeData` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
