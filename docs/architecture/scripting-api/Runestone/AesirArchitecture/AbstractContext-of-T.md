---
title: AbstractContext<T>
description: "Runestone.AesirArchitecture.AbstractContext<T> 的 API 文档"
---

# `AbstractContext<T>`

!!! note ""

    - **种类:** `abstract class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `AbstractContext<T>`

**实现接口:** `Runestone.AesirArchitecture.IContext`，`System.IDisposable`

**类型参数**

- `T` — 具体上下文子类类型，必须具有无参公共构造函数

## 声明

``` csharp
[Serializable]
public abstract class AbstractContext<T> : Runestone.AesirArchitecture.IContext, 
System.IDisposable where T : new(), Runestone.AesirArchitecture.AbstractContext<T>
```

上下文基类。纯 C# 实现，不依赖 MonoBehaviour。
子类在 Configure 中注册 Model 和 Service，通过 Instance 获取全局单例。

**备注**

本类采用泛型自引用模式（CRTP）：泛型约束 where T : AbstractContext<T>, new() 要求子类将自身作为类型参数传入，例如 class MyContext : AbstractContext<MyContext>。 这样 Instance 静态属性就能在编译期确定具体类型并返回其单例， 避免了反射或运行时类型查找的开销。

初始化流程（由 Instance 首次访问触发）： 创建子类实例 new T() 调用 Initialize Initialize 先调用 Configure，由子类注册全部 Model 和 Service 按注册顺序依次调用各 Model 的 Initialize 按注册顺序依次调用各 Service 的 Initialize

释放流程（由 Dispose 触发）： 逆序于初始化地先销毁所有 Service 再逆序于初始化地销毁所有 Model 清空 Model 与 Service 容器 先 Service 后 Model 的销毁顺序确保 Service 在销毁时仍可访问所依赖的 Model。

域加载安全：静态构造函数通过 Register(Action) 注册"释放并置空单例"的重置回调。当 Unity 关闭 Domain Reload（Enter Play Mode Settings） 时，静态字段不会被运行时自动清零，该回调确保下次进入 Play 模式时单例被正确重建。 之所以经助手注册而非类内声明 [RuntimeInitializeOnLoadMethod]：泛型类中的该方法特性 会被 Unity 静默跳过（不执行也不报错），只能由非泛型的中心位置代为触发。

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Initialized`](#property-initialized) | 是否已初始化（只读） |
| [`Instance`](#property-instance) | 获取当前上下文类型的单例接口实例。首次访问时自动创建并初始化。 |

</div>

### Initialized {#property-initialized}

是否已初始化（只读）

``` csharp
public bool Initialized { get; private set; }
```

### Instance {#property-instance}

获取当前上下文类型的单例接口实例。首次访问时自动创建并初始化。

**备注**

采用懒加载单例模式：首次访问时通过 new T() 创建实例并调用 Initialize， 初始化成功后才写入静态字段 _instance。后续访问直接返回缓存实例，不再重复初始化。
初始化失败：单例不会被缓存，后续每次访问都会重新创建并重新初始化， 根因异常每次抛出（而非只抛一次后拿到 Initialized 为 false 的坏上下文）。 已初始化到一半的模块不做回滚 Dispose，随被丢弃的实例交由 GC 回收——初始化失败属启动期编程错误， 应修复根因而非优雅降级。

重入约定：Configure 及各模块的初始化方法中禁止访问 Instance。 单例在 Initialize 完成后才发布，初始化期间访问 Instance 会再次触发 创建 → 初始化 → 访问的无限递归，最终栈溢出（StackOverflowException 不可捕获、直接崩溃进程）。 为此 Instance 设重入守卫：初始化期间检测到重入即抛含修复提示的 InvalidOperationException，把不可诊断的栈溢出转为可操作的快速失败。

该属性返回具体上下文类型 T， 访问 IContext 接口成员时自动向上转型，无需强转。

``` csharp
public static T Instance { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetModel()`](#method-getmodel) | 获取已注册的 Model。 |
| [`GetService()`](#method-getservice) | 获取已注册的 Service。 |
| [`Dispose()`](#method-dispose) | 释放资源。逆序销毁 Service 和 Model，清空容器。 |
| [`Initialize()`](#method-initialize) | 统一初始化。调用 Configure 注册模块后，按注册顺序依次初始化 Model 和 Service。 开发者需保证注册顺序满足依赖关系——被依赖的模块先注册。运行时通过 GetModel / GetService 获取未注册模块会抛出异常。 |
| [`RegisterModel(TModel)`](#method-registermodel-tmodel) | 注册 Model 并绑定上下文。 若该类型已注册，视为动态替换：输出一条 Warning 日志，旧实例会被 Dispose 后再覆盖。 |
| [`RegisterService(TService)`](#method-registerservice-tservice) | 注册 Service 并绑定上下文。 若上下文已完成统一初始化，则立即初始化该 Service。若该类型已注册，视为动态替换：输出一条 Warning 日志，旧实例会被 Dispose 后再覆盖。 |
| [`Configure()`](#method-configure) | 配置上下文模块，子类在此注册 Model 和 Service。 |
| [`OnDispose()`](#method-ondispose) | 子类可选覆写，在释放前执行自定义清理。 这是本类唯一的释放定制点——Dispose 非 virtual，不可覆写。 |

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

### GetModel() {#method-getmodel}

获取已注册的 Model。

``` csharp
public TModel GetModel<TModel>()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `TModel` | 已注册的 Model 实例 |

</div>

### GetService() {#method-getservice}

获取已注册的 Service。

``` csharp
public TService GetService<TService>()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `TService` | 已注册的 Service 实例 |

</div>

### Dispose() {#method-dispose}

释放资源。逆序销毁 Service 和 Model，清空容器。

**备注**

释放顺序与初始化顺序相反：先销毁所有 Service，再销毁所有 Model，最后清空两个容器。 各容器内部亦按注册的逆序销毁（后注册的先销毁），与"按注册顺序初始化"严格镜像。
先 Service 后 Model 的原因：Service 通常依赖 Model 完成自身逻辑， 在 Service 释放时仍可能需要读取 Model 状态，因此 Model 必须晚于 Service 销毁。

若上下文尚未初始化，此方法直接返回不做任何操作。

释放后解除 Instance 的单例缓存——再次访问 Instance 将重建并重新初始化全新上下文，而非返回已释放的空壳实例。

Reverse() 在关停路径产生一次枚举分配，属可接受的一次性开销。

不可覆写：本方法刻意非 virtual（模板方法），以保证"解除单例缓存 + 清空容器"这段 框架级收尾永远被执行到底，不会被子类的部分实现跳过。自定义的释放逻辑请写入 OnDispose——它是本类唯一的释放定制点。

异常安全：释放链路（OnDispose 与各模块的 Dispose，例如访问已被 上一局销毁的 MonoBehaviour）抛出的异常照旧向上传播（fail-fast），但收尾不变量放在 finally 中执行——置 Initialized 为 false 并解除单例缓存。 否则异常会让 Instance 长期指向半释放的上下文，且 Initialize 因 Initialized 早退而永不重建（与静态重置回调"先摘单例再释放"的硬化同构）。

``` csharp
public void Dispose()
```

### Initialize() {#method-initialize}

统一初始化。调用 Configure 注册模块后，按注册顺序依次初始化 Model 和 Service。
开发者需保证注册顺序满足依赖关系——被依赖的模块先注册。运行时通过 GetModel / GetService 获取未注册模块会抛出异常。

**备注**

此方法由 Instance 在首次访问时自动调用，通常不需要手动调用。
执行步骤： 调用 Configure，让子类在其中通过 RegisterModel{TModel} 和 RegisterService{TService} 注册所有模块。此阶段只登记，不初始化 （Initialized 尚未置位，注册不触发初始化） 按注册顺序遍历并初始化全部 Model（跳过已初始化者） 按注册顺序遍历并初始化全部 Service（跳过已初始化者）

初始化期注册：模块的 OnInitialize 中注册新模块时，GetAll() 的快照已经取定， 新模块不在本轮快照内；此时 Initialized 仍为 false，故注册不会触发即时初始化。 每一步都以「重复取快照直到没有未初始化模块」补齐，保证新增模块在本次 Initialize 返回前完成初始化且不重复初始化。

若已初始化则直接返回，保证幂等性。初始化过程中抛出的异常直接向上传播，不做回滚—— 初始化失败属启动期编程错误，应修复根因（见 Instance 备注）。

``` csharp
public void Initialize()
```

### RegisterModel(TModel) {#method-registermodel-tmodel}

注册 Model 并绑定上下文。
若该类型已注册，视为动态替换：输出一条 Warning 日志，旧实例会被 Dispose 后再覆盖。

**备注**

运行时替换 Model 属测试/调试用途，替换后旧实例上的订阅不会迁移——已订阅的 View 需自行重新订阅。 首次注册不输出日志，仅动态替换时输出 Warning 提醒（详见 LogReplacementWarning）。

``` csharp
public void RegisterModel<TModel>(TModel model)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `model` | `TModel` | 要注册的 Model 实例，注册后会绑定到当前上下文 |

</div>

### RegisterService(TService) {#method-registerservice-tservice}

注册 Service 并绑定上下文。
若上下文已完成统一初始化，则立即初始化该 Service。若该类型已注册，视为动态替换：输出一条 Warning 日志，旧实例会被 Dispose 后再覆盖。

**备注**

运行时替换 Service 属测试/调试用途，替换后旧实例上的订阅不会迁移——已订阅方需自行重新订阅。 首次注册不输出日志，仅动态替换时输出 Warning 提醒（详见 LogReplacementWarning）。

``` csharp
public void RegisterService<TService>(TService service)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `service` | `TService` | 要注册的 Service 实例，注册后会绑定到当前上下文 |

</div>

### Configure() {#method-configure}

配置上下文模块，子类在此注册 Model 和 Service。

``` csharp
protected abstract void Configure()
```

### OnDispose() {#method-ondispose}

子类可选覆写，在释放前执行自定义清理。
这是本类唯一的释放定制点——Dispose 非 virtual，不可覆写。

``` csharp
protected virtual void OnDispose()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
