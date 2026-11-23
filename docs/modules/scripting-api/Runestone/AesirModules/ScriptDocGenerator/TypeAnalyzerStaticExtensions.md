---
title: TypeAnalyzerStaticExtensions
description: "Runestone.AesirModules.ScriptDocGenerator.TypeAnalyzerStaticExtensions 的 API 文档"
---

# `TypeAnalyzerStaticExtensions`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules.ScriptDocGenerator`
    - **程序集:** `Runestone.AesirModules.OdinInspector`

**继承链:** `System.Object` → `TypeAnalyzerStaticExtensions`

## 声明

``` csharp
[Extension]
public static class TypeAnalyzerStaticExtensions
```

类型分析器静态扩展类，统一管理类型分析器有关的静态扩展方法

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetEventAccessModifierType(EventInfo)`](#method-geteventaccessmodifiertype-eventinfo) | 获取事件的访问修饰符类型 |
| [`GetFieldAccessModifier(FieldInfo)`](#method-getfieldaccessmodifier-fieldinfo) | 获取字段访问修饰符 |
| [`GetMethodAccessModifierType(MethodBase)`](#method-getmethodaccessmodifiertype-methodbase) | 获取方法的访问修饰符类型 |
| [`GetPropertyAccessModifierType(PropertyInfo)`](#method-getpropertyaccessmodifiertype-propertyinfo) | 获取属性的访问修饰符类型 |
| [`GetTypeAccessModifier(Type)`](#method-gettypeaccessmodifier-type) | 获取类型的访问修饰符 |
| [`GetUserDefinedFields(Type)`](#method-getuserdefinedfields-type) | 获取开发者声明的字段，剔除自动属性生成的字段 |
| [`GetTypeCategory(Type)`](#method-gettypecategory-type) | 获取类型的种类 |
| [`IsAbstractOrInterface(Type)`](#method-isabstractorinterface-type) | 判断一个类型是否为抽象类或接口 |
| [`IsApiMember(IDerivedMemberData)`](#method-isapimember-iderivedmemberdata) | 判断是否为 API 成员，返回 true 表示是 API 成员，返回 false 表示不是。API 成员指的是公共成员或受保护成员。 |
| [`IsAsyncMethod(MethodBase)`](#method-isasyncmethod-methodbase) | 判断方法是否是异步方法 |
| [`IsDelegate(Type)`](#method-isdelegate-type) | 判断指定类型是否为委托类型 |
| [`IsDynamicField(FieldInfo)`](#method-isdynamicfield-fieldinfo) | 判断是否为动态字段 |
| [`IsFromInheritance(MemberInfo)`](#method-isfrominheritance-memberinfo) | 判断成员是否从继承中获取，这里的成员不包括 Type 类型 |
| [`IsFromInterfaceImplementMethod(MethodBase)`](#method-isfrominterfaceimplementmethod-methodbase) | 判断是否为接口的实现方法 |
| [`IsInheritedOverrideFromAncestor(MethodInfo, Type)`](#method-isinheritedoverridefromancestor-methodinfo-type) | 判断方法是否为从祖先类继承的重写方法，重写声明不是在当前类中 |
| [`IsOperatorMethod(MethodBase)`](#method-isoperatormethod-methodbase) | 判断方法是否是运算符方法 |
| [`IsOverrideMethod(MethodInfo)`](#method-isoverridemethod-methodinfo) | 方法是否具有 override 的特性 |
| [`IsRecord(Type)`](#method-isrecord-type) | 判断指定类型是否为 record（包括 record class 和 record struct） |
| [`IsRecordStruct(Type)`](#method-isrecordstruct-type) | 判断类型是否为 record struct（值类型 record） |
| [`IsReferenceTypeExcludeString(Type)`](#method-isreferencetypeexcludestring-type) | 判断一个类型是否为非字符串的引用类型（非值类型） |
| [`IsStaticProperty(PropertyInfo)`](#method-isstaticproperty-propertyinfo) | 判断是否为静态属性 |
| [`TryAsIMemberData(IDerivedMemberData, ref IMemberData)`](#method-tryasimemberdata-iderivedmemberdata-ref-imemberdata) | 将 IDerivedMemberData 转换为 IMemberData，转换成功返回 true，转换失败返回 false |
| [`TryGetFieldCustomDefaultValue(FieldInfo, ref Object)`](#method-trygetfieldcustomdefaultvalue-fieldinfo-ref-object) | 获取字段的自定义默认值，不能获取到值则返回 null。只获取静态字段和常量字段的默认值。 |
| [`TryGetPropertyCustomDefaultValue(PropertyInfo, ref Object)`](#method-trygetpropertycustomdefaultvalue-propertyinfo-ref-object) | 获取属性的自定义默认值，不能获取到值则返回 null，只获取静态属性的默认值。 |
| [`GetAttributesDeclarationWithMultiLine(MemberInfo, IAttributeFilter)`](#method-getattributesdeclarationwithmultiline-memberinfo-iattributefilter) | 获取特性声明字符串，多行显示 |
| [`GetMethodNameAndParameters(MethodBase)`](#method-getmethodnameandparameters-methodbase) | 获取方法名称和参数列表，不包含返回值和修饰符 |
| [`GetParametersNameWithDefaultValue(MethodBase)`](#method-getparametersnamewithdefaultvalue-methodbase) | 获取方法的参数签名，包含默认值 |
| [`GetReadableTypeName(Type, bool)`](#method-getreadabletypename-type-bool) | 将反射获取到的系统类型名称转换为人类可读的 C# 风格类型名称 |
| [`GetInheritanceChain(Type)`](#method-getinheritancechain-type) | 获取一个类型的继承链，不包括接口 |
| [`GetInterfaceArray(Type)`](#method-getinterfacearray-type) | 获取一个类型继承的所有接口 |
| [`GetReferenceLinks(Type)`](#method-getreferencelinks-type) | 获取一个数组，内容是所有的 ReferenceLinkURL 特性中的网页链接字符串 |

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

### GetEventAccessModifierType(EventInfo) {#method-geteventaccessmodifiertype-eventinfo}

获取事件的访问修饰符类型

``` csharp
[Extension]
[Ext] public static AccessModifierType GetEventAccessModifierType(this EventInfo eventInfo)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `eventInfo` | `EventInfo` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AccessModifierType` | — |

</div>

### GetFieldAccessModifier(FieldInfo) {#method-getfieldaccessmodifier-fieldinfo}

获取字段访问修饰符

``` csharp
[Extension]
[Ext] public static AccessModifierType GetFieldAccessModifier(this FieldInfo fieldInfo)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `fieldInfo` | `FieldInfo` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AccessModifierType` | — |

</div>

### GetMethodAccessModifierType(MethodBase) {#method-getmethodaccessmodifiertype-methodbase}

获取方法的访问修饰符类型

``` csharp
[Extension]
[Ext] public static AccessModifierType GetMethodAccessModifierType(this MethodBase method)
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
| `AccessModifierType` | — |

