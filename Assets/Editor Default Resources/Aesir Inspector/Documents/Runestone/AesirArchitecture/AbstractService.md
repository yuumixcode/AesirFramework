# `AbstractService`

## 介绍

- 种类: `abstract class`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
[Serializable]
public abstract class AbstractService : Runestone.AesirArchitecture.AbstractSubmodule, 
Runestone.AesirArchitecture.IContextHolder, 
Runestone.AesirArchitecture.IService, 
Runestone.AesirArchitecture.ICanSetContext, 
Runestone.AesirArchitecture.ICanInitialize, 
Runestone.AesirArchitecture.ICanGetModel, 
Runestone.AesirArchitecture.ICanGetService, 
System.IDisposable
```

### 注释

- Service 基类。继承 AbstractSubmodule 获得生命周期管理，实现 IService 标记服务层角色。

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public Type GetType()` |
| `public virtual bool Equals(object obj)` |
| `public virtual int GetHashCode()` |
| `public virtual string ToString()` |
| `public virtual void Dispose()` |
| `protected object MemberwiseClone()` |
| `protected virtual void Finalize()` |
| `protected virtual void OnDispose()` |
| `protected virtual void OnInitialize()` |

### 继承的普通方法

| 普通方法名称 | 注释 | 声明方法的类 |
| :--- | :--- | :--- |
| `public Type GetType` |  | `System.Object` |
| `public virtual bool Equals` |  | `System.Object` |
| `public virtual int GetHashCode` |  | `System.Object` |
| `public virtual string ToString` |  | `System.Object` |
| `public virtual void Dispose` | 释放资源，触发 OnDispose | `Runestone.AesirArchitecture.AbstractSubmodule` |
| `protected object MemberwiseClone` |  | `System.Object` |
| `protected virtual void Finalize` |  | `System.Object` |
| `protected virtual void OnDispose` | 释放时的清理逻辑，子类可覆写 | `Runestone.AesirArchitecture.AbstractSubmodule` |
| `protected virtual void OnInitialize` | 初始化逻辑，子类必须实现 | `Runestone.AesirArchitecture.AbstractSubmodule` |

## 属性

### 继承的属性

| 属性签名 | 注释 | 声明属性的类 | 
| :--- | :--- | :--- |
| `public bool Initialized { get; }` | 是否已初始化（只读） | `Runestone.AesirArchitecture.AbstractSubmodule` |

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Aesir Inspector`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。
