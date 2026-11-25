---
title: IGenericLocator<T>
description: "Runestone.AesirArchitecture.IGenericLocator<T> 的 API 文档"
---

# `IGenericLocator<T>`

!!! note ""

    - **种类:** `interface`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**实现接口:** `System.IDisposable`

**类型参数**

- `T` — 定位器管理的基类型，所有注册的实例必须可赋值给该类型。

## 声明

``` csharp
public interface IGenericLocator<T> : System.IDisposable where T : class
```

泛型定位器接口。提供按类型注册、查询与获取对象实例的契约。

**备注**

定位器的抽象契约，定义了注册、查询、获取与注销实例的标准接口。 GenericLocator{T} 是其默认实现，内部以 Dictionary{TKey,TValue} 存储注册关系。

注册与查询须使用相同的类型参数。若以具体类型注册（如 Register<Sword>）， 再以接口类型查询（如 Get<IWeapon>），将返回 null。

继承 IDisposable 而非另行声明 Dispose，目的是让"清空容器"成为契约的一部分： Dispose 只持有 IGenericLocator<T> 抽象， 需要经接口而非具体实现清空。清空仅解除注册关系，不销毁被注册的实例—— 实例的释放由调用方（如 Context 逆序 Dispose 模块）负责。

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetAllEntries()`](#method-getallentries) | 按注册键获取所有已注册的键值对（诊断用途）。 |
| [`GetAll()`](#method-getall) | 按注册顺序获取所有已注册的实例。 |
| [`Get()`](#method-get) | 获取已注册的实例，不存在则返回 null。 |
| [`TryGet(ref TItem)`](#method-tryget-ref-titem) | 尝试获取已注册的实例。返回是否成功找到对应类型的注册。 |
| [`Register(Type, T)`](#method-register-type-t) | 注册实例，以 Type 作为键。重复注册将覆盖已有实例。 |
| [`Register(TItem)`](#method-register-titem) | 注册实例，以 typeof(TItem) 作为键。重复注册将覆盖已有实例。 |
| [`Unregister()`](#method-unregister) | 注销指定类型的注册。 |

</div>

### GetAllEntries() {#method-getallentries}

按注册键获取所有已注册的键值对（诊断用途）。

**备注**

正常查询请使用 Get{TItem} / TryGet{TItem}。 此成员服务"近失识别"类诊断——例如按实现类注册、按接口查询失败时， 需要遍历注册键值对识别"已注册实例可赋值给查询类型"的近失情况并生成提示。
与 GetAll 一致地返回调用时刻的物化快照：枚举期间修改定位器不会抛"集合已修改"异常。

``` csharp
public abstract IEnumerable<KeyValuePair<Type, T>> GetAllEntries()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `IEnumerable<KeyValuePair<Type, T>>` | 注册键 Type 与实例的键值对枚举，不保证顺序。 |

</div>

### GetAll() {#method-getall}

按注册顺序获取所有已注册的实例。

**备注**

返回调用时刻的完整快照：之后的注册/注销不影响已返回的枚举， 消费端可在枚举期间安全地修改定位器（例如模块初始化过程中动态注册新模块，不会抛"集合已修改"异常）。 与诊断成员 GetAllEntries 取同一份快照语义，两者修改安全性契约一致。

``` csharp
public abstract IEnumerable<T> GetAll()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `IEnumerable<T>` | 所有已注册实例的 IEnumerable{T} 集合，不含类型键，按注册顺序排列。 |

</div>

### Get() {#method-get}

获取已注册的实例，不存在则返回 null。

``` csharp
public abstract TItem Get<TItem>()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `TItem` | 已注册的实例；若未注册则返回 null。 |

</div>

### TryGet(ref TItem) {#method-tryget-ref-titem}

尝试获取已注册的实例。返回是否成功找到对应类型的注册。

``` csharp
public abstract bool TryGet<TItem>(out ref TItem instance)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `instance` | `ref TItem` | 找到时输出已注册的实例；未找到时输出 null。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 成功找到则返回 true；未注册则返回 false。 |

</div>

### Register(Type, T) {#method-register-type-t}

注册实例，以 Type 作为键。重复注册将覆盖已有实例。

``` csharp
public abstract void Register(Type type, T instance)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `type` | `Type` | 注册时使用的键类型，实例必须可赋值给该类型。 |
| `instance` | `T` | 要注册的实例。 |

</div>

### Register(TItem) {#method-register-titem}

注册实例，以 typeof(TItem) 作为键。重复注册将覆盖已有实例。

``` csharp
public abstract void Register<TItem>(TItem instance)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `instance` | `TItem` | 要注册的实例。 |

</div>

### Unregister() {#method-unregister}

注销指定类型的注册。

``` csharp
public abstract void Unregister<TItem>()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