</div>

### GetPropertyAccessModifierType(PropertyInfo) {#method-getpropertyaccessmodifiertype-propertyinfo}

获取属性的访问修饰符类型

``` csharp
[Extension]
[Ext] public static AccessModifierType GetPropertyAccessModifierType(this PropertyInfo propertyInfo)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `propertyInfo` | `PropertyInfo` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AccessModifierType` | — |

</div>

### GetTypeAccessModifier(Type) {#method-gettypeaccessmodifier-type}

获取类型的访问修饰符

``` csharp
[Extension]
[Ext] public static AccessModifierType GetTypeAccessModifier(this Type type)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AccessModifierType` | — |

</div>

### GetUserDefinedFields(Type) {#method-getuserdefinedfields-type}

获取开发者声明的字段，剔除自动属性生成的字段

``` csharp
[Extension]
[Ext] public static FieldInfo[] GetUserDefinedFields(this Type type)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `FieldInfo[]` | — |

</div>

### GetTypeCategory(Type) {#method-gettypecategory-type}

获取类型的种类

**备注**

record class 与 record struct 统一映射 Record（枚举有意不拆分， 避免文档种类分组出现两种 record 条目）；两者的区分在签名生成层完成—— IsRecordStruct 为真时签名追加 "struct " 前缀（"record struct Foo"）。

``` csharp
[Extension]
[Ext] public static TypeCategory GetTypeCategory(this Type type)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `TypeCategory` | — |

