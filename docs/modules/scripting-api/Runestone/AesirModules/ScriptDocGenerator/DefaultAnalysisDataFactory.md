---
title: DefaultAnalysisDataFactory
description: "Runestone.AesirModules.ScriptDocGenerator.DefaultAnalysisDataFactory 的 API 文档"
---

# `DefaultAnalysisDataFactory`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `DefaultAnalysisDataFactory`

**实现接口:** `Runestone.AesirModules.ScriptDocGenerator.IAnalysisDataFactory`

## 声明

``` csharp
[Serializable]
public class DefaultAnalysisDataFactory : Runestone.AesirModules.ScriptDocGenerator.IAnalysisDataFactory
```

Aesir Modules 默认提供的解析数据工厂实现类

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`DefaultAnalysisDataFactory()`](#constructor-defaultanalysisdatafactory) | — |

</div>

### DefaultAnalysisDataFactory() {#constructor-defaultanalysisdatafactory}

``` csharp
public DefaultAnalysisDataFactory()
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`CreateConstructorData(ConstructorInfo, IAttributeFilter)`](#method-createconstructordata-constructorinfo-iattributefilter) | 创建构造函数数据 |
| [`CreateEventData(EventInfo, IAttributeFilter)`](#method-createeventdata-eventinfo-iattributefilter) | 创建事件数据 |
| [`CreateFieldData(FieldInfo, IAttributeFilter)`](#method-createfielddata-fieldinfo-iattributefilter) | 创建字段数据 |
| [`CreateMethodData(MethodInfo, IAttributeFilter)`](#method-createmethoddata-methodinfo-iattributefilter) | 创建方法数据 |
| [`CreatePropertyData(PropertyInfo, IAttributeFilter)`](#method-createpropertydata-propertyinfo-iattributefilter) | 创建属性数据 |
| [`CreateTypeData(Type, IAnalysisDataFactory, IAttributeFilter)`](#method-createtypedata-type-ianalysisdatafactory-iattributefilter) | 创建类型数据 |

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

### CreateConstructorData(ConstructorInfo, IAttributeFilter) {#method-createconstructordata-constructorinfo-iattributefilter}

创建构造函数数据

``` csharp
public IConstructorData CreateConstructorData(ConstructorInfo constructorInfo, IAttributeFilter filter = null)
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

创建事件数据

``` csharp
public IEventData CreateEventData(EventInfo eventInfo, IAttributeFilter filter = null)
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

创建字段数据

``` csharp
public IFieldData CreateFieldData(FieldInfo fieldInfo, IAttributeFilter filter = null)
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

创建方法数据

``` csharp
public IMethodData CreateMethodData(MethodInfo methodInfo, IAttributeFilter filter = null)
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

创建属性数据

``` csharp
public IPropertyData CreatePropertyData(PropertyInfo propertyInfo, IAttributeFilter filter = null)
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
public ITypeData CreateTypeData(Type type, IAnalysisDataFactory factory = null, IAttributeFilter filter = null)
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
