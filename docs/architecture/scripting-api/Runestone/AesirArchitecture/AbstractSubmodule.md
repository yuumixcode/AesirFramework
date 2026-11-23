---
title: AbstractSubmodule
description: "Runestone.AesirArchitecture.AbstractSubmodule 的 API 文档"
---

# `AbstractSubmodule`

!!! note ""

    - **种类:** `abstract class`
    - **命名空间:** `Runestone.AesirArchitecture`
    - **程序集:** `Runestone.AesirArchitecture`

**继承链:** `System.Object` → `AbstractSubmodule`

**实现接口:** `Runestone.AesirArchitecture.IContextHolder`，`Runestone.AesirArchitecture.ICanSetContext`，`Runestone.AesirArchitecture.ICanInitialize`，`System.IDisposable`

## 声明

``` csharp
[Serializable]
public abstract class AbstractSubmodule : Runestone.AesirArchitecture.IContextHolder, 
Runestone.AesirArchitecture.ICanSetContext, 
Runestone.AesirArchitecture.ICanInitialize, 
System.IDisposable
```

子模块基类。持有上下文引用，通过 OnInitialize 和 OnDispose 管理生命周期。
Model 和 Service 的公共逻辑统一在此实现。

**备注**

作为 AbstractModel 和 AbstractService 的公共基类， 统一管理上下文引用和生命周期。上下文通过显式接口实现 SetContext 注入， 供子类经由能力扩展方法（如 GetModel<T>、GetService<T>）访问其他模块。 生命周期由 OnInitialize 和 OnDispose 两个虚方法控制， 子类按需覆写即可在初始化和释放阶段执行自定义逻辑。

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Initialized`](#property-initialized) | 是否已初始化（只读） |

</div>

### Initialized {#property-initialized}

是否已初始化（只读）

**备注**

由 Initialize 在调用 OnInitialize 之后设为 true； Dispose 释放后重置为 false——已释放的模块不再自称已初始化， 防止动态替换场景下旧实例的初始化状态产生误导。

``` csharp
public bool Initialized { get; private set; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Dispose()`](#method-dispose) | 释放资源，触发 OnDispose |
| [`OnDispose()`](#method-ondispose) | 释放时的清理逻辑，子类可覆写 |
| [`OnInitialize()`](#method-oninitialize) | 初始化逻辑，子类可选覆写（默认空实现）。 |

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

### Dispose() {#method-dispose}

释放资源，触发 OnDispose

**备注**

先调用 OnDispose 执行子类清理逻辑，随后断开上下文引用并将 Initialized 重置为 false（重置语义见该属性说明）。
幂等：重复调用直接早退，OnDispose 只执行一次——与 AbstractContext.Dispose 的释放守卫同构，避免框架生命周期释放与手动释放叠加时重复取消订阅 / 重复释放资源。

``` csharp
public void Dispose()
```

### OnDispose() {#method-ondispose}

释放时的清理逻辑，子类可覆写

**备注**

默认空实现。子类覆写此方法以执行清理逻辑，例如取消订阅、释放资源或保存状态。 此方法在 Dispose 中被调用，执行完毕后上下文引用将被清空。

``` csharp
protected virtual void OnDispose()
```

### OnInitialize() {#method-oninitialize}

初始化逻辑，子类可选覆写（默认空实现）。

**备注**

子类按需覆写此方法以执行初始化逻辑，例如创建 ObservableValue{T}、 订阅事件或加载持久化数据；无初始化需求的子类可不覆写（默认空实现）。 此方法在 Initialize 中被调用， 调用完成后 Initialized 自动置为 true。
区别于 AbstractCommand.OnExecute / AbstractQuery.OnExecute（abstract，子类必须实现）—— 本方法为 virtual 空实现，覆写是可选的。

``` csharp
protected virtual void OnInitialize()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