</div>

### IsAbstractOrInterface(Type) {#method-isabstractorinterface-type}

判断一个类型是否为抽象类或接口

``` csharp
[Extension]
[Ext] public static bool IsAbstractOrInterface(this Type type)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### IsApiMember(IDerivedMemberData) {#method-isapimember-iderivedmemberdata}

判断是否为 API 成员，返回 true 表示是 API 成员，返回 false 表示不是。API 成员指的是公共成员或受保护成员。

``` csharp
[Extension]
[Ext] public static bool IsApiMember(this IDerivedMemberData derivedMemberData)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `derivedMemberData` | `IDerivedMemberData` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### IsAsyncMethod(MethodBase) {#method-isasyncmethod-methodbase}

判断方法是否是异步方法

``` csharp
[Extension]
[Ext] public static bool IsAsyncMethod(this MethodBase methodBase)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `methodBase` | `MethodBase` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### IsDelegate(Type) {#method-isdelegate-type}

判断指定类型是否为委托类型

``` csharp
[Extension]
[Ext] public static bool IsDelegate(this Type type)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### IsDynamicField(FieldInfo) {#method-isdynamicfield-fieldinfo}

判断是否为动态字段

``` csharp
[Extension]
[Ext] public static bool IsDynamicField(this FieldInfo fieldInfo)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `fieldInfo` | `FieldInfo` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### IsFromInheritance(MemberInfo) {#method-isfrominheritance-memberinfo}

判断成员是否从继承中获取，这里的成员不包括 Type 类型

``` csharp
[Extension]
[Ext] public static bool IsFromInheritance(this MemberInfo member)
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

### IsFromInterfaceImplementMethod(MethodBase) {#method-isfrominterfaceimplementmethod-methodbase}

判断是否为接口的实现方法

``` csharp
[Extension]
[Ext] public static bool IsFromInterfaceImplementMethod(this MethodBase method)
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

### IsInheritedOverrideFromAncestor(MethodInfo, Type) {#method-isinheritedoverridefromancestor-methodinfo-type}

判断方法是否为从祖先类继承的重写方法，重写声明不是在当前类中

``` csharp
[Extension]
[Ext] public static bool IsInheritedOverrideFromAncestor(this MethodInfo method, Type currentType)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `method` | `MethodInfo` | — |
| `currentType` | `Type` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### IsOperatorMethod(MethodBase) {#method-isoperatormethod-methodbase}

判断方法是否是运算符方法

``` csharp
[Extension]
[Ext] public static bool IsOperatorMethod(this MethodBase methodInfo)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `methodInfo` | `MethodBase` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### IsOverrideMethod(MethodInfo) {#method-isoverridemethod-methodinfo}

方法是否具有 override 的特性

``` csharp
[Extension]
[Ext] public static bool IsOverrideMethod(this MethodInfo methodInfo)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `methodInfo` | `MethodInfo` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### IsRecord(Type) {#method-isrecord-type}

判断指定类型是否为 record（包括 record class 和 record struct）

**备注**

检测依据：编译器为所有 record 生成的合成方法 <Clone>$（C# 9 起的 Roslyn 契约， record class 与 record struct 均携带）。反射层无 "IsRecord" 原生 API，此为社区通行判定； 代价是绑定编译器实现细节——若未来编译器改变合成方法命名，此处会整体失判（表现为 record 被归类为普通 class/struct，无崩溃）。

``` csharp
[Extension]
[Ext] public static bool IsRecord(this Type type)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### IsRecordStruct(Type) {#method-isrecordstruct-type}

判断类型是否为 record struct（值类型 record）

``` csharp
[Extension]
[Ext] public static bool IsRecordStruct(this Type type)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### IsReferenceTypeExcludeString(Type) {#method-isreferencetypeexcludestring-type}

判断一个类型是否为非字符串的引用类型（非值类型）

