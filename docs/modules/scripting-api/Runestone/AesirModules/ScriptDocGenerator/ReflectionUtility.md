---
title: ReflectionUtility
description: "Runestone.AesirModules.ScriptDocGenerator.ReflectionUtility 的 API 文档"
---

# `ReflectionUtility`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `ReflectionUtility`

## 声明

``` csharp
public static class ReflectionUtility
```

反射工具类，提供程序集、命名空间及成员的反射操作方法

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetAssembliesOfNameContainString(string)`](#method-getassembliesofnamecontainstring-string) | 获取名称中包含指定字符串的所有程序集 |
| [`GetAttributes(ICustomAttributeProvider)`](#method-getattributes-icustomattributeprovider) | 获取成员上的指定类型特性。 |
| [`GetAttributes(ICustomAttributeProvider, bool)`](#method-getattributes-icustomattributeprovider-bool) | 获取成员上的指定类型特性。 |
| [`GetBaseClasses(Type, bool)`](#method-getbaseclasses-type-bool) | 获取类型的所有基类。 |
| [`GetBaseTypes(Type, bool)`](#method-getbasetypes-type-bool) | 获取类型的所有基类和接口。 |
| [`GetNamespacesInAssembly(Assembly)`](#method-getnamespacesinassembly-assembly) | 获取指定程序集中的所有命名空间 |
| [`GetReturnType(MemberInfo)`](#method-getreturntype-memberinfo) | 获取成员的返回类型（支持字段、属性、方法和事件）。 |
| [`IsExtensionMethod(MethodBase)`](#method-isextensionmethod-methodbase) | 判断方法是否为扩展方法。 |
| [`IsStatic(MemberInfo)`](#method-isstatic-memberinfo) | 判断成员是否为静态成员。 |
| [`GetMemberValue(MemberInfo, object)`](#method-getmembervalue-memberinfo-object) | 获取成员的值（支持字段和属性）。 |

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

### GetAssembliesOfNameContainString(string) {#method-getassembliesofnamecontainstring-string}

获取名称中包含指定字符串的所有程序集

``` csharp
public static Assembly[] GetAssembliesOfNameContainString(string partOfAssemblyName)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `partOfAssemblyName` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `Assembly[]` | — |

</div>

### GetAttributes(ICustomAttributeProvider) {#method-getattributes-icustomattributeprovider}

获取成员上的指定类型特性。

``` csharp
public static IEnumerable<T> GetAttributes<T>(ICustomAttributeProvider member)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `member` | `ICustomAttributeProvider` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `IEnumerable<T>` | — |

</div>

### GetAttributes(ICustomAttributeProvider, bool) {#method-getattributes-icustomattributeprovider-bool}

获取成员上的指定类型特性。

``` csharp
public static IEnumerable<T> GetAttributes<T>(ICustomAttributeProvider member, bool inherit)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `member` | `ICustomAttributeProvider` | — |
| `inherit` | `bool` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `IEnumerable<T>` | — |

</div>

### GetBaseClasses(Type, bool) {#method-getbaseclasses-type-bool}

获取类型的所有基类。

``` csharp
[IteratorStateMachine]
public static IEnumerable<Type> GetBaseClasses(Type type, bool includeSelf = false)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | — |
| `includeSelf` | `bool` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `IEnumerable<Type>` | — |

</div>

### GetBaseTypes(Type, bool) {#method-getbasetypes-type-bool}

获取类型的所有基类和接口。

``` csharp
public static IEnumerable<Type> GetBaseTypes(Type type, bool includeSelf = false)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | — |
| `includeSelf` | `bool` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `IEnumerable<Type>` | — |

</div>

### GetNamespacesInAssembly(Assembly) {#method-getnamespacesinassembly-assembly}

获取指定程序集中的所有命名空间

``` csharp
public static List<string> GetNamespacesInAssembly(Assembly assembly)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `assembly` | `Assembly` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `List<string>` | — |

</div>

### GetReturnType(MemberInfo) {#method-getreturntype-memberinfo}

获取成员的返回类型（支持字段、属性、方法和事件）。

``` csharp
public static Type GetReturnType(MemberInfo memberInfo)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `memberInfo` | `MemberInfo` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `Type` | — |

</div>

### IsExtensionMethod(MethodBase) {#method-isextensionmethod-methodbase}

判断方法是否为扩展方法。

``` csharp
public static bool IsExtensionMethod(MethodBase method)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `method` | `MethodBase` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### IsStatic(MemberInfo) {#method-isstatic-memberinfo}

判断成员是否为静态成员。

``` csharp
public static bool IsStatic(MemberInfo member)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `member` | `MemberInfo` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### GetMemberValue(MemberInfo, object) {#method-getmembervalue-memberinfo-object}

获取成员的值（支持字段和属性）。

``` csharp
public static object GetMemberValue(MemberInfo member, object obj)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `member` | `MemberInfo` | — |
| `obj` | `object` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `object` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
