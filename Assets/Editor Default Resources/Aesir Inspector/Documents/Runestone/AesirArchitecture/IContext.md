# `IContext`

## 介绍

- 种类: `interface`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
public interface IContext : System.IDisposable
```

### 注释

- 模块上下文接口。提供模块注册与获取。

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public abstract IEnumerable<IModel> GetAllModels()` |
| `public abstract IEnumerable<IService> GetAllServices()` |
| `public abstract T GetModel<T>()` |
| `public abstract T GetService<T>()` |
| `public abstract void RegisterModel<T>(T model)` |
| `public abstract void RegisterService<T>(T service)` |

### 声明的普通方法

| 普通方法名称 | 注释 |
| :--- | :--- | 
| `public abstract IEnumerable<IModel> GetAllModels` | 获取所有已注册的 Model 列表 |
| `public abstract IEnumerable<IService> GetAllServices` | 获取所有已注册的 Service 列表 |
| `public abstract T GetModel<T>` | 获取已注册的 Model |
| `public abstract T GetService<T>` | 获取已注册的 Service |
| `public abstract void RegisterModel<T>` | 注册 Model |
| `public abstract void RegisterService<T>` | 注册 Service |

## 属性

### 声明的属性

| 属性签名 | 注释 |
| :--- | :--- |
| `public bool Initialized { get; }` |  |

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Aesir Inspector`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。