``` csharp
[Extension]
[Ext] public static bool IsReferenceTypeExcludeString(this Type type)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### IsStaticProperty(PropertyInfo) {#method-isstaticproperty-propertyinfo}

判断是否为静态属性

``` csharp
[Extension]
[Ext] public static bool IsStaticProperty(this PropertyInfo propertyInfo)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `propertyInfo` | `PropertyInfo` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### TryAsIMemberData(IDerivedMemberData, ref IMemberData) {#method-tryasimemberdata-iderivedmemberdata-ref-imemberdata}

将 IDerivedMemberData 转换为 IMemberData，转换成功返回 true，转换失败返回 false

``` csharp
[Extension]
[Ext] public static bool TryAsIMemberData(this IDerivedMemberData derivedMemberData, out ref IMemberData memberData)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `derivedMemberData` | `IDerivedMemberData` | — |
| `memberData` | `ref IMemberData` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### TryGetFieldCustomDefaultValue(FieldInfo, ref Object) {#method-trygetfieldcustomdefaultvalue-fieldinfo-ref-object}

获取字段的自定义默认值，不能获取到值则返回 null。只获取静态字段和常量字段的默认值。

``` csharp
[Extension]
[Ext] public static bool TryGetFieldCustomDefaultValue(this FieldInfo fieldInfo, out ref Object defaultValue)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `fieldInfo` | `FieldInfo` | — |
| `defaultValue` | `ref Object` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### TryGetPropertyCustomDefaultValue(PropertyInfo, ref Object) {#method-trygetpropertycustomdefaultvalue-propertyinfo-ref-object}

获取属性的自定义默认值，不能获取到值则返回 null，只获取静态属性的默认值。

``` csharp
[Extension]
[Ext] public static bool TryGetPropertyCustomDefaultValue(this PropertyInfo propertyInfo, out ref Object defaultValue)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `propertyInfo` | `PropertyInfo` | — |
| `defaultValue` | `ref Object` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### GetAttributesDeclarationWithMultiLine(MemberInfo, IAttributeFilter) {#method-getattributesdeclarationwithmultiline-memberinfo-iattributefilter}

获取特性声明字符串，多行显示

``` csharp
[Extension]
[Ext] public static string GetAttributesDeclarationWithMultiLine(this MemberInfo member, IAttributeFilter filter = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `member` | `MemberInfo` | — |
| `filter` | `IAttributeFilter` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### GetMethodNameAndParameters(MethodBase) {#method-getmethodnameandparameters-methodbase}

获取方法名称和参数列表，不包含返回值和修饰符

``` csharp
[Extension]
[Ext] public static string GetMethodNameAndParameters(this MethodBase method)
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
| `string` | — |

</div>

### GetParametersNameWithDefaultValue(MethodBase) {#method-getparametersnamewithdefaultvalue-methodbase}

获取方法的参数签名，包含默认值

``` csharp
[Extension]
[Ext] public static string GetParametersNameWithDefaultValue(this MethodBase method)
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
| `string` | — |

</div>

### GetReadableTypeName(Type, bool) {#method-getreadabletypename-type-bool}

将反射获取到的系统类型名称转换为人类可读的 C# 风格类型名称

``` csharp
[Extension]
[Ext] public static string GetReadableTypeName(this Type type, bool useFullName = false)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | — |
| `useFullName` | `bool` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### GetInheritanceChain(Type) {#method-getinheritancechain-type}

获取一个类型的继承链，不包括接口

``` csharp
[Extension]
[Ext] public static string[] GetInheritanceChain(this Type type)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string[]` | — |

</div>

### GetInterfaceArray(Type) {#method-getinterfacearray-type}

获取一个类型继承的所有接口

``` csharp
[Extension]
[Ext] public static string[] GetInterfaceArray(this Type type)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string[]` | — |

</div>

### GetReferenceLinks(Type) {#method-getreferencelinks-type}

获取一个数组，内容是所有的 ReferenceLinkURL 特性中的网页链接字符串

``` csharp
[Extension]
[Ext] public static string[] GetReferenceLinks(this Type type)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string[]` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
