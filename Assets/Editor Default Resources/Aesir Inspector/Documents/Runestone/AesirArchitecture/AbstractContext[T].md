# `AbstractContext<T>`

## 介绍

- 种类: `abstract class`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
[Serializable]
public abstract class AbstractContext<T> : Runestone.AesirArchitecture.IContext, 
System.IDisposable where T : new(), Runestone.AesirArchitecture.AbstractContext<T>
```

### 注释

- 上下文基类。纯 C# 实现，不依赖 MonoBehaviour。 子类在 Configure 中注册 Model 和 Service，通过 Interface 获取全局单例。

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public IEnumerable<IModel> GetAllModels()` |
| `public IEnumerable<IService> GetAllServices()` |
| `public TModel GetModel<TModel>()` |
| `public TService GetService<TService>()` |
| `public Type GetType()` |
| `public virtual bool Equals(object obj)` |
| `public virtual int GetHashCode()` |
| `public virtual string ToString()` |
| `public void Dispose()` |
| `public void Initialize()` |
| `public void RegisterModel<TModel>(TModel model)` |
| `public void RegisterService<TService>(TService service)` |
| `protected abstract void Configure()` |
| `protected object MemberwiseClone()` |
| `protected virtual void Finalize()` |
| `protected virtual void OnDispose()` |

### 声明的普通方法

| 普通方法名称 | 注释 |
| :--- | :--- | 
| `public IEnumerable<IModel> GetAllModels` | 获取所有已注册的 Model 列表 |
| `public IEnumerable<IService> GetAllServices` | 获取所有已注册的 Service 列表 |
| `public TModel GetModel<TModel>` | 获取已注册的 Model。 |
| `public TService GetService<TService>` | 获取已注册的 Service。 |
| `public void Dispose` | 释放资源。逆序销毁 Service 和 Model，清空容器。 |
| `public void Initialize` | 统一初始化。调用 Configure 注册模块后，按注册顺序依次初始化 Model 和 Service。 开发者需保证注册顺序满足依赖关系——被依赖的模块先注册。运行时通过 GetModel / GetService 获取未注册模块会抛出异常。 |
| `public void RegisterModel<TModel>` | 注册 Model 并绑定上下文。 若该类型已注册，旧实例会被 Dispose 后再覆盖，避免事件订阅等资源泄漏。 |
| `public void RegisterService<TService>` | 注册 Service 并绑定上下文。 若上下文已完成统一初始化，则立即初始化该 Service。若该类型已注册，旧实例会被 Dispose 后再覆盖，避免资源泄漏。 |
| `protected abstract void Configure` | 配置上下文模块，子类在此注册 Model 和 Service。 |
| `protected virtual void OnDispose` | 子类可选覆写，在释放前执行自定义清理 |

### 继承的普通方法

| 普通方法名称 | 注释 | 声明方法的类 |
| :--- | :--- | :--- |
| `public Type GetType` |  | `System.Object` |
| `public virtual bool Equals` |  | `System.Object` |
| `public virtual int GetHashCode` |  | `System.Object` |
| `public virtual string ToString` |  | `System.Object` |
| `protected object MemberwiseClone` |  | `System.Object` |
| `protected virtual void Finalize` |  | `System.Object` |

## 属性

### 声明的属性

| 属性签名 | 注释 |
| :--- | :--- |
| `public bool Initialized { get; private set; }` | 是否已初始化（只读） |
| `public static IContext Interface { get; }` | 获取当前上下文类型的单例接口实例。首次访问时自动创建并初始化。 |

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Aesir Inspector`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。
