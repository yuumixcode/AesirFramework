# `AbstractSubmodule`

## 介绍

- 种类: `abstract class`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
[Serializable]
public abstract class AbstractSubmodule : Runestone.AesirArchitecture.IContextHolder, 
Runestone.AesirArchitecture.ICanSetContext, 
Runestone.AesirArchitecture.ICanInitialize, 
System.IDisposable
```

### 注释

- 子模块基类。持有上下文引用，通过 OnInitialize 和 OnDispose 管理生命周期。 Model 和 Service 的公共逻辑统一在此实现。

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public Type GetType()` |
| `public virtual bool Equals(object obj)` |
| `public virtual int GetHashCode()` |
| `public virtual string ToString()` |
| `public void Dispose()` |
| `protected object MemberwiseClone()` |
| `protected virtual void Finalize()` |
| `protected virtual void OnDispose()` |
| `protected virtual void OnInitialize()` |

### 声明的普通方法

| 普通方法名称 | 注释 |
| :--- | :--- | 
| `public void Dispose` | 释放资源，触发 OnDispose |
| `protected virtual void OnDispose` | 释放时的清理逻辑，子类可覆写 |
| `protected virtual void OnInitialize` | 初始化逻辑，子类必须实现 |

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

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Aesir Inspector`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。
